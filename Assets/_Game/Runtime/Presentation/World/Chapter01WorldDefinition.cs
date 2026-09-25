using System;
using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    [Serializable]
    public sealed class WorldZoneAuthoring
    {
        [SerializeField] private string _id;
        [SerializeField] private Vector3 _center;
        [SerializeField] private Vector3 _landmarkPosition;
        [SerializeField] private Color _color = Color.white;

        public WorldZoneConfiguration CreateConfiguration()
        {
            return new WorldZoneConfiguration(_id, _center, _landmarkPosition, _color);
        }
    }

    [Serializable]
    public sealed class WorldGateAuthoring
    {
        [SerializeField] private string _id;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _size = new Vector3(5f, 2.5f, 0.6f);

        public WorldGateConfiguration CreateConfiguration()
        {
            return new WorldGateConfiguration(_id, _position, _size);
        }
    }

    [CreateAssetMenu(fileName = "Chapter01WorldDefinition", menuName = "Gravivore/World/Chapter 01 World Definition")]
    public sealed class Chapter01WorldDefinition : ScriptableObject
    {
        [SerializeField] private string _worldId = "chapter01-scrap-exclusion";
        [SerializeField] private Vector3 _basinCenter;
        [SerializeField] private Vector3 _groundCenter = new Vector3(0f, 0f, 8f);
        [SerializeField] private Vector2 _groundSize = new Vector2(40f, 48f);
        [SerializeField] private WorldZoneAuthoring[] _zones = Array.Empty<WorldZoneAuthoring>();
        [SerializeField] private WorldGateAuthoring _eliteGate;
        [SerializeField] private WorldGateAuthoring _bossGate;
        [SerializeField] private Vector3 _bossArenaCenter = new Vector3(0f, 0f, 27f);
        [SerializeField, Min(1f)] private float _bossArenaRadius = 5f;
        [SerializeField] private string _eliteEnemyId = "magnetar-guard";
        [SerializeField] private string[] _requiredFirstKillEnemyIds = Array.Empty<string>();
        [SerializeField, Min(1)] private long _minimumAssimilationScore = 25;

        public Chapter01WorldConfiguration Configuration => CreateConfiguration();

        public Chapter01WorldConfiguration CreateConfiguration()
        {
            if (_zones == null) throw new InvalidOperationException("World zones are required.");
            if (_eliteGate == null || _bossGate == null) throw new InvalidOperationException("Both world gates are required.");

            var zones = new WorldZoneConfiguration[_zones.Length];
            for (var i = 0; i < zones.Length; i++)
            {
                if (_zones[i] == null) throw new InvalidOperationException($"World zone {i} is missing.");
                zones[i] = _zones[i].CreateConfiguration();
            }

            return new Chapter01WorldConfiguration(
                _worldId,
                _basinCenter,
                _groundCenter,
                _groundSize,
                zones,
                _eliteGate.CreateConfiguration(),
                _bossGate.CreateConfiguration(),
                _bossArenaCenter,
                _bossArenaRadius,
                _eliteEnemyId,
                new EliteGateRequirement(_requiredFirstKillEnemyIds, _minimumAssimilationScore));
        }

        public void ValidateOrThrow() => _ = CreateConfiguration();
    }
}
