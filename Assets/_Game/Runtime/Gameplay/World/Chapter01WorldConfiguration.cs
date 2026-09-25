using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Gameplay.World
{
    public readonly struct WorldZoneConfiguration
    {
        public WorldZoneConfiguration(string id, Vector3 center, Vector3 landmarkPosition, Color color)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Zone id is required.", nameof(id));
            Id = id;
            Center = center;
            LandmarkPosition = landmarkPosition;
            Color = color;
        }

        public string Id { get; }
        public Vector3 Center { get; }
        public Vector3 LandmarkPosition { get; }
        public Color Color { get; }
    }

    public readonly struct WorldGateConfiguration
    {
        public WorldGateConfiguration(string id, Vector3 position, Vector3 size)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Gate id is required.", nameof(id));
            if (size.x <= 0f || size.y <= 0f || size.z <= 0f) throw new ArgumentOutOfRangeException(nameof(size));
            Id = id;
            Position = position;
            Size = size;
        }

        public string Id { get; }
        public Vector3 Position { get; }
        public Vector3 Size { get; }
    }

    public sealed class Chapter01WorldConfiguration
    {
        private readonly WorldZoneConfiguration[] _zones;

        public Chapter01WorldConfiguration(
            string worldId,
            Vector3 basinCenter,
            Vector3 groundCenter,
            Vector2 groundSize,
            IReadOnlyList<WorldZoneConfiguration> zones,
            WorldGateConfiguration eliteGate,
            WorldGateConfiguration bossGate,
            Vector3 bossArenaCenter,
            float bossArenaRadius,
            string eliteEnemyId,
            EliteGateRequirement eliteRequirement)
        {
            if (string.IsNullOrWhiteSpace(worldId)) throw new ArgumentException("World id is required.", nameof(worldId));
            if (zones == null || zones.Count != 5) throw new ArgumentException("Chapter 01 requires exactly five zones.", nameof(zones));
            if (groundSize.x <= 0f || groundSize.y <= 0f) throw new ArgumentOutOfRangeException(nameof(groundSize));
            if (bossArenaRadius <= 0f) throw new ArgumentOutOfRangeException(nameof(bossArenaRadius));
            if (string.IsNullOrWhiteSpace(eliteEnemyId)) throw new ArgumentException("Elite enemy id is required.", nameof(eliteEnemyId));
            if (eliteRequirement == null) throw new ArgumentNullException(nameof(eliteRequirement));
            if (string.Equals(eliteGate.Id, bossGate.Id, StringComparison.Ordinal)) throw new ArgumentException("Gate ids must be distinct.");
            if (eliteGate.Size.x >= groundSize.x || bossGate.Size.x >= groundSize.x)
            {
                throw new ArgumentException("Gate openings must be narrower than the world ground.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            _zones = new WorldZoneConfiguration[zones.Count];
            for (var i = 0; i < zones.Count; i++)
            {
                if (!ids.Add(zones[i].Id)) throw new ArgumentException($"Duplicate zone id: {zones[i].Id}.", nameof(zones));
                _zones[i] = zones[i];
            }

            WorldId = worldId;
            BasinCenter = basinCenter;
            GroundCenter = groundCenter;
            GroundSize = groundSize;
            EliteGate = eliteGate;
            BossGate = bossGate;
            BossArenaCenter = bossArenaCenter;
            BossArenaRadius = bossArenaRadius;
            EliteEnemyId = eliteEnemyId;
            EliteRequirement = eliteRequirement;
        }

        public string WorldId { get; }
        public Vector3 BasinCenter { get; }
        public Vector3 GroundCenter { get; }
        public Vector2 GroundSize { get; }
        public int ZoneCount => _zones.Length;
        public WorldGateConfiguration EliteGate { get; }
        public WorldGateConfiguration BossGate { get; }
        public Vector3 BossArenaCenter { get; }
        public float BossArenaRadius { get; }
        public string EliteEnemyId { get; }
        public EliteGateRequirement EliteRequirement { get; }
        public WorldZoneConfiguration GetZone(int index) => _zones[index];
    }
}
