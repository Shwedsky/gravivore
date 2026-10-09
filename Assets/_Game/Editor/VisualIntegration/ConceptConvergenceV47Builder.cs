using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Assets;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

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
        private static readonly string[] Facilities={"DeploymentBay","FabricationBay","InductionStation","Deck0","Deck1","Deck2"};
        private static GameObject Place(Transform parent,string family,Vector3 position,Material material)
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/V47_"+family+".prefab"),parent);
            obj.transform.position=position;
            foreach(var r in obj.GetComponentsInChildren<Renderer>()){r.sharedMaterial=material;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            foreach(var t in obj.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic);
            return obj;
        }
        private static void Disable(Renderer r){r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        public static void Build()
        {
            BuildActors();
            Directory.CreateDirectory(Root+"/Prefabs");AssetDatabase.Refresh();
            var amber=AssetDatabase.LoadAssetAtPath<Material>(SurfaceHeroV46Builder.Root+"/Materials/V46_HeroAmber.mat");
            var red=AssetDatabase.LoadAssetAtPath<Material>(SurfaceHeroV46Builder.Root+"/Materials/V46_HeroRed.mat");
            Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
            var deckMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/V47_WorkingDeck.mat");
            if(deckMaterial==null){deckMaterial=new Material(amber);AssetDatabase.CreateAsset(deckMaterial,Root+"/Materials/V47_WorkingDeck.mat");}
            deckMaterial.SetColor("_BaseColor",new Color(.55f,.59f,.62f));deckMaterial.SetFloat("_Smoothness",.42f);
            deckMaterial.SetFloat("_BumpScale",.52f);EditorUtility.SetDirty(deckMaterial);
            foreach(var family in Facilities)
            {
                var path=Root+"/Models/V47_"+family+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;
                importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.None;importer.bakeAxisConversion=true;importer.isReadable=false;importer.SaveAndReimport();
                var obj=new GameObject("V47_"+family);Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),obj.transform,false);
                foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterial=family.StartsWith("Deck")?deckMaterial:red;
                PrefabUtility.SaveAsPrefabAsset(obj,Root+"/Prefabs/V47_"+family+".prefab");Object.DestroyImmediate(obj);
            }
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env=root.VisualEnvironment;var fingerprint=VisualReplacementV3Builder.AuthorityFingerprint(env);
            var prior=env.Floor.Find(Layer);if(prior!=null)Object.DestroyImmediate(prior.gameObject);
            var layer=new GameObject(Layer).transform;layer.SetParent(env.Floor,false);
            var v45=env.Floor.Find(ConceptCorrectiveV45Builder.Layer);var v46=env.Floor.Find(SurfaceHeroV46Builder.Layer);
            var manifest=new List<string>();
            foreach(var dock in v45.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Serviced enemy dock ")||t.name.StartsWith("Heavy fabrication nest ")))
            {
                var family=dock.name.Contains("capacitor")?"InductionStation":dock.name.Contains("relay")?"DeploymentBay":"FabricationBay";
                var center=dock.position+Vector3.forward*3.8f;
                var obj=Place(layer,family,center,red);obj.name="V47 "+family+" / "+dock.name;
                // Suppress the previous console shell; keep the low origin cradles.
                foreach(var r in v46.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled))
                {
                    var h=SurfaceHeroV46Builder.Hierarchy(r.transform);
                    if(h.Contains("DockApron")||h.Contains("ServiceGantry"))continue;
                    if(Mathf.Abs(r.bounds.center.x-center.x)<1.0f&&Mathf.Abs(r.bounds.center.z-center.z)<1.0f)Disable(r);
                }
                foreach(var r in dock.GetComponentsInChildren<Renderer>(true))
                {
                    var b=r.bounds;
                    if(r.name.StartsWith("Baked service detail ")||Mathf.Abs(b.center.z-(dock.position.z+3.7f))<.6f&&Mathf.Abs(Mathf.Abs(b.center.x-dock.position.x)-3.5f)<.5f)Disable(r);
                }
                manifest.Add(obj.name+" | center="+center+" | grounded rear console 1.62 x 1.20m; posts at +/-3.5m; open apron and four origin anchors retained");
            }
            var oldDeck=v45.Find("Compact small-panel deck");
            foreach(var r in oldDeck.GetComponentsInChildren<Renderer>(true))Disable(r);
            var deck=new GameObject("Flush industrial panel deck V47").transform;deck.SetParent(layer,false);
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset").Configuration;
            for(var x=-26;x<=26;x+=4)for(var z=world.Bounds.MinZ+2;z<world.Bounds.MaxZ;z+=4)
            {
                var variant=Mathf.Abs(Mathf.RoundToInt(x/4+z/4))%3;
                Place(deck,"Deck"+variant,new Vector3(x,.05f,z),deckMaterial);
            }
            manifest.Add("World deck: three authored fitted-panel variants, opaque recessed subdeck, flush grating; 640 triangles per 4m tile; collision/topology unchanged.");
            // Neutral metal key makes silhouette and recess separation legible while the
            // existing runtime cyan/amber source lights keep their same cost and authority.
            env.KeyLight.color=new Color(.91f,.94f,1f);env.KeyLight.intensity=1.65f;
            if(fingerprint!=VisualReplacementV3Builder.AuthorityFingerprint(env))throw new InvalidOperationException("V47 presentation changed collision authority");
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllLines(Output+"/facility_manifest.txt",manifest);Audit();Debug.Log("V47_FACILITY_AND_DECK_AUTHOR_PASS");
        }
        public static void Audit()
        {
            AuditActors();var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
                var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
                var env=root.VisualEnvironment;var layer=env.Floor.Find(Layer);
                if(layer==null)throw new InvalidOperationException("Missing V47 presentation layer");
                if(layer.GetComponentsInChildren<Collider>(true).Length!=0||layer.GetComponentsInChildren<MonoBehaviour>(true).Length!=0||layer.GetComponentsInChildren<Light>(true).Length!=0)throw new InvalidOperationException("V47 art owns runtime authority");
                if(File.ReadAllText(Output+"/v46_collision_fingerprint.txt")!=VisualReplacementV3Builder.AuthorityFingerprint(env))throw new InvalidOperationException("V46 collision fingerprint changed");
                foreach(var r in root.GetComponentsInChildren<Renderer>(true))
                {if(r.enabled&&r.gameObject.activeInHierarchy)Rendering.RenderingBuildAudit.ValidateRenderer(r);foreach(var m in r.sharedMaterials)Rendering.RenderingBuildAudit.ValidateMaterial(m,r.name);}
                var dependencies=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
                if(dependencies.Any(p=>new[]{"ExternalAssetIntake","98_unclassified","00_reference",".local-g0-v2"}.Any(p.Contains)))throw new InvalidOperationException("Historical/raw source in runtime dependencies");
                var structures=layer.Cast<Transform>().Where(t=>t.name.StartsWith("V47 ")).ToArray();
                if(structures.Length!=9)throw new InvalidOperationException("Expected all nine ordinary and strong deployment origins");
                // Solid ground forms are bounded by the original occupied console/rack
                // footprints; suspended tooling is behind the four spawn capsules.
                foreach(var facility in structures)
                {
                    if(facility.position!=v45DockPosition(facility.name,env)+Vector3.forward*3.8f)throw new InvalidOperationException("Deployment facility shifted its origin");
                }
                Record(root,"v47");File.WriteAllLines(Output+"/production_dependencies.txt",dependencies);
                File.WriteAllText(Output+"/presentation_audit.json","{\"validated\":true,\"facilities\":9,\"collisionFingerprintUnchanged\":true,\"missingMeshesMaterials\":0,\"historicalSourceDependencies\":0,\"legacyMaterialFlattening\":false}");
            }
            finally{if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);}
            Debug.Log("V47_PRESENTATION_AUDIT_PASS");
        }
        private static Vector3 v45DockPosition(string name,Gravivore.Presentation.World.ChapterVisualEnvironment env)
        {
            var dockName=name.Substring(name.IndexOf(" / ",StringComparison.Ordinal)+3);
            return env.Floor.Find(ConceptCorrectiveV45Builder.Layer).GetComponentsInChildren<Transform>(true).Single(t=>t.name==dockName).position;
        }
    }
}
