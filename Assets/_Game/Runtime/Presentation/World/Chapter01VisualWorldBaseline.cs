using System;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Assets;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    public readonly struct VisualWorldBaselineReport
    {
        public VisualWorldBaselineReport(GameObject root, int zoneLandmarkCount)
        {
            Root = root != null ? root : throw new ArgumentNullException(nameof(root));
            ZoneLandmarkCount = zoneLandmarkCount;
            RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
            EnabledColliderCount = IndustrialPrimitiveFactory.CountEnabledColliders(root);
        }

        public GameObject Root { get; }
        public int ZoneLandmarkCount { get; }
        public int RendererCount { get; }
        public int EnabledColliderCount { get; }
    }

    public static class Chapter01VisualWorldBaseline
    {
        private static readonly Color Dark = new Color(0.035f, 0.045f, 0.052f, 1f);
        private static readonly Color Steel = new Color(0.16f, 0.19f, 0.21f, 1f);
        private static readonly Color Plate = new Color(0.27f, 0.30f, 0.32f, 1f);
        private static readonly Color Worn = new Color(0.22f, 0.19f, 0.16f, 1f);
        private static readonly Color Rust = new Color(0.34f, 0.15f, 0.065f, 1f);
        private static readonly Color Cyan = new Color(0.06f, 0.62f, 0.82f, 1f);
        private static readonly Color Hostile = new Color(0.82f, 0.10f, 0.045f, 1f);
        private static readonly Color Hazard = new Color(0.92f, 0.38f, 0.055f, 1f);
        private static readonly Color Amber = new Color(0.76f, 0.55f, 0.10f, 1f);

        public static VisualWorldBaselineReport Build(
            Transform parent,
            Chapter01WorldConfiguration configuration,
            Material sharedMaterial)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (sharedMaterial == null) throw new ArgumentNullException(nameof(sharedMaterial));

            var root = IndustrialPrimitiveFactory.Group(parent, "Phase 3 Visual World Baseline", Vector3.zero);
            BuildPerimeterStructure(root, configuration, sharedMaterial);
            BuildRepairHub(root, configuration.BasinCenter, sharedMaterial);
            BuildRouteDressing(root, configuration, sharedMaterial);

            var landmarks = 0;
            for (var i = 0; i < configuration.ZoneCount; i++)
            {
                BuildZoneLandmark(root, configuration.GetZone(i), sharedMaterial);
                landmarks++;
            }

            BuildEliteApproach(root, configuration, sharedMaterial);
            BuildBossDestination(root, configuration, sharedMaterial);
            return new VisualWorldBaselineReport(root.gameObject, landmarks);
        }

        private static void BuildPerimeterStructure(
            Transform root,
            Chapter01WorldConfiguration configuration,
            Material material)
        {
            var bounds = configuration.Bounds;
            var zStart = bounds.MinZ + 7f;
            var zEnd = bounds.MaxZ - 7f;
            const int segmentCount = 8;
            for (var i = 0; i < segmentCount; i++)
            {
                var t = segmentCount == 1 ? 0f : i / (float)(segmentCount - 1);
                var z = Mathf.Lerp(zStart, zEnd, t);
                BuildWallFrame(root, material, bounds.MinX + 1.1f, z, true, i);
                BuildWallFrame(root, material, bounds.MaxX - 1.1f, z, false, i);
            }

            for (var i = 0; i < 6; i++)
            {
                var x = Mathf.Lerp(bounds.MinX + 6f, bounds.MaxX - 6f, i / 5f);
                IndustrialPrimitiveFactory.Create(root, $"South Service Block {i}", PrimitiveType.Cube,
                    new Vector3(x, 0.48f, bounds.MinZ + 1.2f), new Vector3(5.2f, 0.90f, 1.30f),
                    Quaternion.identity, material, Dark);
                IndustrialPrimitiveFactory.Create(root, $"North Service Block {i}", PrimitiveType.Cube,
                    new Vector3(x, 0.62f, bounds.MaxZ - 1.2f), new Vector3(5.2f, 1.16f, 1.30f),
                    Quaternion.identity, material, Steel);
            }
        }

        private static void BuildWallFrame(
            Transform root,
            Material material,
            float x,
            float z,
            bool west,
            int index)
        {
            var side = west ? -1f : 1f;
            IndustrialPrimitiveFactory.Create(root, $"{(west ? "West" : "East")} Buttress {index}", PrimitiveType.Cube,
                new Vector3(x, 1.55f, z), new Vector3(0.52f, 3.10f, 1.18f),
                Quaternion.Euler(0f, 0f, side * 4f), material, index % 2 == 0 ? Dark : Steel);
            IndustrialPrimitiveFactory.Create(root, $"{(west ? "West" : "East")} Pipe {index}", PrimitiveType.Cylinder,
                new Vector3(x - side * 0.46f, 1.06f, z), new Vector3(0.18f, 1.58f, 0.18f),
                Quaternion.identity, material, Worn, 0.75f, 0.18f);
        }

        private static void BuildRepairHub(Transform root, Vector3 center, Material material)
        {
            var hub = IndustrialPrimitiveFactory.Group(root, "Repair Hub", center);
            IndustrialPrimitiveFactory.Create(hub, "Repair Platform", PrimitiveType.Cylinder,
                new Vector3(0f, 0.05f, 0f), new Vector3(7.8f, 0.08f, 7.8f),
                Quaternion.identity, material, Steel);
            IndustrialPrimitiveFactory.Create(hub, "Diagnostic Core", PrimitiveType.Cylinder,
                new Vector3(0f, 0.14f, 0f), new Vector3(3.7f, 0.045f, 3.7f),
                Quaternion.identity, material, Cyan, 0.2f, 0.75f);

            BuildRepairArm(hub, material, -3.2f, -1.9f, 28f, 0);
            BuildRepairArm(hub, material, 3.2f, -1.9f, -28f, 1);
            BuildRepairArm(hub, material, -3.2f, 1.9f, 150f, 2);
            BuildRepairArm(hub, material, 3.2f, 1.9f, -150f, 3);

            IndustrialPrimitiveFactory.Create(hub, "Left Service Tower", PrimitiveType.Cube,
                new Vector3(-4.35f, 1.65f, -0.35f), new Vector3(1.10f, 3.30f, 2.30f),
                Quaternion.Euler(0f, 8f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Create(hub, "Right Service Tower", PrimitiveType.Cube,
                new Vector3(4.35f, 1.65f, -0.35f), new Vector3(1.10f, 3.30f, 2.30f),
                Quaternion.Euler(0f, -8f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Beam(hub, "Rear Power Trunk",
                new Vector3(-4.20f, 2.85f, -2.55f), new Vector3(4.20f, 2.85f, -2.55f),
                0.30f, material, Steel);
        }

        private static void BuildRepairArm(
            Transform hub,
            Material material,
            float x,
            float z,
            float yaw,
            int index)
        {
            var arm = IndustrialPrimitiveFactory.Group(hub, $"Service Manipulator {index + 1}", new Vector3(x, 0f, z));
            arm.localRotation = Quaternion.Euler(0f, yaw, 0f);
            IndustrialPrimitiveFactory.Create(arm, "Pedestal", PrimitiveType.Cylinder,
                new Vector3(0f, 0.55f, 0f), new Vector3(0.62f, 0.55f, 0.62f),
                Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Beam(arm, "Upper Arm",
                new Vector3(0f, 0.95f, 0f), new Vector3(0f, 2.05f, 0.72f),
                0.24f, material, Steel);
            IndustrialPrimitiveFactory.Beam(arm, "Forearm",
                new Vector3(0f, 2.05f, 0.72f), new Vector3(0f, 1.36f, 1.72f),
                0.20f, material, Plate);
            IndustrialPrimitiveFactory.Create(arm, "Repair Emitter", PrimitiveType.Cylinder,
                new Vector3(0f, 1.30f, 1.82f), new Vector3(0.20f, 0.16f, 0.20f),
                Quaternion.Euler(90f, 0f, 0f), material, Cyan, 0.15f, 0.8f);
        }

        private static void BuildRouteDressing(
            Transform root,
            Chapter01WorldConfiguration configuration,
            Material material)
        {
            var basin = configuration.BasinCenter;
            var elite = configuration.EliteGate.Position;
            for (var i = 0; i < 7; i++)
            {
                var t = (i + 1f) / 8f;
                var z = Mathf.Lerp(basin.z + 8f, elite.z - 4f, t);
                var x = i % 2 == 0 ? -7.5f : 7.5f;
                IndustrialPrimitiveFactory.Create(root, $"Route Utility {i + 1}", PrimitiveType.Cube,
                    new Vector3(x, 0.55f, z), new Vector3(1.25f, 1.10f, 1.65f),
                    Quaternion.Euler(0f, i % 2 == 0 ? -7f : 7f, 0f), material, i % 3 == 0 ? Worn : Dark);
                IndustrialPrimitiveFactory.Create(root, $"Route Conduit {i + 1}", PrimitiveType.Cylinder,
                    new Vector3(x * 0.92f, 0.42f, z + 1.05f), new Vector3(0.16f, 0.82f, 0.16f),
                    Quaternion.Euler(90f, 0f, 0f), material, Steel);
            }
        }

        private static void BuildZoneLandmark(
            Transform root,
            WorldZoneConfiguration zone,
            Material material)
        {
            var group = IndustrialPrimitiveFactory.Group(root, $"Zone Landmark [{zone.Id}]", zone.LandmarkPosition);
            switch (zone.Id)
            {
                case "relay-yard":
                    BuildRelayYard(group, material);
                    break;
                case "cutting-floor":
                    BuildCuttingFloor(group, material);
                    break;
                case "shield-dump":
                    BuildShieldDump(group, material);
                    break;
                case "capacitor-field":
                    BuildCapacitorField(group, material);
                    break;
                case "hauler-graveyard":
                    BuildHaulerGraveyard(group, material);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown Chapter 1 zone: {zone.Id}.");
            }
        }

        private static void BuildRelayYard(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Relay Mast", PrimitiveType.Cylinder,
                new Vector3(0f, 2.40f, 0f), new Vector3(0.34f, 2.40f, 0.34f),
                Quaternion.identity, material, Steel);
            IndustrialPrimitiveFactory.Beam(root, "Relay Crossbar",
                new Vector3(-1.45f, 3.25f, 0f), new Vector3(1.45f, 3.25f, 0f),
                0.14f, material, Plate);
            IndustrialPrimitiveFactory.Create(root, "Dish", PrimitiveType.Cylinder,
                new Vector3(0f, 4.45f, 0.10f), new Vector3(1.60f, 0.10f, 1.60f),
                Quaternion.Euler(68f, 0f, 0f), material, Plate);
            IndustrialPrimitiveFactory.Create(root, "Relay Light", PrimitiveType.Cylinder,
                new Vector3(0f, 4.60f, 0.55f), new Vector3(0.18f, 0.10f, 0.18f),
                Quaternion.Euler(90f, 0f, 0f), material, Cyan);
            IndustrialPrimitiveFactory.Create(root, "Cabinet A", PrimitiveType.Cube,
                new Vector3(-1.55f, 0.72f, -0.70f), new Vector3(1.10f, 1.44f, 0.90f),
                Quaternion.Euler(0f, 8f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Cabinet B", PrimitiveType.Cube,
                new Vector3(1.45f, 0.56f, -0.82f), new Vector3(0.94f, 1.12f, 0.84f),
                Quaternion.Euler(0f, -12f, 0f), material, Worn);
        }

        private static void BuildCuttingFloor(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Cutting Bed", PrimitiveType.Cube,
                new Vector3(0f, 0.20f, 0f), new Vector3(4.80f, 0.30f, 5.80f),
                Quaternion.identity, material, Worn);
            IndustrialPrimitiveFactory.Create(root, "Left Gantry", PrimitiveType.Cube,
                new Vector3(-2.45f, 2.10f, 0f), new Vector3(0.42f, 4.20f, 0.70f),
                Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Right Gantry", PrimitiveType.Cube,
                new Vector3(2.45f, 2.10f, 0f), new Vector3(0.42f, 4.20f, 0.70f),
                Quaternion.identity, material, Dark);
            IndustrialPrimitiveFactory.Beam(root, "Gantry Bridge",
                new Vector3(-2.45f, 3.92f, 0f), new Vector3(2.45f, 3.92f, 0f),
                0.38f, material, Steel);
            IndustrialPrimitiveFactory.Beam(root, "Cutter Head",
                new Vector3(0.40f, 3.58f, 0f), new Vector3(-0.25f, 1.30f, 0.25f),
                0.26f, material, Hazard);
            IndustrialPrimitiveFactory.Create(root, "Rail Left", PrimitiveType.Cube,
                new Vector3(-1.45f, 0.42f, 0f), new Vector3(0.18f, 0.14f, 5.20f),
                Quaternion.identity, material, Plate);
            IndustrialPrimitiveFactory.Create(root, "Rail Right", PrimitiveType.Cube,
                new Vector3(1.45f, 0.42f, 0f), new Vector3(0.18f, 0.14f, 5.20f),
                Quaternion.identity, material, Plate);
        }

        private static void BuildShieldDump(Transform root, Material material)
        {
            BuildBrokenEmitterRing(root, material, new Vector3(0f, 1.05f, 0f), 2.30f, 0f, 9);
            BuildBrokenEmitterRing(root, material, new Vector3(-1.25f, 0.65f, -1.60f), 1.45f, 24f, 6);
            IndustrialPrimitiveFactory.Create(root, "Generator A", PrimitiveType.Cube,
                new Vector3(2.20f, 0.72f, -1.15f), new Vector3(1.30f, 1.44f, 1.10f),
                Quaternion.Euler(0f, -18f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Generator B", PrimitiveType.Cube,
                new Vector3(-2.05f, 0.56f, 1.35f), new Vector3(1.05f, 1.12f, 0.92f),
                Quaternion.Euler(0f, 14f, 0f), material, Worn);
            IndustrialPrimitiveFactory.Create(root, "Residual Field", PrimitiveType.Cylinder,
                new Vector3(0f, 1.05f, 0f), new Vector3(1.30f, 0.045f, 1.30f),
                Quaternion.Euler(90f, 0f, 0f), material, new Color(0.12f, 0.52f, 0.28f, 1f), 0.15f, 0.8f);
        }

        private static void BuildBrokenEmitterRing(
            Transform root,
            Material material,
            Vector3 center,
            float radius,
            float tilt,
            int segmentCount)
        {
            var ring = IndustrialPrimitiveFactory.Group(root, $"Broken Shield Ring {radius:0.0}", center);
            ring.localRotation = Quaternion.Euler(tilt, 0f, tilt * 0.3f);
            for (var i = 0; i < segmentCount; i++)
            {
                if (i == 2 || i == segmentCount - 1) continue;
                var angle = (360f / segmentCount) * i;
                var radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Cos(radians) * radius, 0f, Mathf.Sin(radians) * radius);
                IndustrialPrimitiveFactory.Create(ring, $"Emitter Segment {i}", PrimitiveType.Cube,
                    position, new Vector3(0.80f, 0.22f, 0.30f),
                    Quaternion.Euler(0f, -angle + 90f, 0f), material, Plate);
            }
        }

        private static void BuildCapacitorField(Transform root, Material material)
        {
            var positions = new[]
            {
                new Vector3(-1.70f, 0f, -1.20f),
                new Vector3(1.55f, 0f, -0.95f),
                new Vector3(-0.95f, 0f, 1.35f),
                new Vector3(1.25f, 0f, 1.55f)
            };
            for (var i = 0; i < positions.Length; i++)
            {
                var height = 2.5f + i * 0.42f;
                IndustrialPrimitiveFactory.Create(root, $"Capacitor {i + 1}", PrimitiveType.Cylinder,
                    positions[i] + Vector3.up * (height * 0.5f), new Vector3(0.60f, height * 0.5f, 0.60f),
                    Quaternion.identity, material, i == 2 ? Worn : Steel);
                IndustrialPrimitiveFactory.Create(root, $"Capacitor Crown {i + 1}", PrimitiveType.Cylinder,
                    positions[i] + Vector3.up * (height + 0.16f), new Vector3(0.78f, 0.12f, 0.78f),
                    Quaternion.identity, material, i == 2 ? Rust : Amber);
            }
            IndustrialPrimitiveFactory.Beam(root, "Bus Bar A",
                new Vector3(-1.70f, 3.05f, -1.20f), new Vector3(1.55f, 3.45f, -0.95f),
                0.16f, material, Plate);
            IndustrialPrimitiveFactory.Beam(root, "Bus Bar B",
                new Vector3(-0.95f, 3.75f, 1.35f), new Vector3(1.25f, 4.15f, 1.55f),
                0.16f, material, Plate);
        }

        private static void BuildHaulerGraveyard(Transform root, Material material)
        {
            IndustrialPrimitiveFactory.Create(root, "Wreck A", PrimitiveType.Cube,
                new Vector3(-1.35f, 0.78f, -0.35f), new Vector3(3.60f, 1.20f, 2.05f),
                Quaternion.Euler(8f, 28f, 11f), material, Worn);
            IndustrialPrimitiveFactory.Create(root, "Wreck B", PrimitiveType.Cube,
                new Vector3(1.70f, 0.58f, 1.40f), new Vector3(2.45f, 0.92f, 1.65f),
                Quaternion.Euler(-5f, -32f, -8f), material, Dark);
            IndustrialPrimitiveFactory.Create(root, "Track A", PrimitiveType.Cylinder,
                new Vector3(-2.15f, 0.36f, -0.10f), new Vector3(0.72f, 0.24f, 0.72f),
                Quaternion.Euler(0f, 0f, 90f), material, Steel);
            IndustrialPrimitiveFactory.Create(root, "Track B", PrimitiveType.Cylinder,
                new Vector3(2.32f, 0.32f, 1.12f), new Vector3(0.62f, 0.22f, 0.62f),
                Quaternion.Euler(0f, 0f, 90f), material, Steel);
            IndustrialPrimitiveFactory.Beam(root, "Collapsed Crane",
                new Vector3(-2.70f, 0.55f, 2.25f), new Vector3(2.55f, 2.85f, -1.70f),
                0.30f, material, Rust);
            IndustrialPrimitiveFactory.Create(root, "Broken Cabin", PrimitiveType.Cube,
                new Vector3(-0.55f, 1.38f, -0.50f), new Vector3(1.45f, 1.12f, 1.20f),
                Quaternion.Euler(6f, 30f, 8f), material, Plate);
        }

        private static void BuildEliteApproach(
            Transform root,
            Chapter01WorldConfiguration configuration,
            Material material)
        {
            var approach = IndustrialPrimitiveFactory.Group(root, "Elite Approach",
                Vector3.Lerp(configuration.EliteGate.Position, configuration.BossGate.Position, 0.34f));
            for (var side = -1; side <= 1; side += 2)
            {
                var x = side * 5.6f;
                IndustrialPrimitiveFactory.Create(approach, $"Magnetic Pylon {side}", PrimitiveType.Cylinder,
                    new Vector3(x, 2.0f, 0f), new Vector3(0.55f, 2.0f, 0.55f),
                    Quaternion.identity, material, Dark);
                IndustrialPrimitiveFactory.Create(approach, $"Pylon Coil {side}", PrimitiveType.Cylinder,
                    new Vector3(x, 2.55f, 0f), new Vector3(0.78f, 0.30f, 0.78f),
                    Quaternion.identity, material, Hazard);
                IndustrialPrimitiveFactory.Create(approach, $"Pylon Base {side}", PrimitiveType.Cube,
                    new Vector3(x, 0.28f, 0f), new Vector3(1.60f, 0.48f, 1.70f),
                    Quaternion.identity, material, Steel);
            }
        }

        private static void BuildBossDestination(
            Transform root,
            Chapter01WorldConfiguration configuration,
            Material material)
        {
            var destination = IndustrialPrimitiveFactory.Group(root, "Boss Destination", configuration.BossArenaCenter);
            var radius = configuration.BossArenaRadius + 1.15f;
            const int segments = 14;
            for (var i = 0; i < segments; i++)
            {
                var angle = (360f / segments) * i;
                var radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Cos(radians) * radius, 0.14f, Mathf.Sin(radians) * radius);
                IndustrialPrimitiveFactory.Create(destination, $"Arena Segment {i + 1}", PrimitiveType.Cube,
                    position, new Vector3(2.10f, 0.20f, 0.48f),
                    Quaternion.Euler(0f, -angle + 90f, 0f), material, i % 3 == 0 ? Hazard : Steel);
            }

            IndustrialPrimitiveFactory.Create(destination, "West Boss Service Tower", PrimitiveType.Cube,
                new Vector3(-7.6f, 2.20f, -0.80f), new Vector3(1.20f, 4.40f, 2.60f),
                Quaternion.Euler(0f, 5f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Create(destination, "East Boss Service Tower", PrimitiveType.Cube,
                new Vector3(7.6f, 2.20f, -0.80f), new Vector3(1.20f, 4.40f, 2.60f),
                Quaternion.Euler(0f, -5f, 0f), material, Dark);
            IndustrialPrimitiveFactory.Beam(destination, "Broken Crane Boom",
                new Vector3(-7.1f, 3.65f, 2.0f), new Vector3(-3.1f, 5.10f, 4.3f),
                0.34f, material, Worn);
            IndustrialPrimitiveFactory.Create(destination, "Hazard Reactor Block", PrimitiveType.Cube,
                new Vector3(6.8f, 0.75f, 3.3f), new Vector3(1.65f, 1.50f, 1.65f),
                Quaternion.Euler(0f, -12f, 0f), material, Rust);
            IndustrialPrimitiveFactory.Create(destination, "Hazard Beacon", PrimitiveType.Cylinder,
                new Vector3(6.8f, 1.70f, 3.3f), new Vector3(0.32f, 0.25f, 0.32f),
                Quaternion.identity, material, Hostile, 0.15f, 0.8f);
        }
    }
}
