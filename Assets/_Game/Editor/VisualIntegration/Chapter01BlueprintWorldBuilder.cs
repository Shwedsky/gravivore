using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    public static class Chapter01BlueprintWorldBuilder
    {
        public const string Root = "Assets/_Game/Content/BlueprintWorldR1";
        public const string Output = "docs/history/implementation-passes/chapter01-blueprint-world-r1/verification";
        public const string Layer = "Chapter 01 Blueprint World R1";
        public const string ScenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        private const string WorldPath = "Assets/_Game/Content/Definitions/S08_Chapter01World.asset";
        [Serializable] public sealed class Volume { public string id; public Vector3 center, size; }
        [Serializable] public sealed class Sector { public string id, model; public Vector3 center; public float width, depth; }
        [Serializable] public sealed class LayoutDocument
        {
            public Vector3 groundCenter, eliteEncounterPosition; public Vector2 groundSize;
            public Sector[] sectors; public Volume[] surfaces, blockers;
            public Vector3[] route, strongSpotPositions;
        }
        [Serializable] private sealed class WorldAudit
        {
            public bool validated, actorsRedesigned, uiRedesigned, oldCollisionFingerprintRequired;
            public int sectors, movementSurfaces, architecturalBlockers, activeV45, activeV46, activeV47, activeRepeatedDeck;
            public int staticRenderers, materials, lights, missingMaterials, missingMeshes;
            public long staticTriangles, textureBytes;
            public string[] dependencies;
        }
        public static bool IsBlueprintWorld() => AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(WorldPath)?.Layout != null;
        public static LayoutDocument ReadLayout() => JsonUtility.FromJson<LayoutDocument>(File.ReadAllText("Tools/blueprint-world-r1/layout.json"));
        public static void Build()
        {
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Root + "/Materials");
            Directory.CreateDirectory(Root + "/Prefabs");
            AssetDatabase.Refresh();
            BuildMaterials();
            var layout = ReadLayout();
            foreach (var name in layout.sectors.Select(s => s.model).Concat(new[] { "ServiceNetwork", "GateBarrier" })) BuildPrefab(name);
            var authority = BuildLayout(layout);
            RemapDefinitions(layout, authority);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var root = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var old = root.VisualEnvironment;
            var definition = old.Definition;
            var keyLight = Object.Instantiate(old.KeyLight.gameObject);
            var environmentObject = new GameObject(Layer, typeof(ChapterVisualEnvironment));
            environmentObject.transform.SetParent(root.transform, false);
            var environment = environmentObject.GetComponent<ChapterVisualEnvironment>();
            var data = new SerializedObject(environment);
            data.FindProperty("_definition").objectReferenceValue = definition;
            data.FindProperty("_fullChapterProduction").boolValue = true;
            data.FindProperty("_blueprintWorldOnly").boolValue = true;
            data.FindProperty("_showFallbackEnvironment").boolValue = false;
            var fallback = Group(environment.transform, "Inactive compatibility anchors"); fallback.gameObject.SetActive(false);
            data.FindProperty("_fallbackRoot").objectReferenceValue = fallback;
            foreach (var field in new[] { "_floor", "_structures", "_props", "_pipes", "_machinery", "_lighting", "_debris" })
                data.FindProperty(field).objectReferenceValue = Group(environment.transform, field.Substring(1));
            data.ApplyModifiedPropertiesWithoutUndo();
            keyLight.transform.SetParent(environment.Lighting, false);
            var light = keyLight.GetComponent<Light>();
            light.color = new Color(.80f,.88f,1f); light.intensity = 1.65f;
            light.transform.rotation = Quaternion.Euler(51,-32,0);
            data = new SerializedObject(environment); data.FindProperty("_keyLight").objectReferenceValue = light;
            data.FindProperty("_sliceGate").objectReferenceValue = Prefab("GateBarrier");
            data.FindProperty("_sliceObstacles").arraySize = 0;
            data.FindProperty("_dressing").arraySize = 0;
            var regions = data.FindProperty("_regions"); regions.arraySize = 11;
            var bindings = new List<(string id, string enemy, Vector3 center)>();
            foreach (var sector in layout.sectors)
            {
                var anchor = Group(environment.Floor, sector.id); anchor.position = sector.center;
                var model = (GameObject)PrefabUtility.InstantiatePrefab(Prefab(sector.model), anchor);
                model.name = sector.model + " / authored industrial sector";
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                if (sector.id == "repair-hub")
                {
                    Group(anchor, "MainPlatform"); Group(anchor, "PlayerDockPoint");
                }
                var enemy = EnemyId(sector.id);
                bindings.Add((sector.id, enemy, sector.center));
                // Bounded existing atmosphere uses authored service endpoints.
                var fault = Group(anchor, "Fidelity service fault"); fault.localPosition = new Vector3(sector.id == "boss-arena" ? -9 : -6, .3f, 6);
                var energy = Group(anchor, "Fidelity light " + (sector.id == "repair-hub" || sector.id == "capacitor-field" ? "cyan" : "amber"));
                energy.localPosition = new Vector3(sector.id == "elite-arena" ? 8.5f : -6, 2.5f, 6);
            }
            bindings.Add(("start-region", "", new Vector3(0,0,-30)));
            bindings.Add(("elite-approach", "", new Vector3(0,0,44)));
            bindings.Add(("boss-approach", "", new Vector3(0,0,64)));
            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                var anchor = environment.Floor.Find(binding.id) ?? Group(environment.Floor, binding.id);
                anchor.position = binding.center;
                var landmark = Group(anchor, "Landmark anchor"); landmark.localPosition = new Vector3(0,0,8);
                var entry = regions.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_id").stringValue = binding.id;
                entry.FindPropertyRelative("_enemyId").stringValue = binding.enemy;
                entry.FindPropertyRelative("_root").objectReferenceValue = anchor;
                entry.FindPropertyRelative("_landmark").objectReferenceValue = landmark;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            var services = (GameObject)PrefabUtility.InstantiatePrefab(Prefab("ServiceNetwork"), environment.Floor);
            services.name = "Authored cross-sector service network";
            services.transform.localPosition = Vector3.zero;
            var rootData = new SerializedObject(root);
            rootData.FindProperty("_visualEnvironment").objectReferenceValue = environment;
            rootData.FindProperty("_playerSpawn").vector3Value = layout.sectors[0].center;
            rootData.ApplyModifiedPropertiesWithoutUndo();
            Object.DestroyImmediate(old.gameObject);
            foreach (var node in environment.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(node.gameObject, StaticEditorFlags.BatchingStatic);
            environment.ValidateOrThrow();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.40f,.48f,.58f);
            RenderSettings.ambientEquatorColor = new Color(.20f,.26f,.32f);
            RenderSettings.ambientGroundColor = new Color(.075f,.085f,.10f);
            BuildIndustrialReflection();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Audit();
            Debug.Log("CHAPTER01_BLUEPRINT_WORLD_R1_AUTHORED");
        }
        private static Transform Group(Transform parent, string name)
        { var node = new GameObject(name).transform; node.SetParent(parent,false); return node; }
        private static string EnemyId(string id)
        {
            if (id == "relay-yard") return "scout-drone";
            if (id == "cutting-floor") return "cutter-unit";
            if (id == "capacitor-field") return "arc-drone";
            if (id == "shield-dump") return "warden";
            if (id == "hauler-graveyard") return "carrier";
            if (id == "elite-arena") return "magnetar-guard";
            if (id == "boss-arena") return "custodian-m0";
            return "";
        }
        private static GameObject Prefab(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/R1_" + name + ".prefab");
        private static void BuildIndustrialReflection()
        {
            // Offline reflection cards provide bounded specular light for metal.
            // There are no realtime reflection cameras or additional lights.
            const int size=64;
            var path=Root+"/Textures/R1_IndustrialReflection.asset";
            var cube=AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if(cube==null){cube=new Cubemap(size,TextureFormat.RGBAHalf,true);AssetDatabase.CreateAsset(cube,path);}
            for(var face=0;face<6;face++)
            {
                var pixels=new Color[size*size];
                for(var y=0;y<size;y++)for(var x=0;x<size;x++)
                {
                    var u=2f*(x+.5f)/size-1;var v=2f*(y+.5f)/size-1;
                    var direction=face==0?new Vector3(1,v,-u):face==1?new Vector3(-1,v,u):face==2?new Vector3(u,1,-v):face==3?new Vector3(u,-1,v):face==4?new Vector3(u,v,1):new Vector3(-u,v,-1);
                    direction.Normalize();
                    var color=Color.Lerp(new Color(.10f,.13f,.17f),new Color(.62f,.72f,.83f),Mathf.Clamp01(direction.y*.8f+.42f));
                    if(direction.y>.30f && Mathf.Abs(direction.x)<.09f)color+=new Color(1.1f,1.3f,1.5f);
                    if(Mathf.Abs(direction.y)<.12f && direction.z>.65f)color+=new Color(.08f,.15f,.18f);
                    pixels[y*size+x]=color;
                }
                cube.SetPixels(pixels,(CubemapFace)face);
            }
            cube.Apply(true,false);EditorUtility.SetDirty(cube);
            RenderSettings.defaultReflectionMode=UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture=cube;RenderSettings.reflectionIntensity=.8f;
        }
        private static void BuildMaterials()
        {
            foreach (var path in Directory.GetFiles(Root + "/Textures", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = path.EndsWith("_Normal.png",StringComparison.Ordinal) ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = path.EndsWith("_BaseColor.png",StringComparison.Ordinal) || path.EndsWith("_Emission.png",StringComparison.Ordinal);
                importer.maxTextureSize = 1024; importer.mipmapEnabled = true; importer.isReadable = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
            foreach (var name in new[] { "R1_Trim1", "R1_Trim2", "R1_Trim3", "R1_Industrial" })
            {
                var path = Root + "/Materials/" + name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material,path); }
                foreach (var pair in new[] { ("_BaseMap","BaseColor"),("_BumpMap","Normal"),("_MetallicGlossMap","MetallicSmoothness"),("_OcclusionMap","Occlusion"),("_EmissionMap","Emission") })
                {
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/" + name + "_" + pair.Item2 + ".png");
                    if (texture != null) material.SetTexture(pair.Item1,texture);
                }
                material.SetColor("_BaseColor",Color.white);
                material.SetFloat("_BumpScale",.8f); material.SetFloat("_Smoothness",.65f); material.SetFloat("_OcclusionStrength",1);
                material.EnableKeyword("_NORMALMAP"); material.EnableKeyword("_METALLICSPECGLOSSMAP"); material.EnableKeyword("_OCCLUSIONMAP");
                if (material.GetTexture("_EmissionMap") != null)
                { material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive; material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor",Color.white*2.4f); }
                material.enableInstancing = true; EditorUtility.SetDirty(material);
            }
        }
        private static void BuildPrefab(string name)
        {
            var path = Root + "/Models/R1_" + name + ".fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Missing authored sector model " + path);
            importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
            importer.animationType = ModelImporterAnimationType.None; importer.bakeAxisConversion = true;
            importer.isReadable = false; importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var obj = new GameObject("R1_" + name);
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),obj.transform,false);
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m =>
                {
                    var key = m == null ? "R1_Industrial" : m.name.StartsWith("R1_Trim1") ? "R1_Trim1" : m.name.StartsWith("R1_Trim2") ? "R1_Trim2" : m.name.StartsWith("R1_Trim3") ? "R1_Trim3" : "R1_Industrial";
                    return AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/" + key + ".mat");
                }).ToArray();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            PrefabUtility.SaveAsPrefabAsset(obj,Root + "/Prefabs/R1_" + name + ".prefab"); Object.DestroyImmediate(obj);
        }
        private static ChapterWorldLayoutDefinition BuildLayout(LayoutDocument layout)
        {
            var path = Root + "/Chapter01WorldLayout.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ChapterWorldLayoutDefinition>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<ChapterWorldLayoutDefinition>(); AssetDatabase.CreateAsset(asset,path); }
            var data = new SerializedObject(asset);
            foreach (var pair in new[] { ("_surfaces",layout.surfaces),("_blockers",layout.blockers) })
            {
                var array = data.FindProperty(pair.Item1); array.arraySize = pair.Item2.Length;
                for (var i = 0; i < array.arraySize; i++)
                {
                    var entry = array.GetArrayElementAtIndex(i); var source = pair.Item2[i];
                    entry.FindPropertyRelative("_id").stringValue = source.id;
                    entry.FindPropertyRelative("_center").vector3Value = source.center;
                    entry.FindPropertyRelative("_size").vector3Value = source.size;
                }
            }
            foreach (var pair in new[] { ("_route",layout.route),("_strongSpotPositions",layout.strongSpotPositions) })
            {
                var array = data.FindProperty(pair.Item1); array.arraySize = pair.Item2.Length;
                for (var i = 0; i < array.arraySize; i++) array.GetArrayElementAtIndex(i).vector3Value = pair.Item2[i];
            }
            data.ApplyModifiedPropertiesWithoutUndo(); asset.ValidateOrThrow(); return asset;
        }
        private static void RemapDefinitions(LayoutDocument layout, ChapterWorldLayoutDefinition authority)
        {
            var world = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(WorldPath);
            var data = new SerializedObject(world);
            data.FindProperty("_layout").objectReferenceValue = authority;
            data.FindProperty("_basinCenter").vector3Value = layout.sectors[0].center;
            data.FindProperty("_groundCenter").vector3Value = layout.groundCenter;
            data.FindProperty("_groundSize").vector2Value = layout.groundSize;
            var zones = data.FindProperty("_zones");
            for (var i = 0; i < zones.arraySize; i++)
            {
                var zone = zones.GetArrayElementAtIndex(i); var id = zone.FindPropertyRelative("_id").stringValue;
                var sector = layout.sectors.Single(s => s.id == id);
                zone.FindPropertyRelative("_center").vector3Value = sector.center;
                zone.FindPropertyRelative("_landmarkPosition").vector3Value = sector.center + Vector3.forward*8;
                var paths = AssetDatabase.FindAssets("t:SpawnSpotDefinition",new[] { "Assets/_Game/Content/Definitions" });
                var spawn = paths.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SpawnSpotDefinition>).Single(s => s.Id == id);
                var spawnData = new SerializedObject(spawn); spawnData.FindProperty("_worldOrigin").vector3Value = sector.center; spawnData.ApplyModifiedPropertiesWithoutUndo();
            }
            data.FindProperty("_eliteGate").FindPropertyRelative("_position").vector3Value = new Vector3(0,0,44);
            data.FindProperty("_bossGate").FindPropertyRelative("_position").vector3Value = new Vector3(0,0,64);
            foreach (var gate in new[] { "_eliteGate","_bossGate" }) data.FindProperty(gate).FindPropertyRelative("_size").vector3Value = new Vector3(8,2.5f,.6f);
            data.FindProperty("_bossArenaCenter").vector3Value = new Vector3(0,0,76);
            data.ApplyModifiedPropertiesWithoutUndo(); world.ValidateOrThrow();
            var elite = new SerializedObject(AssetDatabase.LoadAssetAtPath<MagnetarGuardDefinition>("Assets/_Game/Content/Definitions/S09_MagnetarGuard.asset"));
            elite.FindProperty("_spawnPosition").vector3Value = layout.eliteEncounterPosition; elite.ApplyModifiedPropertiesWithoutUndo();
            var boss = new SerializedObject(AssetDatabase.LoadAssetAtPath<CustodianBossDefinition>("Assets/_Game/Content/Definitions/S09_CustodianM0.asset"));
            boss.FindProperty("_startPosition").vector3Value = new Vector3(0,0,76); boss.ApplyModifiedPropertiesWithoutUndo();
        }
        public static void Audit()
        {
            Directory.CreateDirectory(Output);
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(ScenePath);
                var root = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
                var environment = root.VisualEnvironment;
                if (!environment.BlueprintWorldOnly) throw new InvalidOperationException("Production scene has no rebuilt world.");
                environment.ValidateOrThrow();
                var layout = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(WorldPath).Layout;
                if (layout == null) throw new InvalidOperationException("New collision authority is missing.");
                layout.ValidateOrThrow();
                var renderers = environment.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
                var names = environment.GetComponentsInChildren<Transform>(true).Where(t => t.gameObject.activeInHierarchy).Select(t => t.name).ToArray();
                var report = new WorldAudit { validated = true, sectors = ReadLayout().sectors.Length,
                    movementSurfaces = layout.SurfaceCount, architecturalBlockers = layout.BlockerCount,
                    activeV45 = names.Count(n => n == ConceptCorrectiveV45Builder.Layer),
                    activeV46 = names.Count(n => n == SurfaceHeroV46Builder.Layer),
                    activeV47 = names.Count(n => n == ConceptConvergenceV47Builder.Layer),
                    activeRepeatedDeck = names.Count(n => n.Contains("Compact small-panel deck") || n.Contains("Flush industrial panel deck V47")),
                    staticRenderers = renderers.Length, materials = renderers.SelectMany(r => r.sharedMaterials).Distinct().Count(),
                    lights = environment.GetComponentsInChildren<Light>(true).Count(l => l.enabled), dependencies = AssetDatabase.GetDependencies(ScenePath,true) };
                if (report.activeV45 + report.activeV46 + report.activeV47 + report.activeRepeatedDeck != 0)
                    throw new InvalidOperationException("Historical world implementation remains active.");
                foreach (var renderer in renderers)
                {
                    Rendering.RenderingBuildAudit.ValidateRenderer(renderer);
                    var mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                    if (mesh == null) throw new InvalidOperationException("Missing world mesh.");
                    if (!AssetDatabase.GetAssetPath(mesh).StartsWith(Root + "/Models/",StringComparison.Ordinal))
                        throw new InvalidOperationException("World uses a stale or primitive presentation mesh: " + renderer.name);
                    for (var i = 0; i < mesh.subMeshCount; i++) report.staticTriangles += (long)mesh.GetIndexCount(i)/3;
                    foreach (var material in renderer.sharedMaterials)
                    {
                        Rendering.RenderingBuildAudit.ValidateMaterial(material,renderer.name);
                        foreach (var map in new[] { "_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap" })
                            if (material.GetTexture(map) == null) throw new InvalidOperationException("World surface lacks " + map);
                    }
                }
                foreach (var sector in ReadLayout().sectors)
                    if (environment.Floor.Find(sector.id) == null || environment.Floor.Find(sector.id).GetComponentsInChildren<Renderer>().Length < 1)
                        throw new InvalidOperationException("Missing visual sector " + sector.id);
                if (report.dependencies.Any(p => p.Contains("ExternalAssetIntake") || p.Contains("98_unclassified") || p.Contains(".local-g0-v2")))
                    throw new InvalidOperationException("Raw or historical intake escaped into runtime references.");
                report.textureBytes = report.dependencies.Select(AssetDatabase.LoadAssetAtPath<Texture>).Where(t => t != null).Distinct().Sum(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong);
                ConceptConvergenceV47Builder.AuditActors();
                File.WriteAllText(Output + "/world-audit.json",JsonUtility.ToJson(report,true));
                File.WriteAllLines(Output + "/production-dependencies.txt",report.dependencies);
                Debug.Log("CHAPTER01_BLUEPRINT_WORLD_R1_AUDIT_PASS");
            }
            finally
            {
                if (setup.Any(s => s.isLoaded) && setup.Count(s => s.isActive) == 1) EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            }
        }
    }
}
