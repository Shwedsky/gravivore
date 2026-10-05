using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Gravivore.Core.Stats;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Persistence.Profile;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.EditMode
{
    public sealed class SaveOfflineTests
    {
        private readonly List<string> _temporaryDirectories = new List<string>();

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < _temporaryDirectories.Count; i++)
            {
                if (Directory.Exists(_temporaryDirectories[i])) Directory.Delete(_temporaryDirectories[i], true);
            }
            _temporaryDirectories.Clear();
        }

        [TestCase(0, 0, false, OfflineClockAnomaly.NonPositiveElapsed)]
        [TestCase(30, 15, false, OfflineClockAnomaly.None)]
        [TestCase(120, 60, false, OfflineClockAnomaly.None)]
        [TestCase(180, 60, true, OfflineClockAnomaly.None)]
        public void OfflineReward_IsDeterministicAndCapped(
            int elapsedMinutes,
            long expectedReward,
            bool expectedCapped,
            OfflineClockAnomaly expectedAnomaly)
        {
            var now = Utc(2026, 9, 29, 12);
            var state = new OfflineRewardState();
            var service = new OfflineRewardService(OfflineConfiguration(), state);

            var summary = service.Accrue(now.AddMinutes(-elapsedMinutes), now);

            Assert.That(summary.EarnedAmount, Is.EqualTo(expectedReward));
            Assert.That(summary.TotalPendingAmount, Is.EqualTo(expectedReward));
            Assert.That(summary.WasCapped, Is.EqualTo(expectedCapped));
            Assert.That(summary.ClockAnomaly, Is.EqualTo(expectedAnomaly));
        }

        [Test]
        public void OfflineReward_NegativeAndHugeClockDeltasAreSafe()
        {
            var service = new OfflineRewardService(OfflineConfiguration(), new OfflineRewardState());
            var now = Utc(2026, 9, 29, 12);

            var negative = service.Accrue(now.AddMinutes(1), now);
            var huge = service.Accrue(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc));

            Assert.That(negative.EarnedAmount, Is.Zero);
            Assert.That(negative.ClockAnomaly, Is.EqualTo(OfflineClockAnomaly.NonPositiveElapsed));
            Assert.That(huge.EarnedAmount, Is.EqualTo(60));
            Assert.IsTrue(huge.WasCapped);
        }

        [Test]
        public void OfflineReward_UsesAwayFromZeroRoundingAndClaimsOnce()
        {
            var state = new OfflineRewardState();
            var configuration = new OfflineRewardConfiguration(10d, 0.5d, TimeSpan.FromHours(2));
            var service = new OfflineRewardService(configuration, state);
            var now = Utc(2026, 9, 29, 12);

            var summary = service.Accrue(now.AddMinutes(-30), now);

            Assert.That(summary.EarnedAmount, Is.EqualTo(3));
            Assert.IsTrue(service.ClaimPendingReward());
            Assert.That(state.MaterialBalance, Is.EqualTo(3));
            Assert.That(state.PendingReward, Is.Zero);
            Assert.IsFalse(service.ClaimPendingReward());
            Assert.That(state.MaterialBalance, Is.EqualTo(3));
        }

        [Test]
        public void FullProfile_RoundTripsPermanentGraphAndEquipmentDerivedStats()
        {
            var context = CreateContext();
            var now = Utc(2026, 9, 29, 12);
            var source = ProfileSaveMapper.CreateFresh(context, now, Guid.Parse("11111111-1111-1111-1111-111111111111"));
            var equipment = new EquipmentService(source.PlayerStats, context.Equipment, source.Inventory);
            using var progression = new AssimilationProgressionService(source.PlayerStats, source.Progression, context.Progression);
            using var quests = new QuestService(context.Quests, source.Quests, progression);
            quests.RecordMovementPerformed();
            progression.TryGrant(Death(1, "scout-drone"));
            progression.TryGrant(Death(2, "scout-drone"));
            equipment.GrantEquipment("power-core");
            equipment.Equip("power-core", EquipmentSlot.Core);
            source.World.TryUnlockEliteGate();
            new OfflineRewardService(OfflineConfiguration(), source.Offline).Accrue(now.AddMinutes(-30), now);
            var expectedDerived = source.PlayerStats.DerivedStats;

            var dto = ProfileSaveMapper.ToDto(source, context);
            var json = new UnityJsonSaveSerializer().Serialize(dto);
            var restoredDto = new UnityJsonSaveSerializer().Deserialize<SaveRootDto>(json);
            var restored = ProfileSaveMapper.Restore(restoredDto, context);
            _ = new EquipmentService(restored.PlayerStats, context.Equipment, restored.Inventory);

            Assert.That(restored.ProfileId, Is.EqualTo(source.ProfileId));
            Assert.That(restored.PlayerStats.BaseLevels.Power, Is.EqualTo(source.PlayerStats.BaseLevels.Power));
            Assert.That(restored.Progression.GetStatExperience(PlayerStatType.Power), Is.EqualTo(source.Progression.GetStatExperience(PlayerStatType.Power)));
            Assert.That(restored.Progression.TotalAssimilationScore, Is.EqualTo(2));
            Assert.IsTrue(restored.Progression.HasFirstKill("scout-drone"));
            Assert.That(restored.Progression.ProcessedLifeCount, Is.Zero);
            Assert.IsTrue(restored.Quests.IsObjectiveCompleted("intro-relay-yard"));
            Assert.IsTrue(restored.World.EliteGateUnlocked);
            Assert.IsTrue(restored.Inventory.TryGetEquipped(EquipmentSlot.Core, out var itemId));
            Assert.That(itemId, Is.EqualTo("power-core"));
            Assert.IsTrue(restored.PlayerStats.DerivedStats.HasSameValues(expectedDerived));
            Assert.That(restored.Offline.PendingReward, Is.EqualTo(15));

            using var restoredProgression = new AssimilationProgressionService(restored.PlayerStats, restored.Progression, context.Progression);
            var repeatedFirstKill = 0;
            restoredProgression.FirstKill += _ => repeatedFirstKill++;
            restoredProgression.TryGrant(Death(3, "scout-drone"));
            Assert.That(repeatedFirstKill, Is.Zero);
        }

        [Test]
        public void Repository_CreatesStableCurrentProfileAndMigratesV0()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            Func<SaveRootDto> fresh = () => ProfileSaveMapper.ToDto(ProfileSaveMapper.CreateFresh(context, time.UtcNow), context);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };

            var first = repository.LoadOrCreate(fresh, validate);
            var second = repository.LoadOrCreate(fresh, validate);

            Assert.IsTrue(first.WasCreated);
            Assert.IsFalse(second.WasCreated);
            Assert.That(second.Save.profileId, Is.EqualTo(first.Save.profileId));
            Assert.That(second.Save.schemaVersion, Is.EqualTo(SaveSchema.CurrentVersion));

            var legacy = new LegacySaveRootV0Dto
            {
                schemaVersion = 0,
                profileId = first.Save.profileId,
                createdUtc = first.Save.createdUtc,
                lastSeenUtc = first.Save.lastSeenUtc,
                player = first.Save.player,
                world = first.Save.world,
                boss = first.Save.boss,
                quest = first.Save.quest,
                inventory = first.Save.inventory
            };
            File.WriteAllText(repository.MainPath, new UnityJsonSaveSerializer().Serialize(legacy));

            var migrated = repository.LoadOrCreate(fresh, validate);

            Assert.IsTrue(migrated.WasMigrated);
            Assert.That(migrated.Save.schemaVersion, Is.EqualTo(1));
            Assert.That(migrated.Save.profileId, Is.EqualTo(first.Save.profileId));
            Assert.IsNotNull(migrated.Save.offline);
            Assert.That(migrated.Save.offline.pendingReward, Is.Zero);

            File.WriteAllText(repository.MainPath, "{}");
            if (File.Exists(repository.BackupPath)) File.Delete(repository.BackupPath);
            var incompleteLegacy = repository.LoadOrCreate(fresh, validate);
            Assert.IsTrue(incompleteLegacy.WasCreated);
            Assert.That(incompleteLegacy.Save.schemaVersion, Is.EqualTo(1));
        }

        [Test]
        public void Migration_RejectsFutureNegativeAndMissingSteps()
        {
            var serializer = new UnityJsonSaveSerializer();
            var defaults = new SaveRootDto { schemaVersion = 1 };
            var pipeline = new SaveMigrationPipeline(1, serializer, new ISaveMigration[] { new SaveMigrationV0ToV1() });

            Assert.Throws<NotSupportedException>(() => pipeline.MigrateToCurrent("{\"schemaVersion\":2}", defaults));
            Assert.Throws<ArgumentException>(() => pipeline.MigrateToCurrent("{\"schemaVersion\":-1}", defaults));
            var missing = new SaveMigrationPipeline(2, serializer, new ISaveMigration[] { new SaveMigrationV0ToV1() });
            Assert.Throws<NotSupportedException>(() => missing.MigrateToCurrent("{\"schemaVersion\":0}", defaults));
            var invalidResult = new SaveMigrationPipeline(
                1,
                serializer,
                new ISaveMigration[] { new InvalidResultMigration() });
            Assert.Throws<InvalidOperationException>(() =>
                invalidResult.MigrateToCurrent("{\"schemaVersion\":0}", defaults));
        }

        [Test]
        public void Repository_RecoversBackupAndPreservesBothCorruptFiles()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            Func<SaveRootDto> fresh = () => ProfileSaveMapper.ToDto(ProfileSaveMapper.CreateFresh(context, time.UtcNow), context);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var original = repository.LoadOrCreate(fresh, validate).Save;
            repository.Save(original, fresh, validate);
            File.WriteAllText(repository.MainPath, "{broken");

            var recovered = repository.LoadOrCreate(fresh, validate);

            Assert.IsTrue(recovered.RecoveredFromBackup);
            Assert.That(recovered.Save.profileId, Is.EqualTo(original.profileId));
            Assert.That(Directory.GetFiles(directory, "profile.corrupt.*.json").Length, Is.EqualTo(1));

            File.WriteAllText(repository.MainPath, "{broken-main");
            File.WriteAllText(repository.BackupPath, "{broken-backup");
            var freshAfterCorruption = repository.LoadOrCreate(fresh, validate);

            Assert.IsTrue(freshAfterCorruption.WasCreated);
            Assert.That(Directory.GetFiles(directory, "*.corrupt.*.json").Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(diagnostics.WarningCount, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Repository_InterruptedTempDoesNotDamageMainAndInvalidRangeFallsBack()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            Func<SaveRootDto> fresh = () => ProfileSaveMapper.ToDto(ProfileSaveMapper.CreateFresh(context, time.UtcNow), context);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var original = repository.LoadOrCreate(fresh, validate).Save;
            File.WriteAllText(repository.TempPath, "interrupted");

            var loaded = repository.LoadOrCreate(fresh, validate);

            Assert.That(loaded.Save.profileId, Is.EqualTo(original.profileId));
            loaded.Save.player.powerLevel = 0;
            File.WriteAllText(repository.MainPath, new UnityJsonSaveSerializer().Serialize(loaded.Save));
            if (File.Exists(repository.BackupPath)) File.Delete(repository.BackupPath);

            var fallback = repository.LoadOrCreate(fresh, validate);
            Assert.IsTrue(fallback.WasCreated);
            Assert.That(fallback.Save.player.powerLevel, Is.EqualTo(1));
        }

        [Test]
        public void Repository_SuccessfulSavePromotesPreviousMainAndIgnoresStaleTemp()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            var serializer = new UnityJsonSaveSerializer();
            Func<SaveRootDto> fresh = () => FreshDto(context, time.UtcNow);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var first = repository.LoadOrCreate(fresh, validate).Save;
            var firstMain = File.ReadAllText(repository.MainPath);
            var second = serializer.Deserialize<SaveRootDto>(serializer.Serialize(first));
            second.player.powerLevel = 2;

            repository.Save(second, fresh, validate);
            File.WriteAllText(repository.TempPath, serializer.Serialize(first));
            var loaded = repository.LoadOrCreate(fresh, validate);

            Assert.That(loaded.Save.player.powerLevel, Is.EqualTo(2));
            Assert.That(File.ReadAllText(repository.BackupPath), Is.EqualTo(firstMain));
            Assert.That(serializer.Deserialize<SaveRootDto>(File.ReadAllText(repository.BackupPath)).player.powerLevel,
                Is.EqualTo(1));
        }

        [Test]
        public void Repository_FailedTempWriteLeavesValidatedMainAndBackupUntouched()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            var serializer = new UnityJsonSaveSerializer();
            Func<SaveRootDto> fresh = () => FreshDto(context, time.UtcNow);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var save = repository.LoadOrCreate(fresh, validate).Save;
            repository.Save(save, fresh, validate);
            var mainBefore = File.ReadAllText(repository.MainPath);
            var backupBefore = File.ReadAllText(repository.BackupPath);
            var changed = serializer.Deserialize<SaveRootDto>(serializer.Serialize(save));
            changed.player.powerLevel = 2;
            var fileSystem = new FaultingProfileFileSystem { WriteFailurePath = repository.TempPath };
            var faultingRepository = CreateRepository(directory, time, null, fileSystem);

            Assert.Throws<IOException>(() => faultingRepository.Save(changed, fresh, validate));

            Assert.That(File.ReadAllText(repository.MainPath), Is.EqualTo(mainBefore));
            Assert.That(File.ReadAllText(repository.BackupPath), Is.EqualTo(backupBefore));
        }

        [Test]
        public void ProfileRestore_RejectsEncounterQuestContradictionsAndAcceptsCompletedGraph()
        {
            var context = CreateContext();
            var now = Utc(2026, 9, 29, 12);

            var defeatedEliteWithoutQuest = FreshDto(context, now);
            SetEliteDefeated(defeatedEliteWithoutQuest);
            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(defeatedEliteWithoutQuest, context));

            var eliteQuestWithoutDefeat = FreshDto(context, now);
            CompleteObjective(eliteQuestWithoutDefeat, context.EliteObjectiveId);
            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(eliteQuestWithoutDefeat, context));

            var defeatedBossWithoutQuest = FreshDto(context, now);
            SetEliteDefeated(defeatedBossWithoutQuest);
            CompleteObjective(defeatedBossWithoutQuest, context.EliteObjectiveId);
            defeatedBossWithoutQuest.boss.defeated = true;
            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(defeatedBossWithoutQuest, context));

            var bossQuestWithoutDefeat = FreshDto(context, now);
            SetEliteDefeated(bossQuestWithoutDefeat);
            CompleteObjective(bossQuestWithoutDefeat, context.EliteObjectiveId);
            CompleteObjective(bossQuestWithoutDefeat, context.BossObjectiveId);
            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(bossQuestWithoutDefeat, context));

            var validCompleted = FreshDto(context, now);
            SetEliteDefeated(validCompleted);
            CompleteObjective(validCompleted, context.EliteObjectiveId);
            CompleteObjective(validCompleted, context.BossObjectiveId);
            validCompleted.boss.defeated = true;

            var restored = ProfileSaveMapper.Restore(validCompleted, context);
            var roundTripped = ProfileSaveMapper.Restore(ProfileSaveMapper.ToDto(restored, context), context);

            Assert.IsTrue(roundTripped.World.EliteDefeated);
            Assert.IsTrue(roundTripped.World.BossGateUnlocked);
            Assert.IsTrue(roundTripped.Boss.IsDefeated);
            Assert.IsTrue(roundTripped.Quests.IsObjectiveCompleted(context.EliteObjectiveId));
            Assert.IsTrue(roundTripped.Quests.IsObjectiveCompleted(context.BossObjectiveId));
        }

        [Test]
        public void Repository_SemanticallyCorruptMainRecoversValidBackup()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            var serializer = new UnityJsonSaveSerializer();
            Func<SaveRootDto> fresh = () => FreshDto(context, time.UtcNow);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var valid = repository.LoadOrCreate(fresh, validate).Save;
            repository.Save(valid, fresh, validate);
            var corrupt = serializer.Deserialize<SaveRootDto>(serializer.Serialize(valid));
            SetEliteDefeated(corrupt);
            File.WriteAllText(repository.MainPath, serializer.Serialize(corrupt));

            var recovered = repository.LoadOrCreate(fresh, validate);

            Assert.IsTrue(recovered.RecoveredFromBackup);
            Assert.That(recovered.Save.profileId, Is.EqualTo(valid.profileId));
            Assert.IsFalse(recovered.Save.world.eliteDefeated);
            Assert.That(Directory.GetFiles(directory, "profile.corrupt.*.json").Length, Is.EqualTo(1));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Repository_TransientReadIOExceptionPropagatesWithoutPreservingSave(bool failMainRead)
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            Func<SaveRootDto> fresh = () => FreshDto(context, time.UtcNow);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var valid = repository.LoadOrCreate(fresh, validate).Save;
            repository.Save(valid, fresh, validate);
            if (!failMainRead) File.WriteAllText(repository.MainPath, "{broken-main");
            var mainBefore = File.ReadAllText(repository.MainPath);
            var backupBefore = File.ReadAllText(repository.BackupPath);
            var fileSystem = new FaultingProfileFileSystem
            {
                ReadFailurePath = failMainRead ? repository.MainPath : repository.BackupPath
            };
            var faultingRepository = CreateRepository(directory, time, null, fileSystem);

            Assert.Throws<IOException>(() => faultingRepository.LoadOrCreate(fresh, validate));

            Assert.That(File.ReadAllText(repository.MainPath), Is.EqualTo(mainBefore));
            Assert.That(File.ReadAllText(repository.BackupPath), Is.EqualTo(backupBefore));
            Assert.That(Directory.GetFiles(directory, "*.corrupt.*.json"), Is.Empty);
        }

        [Test]
        public void ProfileSession_RecoveryWriteFailureSuspendsPersistenceWithoutDestroyingBackup()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            Func<SaveRootDto> fresh = () => FreshDto(context, time.UtcNow);
            Action<SaveRootDto> validate = dto => { _ = ProfileSaveMapper.Restore(dto, context); };
            var valid = repository.LoadOrCreate(fresh, validate).Save;
            repository.Save(valid, fresh, validate);
            File.WriteAllText(repository.MainPath, "{broken-main");
            var mainBefore = File.ReadAllText(repository.MainPath);
            var backupBefore = File.ReadAllText(repository.BackupPath);
            var fileSystem = new FaultingProfileFileSystem { WriteFailurePath = repository.TempPath };
            var diagnostics = new RecordingDiagnostics();
            var faultingRepository = CreateRepository(directory, time, diagnostics, fileSystem);

            var session = ProfileSession.Start(
                faultingRepository,
                context,
                new SaveOfflineConfiguration(1, 2f, OfflineConfiguration()),
                time,
                diagnostics);

            Assert.IsTrue(session.PersistenceSuspended);
            Assert.IsFalse(session.StartupCheckpointSucceeded);
            Assert.That(fileSystem.WriteCount, Is.EqualTo(1));
            Assert.That(diagnostics.LastErrorMessage, Does.Contain("Persistence is suspended"));
            Assert.That(File.ReadAllText(repository.MainPath), Is.EqualTo(mainBefore));
            Assert.That(File.ReadAllText(repository.BackupPath), Is.EqualTo(backupBefore));
            var writesAfterFailure = fileSystem.WriteCount;
            var equipment = new EquipmentService(session.State.PlayerStats, context.Equipment, session.State.Inventory);
            using var progression = new AssimilationProgressionService(
                session.State.PlayerStats,
                session.State.Progression,
                context.Progression);
            using var quests = new QuestService(context.Quests, session.State.Quests, progression);
            using var coordinator = new SaveCoordinator(
                session,
                session.State.PlayerStats,
                progression,
                equipment,
                quests,
                session.State.World,
                session.State.Boss,
                session.State.Offline,
                2f);
            session.State.PlayerStats.SetLevel(PlayerStatType.Power, 2);
            coordinator.Tick(10f);
            Assert.IsFalse(coordinator.FlushNow());
            Assert.That(fileSystem.WriteCount, Is.EqualTo(writesAfterFailure));
            Assert.That(File.ReadAllText(repository.MainPath), Is.EqualTo(mainBefore));
            Assert.That(File.ReadAllText(repository.BackupPath), Is.EqualTo(backupBefore));
        }

        [Test]
        public void ProfileSession_ValidatedMainAndBackupCorruptionKeepsPersistenceEnabled()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            _ = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            File.WriteAllText(repository.MainPath, "{broken-main");
            File.WriteAllText(repository.BackupPath, "{broken-backup");

            var recovered = ProfileSession.Start(repository, context, configuration, time, diagnostics);

            Assert.IsFalse(recovered.PersistenceSuspended);
            Assert.IsTrue(recovered.LoadResult.WasCreated);
            Assert.IsTrue(recovered.StartupCheckpointSucceeded);
        }

        [Test]
        public void ProfileSession_LockedOnboardingDoesNotBankTimeThenEligibleProfileAccrues()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            var freshSession = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            Assert.That(freshSession.ReturnSummary.EarnedAmount, Is.Zero);
            Assert.That(freshSession.State.Offline.PendingReward, Is.Zero);

            time.UtcNow = time.UtcNow.AddHours(2);
            var firstLockedReturn = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            Assert.IsFalse(OfflineRewardEligibility.IsUnlocked(firstLockedReturn.State.Quests));
            Assert.That(firstLockedReturn.ReturnSummary.EarnedAmount, Is.Zero);
            Assert.That(firstLockedReturn.State.Offline.PendingReward, Is.Zero);
            Assert.That(firstLockedReturn.State.LastSeenUtc, Is.EqualTo(time.UtcNow));

            time.UtcNow = time.UtcNow.AddHours(2);
            var repeatedLockedReturn = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            Assert.That(repeatedLockedReturn.ReturnSummary.EarnedAmount, Is.Zero);
            Assert.That(repeatedLockedReturn.State.Offline.PendingReward, Is.Zero);
            UnlockOfflineReward(repeatedLockedReturn, context, 500);
            var assimilationBeforeReturn = repeatedLockedReturn.State.Progression.TotalAssimilationScore;
            var completedObjectivesBeforeReturn = repeatedLockedReturn.State.Quests.CompletedObjectiveCount;
            var worldBeforeReturn = repeatedLockedReturn.State.World.ExportSnapshot();

            time.UtcNow = time.UtcNow.AddMinutes(30);
            var eligibleReturn = ProfileSession.Start(repository, context, configuration, time, diagnostics);

            Assert.IsTrue(OfflineRewardEligibility.IsUnlocked(eligibleReturn.State.Quests));
            Assert.That(eligibleReturn.ReturnSummary.EarnedAmount, Is.EqualTo(15));
            Assert.That(eligibleReturn.State.Offline.PendingReward, Is.EqualTo(15));
            Assert.That(eligibleReturn.State.Progression.TotalAssimilationScore, Is.EqualTo(assimilationBeforeReturn));
            Assert.That(eligibleReturn.State.Quests.CompletedObjectiveCount, Is.EqualTo(completedObjectivesBeforeReturn));
            Assert.That(eligibleReturn.State.World.EliteGateUnlocked, Is.EqualTo(worldBeforeReturn.EliteGateUnlocked));
            Assert.That(eligibleReturn.State.World.EliteDefeated, Is.EqualTo(worldBeforeReturn.EliteDefeated));
            Assert.That(eligibleReturn.State.World.BossGateUnlocked, Is.EqualTo(worldBeforeReturn.BossGateUnlocked));
        }

        [Test]
        public void ProfileSession_RepeatedTimestampDoesNotDuplicatePendingAndClaimPersists()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            var initial = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            UnlockOfflineReward(initial, context, 600);
            var progressionBeforeOffline = initial.State.Progression.TotalAssimilationScore;
            time.UtcNow = time.UtcNow.AddMinutes(30);

            var firstReturn = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            var secondReturn = ProfileSession.Start(repository, context, configuration, time, diagnostics);

            Assert.That(firstReturn.ReturnSummary.EarnedAmount, Is.EqualTo(15));
            Assert.That(secondReturn.ReturnSummary.EarnedAmount, Is.Zero);
            Assert.That(secondReturn.State.Offline.PendingReward, Is.EqualTo(15));
            time.UtcNow = time.UtcNow.AddMinutes(30);
            var accumulatedReturn = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            Assert.That(accumulatedReturn.ReturnSummary.EarnedAmount, Is.EqualTo(15));
            Assert.That(accumulatedReturn.ReturnSummary.TotalPendingAmount, Is.EqualTo(30));
            Assert.IsTrue(accumulatedReturn.OfflineRewards.ClaimPendingReward());
            Assert.IsFalse(accumulatedReturn.OfflineRewards.ClaimPendingReward());
            Assert.IsTrue(accumulatedReturn.FlushNow());

            var reloaded = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            Assert.That(reloaded.State.Offline.MaterialBalance, Is.EqualTo(30));
            Assert.That(reloaded.State.Offline.PendingReward, Is.Zero);
            Assert.That(
                reloaded.State.Progression.TotalAssimilationScore,
                Is.EqualTo(progressionBeforeOffline));
        }

        [Test]
        public void ProfileSession_ClockRollbackCheckpointsNewLastSeenWithoutReward()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            var initial = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            UnlockOfflineReward(initial, context, 700);
            time.UtcNow = time.UtcNow.AddHours(-1);

            var rollback = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            var repeated = ProfileSession.Start(repository, context, configuration, time, diagnostics);

            Assert.That(rollback.ReturnSummary.EarnedAmount, Is.Zero);
            Assert.That(rollback.ReturnSummary.ClockAnomaly, Is.EqualTo(OfflineClockAnomaly.NonPositiveElapsed));
            Assert.That(rollback.State.LastSeenUtc, Is.EqualTo(time.UtcNow));
            Assert.IsTrue(rollback.StartupCheckpointSucceeded);
            Assert.That(repeated.State.Offline.PendingReward, Is.Zero);
        }

        [Test]
        public void SaveCoordinator_DebouncesRoutineMutationsAndFlushesAfterDelay()
        {
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = new RecordingRepository();
            var diagnostics = new RecordingDiagnostics();
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            var session = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            var equipment = new EquipmentService(session.State.PlayerStats, context.Equipment, session.State.Inventory);
            using var progression = new AssimilationProgressionService(
                session.State.PlayerStats,
                session.State.Progression,
                context.Progression);
            using var quests = new QuestService(context.Quests, session.State.Quests, progression);
            using var coordinator = new SaveCoordinator(
                session,
                session.State.PlayerStats,
                progression,
                equipment,
                quests,
                session.State.World,
                session.State.Boss,
                session.State.Offline,
                2f);
            var startupSaves = repository.SaveCount;

            coordinator.MarkDirty();
            Assert.IsTrue(coordinator.IsDirty);
            coordinator.Tick(0.5f);
            coordinator.MarkDirty();
            coordinator.Tick(0.5f);
            coordinator.MarkDirty();
            coordinator.Tick(0.5f);
            coordinator.MarkDirty();
            Assert.That(repository.SaveCount, Is.EqualTo(startupSaves));
            coordinator.Tick(0.5f);

            Assert.IsFalse(coordinator.IsDirty);
            Assert.That(repository.SaveCount, Is.EqualTo(startupSaves + 1));
            coordinator.MarkDirty();
            coordinator.Tick(1.5f);
            Assert.That(repository.SaveCount, Is.EqualTo(startupSaves + 1));
            coordinator.Tick(0.5f);
            Assert.IsFalse(coordinator.IsDirty);
            Assert.That(repository.SaveCount, Is.EqualTo(startupSaves + 2));
        }

        [Test]
        public void SaveFailure_IsReportedAndDoesNotRollBackCommittedRuntimeState()
        {
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = new RecordingRepository();
            var diagnostics = new RecordingDiagnostics();
            var session = ProfileSession.Start(
                repository,
                context,
                new SaveOfflineConfiguration(1, 2f, OfflineConfiguration()),
                time,
                diagnostics);
            var equipment = new EquipmentService(session.State.PlayerStats, context.Equipment, session.State.Inventory);
            using var progression = new AssimilationProgressionService(
                session.State.PlayerStats,
                session.State.Progression,
                context.Progression);
            using var quests = new QuestService(context.Quests, session.State.Quests, progression);
            using var coordinator = new SaveCoordinator(
                session,
                session.State.PlayerStats,
                progression,
                equipment,
                quests,
                session.State.World,
                session.State.Boss,
                session.State.Offline,
                2f);
            session.OfflineRewards.Accrue(time.UtcNow.AddMinutes(-30), time.UtcNow);
            repository.ThrowOnSave = true;

            Assert.IsTrue(coordinator.IsDirty);
            Assert.IsFalse(coordinator.FlushNow());
            Assert.IsTrue(coordinator.IsDirty);
            Assert.That(session.State.Offline.PendingReward, Is.EqualTo(15));
            Assert.That(diagnostics.ErrorCount, Is.EqualTo(1));
            var savesAfterFailure = repository.SaveCount;
            repository.ThrowOnSave = false;
            coordinator.Tick(1.5f);
            Assert.That(repository.SaveCount, Is.EqualTo(savesAfterFailure));
            Assert.IsTrue(coordinator.IsDirty);
            coordinator.Tick(0.5f);
            Assert.That(repository.SaveCount, Is.EqualTo(savesAfterFailure + 1));
            Assert.IsFalse(coordinator.IsDirty);
        }

        [Test]
        public void EquipmentDerivedStatsObserverFailureStillPublishesInventoryDirtyBoundary()
        {
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = new RecordingRepository();
            var diagnostics = new RecordingDiagnostics();
            var session = ProfileSession.Start(
                repository,
                context,
                new SaveOfflineConfiguration(1, 2f, OfflineConfiguration()),
                time,
                diagnostics);
            var equipment = new EquipmentService(session.State.PlayerStats, context.Equipment, session.State.Inventory);
            using var progression = new AssimilationProgressionService(
                session.State.PlayerStats,
                session.State.Progression,
                context.Progression);
            using var quests = new QuestService(context.Quests, session.State.Quests, progression);
            using var coordinator = new SaveCoordinator(
                session,
                session.State.PlayerStats,
                progression,
                equipment,
                quests,
                session.State.World,
                session.State.Boss,
                session.State.Offline,
                2f);
            equipment.GrantEquipment("power-core");
            Assert.IsTrue(coordinator.FlushNow());
            var laterDerivedObserverCalled = false;
            session.State.PlayerStats.DerivedStatsChanged += _ =>
                throw new InvalidOperationException("derived stats presentation failed");
            session.State.PlayerStats.DerivedStatsChanged += _ => laterDerivedObserverCalled = true;
            LogAssert.Expect(
                LogType.Exception,
                new Regex("InvalidOperationException: derived stats presentation failed"));

            Assert.IsTrue(equipment.Equip("power-core", EquipmentSlot.Core));

            Assert.IsTrue(session.State.Inventory.TryGetEquipped(EquipmentSlot.Core, out var equippedId));
            Assert.That(equippedId, Is.EqualTo("power-core"));
            Assert.IsTrue(laterDerivedObserverCalled);
            Assert.IsTrue(coordinator.IsDirty);
        }

        [Test]
        public void ResumeProcessingUsesThresholdEligibilityAndCheckpointsPendingWithoutDuplicates()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            var session = ProfileSession.Start(repository, context, configuration, time, diagnostics);

            using (var progression = new AssimilationProgressionService(
                       session.State.PlayerStats,
                       session.State.Progression,
                       context.Progression))
            using (var quests = new QuestService(context.Quests, session.State.Quests, progression))
            using (var coordinator = new SaveCoordinator(
                       session,
                       session.State.PlayerStats,
                       progression,
                       new EquipmentService(session.State.PlayerStats, context.Equipment, session.State.Inventory),
                       quests,
                       session.State.World,
                       session.State.Boss,
                       session.State.Offline,
                       2f))
            {
                time.UtcNow = time.UtcNow.AddMinutes(10);
                var locked = coordinator.ProcessResume();
                Assert.That(locked.EarnedAmount, Is.Zero);
                Assert.That(session.State.Offline.PendingReward, Is.Zero);

                quests.RecordMovementPerformed();
                progression.TryGrant(Death(900, "scout-drone"));
                Assert.IsTrue(session.State.Quests.ExpandedObjectivesUnlocked);
                Assert.IsTrue(coordinator.FlushNow());

                time.UtcNow = time.UtcNow.AddSeconds(30);
                var shortAbsence = coordinator.ProcessResume();
                Assert.That(shortAbsence.EarnedAmount, Is.Zero);

                time.UtcNow = time.UtcNow.AddMinutes(10);
                var eligible = coordinator.ProcessResume();
                var repeated = coordinator.ProcessResume();

                Assert.That(eligible.EarnedAmount, Is.EqualTo(5));
                Assert.That(session.State.Offline.PendingReward, Is.EqualTo(5));
                Assert.That(repeated.EarnedAmount, Is.Zero);
                Assert.That(session.State.Offline.PendingReward, Is.EqualTo(5));
            }

            var reloaded = ProfileSession.Start(repository, context, configuration, time, diagnostics);
            Assert.That(reloaded.State.Offline.PendingReward, Is.EqualTo(5));
        }

        [Test]
        public void DevelopmentProfileResetPreventsRepeatedLifecycleFlushFromRecreatingSave()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var repository = CreateRepository(directory, time);
            var session = ProfileSession.Start(
                repository,
                context,
                new SaveOfflineConfiguration(1, 2f, OfflineConfiguration()),
                time,
                new RecordingDiagnostics());
            Assert.IsTrue(File.Exists(repository.MainPath));

            Assert.IsTrue(session.ResetProfileForDevelopment());
            Assert.IsFalse(session.FlushNow());
            Assert.IsFalse(session.FlushNow());

            Assert.IsFalse(File.Exists(repository.MainPath));
            Assert.IsFalse(File.Exists(repository.BackupPath));
            Assert.IsFalse(File.Exists(repository.TempPath));
        }

        [Test]
        public void ProfileRestore_ExistingCurrentV1SaveRestoresWithoutDataLoss()
        {
            var context = CreateContext();
            var dto = FreshDto(context, Utc(2026, 10, 4, 12));

            var restored = ProfileSaveMapper.Restore(dto, context);

            Assert.That(restored.ProfileId.ToString("D"), Is.EqualTo(dto.profileId));
            Assert.That(restored.PlayerStats.BaseLevels.Power, Is.EqualTo(dto.player.powerLevel));
            Assert.That(restored.Progression.TotalAssimilationScore, Is.EqualTo(dto.player.totalAssimilationScore));
        }

        [Test]
        public void Repository_MalformedMainStillRestoresValidatedBackup()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 10, 4, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            Func<SaveRootDto> freshFactory = () => FreshDto(context, time.UtcNow);
            Action<SaveRootDto> validate = dto => ProfileSaveMapper.Restore(dto, context);

            var first = freshFactory();
            repository.Save(first, freshFactory, validate);
            first.lastSeenUtc = time.UtcNow.AddMinutes(1).ToString("O");
            repository.Save(first, freshFactory, validate);
            File.WriteAllText(repository.MainPath, "{");

            var loaded = repository.LoadOrCreate(freshFactory, validate);

            Assert.That(loaded.RecoveredFromBackup, Is.True);
            Assert.That(loaded.WasCreated, Is.False);
            Assert.That(loaded.Save.profileId, Is.EqualTo(first.profileId));
            Assert.That(diagnostics.WarningCount, Is.GreaterThan(0));
        }

        [Test]
        public void ProfileRestore_TuningChangeNormalizesResidualXpWithoutWipingProgress()
        {
            var oldContext = CreateContext(10, 10f);
            var dto = FreshDto(oldContext, Utc(2026, 10, 4, 12));
            dto.player.powerLevel = 1;
            dto.player.powerExperience = 5f;
            Assert.DoesNotThrow(() => ProfileSaveMapper.Restore(dto, oldContext));

            var currentContext = CreateContext(10, 2f);
            var warnings = new List<string>();
            var restored = ProfileSaveMapper.Restore(dto, currentContext, warnings.Add);

            Assert.That(restored.PlayerStats.BaseLevels.Power, Is.EqualTo(3));
            Assert.That(restored.Progression.GetStatExperience(PlayerStatType.Power), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(restored.ProfileId.ToString("D"), Is.EqualTo(dto.profileId));
            Assert.That(warnings.Count, Is.GreaterThan(0));
        }

        [Test]
        public void ProfileRestore_ReducedMaximumLevelClampsSafelyAndClearsMaxLevelResidualXp()
        {
            var originalContext = CreateContext();
            var dto = FreshDto(originalContext, Utc(2026, 10, 4, 12));
            dto.player.powerLevel = 7;
            dto.player.powerExperience = 1.5f;
            var reducedContext = CreateContext(3);
            var warnings = new List<string>();

            var restored = ProfileSaveMapper.Restore(dto, reducedContext, warnings.Add);

            Assert.That(restored.PlayerStats.BaseLevels.Power, Is.EqualTo(3));
            Assert.That(restored.Progression.GetStatExperience(PlayerStatType.Power), Is.Zero);
            Assert.That(restored.Progression.TotalAssimilationScore, Is.EqualTo(dto.player.totalAssimilationScore));
            Assert.That(warnings.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void ProfileRestore_UnknownOptionalQuestObjectiveIsDroppedWithoutResettingProfile()
        {
            var context = CreateContext();
            var dto = FreshDto(context, Utc(2026, 10, 4, 12));
            var original = dto.quest.objectives;
            var expanded = new Gravivore.Persistence.Quests.QuestObjectiveSaveDto[original.Length + 1];
            Array.Copy(original, expanded, original.Length);
            expanded[original.Length] = new Gravivore.Persistence.Quests.QuestObjectiveSaveDto
            {
                objectiveId = "removed-optional-objective",
                progress = 99,
                completed = true
            };
            dto.quest.objectives = expanded;
            var warnings = new List<string>();

            var restored = ProfileSaveMapper.Restore(dto, context, warnings.Add);

            Assert.That(restored.ProfileId.ToString("D"), Is.EqualTo(dto.profileId));
            Assert.That(restored.Quests.IsObjectiveCompleted("movement"), Is.False);
            Assert.That(warnings.Exists(message => message.Contains("removed-optional-objective")), Is.True);
        }

        [Test]
        public void ProfileRestore_UnknownOptionalEquipmentIsDroppedButKnownInventorySurvives()
        {
            var context = CreateContext();
            var dto = FreshDto(context, Utc(2026, 10, 4, 12));
            dto.inventory.OwnedItemIds = new[] { "power-core", "removed-module" };
            dto.inventory.EquippedItems = new[]
            {
                new Gravivore.Persistence.EquippedItemSaveDto
                {
                    Slot = (int)EquipmentSlot.Module,
                    ItemId = "removed-module"
                }
            };
            var warnings = new List<string>();

            var restored = ProfileSaveMapper.Restore(dto, context, warnings.Add);

            Assert.That(restored.Inventory.HasItem("power-core"), Is.True);
            Assert.That(restored.Inventory.HasItem("removed-module"), Is.False);
            Assert.That(restored.Inventory.TryGetEquipped(EquipmentSlot.Module, out _), Is.False);
            Assert.That(warnings.Exists(message => message.Contains("removed-module")), Is.True);
        }

        [Test]
        public void ProfileRestore_UnknownEquippedItemStillRequiresOwnership()
        {
            var context = CreateContext();
            var dto = FreshDto(context, Utc(2026, 10, 4, 12));
            dto.inventory.OwnedItemIds = Array.Empty<string>();
            dto.inventory.EquippedItems = new[]
            {
                new Gravivore.Persistence.EquippedItemSaveDto
                {
                    Slot = (int)EquipmentSlot.Module,
                    ItemId = "removed-module"
                }
            };

            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(dto, context));
        }

        [Test]
        public void ProfileRestore_UnknownCriticalIdentityStillRejectsSave()
        {
            var context = CreateContext();
            var bossDto = FreshDto(context, Utc(2026, 10, 4, 12));
            bossDto.boss.bossId = "removed-critical-boss";
            var questDto = FreshDto(context, Utc(2026, 10, 4, 12));
            questDto.quest.questId = "removed-critical-quest";

            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(bossDto, context));
            Assert.Throws<ArgumentException>(() => ProfileSaveMapper.Restore(questDto, context));
        }

        [Test]
        public void ProfileRestore_NormalizedProgressionRoundTripDoesNotDuplicateLevelsOrXp()
        {
            var context = CreateContext();
            var dto = FreshDto(context, Utc(2026, 10, 4, 12));
            dto.player.powerLevel = 1;
            dto.player.powerExperience = 5f;

            var first = ProfileSaveMapper.Restore(dto, context);
            var normalizedDto = ProfileSaveMapper.ToDto(first, context);
            var second = ProfileSaveMapper.Restore(normalizedDto, context);

            Assert.That(second.PlayerStats.BaseLevels.Power, Is.EqualTo(first.PlayerStats.BaseLevels.Power));
            Assert.That(
                second.Progression.GetStatExperience(PlayerStatType.Power),
                Is.EqualTo(first.Progression.GetStatExperience(PlayerStatType.Power)).Within(0.0001f));
            Assert.That(second.Progression.TotalAssimilationScore, Is.EqualTo(first.Progression.TotalAssimilationScore));
        }

        [Test]
        public void ProfileRestore_UnknownFirstKillReferenceIsDroppedWithoutDuplicatingProgression()
        {
            var context = CreateContext();
            var dto = FreshDto(context, Utc(2026, 10, 4, 12));
            dto.player.firstKillEnemyIds = new[] { "scout-drone", "removed-enemy" };
            var warnings = new List<string>();

            var restored = ProfileSaveMapper.Restore(dto, context, warnings.Add);

            Assert.That(restored.Progression.HasFirstKill("scout-drone"), Is.True);
            Assert.That(restored.Progression.HasFirstKill("removed-enemy"), Is.False);
            Assert.That(warnings.Exists(message => message.Contains("removed-enemy")), Is.True);
        }

        private string CreateTemporaryDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "gravivore-s12-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            _temporaryDirectories.Add(path);
            return path;
        }

        private static JsonProfileRepository CreateRepository(
            string directory,
            ITimeProvider time,
            ISaveDiagnostics diagnostics = null,
            IProfileFileSystem fileSystem = null)
        {
            var serializer = new UnityJsonSaveSerializer();
            return new JsonProfileRepository(
                directory,
                serializer,
                new SaveMigrationPipeline(1, serializer, new ISaveMigration[] { new SaveMigrationV0ToV1() }),
                time,
                diagnostics ?? new RecordingDiagnostics(),
                fileSystem);
        }

        private static ProfileRestoreContext CreateContext(
            int maximumLevel = 10,
            float thresholdLevelOneCost = 2f)
        {
            var curve = new StatCurve(maximumLevel, 1f, 1f, 0f, 0f, 1000f);
            var stats = new PlayerStatsConfiguration(
                curve,
                new StatCurve(maximumLevel, 100f, 10f, 0f, 1f, 1000f),
                curve,
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                curve,
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            var progression = new ProgressionConfiguration(
                new ProgressionThresholdCurve(thresholdLevelOneCost, 0f, 0f, 1000f),
                new[]
                {
                    new CoreReward("scout-drone", PlayerStatType.Power, 1f, 1),
                    new CoreReward("cutter-unit", PlayerStatType.Hull, 1f, 1),
                    new CoreReward("warden", PlayerStatType.Armor, 1f, 1),
                    new CoreReward("arc-drone", PlayerStatType.Flux, 1f, 1),
                    new CoreReward("carrier", PlayerStatType.Mobility, 1f, 1)
                });
            var equipment = new EquipmentCatalog(new[]
            {
                new EquipmentItem("power-core", "Power Core", EquipmentSlot.Core, new EquipmentFlatModifier(4f, 0f, 0f, 0f, 0f)),
                new EquipmentItem("hull-chassis", "Hull Chassis", EquipmentSlot.Chassis, new EquipmentFlatModifier(0f, 20f, 2f, 0f, 0f)),
                new EquipmentItem("speed-module", "Speed Module", EquipmentSlot.Module, new EquipmentFlatModifier(0f, 0f, 0f, 0f, 1f))
            });
            var quests = new QuestCatalog("chapter01-onboarding", new[]
            {
                new QuestObjective("movement", "Move", QuestObjectiveType.MovementPerformed, 1, string.Empty, string.Empty, string.Empty, QuestTargetType.None, string.Empty, false),
                new QuestObjective("intro-relay-yard", "Relay", QuestObjectiveType.EnemyDefeated, 1, "scout-drone", "relay-yard", string.Empty, QuestTargetType.FarmingZone, "relay-yard", false),
                new QuestObjective("assimilation", "Assimilate", QuestObjectiveType.AssimilationReceived, 1, "scout-drone", string.Empty, string.Empty, QuestTargetType.None, string.Empty, true),
                new QuestObjective("elite", "Elite", QuestObjectiveType.EliteDefeated, 1, string.Empty, string.Empty, "magnetar-guard", QuestTargetType.Elite, "magnetar-guard", false),
                new QuestObjective("boss", "Boss", QuestObjectiveType.BossDefeated, 1, string.Empty, string.Empty, "custodian-m0", QuestTargetType.BossArena, "custodian-m0", false)
            });
            return new ProfileRestoreContext(stats, progression, equipment, quests, "elite-gate", "boss-gate", "magnetar-guard", "custodian-m0");
        }

        private static OfflineRewardConfiguration OfflineConfiguration() =>
            new OfflineRewardConfiguration(120d, 0.25d, TimeSpan.FromHours(2));

        private static EnemyDeathEvent Death(int seed, string enemyId) =>
            new EnemyDeathEvent(new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])), enemyId, Vector3.zero);

        private static SaveRootDto FreshDto(ProfileRestoreContext context, DateTime now)
        {
            return ProfileSaveMapper.ToDto(ProfileSaveMapper.CreateFresh(context, now), context);
        }

        private static void SetEliteDefeated(SaveRootDto dto)
        {
            dto.world.eliteGateUnlocked = true;
            dto.world.eliteDefeated = true;
            dto.world.bossGateUnlocked = true;
        }

        private static void CompleteObjective(SaveRootDto dto, string objectiveId)
        {
            for (var i = 0; i < dto.quest.objectives.Length; i++)
            {
                var objective = dto.quest.objectives[i];
                if (!string.Equals(objective.objectiveId, objectiveId, StringComparison.Ordinal)) continue;
                objective.progress = 1;
                objective.completed = true;
                return;
            }

            Assert.Fail($"Objective {objectiveId} was not present in the test profile.");
        }

        private static void UnlockOfflineReward(
            ProfileSession session,
            ProfileRestoreContext context,
            int lifeSeed)
        {
            using var progression = new AssimilationProgressionService(
                session.State.PlayerStats,
                session.State.Progression,
                context.Progression);
            using var quests = new QuestService(context.Quests, session.State.Quests, progression);
            quests.RecordMovementPerformed();
            progression.TryGrant(Death(lifeSeed, "scout-drone"));
            Assert.IsTrue(session.State.Quests.ExpandedObjectivesUnlocked);
            Assert.IsTrue(session.FlushNow());
        }

        private static DateTime Utc(int year, int month, int day, int hour) =>
            new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);

        private sealed class ManualTimeProvider : ITimeProvider
        {
            public ManualTimeProvider(DateTime utcNow) => UtcNow = utcNow;
            public DateTime UtcNow { get; set; }
        }

        private sealed class RecordingDiagnostics : ISaveDiagnostics
        {
            public int WarningCount { get; private set; }
            public int ErrorCount { get; private set; }
            public string LastErrorMessage { get; private set; }
            public void Warning(string message, Exception exception = null) => WarningCount++;
            public void Error(string message, Exception exception)
            {
                ErrorCount++;
                LastErrorMessage = message;
            }
        }

        private sealed class FaultingProfileFileSystem : IProfileFileSystem
        {
            private readonly SystemProfileFileSystem _inner = new SystemProfileFileSystem();

            public string ReadFailurePath { get; set; }
            public string WriteFailurePath { get; set; }
            public int WriteCount { get; private set; }

            public void CreateDirectory(string path) => _inner.CreateDirectory(path);
            public bool FileExists(string path) => _inner.FileExists(path);

            public string ReadAllText(string path)
            {
                if (PathEquals(path, ReadFailurePath)) throw new IOException("simulated transient read failure");
                return _inner.ReadAllText(path);
            }

            public void WriteAllText(string path, string contents)
            {
                WriteCount++;
                if (PathEquals(path, WriteFailurePath)) throw new IOException("simulated recovery write failure");
                _inner.WriteAllText(path, contents);
            }

            public void CopyFile(string source, string destination, bool overwrite) =>
                _inner.CopyFile(source, destination, overwrite);

            public void MoveFile(string source, string destination) => _inner.MoveFile(source, destination);
            public void DeleteFile(string path) => _inner.DeleteFile(path);
            public void ReplaceFile(string source, string destination) => _inner.ReplaceFile(source, destination);

            private static bool PathEquals(string left, string right) =>
                right != null && string.Equals(
                    Path.GetFullPath(left),
                    Path.GetFullPath(right),
                    StringComparison.OrdinalIgnoreCase);
        }

        private sealed class RecordingRepository : IProfileRepository
        {
            public int SaveCount { get; private set; }
            public bool ThrowOnSave { get; set; }

            public ProfileLoadResult LoadOrCreate(
                Func<SaveRootDto> freshFactory,
                Action<SaveRootDto> validate)
            {
                var fresh = freshFactory();
                validate(fresh);
                return new ProfileLoadResult(fresh, true, false, false);
            }

            public void Save(
                SaveRootDto save,
                Func<SaveRootDto> freshFactory,
                Action<SaveRootDto> validate)
            {
                if (ThrowOnSave) throw new IOException("simulated write failure");
                validate(save);
                SaveCount++;
            }
        }

        private sealed class InvalidResultMigration : ISaveMigration
        {
            public int FromVersion => 0;
            public int ToVersion => 1;

            public SaveRootDto Migrate(string sourceJson, ISaveSerializer serializer, SaveRootDto defaults)
            {
                return new SaveRootDto { schemaVersion = 0 };
            }
        }
    }
}
