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
        [SerializeField] private GameObject _presentationPrefab;
        [SerializeField] private Vector3 _localPosition;
        [SerializeField] private Vector3 _localEulerAngles;
        [SerializeField] private Vector3 _localScale = Vector3.one;

        public string Id => _id;
        public int PartCount => _parts?.Length ?? 0;
        public GameObject PresentationPrefab => _presentationPrefab;
        public Vector3 LocalPosition => _localPosition;
        public Quaternion LocalRotation => Quaternion.Euler(_localEulerAngles);
        public Vector3 LocalScale => _localScale;
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

        public bool TryGetLandmark(string id, out S15VisualRecipe recipe)
        {
            return TryGet(_landmarks, id, out recipe);
        }

        public S15VisualRecipe GetLandmark(string id)
        {
            if (!TryGetLandmark(id, out var recipe))
                throw new InvalidOperationException($"No S15 landmark visual recipe is configured for zone {id}.");
            return recipe;
        }

        public void ValidateOrThrow()
        {
            if (_bodyMaterial == null || _accentMaterial == null || _darkMaterial == null)
                throw new InvalidOperationException("S15 visual catalog requires three authored materials.");
            ValidateRecipe(_player, "player");
            ValidateUniqueRecipes(_enemies, "enemy", 5);
            ValidateUniqueRecipes(_landmarks, "landmark", 5);
        }

        public void ValidateCoverageOrThrow(
            IReadOnlyList<string> enemyIds,
            IReadOnlyList<string> landmarkIds)
        {
            ValidateOrThrow();
            ValidateCoverage(_enemies, enemyIds, "enemy");
            ValidateCoverage(_landmarks, landmarkIds, "landmark");
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

        private static void ValidateCoverage(
            S15VisualRecipe[] recipes,
            IReadOnlyList<string> requiredIds,
            string label)
        {
            if (requiredIds == null || requiredIds.Count == 0)
                throw new ArgumentException($"At least one canonical S15 {label} id is required.", nameof(requiredIds));
            var required = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < requiredIds.Count; i++)
            {
                var id = requiredIds[i];
                if (string.IsNullOrWhiteSpace(id) || !required.Add(id))
                    throw new InvalidOperationException($"Canonical S15 {label} ids must be non-empty and unique.");
                if (!TryGet(recipes, id, out _))
                    throw new InvalidOperationException($"Missing S15 {label} visual recipe for canonical id {id}.");
            }
        }

        private static void ValidateRecipe(S15VisualRecipe recipe, string label)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id) ||
                recipe.PresentationPrefab == null && recipe.PartCount == 0)
                throw new InvalidOperationException($"S15 {label} recipe is incomplete.");
            PresentationModelBinding.ValidateTransform(recipe.LocalPosition, recipe.LocalRotation.eulerAngles, recipe.LocalScale);
            if (recipe.PresentationPrefab != null)
            {
                PresentationPrefabValidation.ValidateOrThrow(recipe.PresentationPrefab);
                return;
            }
            for (var i = 0; i < recipe.PartCount; i++)
            {
                var part = recipe.GetPart(i);
                if (part == null || part.SourceModel == null || part.LocalScale == Vector3.zero)
                    throw new InvalidOperationException($"S15 {label} part {i} is invalid.");
                if (part.SourceModel.GetComponentInChildren<MeshFilter>(true) == null)
                    throw new InvalidOperationException($"S15 {label} part {i} has no mesh.");
                PresentationPrefabValidation.ValidateSourceOrThrow(part.SourceModel);
            }
        }
    }

    public static class PresentationPrefabValidation
    {
        public static void ValidateOrThrow(GameObject prefab)
        {
            if (prefab == null || prefab.GetComponentInChildren<MeshFilter>(true) == null &&
                prefab.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
                throw new InvalidOperationException("Presentation prefab requires mesh geometry.");
            ValidateComponents(prefab, false);
            _ = new PresentationSocketSet(prefab.transform);
        }

        // Legacy kitbash parts sanitize physics/light/camera components, but scripts must never be instantiated.
        public static void ValidateSourceOrThrow(GameObject source) => ValidateComponents(source, true);

        private static void ValidateComponents(GameObject prefab, bool legacyPart)
        {
            foreach (var component in prefab.GetComponentsInChildren<Component>(true))
            {
                if (component is Transform || component is MeshFilter || component is Renderer || component is LODGroup)
                    continue;
                // Imported, in-place mechanical animation is presentation only.
                if (component is Animator animator && !animator.applyRootMotion && animator.runtimeAnimatorController != null)
                    continue;
                if (legacyPart && (component is Collider || component is Light || component is UnityEngine.Camera))
                    continue;
                throw new InvalidOperationException("Presentation prefab must contain only transforms, mesh renderers and LODs; remove scripts, physics, lights, cameras and playback components.");
            }
        }
    }
}
