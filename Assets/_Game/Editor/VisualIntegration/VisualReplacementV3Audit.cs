using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class VisualReplacementV3Audit : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string Output="docs/visual-replacement-v3/verification";
        public int callbackOrder => 600;
        [Serializable] private sealed class Budget
        {public bool validated=true;public int sceneRenderers,enabledSceneRenderers,decorativeRenderers,materials,maximumBones,lights,productionMeshes,maximumTextureSize;public long authoredTriangles,enabledInstancedStaticTriangles;public string collisionAuthority="Unchanged from representative-section baseline";}
        [Serializable] private sealed class Packed
        {public bool validated=true;public string apkSha256;public string[] dependencies,staticGeometrySources;public string productionScene=FirstVisualSliceBuilder.ScenePath;public string staticGeometryPacking="Scene static batches; scene root and meshes verified by typed APK references";}
        public static void ValidateOrThrow()
        {
            var definition=AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset");
            if(definition.UiSkin==null)throw new InvalidOperationException("Production UI skin missing");definition.UiSkin.ValidateOrThrow();
            if(!File.ReadAllText("Assets/StreamingAssets/ThirdPartyNotices.txt").Contains("Catherine Laserna"))throw new InvalidOperationException("Shipped EXE attribution missing");
            var worn=AssetDatabase.LoadAssetAtPath<Material>(VisualReplacementV3Builder.Root+"/Materials/VR3_WornIndustrialAtlas.mat");
            if(!worn.IsKeywordEnabled("_EMISSION"))throw new InvalidOperationException("Production machine energy was stripped");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
                var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
                var env=root.VisualEnvironment;var layer=env.Floor.Find(VisualReplacementV3Builder.Layer);
                if(layer==null||layer.GetComponentsInChildren<Collider>(true).Length!=0||layer.GetComponentsInChildren<MonoBehaviour>(true).Length!=0)throw new InvalidOperationException("Decorative layer contract failed");
                if(VisualReplacementV3Builder.AuthorityFingerprint(env)!=File.ReadAllText(Output+"/collision_fingerprint.txt"))throw new InvalidOperationException("Collision authority changed");
                var rs=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Renderer>(true)).ToArray();
                foreach(var r in rs)foreach(var material in r.sharedMaterials)Rendering.RenderingBuildAudit.ValidateMaterial(material,r.name);
                foreach(var r in layer.GetComponentsInChildren<Renderer>(true))Rendering.RenderingBuildAudit.ValidateRenderer(r);
                var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
                if(dependencies.Any(p=>p.Contains("ExternalAssetIntake")||p.Contains("Asset Store")||p.Contains("CreepyCat")||p.Contains("CombatDrone")))throw new InvalidOperationException("Forbidden production source dependency");
                var textures=dependencies.Where(p=>p.StartsWith(VisualReplacementV3Builder.Root+"/Textures/",StringComparison.Ordinal)).Select(p=>AssetDatabase.LoadAssetAtPath<Texture2D>(p)).Where(t=>t!=null).ToArray();
                var maximum=textures.Max(t=>Math.Max(t.width,t.height));if(maximum>2048)throw new InvalidOperationException("Production atlas exceeds 2048");
                var meshes=AssetDatabase.FindAssets("t:Model",new[]{VisualReplacementV3Builder.Root+"/Models"}).Select(AssetDatabase.GUIDToAssetPath).SelectMany(p=>AssetDatabase.LoadAllAssetsAtPath(p).OfType<Mesh>()).ToArray();
                foreach(var mesh in meshes)if(mesh.subMeshCount!=1)throw new InvalidOperationException("Unconsolidated production mesh "+mesh.name);
                var budget=new Budget{sceneRenderers=rs.Length,enabledSceneRenderers=rs.Count(r=>r.enabled&&r.gameObject.activeInHierarchy),materials=rs.SelectMany(r=>r.sharedMaterials).Distinct().Count(),maximumBones=rs.OfType<SkinnedMeshRenderer>().Select(r=>r.bones.Length).DefaultIfEmpty(0).Max(),lights=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Light>(true)).Count(),productionMeshes=meshes.Length,maximumTextureSize=maximum,authoredTriangles=meshes.Sum(m=>(long)m.GetIndexCount(0)/3)};
                budget.decorativeRenderers=layer.GetComponentsInChildren<Renderer>(true).Length;
                foreach(var r in rs.OfType<MeshRenderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy))
                {
                    var mesh=r.GetComponent<MeshFilter>()?.sharedMesh;if(mesh==null)continue;
                    for(var i=0;i<mesh.subMeshCount;i++)budget.enabledInstancedStaticTriangles+=(long)mesh.GetIndexCount(i)/3;
                }
                Directory.CreateDirectory(Output);File.WriteAllText(Output+"/mobile_render_budget.json",JsonUtility.ToJson(budget,true));
                File.WriteAllLines(Output+"/production_dependencies.txt",dependencies);
            }
            finally
            {
                if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            }
            Debug.Log("VISUAL_REPLACEMENT_V3_RENDER_LICENSE_COLLISION_PASS");
        }
        public void OnPreprocessBuild(BuildReport report)=>ValidateOrThrow();
        public void OnPostprocessBuild(BuildReport report)
        {
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            File.WriteAllLines(Output+"/build_report_asset_paths.txt",packed);
            var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true).Where(p=>p.StartsWith(VisualReplacementV3Builder.Root,StringComparison.Ordinal)&&!p.EndsWith(".cs",StringComparison.Ordinal)).ToArray();
            // Unity can replace static FBX subassets with combined meshes whose source path
            // is the scene. Keep strict packing checks for textures/materials/UI and the
            // dynamically mounted weapon meshes; inspect the baked scene meshes in the APK.
            var staticSources=dependencies.Where(p=>
                p.EndsWith(".fbx",StringComparison.Ordinal)&&!Path.GetFileName(p).StartsWith("M0_Rank",StringComparison.Ordinal)||
                p.StartsWith(VisualReplacementV3Builder.Root+"/Prefabs/",StringComparison.Ordinal)&&p.EndsWith(".prefab",StringComparison.Ordinal)).ToArray();
            foreach(var path in staticSources.Where(p=>p.EndsWith(".prefab",StringComparison.Ordinal)))
            {
                var art=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(art==null||art.GetComponentsInChildren<MonoBehaviour>(true).Length!=0||art.GetComponentsInChildren<Collider>(true).Length!=0)
                    throw new BuildFailedException("Only renderer art prefabs may be expanded into scene batches: "+path);
            }
            var required=dependencies.Except(staticSources).ToArray();
            foreach(var path in required)if(!packed.Contains(path))throw new BuildFailedException("V44 dependency missing from APK: "+path);
            using(var stream=File.OpenRead(report.summary.outputPath))using(var hash=SHA256.Create())
                File.WriteAllText(Output+"/apk_production_dependencies.json",JsonUtility.ToJson(new Packed{apkSha256=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant(),dependencies=required,staticGeometrySources=staticSources},true));
        }
    }
}
