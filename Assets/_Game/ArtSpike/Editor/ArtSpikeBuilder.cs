using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    /// <summary>Reproducible isolated proxy authoring. No production catalog binding.</summary>
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
            ArtSpikeProxyMeshes.Build();
            ArtSpikeProxyMeshes.BuildPlayerRefinement();
            ArtSpikePbr.Build();
            var composition = new ArtSpikeComposition();
            composition.CreateMaterials();
            for (var tier = 0; tier < 3; tier++)
                Save(composition.Player(tier), CharacterPaths[tier]);
            Save(composition.Cutter(), CharacterPaths[3]);
            ArtSpikeArticulation.BuildAndProve();
            Save(composition.Environment(), Root + "/Prefabs/Environment/IndustrialBay_ArtSpike.prefab");
            AssetDatabase.SaveAssets();
            ArtSpikeScene.Create();
            ArtSpikeAudit.WriteSnapshot();
            Debug.Log("ART SPIKE prefab authoring and comparison scene completed.");
        }

        [MenuItem("Gravivore/Art Spike/Rebuild Player Mecha Only")]
        public static void RebuildPlayerMecha()
        {
            ArtSpikeProxyMeshes.BuildPlayerRefinement();
            var composition = new ArtSpikeComposition();
            composition.UseExistingPlayerMaterials();
            for (var tier = 0; tier < 3; tier++)
                Save(composition.Player(tier), CharacterPaths[tier]);
            ArtSpikeArticulation.BuildAndProve();
            ArtRuntimePreview.Bind();
            AssetDatabase.SaveAssets();
            Debug.Log("G-0 biped mecha rebuilt; current whole-form runtime bindings retained.");
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

    internal sealed partial class ArtSpikeComposition
    {
        private Material _metal, _armor, _secondary, _player, _hostile, _industrial, _floor, _proxyArmor, _proxyMetal;

        public void UseExistingPlayerMaterials()
        {
            _secondary = ExistingMaterial("Gravivore_SecondaryMetal");
            _player = ExistingMaterial("Gravivore_PlayerCore");
            _proxyArmor = ExistingMaterial("Gravivore_ProxyArmor_PBR");
            _proxyMetal = ExistingMaterial("Gravivore_ProxyMetal_PBR");
        }

        private static Material ExistingMaterial(string name)
        {
            var path = ArtSpikeBuilder.Root + "/Materials/" + name + ".mat";
            return AssetDatabase.LoadAssetAtPath<Material>(path) ??
                throw new FileNotFoundException("Existing player material is required.", path);
        }

        public void CreateMaterials()
        {
            _metal = Material("Gravivore_DarkMetal", new Color(.18f, .215f, .245f), .55f, .24f);
            _armor = Material("Gravivore_Armor", new Color(.34f, .40f, .45f), .40f, .28f);
            _secondary = Material("Gravivore_SecondaryMetal", new Color(.105f, .13f, .16f), .65f, .32f);
            _player = Material("Gravivore_PlayerCore", new Color(.02f, .64f, .82f), .35f, .65f,
                new Color(.03f, .88f, 1f) * 2.5f);
            _hostile = Material("Gravivore_HostileCore", new Color(.68f, .035f, .035f), .35f, .55f,
                new Color(1f, .045f, .025f) * 2f);
            _industrial = Material("Gravivore_IndustrialEnergy", new Color(.54f, .22f, .035f), .4f, .5f,
                new Color(1f, .35f, .035f) * 1.5f);
            _floor = Material("Gravivore_Floor", new Color(.065f, .085f, .105f), .4f, .22f);
            // Counter the blue painted source in linear light, retaining its wear and roughness variation.
            _proxyArmor = Material("Gravivore_ProxyArmor_PBR", new Color(1f, .79f, .68f), .5f, .85f);
            _proxyMetal = Material("Gravivore_ProxyMetal_PBR", new Color(.44f, .30f, .29f), .7f, .85f);
            ArtSpikePbr.Apply(_proxyArmor);
            ArtSpikePbr.Apply(_proxyMetal);
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

        public GameObject Environment()
        {
            var root = new GameObject("IndustrialBay_ArtSpike");
            var deck = Group(root.transform, "01_OpenModularDeck");
            for (var x = -2; x <= 2; x++)
                for (var z = -2; z <= 2; z++)
                    Donor(deck, "ModularSpaceKit", "template-floor", "Deck_" + x + "_" + z,
                        new Vector3(x * 2.4f, 0, z * 2.4f), new Vector3(2.38f, .12f, 2.38f), Vector3.zero, _floor);
            for (var x = -1; x <= 1; x++)
                for (var z = -1; z <= 1; z++)
                    Donor(deck, "ModularSpaceKit", "template-floor-detail-a", "InsetServicePanel_" + x + "_" + z,
                        new Vector3(x * 3.6f, .003f, z * 3.6f), new Vector3(1.8f, .015f, 1.8f), Vector3.zero, _secondary);

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
                new Vector3(-3.45f, .38f, 4.10f), new Vector3(1.3f, .76f, 1.25f), Vector3.zero, _metal);
            Ring(installation, "ReactorContainmentRing", new Vector3(-3.45f, .79f, 4.10f),
                .44f, .04f, Vector3.zero, _metal);
            Primitive(installation, PrimitiveType.Cylinder, "ReactorEnergyColumn",
                new Vector3(-3.45f, .92f, 4.1f), new Vector3(.30f, .12f, .30f), Vector3.zero, _industrial);
            Ring(installation, "ReactorTopRing", new Vector3(-3.45f, 1.15f, 4.10f),
                .36f, .035f, Vector3.zero, _secondary);
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
                new Vector3(0, 0, 0), new Vector3(20f, .12f, 20f), Vector3.zero, _floor);
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
