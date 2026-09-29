using System;
using System.Collections.Generic;
using System.IO;
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
        public void ProfileSession_RepeatedTimestampDoesNotDuplicatePendingAndClaimPersists()
        {
            var directory = CreateTemporaryDirectory();
            var context = CreateContext();
            var time = new ManualTimeProvider(Utc(2026, 9, 29, 12));
            var diagnostics = new RecordingDiagnostics();
            var repository = CreateRepository(directory, time, diagnostics);
            var configuration = new SaveOfflineConfiguration(1, 2f, OfflineConfiguration());
            _ = ProfileSession.Start(repository, context, configuration, time, diagnostics);
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
            Assert.That(reloaded.State.Progression.TotalAssimilationScore, Is.Zero);
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
            _ = ProfileSession.Start(repository, context, configuration, time, diagnostics);
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

            Assert.IsTrue(equipment.GrantEquipment("power-core"));
            Assert.IsTrue(coordinator.IsDirty);
            coordinator.Tick(1.99f);
            Assert.That(repository.SaveCount, Is.EqualTo(startupSaves));
            coordinator.Tick(0.01f);

            Assert.IsFalse(coordinator.IsDirty);
            Assert.That(repository.SaveCount, Is.EqualTo(startupSaves + 1));
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
            ISaveDiagnostics diagnostics = null)
        {
            var serializer = new UnityJsonSaveSerializer();
            return new JsonProfileRepository(
                directory,
                serializer,
                new SaveMigrationPipeline(1, serializer, new ISaveMigration[] { new SaveMigrationV0ToV1() }),
                time,
                diagnostics ?? new RecordingDiagnostics());
        }

        private static ProfileRestoreContext CreateContext()
        {
            var curve = new StatCurve(10, 1f, 1f, 0f, 0f, 1000f);
            var stats = new PlayerStatsConfiguration(
                curve,
                new StatCurve(10, 100f, 10f, 0f, 1f, 1000f),
                curve,
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                curve,
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            var progression = new ProgressionConfiguration(
                new ProgressionThresholdCurve(2f, 0f, 0f, 1000f),
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
                new QuestObjective("assimilation", "Assimilate", QuestObjectiveType.AssimilationReceived, 1, "scout-drone", string.Empty, string.Empty, QuestTargetType.None, string.Empty, false),
                new QuestObjective("elite", "Elite", QuestObjectiveType.EliteDefeated, 1, string.Empty, string.Empty, "magnetar-guard", QuestTargetType.Elite, "magnetar-guard", false),
                new QuestObjective("boss", "Boss", QuestObjectiveType.BossDefeated, 1, string.Empty, string.Empty, "custodian-m0", QuestTargetType.BossArena, "custodian-m0", false)
            });
            return new ProfileRestoreContext(stats, progression, equipment, quests, "elite-gate", "boss-gate", "magnetar-guard", "custodian-m0");
        }

        private static OfflineRewardConfiguration OfflineConfiguration() =>
            new OfflineRewardConfiguration(120d, 0.25d, TimeSpan.FromHours(2));

        private static EnemyDeathEvent Death(int seed, string enemyId) =>
            new EnemyDeathEvent(new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])), enemyId, Vector3.zero);

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
            public void Warning(string message, Exception exception = null) => WarningCount++;
            public void Error(string message, Exception exception) => ErrorCount++;
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
