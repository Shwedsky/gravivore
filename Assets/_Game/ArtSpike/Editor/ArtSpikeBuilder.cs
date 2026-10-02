using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    /// <summary>Reproducible Unity transform-only authoring. No production catalog binding.</summary>
    public static class ArtSpikeBuilder
    {
        public const string Root = "Assets/_Game/ArtSpike";
        public const string ScenePath = Root + "/Scenes/ArtSpike_Comparison.unity";
        public static readonly string[] CharacterPaths =
        {
            Root + "/Prefabs/Player/G0_Tier0_ArtSpike.prefab",
            Root + "/Prefabs/Player/G0_Tier1_ArtSpike.prefab",
            Root + "/Prefabs/Player/G0_Tier2_ArtSpike.prefab",
            Root + "/Prefabs/Enemies/Cutter_ArtSpike.prefab"
        };

        [MenuItem("Gravivore/Art Spike/Rebuild Comparison Assets")]
        public static void Build()
        {
            var composition = new ArtSpikeComposition();
            composition.CreateMaterials();
            for (var tier = 0; tier < 3; tier++)
                Save(composition.Player(tier), CharacterPaths[tier]);
            Save(composition.Cutter(), CharacterPaths[3]);
            Save(composition.Environment(), Root + "/Prefabs/Environment/IndustrialBay_ArtSpike.prefab");
            AssetDatabase.SaveAssets();
            ArtSpikeScene.Create();
            ArtSpikeAudit.WriteSnapshot();
            Debug.Log("ART SPIKE prefab authoring and comparison scene completed.");
        }

        private static void Save(GameObject root, string path)
        {
            try
            {
                if (PrefabUtility.SaveAsPrefabAsset(root, path) == null)
                    throw new InvalidOperationException("Could not save art prefab: " + path);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }

    internal sealed class ArtSpikeComposition
    {
        private Material _metal, _armor, _secondary, _player, _hostile, _industrial, _floor;

        public void CreateMaterials()
        {
            _metal = Material("Gravivore_DarkMetal", new Color(.18f, .215f, .245f), .78f, .42f);
            _armor = Material("Gravivore_Armor", new Color(.34f, .40f, .45f), .68f, .34f);
            _secondary = Material("Gravivore_SecondaryMetal", new Color(.105f, .13f, .16f), .65f, .32f);
            _player = Material("Gravivore_PlayerCore", new Color(.02f, .64f, .82f), .35f, .65f,
                new Color(.03f, .88f, 1f) * 2.5f);
            _hostile = Material("Gravivore_HostileCore", new Color(.68f, .035f, .035f), .35f, .55f,
                new Color(1f, .045f, .025f) * 2f);
            _industrial = Material("Gravivore_IndustrialEnergy", new Color(.54f, .22f, .035f), .4f, .5f,
                new Color(1f, .35f, .035f) * 1.5f);
            _floor = Material("Gravivore_Floor", new Color(.065f, .085f, .105f), .4f, .22f);
        }

        private static Material Material(string name, Color color, float metallic, float smoothness,
            Color emission = default)
        {
            var path = ArtSpikeBuilder.Root + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            else material.DisableKeyword("_EMISSION");
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        public GameObject Player(int tier)
        {
            var root = new GameObject("G0_Tier" + tier + "_ArtSpike");
            var chassis = Group(root.transform, "01_CoreChassis_CommonIdentity");
            // This is one machine casing, not an entire donor robot. Its opening faces upward.
            Donor(chassis, "FactoryKit", "machine-connection-hole", "GravityHousing",
                new Vector3(0, .43f, 0), new Vector3(.72f, .28f, .80f), new Vector3(-90, 0, 0), _metal);
            Donor(chassis, "FactoryKit", "cog-d", "CoreCage_LowerRing",
                new Vector3(0, .61f, 0), new Vector3(.77f, .12f, .77f), Vector3.zero, _armor);
            Primitive(chassis, PrimitiveType.Sphere, "GravityCore_Common", new Vector3(0, .68f, 0),
                new Vector3(.38f, .38f, .38f), Vector3.zero, _player);
            Primitive(chassis, PrimitiveType.Cylinder, "CoreLens", new Vector3(0, .875f, 0),
                new Vector3(.19f, .014f, .19f), Vector3.zero, _secondary);
            Primitive(chassis, PrimitiveType.Cylinder, "LensEnergy", new Vector3(0, .898f, 0),
                new Vector3(.075f, .009f, .075f), Vector3.zero, _player);

            var locomotion = Group(root.transform, "02_FourMechanicalSupports_Common");
            for (var side = -1; side <= 1; side += 2)
            {
                for (var end = -1; end <= 1; end += 2)
                {
                    var support = Group(locomotion, (side < 0 ? "Left" : "Right") + (end > 0 ? "Front" : "Rear"));
                    Donor(support, "FactoryKit", "piston-thin-square", "ArticulatedPiston",
                        new Vector3(side * .40f, .32f, end * .35f), new Vector3(.18f, .55f, .18f),
                        new Vector3(end * 44, 0, -side * 54), _secondary);
                    Donor(support, "FactoryKit", "box-long", "ContactSkid",
                        new Vector3(side * .60f, .15f, end * .57f), new Vector3(.23f, .18f, .52f),
                        new Vector3(0, end * side * 16, 0), _metal);
                    Primitive(support, PrimitiveType.Cube, "JointEnergy", new Vector3(side * .62f, .255f, end * .46f),
                        new Vector3(.09f, .035f, .14f), Vector3.zero, _player);
                }
            }

            var attack = Group(root.transform, "03_ForwardGravityMandibles_Common");
            for (var side = -1; side <= 1; side += 2)
            {
                Donor(attack, "FactoryKit", "cone", "EmitterProng",
                    new Vector3(side * .27f, .46f, .61f), new Vector3(.17f, .16f, .59f),
                    new Vector3(90, side * -12, 0), _armor);
                Primitive(attack, PrimitiveType.Cube, "EmitterSlit", new Vector3(side * .29f, .54f, .61f),
                    new Vector3(.065f, .025f, .28f), new Vector3(0, side * -12, 0), _player);
            }
            Donor(chassis, "FactoryKit", "screen-panel-flat", "RearSpine",
                new Vector3(0, .42f, -.46f), new Vector3(.39f, .10f, .53f), new Vector3(0, 0, 0), _metal);

            if (tier >= 1)
            {
                var upgrade = Group(root.transform, "04_Tier1_ArmorAndStabilizers");
                for (var side = -1; side <= 1; side += 2)
                {
                    Donor(upgrade, "FactoryKit", "screen-panel-flat", "SideArmor",
                        new Vector3(side * .69f, .50f, -.04f), new Vector3(.38f, .17f, .91f),
                        new Vector3(0, side * -16, side * -12), _armor);
                    Donor(upgrade, "FactoryKit", "cone", "ForwardBlade",
                        new Vector3(side * .74f, .41f, .65f), new Vector3(.22f, .13f, .94f),
                        new Vector3(90, side * -14, 0), _metal);
                    Primitive(upgrade, PrimitiveType.Cube, "StabilizerEnergy", new Vector3(side * .79f, .595f, -.10f),
                        new Vector3(.065f, .025f, .39f), new Vector3(0, side * -16, 0), _player);
                }
                Donor(upgrade, "FactoryKit", "cog-a", "CoreCage_OuterRing",
                    new Vector3(0, .57f, 0), new Vector3(1.04f, .09f, 1.04f), Vector3.zero, _metal);
            }

            if (tier >= 2)
            {
                var upgrade = Group(root.transform, "05_Tier2_EmitterForksAndContainment");
                for (var side = -1; side <= 1; side += 2)
                {
                    Donor(upgrade, "FactoryKit", "screen-panel-flat", "OuterArmorWing",
                        new Vector3(side * 1.00f, .53f, -.39f), new Vector3(.38f, .15f, .96f),
                        new Vector3(0, side * 32, side * -18), _metal);
                    Donor(upgrade, "FactoryKit", "piston-square", "ForwardEmitterHousing",
                        new Vector3(side * .55f, .64f, .87f), new Vector3(.27f, .26f, .83f),
                        new Vector3(90, side * -10, 0), _secondary);
                    Primitive(upgrade, PrimitiveType.Cylinder, "EmitterMuzzle", new Vector3(side * .60f, .66f, 1.27f),
                        new Vector3(.19f, .035f, .19f), new Vector3(90, 0, 0), _player);
                    Donor(upgrade, "FactoryKit", "cone", "ExtendedAttackBlade",
                        new Vector3(side * .99f, .41f, .85f), new Vector3(.21f, .14f, 1.13f),
                        new Vector3(90, side * -20, 0), _armor);
                    Donor(upgrade, "FactoryKit", "piston-thin-square", "RearPowerStrut",
                        new Vector3(side * .29f, .51f, -.72f), new Vector3(.16f, .17f, .53f),
                        new Vector3(90, side * 12, 0), _armor);
                    Primitive(upgrade, PrimitiveType.Cylinder, "RearEnergyPort", new Vector3(side * .29f, .54f, -.98f),
                        new Vector3(.13f, .025f, .13f), new Vector3(90, 0, 0), _player);
                }
                Donor(upgrade, "FactoryKit", "cog-b", "CoreCage_UpperContainmentRing",
                    new Vector3(0, .60f, 0), new Vector3(1.12f, .075f, 1.12f), Vector3.zero, _secondary);
                // The core stays exposed through the ring; additional anchors change the cage, not its color.
                for (var side = -1; side <= 1; side += 2)
                    Primitive(upgrade, PrimitiveType.Cylinder, "ContainmentAnchor", new Vector3(side * .37f, .73f, 0),
                        new Vector3(.065f, .10f, .065f), Vector3.zero, _player);
            }
            return root;
        }

        public GameObject Cutter()
        {
            var root = new GameObject("Cutter_ArtSpike");
            var body = Group(root.transform, "01_WedgeBodyAndTwinRunners");
            Donor(body, "FactoryKit", "hopper-square", "ArmoredWedge", new Vector3(0, .43f, -.18f),
                new Vector3(.74f, .56f, .91f), new Vector3(0, 180, 0), _metal);
            Donor(body, "FactoryKit", "screen-panel-flat", "DorsalShield", new Vector3(0, .77f, -.26f),
                new Vector3(.62f, .08f, .72f), new Vector3(0, 0, 12), _armor);
            for (var side = -1; side <= 1; side += 2)
            {
                Donor(body, "FactoryKit", "box-long", "Runner", new Vector3(side * .47f, .18f, -.18f),
                    new Vector3(.30f, .28f, 1.04f), Vector3.zero, _secondary);
                Donor(body, "FactoryKit", "piston-square", "CutterActuator", new Vector3(side * .38f, .47f, .32f),
                    new Vector3(.18f, .17f, .60f), new Vector3(90, side * -22, 0), _metal);
            }
            var weapon = Group(root.transform, "02_OffsetRotaryCutters");
            Donor(weapon, "FactoryKit", "cog-e", "LargeLeftSaw", new Vector3(-.53f, .36f, .68f),
                new Vector3(.83f, .12f, .83f), new Vector3(0, 15, -10), _armor);
            Donor(weapon, "FactoryKit", "cog-e", "SmallRightSaw", new Vector3(.48f, .37f, .59f),
                new Vector3(.60f, .12f, .60f), new Vector3(0, -10, 8), _metal);
            var energy = Group(root.transform, "03_HostileEnergy");
            Primitive(energy, PrimitiveType.Cube, "RectangularHostileCore", new Vector3(0, .83f, -.21f),
                new Vector3(.17f, .055f, .42f), Vector3.zero, _hostile);
            Primitive(energy, PrimitiveType.Cylinder, "LeftSawHub", new Vector3(-.53f, .44f, .68f),
                new Vector3(.19f, .025f, .19f), Vector3.zero, _hostile);
            Primitive(energy, PrimitiveType.Cylinder, "RightSawHub", new Vector3(.48f, .45f, .59f),
                new Vector3(.14f, .025f, .14f), Vector3.zero, _hostile);
            return root;
        }

        public GameObject Environment()
        {
            var root = new GameObject("IndustrialBay_ArtSpike");
            var deck = Group(root.transform, "01_OpenModularDeck");
            for (var x = -2; x <= 2; x++)
                for (var z = -2; z <= 2; z++)
                    Donor(deck, "ModularSpaceKit", "template-floor", "Deck_" + x + "_" + z,
                        new Vector3(x * 2.4f, -.08f, z * 2.4f), new Vector3(2.38f, .12f, 2.38f), Vector3.zero, _floor);

            var framing = Group(root.transform, "02_LowPerimeterFraming");
            for (var x = -2; x <= 2; x++)
                Donor(framing, "ModularSpaceKit", "template-wall-half", "RearBulkhead_" + x,
                    new Vector3(x * 2.4f, .36f, 5.7f), new Vector3(2.38f, .9f, .35f), Vector3.zero, _secondary);
            for (var side = -1; side <= 1; side += 2)
            {
                Donor(framing, "ModularSpaceKit", "template-wall-detail-a", "SideFrame",
                    new Vector3(side * 5.8f, .30f, .6f), new Vector3(.35f, .7f, 4f), new Vector3(0, 90, 0), _metal);
                Donor(framing, "FactoryKit", "pipe-large-long", "PerimeterConduit",
                    new Vector3(side * 5.10f, .20f, -.7f), new Vector3(.35f, .35f, 4.6f), Vector3.zero, _secondary);
                Primitive(framing, PrimitiveType.Cube, "ConduitStatus", new Vector3(side * 5.1f, .42f, -.70f),
                    new Vector3(.09f, .025f, .60f), Vector3.zero, _industrial);
            }

            var installation = Group(root.transform, "03_ReactorInstallation");
            Donor(installation, "FactoryKit", "machine-fortified", "ReactorPlinth",
                new Vector3(-3.45f, .52f, 4.10f), new Vector3(1.9f, 1.0f, 1.6f), Vector3.zero, _metal);
            Donor(installation, "FactoryKit", "cog-d", "ReactorContainmentRing",
                new Vector3(-3.45f, 1.16f, 4.10f), new Vector3(1.45f, .18f, 1.45f), Vector3.zero, _armor);
            Primitive(installation, PrimitiveType.Cylinder, "ReactorEnergyColumn",
                new Vector3(-3.45f, 1.40f, 4.1f), new Vector3(.43f, .29f, .43f), Vector3.zero, _industrial);
            Donor(installation, "FactoryKit", "cog-a", "ReactorTopRing",
                new Vector3(-3.45f, 1.98f, 4.10f), new Vector3(1.17f, .17f, 1.17f), Vector3.zero, _secondary);
            Donor(installation, "FactoryKit", "pipe-large-bend", "ReactorFeed",
                new Vector3(-4.62f, .45f, 3.10f), new Vector3(.95f, .60f, 1.35f), Vector3.zero, _metal);
            Donor(installation, "FactoryKit", "machine-fortified", "AuxiliaryGenerator",
                new Vector3(3.8f, .50f, 4.1f), new Vector3(1.35f, 1.0f, 1.5f), new Vector3(0, -90, 0), _secondary);
            Donor(installation, "FactoryKit", "box-long", "SealedContainer",
                new Vector3(4.3f, .34f, 2.2f), new Vector3(1.45f, .68f, .95f), new Vector3(0, 90, 0), _metal);
            Donor(installation, "ModularSpaceKit", "cables", "CableBundle",
                new Vector3(3.8f, .06f, 3.1f), new Vector3(.8f, .07f, 1.4f), Vector3.zero, _secondary);
            return root;
        }

        public GameObject ReviewStage(string name)
        {
            var root = new GameObject(name);
            Donor(root.transform, "ModularSpaceKit", "template-floor", "ReviewDeck",
                new Vector3(0, -.09f, 0), new Vector3(11.8f, .12f, 7.5f), Vector3.zero, _floor);
            var ruler = Group(root.transform, "OneMetreReference");
            Primitive(ruler, PrimitiveType.Cube, "1mBaseline", new Vector3(0, .01f, -2f),
                new Vector3(1f, .014f, .025f), Vector3.zero, _armor);
            for (var tick = 0; tick <= 5; tick++)
                Primitive(ruler, PrimitiveType.Cube, "20cmTick", new Vector3(-.5f + tick * .2f, .012f, -2f),
                    new Vector3(.02f, .014f, .12f), Vector3.zero, _armor);
            return root;
        }

        internal static Transform Group(Transform parent, string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static void Donor(Transform parent, string pack, string model, string name,
            Vector3 position, Vector3 size, Vector3 rotation, Material material)
        {
            var path = ArtSpikeBuilder.Root + "/Imported/Kenney/" + pack + "/Models/" + model + ".fbx";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new FileNotFoundException("Missing licensed donor", path);
            var wrapper = Group(parent, name + " [" + model + "]");
            // Unpack the instance hierarchy only. Mesh references still point to unchanged vendor FBX.
            var instance = Object.Instantiate(source, wrapper, false);
            instance.name = model;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(rotation);
            instance.transform.localScale = Vector3.one;
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Donor has no renderers: " + path);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.x <= 0 || bounds.size.z <= 0)
                throw new InvalidOperationException("Donor has degenerate bounds: " + path);
            // Bounds were measured while wrapper and all ancestors are identity transforms.
            instance.transform.localPosition -= bounds.center;
            // A planar vendor floor has no thickness. Preserve that plane instead of inventing vertices.
            wrapper.localScale = new Vector3(size.x / bounds.size.x,
                bounds.size.y < .0001f ? 1f : size.y / bounds.size.y, size.z / bounds.size.z);
            wrapper.localPosition = position;
            foreach (var renderer in renderers)
            {
                var slots = new Material[Mathf.Max(1, renderer.sharedMaterials.Length)];
                for (var i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            foreach (var light in instance.GetComponentsInChildren<Light>(true)) Object.DestroyImmediate(light);
            foreach (var camera in instance.GetComponentsInChildren<Camera>(true)) Object.DestroyImmediate(camera);
            foreach (var animator in instance.GetComponentsInChildren<Animator>(true)) Object.DestroyImmediate(animator);
        }

        private static void Primitive(Transform parent, PrimitiveType type, string name, Vector3 position,
            Vector3 scale, Vector3 rotation, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.transform.localRotation = Quaternion.Euler(rotation);
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
