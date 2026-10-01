#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using Gravivore.Core.Stats;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Persistence.Profile;
using Gravivore.Presentation.Development;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class S16DevelopmentToolsTests
    {
        [Test]
        public void TelemetryEnvelope_SerializesStableMetadataAndTypedPayload()
        {
            var session = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var profile = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sink = new RecordingSink();
            var recorder = new DevelopmentTelemetryRecorder(
                session,
                profile,
                sink,
                () => new DateTime(2032, 1, 2, 3, 4, 5, DateTimeKind.Utc));

            Assert.IsTrue(recorder.TryRecord("stat_level_changed", new StatLevelChangedPayload("Power", 2, 3)));
            var envelope = JsonUtility.FromJson<DevelopmentEventEnvelope<StatLevelChangedPayload>>(sink.Lines[0]);

            Assert.That(envelope.SchemaVersion, Is.EqualTo(1));
            Assert.That(envelope.SessionId, Is.EqualTo(session.ToString("D")));
            Assert.That(envelope.ProfileId, Is.EqualTo(profile.ToString("D")));
            Assert.That(envelope.Sequence, Is.EqualTo(1));
            Assert.That(envelope.EventName, Is.EqualTo("stat_level_changed"));
            Assert.That(envelope.Payload.stat, Is.EqualTo("Power"));
            Assert.That(envelope.Payload.previousLevel, Is.EqualTo(2));
            Assert.That(envelope.Payload.newLevel, Is.EqualTo(3));
        }

        [Test]
        public void TelemetrySinkFailure_IsContainedAndDoesNotMutateGameplayState()
        {
            var stats = CreateStats();
            var before = stats.BaseLevels;
            var diagnostics = 0;
            var recorder = new DevelopmentTelemetryRecorder(
                Guid.NewGuid(), Guid.NewGuid(), new ThrowingSink(), errorReporter: _ => diagnostics++);

            var laterObserverRan = false;
            stats.StatChanged += _ => recorder.TryRecord("stat_level_changed", new EmptyPayload());
            stats.StatChanged += _ => laterObserverRan = true;

            Assert.DoesNotThrow(() => stats.SetLevel(PlayerStatType.Power, 2));
            Assert.That(diagnostics, Is.EqualTo(1));
            Assert.That(stats.BaseLevels.Power, Is.EqualTo(before.Power + 1));
            Assert.That(stats.BaseLevels.Hull, Is.EqualTo(before.Hull));
            Assert.IsTrue(laterObserverRan);
        }

        [Test]
        public void SessionSummary_TracksRequiredCountersDeterministically()
        {
            var now = 10d;
            var summary = new DevelopmentSessionSummary(() => now);
            summary.RecordOrdinaryEnemyDefeated();
            summary.RecordPlayerDeath();
            summary.RecordStatChange(2, 5);
            summary.RecordEliteDefeated();
            summary.RecordBossStarted();
            summary.RecordBossReset();
            summary.RecordBossDefeated();
            now = 14.5d;

            Assert.That(summary.ElapsedSeconds, Is.EqualTo(4.5d));
            Assert.That(summary.OrdinaryEnemiesDefeated, Is.EqualTo(1));
            Assert.That(summary.PlayerDeaths, Is.EqualTo(1));
            Assert.That(summary.StatLevelUps, Is.EqualTo(3));
            Assert.IsTrue(summary.EliteDefeated);
            Assert.That(summary.BossAttempts, Is.EqualTo(1));
            Assert.That(summary.BossResets, Is.EqualTo(1));
            Assert.IsTrue(summary.BossDefeated);
        }

        [Test]
        public void StatCommands_ClampAndMarkPersistenceDirty()
        {
            using var fixture = new CommandFixture();
            Assert.IsTrue(fixture.Commands.GrantStat(PlayerStatType.Power, 5));
            Assert.That(fixture.Stats.BaseLevels.Power, Is.EqualTo(6));
            Assert.IsTrue(fixture.Commands.GrantStat(PlayerStatType.Power, 99));
            Assert.That(fixture.Stats.BaseLevels.Power, Is.EqualTo(10));
            Assert.IsFalse(fixture.Commands.GrantStat(PlayerStatType.Power, 1));
            Assert.That(fixture.DirtyCalls, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void UnlockCommands_CreateConsistentAuthoritativeWorldAndQuestState()
        {
            using var fixture = new CommandFixture();
            Assert.IsTrue(fixture.Commands.UnlockElite());
            Assert.IsTrue(fixture.World.EliteGateUnlocked);
            Assert.IsFalse(fixture.World.BossGateUnlocked);
            Assert.IsTrue(fixture.EliteRequirement.IsSatisfied(fixture.Progression.State, fixture.Quests.State));
            Assert.That(
                fixture.Progression.State.TotalAssimilationScore,
                Is.EqualTo(fixture.EliteRequirement.MinimumAssimilationScore));
            Assert.IsTrue(fixture.Quests.State.IsObjectiveCompleted("elite-prerequisite"));
            Assert.That(fixture.DirtyCalls, Is.EqualTo(1));

            Assert.IsTrue(fixture.Commands.UnlockBoss());
            Assert.IsTrue(fixture.World.EliteDefeated);
            Assert.IsTrue(fixture.World.BossGateUnlocked);
            Assert.IsTrue(fixture.Quests.State.IsObjectiveCompleted("elite-objective"));
        }

        [Test]
        public void ResetBoss_ClearsCompletionAndQuestWithoutReplayingDefeat()
        {
            using var fixture = new CommandFixture();
            fixture.Commands.UnlockBoss();
            fixture.Quests.CompleteEncounterObjectiveForDevelopment(QuestObjectiveType.BossDefeated, "custodian-m0");
            var defeatEvents = 0;
            fixture.Completion.Defeated += _ => defeatEvents++;
            fixture.Completion.TryRecordDefeat("custodian-m0", Vector3.zero);

            Assert.IsTrue(fixture.Commands.ResetBoss());
            Assert.IsFalse(fixture.Completion.IsDefeated);
            Assert.IsFalse(fixture.Quests.State.IsObjectiveCompleted("boss-objective"));
            Assert.IsTrue(fixture.World.BossGateUnlocked);
            Assert.That(defeatEvents, Is.EqualTo(1));
        }

        [Test]
        public void GodMode_BlocksDamageUntilDisabled()
        {
            using var fixture = new CommandFixture();
            var initial = fixture.Health.CurrentHitPoints;
            Assert.IsTrue(fixture.Commands.ToggleGodMode());
            var blocked = fixture.Health.ApplyDamage(new DamageRequest(20f, DamageType.Physical));
            Assert.That(blocked.AppliedDamage, Is.Zero);
            Assert.That(fixture.Health.CurrentHitPoints, Is.EqualTo(initial));

            Assert.IsFalse(fixture.Commands.ToggleGodMode());
            fixture.Health.ApplyDamage(new DamageRequest(20f, DamageType.Physical));
            Assert.That(fixture.Health.CurrentHitPoints, Is.LessThan(initial));
        }

        [Test]
        public void ProfileReset_DeletesOnlyAuthoritativeProfileFiles()
        {
            var directory = Path.Combine(Path.GetTempPath(), "gravivore-s16-reset-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var serializer = new UnityJsonSaveSerializer();
                var repository = new JsonProfileRepository(
                    directory,
                    serializer,
                    new SaveMigrationPipeline(SaveSchema.CurrentVersion, serializer, new ISaveMigration[] { new SaveMigrationV0ToV1() }),
                    new SystemUtcTimeProvider(),
                    new SilentDiagnostics());
                File.WriteAllText(repository.MainPath, "main");
                File.WriteAllText(repository.BackupPath, "backup");
                File.WriteAllText(repository.TempPath, "temp");
                var unrelated = Path.Combine(directory, "keep-me.txt");
                File.WriteAllText(unrelated, "keep");

                repository.ResetProfileFilesForDevelopment();

                Assert.IsFalse(File.Exists(repository.MainPath));
                Assert.IsFalse(File.Exists(repository.BackupPath));
                Assert.IsFalse(File.Exists(repository.TempPath));
                Assert.IsTrue(File.Exists(unrelated));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void DevelopmentSources_AreCompileGuardedAndSaveSchemaIsUnchanged()
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var developmentDirectory = Path.Combine(projectRoot, "Assets", "_Game", "Runtime", "Presentation", "Development");
            foreach (var path in Directory.GetFiles(developmentDirectory, "*.cs"))
            {
                StringAssert.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD", File.ReadAllText(path));
            }

            Assert.That(SaveSchema.CurrentVersion, Is.EqualTo(1));
        }

        private static PlayerStatsState CreateStats()
        {
            var curve = new StatCurve(10, 10f, 1f, 0f, 0f, 1000f);
            var configuration = new PlayerStatsConfiguration(
                curve,
                new StatCurve(10, 100f, 10f, 0f, 1f, 10000f),
                curve,
                new StatCurve(10, 1f, 0f, 0f, 0.1f, 10f),
                curve,
                0.2f,
                10f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private sealed class CommandFixture : IDisposable
        {
            private readonly GameObject _playerObject;

            public CommandFixture()
            {
                Stats = CreateStats();
                var catalog = new QuestCatalog("chapter01", new[]
                {
                    new QuestObjective("elite-prerequisite", "Prerequisite", QuestObjectiveType.MovementPerformed, 2, string.Empty, string.Empty, string.Empty, QuestTargetType.None, string.Empty, false),
                    new QuestObjective("elite-objective", "Elite", QuestObjectiveType.EliteDefeated, 1, string.Empty, string.Empty, "magnetar-guard", QuestTargetType.Elite, "magnetar-guard", false),
                    new QuestObjective("boss-objective", "Boss", QuestObjectiveType.BossDefeated, 1, string.Empty, string.Empty, "custodian-m0", QuestTargetType.BossArena, "custodian-m0", false)
                });
                Progression = new AssimilationProgressionService(
                    Stats,
                    new ProgressionState(),
                    new ProgressionConfiguration(
                        new ProgressionThresholdCurve(10f, 0f, 0f, 10f),
                        new[] { new CoreReward("fixture-enemy", PlayerStatType.Power, 1f, 1) }));
                Quests = new QuestService(catalog, new QuestState(catalog), Progression);
                World = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
                EliteRequirement = new EliteGateRequirement(new[] { "elite-prerequisite" }, 7);
                WorldService = new WorldUnlockService(Progression, Quests, EliteRequirement, World);
                Completion = new BossCompletionState("custodian-m0");
                _playerObject = new GameObject("S16 Test Player", typeof(CharacterController), typeof(PlayerHealthController));
                Health = _playerObject.GetComponent<PlayerHealthController>();
                Health.Initialize(_playerObject.GetComponent<CharacterController>(), Stats, Vector3.zero, 0f);
                Commands = new DevelopmentCommandService(
                    Stats, Quests, WorldService, null, Completion, null, Health,
                    "magnetar-guard", "custodian-m0", () => DirtyCalls++, () => true, () => RestartCalls++);
            }

            public PlayerStatsState Stats { get; }
            public QuestService Quests { get; }
            public AssimilationProgressionService Progression { get; }
            public WorldUnlockState World { get; }
            public WorldUnlockService WorldService { get; }
            public EliteGateRequirement EliteRequirement { get; }
            public BossCompletionState Completion { get; }
            public PlayerHealthController Health { get; }
            public DevelopmentCommandService Commands { get; }
            public int DirtyCalls { get; private set; }
            public int RestartCalls { get; private set; }

            public void Dispose()
            {
                WorldService.Dispose();
                Quests.Dispose();
                Progression.Dispose();
                UnityEngine.Object.DestroyImmediate(_playerObject);
            }
        }

        private sealed class RecordingSink : IDevelopmentTelemetrySink
        {
            public readonly List<string> Lines = new List<string>();
            public void Append(string jsonLine) => Lines.Add(jsonLine);
        }

        private sealed class ThrowingSink : IDevelopmentTelemetrySink
        {
            public void Append(string jsonLine) => throw new IOException("expected telemetry failure");
        }

        private sealed class SilentDiagnostics : ISaveDiagnostics
        {
            public void Warning(string message, Exception exception = null) { }
            public void Error(string message, Exception exception) { }
        }
    }
}
#endif
