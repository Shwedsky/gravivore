using System;
using UnityEngine;

namespace Gravivore.Gameplay.World
{
    [Serializable]
    public sealed class WorldLayoutVolume
    {
        [SerializeField] private string _id;
        [SerializeField] private Vector3 _center;
        [SerializeField] private Vector3 _size;
        public string Id => _id;
        public Vector3 Center => _center;
        public Vector3 Size => _size;
        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_id) || !Finite(_center) || !Finite(_size) ||
                _size.x <= 0 || _size.y <= 0 || _size.z <= 0)
                throw new InvalidOperationException("World layout volumes require a stable id and finite positive dimensions.");
        }
        private static bool Finite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }

    /// <summary>Authored collision and route data, independent of presentation meshes and encounter state.</summary>
    [CreateAssetMenu(menuName = "Gravivore/World/Chapter World Layout")]
    public sealed class ChapterWorldLayoutDefinition : ScriptableObject
    {
        [SerializeField] private WorldLayoutVolume[] _surfaces = Array.Empty<WorldLayoutVolume>();
        [SerializeField] private WorldLayoutVolume[] _blockers = Array.Empty<WorldLayoutVolume>();
        [SerializeField] private Vector3[] _route = Array.Empty<Vector3>();
        [SerializeField] private Vector3[] _strongSpotPositions = Array.Empty<Vector3>();
        public System.Collections.Generic.IReadOnlyList<Vector3> StrongSpotPositions => _strongSpotPositions;
        public int SurfaceCount => _surfaces.Length;
        public int BlockerCount => _blockers.Length;
        public int RoutePointCount => _route.Length;
        public WorldLayoutVolume GetSurface(int index) => _surfaces[index];
        public WorldLayoutVolume GetBlocker(int index) => _blockers[index];
        public Vector3 GetRoutePoint(int index) => _route[index];
        public bool HasGround(Vector3 point, float clearance = 0f)
        {
            foreach (var surface in _surfaces)
            {
                var half = surface.Size * .5f;
                if (Mathf.Abs(point.x - surface.Center.x) <= half.x - clearance &&
                    Mathf.Abs(point.z - surface.Center.z) <= half.z - clearance) return true;
            }
            return false;
        }
        public bool IsClear(Vector3 point, float clearance)
        {
            if (!HasGround(point)) return false;
            foreach (var blocker in _blockers)
            {
                var half = blocker.Size * .5f;
                if (Mathf.Abs(point.x - blocker.Center.x) < half.x + clearance &&
                    Mathf.Abs(point.z - blocker.Center.z) < half.z + clearance) return false;
            }
            return true;
        }
        public void ValidateOrThrow()
        {
            if (_surfaces == null || _surfaces.Length == 0 || _blockers == null || _route == null || _route.Length < 2)
                throw new InvalidOperationException("An authored layout requires ground, blockers and a traversal route.");
            var ids = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            foreach (var volume in _surfaces)
            {
                if (volume == null) throw new InvalidOperationException("Missing movement surface.");
                volume.ValidateOrThrow();
                if (!ids.Add(volume.Id)) throw new InvalidOperationException("Duplicate world volume id: " + volume.Id);
            }
            foreach (var volume in _blockers)
            {
                if (volume == null) throw new InvalidOperationException("Missing architectural blocker.");
                volume.ValidateOrThrow();
                if (!ids.Add(volume.Id)) throw new InvalidOperationException("Duplicate world volume id: " + volume.Id);
            }
            foreach (var point in _route)
                if (!IsClear(point, 1f)) throw new InvalidOperationException("Traversal route intersects architecture or lacks ground.");
        }
    }
}
