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
            ValidateFinite(center, nameof(center));
            ValidateFinite(landmarkPosition, nameof(landmarkPosition));
            if (!IsFinite(color.r) || !IsFinite(color.g) || !IsFinite(color.b) || !IsFinite(color.a))
            {
                throw new ArgumentOutOfRangeException(nameof(color));
            }

            Id = id;
            Center = center;
            LandmarkPosition = landmarkPosition;
            Color = color;
        }

        public string Id { get; }
        public Vector3 Center { get; }
        public Vector3 LandmarkPosition { get; }
        public Color Color { get; }

        private static void ValidateFinite(Vector3 value, string parameterName)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y) || !IsFinite(value.z))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public readonly struct WorldGateConfiguration
    {
        public WorldGateConfiguration(string id, Vector3 position, Vector3 size)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Gate id is required.", nameof(id));
            if (!IsFinite(position.x) || !IsFinite(position.y) || !IsFinite(position.z))
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            if (!IsFinite(size.x) || !IsFinite(size.y) || !IsFinite(size.z) ||
                size.x <= 0f || size.y <= 0f || size.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Id = id;
            Position = position;
            Size = size;
        }

        public string Id { get; }
        public Vector3 Position { get; }
        public Vector3 Size { get; }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public readonly struct WorldBoundaryConfiguration
    {
        public WorldBoundaryConfiguration(float thickness, float height)
        {
            if (!IsPositiveFinite(thickness) || !IsPositiveFinite(height))
            {
                throw new ArgumentOutOfRangeException(nameof(thickness), "Boundary dimensions must be finite and positive.");
            }

            Thickness = thickness;
            Height = height;
        }

        public float Thickness { get; }
        public float Height { get; }

        private static bool IsPositiveFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }

    public readonly struct WorldBounds
    {
        public WorldBounds(Vector3 center, Vector2 size)
        {
            if (!IsFinite(center.x) || !IsFinite(center.y) || !IsFinite(center.z) ||
                !IsFinite(size.x) || !IsFinite(size.y) || size.x <= 0f || size.y <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "World bounds must be finite and positive.");
            }

            Center = center;
            Size = size;
            MinX = center.x - size.x * 0.5f;
            MaxX = center.x + size.x * 0.5f;
            MinZ = center.z - size.y * 0.5f;
            MaxZ = center.z + size.y * 0.5f;
        }

        public Vector3 Center { get; }
        public Vector2 Size { get; }
        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public bool Contains(Vector3 point, float clearance = 0f)
        {
            if (!IsFinite(clearance) || clearance < 0f) throw new ArgumentOutOfRangeException(nameof(clearance));
            return point.x >= MinX + clearance && point.x <= MaxX - clearance &&
                   point.z >= MinZ + clearance && point.z <= MaxZ - clearance;
        }

        public bool ContainsRectangle(Vector3 center, Vector2 size)
        {
            if (!IsFinite(size.x) || !IsFinite(size.y) || size.x <= 0f || size.y <= 0f) return false;
            return center.x - size.x * 0.5f > MinX && center.x + size.x * 0.5f < MaxX &&
                   center.z - size.y * 0.5f > MinZ && center.z + size.y * 0.5f < MaxZ;
        }

        public bool ContainsCircle(Vector3 center, float radius)
        {
            return IsFinite(radius) && radius > 0f && Contains(center, radius);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public sealed class Chapter01WorldConfiguration
    {
        private readonly WorldZoneConfiguration[] _zones;

        public Chapter01WorldConfiguration(
            string worldId,
            Vector3 basinCenter,
            Vector3 groundCenter,
            Vector2 groundSize,
            WorldBoundaryConfiguration boundary,
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
            var bounds = new WorldBounds(groundCenter, groundSize);
            if (float.IsNaN(bossArenaRadius) || float.IsInfinity(bossArenaRadius) || bossArenaRadius <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(bossArenaRadius));
            }
            if (string.IsNullOrWhiteSpace(eliteEnemyId)) throw new ArgumentException("Elite enemy id is required.", nameof(eliteEnemyId));
            if (eliteRequirement == null) throw new ArgumentNullException(nameof(eliteRequirement));
            if (string.Equals(eliteGate.Id, bossGate.Id, StringComparison.Ordinal)) throw new ArgumentException("Gate ids must be distinct.");
            if (!bounds.ContainsRectangle(eliteGate.Position, new Vector2(eliteGate.Size.x, eliteGate.Size.z)) ||
                !bounds.ContainsRectangle(bossGate.Position, new Vector2(bossGate.Size.x, bossGate.Size.z)))
            {
                throw new ArgumentException("Gate geometry must fit strictly inside the world boundaries.");
            }

            if (boundary.Height < eliteGate.Size.y || boundary.Height < bossGate.Size.y)
            {
                throw new ArgumentException("World boundaries must be at least as high as gate blockers.", nameof(boundary));
            }

            if (!bounds.Contains(basinCenter) || !bounds.ContainsCircle(bossArenaCenter, bossArenaRadius))
            {
                throw new ArgumentException("Basin and boss arena must fit inside the world boundaries.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            _zones = new WorldZoneConfiguration[zones.Count];
            for (var i = 0; i < zones.Count; i++)
            {
                if (!ids.Add(zones[i].Id)) throw new ArgumentException($"Duplicate zone id: {zones[i].Id}.", nameof(zones));
                if (!bounds.Contains(zones[i].Center) || !bounds.Contains(zones[i].LandmarkPosition))
                {
                    throw new ArgumentException($"Zone {zones[i].Id} must fit inside the world boundaries.", nameof(zones));
                }

                _zones[i] = zones[i];
            }

            WorldId = worldId;
            BasinCenter = basinCenter;
            GroundCenter = groundCenter;
            GroundSize = groundSize;
            Bounds = bounds;
            Boundary = boundary;
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
        public WorldBounds Bounds { get; }
        public WorldBoundaryConfiguration Boundary { get; }
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
