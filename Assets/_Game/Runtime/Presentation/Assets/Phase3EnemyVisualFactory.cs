using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using UnityEngine;

namespace Gravivore.Presentation.Assets
{
    public sealed class Phase3EnemyVisualFactory : IEnemyVisualFactory
    {
        private readonly S15VisualCatalog _legacyCatalog;
        private readonly Material _sharedMaterial;

        public Phase3EnemyVisualFactory(S15VisualCatalog legacyCatalog, Material sharedMaterial)
        {
            _legacyCatalog = legacyCatalog != null ? legacyCatalog : throw new ArgumentNullException(nameof(legacyCatalog));
            _sharedMaterial = sharedMaterial != null ? sharedMaterial : throw new ArgumentNullException(nameof(sharedMaterial));
        }

        public IEnemyVisualState Create(Transform parent)
        {
            return new Phase3EnemyVisualState(parent, _legacyCatalog, _sharedMaterial);
        }

        public static GameObject BuildPreview(Transform parent, string enemyId, Material sharedMaterial)
        {
            return BuildModel(parent, enemyId, sharedMaterial);
        }

        internal static GameObject BuildModel(Transform parent, string enemyId, Material sharedMaterial)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (sharedMaterial == null) throw new ArgumentNullException(nameof(sharedMaterial));
            if (string.IsNullOrWhiteSpace(enemyId)) throw new ArgumentException("Enemy id is required.", nameof(enemyId));

            var root = new GameObject($"Phase3 Enemy [{enemyId}]");
            root.transform.SetParent(parent, false);
            switch (enemyId)
            {
                case "scout-drone":
                    BuildScout(root.transform, sharedMaterial);
                    break;
                case "cutter-unit":
                    BuildCutterFallback(root.transform, sharedMaterial);
                    break;
                case "arc-drone":
                    BuildArcDrone(root.transform, sharedMaterial);
                    break;
                case "warden":
                    BuildWarden(root.transform, sharedMaterial);
                    break;
                case "carrier":
                    BuildCarrier(root.transform, sharedMaterial);
                    break;
                default:
                    UnityEngine.Object.Destroy(root);
                    throw new InvalidOperationException($"No Phase 3 ordinary-enemy visual exists for {enemyId}.");
            }

            return root;
        }

        private static readonly Color Dark = new Color(0.045f, 0.055f, 0.065f, 1f);
        private static readonly Color Steel = new Color(0.20f, 0.23f, 0.25f, 1f);
        private static readonly Color Plate = new Color(0.34f, 0.37f, 0.39f, 1f);
        private static readonly Color Hostile = new Color(0.88f, 0.11f, 0.055f, 1f);
        private static readonly Color Heat = new Color(0.92f, 0.36f, 0.07f, 1f);

