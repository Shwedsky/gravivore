using System;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Assets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public static class ConceptConvergenceV47Builder
    {
        public const string Root="Assets/_Game/Content/ConceptConvergenceV47";
        public const string Output="docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/verification";
        public const string Layer="Chapter 01 Concept Convergence V47";
        [Serializable] private sealed class Metrics { public int renderers,materials,lights,textureCount; public long staticTriangles,textureBytes; public Actor[] actors; }
        [Serializable] private sealed class Actor { public string name; public long[] lodTriangles; public int bones; }
        public static readonly (string folder,string name)[] Actors={ ("VisualSlice","Scout_V1"),("VisualSlice","Cutter_V1"),("Chapter01Production","Warden_V1"),("Chapter01Production","ArcDrone_V1"),("Chapter01Production","Carrier_V1"),("VisualSlice","Magnetar_V1"),("Chapter01V3","Custodian_V3") };
        public static void Baseline()
        {
            Directory.CreateDirectory(Output);
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            File.WriteAllText(Output+"/v46_collision_fingerprint.txt",VisualReplacementV3Builder.AuthorityFingerprint(root.VisualEnvironment));
            Record(root,"v46");
            Debug.Log("V47_BASELINE_RECORDED");
        }
        private static void Record(S01SceneCompositionRoot root,string phase)
        {
            var rs=root.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
            var textures=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true).Select(p=>AssetDatabase.LoadAssetAtPath<Texture>(p)).Where(t=>t!=null).Distinct().ToArray();
            var m=new Metrics{renderers=rs.Length,materials=rs.SelectMany(r=>r.sharedMaterials).Distinct().Count(),lights=root.GetComponentsInChildren<Light>(true).Length,textureCount=textures.Length,textureBytes=textures.Sum(t=>UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t))};
            foreach(var r in rs.OfType<MeshRenderer>()){var mesh=r.GetComponent<MeshFilter>()?.sharedMesh;if(mesh!=null)for(var i=0;i<mesh.subMeshCount;i++)m.staticTriangles+=(long)mesh.GetIndexCount(i)/3;}
            m.actors=Actors.Select(pair=>{var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/"+pair.folder+"/Prefabs/"+pair.name+".prefab");return new Actor{name=pair.name,lodTriangles=p.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(r=>(long)r.sharedMesh.GetIndexCount(0)/3).ToArray(),bones=p.GetComponentInChildren<SkinnedMeshRenderer>().bones.Length};}).ToArray();
            File.WriteAllText(Output+"/"+phase+"_metrics.json",JsonUtility.ToJson(m,true));
        }
        public static void BuildActors()
        {
            AssetDatabase.Refresh();
            foreach(var pair in Actors)
            {
                var modelPath="Assets/_Game/Content/"+pair.folder+"/Models/"+pair.name+".fbx";
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                var path="Assets/_Game/Content/"+pair.folder+"/Prefabs/"+pair.name+".prefab";
                var obj=PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var transforms=obj.GetComponentsInChildren<Transform>(true).GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
                    var material=AssetDatabase.LoadAssetAtPath<Material>(SurfaceHeroV46Builder.Root+"/Materials/V46_Hero"+(pair.name=="Magnetar_V1"?"Amber":"Red")+".mat");
                    foreach(var r in obj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        var source=model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(s=>s.name==r.name);
                        r.sharedMesh=source.sharedMesh;r.bones=source.bones.Select(b=>transforms[b.name]).ToArray();
                        r.rootBone=source.rootBone!=null?transforms[source.rootBone.name]:r.rootBone;
                        r.localBounds=source.localBounds;r.sharedMaterials=new[]{material};
                    }
                    PrefabUtility.SaveAsPrefabAsset(obj,path);
                }
                finally{PrefabUtility.UnloadPrefabContents(obj);}
            }
            AssetDatabase.SaveAssets();AuditActors();
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            Record(scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single(),"v47_actors");
            Debug.Log("V47_ACTOR_INTEGRATION_PASS");
        }
        public static void AuditActors()
        {
            foreach(var pair in Actors)
            {
                var obj=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/"+pair.folder+"/Prefabs/"+pair.name+".prefab");
                PresentationPrefabValidation.ValidateOrThrow(obj);
                foreach(var r in obj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if(r.sharedMesh==null||r.bones.Any(b=>b==null)||r.sharedMesh.subMeshCount!=1)throw new InvalidOperationException("Broken V47 skin: "+pair.name);
                    var m=r.sharedMaterial;
                    foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap","_EmissionMap"})
                        if(m.GetTexture(map)==null)throw new InvalidOperationException("V47 actor lost PBR map "+pair.name+" "+map);
                    Rendering.RenderingBuildAudit.ValidateMaterial(m,pair.name);
                }
            }
        }
        public static void Build()=>BuildActors();
        public static void Audit()=>AuditActors();
    }
}
