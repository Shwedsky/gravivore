using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Combat;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyPopulationController : MonoBehaviour
    {
        private SpawnSpotRuntime[] _spots;
        private LiveEnemyCapCoordinator _globalCapacity;
        private bool _isInitialized;

        public int SpotCount => _spots != null ? _spots.Length : 0;

        public int LiveEnemyCount => _globalCapacity != null ? _globalCapacity.LiveCount : 0;

        public int GlobalLiveEnemyCap => _globalCapacity != null ? _globalCapacity.MaximumLiveCount : 0;

        public event Action<EnemyDeathEvent> EnemyDied;

        public void Initialize(
            IReadOnlyList<SpawnSpotRuntimeConfiguration> spotConfigurations,
            Transform player,
            IDamageable playerDamageable,
            int globalLiveEnemyCap,
            int targetLayer)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Enemy population controller is already initialized.");
            }

            if (spotConfigurations == null || spotConfigurations.Count == 0)
            {
                throw new ArgumentException("At least one spawn spot is required.", nameof(spotConfigurations));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (playerDamageable == null)
            {
                throw new ArgumentNullException(nameof(playerDamageable));
            }

            _globalCapacity = new LiveEnemyCapCoordinator(globalLiveEnemyCap);
            var poolObject = new GameObject("Ordinary Enemy Pool");
            poolObject.transform.SetParent(transform, false);
            var pool = new OrdinaryEnemyPool(poolObject.transform, globalLiveEnemyCap, targetLayer);
            _spots = new SpawnSpotRuntime[spotConfigurations.Count];
            for (var i = 0; i < _spots.Length; i++)
            {
                _spots[i] = new SpawnSpotRuntime(
                    spotConfigurations[i],
                    pool,
                    _globalCapacity,
                    player,
                    playerDamageable,
                    new SystemRandomSource(1709 + (i * 7919)));
                _spots[i].EnemyDied += HandleEnemyDied;
            }

            _isInitialized = true;
        }

        public SpawnSpotRuntime GetSpot(int index)
        {
            if (_spots == null || index < 0 || index >= _spots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _spots[index];
        }

        public void Tick(float deltaTime)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Enemy population controller must be initialized before ticking.");
            }

            for (var i = 0; i < _spots.Length; i++)
            {
                _spots[i].Tick(deltaTime);
            }
        }

        private void Update()
        {
            if (_isInitialized)
            {
                Tick(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_spots == null)
            {
                return;
            }

            for (var i = 0; i < _spots.Length; i++)
            {
                _spots[i].EnemyDied -= HandleEnemyDied;
                _spots[i].Dispose();
            }
        }

        private void HandleEnemyDied(EnemyDeathEvent death)
        {
            EnemyDied?.Invoke(death);
        }
    }
}