        private static void BuildScout(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Sensor Chassis", PrimitiveType.Cube,
                new Vector3(0f, 0.78f, 0f), new Vector3(0.85f, 0.22f, 1.12f), Quaternion.identity, material, Steel);
            IndustrialPrimitiveFactory.Create(root, "Forward Sensor", PrimitiveType.Cylinder,
                new Vector3(0f, 0.79f, 0.61f), new Vector3(0.26f, 0.08f, 0.26f),
                Quaternion.Euler(90f, 0f, 0f), material, Hostile, 0.2f, 0.75f);
            IndustrialPrimitiveFactory.Create(root, "Left Fin", PrimitiveType.Cube,
                new Vector3(-0.58f, 0.76f, -0.02f), new Vector3(0.52f, 0.08f, 0.74f),
                Quaternion.Euler(0f, -12f, -5f), material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Right Fin", PrimitiveType.Cube,
                new Vector3(0.58f, 0.76f, -0.02f), new Vector3(0.52f, 0.08f, 0.74f),
                Quaternion.Euler(0f, 12f, 5f), material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Left Thruster", PrimitiveType.Cylinder,
                new Vector3(-0.31f, 0.74f, -0.62f), new Vector3(0.18f, 0.24f, 0.18f),
                Quaternion.Euler(90f, 0f, 0f), material, Heat);
            IndustrialPrimitiveFactory.Create(root, "Right Thruster", PrimitiveType.Cylinder,
                new Vector3(0.31f, 0.74f, -0.62f), new Vector3(0.18f, 0.24f, 0.18f),
                Quaternion.Euler(90f, 0f, 0f), material, Heat);
            IndustrialPrimitiveFactory.Create(root, "Lower Sensor Keel", PrimitiveType.Cube,
                new Vector3(0f, 0.54f, 0.08f), new Vector3(0.22f, 0.22f, 0.64f),
                Quaternion.Euler(6f, 0f, 0f), material, Dark);
        }

        private static void BuildCutterFallback(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Low Chassis", PrimitiveType.Cube,
                new Vector3(0f, 0.50f, 0f), new Vector3(1.24f, 0.30f, 1.38f), Quaternion.identity, material, Steel);
            IndustrialPrimitiveFactory.Create(root, "Left Runner", PrimitiveType.Cube,
                new Vector3(-0.53f, 0.27f, 0f), new Vector3(0.25f, 0.22f, 1.52f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Right Runner", PrimitiveType.Cube,
                new Vector3(0.53f, 0.27f, 0f), new Vector3(0.25f, 0.22f, 1.52f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Beam(root, "Long Cutter", new Vector3(-0.38f, 0.55f, 0.35f),
                new Vector3(-0.78f, 0.42f, 1.30f), 0.17f, material, Plate);
            IndustrialPrimitiveFactory.Beam(root, "Short Cutter", new Vector3(0.38f, 0.55f, 0.32f),
                new Vector3(0.65f, 0.49f, 1.00f), 0.20f, material, Plate);
            IndustrialPrimitiveFactory.Create(root, "Offset Shield", PrimitiveType.Cube,
                new Vector3(0.52f, 0.70f, -0.08f), new Vector3(0.52f, 0.56f, 0.20f),
                Quaternion.Euler(0f, -12f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Hostile Core", PrimitiveType.Cylinder,
                new Vector3(-0.12f, 0.69f, 0.52f), new Vector3(0.20f, 0.08f, 0.20f),
                Quaternion.Euler(90f, 0f, 0f), material, Hostile, 0.15f, 0.8f);
        }

        private static void BuildArcDrone(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Arc Body", PrimitiveType.Cylinder,
                new Vector3(0f, 0.76f, 0f), new Vector3(0.72f, 0.24f, 0.72f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Arc Armor", PrimitiveType.Cube,
                new Vector3(0f, 0.76f, 0f), new Vector3(0.88f, 0.24f, 0.72f),
                Quaternion.Euler(0f, 45f, 0f), material, Steel);
            IndustrialPrimitiveFactory.Beam(root, "Left Arc Fork",
                new Vector3(-0.25f, 0.78f, 0.24f), new Vector3(-0.44f, 1.36f, 0.74f),
                0.13f, material, Plate);
            IndustrialPrimitiveFactory.Beam(root, "Right Arc Fork",
                new Vector3(0.25f, 0.78f, 0.24f), new Vector3(0.44f, 1.36f, 0.74f),
                0.13f, material, Plate);
            IndustrialPrimitiveFactory.Create(root, "Left Coil", PrimitiveType.Cylinder,
                new Vector3(-0.42f, 1.17f, 0.56f), new Vector3(0.18f, 0.24f, 0.18f),
                Quaternion.Euler(24f, 0f, -18f), material, Hostile);
            IndustrialPrimitiveFactory.Create(root, "Right Coil", PrimitiveType.Cylinder,
                new Vector3(0.42f, 1.17f, 0.56f), new Vector3(0.18f, 0.24f, 0.18f),
                Quaternion.Euler(24f, 0f, 18f), material, Hostile);
            IndustrialPrimitiveFactory.Create(root, "Rear Stabilizer", PrimitiveType.Cube,
                new Vector3(0f, 0.59f, -0.58f), new Vector3(0.65f, 0.16f, 0.56f), Quaternion.identity, material, Steel);
        }

        private static void BuildWarden(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Warden Chassis", PrimitiveType.Cube,
                new Vector3(0f, 0.72f, -0.04f), new Vector3(1.34f, 0.46f, 1.34f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Forward Shield", PrimitiveType.Cube,
                new Vector3(0f, 0.86f, 0.70f), new Vector3(1.58f, 0.78f, 0.20f),
                Quaternion.Euler(-8f, 0f, 0f), material, Plate);
            IndustrialPrimitiveFactory.Create(root, "Shield Core", PrimitiveType.Cylinder,
                new Vector3(0f, 0.88f, 0.82f), new Vector3(0.22f, 0.08f, 0.22f),
                Quaternion.Euler(90f, 0f, 0f), material, Hostile);
            BuildWardenLeg(root, material, -0.50f, 0.34f);
            BuildWardenLeg(root, material, 0.50f, 0.34f);
            BuildWardenLeg(root, material, -0.50f, -0.45f);
            BuildWardenLeg(root, material, 0.50f, -0.45f);
            IndustrialPrimitiveFactory.Create(root, "Top Armor", PrimitiveType.Cube,
                new Vector3(0f, 1.10f, -0.16f), new Vector3(0.90f, 0.20f, 0.72f),
                Quaternion.Euler(0f, 0f, 0f), material, Steel);
        }

        private static void BuildWardenLeg(Transform root, Material material, float x, float z)
        {
            IndustrialPrimitiveFactory.Beam(root, $"Leg {x:0.0} {z:0.0}",
                new Vector3(x, 0.62f, z), new Vector3(x * 1.35f, 0.18f, z * 1.28f),
                0.18f, material, Steel);
            IndustrialPrimitiveFactory.Create(root, $"Foot {x:0.0} {z:0.0}", PrimitiveType.Cube,
                new Vector3(x * 1.35f, 0.12f, z * 1.28f), new Vector3(0.36f, 0.14f, 0.42f),
                Quaternion.identity, material, Dark);
        }

        private static void BuildCarrier(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Carrier Lower Chassis", PrimitiveType.Cube,
                new Vector3(0f, 0.52f, 0f), new Vector3(1.54f, 0.42f, 1.72f), Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Cargo Power Pack", PrimitiveType.Cube,
                new Vector3(0f, 1.02f, -0.30f), new Vector3(1.30f, 0.68f, 0.92f),
                Quaternion.Euler(-4f, 0f, 0f), material, Steel);
            IndustrialPrimitiveFactory.Create(root, "Front Ram", PrimitiveType.Cube,
                new Vector3(0f, 0.58f, 0.96f), new Vector3(1.12f, 0.26f, 0.34f),
                Quaternion.Euler(-8f, 0f, 0f), material, Plate);
            IndustrialPrimitiveFactory.Beam(root, "Left Fork",
                new Vector3(-0.38f, 0.45f, 0.62f), new Vector3(-0.50f, 0.26f, 1.42f),
                0.17f, material, Plate);
            IndustrialPrimitiveFactory.Beam(root, "Right Fork",
                new Vector3(0.38f, 0.45f, 0.62f), new Vector3(0.50f, 0.26f, 1.42f),
                0.17f, material, Plate);
            BuildCarrierWheel(root, material, -0.73f, 0.48f);
            BuildCarrierWheel(root, material, 0.73f, 0.48f);
            BuildCarrierWheel(root, material, -0.73f, -0.50f);
            BuildCarrierWheel(root, material, 0.73f, -0.50f);
            IndustrialPrimitiveFactory.Create(root, "Rear Hostile Reactor", PrimitiveType.Cylinder,
                new Vector3(0f, 1.07f, -0.79f), new Vector3(0.30f, 0.12f, 0.30f),
                Quaternion.Euler(90f, 0f, 0f), material, Hostile);
        }

        private static void BuildCarrierWheel(Transform root, Material material, float x, float z)
        {
            IndustrialPrimitiveFactory.Create(root, $"Wheel {x:0.0} {z:0.0}", PrimitiveType.Cylinder,
                new Vector3(x, 0.35f, z), new Vector3(0.34f, 0.16f, 0.34f),
                Quaternion.Euler(0f, 0f, 90f), material, Steel);
        }
    }

    public sealed class Phase3EnemyVisualState : IEnemyVisualState
    {
        private readonly Transform _root;
        private readonly S15VisualCatalog _legacyCatalog;
        private readonly Material _sharedMaterial;
        private readonly Dictionary<string, GameObject> _visuals = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private GameObject _activeVisual;

        public Phase3EnemyVisualState(Transform parent, S15VisualCatalog legacyCatalog, Material sharedMaterial)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _legacyCatalog = legacyCatalog != null ? legacyCatalog : throw new ArgumentNullException(nameof(legacyCatalog));
            _sharedMaterial = sharedMaterial != null ? sharedMaterial : throw new ArgumentNullException(nameof(sharedMaterial));
            _root = new GameObject("Phase 3 Enemy Art Root").transform;
            _root.SetParent(parent, false);
        }

        public string ActiveId { get; private set; }

        public void Apply(string enemyId)
        {
            Reset();
            if (!_visuals.TryGetValue(enemyId, out _activeVisual))
            {
                if (string.Equals(enemyId, "cutter-unit", StringComparison.Ordinal) &&
                    _legacyCatalog.TryGetEnemy(enemyId, out var legacyRecipe) &&
                    legacyRecipe.PresentationPrefab != null)
                {
                    _activeVisual = S15VisualFactory.Build(_root, legacyRecipe, _legacyCatalog);
                    _activeVisual.name = "Phase3 Enemy [cutter-unit / accepted ART V3]";
                }
                else
                {
                    _activeVisual = Phase3EnemyVisualFactory.BuildModel(_root, enemyId, _sharedMaterial);
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
