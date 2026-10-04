using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    public enum Phase3VisualPartKind
    {
        Primitive,
        Beam
    }

    public enum Phase3VisualMaterialRole
    {
        Dark,
        Steel,
        Plate,
        Hostile,
        Heat
    }

    [Serializable]
    public sealed class Phase3VisualPart
    {
        [SerializeField] private string _name;
        [SerializeField] private Phase3VisualPartKind _kind;
        [SerializeField] private PrimitiveType _primitiveType = PrimitiveType.Cube;
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localScale = Vector3.one;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _beamFrom;
        [SerializeField] private Vector3 _beamTo = Vector3.forward;
        [SerializeField, Min(0.01f)] private float _beamThickness = 0.1f;
        [SerializeField] private Phase3VisualMaterialRole _materialRole;

        public string Name => _name;
        public Phase3VisualPartKind Kind => _kind;
        public PrimitiveType PrimitiveType => _primitiveType;
        public Vector3 LocalPosition => _localPosition;
        public Vector3 LocalScale => _localScale;
        public Quaternion LocalRotation => Quaternion.Euler(_localEulerAngles);
        public Vector3 BeamFrom => _beamFrom;
        public Vector3 BeamTo => _beamTo;
        public float BeamThickness => _beamThickness;
        public Phase3VisualMaterialRole MaterialRole => _materialRole;

        public void ValidateOrThrow(string ownerId, int index)
        {
            if (string.IsNullOrWhiteSpace(_name))
                throw new InvalidOperationException($"Phase 3 visual part {index} for {ownerId} requires a name.");
            if (_kind == Phase3VisualPartKind.Primitive)
            {
                if (!IsPositiveFinite(_localScale.x) || !IsPositiveFinite(_localScale.y) || !IsPositiveFinite(_localScale.z))
                    throw new InvalidOperationException($"Phase 3 primitive {_name} for {ownerId} requires positive finite scale.");
                return;
            }

            if (_kind == Phase3VisualPartKind.Beam)
            {
                if ((_beamTo - _beamFrom).sqrMagnitude <= 0.000001f || !IsPositiveFinite(_beamThickness))
                    throw new InvalidOperationException($"Phase 3 beam {_name} for {ownerId} is invalid.");
                return;
            }

            throw new InvalidOperationException($"Unknown Phase 3 part kind for {_name}.");
        }

        private static bool IsPositiveFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }

    [Serializable]
    public sealed class Phase3EnemyVisualRecipe
    {
        [SerializeField] private string _enemyId;
        [SerializeField] private bool _useLegacyPresentationPrefab;
        [SerializeField] private Phase3VisualPart[] _parts = Array.Empty<Phase3VisualPart>();

        public string EnemyId => _enemyId;
        public bool UseLegacyPresentationPrefab => _useLegacyPresentationPrefab;
        public int PartCount => _parts?.Length ?? 0;
        public Phase3VisualPart GetPart(int index) => _parts[index];

        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(_enemyId))
                throw new InvalidOperationException("Phase 3 enemy visual recipe requires an enemy id.");
            if (_useLegacyPresentationPrefab) return;
            if (_parts == null || _parts.Length == 0)
                throw new InvalidOperationException($"Phase 3 enemy visual recipe {_enemyId} requires parts.");
            for (var i = 0; i < _parts.Length; i++)
            {
                if (_parts[i] == null)
                    throw new InvalidOperationException($"Phase 3 enemy visual recipe {_enemyId} has a missing part at {i}.");
                _parts[i].ValidateOrThrow(_enemyId, i);
            }
        }
    }

    [CreateAssetMenu(fileName = "Phase3EnemyVisualCatalog", menuName = "Gravivore/Presentation/Phase 3 Enemy Visual Catalog")]
    public sealed class Phase3EnemyVisualCatalog : ScriptableObject
    {
        [SerializeField] private Phase3EnemyVisualRecipe[] _recipes = Array.Empty<Phase3EnemyVisualRecipe>();

        public int Count => _recipes?.Length ?? 0;

        public bool TryGet(string enemyId, out Phase3EnemyVisualRecipe recipe)
        {
            if (_recipes != null)
            {
                for (var i = 0; i < _recipes.Length; i++)
                {
                    if (_recipes[i] != null && string.Equals(_recipes[i].EnemyId, enemyId, StringComparison.Ordinal))
                    {
                        recipe = _recipes[i];
                        return true;
                    }
                }
            }

            recipe = null;
            return false;
        }

        public Phase3EnemyVisualRecipe Get(string enemyId)
        {
            if (!TryGet(enemyId, out var recipe))
                throw new InvalidOperationException($"No Phase 3 enemy visual recipe is configured for {enemyId}.");
            return recipe;
        }

        public void ValidateOrThrow()
        {
            if (_recipes == null || _recipes.Length == 0)
                throw new InvalidOperationException("Phase 3 enemy visual catalog requires recipes.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < _recipes.Length; i++)
            {
                if (_recipes[i] == null)
                    throw new InvalidOperationException($"Phase 3 enemy visual recipe {i} is missing.");
                _recipes[i].ValidateOrThrow();
                if (!ids.Add(_recipes[i].EnemyId))
                    throw new InvalidOperationException($"Duplicate Phase 3 enemy visual id: {_recipes[i].EnemyId}.");
            }
        }
    }
}
