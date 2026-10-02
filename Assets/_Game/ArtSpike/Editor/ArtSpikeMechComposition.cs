using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    internal sealed partial class ArtSpikeComposition
    {
        internal const string MechSource = ArtSpikeBuilder.Root +
            "/Imported/Julius/MechSketch/Models/mechscetch_unfinished.obj";

        public GameObject Player(int tier)
        {
            var root = new GameObject("G0_Tier" + tier + "_ArtSpike");
            var chassis = Group(root.transform, "01_CoreChassis_CommonIdentity");
            MechPart(chassis, "torso_default2", "ArmoredCoreChassis", new Vector3(0, .46f, -.08f),
                new Vector3(.88f, .35f, 1.04f), new Vector3(-90, 0, 0), _metal);
            Ring(chassis, "CoreSeat", new Vector3(0, .62f, 0), .28f, .045f, Vector3.zero, _secondary);
            Primitive(chassis, PrimitiveType.Sphere, "GravityCore_Common", new Vector3(0, .68f, 0),
                new Vector3(.30f, .30f, .30f), Vector3.zero, _player);

            var locomotion = Group(root.transform, "02_FourMechanicalSupports_Common");
            for (var side = -1; side <= 1; side += 2)
                for (var end = -1; end <= 1; end += 2)
                {
                    var support = Group(locomotion, (side < 0 ? "Left" : "Right") + (end > 0 ? "Front" : "Rear"));
                    // Each limb is a single intact vendor mesh: armor, knee, linkage and foot together.
                    MechPart(support, side < 0 ? "leg_l_copy14_default2" : "leg_l_copy10_default2",
                        "ArmoredArticulatedSupport", new Vector3(side * .45f, .285f, end * .31f),
                        new Vector3(.37f, .57f, .60f), new Vector3(0, end < 0 ? 180 : 0, -side * 27), _armor);
                    Primitive(support, PrimitiveType.Cylinder, "HipSocket", new Vector3(side * .34f, .46f, end * .28f),
                        new Vector3(.17f, .065f, .17f), new Vector3(0, 0, 90), _secondary);
                }

            var attack = Group(root.transform, "03_ForwardGravityMandibles_Common");
            for (var side = -1; side <= 1; side += 2)
            {
                MechPart(attack, side < 0 ? "arm_r_copy13_default2" : "arm_r_default2",
                    "GravityMandible", new Vector3(side * .33f, .40f, .64f),
                    new Vector3(.28f, .31f, .80f), new Vector3(-90, side * -7, 0), _metal);
            }

            if (tier >= 1)
            {
                var upgrade = Group(root.transform, "04_Tier1_ArmorAndStabilizers");
                for (var side = -1; side <= 1; side += 2)
                {
                    MechPart(upgrade, side < 0 ? "shoulder_r_copy12_default2" : "shoulder_r_default2",
                        "FlankArmor", new Vector3(side * .59f, .54f, -.10f),
                        new Vector3(.38f, .25f, 1.00f), new Vector3(90, side * -12, side * 8), _armor);
                    Primitive(upgrade, PrimitiveType.Cube, "FlankPowerStrip", new Vector3(side * .60f, .676f, -.09f),
                        new Vector3(.045f, .012f, .36f), new Vector3(0, side * -12, 0), _player);
                }
                Ring(upgrade, "OuterCoreContainment", new Vector3(0, .60f, 0), .39f, .032f, Vector3.zero, _metal);
            }

            if (tier >= 2)
            {
                var upgrade = Group(root.transform, "05_Tier2_EmitterForksAndContainment");
                for (var side = -1; side <= 1; side += 2)
                {
                    MechPart(upgrade, side < 0 ? "shoulder_r_copy12_default2" : "shoulder_r_default2",
                        "HeavyOuterArmor", new Vector3(side * .84f, .47f, -.32f),
                        new Vector3(.32f, .26f, .96f), new Vector3(90, side * 24, side * -12), _metal);
                    MechPart(upgrade, side < 0 ? "arm_r_copy13_default2" : "arm_r_default2",
                        "ForwardWeaponAssembly", new Vector3(side * .68f, .44f, .72f),
                        new Vector3(.34f, .38f, 1.10f), new Vector3(-90, side * -13, 0), _armor);
                }
                Ring(upgrade, "UpperCoreContainment", new Vector3(0, .76f, 0), .27f, .028f,
                    new Vector3(0, 0, 16), _armor);
                for (var side = -1; side <= 1; side += 2)
                    Primitive(upgrade, PrimitiveType.Cylinder, "ContainmentPost", new Vector3(side * .255f, .665f, 0),
                        new Vector3(.07f, .08f, .07f), Vector3.zero, _secondary);
            }
            return root;
        }

        public GameObject Cutter()
        {
            var root = new GameObject("Cutter_ArtSpike");
            var chassis = Group(root.transform, "01_ArmoredBipedChassis");
            MechPart(chassis, "torso_default2", "LowHostileChassis", new Vector3(0, .51f, -.20f),
                new Vector3(.75f, .49f, .95f), new Vector3(-90, 180, 0), _secondary);
            for (var side = -1; side <= 1; side += 2)
                MechPart(chassis, side < 0 ? "leg_l_copy14_default2" : "leg_l_copy10_default2",
                    "ReverseJointRunner", new Vector3(side * .39f, .27f, -.19f),
                    new Vector3(.28f, .54f, .82f), new Vector3(0, 180, side * 18), _metal);

            var cutting = Group(root.transform, "02_ArticulatedCuttingArms");
            MechPart(cutting, "arm_r_copy13_default2", "LongShearArm", new Vector3(-.50f, .45f, .52f),
                new Vector3(.19f, .42f, 1.31f), new Vector3(15, -15, 0), _armor);
            MechPart(cutting, "arm_r_default2", "ShortShearArm", new Vector3(.49f, .43f, .43f),
                new Vector3(.24f, .34f, .96f), new Vector3(15, 21, 0), _metal);
            MechPart(cutting, "shoulder_r_copy12_default2", "OffsetArmShield", new Vector3(-.42f, .68f, -.05f),
                new Vector3(.39f, .25f, .65f), new Vector3(90, -15, -12), _armor);
            MechPart(cutting, "pelvis_default2", "LongCuttingBlade", new Vector3(-.67f, .36f, 1.01f),
                new Vector3(.10f, .24f, 1.00f), new Vector3(-90, -15, 0), _armor);
            MechPart(cutting, "pelvis_default2", "ShortCuttingBlade", new Vector3(.64f, .32f, .84f),
                new Vector3(.12f, .18f, .67f), new Vector3(-90, 21, 0), _armor);

            var energy = Group(root.transform, "03_HostileEnergy");
            Primitive(energy, PrimitiveType.Cube, "RedSpine", new Vector3(0, .775f, -.27f),
                new Vector3(.10f, .018f, .32f), Vector3.zero, _hostile);
            return root;
        }

        private static void MechPart(Transform parent, string meshName, string name,
            Vector3 position, Vector3 size, Vector3 rotation, Material material)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(MechSource);
            if (model == null) throw new FileNotFoundException("Missing licensed mech donor", MechSource);
            var child = model.GetComponentsInChildren<MeshFilter>(true).Single(f => f.name == meshName);
            var wrapper = Group(parent, name + " [Julius/" + meshName + "]");
            // Select one existing OBJ child. No splitting, merging, vertex editing or mesh copies.
            var part = Object.Instantiate(child.gameObject, wrapper, false);
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.Euler(rotation);
            part.transform.localScale = Vector3.one;
            var renderer = part.GetComponent<MeshRenderer>();
            var bounds = renderer.bounds;
            part.transform.localPosition -= bounds.center;
            wrapper.localScale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / bounds.size.z);
            wrapper.localPosition = position;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        private static void Ring(Transform parent, string name, Vector3 position, float radius,
            float tube, Vector3 rotation, Material material)
        {
            // Ring is the explicitly allowed small project-owned primitive augmentation.
            var path = ArtSpikeBuilder.Root + "/Materials/CoreRing.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                const int segments = 32, sides = 6;
                var vertices = new Vector3[segments * sides];
                var normals = new Vector3[vertices.Length];
                var indices = new int[segments * sides * 6];
                for (var s = 0; s < segments; s++)
                    for (var t = 0; t < sides; t++)
                    {
                        var angle = s * Mathf.PI * 2 / segments;
                        var cross = t * Mathf.PI * 2 / sides;
                        var outward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                        var normal = outward * Mathf.Cos(cross) + Vector3.up * Mathf.Sin(cross);
                        var i = s * sides + t;
                        vertices[i] = outward + normal * .1f;
                        normals[i] = normal;
                        var next = ((s + 1) % segments) * sides + t;
                        var right = s * sides + (t + 1) % sides;
                        var diagonal = ((s + 1) % segments) * sides + (t + 1) % sides;
                        var k = i * 6;
                        indices[k] = i; indices[k + 1] = right; indices[k + 2] = next;
                        indices[k + 3] = right; indices[k + 4] = diagonal; indices[k + 5] = next;
                    }
                mesh = new Mesh { name = "ProjectOwnedCoreRing", vertices = vertices, normals = normals, triangles = indices };
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, path);
            }
            var ring = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = position;
            ring.transform.localRotation = Quaternion.Euler(rotation);
            // The small common torus mesh stays shared; thickness follows radius except vertical containment.
            ring.transform.localScale = new Vector3(radius, tube / .1f, radius);
            ring.GetComponent<MeshFilter>().sharedMesh = mesh;
            ring.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
