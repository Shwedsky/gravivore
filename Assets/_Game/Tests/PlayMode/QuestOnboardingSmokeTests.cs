using System;
using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Persistence.Quests;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Quests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Gravivore.Tests.PlayMode
{
    public sealed class QuestOnboardingSmokeTests
    {
        [UnityTest]
        public IEnumerator CanonicalChapter_ComposesQuestWorldAndInactiveEliteAfterOneFrame()
        {
            var operation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(operation);
            yield return operation;
            yield return null;

            var composition = FindCompositionRoot();
            Assert.IsNotNull(composition.Quests);
            Assert.IsNotNull(composition.WorldUnlocks);
            Assert.IsNotNull(composition.MagnetarGuard);
            Assert.IsNotNull(composition.BossCompletion);
            Assert.IsFalse(composition.WorldUnlocks.State.EliteGateUnlocked);
            Assert.IsFalse(composition.MagnetarGuard.IsEncounterActive);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Chapter01Onboarding_TracksMarkersUnlockAndRestore()
        {
            var root = new GameObject("S11 Onboarding Smoke Root");
            var hud = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler)).GetComponent<RectTransform>();
            hud.SetParent(root.transform, false);
            var input = new StubMovementInput();
            var catalog = CreateCatalog();
            using var progression = CreateProgression();
            var elite = new FakeEliteDefeatSource();
            var boss = new BossCompletionState("custodian-m0");
            using var quests = new QuestService(catalog, new QuestState(catalog), progression, elite, boss);
            var worldState = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            var requirement = new EliteGateRequirement(
                new[]
                {
                    "intro-relay-yard",
                    "intro-cutting-floor",
                    "intro-shield-dump",
                    "intro-capacitor-field",
                    "intro-hauler-graveyard"
                },
                25);
            using var world = new WorldUnlockService(
                progression,
                quests,
                requirement,
                worldState);
            var tracker = new GameObject("Tracker", typeof(QuestTrackerPresenter)).GetComponent<QuestTrackerPresenter>();
            tracker.transform.SetParent(root.transform, false);
            tracker.Initialize(quests, progression, requirement, CreateWorld(), new Vector3(0f, 0f, 18f), hud, _ => null);
            var movement = new GameObject("Movement", typeof(QuestMovementSignal)).GetComponent<QuestMovementSignal>();
            movement.Initialize(input, quests, 0.2f, 0.25f);

            Assert.That(tracker.CurrentTrackerText, Is.EqualTo("Move"));
            Assert.IsFalse(tracker.MarkerActive);

            input.Movement = Vector2.up;
            movement.Tick(0.3f);
            Assert.That(quests.ActiveObjective.Value.Id, Is.EqualTo("intro-relay-yard"));
            Assert.IsTrue(tracker.MarkerActive);

            progression.TryGrant(Death(1, "scout-drone"));
            Assert.That(quests.ActiveObjective.Value.Id, Is.EqualTo("intro-cutting-floor"));

            progression.TryGrant(Death(2, "cutter-unit"));
            Assert.IsTrue(quests.State.ExpandedObjectivesUnlocked);
            progression.TryGrant(Death(3, "warden"));
            progression.TryGrant(Death(4, "arc-drone"));
            progression.TryGrant(Death(5, "carrier"));
            Assert.That(progression.State.TotalAssimilationScore, Is.EqualTo(5));
            Assert.IsFalse(worldState.EliteGateUnlocked);
            Assert.That(tracker.CurrentTrackerText, Is.EqualTo("Assimilation 5/25"));
            Assert.IsFalse(tracker.CurrentTrackerText.Contains("Magnetar Guard"));
            Assert.IsFalse(tracker.MarkerActive);

            var dtoBeforeElite = QuestSaveMapper.ToDto(quests.State, catalog);
            var restoredBeforeEliteState = QuestSaveMapper.Restore(catalog, dtoBeforeElite);
            using var restoredBeforeElite = new QuestService(catalog, restoredBeforeEliteState);
            var restoredBeforeEliteTracker = new GameObject("Restored Farming Tracker", typeof(QuestTrackerPresenter)).GetComponent<QuestTrackerPresenter>();
            restoredBeforeEliteTracker.transform.SetParent(root.transform, false);
            restoredBeforeEliteTracker.Initialize(
                restoredBeforeElite,
                progression,
                requirement,
                CreateWorld(),
                new Vector3(0f, 0f, 18f),
                hud,
                _ => null);
            Assert.That(restoredBeforeEliteTracker.CurrentTrackerText, Is.EqualTo("Assimilation 5/25"));
            Assert.IsFalse(restoredBeforeEliteTracker.MarkerActive);

            for (var score = 6; score <= 24; score++)
            {
                progression.TryGrant(Death(100 + score, "scout-drone"));
            }

            Assert.That(progression.State.TotalAssimilationScore, Is.EqualTo(24));
            Assert.IsFalse(worldState.EliteGateUnlocked);
            Assert.That(tracker.CurrentTrackerText, Is.EqualTo("Assimilation 24/25"));
            Assert.IsFalse(tracker.MarkerActive);

            progression.TryGrant(Death(125, "scout-drone"));
            Assert.IsTrue(worldState.EliteGateUnlocked);
            Assert.That(quests.ActiveObjective.Value.Id, Is.EqualTo("defeat-magnetar-guard"));
            Assert.That(tracker.CurrentTrackerText, Is.EqualTo("Magnetar Guard"));
            Assert.IsTrue(tracker.MarkerActive);
            Assert.That(tracker.MarkerPosition, Is.EqualTo(new Vector3(0f, 3.4f, 18f)));

            elite.Raise("magnetar-guard");
            world.RecordEliteDefeated("magnetar-guard");
            Assert.That(quests.ActiveObjective.Value.Id, Is.EqualTo("defeat-custodian-m0"));

            var dto = QuestSaveMapper.ToDto(quests.State, catalog);
            var restoredState = QuestSaveMapper.Restore(catalog, dto);
            using var restoredQuests = new QuestService(catalog, restoredState);
            var restoredTracker = new GameObject("Restored Tracker", typeof(QuestTrackerPresenter)).GetComponent<QuestTrackerPresenter>();
            restoredTracker.transform.SetParent(root.transform, false);
            restoredTracker.Initialize(restoredQuests, progression, requirement, CreateWorld(), new Vector3(0f, 0f, 18f), hud, _ => null);
            Assert.That(restoredTracker.CurrentTrackerText, Is.EqualTo("Custodian M-0"));
            Assert.IsTrue(restoredTracker.MarkerActive);

            boss.TryRecordDefeat("custodian-m0", Vector3.zero);
            Assert.IsTrue(quests.State.Completed);
            Assert.That(quests.ActiveObjective.HasValue, Is.False);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static EnemyDeathEvent Death(int seed, string enemyId)
        {
            return new EnemyDeathEvent(new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])), enemyId, Vector3.zero);
        }

        private static S01SceneCompositionRoot FindCompositionRoot()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i].TryGetComponent<S01SceneCompositionRoot>(out var root)) return root;
            }

            Assert.Fail("Chapter scene composition root was not found.");
            return null;
        }

        private static Chapter01WorldConfiguration CreateWorld()
        {
            return new Chapter01WorldConfiguration(
                "chapter01-scrap-exclusion",
                Vector3.zero,
                new Vector3(0f, 0f, 8f),
                new Vector2(40f, 48f),
                new WorldBoundaryConfiguration(0.6f, 2.5f),
                new[]
                {
                    new WorldZoneConfiguration("relay-yard", new Vector3(-8f, 0f, 6f), new Vector3(-11f, 0f, 6f), Color.cyan),
                    new WorldZoneConfiguration("cutting-floor", new Vector3(0f, 0f, 10f), new Vector3(0f, 0f, 13.5f), Color.red),
                    new WorldZoneConfiguration("shield-dump", new Vector3(8f, 0f, 6f), new Vector3(11f, 0f, 6f), Color.green),
                    new WorldZoneConfiguration("capacitor-field", new Vector3(-7f, 0f, -7f), new Vector3(-10f, 0f, -9f), Color.yellow),
                    new WorldZoneConfiguration("hauler-graveyard", new Vector3(7f, 0f, -7f), new Vector3(10f, 0f, -9f), Color.magenta)
                },
                new WorldGateConfiguration("elite-gate", new Vector3(0f, 0f, 15f), new Vector3(5f, 2.5f, 0.6f)),
                new WorldGateConfiguration("boss-gate", new Vector3(0f, 0f, 21f), new Vector3(5f, 2.5f, 0.6f)),
                new Vector3(0f, 0f, 27f),
                5f,
                "magnetar-guard",
                new EliteGateRequirement(new[]
                {
                    "intro-relay-yard",
                    "intro-cutting-floor",
                    "intro-shield-dump",
                    "intro-capacitor-field",
                    "intro-hauler-graveyard"
                }, 25));
        }

        private static QuestCatalog CreateCatalog()
        {
            return new QuestCatalog(
                "chapter01-onboarding",
                new[]
                {
                    new QuestObjective("chapter01-movement", "Move", QuestObjectiveType.MovementPerformed, 1, string.Empty, string.Empty, string.Empty, QuestTargetType.None, string.Empty, false),
                    new QuestObjective("intro-relay-yard", "Relay Yard", QuestObjectiveType.EnemyDefeated, 1, "scout-drone", "relay-yard", string.Empty, QuestTargetType.FarmingZone, "relay-yard", false),
                    new QuestObjective("first-assimilation", "Assimilate core", QuestObjectiveType.AssimilationReceived, 1, "scout-drone", string.Empty, string.Empty, QuestTargetType.FarmingZone, "cutting-floor", false),
                    new QuestObjective("intro-cutting-floor", "Cutting Floor", QuestObjectiveType.EnemyDefeated, 1, "cutter-unit", "cutting-floor", string.Empty, QuestTargetType.FarmingZone, "cutting-floor", true),
                    new QuestObjective("intro-shield-dump", "Shield Dump", QuestObjectiveType.EnemyDefeated, 1, "warden", "shield-dump", string.Empty, QuestTargetType.FarmingZone, "shield-dump", false),
                    new QuestObjective("intro-capacitor-field", "Capacitor Field", QuestObjectiveType.EnemyDefeated, 1, "arc-drone", "capacitor-field", string.Empty, QuestTargetType.FarmingZone, "capacitor-field", false),
                    new QuestObjective("intro-hauler-graveyard", "Hauler Graveyard", QuestObjectiveType.EnemyDefeated, 1, "carrier", "hauler-graveyard", string.Empty, QuestTargetType.FarmingZone, "hauler-graveyard", false),
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
                        new CoreReward("scout-drone", PlayerStatType.Mobility, 1f, 1),
                        new CoreReward("cutter-unit", PlayerStatType.Power, 1f, 1),
                        new CoreReward("warden", PlayerStatType.Armor, 1f, 1),
                        new CoreReward("arc-drone", PlayerStatType.Flux, 1f, 1),
                        new CoreReward("carrier", PlayerStatType.Hull, 1f, 1)
                    }));
        }

        private sealed class StubMovementInput : IMovementInput
        {
            public Vector2 Movement { get; set; }
        }

        private sealed class FakeEliteDefeatSource : IMagnetarGuardDefeatSource
        {
            public event Action<MagnetarGuardDefeatedEvent> Defeated;
            public void Raise(string id) => Defeated?.Invoke(new MagnetarGuardDefeatedEvent(id, Vector3.zero));
        }
    }
}
