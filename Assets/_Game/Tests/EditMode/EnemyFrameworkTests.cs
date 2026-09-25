using System;
using Gravivore.Gameplay.Enemies;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class EnemyFrameworkTests
    {
        [Test]
        public void StateMachine_TransitionsIdleApproachAttack()
        {
            var brain = CreateBrain();

            Assert.That(brain.Tick(0f, true, 8f).State, Is.EqualTo(OrdinaryEnemyBrainState.Idle));
            var approach = brain.Tick(0f, true, 4f);
            Assert.That(approach.State, Is.EqualTo(OrdinaryEnemyBrainState.Approach));
            Assert.IsTrue(approach.ShouldApproach);
            var attack = brain.Tick(0f, true, 1f);
            Assert.That(attack.State, Is.EqualTo(OrdinaryEnemyBrainState.Attack));
            Assert.IsTrue(attack.ShouldAttack);
        }

        [Test]
        public void StateMachine_AggroUsesEntryAndReleaseRadii()
        {
            var brain = CreateBrain();

            Assert.That(brain.Tick(0f, true, 5.5f).State, Is.EqualTo(OrdinaryEnemyBrainState.Idle));
            Assert.That(brain.Tick(0f, true, 4.5f).State, Is.EqualTo(OrdinaryEnemyBrainState.Approach));
            Assert.That(brain.Tick(0f, true, 6f).State, Is.EqualTo(OrdinaryEnemyBrainState.Approach));
            Assert.That(brain.Tick(0f, true, 7.1f).State, Is.EqualTo(OrdinaryEnemyBrainState.Idle));
        }

        [Test]
        public void StateMachine_AttackCadenceWaitsForConfiguredInterval()
        {
            var brain = CreateBrain();

            Assert.IsTrue(brain.Tick(0f, true, 1f).ShouldAttack);
            Assert.IsFalse(brain.Tick(0.5f, true, 1f).ShouldAttack);
            Assert.IsFalse(brain.Tick(0.49f, true, 1f).ShouldAttack);
            Assert.IsTrue(brain.Tick(0.02f, true, 1f).ShouldAttack);
        }

        [Test]
        public void RuntimeStateAndBrain_ResetCleanlyForPoolReuse()
        {
            var state = new EnemyRuntimeState();
            state.Reset(40f);
            state.ApplyDamage(17f);
            state.MarkPooled();
            state.Reset(40f);
            var brain = CreateBrain();
            brain.Tick(0f, true, 1f);
            brain.Reset();

            Assert.That(state.CurrentHitPoints, Is.EqualTo(40f));
            Assert.IsTrue(state.IsAlive);
            Assert.That(brain.State, Is.EqualTo(OrdinaryEnemyBrainState.Idle));
            Assert.That(brain.AttackCooldown, Is.Zero);
        }

        [Test]
        public void PopulationState_MaintainsDesiredPopulationAfterRecycle()
        {
            var population = new SpawnPopulationState(new SpawnPopulationPolicy(4));
            for (var i = 0; i < 4; i++)
            {
                population.RegisterSpawn();
            }

            population.RegisterRecycle();
            Assert.That(population.LiveCount, Is.EqualTo(3));
            Assert.That(population.PendingRespawns, Is.EqualTo(1));
            Assert.IsTrue(population.NeedsSpawn);

            population.RegisterSpawn();
            Assert.That(population.LiveCount, Is.EqualTo(4));
            Assert.That(population.PendingRespawns, Is.Zero);
        }

        [Test]
        public void PopulationPolicy_AcceptsThreeAndFiveButRejectsOutsideBounds()
        {
            Assert.That(new SpawnPopulationPolicy(3).DesiredPopulation, Is.EqualTo(3));
            Assert.That(new SpawnPopulationPolicy(5).DesiredPopulation, Is.EqualTo(5));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnPopulationPolicy(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnPopulationPolicy(6));
        }

        [Test]
        public void FivePopulationStates_CoexistWithIndependentCounts()
        {
            var total = 0;
            for (var spotIndex = 0; spotIndex < 5; spotIndex++)
            {
                var population = new SpawnPopulationState(new SpawnPopulationPolicy(3));
                for (var enemyIndex = 0; enemyIndex < 3; enemyIndex++)
                {
                    population.RegisterSpawn();
                }

                Assert.That(population.LiveCount, Is.EqualTo(3));
                total += population.LiveCount;
            }

            Assert.That(total, Is.EqualTo(15));
        }

        [Test]
        public void GlobalCap_RejectsReservationsAboveMaximumAndRecoversOnRelease()
        {
            var cap = new LiveEnemyCapCoordinator(2);

            Assert.IsTrue(cap.TryReserve());
            Assert.IsTrue(cap.TryReserve());
            Assert.IsFalse(cap.TryReserve());
            cap.Release();
            Assert.IsTrue(cap.TryReserve());
            Assert.That(cap.LiveCount, Is.EqualTo(2));
        }

        [Test]
        public void AnchorSelector_RejectsOccupiedAndNearPlayerAnchors()
        {
            var distances = new[] { 0.5f, 5f, 6f };
            var occupied = new[] { false, true, false };

            var found = SpawnAnchorSelector.TrySelect(distances, occupied, 3f, 0, out var selected);

            Assert.IsTrue(found);
            Assert.That(selected, Is.EqualTo(2));
        }

        [Test]
        public void RespawnJitter_StaysWithinConfiguredBounds()
        {
            var jitter = new RespawnDelayPolicy(8f, 14f);

            Assert.That(jitter.Sample(0f), Is.EqualTo(8f));
            Assert.That(jitter.Sample(0.5f), Is.EqualTo(11f));
            Assert.That(jitter.Sample(1f), Is.EqualTo(14f));
        }

        [Test]
        public void RespawnSchedule_LaterDeathWithShorterDelayBecomesReadyFirst()
        {
            var jitter = new RespawnDelayPolicy(8f, 14f);
            var schedule = new RespawnSchedule(2);
            schedule.Schedule(0f + jitter.Sample(1f));
            schedule.Schedule(1f + jitter.Sample(0f));

            Assert.That(schedule.EarliestReadyTime, Is.EqualTo(9f));
            Assert.IsTrue(schedule.HasReady(9f));
            Assert.That(schedule.Count, Is.EqualTo(2));
            Assert.That(schedule.ConsumeEarliest(), Is.EqualTo(9f));

            Assert.That(schedule.Count, Is.EqualTo(1));
            Assert.That(schedule.EarliestReadyTime, Is.EqualTo(14f));
            Assert.IsFalse(schedule.HasReady(13.99f));
            Assert.IsTrue(schedule.HasReady(14f));
        }

        [Test]
        public void SensingColliderContract_RequiresOneDedicatedColliderOnly()
        {
            Assert.DoesNotThrow(() => SensingColliderContract.ValidateCounts(1, 0));
            Assert.Throws<InvalidOperationException>(() => SensingColliderContract.ValidateCounts(0, 0));
            Assert.Throws<InvalidOperationException>(() => SensingColliderContract.ValidateCounts(2, 0));
            Assert.Throws<InvalidOperationException>(() => SensingColliderContract.ValidateCounts(1, 1));
        }

        private static OrdinaryEnemyStateMachine CreateBrain()
        {
            var brain = new OrdinaryEnemyStateMachine();
            brain.Configure(new EnemyBehaviorParameters(5f, 7f, 1.2f, 1f));
            return brain;
        }
    }
}
