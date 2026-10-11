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
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Offline asset authoring only. Existing authority positions and accepted slice are read, never moved.</summary>
    public static class Chapter01ProductionBuilder
    {
        public const string Root="Assets/_Game/Content/Chapter01Production";
        public static string Prefab(string name) => Root+"/Prefabs/"+name+".prefab";
        public static readonly string[] Machines={"ArcDrone_V1","Carrier_V1","Warden_V1","Custodian_V1"};
        public static readonly string[] Kit={"Relay_Mast","Relay_Junction","Shield_Stack","Shield_Shell","Capacitor_Bank","Transformer",
            "Hauler_Wreck","Loading_Crane","Service_Arch","Containment_Buttress","Primary_Containment","Facility_Deck","Service_Markings"};
        private static readonly List<(string name, Vector3 center, Vector3 size)> Obstacles=new List<(string,Vector3,Vector3)>();
        private static readonly List<string> Manifest=new List<string>();
        [MenuItem("Gravivore/Chapter 01/Build full production environment")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Prefabs"); Directory.CreateDirectory(Root+"/Materials"); AssetDatabase.Refresh();
            var material=Atlas();
            foreach(var name in Machines) Actor(name,material);
            foreach(var name in Kit) Static(name,material);
            var catalog=new SerializedObject(AssetDatabase.LoadAssetAtPath<S15VisualCatalog>("Assets/_Game/Content/Definitions/S15_VisualCatalog.asset"));
            var recipes=catalog.FindProperty("_enemies");
            foreach(var pair in new[] {("arc-drone","ArcDrone_V1"),("carrier","Carrier_V1"),("warden","Warden_V1")})
                for(var i=0;i<recipes.arraySize;i++) if(recipes.GetArrayElementAtIndex(i).FindPropertyRelative("_id").stringValue==pair.Item1)
                    Recipe(recipes.GetArrayElementAtIndex(i),Prefab(pair.Item2));
            // Objective anchors remain clear. Their floor pad identifies the sector; its landmark is nearby in the authored package.
            var landmarks=catalog.FindProperty("_landmarks");
            for(var i=0;i<landmarks.arraySize;i++) Recipe(landmarks.GetArrayElementAtIndex(i),Prefab("Service_Markings"));
            catalog.ApplyModifiedPropertiesWithoutUndo();
            var definition=new SerializedObject(AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset"));
            Bind(definition.FindProperty("_boss"),Prefab("Custodian_V1")); definition.ApplyModifiedPropertiesWithoutUndo();
            Obstacles.Clear(); Manifest.Clear();
            var chapter=new GameObject("Chapter 01 Full Production");
            var floor=new GameObject("Facility deck segmentation").transform; floor.SetParent(chapter.transform,false);
            for(var x=-32;x<=32;x+=8) for(var z=-36;z<=100;z+=8) Place(floor,"Facility_Deck",new Vector3(x,.005f,z),false);
            var relay=Group(chapter,"relay-yard",new Vector3(-26,0,20));
            Place(relay,"Relay_Mast",new Vector3(-30.4f,0,26)); Place(relay,"Relay_Mast",new Vector3(-21.6f,0,26));
            Place(relay,"Relay_Junction",new Vector3(-30.4f,0,14)); Place(relay,"Relay_Junction",new Vector3(-21.6f,0,14),true,90);
            Place(relay,"Relay_Junction",new Vector3(-31,0,29),true,90);
            Dress(relay,new Vector3(-26,0,20),"cyan signal routing",new[] {"Relay_Mast","Relay_Junction"});
            var cutting=Group(chapter,"cutting-floor",new Vector3(0,0,40));
            // The accepted central cutting approach and containment prefab remain unchanged.
            Place(cutting,"Coolant_Pump",new Vector3(-10,0,34),true,0,true);
            Place(cutting,"Freight_Container",new Vector3(10,0,34),true,0,true);
            Place(cutting,"Conduit_Rack",new Vector3(-10,0,43),true,90,true);
            Place(cutting,"Maintenance_Station",new Vector3(10,0,44),true,0,true);
            Place(cutting,"Barrier_Module",new Vector3(-12,0,37),true,90,true);
            Place(cutting,"Barrier_Module",new Vector3(12,0,37),true,90,true);
            Dress(cutting,new Vector3(0,0,36),"orange processing line",new[] {"Coolant_Pump","Freight_Container"});
            var shield=Group(chapter,"shield-dump",new Vector3(26,0,20));
            Place(shield,"Shield_Shell",new Vector3(30.5f,0,26)); Place(shield,"Shield_Stack",new Vector3(21.5f,0,26));
            Place(shield,"Shield_Stack",new Vector3(30.5f,0,14)); Place(shield,"Shield_Stack",new Vector3(21.5f,0,14),true,90);
            Place(shield,"Shield_Shell",new Vector3(33,0,21),true,90);
            Dress(shield,new Vector3(26,0,20),"green defensive salvage",new[] {"Shield_Stack","Shield_Shell"});
            var capacitors=Group(chapter,"capacitor-field",new Vector3(-20,0,-12));
            Place(capacitors,"Transformer",new Vector3(-24.5f,0,-6)); Place(capacitors,"Capacitor_Bank",new Vector3(-24.5f,0,-18));
            Place(capacitors,"Capacitor_Bank",new Vector3(-15.5f,0,-18)); Place(capacitors,"Capacitor_Bank",new Vector3(-27,0,-13));
            Place(capacitors,"Transformer",new Vector3(-15.5f,0,-6));
            Dress(capacitors,new Vector3(-20,0,-12),"amber stored energy",new[] {"Capacitor_Bank","Transformer"});
            var haulers=Group(chapter,"hauler-graveyard",new Vector3(20,0,-12));
            Place(haulers,"Hauler_Wreck",new Vector3(25.1f,0,-6.2f)); Place(haulers,"Hauler_Wreck",new Vector3(15.2f,0,-17.7f));
            Place(haulers,"Hauler_Wreck",new Vector3(29,0,-18),true,35); Place(haulers,"Loading_Crane",new Vector3(15.5f,0,-6.2f),true,90);
            Place(haulers,"Loading_Crane",new Vector3(31.5f,0,-12),true,-90);
            Dress(haulers,new Vector3(20,0,-12),"muted violet freight salvage",new[] {"Hauler_Wreck","Loading_Crane"});
            var traversal=Group(chapter,"Service corridors",Vector3.zero);
            foreach(var x in new[]{-2,2}) for(var z=-34;z<=30;z+=4)
                Place(traversal,"Deck_Module",new Vector3(x,.028f,z),false,0,true);
            foreach(var z in new[] {-28,-20,-12,-4,4,12,20,28}) Place(traversal,"Service_Markings",new Vector3(0,.055f,z),false);
            foreach(var z in new[] {-24,0,24,84}) Place(traversal,"Service_Arch",new Vector3(0,0,z));
            foreach(var z in new[] {-32,-24,-16,-8,0,8,16,24,32}) foreach(var x in new[] {-4.9f,4.9f})
                Place(traversal,"Conduit_Rack",new Vector3(x,0,z),true,90,true);
            foreach(var side in new[]{-1,1}) foreach(var z in new[]{-28,-20,-4,4,12,28,34})
            {
                Place(traversal,"Structural_Support",new Vector3(side*4.9f,0,z),true,0,true);
                Place(traversal,"Maintenance_Station",new Vector3(side*5.6f,0,z+1.5f),true,side*90,true);
            }
            foreach(var side in new[] {-1,1}) foreach(var z in new[] {-30,-22,0,12,32,44,84})
                Place(traversal,"Maintenance_Station",new Vector3(side*12,0,z),true,side*90,true);
            foreach(var x in new[] {-16,-8,8,16})
            {
                Place(traversal,"Service_Markings",new Vector3(x,.055f,20),false,90);
                Place(traversal,"Service_Markings",new Vector3(x,.055f,-12),false,90);
            }
            foreach(var side in new[] {-1,1}) foreach(var z in new[] {-32,-24,-16,-8,0,8,16,24,32,40,48,56,64,72,80,88,96})
            {
                Place(traversal,"Bulkhead_Module",new Vector3(side*35.5f,0,z),true,side*90,true,new Vector3(2,1,1));
                if(z%16==0) Place(traversal,"Structural_Support",new Vector3(side*35.5f,0,z),true,0,true);
            }
            foreach(var x in new[] {-32,-24,-16,-8,0,8,16,24,32})
                Place(traversal,"Bulkhead_Module",new Vector3(x,0,-39.5f),true,0,true,new Vector3(2,1,1));
            foreach(var side in new[]{-1,1}) foreach(var z in new[]{60,80}) for(var x=6;x<=34;x+=4)
                Place(traversal,"Bulkhead_Module",new Vector3(side*x,0,z),false,0,true);
            Manifest.Add("Service corridors: continuous deck, directional service paint, raised cable arches, boundary bulkheads and maintenance infrastructure");
            var boss=Group(chapter,"Custodian containment complex",new Vector3(0,0,94));
            foreach(var side in new[] {-1,1})
            {
                foreach(var z in new[] {82,88,94,99}) Place(boss,"Containment_Buttress",new Vector3(side*6.2f,0,z));
                Place(boss,"Primary_Containment",new Vector3(side*8.2f,0,94));
                Place(boss,"Conduit_Rack",new Vector3(side*7.5f,2.0f,85),false,90,true,new Vector3(1.3f,1,1));
                Place(boss,"Bulkhead_Module",new Vector3(side*4.6f,0,80),true,0,true,new Vector3(.9f,1.4f,1));
                Place(boss,"Structural_Support",new Vector3(side*2.96f,0,80),true,0,true,new Vector3(1,1.5f,1));
            }
            foreach(var z in new[] {78,86,94}) Place(boss,"Service_Markings",new Vector3(0,.055f,z),false);
            for(var x=-32;x<=32;x+=8) Place(boss,"Bulkhead_Module",new Vector3(x,0,100),true,0,true,new Vector3(2,1.5f,1));
            // Raised service headers clear all controller routes, charge geometry and camera.
            Place(boss,"Conduit_Rack",new Vector3(0,3.55f,80),false,0,true,new Vector3(1.5f,1,.35f));
            Manifest.Add("Custodian complex: reinforced approach and open five-metre arena, primary pressure vessels outside combat circle");
            Save(chapter,"Full_Chapter_Environment");
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var composition=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env=composition.VisualEnvironment;
            var prior=env.Floor.Find("Chapter 01 Full Production"); if(prior!=null) Object.DestroyImmediate(prior.gameObject);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("Full_Chapter_Environment")),env.Floor);
            instance.name="Chapter 01 Full Production"; instance.transform.localPosition=Vector3.zero;
            // Retire prior prototype floor, corridor props and local recipes, retaining the accepted slice and repair hub.
            foreach(var node in env.GetComponentsInChildren<Transform>(true).ToArray())
                if(node.name.StartsWith("Phase3C",StringComparison.Ordinal)) node.gameObject.SetActive(false);
            var data=new SerializedObject(env); data.FindProperty("_fullChapterProduction").boolValue=true;
            var dressing=data.FindProperty("_dressing");
            for(var i=dressing.arraySize-1;i>=0;i--) dressing.DeleteArrayElementAtIndex(i);
            var blockers=data.FindProperty("_sliceObstacles");
            // Keep the approved slice proxies. Rebuild only new chapter proxies on repeat integration.
            for(var i=blockers.arraySize-1;i>=0;i--)
                if(blockers.GetArrayElementAtIndex(i).FindPropertyRelative("_name").stringValue.StartsWith("Chapter01 ",StringComparison.Ordinal)) blockers.DeleteArrayElementAtIndex(i);
            var begin=blockers.arraySize; blockers.arraySize+=Obstacles.Count;
            for(var i=0;i<Obstacles.Count;i++)
            {
                var entry=blockers.GetArrayElementAtIndex(begin+i);
                entry.FindPropertyRelative("_name").stringValue="Chapter01 "+Obstacles[i].name;
                entry.FindPropertyRelative("_center").vector3Value=Obstacles[i].center;
                entry.FindPropertyRelative("_size").vector3Value=Obstacles[i].size;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            env.ValidateOrThrow(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/chapter01-production");
            File.WriteAllLines("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/environment_manifest.txt",Manifest);
            Chapter01ProductionDependencies.ValidateOrThrow();
            Debug.Log("CHAPTER01_FULL_PRODUCTION_INTEGRATED");
        }
        private static Transform Group(GameObject parent,string name,Vector3 center)
        { var root=new GameObject(name).transform;root.SetParent(parent.transform,false);root.position=center;return root; }
        private static void Dress(Transform root,Vector3 center,string identity,string[] heroes)
        {
            // Match the approved four-metre deck language inside every combat apron.
            foreach(var x in new[]{-2,2}) foreach(var z in new[]{-6,-2,2,6})
                Place(root,"Deck_Module",center+new Vector3(x,.028f,z),false,0,true);
            Place(root,"Service_Markings",center+Vector3.up*.055f,false);
            foreach(var side in new[] {-1,1})
            {
                Place(root,"Conduit_Rack",center+new Vector3(side*4.5f,0,0),true,90,true);
                Place(root,"Structural_Support",center+new Vector3(side*4.2f,0,-4),true,0,true);
                Place(root,"Maintenance_Station",center+new Vector3(side*4.4f,0,-7),true,side*90,true);
            }
            Manifest.Add(root.name+": "+identity+"; landmarks "+string.Join(", ",heroes)+"; foreground/midscale/vertical machinery and floor segmentation");
            // Five independently inspectable packages are included in the full chapter prefab.
            PrefabUtility.SaveAsPrefabAssetAndConnect(root.gameObject,Prefab("Zone_"+root.name),InteractionMode.AutomatedAction);
        }
        private static GameObject Place(Transform parent,string name,Vector3 position,bool solid=true,float yaw=0,bool accepted=false,Vector3? scale=null)
        {
            var path=accepted?FirstVisualSliceBuilder.Prefab(name):Prefab(name);
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),parent);
            obj.transform.position=position;obj.transform.rotation=Quaternion.Euler(0,yaw,0);obj.transform.localScale=scale??Vector3.one;
            foreach(var node in obj.GetComponentsInChildren<Transform>()) GameObjectUtility.SetStaticEditorFlags(node.gameObject,StaticEditorFlags.BatchingStatic);
            if(solid)
            {
                if(name=="Service_Arch")
                {
                    // The merged mesh includes an open doorway. Proxy only its posts and raised truss.
                    foreach(var side in new[]{-1,1}) Obstacles.Add((parent.name+" arch post "+Obstacles.Count,
                        position+Quaternion.Euler(0,yaw,0)*new Vector3(side*3.4f,1.65f,0),new Vector3(1.06f,3.3f,1.12f)));
                    Obstacles.Add((parent.name+" arch truss "+Obstacles.Count,position+Vector3.up*4,new Vector3(6,.4f,.55f)));
                    return obj;
                }
                var renderers=obj.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;
                foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                if(bounds.size.y>.2f) Obstacles.Add((parent.name+" "+name+" "+Obstacles.Count,bounds.center,bounds.size));
            }
            return obj;
        }
        private static void Save(GameObject obj,string name) {PrefabUtility.SaveAsPrefabAsset(obj,Prefab(name));Object.DestroyImmediate(obj);}
        private static void Recipe(SerializedProperty p,string prefab)
        {p.FindPropertyRelative("_presentationPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
            p.FindPropertyRelative("_localPosition").vector3Value=Vector3.zero;p.FindPropertyRelative("_localEulerAngles").vector3Value=Vector3.zero;p.FindPropertyRelative("_localScale").vector3Value=Vector3.one;}
        private static void Bind(SerializedProperty p,string prefab)
        {p.FindPropertyRelative("_prefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);p.FindPropertyRelative("_localPosition").vector3Value=Vector3.zero;
            p.FindPropertyRelative("_localEulerAngles").vector3Value=Vector3.zero;p.FindPropertyRelative("_localScale").vector3Value=Vector3.one;}
        private static Material Atlas()
        {
            var path=Root+"/Materials/Chapter01_IndustrialAtlas.mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            foreach(var name in new[] {"BaseColor","Emission","MetallicSmoothness"})
            {var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Slice_"+name+".png"); importer.sRGBTexture=name=="BaseColor";importer.mipmapEnabled=true;importer.maxTextureSize=256;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();}
            mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Slice_BaseColor.png"));
            mat.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Slice_Emission.png"));
            mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Slice_MetallicSmoothness.png"));
            mat.SetColor("_BaseColor",Color.white);mat.SetColor("_EmissionColor",Color.white);mat.SetFloat("_Smoothness",.32f);mat.SetFloat("_Metallic",.55f);
            mat.EnableKeyword("_EMISSION");mat.EnableKeyword("_METALLICSPECGLOSSMAP");mat.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;mat.enableInstancing=true;
            EditorUtility.SetDirty(mat);return mat;
        }
        private static void Import(string name,bool animated)
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/Models/"+name+".fbx");
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.animationType=animated?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;
            importer.importAnimation=animated;importer.importCameras=false;importer.importLights=false;importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;
            importer.isReadable=false;importer.meshCompression=ModelImporterMeshCompression.Off;
            if(animated)
            {
                importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.motionNodeName="ROOT";importer.animationCompression=ModelImporterAnimationCompression.Off;
                importer.optimizeGameObjects=false;importer.skinWeights=ModelImporterSkinWeights.Custom;importer.maxBonesPerVertex=1;importer.SaveAndReimport();
                var clips=importer.defaultClipAnimations;foreach(var clip in clips)
                {clip.name=clip.name.Split('|').Last();clip.loopTime=clip.name=="Idle"||clip.name=="Run";clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.lockRootRotation=true;clip.keepOriginalPositionXZ=true;clip.keepOriginalPositionY=true;clip.keepOriginalOrientation=true;}
                importer.clipAnimations=clips;
            }
            importer.SaveAndReimport();
        }
        private static void Static(string name,Material mat)
        {Import(name,false);var obj=new GameObject(name);Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+".fbx"),obj.transform,false);
            foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterials=new[]{mat};Save(obj,name);}
        private static void Actor(string name,Material mat)
        {
            Import(name,true);var path=Root+"/Models/"+name+".fbx";var controllerPath=Root+"/"+name+".controller";
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)??AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;foreach(var old in machine.states)machine.RemoveState(old.state);
            var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
            foreach(var stateName in new[]{"Idle","Run","Attack","Hit","Death"})
            {var state=machine.AddState(stateName);state.motion=clips.Single(c=>c.name==stateName);state.writeDefaultValues=false;if(stateName=="Idle")machine.defaultState=state;}
            EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
            var obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));obj.name=name;
            var animator=obj.GetComponent<Animator>()??obj.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
            var skins=obj.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r=>r.name).ToArray();foreach(var r in skins){r.sharedMaterials=new[]{mat};r.updateWhenOffscreen=false;}
            foreach(var group in obj.GetComponentsInChildren<LODGroup>(true))Object.DestroyImmediate(group);
            var lod=obj.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.12f,new Renderer[]{skins[0]}),new LOD(.055f,new Renderer[]{skins[1]}),new LOD(.015f,new Renderer[]{skins[2]})});lod.RecalculateBounds();
            var sockets=new GameObject("Presentation Sockets").transform;sockets.SetParent(obj.transform,false);
            foreach(var socket in new[]{"AttackOrigin","HitCenter","HealthAnchor","DeathOrigin"})
            {var point=new GameObject(socket).transform;point.SetParent(sockets,false);point.localPosition=new Vector3(0,name=="Custodian_V1"?1.5f:.8f,socket=="AttackOrigin"?.4f:0);}
            if(name=="Custodian_V1") foreach(var origin in new[]{"ConeAttackOrigin","LineAttackOrigin","CircleAttackOrigin"})
                new GameObject(origin).transform.SetParent(obj.transform,false);
            Save(obj,name);
        }
    }
}
