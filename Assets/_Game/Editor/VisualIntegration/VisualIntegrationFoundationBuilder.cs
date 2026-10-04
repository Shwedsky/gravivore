using System;
using System.IO;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using CameraSettings = Gravivore.Presentation.Camera.CameraFollowSettings;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Creates missing foundation assets only. Never overwrites an artist's authored layer.</summary>
    public static class VisualIntegrationFoundationBuilder
    {
        public const string ChapterPath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        public const string DefinitionPath = "Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset";
        public const string HubPath = "Assets/_Game/Content/Prefabs/Integration/RepairHub_Anchors.prefab";
        public const string ReviewPath = "Assets/_Game/ArtReview/Scenes/VisualIntegration_Review.unity";
        public const string CameraPath = "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset";
        public static readonly string[] HubAnchors =
        {
            "MainPlatform", "ManipulatorLeft", "ManipulatorRight", "RearManipulatorA",
            "RearManipulatorB", "RepairBeamOriginLeft", "RepairBeamOriginRight",
            "RepairBeamOriginRearA", "RepairBeamOriginRearB", "PlayerDockPoint", "AmbientFxRoot"
        };

        [MenuItem("Gravivore/Art Intake/Create Missing Integration Foundation")]
        public static void Build()
        {
            var definition = AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ChapterVisualIntegrationDefinition>();
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HubPath) == null) CreateHubPrefab();
            var previousActive = SceneManager.GetActiveScene();
            var scene = SceneManager.GetSceneByPath(ChapterPath);
            var opened = !scene.IsValid() || !scene.isLoaded;
            try
            {
                if (opened) scene = EditorSceneManager.OpenScene(ChapterPath, OpenSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                var root = FindRoot(scene);
                if (root.VisualEnvironment == null)
                {
                    CreateEnvironment(root, definition);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                ConfigureEmptyPlayerSlots();
                if (!File.Exists(ReviewPath)) CreateReviewScene();
                AssetDatabase.SaveAssets();
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (opened && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene,true);
            }
            Debug.Log("VISUAL WORLD INTEGRATION FOUNDATION assets created; gameplay/build settings preserved.");
        }

        private static S01SceneCompositionRoot FindRoot(Scene scene)
        {
            foreach (var obj in scene.GetRootGameObjects())
                if (obj.TryGetComponent<S01SceneCompositionRoot>(out var root)) return root;
            throw new InvalidOperationException("Chapter composition root is missing.");
        }

        private static void CreateHubPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(HubPath));
            AssetDatabase.Refresh();
            var root = new GameObject("RepairHub Integration Anchors");
            var positions = new[] { Vector3.zero, new Vector3(-2.2f,1.4f,0), new Vector3(2.2f,1.4f,0),
                new Vector3(-1.4f,1.6f,-2), new Vector3(1.4f,1.6f,-2),
                new Vector3(-1.2f,1.2f,0), new Vector3(1.2f,1.2f,0),
                new Vector3(-.8f,1.2f,-1.2f), new Vector3(.8f,1.2f,-1.2f), Vector3.zero, Vector3.zero };
            for (var i = 0; i < HubAnchors.Length; i++) Child(root.transform, HubAnchors[i]).localPosition = positions[i];
            PrefabUtility.SaveAsPrefabAsset(root, HubPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void CreateEnvironment(S01SceneCompositionRoot root, ChapterVisualIntegrationDefinition definition)
        {
            var serializedRoot = new SerializedObject(root);
            var world = ((Chapter01WorldDefinition)serializedRoot.FindProperty("_worldDefinition").objectReferenceValue).Configuration;
            var elite = (MagnetarGuardDefinition)serializedRoot.FindProperty("_magnetarGuardDefinition").objectReferenceValue;
            var boss = (CustodianBossDefinition)serializedRoot.FindProperty("_custodianBossDefinition").objectReferenceValue;
            var spawn = serializedRoot.FindProperty("_playerSpawn").vector3Value;
            var environmentRoot = Child(root.transform, "Chapter 01 Visual Environment");
            var environment = environmentRoot.gameObject.AddComponent<ChapterVisualEnvironment>();
            var serialized = new SerializedObject(environment);
            serialized.FindProperty("_definition").objectReferenceValue = definition;
            var fields = new[] { "_floor", "_structures", "_props", "_pipes", "_machinery", "_lighting", "_debris", "_fallbackRoot" };
            var names = new[] { "FloorTerrain", "WallsStructures", "Props", "Pipes", "Machinery", "Lighting", "Debris", "FallbackPrototypeEnvironment" };
            for (var i = 0; i < fields.Length; i++)
                serialized.FindProperty(fields[i]).objectReferenceValue = Child(environmentRoot, names[i]);
            var lighting = (Transform)serialized.FindProperty("_lighting").objectReferenceValue;
            var key = Child(lighting, "Chapter Baseline Key Light").gameObject.AddComponent<Light>();
            key.type = LightType.Directional; key.intensity = 1.1f; key.color = Color.white;
            key.shadows = LightShadows.None; key.transform.rotation = Quaternion.Euler(50,-30,0);
            serialized.FindProperty("_keyLight").objectReferenceValue = key;
            var regionsRoot = Child(environmentRoot, "Regions");
            var regions = serialized.FindProperty("_regions");
            regions.arraySize = 11;
            var hub = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HubPath), regionsRoot);
            hub.name = "repair-hub"; hub.transform.position = spawn;
            SetRegion(regions, 0, "repair-hub", null, hub.transform, null);
            SetRegion(regions, 1, "start-region", null, Region(regionsRoot,"start-region",spawn + Vector3.forward * 6), null);
            var spots = serializedRoot.FindProperty("_spawnSpotDefinitions");
            for (var i = 0; i < spots.arraySize; i++)
            {
                var config = ((SpawnSpotDefinition)spots.GetArrayElementAtIndex(i).objectReferenceValue).CreateRuntimeConfiguration();
                var region = Region(regionsRoot, config.Id, config.WorldOrigin);
                var landmark = Child(region, "LandmarkAnchor");
                landmark.position = world.GetZone(i).LandmarkPosition;
                landmark.rotation = Quaternion.Euler(0,i * 28,0);
                Child(region, "DressingAnchor");
                SetRegion(regions, i + 2, config.Id, config.Enemy.Id, region, landmark);
            }
            SetRegion(regions, 7, "elite-approach", elite.Id,
                Region(regionsRoot,"elite-approach",world.EliteGate.Position + Vector3.back * 4),null);
            SetRegion(regions, 8, "elite-arena", elite.Id, Region(regionsRoot,"elite-arena",elite.Configuration.SpawnPosition),null);
            SetRegion(regions, 9, "boss-approach", boss.Id,
                Region(regionsRoot,"boss-approach",world.BossGate.Position + Vector3.back * 4),null);
            SetRegion(regions, 10, "boss-arena", boss.Id, Region(regionsRoot,"boss-arena",world.BossArenaCenter),null);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            serializedRoot.FindProperty("_visualEnvironment").objectReferenceValue = environment;
            serializedRoot.ApplyModifiedPropertiesWithoutUndo();
            environment.ValidateOrThrow();
        }

        private static void ConfigureEmptyPlayerSlots()
        {
            var evolution = AssetDatabase.LoadAssetAtPath<EvolutionDefinition>("Assets/_Game/Content/Definitions/S07_Evolution.asset");
            var serialized = new SerializedObject(evolution);
            var slots = serialized.FindProperty("_tierOverrides");
            if (slots.arraySize != 0) return;
            slots.arraySize = 3;
            for (var i = 0; i < 3; i++) slots.GetArrayElementAtIndex(i).FindPropertyRelative("_localScale").vector3Value = Vector3.one;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            evolution.ValidateOrThrow();
        }

        private static void CreateReviewScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReviewPath)); AssetDatabase.Refresh();
            // Unity forbids NewScene(Additive) alongside an untitled scene. Only close empty scratch scenes.
            // Never discard an artist's unsaved hierarchy.
            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scratch = SceneManager.GetSceneAt(i);
                if (!string.IsNullOrEmpty(scratch.path)) continue;
                if (scratch.rootCount != 0)
                    throw new InvalidOperationException("Save the nonempty untitled scene before creating the missing art review scene.");
                EditorSceneManager.CloseScene(scratch,true);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var reviewRoot = new GameObject("Isolated Art Review — no gameplay authority");
            var slotsRoot = Child(reviewRoot.transform, "Standardized Slots");
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(S15AssetConfigurator.CatalogPath);
            var tier2 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/ArtSpike/Prefabs/Player/G0_Tier2_ArtSpike.prefab");
            var material = AssetDatabase.LoadAssetAtPath<Material>(S15AssetConfigurator.BodyMaterialPath);
            var labels = new[] { "PlayerTier2", "Ordinary_CutterUnit", "Elite_MagnetarGuard", "Boss_CustodianM0" };
            for (var i = 0; i < labels.Length; i++)
            {
                var slotRoot = Child(slotsRoot, labels[i]); slotRoot.localPosition = new Vector3(-9 + i * 6,0,0);
                var slot = slotRoot.gameObject.AddComponent<ArtReviewSlot>();
                GameObject fallback;
                if (i == 0) fallback = (GameObject)PrefabUtility.InstantiatePrefab(tier2,slotRoot);
                else if (i == 1)
                {
                    if (!catalog.TryGetEnemy("cutter-unit",out var recipe)) throw new InvalidOperationException("Cutter recipe missing.");
                    fallback = S15VisualFactory.Build(slotRoot,recipe,catalog);
                }
                else
                {
                    fallback = PurePrimitive(slotRoot,PrimitiveType.Capsule,"Current Prototype Fallback",material);
                    fallback.transform.localPosition = Vector3.up;
                    fallback.transform.localScale = i == 2 ? new Vector3(1.05f,1,1.05f) : new Vector3(1.6f,1.2f,1.6f);
                }
                var serialized = new SerializedObject(slot);
                serialized.FindProperty("_fallback").objectReferenceValue = fallback;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var floor = PurePrimitive(reviewRoot.transform,PrimitiveType.Cube,"Neutral Review Floor",material);
            floor.transform.position = new Vector3(0,-.15f,0); floor.transform.localScale = new Vector3(28,.2f,9);
            var reference = PurePrimitive(reviewRoot.transform,PrimitiveType.Cube,"Scale Reference — 1 metre",material);
            reference.transform.position = new Vector3(-13,.5f,0); reference.transform.localScale = Vector3.one;
            var key = Child(reviewRoot.transform,"Neutral Review Key Light").gameObject.AddComponent<Light>();
            key.type = LightType.Directional; key.intensity = 1.1f; key.shadows = LightShadows.None;
            key.transform.rotation = Quaternion.Euler(50,-30,0);
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.3f,.3f,.3f);
            var overview = ReviewCamera(reviewRoot.transform,"Review Overview",new Vector3(0,12,-20),Vector3.up,false);
            overview.orthographic = true; overview.orthographicSize = 8; overview.aspect = 2;
            var settings = AssetDatabase.LoadAssetAtPath<CameraSettings>(CameraPath);
            var gameplay = ReviewCamera(reviewRoot.transform,"Gameplay Camera Option",slotsRoot.GetChild(0).position + settings.Offset,
                slotsRoot.GetChild(0).position + Vector3.up * settings.LookAtHeight,true);
            gameplay.fieldOfView = settings.FieldOfView; gameplay.aspect = 9f/16f;
            var close = ReviewCamera(reviewRoot.transform,"Close Review Camera",slotsRoot.GetChild(0).position + new Vector3(2.4f,1.8f,3.2f),
                slotsRoot.GetChild(0).position + Vector3.up * .9f,true);
            close.fieldOfView = 40;
            EditorSceneManager.SaveScene(scene,ReviewPath);
            EditorSceneManager.CloseScene(scene,true);
        }

        private static UnityEngine.Camera ReviewCamera(Transform parent, string name, Vector3 position, Vector3 target, bool disabled)
        {
            var camera = Child(parent,name).gameObject.AddComponent<UnityEngine.Camera>();
            camera.transform.position = position; camera.transform.LookAt(target);
            camera.enabled = !disabled; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.09f,.11f,.13f); camera.nearClipPlane = .1f; camera.farClipPlane = 200;
            return camera;
        }
        private static GameObject PurePrimitive(Transform parent, PrimitiveType type, string name, Material material)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent,false);
            UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            obj.GetComponent<Renderer>().sharedMaterial = material; return obj;
        }
        private static Transform Region(Transform parent, string id, Vector3 position)
        { var root = Child(parent,id); root.position = position; return root; }
        private static Transform Child(Transform parent, string name)
        { var root = new GameObject(name).transform; root.SetParent(parent,false); return root; }
        private static void SetRegion(SerializedProperty list, int index, string id, string enemyId, Transform root, Transform landmark)
        {
            var entry = list.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("_id").stringValue = id;
            entry.FindPropertyRelative("_enemyId").stringValue = enemyId ?? "";
            entry.FindPropertyRelative("_root").objectReferenceValue = root;
            entry.FindPropertyRelative("_landmark").objectReferenceValue = landmark;
        }
    }
}
