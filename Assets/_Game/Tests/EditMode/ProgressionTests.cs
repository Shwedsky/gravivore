using System;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ProgressionTests
    {
        [TestCase("scout-drone", PlayerStatType.Mobility)]
        [TestCase("cutter-unit", PlayerStatType.Power)]
        [TestCase("warden", PlayerStatType.Armor)]
        [TestCase("arc-drone", PlayerStatType.Flux)]
        [TestCase("carrier", PlayerStatType.Hull)]
        public void RewardRouting_AdvancesConfiguredStatOnly(string enemyId, PlayerStatType expectedStat)
        {
            var stats = CreateStats();
            var before = stats.BaseLevels;
            var service = CreateService(stats, CreateFiveRewards(3f), 3f);

            Assert.IsTrue(service.TryGrant(Death(1, enemyId)));

            Assert.That(stats.BaseLevels.GetLevel(expectedStat), Is.EqualTo(2));
            foreach (PlayerStatType stat in Enum.GetValues(typeof(PlayerStatType)))
            {
                if (stat != expectedStat)
                {
                    Assert.That(stats.BaseLevels.GetLevel(stat), Is.EqualTo(before.GetLevel(stat)));
                }
            }
        }

        [Test]
        public void ThresholdCurve_IsDataDrivenAndGradual()
        {
            var curve = new ProgressionThresholdCurve(3f, 2f, 0.5f, 100f);

            Assert.That(curve.Evaluate(1), Is.EqualTo(3f));
            Assert.That(curve.Evaluate(2), Is.EqualTo(5.5f));
            Assert.That(curve.Evaluate(3), Is.EqualTo(9f));
            Assert.Throws<ArgumentOutOfRangeException>(() => curve.Evaluate(0));
        }

        [Test]
        public void Reward_CarriesRemainderAndSupportsMultipleLevelUps()
        {
            var stats = CreateStats();
            var service = CreateService(
                stats,
                new[] { new CoreReward("enemy", PlayerStatType.Power, 10f, 2) },
                3f,
                1f);
            CoreRewardGrantedEvent granted = default;
            service.RewardGranted += value => granted = value;

            service.TryGrant(Death(1, "enemy"));

            Assert.That(stats.BaseLevels.Power, Is.EqualTo(3));
            Assert.That(service.State.GetStatExperience(PlayerStatType.Power), Is.EqualTo(3f));
            Assert.That(granted.LevelsGained, Is.EqualTo(2));
            Assert.That(granted.RemainingExperience, Is.EqualTo(3f));
        }

        [Test]
        public void DuplicateLife_IsRejectedWithoutEventsOrMutation()
        {
            var stats = CreateStats();
            var service = CreateService(
                stats,
                new[] { new CoreReward("enemy", PlayerStatType.Power, 2f, 4) },
                3f);
            var rewardEvents = 0;
            var dirtyEvents = 0;
            service.RewardGranted += _ => rewardEvents++;
            service.Dirty += _ => dirtyEvents++;
            var death = Death(1, "enemy");

            Assert.IsTrue(service.TryGrant(death));
            Assert.IsFalse(service.TryGrant(death));

            Assert.That(service.State.TotalAssimilationScore, Is.EqualTo(4));
            Assert.That(service.State.GetStatExperience(PlayerStatType.Power), Is.EqualTo(2f));
            Assert.That(service.State.ProcessedLifeCount, Is.EqualTo(1));
            Assert.That(rewardEvents, Is.EqualTo(1));
            Assert.That(dirtyEvents, Is.EqualTo(1));
        }

        [Test]
        public void DistinctLives_GrantTwiceButFirstKillFiresOnce()
        {
            var service = CreateService(
                CreateStats(),
                new[] { new CoreReward("enemy", PlayerStatType.Hull, 1f, 3) },
                10f);
            var firstKillEvents = 0;
            service.FirstKill += _ => firstKillEvents++;

            Assert.IsTrue(service.TryGrant(Death(1, "enemy")));
            Assert.IsTrue(service.TryGrant(Death(2, "enemy")));

            Assert.That(firstKillEvents, Is.EqualTo(1));
            Assert.IsTrue(service.State.HasFirstKill("enemy"));
            Assert.That(service.State.TotalAssimilationScore, Is.EqualTo(6));
        }

        [Test]
        public void UnknownEnemy_DoesNotConsumeLifeOrDirtyState()
        {
            var service = CreateService(
                CreateStats(),
                new[] { new CoreReward("known", PlayerStatType.Power, 1f, 1) },
                3f);
            var dirtyEvents = 0;
            service.Dirty += _ => dirtyEvents++;

            Assert.IsFalse(service.TryGrant(Death(1, "unknown")));

            Assert.That(service.State.ProcessedLifeCount, Is.Zero);
            Assert.That(service.State.TotalAssimilationScore, Is.Zero);
            Assert.That(dirtyEvents, Is.Zero);
        }

        [Test]
        public void CappedStat_DiscardsXpWithoutLoopingAndStillAddsScore()
        {
            var stats = CreateStats(3);
            stats.SetLevel(PlayerStatType.Power, 3);
            var service = CreateService(
                stats,
                new[] { new CoreReward("enemy", PlayerStatType.Power, float.MaxValue, 5) },
                1f);

            Assert.IsTrue(service.TryGrant(Death(1, "enemy")));

            Assert.That(stats.BaseLevels.Power, Is.EqualTo(3));
            Assert.That(service.State.GetStatExperience(PlayerStatType.Power), Is.Zero);
            Assert.That(service.State.TotalAssimilationScore, Is.EqualTo(5));
        }

        [Test]
        public void TypedEvents_ExposeCommittedStateAndDirtyBoundary()
        {
            var service = CreateService(
                CreateStats(),
                new[] { new CoreReward("enemy", PlayerStatType.Armor, 3f, 7) },
                3f);
            var rewardEvents = 0;
            var xpEvents = 0;
            var dirtyEvents = 0;
            service.RewardGranted += reward =>
            {
                rewardEvents++;
                Assert.That(service.State.TotalAssimilationScore, Is.EqualTo(7));
                Assert.That(reward.CurrentLevel, Is.EqualTo(2));
            };
            service.StatExperienceChanged += change =>
            {
                xpEvents++;
                Assert.That(change.Stat, Is.EqualTo(PlayerStatType.Armor));
                Assert.That(change.CurrentLevel, Is.EqualTo(2));
            };
            service.Dirty += dirty =>
            {
                dirtyEvents++;
                Assert.That(dirty.TotalAssimilationScore, Is.EqualTo(7));
            };

            service.TryGrant(Death(1, "enemy"));

            Assert.That(rewardEvents, Is.EqualTo(1));
            Assert.That(xpEvents, Is.EqualTo(1));
            Assert.That(dirtyEvents, Is.EqualTo(1));
        }

        [Test]
        public void ObserverFailure_DoesNotLoseProgressOrDirtyNotification()
        {
            var service = CreateService(
                CreateStats(),
                new[] { new CoreReward("enemy", PlayerStatType.Power, 3f, 2) },
                3f);
            var dirtyEvents = 0;
            service.RewardGranted += _ => throw new InvalidOperationException("presentation failed");
            service.Dirty += _ => dirtyEvents++;

            Assert.Throws<AggregateException>(() => service.TryGrant(Death(1, "enemy")));

            Assert.That(service.State.TotalAssimilationScore, Is.EqualTo(2));
            Assert.That(service.State.ProcessedLifeCount, Is.EqualTo(1));
            Assert.That(dirtyEvents, Is.EqualTo(1));
        }

        [Test]
        public void StatObserverFailure_DoesNotSkipRewardOrDirtyNotification()
        {
            var stats = CreateStats();
            var service = CreateService(
                stats,
                new[] { new CoreReward("enemy", PlayerStatType.Power, 3f, 2) },
                3f);
            var rewardEvents = 0;
            var dirtyEvents = 0;
            stats.StatChanged += _ => throw new InvalidOperationException("stat observer failed");
            service.RewardGranted += _ => rewardEvents++;
            service.Dirty += _ => dirtyEvents++;

            Assert.Throws<AggregateException>(() => service.TryGrant(Death(1, "enemy")));

            Assert.That(stats.BaseLevels.Power, Is.EqualTo(2));
            Assert.That(service.State.TotalAssimilationScore, Is.EqualTo(2));
            Assert.That(rewardEvents, Is.EqualTo(1));
            Assert.That(dirtyEvents, Is.EqualTo(1));
        }

        [Test]
        public void ProgressionUpdatesDerivedStatsAndPlayerHealthWithoutChangingOtherLevels()
        {
            var stats = CreateStats();
            var health = new PlayerHealthRuntime(stats, 0f);
            stats.DerivedStatsChanged += _ => health.SynchronizeDerivedStats();
            var initialDamage = stats.DerivedStats.BaseDamage;
            var initialHitPoints = health.MaximumHitPoints;
            var initialArmor = health.Armor;
            var initialAttackInterval = stats.DerivedStats.AttackInterval;
            var initialMoveSpeed = stats.DerivedStats.MoveSpeed;
            var service = CreateService(stats, CreateFiveRewards(3f), 3f);

            service.TryGrant(Death(1, "cutter-unit"));
            service.TryGrant(Death(2, "carrier"));
            service.TryGrant(Death(3, "warden"));
            service.TryGrant(Death(4, "arc-drone"));
            service.TryGrant(Death(5, "scout-drone"));

            Assert.That(stats.DerivedStats.BaseDamage, Is.GreaterThan(initialDamage));
            Assert.That(health.MaximumHitPoints, Is.GreaterThan(initialHitPoints));
            Assert.That(health.Armor, Is.GreaterThan(initialArmor));
            Assert.That(stats.DerivedStats.AttackInterval, Is.LessThan(initialAttackInterval));
            Assert.That(stats.DerivedStats.MoveSpeed, Is.GreaterThan(initialMoveSpeed));
            health.Respawn();
            Assert.That(health.CurrentHitPoints, Is.EqualTo(health.MaximumHitPoints));
            Assert.That(stats.BaseLevels.Power, Is.EqualTo(2));
            Assert.That(stats.BaseLevels.Hull, Is.EqualTo(2));
            Assert.That(stats.BaseLevels.Armor, Is.EqualTo(2));
            Assert.That(stats.BaseLevels.Flux, Is.EqualTo(2));
            Assert.That(stats.BaseLevels.Mobility, Is.EqualTo(2));
        }

        [Test]
        public void InvalidRewardAndConfiguration_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CoreReward("enemy", PlayerStatType.Power, 0f, 1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new CoreReward("enemy", PlayerStatType.Power, 1f, 0));
            Assert.Throws<ArgumentException>(
                () => new ProgressionConfiguration(
                    new ProgressionThresholdCurve(3f, 1f, 0f, 100f),
                    new[]
                    {
                        new CoreReward("enemy", PlayerStatType.Power, 1f, 1),
                        new CoreReward("enemy", PlayerStatType.Hull, 1f, 1)
                    }));
            Assert.Throws<ArgumentException>(() => new EnemyLifeId(Guid.Empty));
        }

        private static AssimilationProgressionService CreateService(
            PlayerStatsState stats,
            CoreReward[] rewards,
            float levelOneCost,
            float linearGrowth = 0f)
        {
            return new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(
                    new ProgressionThresholdCurve(levelOneCost, linearGrowth, 0f, 1000000f),
                    rewards));
        }

        private static CoreReward[] CreateFiveRewards(float experience)
        {
            return new[]
            {
                new CoreReward("scout-drone", PlayerStatType.Mobility, experience, 1),
                new CoreReward("cutter-unit", PlayerStatType.Power, experience, 1),
                new CoreReward("warden", PlayerStatType.Armor, experience, 1),
                new CoreReward("arc-drone", PlayerStatType.Flux, experience, 1),
                new CoreReward("carrier", PlayerStatType.Hull, experience, 1)
            };
        }

        private static EnemyDeathEvent Death(int seed, string enemyId)
        {
            return new EnemyDeathEvent(
                new EnemyLifeId(new Guid(seed, 0, 0, new byte[8])),
                enemyId,
                new Vector3(seed, 0f, 0f));
        }

        private static PlayerStatsState CreateStats(int maximumLevel = 10)
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(maximumLevel, 10f, 2f, 0f, 0f, 1000f),
                new StatCurve(maximumLevel, 100f, 10f, 0f, 1f, 10000f),
                new StatCurve(maximumLevel, 5f, 2f, 0f, 0f, 1000f),
                new StatCurve(maximumLevel, 1f, -0.1f, 0f, 0.2f, 10f),
                new StatCurve(maximumLevel, 4f, 0.5f, 0f, 0f, 20f),
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }
    }
}
