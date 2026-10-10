using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class ConceptCorrectiveV45Audit : IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        public int callbackOrder=>650;
        [Serializable] private sealed class Budget
        {public bool validated=true;public int sceneRenderers,enabledRenderers,correctiveRenderers,allVR3Renderers,correctiveMaterials,authoredLights,spawnDocks,blockers;public long enabledStaticTriangles;public string runtimeLights="Existing two reused unshadowed local lights plus existing key light";}
        [Serializable] private sealed class Packed {public bool validated=true;public string apkSha256;public string[] requiredResources,staticSectorMeshes;}
        public static void ValidateOrThrow()
        {
            if (Chapter01BlueprintWorldBuilder.IsBlueprintWorld()) { Chapter01BlueprintWorldBuilder.Audit(); return; }
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
                var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
                var env=root.VisualEnvironment;var layer=env.Floor.Find(ConceptCorrectiveV45Builder.Layer);
                if(layer==null||!layer.gameObject.activeInHierarchy||layer.GetComponentsInChildren<Collider>(true).Length!=0||layer.GetComponentsInChildren<MonoBehaviour>(true).Length!=0||layer.GetComponentsInChildren<Light>(true).Length!=0)
                    throw new InvalidOperationException("V45 presentation layer is missing or owns forbidden authority/lights.");
                var settings=(EnemyAmbientMotionSettings)new SerializedObject(root).FindProperty("_enemyAmbientMotionSettings").objectReferenceValue;
                if(settings==null)throw new InvalidOperationException("V45 ambient motion settings are not injected.");settings.ValidateOrThrow();
                if(settings.Parameters.Radius>1.2f||settings.Parameters.Speed>1)throw new InvalidOperationException("V45 ambient movement exceeds the bounded local patrol budget.");
                var rs=root.GetComponentsInChildren<Renderer>(true);var own=layer.GetComponentsInChildren<Renderer>(true);
                foreach(var r in own){Rendering.RenderingBuildAudit.ValidateRenderer(r);foreach(var m in r.sharedMaterials)Rendering.RenderingBuildAudit.ValidateMaterial(m,r.name);}
                var docks=layer.GetComponentsInChildren<Transform>(true).Count(t=>t.name.StartsWith("Serviced enemy dock ",StringComparison.Ordinal)||t.name.StartsWith("Heavy fabrication nest ",StringComparison.Ordinal));
                if(docks!=9)throw new InvalidOperationException("Every ordinary and strong spot requires a local serviced origin.");
                if(File.ReadAllText(ConceptCorrectiveV45Builder.Output+"/collision_fingerprint.txt")!=VisualReplacementV3Builder.AuthorityFingerprint(env))throw new InvalidOperationException("Authored V45 collision receipt differs from the live scene.");
                var budget=new Budget{sceneRenderers=rs.Length,enabledRenderers=rs.Count(r=>r.enabled&&r.gameObject.activeInHierarchy),correctiveRenderers=own.Length,allVR3Renderers=rs.Count(r=>r.name.StartsWith("VR3_",StringComparison.Ordinal)),correctiveMaterials=own.SelectMany(r=>r.sharedMaterials).Distinct().Count(),authoredLights=root.GetComponentsInChildren<Light>(true).Length,spawnDocks=docks,blockers=env.SliceObstacleCount};
                foreach(var r in rs.Where(r=>r.enabled&&r.gameObject.activeInHierarchy))
                {var filter=r.GetComponent<MeshFilter>();if(filter==null)continue;var mesh=filter.sharedMesh;if(mesh==null)continue;for(var i=0;i<mesh.subMeshCount;i++)budget.enabledStaticTriangles+=(long)mesh.GetIndexCount(i)/3;}
                File.WriteAllText(ConceptCorrectiveV45Builder.Output+"/mobile_render_budget.json",JsonUtility.ToJson(budget,true));
            }
            finally
            {if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);}
            Debug.Log("CONCEPT_CORRECTIVE_V45_RENDER_MOTION_ECOLOGY_PASS");
        }
        public void OnPreprocessBuild(BuildReport report)=>ValidateOrThrow();
        public void OnPostprocessBuild(BuildReport report)
        {
            if (Chapter01BlueprintWorldBuilder.IsBlueprintWorld()) return;
            var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true).Where(p=>p.StartsWith(ConceptCorrectiveV45Builder.Root+"/",StringComparison.Ordinal)).ToArray();
            var resources=dependencies.Where(p=>!p.StartsWith(ConceptCorrectiveV45Builder.Root+"/Meshes/",StringComparison.Ordinal)).ToArray();
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            foreach(var p in resources)if(!packed.Contains(p))throw new BuildFailedException("V45 resource omitted from build report: "+p);
            using(var stream=File.OpenRead(report.summary.outputPath))using(var hash=SHA256.Create())
                File.WriteAllText(ConceptCorrectiveV45Builder.Output+"/apk_corrective_dependencies.json",JsonUtility.ToJson(new Packed{apkSha256=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant(),requiredResources=resources,staticSectorMeshes=dependencies.Except(resources).ToArray()},true));
        }
    }
}
