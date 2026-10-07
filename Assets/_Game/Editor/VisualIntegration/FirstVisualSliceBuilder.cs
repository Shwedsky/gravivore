using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Editor.VisualIntegration
{
    public static class FirstVisualSliceBuilder
    {
        public const string Root = "Assets/_Game/Content/VisualSlice";
        public const string ScenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        // Device-reviewed visual sizes relative to APK v1; gameplay bodies are unchanged.
        public static readonly float[] G0TierVisualMultipliers = { 1.15f, 1.22f, 1.30f };
        private static readonly string[] States = { "Idle", "Run", "Attack", "Hit", "Death" };
        private static readonly List<(string name, Vector3 center, Vector3 size)> Obstacles = new List<(string, Vector3, Vector3)>();

        [MenuItem("Gravivore/Visual Slice/Build production integration")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Prefabs"); Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh();
            var material = AtlasMaterial();
            foreach (var name in new[] { "Scout_V1", "Cutter_V1", "Magnetar_V1" }) MakeActor(name, material);
            foreach (var name in new[] { "Deck_Module", "Bulkhead_Module", "Hero_Reactor", "Power_Bank", "Freight_Container", "Coolant_Pump",
                "Conduit_Rack", "Maintenance_Station", "Structural_Support", "Barrier_Module", "Containment_Gate", "Deck_ServiceMarkings" }) MakeStatic(name, material);
            IntegrateG0();
            var correction = MusicAndMix();
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<S15VisualCatalog>("Assets/_Game/Content/Definitions/S15_VisualCatalog.asset"));
            Recipe(catalog.FindProperty("_enemies").GetArrayElementAtIndex(0), "Scout_V1");
            Recipe(catalog.FindProperty("_enemies").GetArrayElementAtIndex(1), "Cutter_V1");
            catalog.ApplyModifiedPropertiesWithoutUndo();
            var definition = new SerializedObject(AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset"));
            Bind(definition.FindProperty("_elite"), Prefab("Magnetar_V1"), Vector3.one);
            definition.ApplyModifiedPropertiesWithoutUndo();
            MakeEnvironment();
            MakeBossApproachBoundary();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var composition = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env = composition.VisualEnvironment;
            var rootSettings = new SerializedObject(composition);
            rootSettings.FindProperty("_deviceCorrection").objectReferenceValue = correction;
            rootSettings.ApplyModifiedPropertiesWithoutUndo();
            var prior = env.Floor.Find("First Visual Slice Industrial Containment");
            if (prior != null) UnityEngine.Object.DestroyImmediate(prior.gameObject);
            var dressing = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("Environment_Slice")), env.Floor);
            dressing.name = "First Visual Slice Industrial Containment";
            // The former corridor decks are thicker than the new floor finish.
            // Retire only panels inside this rebuilt section so they cannot cover it.
            var routes = env.Floor.Find("Phase3C Routes");
            if (routes != null)
                foreach (var panel in routes.Cast<Transform>().ToArray())
                    if (panel.localPosition.z >= 36f && panel.localPosition.z <= 76f)
                        UnityEngine.Object.DestroyImmediate(panel.gameObject);
            var serialized = new SerializedObject(env);
            serialized.FindProperty("_sliceGate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("Containment_Gate"));
            // Retire the three previous local dressing recipes; everything else retains its existing authoring.
            var entries = serialized.FindProperty("_dressing");
            for (var i = entries.arraySize - 1; i >= 0; i--)
            {
                var anchor = (Transform)entries.GetArrayElementAtIndex(i).FindPropertyRelative("_anchor").objectReferenceValue;
                if (anchor != null && new[] { "cutting-floor", "elite-approach", "elite-arena" }.Contains(anchor.parent.name))
                    entries.DeleteArrayElementAtIndex(i);
                else if (anchor != null && anchor.parent.name == "boss-approach")
                    Bind(entries.GetArrayElementAtIndex(i).FindPropertyRelative("_model"), Prefab("BossApproach_SliceBoundary"), Vector3.one);
            }
            var blockers = serialized.FindProperty("_sliceObstacles"); blockers.arraySize = Obstacles.Count;
            for (var i = 0; i < Obstacles.Count; i++)
            {
                var p = blockers.GetArrayElementAtIndex(i); p.FindPropertyRelative("_name").stringValue = Obstacles[i].name;
                p.FindPropertyRelative("_center").vector3Value = Obstacles[i].center; p.FindPropertyRelative("_size").vector3Value = Obstacles[i].size;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.34f, .41f, .49f);
            var probe = new SphericalHarmonicsL2(); probe.AddAmbientLight(RenderSettings.ambientLight); RenderSettings.ambientProbe = probe;
            env.KeyLight.color = new Color(.91f, .94f, 1f); env.KeyLight.intensity = 1.65f;
            env.KeyLight.transform.rotation = Quaternion.Euler(48, -32, 0);
            env.KeyLight.shadows = LightShadows.Soft; env.KeyLight.shadowStrength = .72f;
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            FirstVisualSliceDependencies.ValidateOrThrow();
            Debug.Log("FIRST_VISUAL_SLICE_INTEGRATED: G0 V3 / Scout V1 / Cutter V1 / Magnetar V1 / Chapter01 containment area");
        }
        public static string Prefab(string name) => Root + "/Prefabs/" + name + ".prefab";
        private static Gravivore.Presentation.AudioVfx.DeviceCorrectionDefinition MusicAndMix()
        {
            foreach (var name in new[] { "Containment_Exploration", "Containment_CombatLayer" })
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(Root + "/Audio/" + name + ".wav");
                var settings = importer.defaultSampleSettings; settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .65f;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings; importer.forceToMono = false; importer.SaveAndReimport();
            }
            var path = Root + "/DeviceCorrection.asset";
            var asset = AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.AudioVfx.DeviceCorrectionDefinition>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<Gravivore.Presentation.AudioVfx.DeviceCorrectionDefinition>(); AssetDatabase.CreateAsset(asset,path); }
            var data = new SerializedObject(asset);
            data.FindProperty("_exploration").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Containment_Exploration.wav");
            data.FindProperty("_combatLayer").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Containment_CombatLayer.wav");
            var materialPath = Root + "/Materials/DeviceMotionStreak.mat";
            var streak = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (streak == null)
            {
                streak = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Content/Presentation/Phase6B/VFX/Materials/M_Phase6B_PlayerStreak.mat"));
                AssetDatabase.CreateAsset(streak,materialPath);
            }
            streak.SetColor("_BaseColor",Color.white); streak.SetColor("_Color",Color.white);
            EditorUtility.SetDirty(streak); data.FindProperty("_motionStreakMaterial").objectReferenceValue = streak;
            data.ApplyModifiedPropertiesWithoutUndo(); asset.ValidateOrThrow();
            var bank = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/_Game/Content/Presentation/Phase6B/Audio/Phase6B_AudioBank.asset"));
            var entries = bank.FindProperty("_entries");
            var volumes = new[] { .30f,.28f,.48f,.60f,.56f,.39f,.48f,.51f,.63f,.72f,.78f,.82f,.80f,.42f,.60f,.64f };
            for (var i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i); var cue = entry.FindPropertyRelative("_cue").enumValueIndex;
                if (cue < volumes.Length) entry.FindPropertyRelative("_volume").floatValue = volumes[cue];
            }
            bank.ApplyModifiedPropertiesWithoutUndo(); return asset;
        }
        private static void MakeBossApproachBoundary()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Phase3D/Prefabs/Environment/BossApproach_Phase3D.prefab");
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(source);
            var deck = obj.transform.Find("Deck");
            var halfLength = deck.GetComponent<Renderer>().bounds.extents.z;
            var scale = deck.localScale; scale.z *= .5f; deck.localScale = scale;
            deck.localPosition += Vector3.forward * halfLength * .5f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(deck);
            PrefabUtility.SaveAsPrefabAsset(obj, Prefab("BossApproach_SliceBoundary"));
            UnityEngine.Object.DestroyImmediate(obj);
        }
        private static Material AtlasMaterial()
        {
            var path = Root + "/Materials/Slice_IndustrialAtlas.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            foreach (var name in new[] { "BaseColor", "Emission", "MetallicSmoothness" })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(Root + "/Textures/Slice_" + name + ".png");
                importer.sRGBTexture = name == "BaseColor"; importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 256; importer.SaveAndReimport();
            }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/Slice_BaseColor.png"));
            mat.SetTexture("_EmissionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/Slice_Emission.png"));
            mat.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/Slice_MetallicSmoothness.png"));
            mat.SetColor("_BaseColor", Color.white); mat.SetColor("_EmissionColor", Color.white * 1.15f);
            mat.SetFloat("_Metallic", .55f); mat.SetFloat("_Smoothness", .32f); mat.SetFloat("_SmoothnessTextureChannel", 0);
            mat.EnableKeyword("_EMISSION"); mat.EnableKeyword("_METALLICSPECGLOSSMAP"); mat.enableInstancing = true;
            // URP material validation uses AnyEmissive to retain the emission keyword.
            // BakedEmissive does not add a realtime light to this unbaked slice.
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            EditorUtility.SetDirty(mat); return mat;
        }
        private static void Import(string name, bool animated)
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Root + "/Models/" + name + ".fbx");
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.animationType = animated ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
            importer.importAnimation = animated; importer.importCameras = false; importer.importLights = false;
            importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.isReadable = false; importer.meshCompression = ModelImporterMeshCompression.Off;
            if (animated)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; importer.motionNodeName = "ROOT";
                importer.animationCompression = ModelImporterAnimationCompression.Off;
                importer.optimizeGameObjects = false; importer.skinWeights = ModelImporterSkinWeights.Custom; importer.maxBonesPerVertex = 1;
                importer.SaveAndReimport();
                var clips = importer.defaultClipAnimations;
                foreach (var clip in clips)
                {
                    clip.name = clip.name.Split('|').Last(); clip.loopTime = clip.name == "Idle" || clip.name == "Run";
                    clip.lockRootPositionXZ = true; clip.lockRootHeightY = true; clip.lockRootRotation = true;
                    clip.keepOriginalPositionXZ = true; clip.keepOriginalPositionY = true; clip.keepOriginalOrientation = true;
                }
                importer.clipAnimations = clips;
            }
            importer.SaveAndReimport();
        }
        private static void MakeActor(string name, Material material)
        {
            Import(name, true);
            var path = Root + "/Models/" + name + ".fbx";
            var controllerPath = Root + "/" + name + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) ?? AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
            var machine = controller.layers[0].stateMachine;
            foreach (var state in machine.states) machine.RemoveState(state.state);
            var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
            foreach (var nameState in States)
            {
                var state = machine.AddState(nameState); state.motion = clips.Single(c => c.name == nameState); state.writeDefaultValues = false;
                if (nameState == "Idle") machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
            var obj = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path)); obj.name = name;
            var animator = obj.GetComponent<Animator>();
            if (animator == null) animator = obj.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            var renderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r => r.name).ToArray();
            foreach (var r in renderers) { r.sharedMaterials = new[] { material }; r.updateWhenOffscreen = false; }
            var lod = obj.GetComponent<LODGroup>();
            if (lod == null) lod = obj.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(.12f, new Renderer[] { renderers[0] }), new LOD(.055f, new Renderer[] { renderers[1] }), new LOD(.015f, new Renderer[] { renderers[2] }) });
            lod.RecalculateBounds(); Sockets(obj.transform, name == "Magnetar_V1" ? 1.15f : .63f);
            PrefabUtility.SaveAsPrefabAsset(obj, Prefab(name)); UnityEngine.Object.DestroyImmediate(obj);
        }
        private static void MakeStatic(string name, Material material)
        {
            Import(name, false);
            // Keep FBX's axis conversion on a child. Authored placement rotates the identity wrapper.
            var obj = new GameObject(name);
            UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/" + name + ".fbx"),obj.transform,false);
            foreach (var r in obj.GetComponentsInChildren<Renderer>()) r.sharedMaterials = new[] { material };
            PrefabUtility.SaveAsPrefabAsset(obj, Prefab(name)); UnityEngine.Object.DestroyImmediate(obj);
        }
        private static void IntegrateG0()
        {
            var definition = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/_Game/Content/Definitions/S07_Evolution.asset"));
            for (var tier = 0; tier < 3; tier++)
            {
                var wrapper = new GameObject("G0_V3_Live_Tier" + tier);
                var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(G0ProductionV3Review.Prefab), wrapper.transform);
                model.transform.localScale = Vector3.one * G0ProductionV3Review.PresentationFit * G0TierVisualMultipliers[tier];
                // Source -Y imports as Unity -Z (verified chest-center probe in V2.1).
                // Keep the form and sockets +Z; correct only the imported presentation child.
                model.transform.localRotation = Quaternion.Euler(0, 180, 0);
                var forward = new GameObject("Visible Front (-Z imported)").transform;
                forward.SetParent(model.transform, false);
                forward.localPosition = new Vector3(0, 2.8f, -.30f);
                forward.localRotation = Quaternion.Euler(0, 180, 0);
                model.GetComponent<Animator>().applyRootMotion = false;
                model.GetComponent<Animator>().cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>()) r.updateWhenOffscreen = false;
                if (tier > 0) AddEvolutionArmor(model, tier);
                // Live material copies leave the approved isolated source untouched.
                {
                    var source = model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
                    var path = Root + "/Materials/G0_V3_Tier" + tier + ".mat";
                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, path); }
                    material.SetColor("_EmissionColor", source.GetColor("_EmissionColor") * (1 + tier * .12f));
                    material.EnableKeyword("_EMISSION"); material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                    EditorUtility.SetDirty(material);
                    foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                        if (r.name.StartsWith("G0_LOD", StringComparison.Ordinal)) r.sharedMaterial = material;
                }
                Sockets(wrapper.transform, 1.12f * G0TierVisualMultipliers[tier]);
                var name = "G0_V3_Live_Tier" + tier;
                PrefabUtility.SaveAsPrefabAsset(wrapper, Prefab(name)); UnityEngine.Object.DestroyImmediate(wrapper);
                Bind(definition.FindProperty("_tierOverrides").GetArrayElementAtIndex(tier), Prefab(name), Vector3.one);
            }
            definition.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void AddEvolutionArmor(GameObject model, int tier)
        {
            var path = Root + "/Models/G0_Tier" + tier + "Armor.fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.animationType = ModelImporterAnimationType.Generic; importer.importAnimation = false;
            importer.importCameras = false; importer.importLights = false;
            importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.optimizeGameObjects = false; importer.maxBonesPerVertex = 1;
            importer.SaveAndReimport();
            var originalBones = model.GetComponentsInChildren<SkinnedMeshRenderer>()[0].bones.ToDictionary(b => b.name);
            var armor = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), model.transform, false);
            armor.name = "Tier " + tier + " Articulated Production Armor";
            // The base mech's three LOD levels also own the armor renderers.
            // FBX auto-generated LOD groups must not register those renderers again.
            foreach (var importedLod in armor.GetComponentsInChildren<LODGroup>(true))
                UnityEngine.Object.DestroyImmediate(importedLod);
            var skins = armor.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(s => s.name).ToArray();
            foreach (var skin in skins)
            {
                skin.bones = skin.bones.Select(b => originalBones[b.name]).ToArray();
                skin.rootBone = originalBones["ROOT"];
                skin.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/Slice_IndustrialAtlas.mat");
                skin.updateWhenOffscreen = false;
            }
            // Imported duplicate skeleton is unnecessary after remapping all rigid weights.
            foreach (Transform child in armor.transform.Cast<Transform>().ToArray())
                if (child.GetComponentsInChildren<SkinnedMeshRenderer>().Length == 0)
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            var lod = model.GetComponent<LODGroup>(); var levels = lod.GetLODs();
            for (var i = 0; i < levels.Length; i++) levels[i].renderers = levels[i].renderers.Concat(new Renderer[] { skins[i] }).ToArray();
            lod.SetLODs(levels); lod.RecalculateBounds();
        }
        private static void Sockets(Transform root, float height)
        {
            var container = new GameObject("Presentation Sockets").transform; container.SetParent(root, false);
            foreach (var role in new[] { "Core", "Sensor", "AttackOrigin", "HitCenter", "GroundContact", "TelegraphOrigin" })
            {
                var socket = new GameObject(role).transform; socket.SetParent(container, false);
                socket.localPosition = role == "GroundContact" || role == "TelegraphOrigin" ? Vector3.zero :
                    new Vector3(0, height, role == "AttackOrigin" ? .38f : role == "Core" ? .26f : 0);
            }
        }
        private static void Recipe(SerializedProperty recipe, string name)
        {
            recipe.FindPropertyRelative("_presentationPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));
            recipe.FindPropertyRelative("_localScale").vector3Value = Vector3.one;
            recipe.FindPropertyRelative("_localPosition").vector3Value = Vector3.zero;
        }
        private static void Bind(SerializedProperty binding, string path, Vector3 scale)
        {
            binding.FindPropertyRelative("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            binding.FindPropertyRelative("_localPosition").vector3Value = Vector3.zero;
            binding.FindPropertyRelative("_localEulerAngles").vector3Value = Vector3.zero;
            binding.FindPropertyRelative("_localScale").vector3Value = scale;
        }
        private static GameObject Place(Transform root, string kit, Vector3 position, float yaw = 0, Vector3? scale = null)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(kit)), root);
            obj.transform.localPosition = position; obj.transform.localRotation = Quaternion.Euler(0, yaw, 0); obj.transform.localScale = scale ?? Vector3.one;
            foreach (var node in obj.GetComponentsInChildren<Transform>()) GameObjectUtility.SetStaticEditorFlags(node.gameObject, StaticEditorFlags.BatchingStatic);
            return obj;
        }
        private static void Obstacle(string name, Vector3 center, Vector3 size) => Obstacles.Add(("Slice " + name, center, size));
        private static void MakeEnvironment()
        {
            Obstacles.Clear(); var root = new GameObject("First Visual Slice Industrial Containment");
            // 16x24m main containment zone plus an 8x20m existing Cutting Floor approach.
            for (var x = -6; x <= 6; x += 4) for (var z = 54; z <= 74; z += 4) Place(root.transform, "Deck_Module", new Vector3(x, .01f, z));
            for (var x = -2; x <= 2; x += 4) for (var z = 38; z <= 50; z += 4) Place(root.transform, "Deck_Module", new Vector3(x, .01f, z));
            // Existing strong-elite-a is at (-18,55): dress its service apron rather than relocating gameplay.
            foreach (var x in new[] { -22, -18 }) foreach (var z in new[] { 50, 54, 58 })
                Place(root.transform, "Deck_Module", new Vector3(x, .01f, z));
            foreach (var x in new[] { -14, -10 }) Place(root.transform, "Deck_Module", new Vector3(x, .01f, 54));
            // Opaque painted service strips break up the broad floor without adding props or collision.
            foreach (var z in new[] { 40,48,56,68 }) Place(root.transform,"Deck_ServiceMarkings",new Vector3(0,.074f,z));
            for (var z = 54; z <= 58; z += 4) Place(root.transform, "Bulkhead_Module", new Vector3(-24, 0, z), -90);
            Obstacle("Strong service apron wall", new Vector3(-24, 1.3f, 56), new Vector3(.5f, 2.6f, 8));
            Place(root.transform, "Coolant_Pump", new Vector3(-22.3f, 0, 54));
            Obstacle("Strong service pump", new Vector3(-22.3f, .9f, 54), new Vector3(1.4f, 1.8f, 1.8f));
            Place(root.transform, "Maintenance_Station", new Vector3(-22.3f, 0, 57));
            Obstacle("Strong service maintenance", new Vector3(-22.3f, 1, 57), new Vector3(1.4f, 2, 1.5f));
            foreach (var side in new[] { -1, 1 })
            {
                for (var z = side < 0 ? 58 : 54; z <= 74; z += 4) Place(root.transform, "Bulkhead_Module", new Vector3(side * 8f, 0, z), side * 90);
                Obstacle("Containment bulkhead " + side, new Vector3(side * 8f, 1.3f, side < 0 ? 66 : 64), new Vector3(.5f, 2.6f, side < 0 ? 20 : 24));
                for (var z = 40; z <= 48; z += 4)
                {
                    Place(root.transform, "Barrier_Module", new Vector3(side * 5f, 0, z), 90);
                    Obstacle("Approach barrier " + side + " " + z, new Vector3(side * 5f, .6f, z), new Vector3(.74f, 1.3f, 3.8f));
                }
                Place(root.transform, "Power_Bank", new Vector3(side * 4.6f, 0, 69 + (side < 0 ? .35f : -.2f)));
                // Power banks occupy the two existing authority containment proxies.
                Place(root.transform, "Structural_Support", new Vector3(side * 2.96f, 0, 60), 0, new Vector3(1, 1.2f, 1));
                Place(root.transform, "Bulkhead_Module", new Vector3(side * 4.65f, 0, 60), 0, new Vector3(.9f, 1, 1));
                for (var z = side < 0 ? 64 : 56; z <= 72; z += 8)
                {
                    Place(root.transform, "Conduit_Rack", new Vector3(side * 7.2f, 0, z), 90);
                    Obstacle("Conduit rack " + side + " " + z, new Vector3(side * 7.2f, .85f, z), new Vector3(.6f, 1.8f, 3.8f));
                }
            }
            Place(root.transform, "Hero_Reactor", new Vector3(-5.3f, 0, 73));
            Obstacle("Hero reactor", new Vector3(-5.3f, 1.7f, 73), new Vector3(2.5f, 3.5f, 2.5f));
            Place(root.transform, "Coolant_Pump", new Vector3(-6.1f, 0, 65));
            Obstacle("Coolant pump", new Vector3(-6.1f, .85f, 65), new Vector3(1.3f, 1.8f, 1.8f));
            Place(root.transform, "Maintenance_Station", new Vector3(6.1f, 0, 65), -30);
            Obstacle("Maintenance station", new Vector3(6.1f, 1, 65), new Vector3(1.5f, 2, 1.5f));
            foreach (var z in new[] { 55, 74 })
            {
                Place(root.transform, "Freight_Container", new Vector3(6, 0, z), 15);
                Place(root.transform, "Freight_Container", new Vector3(6, 1.25f, z), -8);
                Obstacle("Freight stack " + z, new Vector3(6, 1.25f, z), new Vector3(1.9f, 2.5f, 2));
            }
            // Raised gate service header clears the unchanged player and movement controller.
            Place(root.transform, "Conduit_Rack", new Vector3(0, 2.55f, 60), 0, new Vector3(1.5f, 1, .35f));
            PrefabUtility.SaveAsPrefabAsset(root, Prefab("Environment_Slice")); UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
