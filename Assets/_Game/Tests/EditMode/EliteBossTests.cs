using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class EliteBossTests
    {
        [Test]
        public void EncounterDisplacementClasses_KeepEliteReducedAndBossImmune()
        {
            var elite = CreateEliteConfiguration();
            var boss = CreateBossConfiguration();
            var policy = new DisplacementPolicy(0.35f);

            Assert.That(elite.DisplacementClass, Is.EqualTo(DisplacementClass.Elite));
            Assert.That(boss.DisplacementClass, Is.EqualTo(DisplacementClass.Boss));
            Assert.That(policy.CalculatePullDistance(10f, 2f, elite.DisplacementClass), Is.EqualTo(2.8f).Within(0.001f));
            Assert.That(policy.CalculatePullDistance(10f, 2f, boss.DisplacementClass), Is.Zero);
        }

        [Test]
        public void CircleGeometry_UsesImpactPositionAndIgnoresHeight()
        {
            Assert.IsTrue(BossAttackGeometry.IsInsideCircle(Vector3.zero, new Vector3(3f, 20f, 0f), 3f));
            Assert.IsFalse(BossAttackGeometry.IsInsideCircle(Vector3.zero, new Vector3(3.01f, 0f, 0f), 3f));
        }

        [Test]
        public void ConeGeometry_RejectsOutsideAngleAndRange()
        {
            Assert.IsTrue(BossAttackGeometry.IsInsideCone(Vector3.zero, Vector3.forward, new Vector3(1f, 0f, 3f), 5f, 30f));
            Assert.IsFalse(BossAttackGeometry.IsInsideCone(Vector3.zero, Vector3.forward, new Vector3(3f, 0f, 1f), 5f, 30f));
            Assert.IsFalse(BossAttackGeometry.IsInsideCone(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 5.01f), 5f, 30f));
        }

        [Test]
        public void LineGeometry_UsesFiniteCorridorAndChargeClampsToWorldBounds()
        {
            Assert.IsTrue(BossAttackGeometry.IsInsideLine(Vector3.zero, Vector3.forward * 5f, new Vector3(0.9f, 0f, 4f), 1f));
            Assert.IsFalse(BossAttackGeometry.IsInsideLine(Vector3.zero, Vector3.forward * 5f, new Vector3(1.1f, 0f, 4f), 1f));
            Assert.IsFalse(BossAttackGeometry.IsInsideLine(Vector3.zero, Vector3.forward * 5f, new Vector3(0f, 0f, 5.1f), 1f));

            var bounds = new WorldBounds(Vector3.zero, new Vector2(10f, 10f));
            var destination = BossAttackGeometry.ClampChargeDestination(
                new Vector3(0f, 2f, 4f), Vector3.forward, 10f, bounds, 1f);
            Assert.That(destination, Is.EqualTo(new Vector3(0f, 2f, 4f)));
        }

        [Test]
        public void BossStateMachine_OrdersTelegraphResolveRecoveryWithoutEarlyDamageSignal()
        {
            var machine = new CustodianBossStateMachine(CreateBossConfiguration());
            Assert.IsTrue(machine.Engage());

            var telegraph = machine.Tick(0f, 1f);
            Assert.That(telegraph.State, Is.EqualTo(CustodianBossState.Telegraphing));
            Assert.IsTrue(telegraph.TelegraphBegan);
            Assert.IsFalse(telegraph.ResolveAttack);
            Assert.That(telegraph.Attack, Is.EqualTo(BossAttackType.CirclePulse));

            var warning = machine.Tick(0.99f, 1f);
            Assert.IsFalse(warning.ResolveAttack);
            var impact = machine.Tick(0.02f, 1f);
            Assert.IsTrue(impact.ResolveAttack);
            Assert.That(impact.State, Is.EqualTo(CustodianBossState.ExecutingAttack));

            var recovery = machine.Tick(0f, 1f);
            Assert.That(recovery.State, Is.EqualTo(CustodianBossState.Recovery));
            Assert.IsFalse(recovery.TelegraphBegan);
            machine.Tick(0.99f, 1f);
            Assert.That(machine.State, Is.EqualTo(CustodianBossState.Recovery));
            machine.Tick(0.02f, 1f);
            Assert.That(machine.State, Is.EqualTo(CustodianBossState.Engaging));
        }

        [Test]
        public void LowHealthPhase_SwitchesOnceAndOnlyShortensRecovery()
        {
            var configuration = CreateBossConfiguration();
            var machine = new CustodianBossStateMachine(configuration);
            machine.Engage();
            var transition = machine.Tick(0f, 0.3f);
            Assert.IsTrue(transition.PhaseChanged);
            Assert.IsTrue(machine.IsLowHealthPhase);
            Assert.That(transition.Attack, Is.EqualTo(BossAttackType.CirclePulse));
            Assert.IsFalse(machine.Tick(0.1f, 0.2f).PhaseChanged);

            machine.Tick(1f, 0.2f);
            machine.Tick(0f, 0.2f);
            Assert.That(machine.State, Is.EqualTo(CustodianBossState.Recovery));
            Assert.That(machine.RemainingTime, Is.EqualTo(0.5f).Within(0.001f));
            machine.Tick(0.5f, 0.2f);
            var next = machine.Tick(0f, 0.2f);
            Assert.That(next.Attack, Is.EqualTo(BossAttackType.ConeSweep));
        }

        [Test]
        public void BossReset_ClearsTelegraphPhaseSequenceAndIsIdempotent()
        {
            var machine = new CustodianBossStateMachine(CreateBossConfiguration());
            machine.Engage();
            machine.Tick(0f, 0.3f);
            Assert.That(machine.State, Is.EqualTo(CustodianBossState.Telegraphing));
            Assert.IsTrue(machine.BeginReset());
            Assert.IsFalse(machine.IsLowHealthPhase);
            Assert.That(machine.CurrentAttack, Is.EqualTo(default(BossAttackType)));
            Assert.IsFalse(machine.BeginReset());
            machine.CompleteReset();
            Assert.That(machine.State, Is.EqualTo(CustodianBossState.Dormant));

            machine.Engage();
            Assert.That(machine.Tick(0f, 1f).Attack, Is.EqualTo(BossAttackType.CirclePulse));
        }

        [Test]
        public void BossCompletion_IsMonotonicTypedAndRestoresWithoutReplay()
        {
            var state = new BossCompletionState("custodian-m0");
            var count = 0;
            state.Defeated += defeated =>
            {
                Assert.That(defeated.BossId, Is.EqualTo("custodian-m0"));
                count++;
            };

            Assert.IsTrue(state.TryRecordDefeat("custodian-m0", Vector3.one));
            Assert.IsFalse(state.TryRecordDefeat("custodian-m0", Vector3.one));
            Assert.That(count, Is.EqualTo(1));
            var restored = BossCompletionState.Restore("custodian-m0", state.ExportSnapshot());
            Assert.IsTrue(restored.IsDefeated);
            Assert.IsFalse(restored.TryRecordDefeat("custodian-m0", Vector3.zero));
        }

        [Test]
        public void EliteDefeatBridge_CallsWorldUnlockOnceAndOpensBossGate()
        {
            using var progression = CreateProgression();
            var state = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            using var quests = CreateCompletedQuestService();
            using var world = new WorldUnlockService(
                progression,
                quests,
                new EliteGateRequirement(new[] { "intro-relay-yard" }, 1),
                state);
            progression.TryGrant(new EnemyDeathEvent(
                new EnemyLifeId(Guid.NewGuid()), "scout-drone", Vector3.zero));
            Assert.IsTrue(state.EliteGateUnlocked);
            var source = new FakeEliteDefeatSource();
            using var bridge = new EliteWorldUnlockBridge(source, world);
            var eliteEvents = 0;
            state.EliteWasDefeated += _ => eliteEvents++;

            source.Raise("magnetar-guard");
            source.Raise("magnetar-guard");

            Assert.IsTrue(state.BossGateUnlocked);
            Assert.That(eliteEvents, Is.EqualTo(1));
        }

        [Test]
        public void InvalidTelegraphAndPhaseConfiguration_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateBossConfiguration(telegraph: 0.79f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateBossConfiguration(lowHealthThreshold: 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateBossConfiguration(cadenceMultiplier: 0f));
        }

        [Test]
        public void LockedEliteGate_KeepsMagnetarCapabilitiesAiAndDamageInactiveUntilUnlock()
        {
            using var rig = new MagnetarControllerRig(new Vector3(0f, 0f, 14.9f));
            var world = new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
            var activations = 0;
            rig.Elite.Activated += _ => activations++;
            using var activation = new EliteEncounterActivationBridge(world, rig.Elite);
            var start = rig.Elite.transform.position;

            Assert.IsFalse(rig.Elite.IsEncounterActive);
            Assert.IsFalse(rig.Elite.CanBeTargeted);
            var lockedHealth = rig.Elite.CurrentHitPoints;
            Assert.That(rig.Elite.ApplyDamage(new DamageRequest(100f, DamageType.Gravity)).AppliedDamage, Is.Zero);
            Assert.That(rig.Elite.CurrentHitPoints, Is.EqualTo(lockedHealth));
            Assert.IsFalse(rig.Elite.TryDisplace(start + Vector3.forward, default));
            rig.Elite.Tick(10f);
            Assert.That(rig.Elite.State, Is.EqualTo(MagnetarGuardState.Waiting));
            Assert.That(rig.Elite.transform.position, Is.EqualTo(start));
            Assert.That(rig.PlayerDamage.DamageCount, Is.Zero, "Locked-gate player received a shockwave through the barrier.");

            Assert.IsTrue(world.TryUnlockEliteGate());
            Assert.IsTrue(rig.Elite.IsEncounterActive);
            Assert.IsTrue(rig.Elite.CanBeTargeted);
            Assert.That(activations, Is.EqualTo(1));
            Assert.IsFalse(world.TryUnlockEliteGate());
            Assert.IsFalse(activation.Synchronize());
            Assert.That(activations, Is.EqualTo(1));
        }

        [Test]
        public void RestoredUnlockedEliteGate_ActivatesImmediatelyWithoutMilestoneReplay()
        {
            using var rig = new MagnetarControllerRig(Vector3.zero);
            var world = WorldUnlockState.Restore(
                "elite-gate",
                "boss-gate",
                "magnetar-guard",
                new WorldUnlockSnapshot(true, false, false));
            var gateEvents = 0;
            var activations = 0;
            world.GateUnlocked += _ => gateEvents++;
            rig.Elite.Activated += _ => activations++;

            using var activation = new EliteEncounterActivationBridge(world, rig.Elite);

            Assert.IsTrue(rig.Elite.IsEncounterActive);
            Assert.That(activations, Is.EqualTo(1));
            Assert.That(gateEvents, Is.Zero);
        }

        [Test]
        public void MagnetarShockwave_ResolvesAgainstImmutableTelegraphSnapshotAfterPull()
        {
            using var rig = new MagnetarControllerRig(new Vector3(0f, 0f, 18f));
            rig.Elite.ActivateEncounter();
            EliteShockwaveTelegraphEvent telegraph = default;
            var telegraphCount = 0;
            rig.Elite.TelegraphStarted += value =>
            {
                telegraph = value;
                telegraphCount++;
            };

            rig.Elite.Tick(0f);
            Assert.That(telegraphCount, Is.EqualTo(1));
            Assert.IsTrue(rig.Elite.TryDisplace(telegraph.Origin + Vector3.forward * 5f, default));
            rig.Player.transform.position = telegraph.Origin;
            rig.Elite.Tick(1.01f);

            Assert.That(Vector3.Distance(rig.Elite.transform.position, telegraph.Origin), Is.GreaterThan(telegraph.Radius));
            Assert.That(rig.PlayerDamage.DamageCount, Is.EqualTo(1));
        }

        [Test]
        public void BossController_DoesNotDamageDuringTelegraphAndNeverDisplaces()
        {
            using var rig = new BossControllerRig(new Vector3(0f, 0f, 27f));
            var initialHealth = rig.Player.CurrentHitPoints;
            rig.Boss.Tick(0f);

            rig.Boss.Tick(0.99f);
            Assert.That(rig.Player.CurrentHitPoints, Is.EqualTo(initialHealth));
            var start = rig.Boss.transform.position;
            Assert.IsFalse(rig.Boss.TryDisplace(start + Vector3.left * 10f, default));
            Assert.That(rig.Boss.transform.position, Is.EqualTo(start));

            rig.Boss.Tick(0.02f);
            Assert.That(rig.Player.CurrentHitPoints, Is.LessThan(initialHealth));
        }

        [Test]
        public void BossController_ArenaExitAndPlayerDeathRestoreFullStartState()
        {
            using var rig = new BossControllerRig(new Vector3(0f, 0f, 27f));
            rig.Boss.Tick(0f);
            rig.Boss.ApplyDamage(new DamageRequest(100f, DamageType.Gravity));
            rig.Player.transform.position = new Vector3(10f, 0f, 27f);
            rig.Boss.Tick(0f);
            AssertReset(rig.Boss);

            rig.Player.transform.position = new Vector3(0f, 0f, 27f);
            rig.Boss.Tick(0f);
            rig.Boss.ApplyDamage(new DamageRequest(100f, DamageType.Gravity));
            rig.Player.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));
            AssertReset(rig.Boss);
        }

        [Test]
        public void BossController_DeathCompletesOnceAndDeadBossCannotAttack()
        {
            using var rig = new BossControllerRig(new Vector3(0f, 0f, 27f));
            rig.Boss.Tick(0f);
            var completions = 0;
            rig.Completion.Defeated += _ => completions++;
            Assert.IsTrue(rig.Boss.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity)).WasLethal);
            Assert.IsFalse(rig.Boss.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity)).WasLethal);
            var playerHealth = rig.Player.CurrentHitPoints;
            rig.Boss.Tick(100f);

            Assert.That(completions, Is.EqualTo(1));
            Assert.That(rig.Boss.State, Is.EqualTo(CustodianBossState.Dead));
            Assert.That(rig.Player.CurrentHitPoints, Is.EqualTo(playerHealth));
        }

        [Test]
        public void BossController_ClosedGateBlocksArenaBypassTargetingDamageAndCompletion()
        {
            using var rig = new BossControllerRig(new Vector3(0f, 0f, 27f), false);

            rig.Boss.Tick(0f);
            var blocked = rig.Boss.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));

            Assert.That(rig.Boss.State, Is.EqualTo(CustodianBossState.Dormant));
            Assert.IsFalse(rig.Boss.CanBeTargeted);
            Assert.That(blocked.AppliedDamage, Is.Zero);
            Assert.IsFalse(rig.Completion.IsDefeated);

            rig.Access.CanEngage = true;
            rig.Boss.Tick(0f);

            Assert.IsTrue(rig.Boss.CanBeTargeted);
            Assert.IsTrue(rig.Boss.ApplyDamage(
                new DamageRequest(10000f, DamageType.Gravity)).WasLethal);
            Assert.IsTrue(rig.Completion.IsDefeated);
        }

        internal static MagnetarGuardConfiguration CreateEliteConfiguration()
        {
            return new MagnetarGuardConfiguration(
                "magnetar-guard", new Vector3(0f, 0f, 18f), 250f, 10f, 1.8f, 14f,
                0.65f, 1.2f, 8f, 2.4f, 3.2f, 1f, 1.4f);
        }

        internal static CustodianBossConfiguration CreateBossConfiguration(
            float telegraph = 1f,
            float lowHealthThreshold = 0.35f,
            float cadenceMultiplier = 0.5f)
        {
            var attacks = new[]
            {
                new BossAttackConfiguration(BossAttackType.CirclePulse, telegraph, 20f, 4f, 0f, 0f, 0f),
                new BossAttackConfiguration(BossAttackType.ConeSweep, telegraph, 22f, 6f, 35f, 0f, 0f),
                new BossAttackConfiguration(BossAttackType.LineCharge, telegraph, 24f, 7f, 0f, 2f, 5f)
            };
            return new CustodianBossConfiguration(
                "custodian-m0", new Vector3(0f, 0f, 27f), new Vector3(0f, 0f, 27f), 5f,
                new WorldBounds(new Vector3(0f, 0f, 8f), new Vector2(40f, 48f)),
                1000f, 20f, 1f, 1.5f, 1f, lowHealthThreshold, cadenceMultiplier,
                attacks, new[] { BossAttackType.CirclePulse, BossAttackType.ConeSweep, BossAttackType.LineCharge });
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
            var state = QuestState.Restore(
                catalog,
                new QuestSnapshot(
                    catalog.QuestId,
                    new System.Collections.Generic.Dictionary<string, int>(StringComparer.Ordinal)
                    {
                        { "intro-relay-yard", 1 }
                    },
                    new[] { "intro-relay-yard" },
                    Array.Empty<EnemyLifeId>(),
                    false,
                    false));
            return new QuestService(catalog, state);
        }

        private sealed class FakeEliteDefeatSource : IMagnetarGuardDefeatSource
        {
            public event Action<MagnetarGuardDefeatedEvent> Defeated;
            public void Raise(string id) => Defeated?.Invoke(new MagnetarGuardDefeatedEvent(id, Vector3.zero));
        }

        private static void AssertReset(CustodianBossController boss)
        {
            Assert.That(boss.State, Is.EqualTo(CustodianBossState.Dormant));
            Assert.That(boss.CurrentHitPoints, Is.EqualTo(boss.MaximumHitPoints));
            Assert.That(boss.transform.position, Is.EqualTo(new Vector3(0f, 0f, 27f)));
            Assert.IsFalse(boss.IsLowHealthPhase);
        }

        private sealed class BossControllerRig : IDisposable
        {
            private readonly GameObject _root;

            public BossControllerRig(Vector3 playerPosition, bool canEngage = true)
            {
                _root = new GameObject("S09 EditMode Boss Rig");
                var playerObject = new GameObject("Player", typeof(CharacterController), typeof(PlayerHealthController));
                playerObject.transform.SetParent(_root.transform, false);
                playerObject.transform.position = playerPosition;
                Player = playerObject.GetComponent<PlayerHealthController>();
                Player.Initialize(
                    playerObject.GetComponent<CharacterController>(),
                    CreatePlayerStats(),
                    Vector3.zero,
                    0f);

                var bossObject = new GameObject("Boss", typeof(CharacterController), typeof(CustodianBossController));
                bossObject.transform.SetParent(_root.transform, false);
                var sensorObject = new GameObject("Combat Target Sensor", typeof(SphereCollider));
                sensorObject.layer = 9;
                sensorObject.transform.SetParent(bossObject.transform, false);
                Completion = new BossCompletionState("custodian-m0");
                Access = new MutableBossEncounterAccess(canEngage);
                Boss = bossObject.GetComponent<CustodianBossController>();
                Boss.Initialize(
                    bossObject.GetComponent<CharacterController>(),
                    sensorObject.transform,
                    sensorObject.GetComponent<Collider>(),
                    9,
                    CreateBossConfiguration(),
                    playerObject.transform,
                    Player,
                    new PassthroughPullResolver(),
                    Access,
                    Completion);
            }

            public PlayerHealthController Player { get; }
            public CustodianBossController Boss { get; }
            public BossCompletionState Completion { get; }
            public MutableBossEncounterAccess Access { get; }

            public void Dispose()
            {
                if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
            }
        }

        private sealed class MagnetarControllerRig : IDisposable
        {
            private readonly GameObject _root;

            public MagnetarControllerRig(Vector3 playerPosition)
            {
                _root = new GameObject("S09 EditMode Magnetar Rig");
                Player = new GameObject("Player");
                Player.transform.SetParent(_root.transform, false);
                Player.transform.position = playerPosition;
                PlayerDamage = new RecordingDamageable();

                var eliteObject = new GameObject("Magnetar Guard", typeof(CharacterController), typeof(MagnetarGuardController));
                eliteObject.transform.SetParent(_root.transform, false);
                var sensorObject = new GameObject("Combat Target Sensor", typeof(SphereCollider));
                sensorObject.layer = 9;
                sensorObject.transform.SetParent(eliteObject.transform, false);
                Elite = eliteObject.GetComponent<MagnetarGuardController>();
                Elite.Initialize(
                    eliteObject.GetComponent<CharacterController>(),
                    sensorObject.transform,
                    sensorObject.GetComponent<Collider>(),
                    9,
                    CreateEliteConfiguration(),
                    Player.transform,
                    PlayerDamage);
            }

            public GameObject Player { get; }
            public RecordingDamageable PlayerDamage { get; }
            public MagnetarGuardController Elite { get; }

            public void Dispose()
            {
                if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
            }
        }

        private sealed class RecordingDamageable : IDamageable
        {
            public bool IsAlive => true;
            public int DamageCount { get; private set; }

            public DamageResult ApplyDamage(in DamageRequest request)
            {
                DamageCount++;
                return new DamageResult(request.RawDamage, false);
            }
        }

        private static PlayerStatsState CreatePlayerStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 100f, 1f, 0f, 1f, 1000f),
                new StatCurve(10, 0f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                new StatCurve(10, 4f, 0f, 0f, 0f, 20f),
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private sealed class PassthroughPullResolver : IPullDestinationResolver
        {
            public Vector3 Resolve(Vector3 origin, Vector3 requestedDestination, float collisionRadius)
            {
                return requestedDestination;
            }
        }

        private sealed class MutableBossEncounterAccess : IBossEncounterAccess
        {
            public MutableBossEncounterAccess(bool canEngage) => CanEngage = canEngage;
            public bool CanEngage { get; set; }
        }
    }
}
