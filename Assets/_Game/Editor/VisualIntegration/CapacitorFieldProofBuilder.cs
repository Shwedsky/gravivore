using System;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    public static class CapacitorFieldProofBuilder
    {
        public const string Root = "Assets/_Game/Content/CapacitorFieldProof";
        public const string Model = Root + "/Models/CapacitorField.fbx";
        public const string Prefab = Root + "/Prefabs/CapacitorField.prefab";
        public const string Profile = Root + "/Materials/CF_PostProcessing.asset";
        public const string Output = "docs/history/implementation-passes/capacitor-field-proof/verification";
        public static bool IsInstalled => AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) != null;

        public static void Build()
        {
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(Root + "/Materials"); Directory.CreateDirectory(Root + "/Prefabs");
            AssetDatabase.Refresh(); UrpConfigurator.ConfigureUrp();
            BuildMaterials(); BuildPrefab();
            var scene = EditorSceneManager.OpenScene(Chapter01BlueprintWorldBuilder.ScenePath);
            var composition = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var sector = composition.VisualEnvironment.GetRegion("capacitor-field").Root;
            foreach (var child in sector.Cast<Transform>().Where(t => t.name == "CapacitorField / authored industrial sector").ToArray())
                Object.DestroyImmediate(child.gameObject);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), sector);
            instance.name = "CapacitorField / authored industrial sector";
            foreach (var node in instance.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(node.gameObject, StaticEditorFlags.BatchingStatic);
            RebuildFootprints();
            var camera = AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.Camera.CameraFollowSettings>("Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            var settings = new SerializedObject(camera);
            settings.FindProperty("_offset").vector3Value = new Vector3(3.4f,19.8f,-13.6f);
            settings.FindProperty("_lookAtHeight").floatValue = 3.2f;
            settings.FindProperty("_fieldOfView").floatValue = 48;
            settings.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(camera);
            PlayerSettings.colorSpace = ColorSpace.Linear;
            foreach (var light in composition.VisualEnvironment.Lighting.GetComponentsInChildren<Light>(true))
            {
                if (light.name.StartsWith("capacitor-field /",StringComparison.Ordinal))
                {
                    light.intensity = light.name.Contains("machinery") ? 28 : 16;
                    if (!light.name.Contains("machinery")) light.color = new Color(1f,.43f,.16f);
                    light.range = light.name.Contains("machinery") ? 9 : 6.5f;
                    light.transform.position = sector.position + (light.name.Contains("machinery") ? new Vector3(-3,3.6f,5) : new Vector3(6,2.5f,-1));
                }
            }
            var key = composition.VisualEnvironment.KeyLight;
            key.intensity = 1.65f; key.color = new Color(.80f,.87f,1f); key.shadows = LightShadows.Soft;
            RenderSettings.ambientSkyColor = new Color(.26f,.30f,.36f);
            RenderSettings.ambientEquatorColor = new Color(.12f,.15f,.20f);
            RenderSettings.ambientGroundColor = new Color(.055f,.066f,.088f);
            RenderSettings.reflectionIntensity = .65f;
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpConfigurator.UrpAssetPath);
            pipeline.supportsHDR = true; pipeline.msaaSampleCount = 1; pipeline.renderScale = 1;
            BuildPostProcessing(composition);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            ValidateOrThrow(); Chapter01BlueprintWorldBuilder.Audit();
            Debug.Log("CAPACITOR_FIELD_PROOF_AUTHORED");
        }
        private static void BuildMaterials()
        {
            foreach (var path in Directory.GetFiles(Root+"/Textures","*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = path.EndsWith("_Normal.png",StringComparison.Ordinal) ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = path.EndsWith("_BaseColor.png",StringComparison.Ordinal);
                importer.maxTextureSize = 512; importer.mipmapEnabled = true; importer.isReadable = false;
                importer.wrapMode = path.Contains("DonorPalette") ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                var android = importer.GetPlatformTextureSettings("Android"); android.overridden = true;
                android.maxTextureSize = 512; android.format = TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(android); importer.SaveAndReimport();
            }
            foreach (var name in new[] {"Graphite","Shell","Steel","Deck","Ceramic","Warning","Coil","Cold","Hot","DonorPalette"})
            {
                var path = Root+"/Materials/CF_"+name+".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material,path); }
                foreach (var map in new[] {("_BaseMap","BaseColor"),("_BumpMap","Normal"),("_MetallicGlossMap","MetallicSmoothness"),("_OcclusionMap","Occlusion")})
                {
                    var source = name == "DonorPalette" && map.Item1 != "_BaseMap" ? "Steel" : name;
                    material.SetTexture(map.Item1,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/CF_"+source+"_"+map.Item2+".png"));
                }
                material.SetColor("_BaseColor",Color.white); material.SetFloat("_BumpScale",.45f);
                material.SetFloat("_Smoothness",.65f); material.SetFloat("_OcclusionStrength",1);
                material.EnableKeyword("_NORMALMAP"); material.EnableKeyword("_METALLICSPECGLOSSMAP"); material.EnableKeyword("_OCCLUSIONMAP");
                if (name == "Cold" || name == "Hot")
                {
                    material.EnableKeyword("_EMISSION"); material.SetTexture("_EmissionMap",material.GetTexture("_BaseMap"));
                    material.SetColor("_EmissionColor",name == "Cold" ? new Color(.10f,.85f,2.2f) : new Color(2.2f,.18f,.025f));
                }
                else {material.DisableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.black);}
                material.enableInstancing = true; EditorUtility.SetDirty(material);
            }
        }
        private static void BuildPrefab()
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
            importer.animationType = ModelImporterAnimationType.None; importer.bakeAxisConversion = true;
            importer.isReadable = false; importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard; importer.SaveAndReimport();
            var owner = new GameObject("Capacitor Field / pressure bank and distribution");
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model),owner.transform,false);
            foreach (var renderer in owner.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m =>
                    AssetDatabase.LoadAssetAtPath<Material>(m.name.StartsWith("R1_Trim",StringComparison.Ordinal)
                    ? Chapter01BlueprintWorldBuilder.Root+"/Materials/"+m.name+".mat" : Root+"/Materials/"+m.name+".mat")
                    ?? throw new InvalidOperationException("Missing CF authored material "+m.name)).ToArray();
                renderer.shadowCastingMode = ShadowCastingMode.On;
            }
            PrefabUtility.SaveAsPrefabAsset(owner,Prefab); Object.DestroyImmediate(owner);
        }
        private static void RebuildFootprints()
        {
            var layout = AssetDatabase.LoadAssetAtPath<ChapterWorldLayoutDefinition>(Chapter01BlueprintWorldBuilder.Root+"/Chapter01WorldLayout.asset");
            var settings = new SerializedObject(layout); var blockers = settings.FindProperty("_blockers");
            for (var i=0;i<blockers.arraySize;i++)
            {
                var entry = blockers.GetArrayElementAtIndex(i); var id = entry.FindPropertyRelative("_id").stringValue;
                if (id == "capacitor-west-banks")
                {entry.FindPropertyRelative("_center").vector3Value = new Vector3(-21.0f,2f,8); entry.FindPropertyRelative("_size").vector3Value = new Vector3(3.0f,4f,13.6f);}
                if (id == "capacitor-rear-distributor")
                {entry.FindPropertyRelative("_center").vector3Value = new Vector3(-15,3.8f,17.2f); entry.FindPropertyRelative("_size").vector3Value = new Vector3(19.2f,7.6f,6.0f);}
            }
            // One coarse footprint per machine bank; small decorative ribs are
            // not colliders. The same authority drives the minimap at runtime.
            foreach (var (id,z) in new[] {("capacitor-east-transformers",5.8f),("capacitor-east-upper-transformer",13f)})
            {
                var index = Enumerable.Range(0,blockers.arraySize).Where(i => blockers.GetArrayElementAtIndex(i).FindPropertyRelative("_id").stringValue == id).DefaultIfEmpty(-1).First();
                if (index < 0) { index=blockers.arraySize; blockers.arraySize++; }
                var extra=blockers.GetArrayElementAtIndex(index); extra.FindPropertyRelative("_id").stringValue=id;
                extra.FindPropertyRelative("_center").vector3Value=new Vector3(-9f,2f,z);
                extra.FindPropertyRelative("_size").vector3Value=new Vector3(2.65f,4f,3.25f);
            }
            settings.ApplyModifiedPropertiesWithoutUndo(); layout.ValidateOrThrow(); EditorUtility.SetDirty(layout);
        }
        private static void BuildPostProcessing(S01SceneCompositionRoot root)
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Profile);
            if (profile == null) { profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Profile); }
            T Get<T>() where T:VolumeComponent => profile.TryGet<T>(out var component) ? component : Add<T>(profile);
            var bloom=Get<Bloom>();bloom.threshold.Override(1.25f);bloom.intensity.Override(.12f);bloom.scatter.Override(.45f);bloom.highQualityFiltering.Override(false);
            var tone=Get<Tonemapping>();tone.mode.Override(TonemappingMode.ACES);
            var grade=Get<ColorAdjustments>();grade.postExposure.Override(.10f);grade.contrast.Override(8);grade.saturation.Override(-4);
            foreach (var volume in root.GetComponentsInChildren<Volume>(true)) Object.DestroyImmediate(volume.gameObject);
            var node=new GameObject("Production restrained post processing",typeof(Volume));node.transform.SetParent(root.transform,false);
            var global=node.GetComponent<Volume>();global.isGlobal=true;global.priority=10;global.sharedProfile=profile;
            EditorUtility.SetDirty(profile);
        }
        private static T Add<T>(VolumeProfile profile) where T:VolumeComponent
        {var component=profile.Add<T>(true);AssetDatabase.AddObjectToAsset(component,profile);return component;}
        public static void ValidateOrThrow()
        {
            ValidateCameraOrThrow();
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpConfigurator.RendererDataPath);
            if (renderer?.postProcessData == null) throw new InvalidOperationException("CF_RENDER postProcessData missing.");
            if (PlayerSettings.colorSpace != ColorSpace.Linear) throw new InvalidOperationException("CF_RENDER requires Linear PBR.");
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            if (prefab==null || prefab.GetComponentsInChildren<Collider>(true).Length>0 || prefab.GetComponentsInChildren<MonoBehaviour>(true).Length>0)
                throw new InvalidOperationException("CF_ENV authored presentation must have independent gameplay authority.");
            foreach (var item in prefab.GetComponentsInChildren<Renderer>(true)) Rendering.RenderingBuildAudit.ValidateRenderer(item);
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Profile);
            if (profile==null || !profile.TryGet<Bloom>(out var bloom) || bloom.intensity.value>.2f || !profile.TryGet<Tonemapping>(out var tone) || tone.mode.value!=TonemappingMode.ACES)
                throw new InvalidOperationException("CF_RENDER restrained bloom/ACES contract invalid.");
            Directory.CreateDirectory(Output); File.WriteAllText(Output+"/render-contract.json",JsonUtility.ToJson(new Contract
            {linear=true,postProcessData=true,bloom=bloom.intensity.value,rendererFeatures=renderer.rendererFeatures.Count},true));
        }
        [Serializable] private sealed class Contract {public bool linear,postProcessData;public float bloom;public int rendererFeatures;}
        public static void ValidateCameraOrThrow()
        {
            var camera=AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.Camera.CameraFollowSettings>("Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            if(camera==null)throw new InvalidOperationException("CF_CAMERA settings missing.");
            var o=camera.Offset;
            foreach(var v in new[]{o.x,o.y,o.z,camera.LookAtHeight,camera.FieldOfView,camera.PositionDamping})
                if(float.IsNaN(v)||float.IsInfinity(v))throw new InvalidOperationException("CF_CAMERA nonfinite value.");
            if(o.x<2||o.x>5||o.y<18||o.y>22||o.z< -16||o.z> -12||camera.LookAtHeight<2||camera.LookAtHeight>4||camera.FieldOfView<44||camera.FieldOfView>52||camera.PositionDamping<=0||camera.PositionDamping>.5f)
                throw new InvalidOperationException("CF_CAMERA outside reviewed portrait composition bands.");
        }
    }
}
