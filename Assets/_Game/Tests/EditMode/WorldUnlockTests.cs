using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.World;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class WorldUnlockTests
    {
        private static readonly string[] EnemyIds =
        {
            "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier"
        };

        [Test]
        public void FreshState_StartsWithBothGatesLocked()
        {
            var state = CreateWorldState();
            Assert.IsFalse(state.EliteGateUnlocked);
            Assert.IsFalse(state.BossGateUnlocked);
        }

        [Test]
        public void EliteRequirement_RequiresScoreAndEveryFirstKill()
        {
            using var progression = CreateProgression(1);
            var requirement = new EliteGateRequirement(EnemyIds, 5);
            var state = CreateWorldState();
            using var world = new WorldUnlockService(progression, requirement, state);

            for (var i = 0; i < 5; i++) progression.TryGrant(Death(i + 1, "scout-drone"));
            Assert.IsFalse(state.EliteGateUnlocked, "Assimilation score alone must not unlock the gate.");

            for (var i = 1; i < EnemyIds.Length; i++) progression.TryGrant(Death(i + 20, EnemyIds[i]));
            Assert.IsTrue(state.EliteGateUnlocked);
        }

        [Test]
        public void EliteRequirement_AllFirstKillsBelowScoreRemainLocked()
        {
            using var progression = CreateProgression(1);
            var state = CreateWorldState();
            using var world = new WorldUnlockService(
                progression,
                new EliteGateRequirement(EnemyIds, 6),
                state);

            GrantFirstKills(progression);
            Assert.IsFalse(state.EliteGateUnlocked);
        }

        [Test]
        public void EliteRequirement_UnlocksAtExactScoreBoundary()
        {
            using var progression = CreateProgression(1);
            var state = CreateWorldState();
            var eventCount = 0;
            state.GateUnlocked += _ => eventCount++;
            using var world = new WorldUnlockService(
                progression,
                new EliteGateRequirement(EnemyIds, 5),
                state);

            GrantFirstKills(progression);

            Assert.That(progression.State.TotalAssimilationScore, Is.EqualTo(5));
            Assert.IsTrue(state.EliteGateUnlocked);
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void GateEvents_AreIdempotentAndMonotonic()
        {
            var state = CreateWorldState();
            var unlockCount = 0;
            var eliteCount = 0;
            state.GateUnlocked += _ => unlockCount++;
            state.EliteWasDefeated += _ => eliteCount++;

            Assert.IsTrue(state.TryUnlockEliteGate());
            Assert.IsFalse(state.TryUnlockEliteGate());
            Assert.IsTrue(state.RecordEliteDefeated("magnetar-guard"));
            Assert.IsFalse(state.RecordEliteDefeated("magnetar-guard"));
            Assert.That(unlockCount, Is.EqualTo(2));
            Assert.That(eliteCount, Is.EqualTo(1));
            Assert.IsTrue(state.EliteGateUnlocked);
            Assert.IsTrue(state.BossGateUnlocked);
        }

        [Test]
        public void EliteDefeat_RequiresUnlockedGateAndAuthoritativeId()
        {
            var state = CreateWorldState();
            Assert.IsFalse(state.RecordEliteDefeated("magnetar-guard"));
            state.TryUnlockEliteGate();
            Assert.IsFalse(state.RecordEliteDefeated("other-elite"));
            Assert.IsFalse(state.BossGateUnlocked);
        }

        [Test]
        public void Snapshot_RoundTripsWithoutReplayingMilestoneEvents()
        {
            var source = CreateWorldState();
            source.TryUnlockEliteGate();
            source.RecordEliteDefeated("magnetar-guard");
            var snapshot = source.ExportSnapshot();

            var restored = WorldUnlockState.Restore("elite-gate", "boss-gate", "magnetar-guard", snapshot);
            var eventCount = 0;
            restored.GateUnlocked += _ => eventCount++;

            Assert.IsTrue(restored.EliteGateUnlocked);
            Assert.IsTrue(restored.EliteDefeated);
            Assert.IsTrue(restored.BossGateUnlocked);
            Assert.That(eventCount, Is.Zero);
            Assert.IsFalse(restored.TryUnlockEliteGate());
            Assert.IsFalse(restored.RecordEliteDefeated("magnetar-guard"));
        }

        [Test]
        public void UnlockEvaluation_CompletesEvenWhenLaterPresentationObserverThrows()
        {
            using var progression = CreateProgression(1);
            var state = CreateWorldState();
            using var world = new WorldUnlockService(
                progression,
                new EliteGateRequirement(EnemyIds, 5),
                state);
            progression.Dirty += _ => throw new InvalidOperationException("presentation failure");

            for (var i = 0; i < EnemyIds.Length; i++)
            {
                Assert.Throws<AggregateException>(() => progression.TryGrant(Death(i + 1, EnemyIds[i])));
            }

            Assert.IsTrue(state.EliteGateUnlocked);
        }

        [Test]
        public void InvalidRequirementAndSnapshot_AreRejected()
        {
            Assert.Throws<ArgumentException>(() => new EliteGateRequirement(new[] { "a", "a" }, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EliteGateRequirement(new[] { "a" }, 0));
            Assert.Throws<ArgumentException>(() => new WorldUnlockSnapshot(false, true, true));
            Assert.Throws<ArgumentException>(() => new WorldUnlockSnapshot(true, false, true));
        }

        private static void GrantFirstKills(AssimilationProgressionService progression)
        {
            for (var i = 0; i < EnemyIds.Length; i++) progression.TryGrant(Death(i + 1, EnemyIds[i]));
        }

        private static WorldUnlockState CreateWorldState()
        {
            return new WorldUnlockState("elite-gate", "boss-gate", "magnetar-guard");
        }

        private static AssimilationProgressionService CreateProgression(long scorePerKill)
        {
            var curve = new StatCurve(10, 1f, 1f, 0f, 0f, 1000f);
            var statsConfiguration = new PlayerStatsConfiguration(
                curve,
                new StatCurve(10, 100f, 1f, 0f, 1f, 1000f),
                curve,
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                curve,
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            var stats = new PlayerStatsState(statsConfiguration, statsConfiguration.StartingLevels);
            var rewards = new[]
            {
                new CoreReward(EnemyIds[0], PlayerStatType.Mobility, 1f, scorePerKill),
                new CoreReward(EnemyIds[1], PlayerStatType.Power, 1f, scorePerKill),
                new CoreReward(EnemyIds[2], PlayerStatType.Armor, 1f, scorePerKill),
                new CoreReward(EnemyIds[3], PlayerStatType.Flux, 1f, scorePerKill),
                new CoreReward(EnemyIds[4], PlayerStatType.Hull, 1f, scorePerKill)
            };
            return new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(new ProgressionThresholdCurve(100f, 0f, 0f, 1000f), rewards));
        }

        private static EnemyDeathEvent Death(int seed, string enemyId)
        {
            return new EnemyDeathEvent(new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])), enemyId, Vector3.zero);
        }
    }
}
