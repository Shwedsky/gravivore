using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    public sealed class Phase3EnemyVisualFactory : IEnemyVisualFactory
    {
        private readonly S15VisualCatalog _legacyCatalog;
        private readonly Phase3EnemyVisualCatalog _catalog;
        private readonly Material _sharedMaterial;

        public Phase3EnemyVisualFactory(
            S15VisualCatalog legacyCatalog,
            Phase3EnemyVisualCatalog catalog,
            Material sharedMaterial)
        {
            _legacyCatalog = legacyCatalog != null ? legacyCatalog : throw new ArgumentNullException(nameof(legacyCatalog));
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _sharedMaterial = sharedMaterial != null ? sharedMaterial : throw new ArgumentNullException(nameof(sharedMaterial));
            _catalog.ValidateOrThrow();
        }

        public IEnemyVisualState Create(Transform parent)
        {
            return new Phase3EnemyVisualState(parent, _legacyCatalog, _catalog, _sharedMaterial);
        }

        public static GameObject BuildPreview(
            Transform parent,
            Phase3EnemyVisualRecipe recipe,
            Material sharedMaterial)
        {
            return BuildModel(parent, recipe, sharedMaterial);
        }

        internal static GameObject BuildModel(
            Transform parent,
            Phase3EnemyVisualRecipe recipe,
            Material sharedMaterial)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (sharedMaterial == null) throw new ArgumentNullException(nameof(sharedMaterial));

            var root = new GameObject($"Phase3 Enemy [{recipe.EnemyId}]");
            root.transform.SetParent(parent, false);
            for (var i = 0; i < recipe.PartCount; i++)
            {
                BuildPart(root.transform, recipe.GetPart(i), sharedMaterial);
            }

            return root;
        }

        private static void BuildPart(
            Transform parent,
            Phase3VisualPart part,
            Material material)
        {
            var color = ResolveColor(part.MaterialRole);
            switch (part.Kind)
            {
                case Phase3VisualPartKind.Primitive:
                    IndustrialPrimitiveFactory.Create(
                        parent,
                        part.Name,
                        part.PrimitiveType,
                        part.LocalPosition,
                        part.LocalScale,
                        part.LocalRotation,
                        material,
                        color,
                        ResolveMetallic(part.MaterialRole),
                        ResolveSmoothness(part.MaterialRole));
                    return;
                case Phase3VisualPartKind.Beam:
                    IndustrialPrimitiveFactory.Beam(
                        parent,
                        part.Name,
                        part.BeamFrom,
                        part.BeamTo,
                        part.BeamThickness,
                        material,
                        color,
                        ResolveMetallic(part.MaterialRole),
                        ResolveSmoothness(part.MaterialRole));
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "Unknown Phase 3 visual part kind.");
            }
        }

        private static Color ResolveColor(Phase3VisualMaterialRole role)
        {
            switch (role)
            {
                case Phase3VisualMaterialRole.Dark:
                    return new Color(0.045f, 0.055f, 0.065f, 1f);
                case Phase3VisualMaterialRole.Steel:
                    return new Color(0.20f, 0.23f, 0.25f, 1f);
                case Phase3VisualMaterialRole.Plate:
                    return new Color(0.34f, 0.37f, 0.39f, 1f);
                case Phase3VisualMaterialRole.Hostile:
                    return new Color(0.88f, 0.11f, 0.055f, 1f);
                case Phase3VisualMaterialRole.Heat:
                    return new Color(0.92f, 0.36f, 0.07f, 1f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown Phase 3 visual material role.");
            }
        }

        private static float ResolveMetallic(Phase3VisualMaterialRole role)
        {
            return role == Phase3VisualMaterialRole.Hostile || role == Phase3VisualMaterialRole.Heat ? 0.2f : 0.72f;
        }

        private static float ResolveSmoothness(Phase3VisualMaterialRole role)
        {
            return role == Phase3VisualMaterialRole.Hostile || role == Phase3VisualMaterialRole.Heat ? 0.78f : 0.24f;
        }
    }

    public sealed class Phase3EnemyVisualState : IEnemyVisualState
    {
        private readonly Transform _root;
        private readonly S15VisualCatalog _legacyCatalog;
        private readonly Phase3EnemyVisualCatalog _catalog;
        private readonly Material _sharedMaterial;
        private readonly Dictionary<string, GameObject> _visuals = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private GameObject _activeVisual;

        public Phase3EnemyVisualState(
            Transform parent,
            S15VisualCatalog legacyCatalog,
            Phase3EnemyVisualCatalog catalog,
            Material sharedMaterial)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _legacyCatalog = legacyCatalog != null ? legacyCatalog : throw new ArgumentNullException(nameof(legacyCatalog));
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _sharedMaterial = sharedMaterial != null ? sharedMaterial : throw new ArgumentNullException(nameof(sharedMaterial));
            _root = new GameObject("Phase 3 Enemy Art Root").transform;
            _root.SetParent(parent, false);
        }

        public string ActiveId { get; private set; }

        public void Apply(string enemyId)
        {
            Reset();
            var recipe = _catalog.Get(enemyId);
            if (!_visuals.TryGetValue(enemyId, out _activeVisual))
            {
                if (recipe.UseLegacyPresentationPrefab &&
                    _legacyCatalog.TryGetEnemy(enemyId, out var legacyRecipe) &&
                    legacyRecipe.PresentationPrefab != null)
                {
                    _activeVisual = S15VisualFactory.Build(_root, legacyRecipe, _legacyCatalog);
                    _activeVisual.name = $"Phase3 Enemy [{enemyId} / accepted legacy presentation]";
                }
                else
                {
                    _activeVisual = Phase3EnemyVisualFactory.BuildModel(_root, recipe, _sharedMaterial);
                }

                _visuals.Add(enemyId, _activeVisual);
            }

            _activeVisual.SetActive(true);
            ActiveId = enemyId;
        }

        public void Reset()
        {
            if (_activeVisual != null) _activeVisual.SetActive(false);
            _activeVisual = null;
            ActiveId = null;
            _root.localPosition = Vector3.zero;
            _root.localRotation = Quaternion.identity;
            _root.localScale = Vector3.one;
        }
    }

    public static class Phase3EncounterVisualFactory
    {
        private static readonly Color Dark = new Color(0.035f, 0.045f, 0.052f, 1f);
        private static readonly Color Steel = new Color(0.18f, 0.21f, 0.23f, 1f);
        private static readonly Color Plate = new Color(0.31f, 0.34f, 0.36f, 1f);
        private static readonly Color Heat = new Color(0.88f, 0.15f, 0.055f, 1f);
        private static readonly Color Hazard = new Color(0.95f, 0.40f, 0.06f, 1f);

        public static GameObject BuildMagnetarGuard(Transform parent, Material material)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (material == null) throw new ArgumentNullException(nameof(material));
            var root = new GameObject("Phase3 Magnetar Guard Visual");
            root.transform.SetParent(parent, false);

            IndustrialPrimitiveFactory.Create(root.transform, "Reactor Torso", PrimitiveType.Cube,
                new Vector3(0f, 1.25f, 0f), new Vector3(1.55f, 0.72f, 1.02f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root.transform, "Chest Core", PrimitiveType.Cylinder,
                new Vector3(0f, 1.27f, 0.55f), new Vector3(0.30f, 0.10f, 0.30f),
                Quaternion.Euler(90f, 0f, 0f), material, Heat, 0.2f, 0.8f);
            IndustrialPrimitiveFactory.Create(root.transform, "Left Coil", PrimitiveType.Cylinder,
                new Vector3(-0.93f, 1.52f, 0f), new Vector3(0.40f, 0.72f, 0.40f),
                Quaternion.Euler(0f, 0f, 90f), material, Hazard);
            IndustrialPrimitiveFactory.Create(root.transform, "Right Coil", PrimitiveType.Cylinder,
                new Vector3(0.93f, 1.52f, 0f), new Vector3(0.40f, 0.72f, 0.40f),
                Quaternion.Euler(0f, 0f, 90f), material, Hazard);
            IndustrialPrimitiveFactory.Create(root.transform, "Left Shoulder Armor", PrimitiveType.Cube,
                new Vector3(-0.86f, 1.56f, -0.10f), new Vector3(0.72f, 0.46f, 0.92f),
                Quaternion.Euler(0f, 0f, -9f), material, Plate);
            IndustrialPrimitiveFactory.Create(root.transform, "Right Shoulder Armor", PrimitiveType.Cube,
                new Vector3(0.86f, 1.56f, -0.10f), new Vector3(0.72f, 0.46f, 0.92f),
                Quaternion.Euler(0f, 0f, 9f), material, Plate);
            BuildGuardLeg(root.transform, material, -0.53f);
            BuildGuardLeg(root.transform, material, 0.53f);
            IndustrialPrimitiveFactory.Create(root.transform, "Rear Power Spine", PrimitiveType.Cube,
                new Vector3(0f, 1.48f, -0.64f), new Vector3(0.42f, 1.05f, 0.38f),
                Quaternion.Euler(-7f, 0f, 0f), material, Steel);
            return root;
        }

        public static GameObject BuildCustodianM0(Transform parent, Material material)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (material == null) throw new ArgumentNullException(nameof(material));
            var root = new GameObject("Phase3 Custodian M-0 Visual");
            root.transform.SetParent(parent, false);

            IndustrialPrimitiveFactory.Create(root.transform, "Lower Maintenance Chassis", PrimitiveType.Cube,
                new Vector3(0f, 0.72f, 0f), new Vector3(3.55f, 0.78f, 2.50f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root.transform, "Upper Reactor Housing", PrimitiveType.Cube,
                new Vector3(-0.28f, 1.66f, -0.16f), new Vector3(2.55f, 1.12f, 1.82f),
                Quaternion.Euler(-3f, 0f, 0f), material, Steel);
            IndustrialPrimitiveFactory.Create(root.transform, "Exposed Reactor", PrimitiveType.Cylinder,
                new Vector3(-0.18f, 1.72f, 0.82f), new Vector3(0.58f, 0.18f, 0.58f),
                Quaternion.Euler(90f, 0f, 0f), material, Heat, 0.2f, 0.82f);
            IndustrialPrimitiveFactory.Create(root.transform, "Left Armor Slab", PrimitiveType.Cube,
                new Vector3(-1.46f, 1.54f, -0.05f), new Vector3(0.58f, 1.10f, 1.88f),
                Quaternion.Euler(0f, 0f, -7f), material, Plate);
            IndustrialPrimitiveFactory.Create(root.transform, "Right Machinery Block", PrimitiveType.Cube,
                new Vector3(1.38f, 1.40f, -0.22f), new Vector3(0.88f, 0.92f, 1.48f),
                Quaternion.Euler(0f, 0f, 5f), material, Dark);
            BuildBossSupport(root.transform, material, -1.35f, -0.70f);
            BuildBossSupport(root.transform, material, 1.35f, -0.70f);
            BuildBossSupport(root.transform, material, -1.35f, 0.65f);
            BuildBossSupport(root.transform, material, 1.35f, 0.65f);

            IndustrialPrimitiveFactory.Beam(root.transform, "Crane Boom",
                new Vector3(-1.05f, 2.03f, -0.46f), new Vector3(-2.30f, 2.82f, 0.65f),
                0.28f, material, Plate);
            IndustrialPrimitiveFactory.Beam(root.transform, "Crane Forearm",
                new Vector3(-2.30f, 2.82f, 0.65f), new Vector3(-2.10f, 1.72f, 1.80f),
                0.24f, material, Steel);
            IndustrialPrimitiveFactory.Create(root.transform, "Crane Tool", PrimitiveType.Cube,
                new Vector3(-2.06f, 1.55f, 1.96f), new Vector3(0.64f, 0.34f, 0.58f),
                Quaternion.Euler(-10f, 12f, 0f), material, Hazard);

            IndustrialPrimitiveFactory.Beam(root.transform, "Right Manipulator Upper",
                new Vector3(1.34f, 1.67f, 0.16f), new Vector3(2.20f, 1.54f, 0.74f),
                0.32f, material, Steel);
            IndustrialPrimitiveFactory.Beam(root.transform, "Right Manipulator Lower",
                new Vector3(2.20f, 1.54f, 0.74f), new Vector3(2.42f, 0.90f, 1.54f),
                0.26f, material, Plate);
            IndustrialPrimitiveFactory.Create(root.transform, "Right Cutter Tool", PrimitiveType.Cube,
                new Vector3(2.45f, 0.82f, 1.72f), new Vector3(0.32f, 0.28f, 0.90f),
                Quaternion.Euler(-18f, 0f, 0f), material, Hazard);

            IndustrialPrimitiveFactory.Create(root.transform, "Rear Stack Left", PrimitiveType.Cylinder,
                new Vector3(-0.70f, 2.52f, -0.63f), new Vector3(0.28f, 0.72f, 0.28f),
                Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root.transform, "Rear Stack Right", PrimitiveType.Cylinder,
                new Vector3(0.42f, 2.38f, -0.71f), new Vector3(0.34f, 0.62f, 0.34f),
                Quaternion.identity, material, Dark);
            return root;
        }

        private static void BuildGuardLeg(Transform root, Material material, float x)
        {
            IndustrialPrimitiveFactory.Beam(root, $"Upper Support {x:0.0}",
                new Vector3(x, 1.06f, -0.10f), new Vector3(x * 1.25f, 0.57f, 0.10f),
                0.26f, material, Steel);
            IndustrialPrimitiveFactory.Beam(root, $"Lower Support {x:0.0}",
                new Vector3(x * 1.25f, 0.57f, 0.10f), new Vector3(x * 1.18f, 0.18f, 0.36f),
                0.23f, material, Dark);
            IndustrialPrimitiveFactory.Create(root, $"Foot {x:0.0}", PrimitiveType.Cube,
                new Vector3(x * 1.18f, 0.12f, 0.43f), new Vector3(0.62f, 0.18f, 0.78f),
                Quaternion.identity, material, Plate);
        }

        private static void BuildBossSupport(Transform root, Material material, float x, float z)
        {
            IndustrialPrimitiveFactory.Beam(root, $"Boss Support {x:0.0} {z:0.0}",
                new Vector3(x * 0.82f, 0.80f, z * 0.85f), new Vector3(x, 0.22f, z),
                0.30f, material, Steel);
            IndustrialPrimitiveFactory.Create(root, $"Boss Foot {x:0.0} {z:0.0}", PrimitiveType.Cube,
                new Vector3(x, 0.13f, z), new Vector3(0.72f, 0.20f, 0.86f),
                Quaternion.Euler(0f, x > 0f ? -6f : 6f, 0f), material, Plate);
        }
    }

}
