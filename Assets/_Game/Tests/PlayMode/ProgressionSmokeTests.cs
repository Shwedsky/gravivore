using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ProgressionSmokeTests
    {
        [UnityTest]
        public IEnumerator PopulationDeath_GrantsOnceAndPooledReuseGetsNewLifeReward()
        {
            var root = new GameObject("Progression Smoke Root", typeof(EnemyPopulationController));
            var player = new GameObject("Progression Smoke Player");
            player.transform.position = new Vector3(20f, 0f, 20f);
            var population = root.GetComponent<EnemyPopulationController>();
            population.Initialize(
                new[] { CreateSpotConfiguration() },
                player.transform,
                new RecordingDamageable(),
                1,
                9);
            population.Tick(0f);

            var stats = CreateStats();
            var progression = new AssimilationProgressionService(
                stats,
                new ProgressionState(),
                new ProgressionConfiguration(
                    new ProgressionThresholdCurve(1f, 0f, 0f, 100f),
                    new[] { new CoreReward("test-enemy", PlayerStatType.Power, 1f, 2) }),
                population);
            var firstKillCount = 0;
            var rewardCount = 0;
            var firstLife = default(EnemyLifeId);
            var secondLife = default(EnemyLifeId);
            progression.FirstKill += _ => firstKillCount++;
            progression.RewardGranted += reward =>
            {
                rewardCount++;
                if (rewardCount == 1)
                {
                    firstLife = reward.LifeId;
                }
                else
                {
                    secondLife = reward.LifeId;
                }
            };

            var firstInstance = population.GetSpot(0).GetLiveEnemy(0);
            firstInstance.ApplyDamage(new DamageRequest(1000f, DamageType.Gravity));
            Assert.That(progression.State.TotalAssimilationScore, Is.EqualTo(2));
            Assert.That(rewardCount, Is.EqualTo(1));

            population.Tick(0f);
            var reusedInstance = population.GetSpot(0).GetLiveEnemy(0);
            Assert.AreSame(firstInstance, reusedInstance);
            reusedInstance.ApplyDamage(new DamageRequest(1000f, DamageType.Gravity));

            Assert.That(progression.State.TotalAssimilationScore, Is.EqualTo(4));
            Assert.That(rewardCount, Is.EqualTo(2));
            Assert.That(firstKillCount, Is.EqualTo(1));
            Assert.That(firstLife, Is.Not.EqualTo(secondLife));
            Assert.That(stats.BaseLevels.Power, Is.EqualTo(3));

            progression.Dispose();
            Object.Destroy(root);
            Object.Destroy(player);
            yield return null;
        }

        private static SpawnSpotRuntimeConfiguration CreateSpotConfiguration()
        {
            return new SpawnSpotRuntimeConfiguration(
                "progression-spot",
                new EnemyRuntimeConfiguration(
                    "test-enemy",
                    10f,
                    2f,
                    1f,
                    0.4f,
                    0.9f,
                    new EnemyBehaviorParameters(5f, 7f, 1f, 1f)),
                Vector3.zero,
                new[] { Vector3.zero },
                1,
                2f,
                0f,
                0f);
        }

        private static PlayerStatsState CreateStats()
        {
            var curve = new StatCurve(10, 1f, 1f, 0f, 0.1f, 100f);
            var configuration = new PlayerStatsConfiguration(
                curve,
                curve,
                curve,
                curve,
                curve,
                0.1f,
                100f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private sealed class RecordingDamageable : IDamageable
        {
            public bool IsAlive => true;

            public DamageResult ApplyDamage(in DamageRequest request)
            {
                return new DamageResult(request.RawDamage, false);
            }
        }
    }
}
