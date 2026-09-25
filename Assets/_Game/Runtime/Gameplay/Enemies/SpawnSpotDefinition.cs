using System;
using UnityEngine;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct SpawnSpotRuntimeConfiguration
    {
        public SpawnSpotRuntimeConfiguration(
            string id,
            EnemyRuntimeConfiguration enemy,
            Vector3 worldOrigin,
            Vector3[] anchorOffsets,
            int desiredPopulation,
            float minimumPlayerDistance,
            float minimumRespawnDelay,
            float maximumRespawnDelay)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Spawn spot id is required.", nameof(id));
            }

            if (anchorOffsets == null)
            {
                throw new ArgumentNullException(nameof(anchorOffsets));
            }

            var population = new SpawnPopulationPolicy(desiredPopulation);
            if (anchorOffsets.Length < population.DesiredPopulation)
            {
                throw new ArgumentException("A spawn spot needs at least one anchor per desired live enemy.");
            }

            if (float.IsNaN(minimumPlayerDistance) || float.IsInfinity(minimumPlayerDistance) ||
                minimumPlayerDistance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumPlayerDistance));
            }

            Id = id;
            Enemy = enemy;
            WorldOrigin = worldOrigin;
            AnchorOffsets = (Vector3[])anchorOffsets.Clone();
            Population = population;
            MinimumPlayerDistance = minimumPlayerDistance;
            RespawnDelay = new RespawnDelayPolicy(minimumRespawnDelay, maximumRespawnDelay);
        }

        public string Id { get; }

        public EnemyRuntimeConfiguration Enemy { get; }

        public Vector3 WorldOrigin { get; }

        public Vector3[] AnchorOffsets { get; }

        public SpawnPopulationPolicy Population { get; }

        public float MinimumPlayerDistance { get; }

        public RespawnDelayPolicy RespawnDelay { get; }

        public Vector3 GetAnchorWorldPosition(int index)
        {
            if (index < 0 || index >= AnchorOffsets.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return WorldOrigin + AnchorOffsets[index];
        }
    }

    [CreateAssetMenu(fileName = "SpawnSpotDefinition", menuName = "Gravivore/Enemies/Spawn Spot Definition")]
    public sealed class SpawnSpotDefinition : ScriptableObject
    {
        [SerializeField] private string _id = "spawn-spot";
        [SerializeField] private EnemyDefinition _enemy;
        [SerializeField] private Vector3 _worldOrigin;
        [SerializeField] private Vector3[] _anchorOffsets = Array.Empty<Vector3>();
        [SerializeField, Range(SpawnPopulationPolicy.MinimumAllowedPopulation,
            SpawnPopulationPolicy.MaximumAllowedPopulation)]
        private int _desiredPopulation = 4;
        [SerializeField, Min(0f)] private float _minimumPlayerDistance = 3f;
        [SerializeField, Min(0f)] private float _minimumRespawnDelay = 8f;
        [SerializeField, Min(0f)] private float _maximumRespawnDelay = 14f;

        public string Id => _id;

        public SpawnSpotRuntimeConfiguration CreateRuntimeConfiguration()
        {
            if (_enemy == null)
            {
                throw new InvalidOperationException($"Spawn spot {_id} requires an enemy definition.");
            }

            return new SpawnSpotRuntimeConfiguration(
                _id,
                _enemy.CreateRuntimeConfiguration(),
                _worldOrigin,
                _anchorOffsets,
                _desiredPopulation,
                _minimumPlayerDistance,
                _minimumRespawnDelay,
                _maximumRespawnDelay);
        }

        public void ValidateOrThrow()
        {
            _ = CreateRuntimeConfiguration();
        }
    }
}
