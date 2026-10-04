using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    public sealed class S15EnemyVisualFactory : IEnemyVisualFactory
    {
        private readonly S15VisualCatalog _catalog;

        public S15EnemyVisualFactory(S15VisualCatalog catalog)
        {
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
        }

        public IEnemyVisualState Create(Transform parent)
        {
            return new S15EnemyVisualState(parent, _catalog);
        }
    }

    public sealed class S15EnemyVisualState : IEnemyVisualState
    {
        private readonly Transform _root;
        private readonly S15VisualCatalog _catalog;
        private readonly Dictionary<string, GameObject> _visuals = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private GameObject _activeVisual;
        private string _activeId;
        private readonly Dictionary<string, PresentationSocketSet> _sockets = new Dictionary<string, PresentationSocketSet>(StringComparer.Ordinal);
        private readonly CharacterVisualBinding _binding;

        public S15EnemyVisualState(Transform parent, S15VisualCatalog catalog)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _root = new GameObject("Enemy Art Root").transform;
            _root.SetParent(parent, false);
            _binding = parent.GetComponent<CharacterVisualBinding>() ?? parent.gameObject.AddComponent<CharacterVisualBinding>();
        }

        public void Apply(string enemyId)
        {
            if (!_catalog.TryGetEnemy(enemyId, out var recipe))
                throw new InvalidOperationException($"No S15 visual recipe is configured for enemy {enemyId}.");
            Reset();
            if (!_visuals.TryGetValue(enemyId, out _activeVisual))
            {
                _activeVisual = S15VisualFactory.Build(_root, recipe, _catalog);
                _visuals.Add(enemyId, _activeVisual);
                _sockets.Add(enemyId, new PresentationSocketSet(ModelRoot(_activeVisual, recipe)));
            }
            _activeVisual.SetActive(true);
            _activeId = enemyId;
            _binding.Bind(_root, ModelRoot(_activeVisual, recipe), _sockets[enemyId]);
        }

        private static Transform ModelRoot(GameObject visual, S15VisualRecipe recipe) =>
            recipe.PresentationPrefab != null ? visual.transform.GetChild(0) : visual.transform;

        public void Reset()
        {
            if (_activeVisual != null) _activeVisual.SetActive(false);
            _activeVisual = null;
            _activeId = null;
            _binding.Bind(_root, null, null);
            _root.localPosition = Vector3.zero;
            _root.localRotation = Quaternion.identity;
            _root.localScale = Vector3.one;
        }

        public string ActiveId => _activeId;
    }

    public static class S15VisualFactory
    {
        public static GameObject Build(Transform parent, S15VisualRecipe recipe, S15VisualCatalog catalog)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var root = new GameObject($"S15 Visual [{recipe.Id}]");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = recipe.LocalPosition;
            root.transform.localRotation = recipe.LocalRotation;
            root.transform.localScale = recipe.LocalScale;
            if (recipe.PresentationPrefab != null)
            {
                PresentationPrefabValidation.ValidateOrThrow(recipe.PresentationPrefab);
                var form = UnityEngine.Object.Instantiate(recipe.PresentationPrefab, root.transform, false);
                form.name = recipe.PresentationPrefab.name;
                form.transform.localPosition = Vector3.zero;
                form.transform.localRotation = Quaternion.identity;
                form.transform.localScale = Vector3.one;
                return root;
            }
            for (var i = 0; i < recipe.PartCount; i++) BuildPart(root.transform, recipe.GetPart(i), catalog, i);
            return root;
        }

        public static GameObject BuildPart(Transform parent, S15VisualPart part, S15VisualCatalog catalog, int index)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (part == null || part.SourceModel == null) throw new ArgumentNullException(nameof(part));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            PresentationPrefabValidation.ValidateSourceOrThrow(part.SourceModel);
            var partObject = new GameObject($"Part {index + 1} {part.SourceModel.name}");
            partObject.transform.SetParent(parent, false);
            partObject.transform.localPosition = part.LocalPosition;
            partObject.transform.localRotation = part.LocalRotation;
            partObject.transform.localScale = part.LocalScale;
            var model = UnityEngine.Object.Instantiate(part.SourceModel, partObject.transform, false);
            model.name = part.SourceModel.name;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            var colliders = model.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
                Remove(colliders[i]);
            }
            DisableAndRemove<Light>(model);
            DisableAndRemove<UnityEngine.Camera>(model);
            var renderers = model.GetComponentsInChildren<MeshRenderer>(true);
            var material = catalog.GetMaterial(part.MaterialRole);
            for (var i = 0; i < renderers.Length; i++)
            {
                var slots = Mathf.Max(1, renderers[i].sharedMaterials.Length);
                var materials = new Material[slots];
                for (var j = 0; j < materials.Length; j++) materials[j] = material;
                renderers[i].sharedMaterials = materials;
            }

            return partObject;
        }

        private static void DisableAndRemove<T>(GameObject root) where T : Behaviour
        {
            var components = root.GetComponentsInChildren<T>(true);
            for (var i = 0; i < components.Length; i++)
            {
                components[i].enabled = false;
                Remove(components[i]);
            }
        }

        private static void Remove(UnityEngine.Object component)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(component);
            else UnityEngine.Object.DestroyImmediate(component);
        }
    }
}
