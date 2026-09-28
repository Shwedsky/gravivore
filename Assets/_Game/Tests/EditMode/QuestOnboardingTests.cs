using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Persistence.Quests;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class QuestOnboardingTests
    {
        private static readonly string[] SpotObjectiveIds =
        {
            "intro-relay-yard", "intro-cutting-floor", "intro-shield-dump", "intro-capacitor-field", "intro-hauler-graveyard"
        };

        private static readonly string[] EnemyIds =
        {
            "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier"
        };

        [Test]
        public void FreshState_StartsMovementObjective()
        {
            using var rig = QuestRig.Create();
            Assert.That(rig.Quests.ActiveObjective.Value.Id, Is.EqualTo("chapter01-movement"));
        }

        [Test]
        public void Movement_CompletesOnceAndAdvancesToScout()
        {
            using var rig = QuestRig.Create();
            var completed = 0;
            rig.Quests.ObjectiveCompleted += objective =>
            {
                if (objective.Objective.Id == "chapter01-movement") completed++;
            };

            Assert.IsTrue(rig.Quests.RecordMovementPerformed());
            Assert.IsFalse(rig.Quests.RecordMovementPerformed());
            Assert.That(completed, Is.EqualTo(1));
            Assert.That(rig.Quests.ActiveObjective.Value.Id, Is.EqualTo("intro-relay-yard"));
        }

        [Test]
        public void ScoutAndFirstAssimilation_CompleteAfterAuthoritativeReward()
        {
            using var rig = QuestRig.Create();
            rig.Quests.RecordMovementPerformed();
            rig.Progression.TryGrant(Death(1, "scout-drone"));

            Assert.IsTrue(rig.Quests.State.IsObjectiveCompleted("intro-relay-yard"));
            Assert.IsTrue(rig.Quests.State.IsObjectiveCompleted("first-assimilation"));
            Assert.That(rig.Quests.ActiveObjective.Value.Id, Is.EqualTo("intro-cutting-floor"));
        }

        [Test]
        public void FiveSpotObjectives_ProgressIndependently()
        {
            using var rig = QuestRig.Create();
            CompleteMovementAndScout(rig);
            for (var i = 1; i < EnemyIds.Length; i++)
            {
                rig.Progression.TryGrant(Death(10 + i, EnemyIds[i]));
                Assert.IsTrue(rig.Quests.State.IsObjectiveCompleted(SpotObjectiveIds[i]));
            }
        }

        [Test]
        public void DuplicateSameEnemyLife_DoesNotDoubleCount()
        {
            using var rig = QuestRig.Create(requiredSpotCount: 2);
            rig.Quests.RecordMovementPerformed();
            var death = Death(1, "scout-drone");
            rig.Progression.TryGrant(death);
            rig.Progression.TryGrant(death);

            Assert.That(rig.Quests.State.GetProgress("intro-relay-yard"), Is.EqualTo(1));
        }

        [Test]
        public void SamePooledComponentWithNewLife_CountsAsNewKill()
        {
            using var rig = QuestRig.Create(requiredSpotCount: 2);
            rig.Quests.RecordMovementPerformed();
            rig.Progression.TryGrant(Death(1, "scout-drone"));
            rig.Progression.TryGrant(Death(2, "scout-drone"));

            Assert.IsTrue(rig.Quests.State.IsObjectiveCompleted("intro-relay-yard"));
        }

        [Test]
        public void EliteAndBossObjectives_AdvanceOnce()
        {
            using var rig = QuestRig.Create();
            CompleteAllSpotObjectives(rig);
            var eliteCount = 0;
            var bossCount = 0;
            rig.Quests.ObjectiveCompleted += completed =>
            {
                if (completed.Objective.Id == "defeat-magnetar-guard") eliteCount++;
                if (completed.Objective.Id == "defeat-custodian-m0") bossCount++;
            };

            rig.Elite.Raise("magnetar-guard");
            rig.Elite.Raise("magnetar-guard");
            rig.Boss.TryRecordDefeat("custodian-m0", Vector3.zero);
            rig.Boss.TryRecordDefeat("custodian-m0", Vector3.zero);

            Assert.That(eliteCount, Is.EqualTo(1));
            Assert.That(bossCount, Is.EqualTo(1));
            Assert.IsTrue(rig.Quests.State.Completed);
        }

        [Test]
        public void SnapshotDto_RoundTripsWithoutReplay()
        {
            using var rig = QuestRig.Create();
            CompleteMovementAndScout(rig);
            rig.Progression.TryGrant(Death(20, "cutter-unit"));
            var dto = QuestSaveMapper.ToDto(rig.Quests.State, rig.Catalog);
            var restoredState = QuestSaveMapper.Restore(rig.Catalog, dto);
            using var restored = new QuestService(rig.Catalog, restoredState);
            var replayed = 0;
            restored.ObjectiveCompleted += _ => replayed++;

            Assert.IsTrue(restored.State.IsObjectiveCompleted("intro-relay-yard"));
            Assert.IsTrue(restored.State.IsObjectiveCompleted("intro-cutting-floor"));
            Assert.That(restored.ActiveObjective.Value.Id, Is.EqualTo("intro-shield-dump"));
            Assert.That(replayed, Is.Zero);
        }

        [Test]
        public void InvalidQuestOrObjectiveId_IsRejected()
        {
            using var rig = QuestRig.Create();
            var dto = QuestSaveMapper.ToDto(rig.Quests.State, rig.Catalog);
            dto.questId = "other";
            Assert.Throws<ArgumentException>(() => QuestSaveMapper.Restore(rig.Catalog, dto));

            dto.questId = rig.Catalog.QuestId;
            dto.objectives[0].objectiveId = "missing";
            Assert.Throws<ArgumentException>(() => QuestSaveMapper.Restore(rig.Catalog, dto));
        }

        private static void CompleteMovementAndScout(QuestRig rig)
        {
            rig.Quests.RecordMovementPerformed();
            rig.Progression.TryGrant(Death(1, "scout-drone"));
        }

        private static void CompleteAllSpotObjectives(QuestRig rig)
        {
            CompleteMovementAndScout(rig);
            for (var i = 1; i < EnemyIds.Length; i++) rig.Progression.TryGrant(Death(10 + i, EnemyIds[i]));
        }

        private static EnemyDeathEvent Death(int seed, string enemyId)
        {
            return new EnemyDeathEvent(new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])), enemyId, Vector3.zero);
        }

        private sealed class QuestRig : IDisposable
        {
            private QuestRig(QuestCatalog catalog, AssimilationProgressionService progression, FakeEliteDefeatSource elite, BossCompletionState boss)
            {
                Catalog = catalog;
                Progression = progression;
                Elite = elite;
                Boss = boss;
                Quests = new QuestService(catalog, new QuestState(catalog), progression, elite, boss);
            }

            public QuestCatalog Catalog { get; }
            public AssimilationProgressionService Progression { get; }
            public FakeEliteDefeatSource Elite { get; }
            public BossCompletionState Boss { get; }
            public QuestService Quests { get; }

            public static QuestRig Create(int requiredSpotCount = 1)
            {
                return new QuestRig(CreateCatalog(requiredSpotCount), CreateProgression(), new FakeEliteDefeatSource(), new BossCompletionState("custodian-m0"));
            }

            public void Dispose()
            {
                Quests.Dispose();
                Progression.Dispose();
            }
        }

        private sealed class FakeEliteDefeatSource : IMagnetarGuardDefeatSource
        {
            public event Action<MagnetarGuardDefeatedEvent> Defeated;
            public void Raise(string id) => Defeated?.Invoke(new MagnetarGuardDefeatedEvent(id, Vector3.zero));
        }

        private static QuestCatalog CreateCatalog(int requiredSpotCount)
        {
            return new QuestCatalog(
                "chapter01-onboarding",
                new[]
                {
                    new QuestObjective("chapter01-movement", "Move", QuestObjectiveType.MovementPerformed, 1, string.Empty, string.Empty, string.Empty, QuestTargetType.None, string.Empty, false),
                    new QuestObjective(SpotObjectiveIds[0], "Relay Yard", QuestObjectiveType.EnemyDefeated, requiredSpotCount, EnemyIds[0], "relay-yard", string.Empty, QuestTargetType.FarmingZone, "relay-yard", false),
                    new QuestObjective("first-assimilation", "Assimilate core", QuestObjectiveType.AssimilationReceived, 1, EnemyIds[0], string.Empty, string.Empty, QuestTargetType.FarmingZone, "cutting-floor", false),
                    new QuestObjective(SpotObjectiveIds[1], "Cutting Floor", QuestObjectiveType.EnemyDefeated, 1, EnemyIds[1], "cutting-floor", string.Empty, QuestTargetType.FarmingZone, "cutting-floor", true),
                    new QuestObjective(SpotObjectiveIds[2], "Shield Dump", QuestObjectiveType.EnemyDefeated, 1, EnemyIds[2], "shield-dump", string.Empty, QuestTargetType.FarmingZone, "shield-dump", false),
                    new QuestObjective(SpotObjectiveIds[3], "Capacitor Field", QuestObjectiveType.EnemyDefeated, 1, EnemyIds[3], "capacitor-field", string.Empty, QuestTargetType.FarmingZone, "capacitor-field", false),
                    new QuestObjective(SpotObjectiveIds[4], "Hauler Graveyard", QuestObjectiveType.EnemyDefeated, 1, EnemyIds[4], "hauler-graveyard", string.Empty, QuestTargetType.FarmingZone, "hauler-graveyard", false),
                    new QuestObjective("defeat-magnetar-guard", "Magnetar Guard", QuestObjectiveType.EliteDefeated, 1, string.Empty, string.Empty, "magnetar-guard", QuestTargetType.Elite, "magnetar-guard", false),
                    new QuestObjective("defeat-custodian-m0", "Custodian M-0", QuestObjectiveType.BossDefeated, 1, string.Empty, string.Empty, "custodian-m0", QuestTargetType.BossArena, "custodian-m0", false)
                });
        }

        private static AssimilationProgressionService CreateProgression()
        {
            var curve = new StatCurve(10, 1f, 1f, 0f, 0f, 1000f);
            var configuration = new PlayerStatsConfiguration(
                curve,
                new StatCurve(10, 100f, 1f, 0f, 1f, 1000f),
                curve,
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                curve,
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            var stats = new PlayerStatsState(configuration, configuration.StartingLevels);
            return new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(
                    new ProgressionThresholdCurve(100f, 0f, 0f, 1000f),
                    new[]
                    {
                        new CoreReward(EnemyIds[0], PlayerStatType.Mobility, 1f, 1),
                        new CoreReward(EnemyIds[1], PlayerStatType.Power, 1f, 1),
                        new CoreReward(EnemyIds[2], PlayerStatType.Armor, 1f, 1),
                        new CoreReward(EnemyIds[3], PlayerStatType.Flux, 1f, 1),
                        new CoreReward(EnemyIds[4], PlayerStatType.Hull, 1f, 1)
                    }));
        }
    }
}
