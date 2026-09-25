using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class EnemyPopulationSmokeTests
    {
        [UnityTest]
        public IEnumerator OrdinaryEnemyAttack_AppliesDamageToPlayerHealth()
        {
            var root = new GameObject("Enemy Attack Smoke Root");
            var player = new GameObject("Enemy Attack Smoke Player", typeof(CharacterController), typeof(PlayerHealthController));
            var stats = CreatePlayerStats();
            var playerHealth = player.GetComponent<PlayerHealthController>();
            playerHealth.Initialize(player.GetComponent<CharacterController>(), stats, Vector3.zero, 1f);
            var pool = new OrdinaryEnemyPool(root.transform, 1, 9);
            var enemy = pool.Acquire(
                CreateEnemyConfiguration(),
                player.transform,
                playerHealth,
                new Vector3(0f, 0f, 1f),
                pool.Return);
            var initialHitPoints = playerHealth.CurrentHitPoints;

            yield return null;

            Assert.That(playerHealth.CurrentHitPoints, Is.LessThan(initialHitPoints));
            pool.Return(enemy);
            Object.Destroy(root);
            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerDeath_RespawnsOnceAtConfiguredPointWithStatsAndFullHealth()
        {
            var respawnPosition = new Vector3(4f, 0f, -3f);
            var player = new GameObject("Player Respawn Smoke", typeof(CharacterController), typeof(PlayerHealthController));
            player.transform.position = new Vector3(2f, 0f, 2f);
            var stats = CreatePlayerStats();
            var originalLevels = stats.BaseLevels;
            var health = player.GetComponent<PlayerHealthController>();
            health.Initialize(player.GetComponent<CharacterController>(), stats, respawnPosition, 1f);
            var deathCount = 0;
            var respawnCount = 0;
            health.Died += _ => deathCount++;
            health.Respawned += _ => respawnCount++;

            var lethal = health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));
            var blockedRepeat = health.ApplyDamage(new DamageRequest(10000f, DamageType.Physical));

            Assert.IsTrue(lethal.WasLethal);
            Assert.That(blockedRepeat.AppliedDamage, Is.Zero);
            Assert.That(deathCount, Is.EqualTo(1));
            Assert.That(respawnCount, Is.EqualTo(1));
            Assert.That(player.transform.position, Is.EqualTo(respawnPosition));
            Assert.That(health.CurrentHitPoints, Is.EqualTo(health.MaximumHitPoints));
            Assert.That(stats.BaseLevels.Power, Is.EqualTo(originalLevels.Power));
            Assert.That(stats.BaseLevels.Hull, Is.EqualTo(originalLevels.Hull));
            Assert.That(stats.BaseLevels.Armor, Is.EqualTo(originalLevels.Armor));
            Assert.That(stats.BaseLevels.Flux, Is.EqualTo(originalLevels.Flux));
            Assert.That(stats.BaseLevels.Mobility, Is.EqualTo(originalLevels.Mobility));

            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyDeath_IsUniquePerLifeAndPooledReuseStartsWithFullHealth()
        {
            var root = new GameObject("Enemy Death Smoke Root");
            var player = new GameObject("Enemy Death Smoke Player");
            var pool = new OrdinaryEnemyPool(root.transform, 1, 9);
            var target = new RecordingDamageable();
            var configuration = CreateEnemyConfiguration();
            var deathCount = 0;
            var first = pool.Acquire(configuration, player.transform, target, Vector3.one, pool.Return);
            first.Died += _ => deathCount++;

            var firstLethal = first.ApplyDamage(new DamageRequest(1000f, DamageType.Gravity));
            var repeated = first.ApplyDamage(new DamageRequest(1000f, DamageType.Gravity));
            Assert.IsTrue(firstLethal.WasLethal);
            Assert.IsFalse(repeated.WasLethal);
            Assert.That(deathCount, Is.EqualTo(1));
            Assert.That(pool.AvailableCount, Is.EqualTo(1));

            var second = pool.Acquire(configuration, player.transform, target, Vector3.forward, pool.Return);
            Assert.AreSame(first, second);
            Assert.That(second.CurrentHitPoints, Is.EqualTo(second.MaximumHitPoints));
            second.Died += _ => deathCount++;
            second.ApplyDamage(new DamageRequest(1000f, DamageType.Gravity));
            Assert.That(deathCount, Is.EqualTo(2));
            Assert.That(pool.AvailableCount, Is.EqualTo(1));

            Object.Destroy(root);
            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OrdinaryEnemyPool_ReusesInstanceWithCleanRuntimeState()
        {
            var root = new GameObject("Pool Smoke Root");
            var player = new GameObject("Pool Smoke Player");
            player.transform.position = Vector3.zero;
            var pool = new OrdinaryEnemyPool(root.transform, 1, 9);
            var configuration = CreateEnemyConfiguration();
            var damageTarget = new RecordingDamageable();
            var first = pool.Acquire(
                configuration,
                player.transform,
                damageTarget,
                new Vector3(0f, 0f, 1f),
                _ => { });
            var attackCount = 0;
            first.AttackRequested += _ => attackCount++;
            yield return null;

            Assert.That(first.BrainState, Is.EqualTo(OrdinaryEnemyBrainState.Attack));
            Assert.That(attackCount, Is.EqualTo(1));
            Assert.That(first.AttackCooldown, Is.GreaterThan(0f));

            first.ApplyDamage(new DamageRequest(10f, DamageType.Gravity));
            Assert.That(first.CurrentHitPoints, Is.EqualTo(30f));
            pool.Return(first);
            Assert.IsNull(first.AggroTarget);
            Assert.That(first.BrainState, Is.EqualTo(OrdinaryEnemyBrainState.Idle));
            Assert.That(first.AttackCooldown, Is.Zero);
            Assert.That(first.CurrentHitPoints, Is.Zero);

            var second = pool.Acquire(
                configuration,
                player.transform,
                damageTarget,
                new Vector3(7f, 0f, 8f),
                _ => { });
            Assert.AreSame(first, second);
            Assert.That(second.CurrentHitPoints, Is.EqualTo(40f));
            Assert.That(second.BrainState, Is.EqualTo(OrdinaryEnemyBrainState.Idle));
            Assert.That(second.AttackCooldown, Is.Zero);
            Assert.AreSame(player.transform, second.AggroTarget);
            Assert.That(second.transform.position, Is.EqualTo(new Vector3(7f, 0f, 8f)));
            Assert.AreNotSame(second.TargetPoint, second.DisplacementRoot);
            var targetLayerColliderCount = 0;
            var colliders = second.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject.layer == 9)
                {
                    targetLayerColliderCount++;
                }
            }

            Assert.That(targetLayerColliderCount, Is.EqualTo(1));
            Assert.AreSame(second.TargetPoint.GetComponent<Collider>(), second.GetComponentInChildren<SphereCollider>());

            pool.Return(second);
            Object.Destroy(root);
            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpawnSpot_RecoversPopulationAfterDeathLikeRecycle()
        {
            var root = new GameObject("Population Recovery Root");
            var player = new GameObject("Population Recovery Player");
            player.transform.position = new Vector3(30f, 0f, 30f);
            var cap = new LiveEnemyCapCoordinator(3);
            var pool = new OrdinaryEnemyPool(root.transform, 3, 9);
            var spot = new SpawnSpotRuntime(
                CreateSpotConfiguration("recovery", Vector3.zero, 3, 0f, 0f),
                pool,
                cap,
                player.transform,
                new RecordingDamageable(),
                new FixedRandomSource(0.5f));
            spot.Tick(0f);
            Assert.That(spot.LiveCount, Is.EqualTo(3));

            var recycled = spot.GetLiveEnemy(0);
            recycled.ApplyDamage(new DamageRequest(1000f, DamageType.Gravity));
            Assert.That(spot.LiveCount, Is.EqualTo(2));
            spot.Tick(0f);

            Assert.That(spot.LiveCount, Is.EqualTo(3));
            Assert.That(cap.LiveCount, Is.EqualTo(3));
            var reused = false;
            for (var i = 0; i < spot.LiveCount; i++)
            {
                reused |= spot.GetLiveEnemy(i) == recycled;
            }

            Assert.IsTrue(reused);
            Assert.That(recycled.CurrentHitPoints, Is.EqualTo(40f));

            spot.Dispose();
            Object.Destroy(root);
            Object.Destroy(player);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FiveSpawnSpots_ExceedDemandCapAndReuseReleasedSlot()
        {
            var root = new GameObject("Five Spots Smoke Root", typeof(EnemyPopulationController));
            var player = new GameObject("Five Spots Smoke Player");
            player.transform.position = new Vector3(50f, 0f, 50f);
            var configurations = new SpawnSpotRuntimeConfiguration[5];
            for (var i = 0; i < configurations.Length; i++)
            {
                configurations[i] = CreateSpotConfiguration(
                    $"spot-{i}",
                    new Vector3(i * 8f, 0f, 0f),
                    4,
                    8f,
                    14f);
            }

            var population = root.GetComponent<EnemyPopulationController>();
            population.Initialize(configurations, player.transform, new RecordingDamageable(), 17, 9);
            population.Tick(0f);

            Assert.That(population.SpotCount, Is.EqualTo(5));
            Assert.That(population.LiveEnemyCount, Is.EqualTo(17));
            Assert.That(population.LiveEnemyCount, Is.LessThanOrEqualTo(population.GlobalLiveEnemyCap));
            for (var i = 0; i < population.SpotCount; i++)
            {
                Assert.That(population.GetSpot(i).LiveCount, Is.GreaterThan(0));
            }

            var finalSpotBeforeRecycle = population.GetSpot(4).LiveCount;
            population.GetSpot(0).GetLiveEnemy(0).ApplyDamage(
                new DamageRequest(1000f, DamageType.Gravity));
            Assert.That(population.LiveEnemyCount, Is.EqualTo(16));

            population.Tick(0f);
            Assert.That(population.LiveEnemyCount, Is.EqualTo(17));
            Assert.That(population.GetSpot(4).LiveCount, Is.EqualTo(finalSpotBeforeRecycle + 1));
            population.Tick(100f);
            Assert.That(population.LiveEnemyCount, Is.EqualTo(17));
            Assert.That(population.LiveEnemyCount, Is.LessThanOrEqualTo(population.GlobalLiveEnemyCap));

            Object.Destroy(root);
            Object.Destroy(player);
            yield return null;
        }

        private static EnemyRuntimeConfiguration CreateEnemyConfiguration()
        {
            return new EnemyRuntimeConfiguration(
                "test-enemy",
                40f,
                2f,
                4f,
                0.4f,
                0.9f,
                new EnemyBehaviorParameters(5f, 7f, 1.2f, 1f));
        }

        private static PlayerStatsState CreatePlayerStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 2f, 0.5f, 0f, 1000f),
                new StatCurve(10, 100f, 10f, 1f, 1f, 10000f),
                new StatCurve(10, 5f, 3f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, -0.1f, 0f, 0.05f, 10f),
                new StatCurve(10, 4f, 0.5f, 0f, 0f, 20f),
                0.4f,
                6f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private static SpawnSpotRuntimeConfiguration CreateSpotConfiguration(
            string id,
            Vector3 origin,
            int desiredPopulation,
            float minimumRespawnDelay,
            float maximumRespawnDelay)
        {
            return new SpawnSpotRuntimeConfiguration(
                id,
                CreateEnemyConfiguration(),
                origin,
                new[]
                {
                    new Vector3(-1f, 0f, -1f),
                    new Vector3(1f, 0f, -1f),
                    new Vector3(-1f, 0f, 1f),
                    new Vector3(1f, 0f, 1f),
                    new Vector3(0f, 0f, 2f)
                },
                desiredPopulation,
                2f,
                minimumRespawnDelay,
                maximumRespawnDelay);
        }

        private sealed class FixedRandomSource : IRandomSource
        {
            private readonly float _value;

            public FixedRandomSource(float value)
            {
                _value = value;
            }

            public float NextUnit()
            {
                return _value;
            }
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
