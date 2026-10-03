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
            var body = Group(root.transform, "01_RobotBody_CommonIdentity");
            Module(body, "MechCarapace", "Torso", new Vector3(0, 1.10f, 0),
                new Vector3(.78f, .50f, .45f), new Vector3(-8, 0, 0));
            Module(body, "UpperSupport", "Abdomen", new Vector3(0, .82f, 0),
                new Vector3(.26f, .14f, .25f), Vector3.zero);
            Module(body, "MechPelvis", "Pelvis", new Vector3(0, .70f, 0),
                new Vector3(.43f, .17f, .31f), Vector3.zero);
            Primitive(body, PrimitiveType.Cylinder, "NeckJoint", new Vector3(0, 1.34f, -.04f),
                new Vector3(.14f, .035f, .14f), Vector3.zero, _secondary);
            Module(body, "MechForearm", "SensorHead", new Vector3(0, 1.42f, -.03f),
                new Vector3(.28f, .17f, .25f), new Vector3(-8,0,0));
            Primitive(body, PrimitiveType.Cube, "CyanSensorVisor", new Vector3(0, 1.435f, .112f),
                new Vector3(.22f, .035f, .018f), Vector3.zero, _player);
            Primitive(body, PrimitiveType.Sphere, "GravityCore_Common", new Vector3(0, 1.10f, .25f),
                new Vector3(.28f, .28f, .15f), Vector3.zero, _player);
            Ring(body, "ChestCoreSeat", new Vector3(0, 1.10f, .245f), .17f, .025f,
                new Vector3(90, 0, 0), _secondary);
            Module(body, "WeaponHousing", "RearReactor", new Vector3(0, 1.09f, -.27f),
                new Vector3(.32f, .30f, .17f), Vector3.zero);

            var legs = Group(root.transform, "02_TwoMechanicalLegs_Common");
            var arms = Group(root.transform, "03_ArticulatedGravityArms_Common");
            for (var side = -1; side <= 1; side += 2)
            {
                var leg = Group(legs, side < 0 ? "LeftLeg" : "RightLeg");
                var hip = Group(leg, "HipPivot");
                hip.localPosition = new Vector3(side * .18f, .65f, 0);
                Module(hip, "UpperSupport", "ThighLink", new Vector3(side * .01f, -.12f, 0),
                    new Vector3(.20f, .26f, .23f), new Vector3(0, 0, side * -5));
                var knee = Group(hip, "KneePivot");
                knee.localPosition = new Vector3(side * .035f, -.29f, .015f);
                Primitive(knee, PrimitiveType.Cylinder, "KneeJoint", Vector3.zero,
                    new Vector3(.15f, .08f, .15f), new Vector3(0, 0, 90), _secondary);
                Module(knee, "LowerSupport", "PistonShin", new Vector3(0, -.12f, 0),
                    new Vector3(.21f, .25f, .23f), Vector3.zero);
                var foot = Group(knee, "FootPivot");
                foot.localPosition = new Vector3(0, -.28f, .055f);
                Module(foot, "Foot", "ArmoredFoot", Vector3.zero,
                    new Vector3(.30f, .13f, .43f), Vector3.zero);

                var arm = Group(arms, side < 0 ? "LeftWeaponPivot" : "RightWeaponPivot");
                arm.localPosition = new Vector3(side * .46f, 1.23f, 0);
                arm.localRotation = Quaternion.Euler(0, side * -7, 0);
                Primitive(arm, PrimitiveType.Cylinder, "ShoulderJoint", Vector3.zero,
                    new Vector3(.21f, .09f, .21f), new Vector3(0, 0, 90), _secondary);
                Module(arm, "UpperSupport", "UpperArmLink", new Vector3(side * .035f, -.16f, 0),
                    new Vector3(.19f, .28f, .22f), new Vector3(0, 0, side * 10));
                var elbow = Group(arm, "ElbowPivot");
                elbow.localPosition = new Vector3(side * .06f, -.34f, .025f);
                Primitive(elbow, PrimitiveType.Cylinder, "ElbowJoint", Vector3.zero,
                    new Vector3(.15f, .07f, .15f), new Vector3(0, 0, 90), _secondary);
                Module(elbow, "MechForearm", "GravityForearm", new Vector3(0, -.13f, .065f),
                    new Vector3(.28f, .25f, .38f), new Vector3(-12,0,0));
                Module(elbow, "EmitterFork", "PalmEmitter", new Vector3(0, -.17f, .26f),
                    new Vector3(.18f, .17f, .20f), Vector3.zero);
            }

            if (tier >= 1)
            {
                var upgrade = Group(body, "04_Tier1_ShoulderArmor");
                for (var side = -1; side <= 1; side += 2)
                    Module(arms.Find(side < 0 ? "LeftWeaponPivot" : "RightWeaponPivot"), "FlankPlate", "ShoulderPauldron", new Vector3(side * .14f, .04f, -.035f),
                        new Vector3(.30f, .19f, .36f), new Vector3(0, 0, side * -22));
                Ring(upgrade, "OuterCoreContainment", new Vector3(0, 1.10f, .265f), .21f, .018f,
                    new Vector3(90, 0, 0), _proxyMetal);
            }
            if (tier >= 2)
            {
                var upgrade = Group(body, "05_Tier2_CombatFrame");
                for (var side = -1; side <= 1; side += 2)
                {
                    Module(arms.Find(side < 0 ? "LeftWeaponPivot" : "RightWeaponPivot"), "FlankPlate", "HeavyShoulderArmor", new Vector3(side * .32f, .025f, -.06f),
                        new Vector3(.28f, .23f, .37f), new Vector3(0, 0, side * -30));
                    Module(upgrade, "UpperSupport", "RearVectorFin", new Vector3(side * .24f, 1.28f, -.34f),
                        new Vector3(.10f, .43f, .19f), new Vector3(-12, 0, side * -14));
                }
                Ring(upgrade, "UpperCoreContainment", new Vector3(0, 1.10f, .285f), .18f, .018f,
                    new Vector3(90, 0, 18), _proxyArmor);
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
