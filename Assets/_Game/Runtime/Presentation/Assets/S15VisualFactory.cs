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

        public S15EnemyVisualState(Transform parent, S15VisualCatalog catalog)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _root = new GameObject("Enemy Art Root").transform;
            _root.SetParent(parent, false);
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
            }
            _activeVisual.SetActive(true);
            _activeId = enemyId;
        }

        public void Reset()
        {
            if (_activeVisual != null) _activeVisual.SetActive(false);
            _activeVisual = null;
            _activeId = null;
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
            for (var i = 0; i < recipe.PartCount; i++) BuildPart(root.transform, recipe.GetPart(i), catalog, i);
            return root;
        }

        private static void BuildPart(Transform parent, S15VisualPart part, S15VisualCatalog catalog, int index)
        {
            var source = part.SourceModel.GetComponentInChildren<MeshFilter>(true);
            var partObject = new GameObject($"Part {index + 1} {part.SourceModel.name}", typeof(MeshFilter), typeof(MeshRenderer));
            partObject.transform.SetParent(parent, false);
            partObject.transform.localPosition = part.LocalPosition;
            partObject.transform.localRotation = part.LocalRotation;
            partObject.transform.localScale = part.LocalScale;
            partObject.GetComponent<MeshFilter>().sharedMesh = source.sharedMesh;
            partObject.GetComponent<MeshRenderer>().sharedMaterial = catalog.GetMaterial(part.MaterialRole);
        }
    }
}
