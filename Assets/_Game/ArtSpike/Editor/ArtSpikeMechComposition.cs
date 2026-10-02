using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.ArtSpike.Editor
{
    internal sealed partial class ArtSpikeComposition
    {
        public GameObject Player(int tier)
        {
            var root = new GameObject("G0_Tier" + tier + "_ArtSpike");
            var chassis = Group(root.transform, "01_CoreChassis_CommonIdentity");
            Module(chassis, "Chassis", "ArmoredCoreChassis", new Vector3(0, .40f, -.08f),
                new Vector3(.96f, .30f, 1.03f), Vector3.zero);
            Ring(chassis, "CoreSeat", new Vector3(0, .62f, 0), .28f, .045f, Vector3.zero, _secondary);
            Primitive(chassis, PrimitiveType.Sphere, "GravityCore_Common", new Vector3(0, .68f, 0),
                new Vector3(.30f, .30f, .30f), Vector3.zero, _player);

            var locomotion = Group(root.transform, "02_FourMechanicalSupports_Common");
            for (var side = -1; side <= 1; side += 2)
                for (var end = -1; end <= 1; end += 2)
                {
                    var support = Group(locomotion, (side < 0 ? "Left" : "Right") + (end > 0 ? "Front" : "Rear"));
                    Support(support, side, end, 1f);
                }

            var attack = Group(root.transform, "03_ForwardGravityMandibles_Common");
            for (var side = -1; side <= 1; side += 2)
            {
                var pivot = Group(attack, side < 0 ? "LeftWeaponPivot" : "RightWeaponPivot");
                pivot.localPosition = new Vector3(side * .32f, .36f, .52f);
                pivot.localRotation = Quaternion.Euler(0, side * -7, 0);
                Module(pivot, "WeaponHousing", "GravityMandible", new Vector3(0, 0, .10f),
                    new Vector3(.23f, .22f, .60f), Vector3.zero);
                Module(pivot, "EmitterFork", "ForwardEmitter", new Vector3(0, 0, .48f),
                    new Vector3(.20f, .19f, .36f), Vector3.zero);
            }

            if (tier >= 1)
            {
                var upgrade = Group(root.transform, "04_Tier1_ArmorAndStabilizers");
                for (var side = -1; side <= 1; side += 2)
                {
                    Module(upgrade, "FlankPlate", "FlankArmor", new Vector3(side * .72f, .48f, -.12f),
                        new Vector3(.34f, .22f, .95f), new Vector3(0, side * -10, side * 6));
                    Primitive(upgrade, PrimitiveType.Cube, "FlankPowerStrip", new Vector3(side * .71f, .609f, -.12f),
                        new Vector3(.045f, .012f, .33f), new Vector3(0, side * -10, 0), _player);
                }
                Ring(upgrade, "OuterCoreContainment", new Vector3(0, .60f, 0), .39f, .032f, Vector3.zero, _proxyMetal);
            }
            if (tier >= 2)
            {
                var upgrade = Group(root.transform, "05_Tier2_EmitterForksAndContainment");
                for (var side = -1; side <= 1; side += 2)
                {
                    Module(upgrade, "FlankPlate", "HeavyOuterArmor", new Vector3(side * .96f, .41f, -.28f),
                        new Vector3(.32f, .24f, .88f), new Vector3(0, side * 16, side * -8));
                    var pivot = Group(upgrade, side < 0 ? "LeftHeavyWeaponPivot" : "RightHeavyWeaponPivot");
                    pivot.localPosition = new Vector3(side * .70f, .40f, .45f);
                    pivot.localRotation = Quaternion.Euler(0, side * -10, 0);
                    Module(pivot, "WeaponHousing", "ForwardWeaponAssembly", new Vector3(0, 0, .18f),
                        new Vector3(.30f, .29f, .78f), Vector3.zero);
                    Module(pivot, "EmitterFork", "HeavyForwardEmitter", new Vector3(0, 0, .67f),
                        new Vector3(.26f, .25f, .42f), Vector3.zero);
                }
                Ring(upgrade, "UpperCoreContainment", new Vector3(0, .76f, 0), .27f, .028f,
                    new Vector3(0, 0, 16), _proxyArmor);
                for (var side = -1; side <= 1; side += 2)
                    Primitive(upgrade, PrimitiveType.Cylinder, "ContainmentPost", new Vector3(side * .255f, .665f, 0),
                        new Vector3(.07f, .08f, .07f), Vector3.zero, _secondary);
            }
            return root;
        }

        private void Support(Transform parent, int side, int end, float scale)
        {
            var hip = Group(parent, "HipPivot");
            hip.localPosition = new Vector3(side * .35f, .40f, end * .31f) * scale;
            hip.localRotation = Quaternion.Euler(0, side < 0 ? 180 : 0, 0);
            Module(hip, "UpperSupport", "UpperLink", new Vector3(.12f, -.02f, 0) * scale,
                new Vector3(.34f, .14f, .20f) * scale, new Vector3(0, 0, -18));
            var knee = Group(hip, "KneePivot");
            knee.localPosition = new Vector3(.22f, -.12f, 0) * scale;
            Module(knee, "LowerSupport", "PistonShin", new Vector3(.015f, -.09f, 0) * scale,
                new Vector3(.15f, .23f, .19f) * scale, new Vector3(0, 0, 8));
            var foot = Group(knee, "FootPivot");
            foot.localPosition = new Vector3(.03f, -.235f, end * .03f * side) * scale;
            Module(foot, "Foot", "ArmoredFoot", Vector3.zero,
                new Vector3(.30f, .09f, .34f) * scale, Vector3.zero);
        }

        public GameObject Cutter()
        {
            var root = new GameObject("Cutter_ArtSpike");
            var chassis = Group(root.transform, "01_ArmoredBipedChassis");
            Module(chassis, "Chassis", "LowHostileChassis", new Vector3(0, .36f, -.13f),
                new Vector3(.69f, .28f, .73f), Vector3.zero);
            for (var side = -1; side <= 1; side += 2)
            {
                var runner = Group(chassis, side < 0 ? "LeftRunner" : "RightRunner");
                runner.localPosition = new Vector3(0, 0, -.22f);
                Support(runner, side, 1, .80f);
            }
            var cutting = Group(root.transform, "02_ArticulatedCuttingArms");
            for (var side = -1; side <= 1; side += 2)
            {
                var arm = Group(cutting, side < 0 ? "LongShearPivot" : "ShortShearPivot");
                arm.localPosition = new Vector3(side * .37f, .30f, .12f);
                arm.localRotation = Quaternion.Euler(0, side < 0 ? -14 : 24, 0);
                Module(arm, "WeaponHousing", side < 0 ? "LongShearArm" : "ShortShearArm",
                    new Vector3(0, 0, .25f), new Vector3(.20f, .22f, side < 0 ? .79f : .56f), Vector3.zero);
                Module(arm, "ShearBlade", side < 0 ? "LongCuttingBlade" : "ShortCuttingBlade",
                    new Vector3(0, -.02f, side < 0 ? .91f : .69f),
                    new Vector3(.12f, .20f, side < 0 ? .67f : .44f), Vector3.zero);
            }
            Module(cutting, "FlankPlate", "OffsetArmShield", new Vector3(-.39f, .50f, -.06f),
                new Vector3(.32f, .18f, .47f), new Vector3(0, -14, -12));
            var energy = Group(root.transform, "03_HostileEnergy");
            Primitive(energy, PrimitiveType.Cube, "RedSpine", new Vector3(0, .514f, -.13f),
                new Vector3(.10f, .018f, .32f), Vector3.zero, _hostile);
            return root;
        }

        private void Module(Transform parent, string meshName, string name, Vector3 position,
            Vector3 size, Vector3 rotation)
        {
            var path = ArtSpikeProxyMeshes.Folder + "/" + meshName + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) throw new FileNotFoundException("Missing project-authored proxy module", path);
            var module = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            module.transform.SetParent(parent, false);
            module.transform.localPosition = position;
            module.transform.localRotation = Quaternion.Euler(rotation);
            module.transform.localScale = size;
            module.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = module.GetComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { _proxyArmor, _proxyMetal };
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
