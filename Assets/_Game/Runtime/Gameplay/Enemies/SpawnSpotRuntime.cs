using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public sealed class SpawnSpotRuntime : IDisposable
    {
        private readonly struct LiveEntry
        {
            public LiveEntry(OrdinaryEnemyController enemy, int anchorIndex)
            {
                Enemy = enemy;
                AnchorIndex = anchorIndex;
            }

            public OrdinaryEnemyController Enemy { get; }

            public int AnchorIndex { get; }
        }

        private readonly SpawnSpotRuntimeConfiguration _configuration;
        private readonly OrdinaryEnemyPool _pool;
        private readonly ILiveEnemyCapacity _globalCapacity;
        private readonly Transform _player;
        private readonly IRandomSource _random;
        private readonly SpawnPopulationState _population;
        private readonly RespawnSchedule _respawnSchedule;
        private readonly List<LiveEntry> _liveEnemies;
        private readonly bool[] _occupiedAnchors;
        private readonly float[] _anchorDistances;
        private float _elapsedTime;
        private int _nextAnchorIndex;
        private bool _isDisposed;

        public SpawnSpotRuntime(
            SpawnSpotRuntimeConfiguration configuration,
            OrdinaryEnemyPool pool,
            ILiveEnemyCapacity globalCapacity,
            Transform player,
            IRandomSource random)
        {
            _configuration = configuration;
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _globalCapacity = globalCapacity ?? throw new ArgumentNullException(nameof(globalCapacity));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _population = new SpawnPopulationState(configuration.Population);
            _respawnSchedule = new RespawnSchedule(configuration.Population.DesiredPopulation);
            _liveEnemies = new List<LiveEntry>(configuration.Population.DesiredPopulation);
            _occupiedAnchors = new bool[configuration.AnchorOffsets.Length];
            _anchorDistances = new float[configuration.AnchorOffsets.Length];

            for (var i = 0; i < configuration.Population.DesiredPopulation; i++)
            {
                _respawnSchedule.Schedule(0f);
            }
        }

        public string Id => _configuration.Id;

        public int LiveCount => _population.LiveCount;

        public int DesiredPopulation => _population.DesiredPopulation;

        public int PendingRespawns => _population.PendingRespawns;

        public void Tick(float deltaTime)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SpawnSpotRuntime));
            }

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            _elapsedTime += deltaTime;
            while (_population.NeedsSpawn && _respawnSchedule.HasReady(_elapsedTime))
            {
                if (!TrySpawnOne())
                {
                    break;
                }

                _respawnSchedule.ConsumeEarliest();
            }
        }

        public OrdinaryEnemyController GetLiveEnemy(int index)
        {
            if (index < 0 || index >= _liveEnemies.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _liveEnemies[index].Enemy;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            for (var i = _liveEnemies.Count - 1; i >= 0; i--)
            {
                _globalCapacity.Release();
                _pool.Return(_liveEnemies[i].Enemy);
            }

            _liveEnemies.Clear();
            _respawnSchedule.Clear();
            _isDisposed = true;
        }

        private bool TrySpawnOne()
        {
            for (var i = 0; i < _configuration.AnchorOffsets.Length; i++)
            {
                _anchorDistances[i] = Vector3.Distance(
                    _player.position,
                    _configuration.GetAnchorWorldPosition(i));
            }

            if (!SpawnAnchorSelector.TrySelect(
                    _anchorDistances,
                    _occupiedAnchors,
                    _configuration.MinimumPlayerDistance,
                    _nextAnchorIndex,
                    out var anchorIndex))
            {
                return false;
            }

            if (!_globalCapacity.TryReserve())
            {
                return false;
            }

            var enemy = _pool.Acquire(
                _configuration.Enemy,
                _player,
                _configuration.GetAnchorWorldPosition(anchorIndex),
                HandleRecycleRequested);
            if (enemy == null)
            {
                _globalCapacity.Release();
                return false;
            }

            _population.RegisterSpawn();
            _occupiedAnchors[anchorIndex] = true;
            _liveEnemies.Add(new LiveEntry(enemy, anchorIndex));
            _nextAnchorIndex = (anchorIndex + 1) % _configuration.AnchorOffsets.Length;
            return true;
        }

        private void HandleRecycleRequested(OrdinaryEnemyController enemy)
        {
            var entryIndex = -1;
            for (var i = 0; i < _liveEnemies.Count; i++)
            {
                if (_liveEnemies[i].Enemy == enemy)
                {
                    entryIndex = i;
                    break;
                }
            }

            if (entryIndex < 0)
            {
                throw new InvalidOperationException("Recycled enemy is not owned by this spawn spot.");
            }

            var anchorIndex = _liveEnemies[entryIndex].AnchorIndex;
            _liveEnemies.RemoveAt(entryIndex);
            _occupiedAnchors[anchorIndex] = false;
            _population.RegisterRecycle();
            _globalCapacity.Release();
            _pool.Return(enemy);
            var delay = _configuration.RespawnDelay.Sample(_random.NextUnit());
            _respawnSchedule.Schedule(_elapsedTime + delay);
        }
    }
}
