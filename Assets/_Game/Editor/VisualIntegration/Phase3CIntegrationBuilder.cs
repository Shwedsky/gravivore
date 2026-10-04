using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEngine.Rendering;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Explicit editor intake/authoring workflow; never runs in a player build.</summary>
    public static class Phase3CIntegrationBuilder
    {
        public const string PackRoot = "Assets/_Game/Phase3B/Prefabs/";
        public const string ReviewRoot = "docs/phase3c";
        [Serializable] private sealed class IntakeBatch { public ArtIntakeReport[] candidates; }

        [MenuItem("Gravivore/Visual Integration/Phase 3C Intake")]
        public static void Intake()
        {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { PackRoot.TrimEnd('/') })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p, StringComparer.Ordinal).ToList();
            paths.Add("Assets/_Game/ArtSpike/Prefabs/Enemies/Cutter_ArtSpike.prefab");
            var reports = paths.Select(path => ArtIntakeTool.Inspect(
                AssetDatabase.LoadAssetAtPath<GameObject>(path), Role(path))).ToArray();
            Directory.CreateDirectory(ReviewRoot);
            File.WriteAllText(ReviewRoot + "/ART_INTAKE.json", JsonUtility.ToJson(new IntakeBatch { candidates = reports }, true) + "\n");
            foreach (var report in reports)
            {
                Debug.Log($"Phase3C intake: {report.sourcePath}: {report.rendererCount} renderers, {report.triangles} triangles, safe={report.runtimeSafe}");
                if (!report.runtimeSafe || report.missingMeshes + report.missingMaterials + report.brokenTextureReferences != 0 ||
                    report.incompatibleShaders.Length != 0)
                    throw new InvalidOperationException("Unsafe/broken Phase3C content: " + report.sourcePath + " " + report.runtimeSafetyReason);
            }
            Debug.Log("Phase3C intake passed: " + reports.Length + " candidates. Soft budget warnings retained for review.");
        }

        private static ArtCandidateRole Role(string path) => path.Contains("Environment/") ? ArtCandidateRole.Environment :
            path.Contains("MagnetarGuard") ? ArtCandidateRole.Elite : path.Contains("CustodianM0") ? ArtCandidateRole.Boss : ArtCandidateRole.Ordinary;

        // Phase3B text export rounded signed 64-bit FBX mesh IDs. Recover only a unique nearby
        // mesh ID in the SAME committed GUID; never substitute geometry or an external asset.
        public static void RepairMeshReferencesAndIntake()
        {
            var repairs = new List<string>();
            foreach (var path in Directory.GetFiles(PackRoot, "*.prefab", SearchOption.AllDirectories).OrderBy(p => p))
            {
                var original = File.ReadAllText(path);
                var repaired = Regex.Replace(original, @"m_Mesh: \{fileID: (-?\d+), guid: ([a-f0-9]+), type: 3\}", match =>
                {
                    var oldId = long.Parse(match.Groups[1].Value);
                    var guid = match.Groups[2].Value;
                    var meshPath = AssetDatabase.GUIDToAssetPath(guid);
                    var ids = AssetDatabase.LoadAllAssetsAtPath(meshPath).OfType<Mesh>().Select(mesh =>
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mesh, out string _, out long id);
                        return id;
                    }).ToArray();
                    if (ids.Contains(oldId)) return match.Value;
                    var nearby = ids.Where(id => Math.Abs((decimal)id - oldId) <= 4096m).ToArray();
                    if (nearby.Length != 1) throw new InvalidOperationException("Cannot uniquely repair mesh reference: " + path + " / " + meshPath);
                    repairs.Add(path + ": " + guid + " " + oldId + " -> " + nearby[0]);
                    return match.Value.Replace("fileID: " + oldId, "fileID: " + nearby[0]);
                });
                if (repaired != original) File.WriteAllText(path, repaired);
            }
            Directory.CreateDirectory(ReviewRoot);
            File.WriteAllLines(ReviewRoot + "/MESH_REFERENCE_REPAIRS.txt", repairs);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Intake();
        }

        [MenuItem("Gravivore/Visual Integration/Configure Phase 3C")]
        public static void Configure()
        {
            Intake(); // Reject real broken/unsafe content before changing ANY binding.
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(S15AssetConfigurator.CatalogPath);
            var catalogObject = new SerializedObject(catalog);
            BindRecipe(catalogObject, "scout-drone", "ScoutDrone", .7f, Vector3.zero);
            BindRecipe(catalogObject, "arc-drone", "ArcDrone", .7f, new Vector3(0,.014f,0));
            BindRecipe(catalogObject, "warden", "Warden", .7f, new Vector3(0,.057f,0));
            BindRecipe(catalogObject, "carrier", "Carrier", .65f, new Vector3(0,.039f,0));
            // Accepted Cutter and all legacy fallback parts remain byte-for-byte authored.
            catalogObject.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(catalog);
            catalog.ValidateOrThrow();
            var definition = AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>(VisualIntegrationFoundationBuilder.DefinitionPath);
            var definitionObject = new SerializedObject(definition);
            Binding(definitionObject.FindProperty("_elite"), PackRoot + "Enemies/MagnetarGuard_Phase3B.prefab", new Vector3(0,.05f,0), Vector3.one * .8f);
            Binding(definitionObject.FindProperty("_boss"), PackRoot + "Enemies/CustodianM0_Phase3B.prefab", new Vector3(0,.085f,0), Vector3.one);
            Binding(definitionObject.FindProperty("_repairHub"), PackRoot + "Environment/RepairHub_Phase3B.prefab", new Vector3(0,-.2f,0), Vector3.one);
            definitionObject.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(definition);
            var scene = EditorSceneManager.OpenScene(VisualIntegrationFoundationBuilder.ChapterPath, OpenSceneMode.Single);
            var composition = scene.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var environment = composition.VisualEnvironment;
            var envObject = new SerializedObject(environment);
            envObject.FindProperty("_showFallbackEnvironment").boolValue = false;
            var regions = new[] { "relay-yard", "cutting-floor", "shield-dump", "capacitor-field", "hauler-graveyard",
                "elite-approach", "elite-arena", "boss-approach", "boss-arena" };
            var prefabs = new[] { "RelayYard", "CuttingFloor", "ShieldDump", "CapacitorField", "HaulerGraveyard",
                "EliteApproach", "EliteArena", "BossApproach", "BossArena" };
            var dressing = envObject.FindProperty("_dressing"); dressing.arraySize = regions.Length;
            for (var i = 0; i < regions.Length; i++)
            {
                var region = environment.GetRegion(regions[i]);
                var entry = dressing.GetArrayElementAtIndex(i);
                // Dedicated identity anchor avoids the old landmark's authored yaw.
                var anchor = region.Root.Find("Phase3C Dressing") ?? new GameObject("Phase3C Dressing").transform;
                anchor.SetParent(region.Root, false); anchor.localPosition = Vector3.zero;
                entry.FindPropertyRelative("_anchor").objectReferenceValue = anchor;
                var offset = i < 5 ? new Vector3(0,-.05f,i == 3 ? 4.5f : 3.5f) : Vector3.zero;
                Binding(entry.FindPropertyRelative("_model"), PackRoot + "Environment/" + prefabs[i] + "_Phase3B.prefab", offset, Vector3.one);
                if (region.Landmark != null) region.Landmark.gameObject.SetActive(false);
            }
            envObject.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(environment);
            AlignHubAnchors(environment);
            BuildFloorAndRoutes(environment);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f,.61f,.68f);
            var ambientProbe = new SphericalHarmonicsL2(); ambientProbe.AddAmbientLight(RenderSettings.ambientLight);
            RenderSettings.ambientProbe = ambientProbe;
            environment.KeyLight.intensity = 1.6f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            const string reflectionPath = "Assets/_Game/Content/Materials/Phase3C_IndustrialReflection.cubemap";
            if (AssetDatabase.LoadAssetAtPath<Cubemap>(reflectionPath) == null &&
                !AssetDatabase.CopyAsset("Assets/_Game/ArtSpike/Materials/ArtSpike_StudioReflection.cubemap",reflectionPath))
                throw new InvalidOperationException("Unable to author shared runtime reflection; comparison-bay lighting stays isolated.");
            RenderSettings.customReflection = AssetDatabase.LoadAssetAtPath<Cubemap>(reflectionPath);
            RenderSettings.reflectionIntensity = .7f;
            environment.ValidateOrThrow();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Unable to save integrated Chapter01.");
            AssetDatabase.SaveAssets();
            VisualIntegrationValidator.ValidateOrThrow();
            Debug.Log("Phase3C bindings and authored visual composition configured; gameplay configuration unchanged.");
        }

        public static void TuneCompositionAndConfigure()
        {
            const string recordPath = ReviewRoot + "/COMPOSITION_OFFSETS.txt";
            if (File.Exists(recordPath)) throw new InvalidOperationException("Composition tuning already applied; use Configure to rebuild bindings idempotently.");
            var records = new List<string>();
            foreach (var path in Directory.GetFiles(PackRoot + "Environment", "*.prefab").OrderBy(p => p))
            {
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var filter in contents.GetComponentsInChildren<MeshFilter>(true))
                    {
                        var node = filter.transform;
                        var mesh = filter.sharedMesh;
                        if (!AssetDatabase.GetAssetPath(mesh).EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                        var previousPosition = node.localPosition; var previousScale = node.localScale; var previousRotation = node.localEulerAngles;
                        var isDeck = node.name.StartsWith("Deck", StringComparison.Ordinal);
                        var size = mesh.bounds.size;
                        if (!isDeck) node.localScale *= Mathf.Min(1f,2f / Mathf.Max(size.x,size.y,size.z));
                        if (node.name == "Crossbeam" || node.name == "Bridge" || node.name == "Frame" ||
                            node.name == "BrokenBridge" || node.name == "CraneBoom") node.localRotation = Quaternion.identity;
                        // Art-pack placements describe prop centers. Imported FBX local pivots do not.
                        node.localPosition -= node.localRotation * Vector3.Scale(mesh.bounds.center,node.localScale);
                        records.Add(path + "/" + node.name + ": position " + previousPosition.ToString("F4") + " -> " + node.localPosition.ToString("F4") +
                            "; rotation " + previousRotation.ToString("F3") + " -> " + node.localEulerAngles.ToString("F3") +
                            "; scale " + previousScale.ToString("F4") + " -> " + node.localScale.ToString("F4"));
                    }
                    if (contents.name.StartsWith("CapacitorField",StringComparison.Ordinal))
                        foreach (var name in new[] { "Cap2", "Coil2" })
                        {
                            var node = contents.transform.Find(name); var previous = node.localPosition;
                            node.localPosition += new Vector3(4.6f,0,1.2f);
                            records.Add(path + "/" + name + ": center capacitor moved to outer bank: " + previous.ToString("F4") + " -> " + node.localPosition.ToString("F4"));
                        }
                    PrefabUtility.SaveAsPrefabAsset(contents,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            File.WriteAllLines(recordPath,records);
            Configure();
        }

        public static void FinalizeComposition()
        {
            var records = new List<string>();
            var structuralMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/ArtSpike/Materials/Gravivore_ProxyMetal_PBR.mat");
            foreach (var path in Directory.GetFiles(PackRoot + "Environment","*.prefab").OrderBy(p => p))
            {
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var renderer in contents.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var node = renderer.transform;
                        if (node.name.StartsWith("Deck",StringComparison.Ordinal))
                        {
                            var position = node.localPosition; position.y = contents.name.StartsWith("RepairHub",StringComparison.Ordinal) ? .22f : .08f;
                            records.Add(path + "/" + node.name + ": deck above visual floor: " + node.localPosition.ToString("F4") + " -> " + position.ToString("F4"));
                            node.localPosition = position;
                            if (contents.name.StartsWith("BossArena",StringComparison.Ordinal))
                            { var scale = node.localScale; scale.z = 3; node.localScale = scale; records.Add(path + "/" + node.name + ": deck z scale = 3; stays within north gameplay boundary."); }
                        }
                        if (node.name.Contains("Bridge") || node.name.Contains("beam") || node.name.Contains("Heavy") ||
                            node.name == "Frame" || node.name.StartsWith("Pylon",StringComparison.Ordinal) || node.name.StartsWith("Crane",StringComparison.Ordinal))
                        {
                            renderer.sharedMaterials = Enumerable.Repeat(structuralMaterial,renderer.sharedMaterials.Length).ToArray();
                            records.Add(path + "/" + node.name + ": shared worn-metal structural material; hostile accent retained only on coils/cores.");
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(contents,path);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            File.WriteAllLines(ReviewRoot + "/FINAL_COMPOSITION_OFFSETS.txt",records);
            Configure();
        }

        public static void FinishDeckSurfacesAndConfigure()
        {
            const string path = "Assets/_Game/Content/Materials/Phase3C_WorkDeck.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Content/Materials/Phase3C_IndustrialFloor.mat"));
                material.name = "Phase3C_WorkDeck"; AssetDatabase.CreateAsset(material,path);
            }
            material.color = new Color(.3f,.33f,.35f); material.mainTextureScale = new Vector2(3,3);
            EditorUtility.SetDirty(material); AssetDatabase.SaveAssets();
            foreach (var prefabPath in Directory.GetFiles(PackRoot + "Environment","*.prefab").OrderBy(p => p))
            {
                var contents = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    foreach (var renderer in contents.GetComponentsInChildren<MeshRenderer>(true))
                        if (renderer.name.StartsWith("Deck",StringComparison.Ordinal)) renderer.sharedMaterial = material;
                    PrefabUtility.SaveAsPrefabAsset(contents,prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            Configure();
        }

        private static void BindRecipe(SerializedObject catalog, string id, string prefab, float scale, Vector3 position)
        {
            var recipes = catalog.FindProperty("_enemies");
            for (var i = 0; i < recipes.arraySize; i++)
            {
                var recipe = recipes.GetArrayElementAtIndex(i);
                if (recipe.FindPropertyRelative("_id").stringValue != id) continue;
                recipe.FindPropertyRelative("_presentationPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PackRoot + "Enemies/" + prefab + "_Phase3B.prefab");
                recipe.FindPropertyRelative("_localScale").vector3Value = Vector3.one * scale;
                recipe.FindPropertyRelative("_localPosition").vector3Value = position;
                return;
            }
            throw new InvalidOperationException("Missing canonical recipe: " + id);
        }
        private static void Binding(SerializedProperty property, string path, Vector3 position, Vector3 scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Missing Phase3C candidate: " + path);
            property.FindPropertyRelative("_prefab").objectReferenceValue = prefab;
            property.FindPropertyRelative("_localPosition").vector3Value = position;
            property.FindPropertyRelative("_localEulerAngles").vector3Value = Vector3.zero;
            property.FindPropertyRelative("_localScale").vector3Value = scale;
        }
        private static void AlignHubAnchors(ChapterVisualEnvironment environment)
        {
            var visual = AssetDatabase.LoadAssetAtPath<GameObject>(PackRoot + "Environment/RepairHub_Phase3B.prefab").transform;
            var hub = environment.GetRegion("repair-hub").Root;
            var anchors = new[] { "ManipulatorLeft", "ManipulatorRight", "RearManipulatorA", "RearManipulatorB",
                "RepairBeamOriginLeft", "RepairBeamOriginRight", "RepairBeamOriginRearA", "RepairBeamOriginRearB", "AmbientFxRoot" };
            var references = new[] { "ManipulatorMount_L", "ManipulatorMount_R", "RearManipulatorMount_A", "RearManipulatorMount_B",
                "BeamEmitter_L", "BeamEmitter_R", "BeamEmitter_RearA", "BeamEmitter_RearB", "AmbientFxVisualRoot" };
            for (var i = 0; i < anchors.Length; i++)
            {
                hub.Find(anchors[i]).localPosition = visual.Find(references[i]).localPosition + new Vector3(0,-.2f,0);
                PrefabUtility.RecordPrefabInstancePropertyModifications(hub.Find(anchors[i]));
            }
            // ServicePoint (.2m) + model (-.2m) matches the unchanged gameplay spawn / DockPoint.
        }
        private static void BuildFloorAndRoutes(ChapterVisualEnvironment environment)
        {
            var existing = environment.Floor.Find("Phase3C Industrial Floor");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            const string meshPath = "Assets/_Game/Content/Prefabs/Integration/Chapter01_IndustrialFloor.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null) { mesh = new Mesh { name = "Chapter01 Industrial Floor" }; AssetDatabase.CreateAsset(mesh, meshPath); }
            mesh.Clear();
            mesh.vertices = new[] { new Vector3(-36,-.03f,-40),new Vector3(-36,-.03f,100),new Vector3(36,-.03f,100),new Vector3(36,-.03f,-40) };
            mesh.uv = new[] { Vector2.zero,new Vector2(0,28),new Vector2(14.4f,28),new Vector2(14.4f,0) };
            mesh.triangles = new[] { 0,1,2,0,2,3 }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            const string materialPath = "Assets/_Game/Content/Materials/Phase3C_IndustrialFloor.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/ArtSpike/Materials/Gravivore_ProxyMetal_PBR.mat"));
                material.name = "Phase3C_IndustrialFloor"; AssetDatabase.CreateAsset(material,materialPath);
            }
            material.color = new Color(.22f,.26f,.28f); material.SetFloat("_Metallic",.1f); material.SetFloat("_Smoothness",.2f); EditorUtility.SetDirty(material);
            var floor = new GameObject("Phase3C Industrial Floor",typeof(MeshFilter),typeof(MeshRenderer));
            floor.transform.SetParent(environment.Floor,false); floor.GetComponent<MeshFilter>().sharedMesh = mesh; floor.GetComponent<Renderer>().sharedMaterial = material;
            // Route surfaces use the same mesh/material vocabulary and never contain collision.
            var routeRoot = environment.Floor.Find("Phase3C Routes");
            if (routeRoot != null) UnityEngine.Object.DestroyImmediate(routeRoot.gameObject);
            routeRoot = new GameObject("Phase3C Routes").transform; routeRoot.SetParent(environment.Floor,false);
            var deck = AssetDatabase.LoadAssetAtPath<GameObject>(PackRoot + "Environment/RelayYard_Phase3B.prefab").transform.Find("Deck");
            for (var z = -22; z <= 88; z += 10)
            {
                var panel = UnityEngine.Object.Instantiate(deck.gameObject,routeRoot,false); panel.name = "Service corridor panel " + z;
                panel.transform.localPosition = new Vector3(0,.008f,z); panel.transform.localRotation = Quaternion.identity; panel.transform.localScale = new Vector3(2,1,4);
            }
            for (var side = -1; side <= 1; side += 2)
                for (var z = -10; z <= 40; z += 25)
                    for (var x = 8; x <= 24; x += 8)
                    {
                        var panel = UnityEngine.Object.Instantiate(deck.gameObject,routeRoot,false); panel.name = "Branch service panel";
                        panel.transform.localPosition = new Vector3(side * x,.008f,z); panel.transform.localScale = new Vector3(4,1,2);
                    }
            var edgeRoot = environment.Structures.Find("Phase3C Service Infrastructure");
            if (edgeRoot != null) UnityEngine.Object.DestroyImmediate(edgeRoot.gameObject);
            edgeRoot = new GameObject("Phase3C Service Infrastructure").transform; edgeRoot.SetParent(environment.Structures,false);
            var pipe = AssetDatabase.LoadAssetAtPath<GameObject>(PackRoot + "Environment/BossApproach_Phase3B.prefab").transform.Find("PipeL");
            var cabinet = AssetDatabase.LoadAssetAtPath<GameObject>(PackRoot + "Environment/RelayYard_Phase3B.prefab").transform.Find("CabinetL");
            for (var side = -1; side <= 1; side += 2)
                for (var z = -18; z <= 46; z += 16)
                {
                    PlaceCentered(pipe,edgeRoot,"Peripheral power trunk",new Vector3(side * 8,.35f,z),new Vector3(1,1,5),Quaternion.identity);
                    PlaceCentered(cabinet,edgeRoot,"Peripheral service cabinet",new Vector3(side * 10,.65f,z + 4),new Vector3(.55f,.55f,.55f),Quaternion.Euler(0,side * 90,0));
                }
        }
        private static void PlaceCentered(Transform source,Transform parent,string name,Vector3 position,Vector3 scale,Quaternion rotation)
        {
            var prop = UnityEngine.Object.Instantiate(source.gameObject,parent,false); prop.name = name;
            prop.transform.localScale = scale; prop.transform.localRotation = rotation;
            prop.transform.localPosition = position - rotation * Vector3.Scale(prop.GetComponent<MeshFilter>().sharedMesh.bounds.center,scale);
        }
    }
}
