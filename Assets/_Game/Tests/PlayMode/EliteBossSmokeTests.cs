using System;
using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class EliteBossSmokeTests
    {
        [UnityTest]
        public IEnumerator MagnetarGuard_IsTargetableUsesReducedPullAndUnlocksBossGateOnce()
        {
            var root = new GameObject("S09 Elite Smoke Root");
            var player = CreatePlayer(root.transform, Vector3.zero, Vector3.zero);
            var elite = CreateElite(root.transform, player);
            using var progression = CreateProgression();
            var worldState = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            using var quests = CreateCompletedQuestService();
            using var world = new WorldUnlockService(
                progression,
                quests,
                new EliteGateRequirement(new[] { "intro-relay-yard" }, 1),
                worldState);
            progression.TryGrant(new EnemyDeathEvent(
                new EnemyLifeId(Guid.NewGuid()), "scout-drone", Vector3.zero));
            using var bridge = new EliteWorldUnlockBridge(elite, world);
            var defeatCount = 0;
            elite.Defeated += _ => defeatCount++;

            Assert.IsTrue(elite.CanBeTargeted);
            Assert.That(elite.DisplacementClass, Is.EqualTo(DisplacementClass.Elite));
            var start = elite.transform.position;
            var policy = new DisplacementPolicy(0.35f);
            var standardDistance = policy.CalculatePullDistance(6f, 1f, DisplacementClass.Standard);
            var eliteDistance = policy.CalculatePullDistance(6f, 1f, elite.DisplacementClass);
            Assert.IsTrue(elite.TryDisplace(start + Vector3.back * eliteDistance, default));
            Assert.That(Vector3.Distance(start, elite.transform.position), Is.LessThan(standardDistance));

            var lethal = elite.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            var repeated = elite.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            Assert.IsTrue(lethal.WasLethal);
            Assert.IsFalse(repeated.WasLethal);
            Assert.That(defeatCount, Is.EqualTo(1));
            Assert.IsTrue(worldState.BossGateUnlocked);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossSequence_ShowsEveryTelegraphAndImpactChecksCurrentPlayerGeometry()
        {
            var root = new GameObject("S09 Telegraph Smoke Root");
            var player = CreatePlayer(root.transform, new Vector3(0f, 0f, 31.5f), Vector3.zero);
            var elite = CreateElite(root.transform, player);
            var boss = CreateBoss(root.transform, player, out var completion);
            var presentation = new GameObject("Encounter Telegraphs", typeof(EncounterTelegraphPresenter))
                .GetComponent<EncounterTelegraphPresenter>();
            presentation.transform.SetParent(root.transform, false);
            presentation.Initialize(elite, boss, completion);
            var initialHp = player.CurrentHitPoints;

            boss.Tick(0f);
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Telegraphing));
            Assert.IsTrue(presentation.BossTelegraphVisible);
            Assert.That(presentation.CurrentBossAttack, Is.EqualTo(BossAttackType.CirclePulse));
            boss.Tick(0.99f);
            Assert.That(player.CurrentHitPoints, Is.EqualTo(initialHp), "Circle dealt damage during warning.");
            boss.Tick(0.02f);
            Assert.That(player.CurrentHitPoints, Is.EqualTo(initialHp), "Player outside circle was hit at impact.");

            AdvanceToNextTelegraph(boss);
            Assert.IsTrue(presentation.BossTelegraphVisible);
            Assert.That(presentation.CurrentBossAttack, Is.EqualTo(BossAttackType.ConeSweep));
            player.transform.position = new Vector3(0f, 0f, 23f);
            boss.Tick(1.01f);
            Assert.That(player.CurrentHitPoints, Is.EqualTo(initialHp), "Player outside locked cone direction was hit.");

            AdvanceToNextTelegraph(boss);
            Assert.IsTrue(presentation.BossTelegraphVisible);
            Assert.That(presentation.CurrentBossAttack, Is.EqualTo(BossAttackType.LineCharge));
            player.transform.position = new Vector3(4f, 0f, 27f);
            boss.Tick(1.01f);
            Assert.That(player.CurrentHitPoints, Is.EqualTo(initialHp), "Player outside charge corridor was hit.");

            presentation.Shutdown();
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerDeathAndArenaExit_ResetBossHealthPositionAndTransientState()
        {
            var root = new GameObject("S09 Reset Smoke Root");
            var player = CreatePlayer(root.transform, new Vector3(0f, 0f, 27f), Vector3.zero);
            var boss = CreateBoss(root.transform, player, out _);
            boss.Tick(0f);
            boss.ApplyDamage(new DamageRequest(100f, DamageType.Gravity));
            Assert.That(boss.CurrentHitPoints, Is.LessThan(boss.MaximumHitPoints));

            player.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Dormant));
            Assert.That(boss.CurrentHitPoints, Is.EqualTo(boss.MaximumHitPoints));
            Assert.That(boss.transform.position, Is.EqualTo(new Vector3(0f, 0f, 27f)));

            player.transform.position = new Vector3(0f, 0f, 27f);
            boss.Tick(0f);
            boss.ApplyDamage(new DamageRequest(100f, DamageType.Gravity));
            player.transform.position = new Vector3(10f, 0f, 27f);
            boss.Tick(0f);
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Dormant));
            Assert.That(boss.CurrentHitPoints, Is.EqualTo(boss.MaximumHitPoints));
            Assert.IsFalse(boss.ResetEncounter(), "Repeated reset must be idempotent while dormant.");

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Boss_TakesDamageNeverDisplacesAndCompletesExactlyOnce()
        {
            var root = new GameObject("S09 Completion Smoke Root");
            var player = CreatePlayer(root.transform, new Vector3(0f, 0f, 27f), Vector3.zero);
            var boss = CreateBoss(root.transform, player, out var completion);
            boss.Tick(0f);
            var start = boss.transform.position;
            var completionCount = 0;
            completion.Defeated += defeated =>
            {
                Assert.That(defeated.BossId, Is.EqualTo("custodian-m0"));
                completionCount++;
            };

            var nonLethal = boss.ApplyDamage(new DamageRequest(10f, DamageType.Gravity));
            Assert.That(nonLethal.AppliedDamage, Is.GreaterThan(0f));
            Assert.IsFalse(boss.TryDisplace(start + Vector3.left * 10f, default));
            Assert.That(boss.transform.position, Is.EqualTo(start));
            var lethal = boss.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            var repeated = boss.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            boss.Tick(100f);

            Assert.IsTrue(lethal.WasLethal);
            Assert.That(repeated.AppliedDamage, Is.Zero);
            Assert.That(completionCount, Is.EqualTo(1));
            Assert.IsTrue(completion.IsDefeated);
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Dead));
            Assert.IsFalse(boss.CanBeTargeted);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RestoredBossDefeat_StartsDeadAndCannotReengage()
        {
            var root = new GameObject("S12 Restored Boss Root");
            var player = CreatePlayer(root.transform, new Vector3(0f, 0f, 27f), Vector3.zero);
            var completion = BossCompletionState.Restore(
                "custodian-m0",
                new BossCompletionSnapshot(true));
            var boss = CreateBoss(root.transform, player, completion);

            Assert.IsTrue(completion.IsDefeated);
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Dead));
            Assert.IsFalse(boss.CanBeTargeted);
            boss.Tick(100f);
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Dead));
            Assert.That(boss.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity)).AppliedDamage, Is.Zero);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MagnetarDeathDuringTelegraph_HidesIndicatorAndCannotResolveDamage()
        {
            var root = new GameObject("S09 Elite Telegraph Death Root");
            var player = CreatePlayer(root.transform, new Vector3(0f, 0f, 18f), Vector3.zero);
            var elite = CreateElite(root.transform, player);
            var boss = CreateBoss(root.transform, player, out var completion);
            var presentation = new GameObject("Encounter Telegraphs", typeof(EncounterTelegraphPresenter))
                .GetComponent<EncounterTelegraphPresenter>();
            presentation.transform.SetParent(root.transform, false);
            presentation.Initialize(elite, boss, completion);
            var initialHealth = player.CurrentHitPoints;
            var cancellationCount = 0;
            var resolveCount = 0;
            elite.ShockwaveCancelled += _ => cancellationCount++;
            elite.ShockwaveResolved += _ => resolveCount++;

            elite.Tick(0f);
            Assert.IsTrue(presentation.EliteTelegraphVisible);
            elite.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            elite.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            Assert.IsFalse(presentation.EliteTelegraphVisible);
            elite.Tick(2f);
            Assert.That(player.CurrentHitPoints, Is.EqualTo(initialHealth));
            Assert.That(cancellationCount, Is.EqualTo(1));
            Assert.That(resolveCount, Is.Zero);

            presentation.Shutdown();
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static void AdvanceToNextTelegraph(CustodianBossController boss)
        {
            boss.Tick(0f);
            boss.Tick(1f);
            boss.Tick(0f);
        }

        private static PlayerHealthController CreatePlayer(Transform parent, Vector3 position, Vector3 respawn)
        {
            var gameObject = new GameObject("S09 Player", typeof(CharacterController), typeof(PlayerHealthController));
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.position = position;
            var health = gameObject.GetComponent<PlayerHealthController>();
            health.Initialize(gameObject.GetComponent<CharacterController>(), CreatePlayerStats(), respawn, 0f);
            return health;
        }

        private static MagnetarGuardController CreateElite(Transform parent, PlayerHealthController player)
        {
            var gameObject = new GameObject("Magnetar Guard Test", typeof(CharacterController), typeof(MagnetarGuardController));
            gameObject.transform.SetParent(parent, false);
            var target = new GameObject("Combat Target Sensor", typeof(SphereCollider));
            target.layer = 9;
            target.transform.SetParent(gameObject.transform, false);
            var controller = gameObject.GetComponent<MagnetarGuardController>();
            controller.Initialize(
                gameObject.GetComponent<CharacterController>(), target.transform, target.GetComponent<Collider>(), 9,
                CreateEliteConfiguration(), player.transform, player);
            controller.ActivateEncounter();
            return controller;
        }

        private static CustodianBossController CreateBoss(
            Transform parent,
            PlayerHealthController player,
            out BossCompletionState completion)
        {
            completion = new BossCompletionState("custodian-m0");
            return CreateBoss(parent, player, completion);
        }

        private static CustodianBossController CreateBoss(
            Transform parent,
            PlayerHealthController player,
            BossCompletionState completion)
        {
            var gameObject = new GameObject("Custodian M-0 Test", typeof(CharacterController), typeof(CustodianBossController));
            gameObject.transform.SetParent(parent, false);
            var target = new GameObject("Combat Target Sensor", typeof(SphereCollider));
            target.layer = 9;
            target.transform.SetParent(gameObject.transform, false);
            var controller = gameObject.GetComponent<CustodianBossController>();
            controller.Initialize(
                gameObject.GetComponent<CharacterController>(), target.transform, target.GetComponent<Collider>(), 9,
                CreateBossConfiguration(), player.transform, player, new PassthroughPullResolver(), completion);
            return controller;
        }

        private static MagnetarGuardConfiguration CreateEliteConfiguration()
        {
            return new MagnetarGuardConfiguration(
                "magnetar-guard", new Vector3(0f, 0f, 18f), 250f, 10f, 1.8f, 14f,
                0.65f, 1.2f, 8f, 2.4f, 3.2f, 1f, 1.4f);
        }

        private static CustodianBossConfiguration CreateBossConfiguration()
        {
            return new CustodianBossConfiguration(
                "custodian-m0", new Vector3(0f, 0f, 27f), new Vector3(0f, 0f, 27f), 5f,
                new WorldBounds(new Vector3(0f, 0f, 8f), new Vector2(40f, 48f)),
                1000f, 20f, 1f, 1.5f, 1f, 0.35f, 0.65f,
                new[]
                {
                    new BossAttackConfiguration(BossAttackType.CirclePulse, 1f, 22f, 4f, 0f, 0f, 0f),
                    new BossAttackConfiguration(BossAttackType.ConeSweep, 1f, 24f, 6f, 35f, 0f, 0f),
                    new BossAttackConfiguration(BossAttackType.LineCharge, 1f, 28f, 7f, 0f, 1.8f, 5f)
                },
                new[] { BossAttackType.CirclePulse, BossAttackType.ConeSweep, BossAttackType.LineCharge });
        }

        private static PlayerStatsState CreatePlayerStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 100f, 1f, 0f, 1f, 1000f),
                new StatCurve(10, 0f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                new StatCurve(10, 4f, 0f, 0f, 0f, 20f),
                0.2f, 20f, new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private static AssimilationProgressionService CreateProgression()
        {
            var stats = CreatePlayerStats();
            return new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(
                    new ProgressionThresholdCurve(100f, 0f, 0f, 1000f),
                    new[] { new CoreReward("scout-drone", PlayerStatType.Power, 1f, 1) }));
        }

        private static QuestService CreateCompletedQuestService()
        {
            var catalog = new QuestCatalog(
                "chapter01-onboarding",
                new[]
                {
                    new QuestObjective(
                        "intro-relay-yard",
                        "Relay Yard",
                        QuestObjectiveType.EnemyDefeated,
                        1,
                        "scout-drone",
                        "relay-yard",
                        string.Empty,
                        QuestTargetType.FarmingZone,
                        "relay-yard",
                        false)
                });
            var progress = new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal)
            {
                { "intro-relay-yard", 1 }
            };
            var state = QuestState.Restore(
                catalog,
                new QuestSnapshot(
                    catalog.QuestId,
                    progress,
                    new[] { "intro-relay-yard" },
                    Array.Empty<EnemyLifeId>(),
                    false,
                    false));
            return new QuestService(catalog, state);
        }

        private sealed class PassthroughPullResolver : IPullDestinationResolver
        {
            public Vector3 Resolve(Vector3 origin, Vector3 requestedDestination, float collisionRadius)
            {
                return requestedDestination;
            }
        }
    }
}
