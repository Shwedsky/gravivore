using System;
using UnityEngine;

namespace Gravivore.Presentation.Map
{
    public readonly struct MapWorldBounds : IEquatable<MapWorldBounds>
    {
        public MapWorldBounds(float minX, float maxX, float minZ, float maxZ)
        {
            if (!(maxX > minX)) throw new ArgumentOutOfRangeException(nameof(maxX), "maxX must be greater than minX.");
            if (!(maxZ > minZ)) throw new ArgumentOutOfRangeException(nameof(maxZ), "maxZ must be greater than minZ.");
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }
        public float Width => MaxX - MinX;
        public float Depth => MaxZ - MinZ;
        public Vector2 Center => new Vector2((MinX + MaxX) * 0.5f, (MinZ + MaxZ) * 0.5f);

        public static MapWorldBounds FromCenterSize(Vector2 center, Vector2 size)
        {
            if (size.x <= 0f || size.y <= 0f) throw new ArgumentOutOfRangeException(nameof(size));
            var half = size * 0.5f;
            return new MapWorldBounds(center.x - half.x, center.x + half.x, center.y - half.y, center.y + half.y);
        }

        public bool Equals(MapWorldBounds other) =>
            Mathf.Approximately(MinX, other.MinX) &&
            Mathf.Approximately(MaxX, other.MaxX) &&
            Mathf.Approximately(MinZ, other.MinZ) &&
            Mathf.Approximately(MaxZ, other.MaxZ);

        public override bool Equals(object obj) => obj is MapWorldBounds other && Equals(other);
        public override int GetHashCode() => MinX.GetHashCode() ^ MaxX.GetHashCode() ^ MinZ.GetHashCode() ^ MaxZ.GetHashCode();
    }

    public sealed class MapProjection
    {
        public const float DefaultChapterWidth = 72f;
        public const float DefaultChapterDepth = 140f;
        public const float DefaultLocalRadius = 28f;

        private readonly MapWorldBounds _chapterBounds;
        private readonly float _localRadius;

        public MapProjection(MapWorldBounds chapterBounds, float localRadius)
        {
            if (!(localRadius > 0f)) throw new ArgumentOutOfRangeException(nameof(localRadius));
            _chapterBounds = chapterBounds;
            _localRadius = localRadius;
        }

        public MapWorldBounds ChapterBounds => _chapterBounds;
        public float LocalRadius => _localRadius;

        public Vector2 ProjectExpanded(Vector3 worldPosition)
        {
            var x = Mathf.InverseLerp(_chapterBounds.MinX, _chapterBounds.MaxX, worldPosition.x);
            var y = Mathf.InverseLerp(_chapterBounds.MinZ, _chapterBounds.MaxZ, worldPosition.z);
            return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
        }

        public MapWorldBounds GetLocalWindow(Vector3 playerWorldPosition)
        {
            var diameter = _localRadius * 2f;
            var centerX = ClampWindowCenter(playerWorldPosition.x, _chapterBounds.MinX, _chapterBounds.MaxX, diameter);
            var centerZ = ClampWindowCenter(playerWorldPosition.z, _chapterBounds.MinZ, _chapterBounds.MaxZ, diameter);

            var halfWidth = Mathf.Min(_localRadius, _chapterBounds.Width * 0.5f);
            var halfDepth = Mathf.Min(_localRadius, _chapterBounds.Depth * 0.5f);
            return new MapWorldBounds(
                centerX - halfWidth,
                centerX + halfWidth,
                centerZ - halfDepth,
                centerZ + halfDepth);
        }

        public bool TryProjectCompact(Vector3 worldPosition, Vector3 playerWorldPosition, out Vector2 normalized)
        {
            var window = GetLocalWindow(playerWorldPosition);
            const float epsilon = 0.0001f;
            if (worldPosition.x < window.MinX - epsilon || worldPosition.x > window.MaxX + epsilon ||
                worldPosition.z < window.MinZ - epsilon || worldPosition.z > window.MaxZ + epsilon)
            {
                normalized = default;
                return false;
            }

            var x = Mathf.InverseLerp(window.MinX, window.MaxX, worldPosition.x);
            var y = Mathf.InverseLerp(window.MinZ, window.MaxZ, worldPosition.z);
            normalized = new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
            return true;
        }

        private static float ClampWindowCenter(float desiredCenter, float minimum, float maximum, float diameter)
        {
            var span = maximum - minimum;
            if (diameter >= span) return (minimum + maximum) * 0.5f;
            var half = diameter * 0.5f;
            return Mathf.Clamp(desiredCenter, minimum + half, maximum - half);
        }
    }
}
