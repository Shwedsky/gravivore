using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    public enum S15MaterialRole
    {
        Body,
        Accent,
        Dark
    }

    [Serializable]
    public sealed class S15VisualPart
    {
        [SerializeField] private GameObject _sourceModel;
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _localScale = Vector3.one;
        [SerializeField] private S15MaterialRole _materialRole;

        public GameObject SourceModel => _sourceModel;
        public Vector3 LocalPosition => _localPosition;
        public Quaternion LocalRotation => Quaternion.Euler(_localEulerAngles);
        public Vector3 LocalScale => _localScale;
        public S15MaterialRole MaterialRole => _materialRole;
    }

    [Serializable]
    public sealed class S15VisualRecipe
    {
        [SerializeField] private string _id;
        [SerializeField] private S15VisualPart[] _parts = Array.Empty<S15VisualPart>();

        public string Id => _id;
        public int PartCount => _parts?.Length ?? 0;
        public S15VisualPart GetPart(int index) => _parts[index];
    }

    [CreateAssetMenu(fileName = "S15_VisualCatalog", menuName = "Gravivore/Presentation/S15 Visual Catalog")]
    public sealed class S15VisualCatalog : ScriptableObject
    {
        [SerializeField] private Material _bodyMaterial;
        [SerializeField] private Material _accentMaterial;
        [SerializeField] private Material _darkMaterial;
        [SerializeField] private S15VisualRecipe _player;
        [SerializeField] private S15VisualRecipe[] _enemies = Array.Empty<S15VisualRecipe>();
        [SerializeField] private S15VisualRecipe[] _landmarks = Array.Empty<S15VisualRecipe>();

        public S15VisualRecipe Player => _player;
        public int EnemyCount => _enemies?.Length ?? 0;
        public int LandmarkCount => _landmarks?.Length ?? 0;

        public Material GetMaterial(S15MaterialRole role)
        {
            switch (role)
            {
                case S15MaterialRole.Accent: return _accentMaterial;
                case S15MaterialRole.Dark: return _darkMaterial;
                default: return _bodyMaterial;
            }
        }

        public bool TryGetEnemy(string id, out S15VisualRecipe recipe)
        {
            return TryGet(_enemies, id, out recipe);
        }

        public S15VisualRecipe GetLandmark(int index)
        {
            if (_landmarks == null || index < 0 || index >= _landmarks.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _landmarks[index];
        }

        public void ValidateOrThrow()
        {
            if (_bodyMaterial == null || _accentMaterial == null || _darkMaterial == null)
                throw new InvalidOperationException("S15 visual catalog requires three authored materials.");
            ValidateRecipe(_player, "player");
            ValidateUniqueRecipes(_enemies, "enemy", 5);
            ValidateUniqueRecipes(_landmarks, "landmark", 5);
        }

        private static bool TryGet(S15VisualRecipe[] recipes, string id, out S15VisualRecipe recipe)
        {
            if (recipes != null)
            {
                for (var i = 0; i < recipes.Length; i++)
                {
                    if (recipes[i] != null && string.Equals(recipes[i].Id, id, StringComparison.Ordinal))
                    {
                        recipe = recipes[i];
                        return true;
                    }
                }
            }
            recipe = null;
            return false;
        }

        private static void ValidateUniqueRecipes(S15VisualRecipe[] recipes, string label, int expectedCount)
        {
            if (recipes == null || recipes.Length != expectedCount)
                throw new InvalidOperationException($"S15 requires exactly {expectedCount} {label} recipes.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < recipes.Length; i++)
            {
                ValidateRecipe(recipes[i], $"{label} {i}");
                if (!ids.Add(recipes[i].Id)) throw new InvalidOperationException($"Duplicate S15 {label} id: {recipes[i].Id}.");
            }
        }

        private static void ValidateRecipe(S15VisualRecipe recipe, string label)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id) || recipe.PartCount == 0)
                throw new InvalidOperationException($"S15 {label} recipe is incomplete.");
            for (var i = 0; i < recipe.PartCount; i++)
            {
                var part = recipe.GetPart(i);
                if (part == null || part.SourceModel == null || part.LocalScale == Vector3.zero)
                    throw new InvalidOperationException($"S15 {label} part {i} is invalid.");
                if (part.SourceModel.GetComponentInChildren<MeshFilter>(true) == null)
                    throw new InvalidOperationException($"S15 {label} part {i} has no mesh.");
            }
        }
    }
}
