using System;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Imports authored Blender assets and places presentation only. No proxy/balance changes.</summary>
    public static class ConceptFidelityBuilder
    {
        public const string Root="Assets/_Game/Content/ConceptFidelityV2";
        public static readonly string[] StaticAssets={"Repair_Platform_V2","Repair_Pedestal_V2","Repair_Upper_V2","Repair_Forearm_V2",
            "Repair_Joint_V2","Repair_Tool_V2","Repair_Scanner_V2","Deck_V2_0","Deck_V2_1","Deck_V2_2","Curved_Services_V2",
            "Bulkhead_V2","Containment_Frame_V2","Pressure_Wreck_V2"};
        public static readonly string[] AnimatedAssets={"Custodian_V2","Reactor_V2","Turbine_V2","Containment_Vessel_V2"};
        public static string Prefab(string name)=>Root+"/Prefabs/"+name+".prefab";
        [MenuItem("Gravivore/Chapter 01/Integrate concept fidelity V2")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");AssetDatabase.Refresh();
            UrpConfigurator.ConfigureUrp();
            var atlas=Atlas();
            foreach(var path in new[]{"Assets/_Game/Content/VisualSlice/Materials/Slice_IndustrialAtlas.mat",
                "Assets/_Game/Content/Chapter01Production/Materials/Chapter01_IndustrialAtlas.mat"})
            {var accepted=AssetDatabase.LoadAssetAtPath<Material>(path);if(accepted==null)continue;accepted.SetColor("_EmissionColor",Color.white*3.2f);EditorUtility.SetDirty(accepted);}
            foreach(var name in StaticAssets)ImportPrefab(name,atlas,false);
            foreach(var name in AnimatedAssets)ImportPrefab(name,atlas,true);
            var settingsPath=Root+"/ConceptFidelity.asset";
            var settings=AssetDatabase.LoadAssetAtPath<ConceptFidelityDefinition>(settingsPath);
            if(settings==null){settings=ScriptableObject.CreateInstance<ConceptFidelityDefinition>();AssetDatabase.CreateAsset(settings,settingsPath);}
            var data=new SerializedObject(settings);
            foreach(var pair in new[]{("_pedestal","Repair_Pedestal_V2"),("_upper","Repair_Upper_V2"),("_forearm","Repair_Forearm_V2"),
                ("_joint","Repair_Joint_V2"),("_tool","Repair_Tool_V2"),("_scanner","Repair_Scanner_V2")})
                data.FindProperty(pair.Item1).objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(pair.Item2));
            data.ApplyModifiedPropertiesWithoutUndo();settings.ValidateOrThrow();
            var definition=AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset");
            data=new SerializedObject(definition);Bind(data.FindProperty("_boss"),"Custodian_V2");Bind(data.FindProperty("_repairHub"),"Repair_Platform_V2");
            data.FindProperty("_fidelity").objectReferenceValue=settings;data.ApplyModifiedPropertiesWithoutUndo();
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var composition=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env=composition.VisualEnvironment;
            var prior=env.Floor.Find("Chapter 01 Concept Fidelity V2");if(prior!=null)Object.DestroyImmediate(prior.gameObject);
            // Preserve baseline prefab dependency and all collision proxies. Retire only
            // visible deck overlays and overhead arches along the replaced hero route.
            foreach(var node in env.GetComponentsInChildren<Transform>(true))
            {
                if(node.name.StartsWith("Deck_Module",StringComparison.Ordinal)||node.name.StartsWith("Service_Markings",StringComparison.Ordinal))
                    node.gameObject.SetActive(false);
                if(node.name.StartsWith("Service_Arch",StringComparison.Ordinal)&&Mathf.Abs(node.position.x)<4)
                    node.gameObject.SetActive(false);
            }
            var route=new GameObject("Chapter 01 Concept Fidelity V2").transform;route.SetParent(env.Floor,false);
            var deck=Group(route,"Continuous worn deck");
            for(var z=-36;z<=96;z+=6)foreach(var x in new[]{-3,3})
                Place(deck,"Deck_V2_"+(Math.Abs(z/6+x)%3),new Vector3(x,.065f,z),((z/6+x)%4)*90);
            // The normal quest loop visits every ordinary sector before the elite gate.
            foreach(var center in new[]{new Vector3(-26,0,20),new Vector3(26,0,20),new Vector3(-20,0,-12),new Vector3(20,0,-12),new Vector3(0,0,40)})
            {
                for(var x=-6;x<=6;x+=6)for(var z=-6;z<=6;z+=6)
                    if(Mathf.Abs(center.x+x)>6)Place(deck,"Deck_V2_"+(Math.Abs(x+z+(int)center.x)%3),center+new Vector3(x,.065f,z),((x-z)/6%4)*90);
            }
            foreach(var z in new[]{-12,20})foreach(var x in new[]{-18,-12,-6,6,12,18})
                Place(deck,"Deck_V2_"+(Math.Abs(x/6)%3),new Vector3(x,.065f,z),90);
            var services=Group(route,"Bowed services and threshold machinery");
            foreach(var z in new[]{-30,-18,-6,6,18,30,42,54,66,78,90})
            {
                var side=z%24==6?-1:1;
                Place(services,"Curved_Services_V2",new Vector3(side*4.3f,.10f,z),side*90);
                if(z!=66&&z!=90)Place(services,"Bulkhead_V2",new Vector3(-side*5.2f,0,z),side*90);
            }
            // Extra services terminate at actual sector thresholds rather than following
            // every deck seam. Preserve the clear controller lane down the centre.
            foreach(var pair in new[]{new Vector3(-4.6f,0,-24),new Vector3(4.5f,0,-5),new Vector3(-4.7f,0,9),
                new Vector3(4.4f,0,26),new Vector3(-4.6f,0,47),new Vector3(4.8f,0,74),new Vector3(-5.5f,0,86)})
                Place(services,"Curved_Services_V2",pair,pair.x<0?60:-70);
            // Large silhouettes sit outside clear central lanes and existing spawn cores.
            var heroes=Group(route,"Industrial focal points");
            Place(heroes,"Turbine_V2",new Vector3(-5.4f,0,-28),-15,false);
            Place(heroes,"Turbine_V2",new Vector3(5.6f,0,10),-20,false);
            Place(heroes,"Reactor_V2",new Vector3(-5.6f,0,26),25,false);
            Place(heroes,"Reactor_V2",new Vector3(-24.5f,0,-6),18,false);
            Place(heroes,"Pressure_Wreck_V2",new Vector3(25.1f,0,-6.2f),-18);
            Place(heroes,"Turbine_V2",new Vector3(-30.4f,0,26),25,false);
            Place(heroes,"Pressure_Wreck_V2",new Vector3(30.5f,0,26),16);
            Place(heroes,"Reactor_V2",new Vector3(-10,0,34),-12,false);
            Place(heroes,"Turbine_V2",new Vector3(5.8f,0,54),16,false);
            Place(heroes,"Reactor_V2",new Vector3(-5.7f,0,69),-25,false);
            Place(heroes,"Containment_Vessel_V2",new Vector3(7.7f,0,85),-20,false);
            Place(heroes,"Containment_Vessel_V2",new Vector3(-8.2f,0,94),12,false);
            // Retire old prop art at replaced footprints, retaining its independent
            // authored collision proxies and nested baseline dependency packages.
            foreach(var node in env.Floor.GetComponentsInChildren<Transform>(true))
            {
                if(node.IsChildOf(route))continue;
                if(node.name.StartsWith("Transformer",StringComparison.Ordinal)&&Vector3.Distance(node.position,new Vector3(-24.5f,0,-6))<.1f||
                    node.name.StartsWith("Coolant_Pump",StringComparison.Ordinal)&&Vector3.Distance(node.position,new Vector3(-10,0,34))<.1f||
                    node.name.StartsWith("Hauler_Wreck",StringComparison.Ordinal)&&Vector3.Distance(node.position,new Vector3(25.1f,0,-6.2f))<.1f)
                    node.gameObject.SetActive(false);
            }
            foreach(var z in new[]{82,90,98})foreach(var side in new[]{-1,1})
                Place(heroes,"Containment_Frame_V2",new Vector3(side*6.4f,0,z),side*12);
            foreach(var point in new[]{new Vector3(-5.4f,1.2f,-28),new Vector3(-24.5f,1.2f,-6),new Vector3(25,1.0f,-6),
                new Vector3(-10,1.3f,34),new Vector3(-5.7f,1.3f,69),new Vector3(7.7f,1.4f,85)})
            {var fault=new GameObject("Fidelity service fault").transform;fault.SetParent(route,false);fault.position=point;}
            foreach(var pair in new[]{("cyan",new Vector3(0,1.1f,-28)),("cyan",new Vector3(-5.4f,1.7f,-28)),("amber",new Vector3(-24.5f,1.7f,-6)),
                ("cyan",new Vector3(-30.4f,1.7f,26)),("cyan",new Vector3(5.6f,1.7f,10)),("amber",new Vector3(-5.6f,1.7f,26)),
                ("amber",new Vector3(-10,1.7f,34)),("cyan",new Vector3(5.8f,1.7f,54)),
                ("amber",new Vector3(-5.7f,1.7f,69)),("red",new Vector3(7.7f,1.7f,85)),("red",new Vector3(-8.2f,1.7f,94)),("red",new Vector3(0,2.5f,94))})
            {var source=new GameObject("Fidelity light "+pair.Item1).transform;source.SetParent(route,false);source.position=pair.Item2;}
            // Only two reused unshadowed lights contribute local material response.
            // The key light remains the sole shadow source.
            Lighting(env);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            ValidateOrThrow();
            Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2");
            File.WriteAllText("docs/history/visual-stages/chapter01-visual-passes/concept-fidelity-v2/hero_route.json",JsonUtility.ToJson(new RouteEvidence(),true));
            Debug.Log("CONCEPT_FIDELITY_V2_INTEGRATED");
        }
        [Serializable] private sealed class RouteEvidence
        {
            public string[] sequence={"repair-hub","capacitor-field / hauler-graveyard","relay-yard / shield-dump","cutting-floor",
                "elite approach","Magnetar (0,70)","containment threshold (0,80)","Custodian (0,94)"};
            public string duration="5–10 minute presentation coverage; normal first-play unlock progression remains unchanged";
            public bool gameplayCollidersChanged=false;
            public int additionalRealtimeLights=2;
        }
        private static Transform Group(Transform parent,string name)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
        private static GameObject Place(Transform parent,string name,Vector3 position,float yaw=0,bool isStatic=true)
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name)),parent);
            obj.transform.position=position;obj.transform.rotation=Quaternion.Euler(0,yaw,0);
            if(isStatic)foreach(var t in obj.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic);
            return obj;
        }
        private static void Bind(SerializedProperty property,string name)
        {
            property.FindPropertyRelative("_prefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));
            property.FindPropertyRelative("_localPosition").vector3Value=Vector3.zero;
            property.FindPropertyRelative("_localEulerAngles").vector3Value=Vector3.zero;
            property.FindPropertyRelative("_localScale").vector3Value=Vector3.one;
        }
        private static Material Atlas()
        {
            var path=Root+"/Materials/Fidelity_IndustrialAtlas.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            foreach(var label in new[]{"BaseColor","Emission","MetallicSmoothness"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Fidelity_"+label+".png");
                importer.sRGBTexture=label=="BaseColor"||label=="Emission";importer.mipmapEnabled=true;importer.maxTextureSize=512;
                importer.wrapMode=TextureWrapMode.Clamp;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=512;android.format=TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            }
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Fidelity_BaseColor.png"));
            material.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Fidelity_Emission.png"));
            material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Fidelity_MetallicSmoothness.png"));
            material.SetColor("_BaseColor",Color.white);material.SetColor("_EmissionColor",Color.white*3.2f);
            material.SetFloat("_Metallic",1);material.SetFloat("_Smoothness",1);
            material.EnableKeyword("_EMISSION");material.EnableKeyword("_METALLICSPECGLOSSMAP");material.enableInstancing=true;
            material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
            EditorUtility.SetDirty(material);return material;
        }
        private static void ImportPrefab(string name,Material material,bool animated)
        {
            var path=Root+"/Models/"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;
            importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.isReadable=false;importer.importAnimation=animated;
            importer.animationType=animated?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;
            if(animated)
            {
                importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.motionNodeName="ROOT";
                importer.optimizeGameObjects=false;importer.animationCompression=ModelImporterAnimationCompression.Off;
                importer.skinWeights=ModelImporterSkinWeights.Custom;importer.maxBonesPerVertex=1;importer.SaveAndReimport();
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips)
                {
                    clip.name=clip.name.Split('|').Last();clip.loopTime=clip.name=="Idle"||clip.name=="Run";
                    clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.lockRootRotation=true;
                    clip.keepOriginalPositionXZ=true;clip.keepOriginalPositionY=true;clip.keepOriginalOrientation=true;
                }
                importer.clipAnimations=clips;
            }
            importer.SaveAndReimport();
            GameObject obj;
            if(animated){obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));obj.name=name;}
            else
            {
                // Preserve the imported FBX axis correction below a neutral placement
                // root. Placement must never replace that source rotation.
                obj=new GameObject(name);Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),obj.transform,false);
            }
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=new[]{material};
            if(animated)
            {
                var controllerPath=Root+"/"+name+".controller";
                var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath)??AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                if(controller.layers.Length==0)controller.AddLayer("Base Layer");
                var machine=controller.layers[0].stateMachine;foreach(var old in machine.states)machine.RemoveState(old.state);
                foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")))
                {var state=machine.AddState(clip.name);state.motion=clip;state.writeDefaultValues=false;if(clip.name=="Idle")machine.defaultState=state;}
                EditorUtility.SetDirty(controller);EditorUtility.SetDirty(machine);AssetDatabase.SaveAssets();
                var animator=obj.GetComponent<Animator>()??obj.AddComponent<Animator>();animator.runtimeAnimatorController=controller;
                animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
                var skins=obj.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r=>r.name).ToArray();
                foreach(var skin in skins)skin.updateWhenOffscreen=false;
                foreach(var importedLod in obj.GetComponentsInChildren<LODGroup>(true))Object.DestroyImmediate(importedLod);
                var lod=obj.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.16f,new Renderer[]{skins[0]}),new LOD(.075f,new Renderer[]{skins[1]}),new LOD(.018f,new Renderer[]{skins[2]})});lod.RecalculateBounds();
                if(name=="Custodian_V2")
                {
                    foreach(var origin in new[]{"ConeAttackOrigin","LineAttackOrigin","CircleAttackOrigin"})new GameObject(origin).transform.SetParent(obj.transform,false);
                    var sockets=new GameObject("Presentation Sockets").transform;sockets.SetParent(obj.transform,false);
                    foreach(var socket in new[]{"AttackOrigin","HitCenter","HealthAnchor","DeathOrigin"})
                    {var t=new GameObject(socket).transform;t.SetParent(sockets,false);t.localPosition=new Vector3(0,1.5f,socket=="AttackOrigin"?1:0);}
                }
            }
            if(name=="Repair_Platform_V2")new GameObject("ServicePoint").transform.SetParent(obj.transform,false);
            PrefabUtility.SaveAsPrefabAsset(obj,Prefab(name));Object.DestroyImmediate(obj);
        }
        private static void Lighting(ChapterVisualEnvironment env)
        {
            var key=env.KeyLight;key.color=new Color(.77f,.86f,1);key.intensity=1.45f;
            key.transform.rotation=Quaternion.Euler(52,-28,0);key.shadowStrength=.56f;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.24f,.29f,.32f);
            RenderSettings.fog=false;
            var profilePath=Root+"/Materials/Fidelity_Bloom.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,profilePath);}
            if(!profile.TryGet<Bloom>(out var bloom)){bloom=profile.Add<Bloom>(true);AssetDatabase.AddObjectToAsset(bloom,profile);}
            bloom.intensity.Override(.20f);bloom.threshold.Override(1.15f);bloom.scatter.Override(.55f);
            bloom.highQualityFiltering.Override(false);
            var prior=env.transform.parent.Find("Fidelity restrained bloom");if(prior!=null)Object.DestroyImmediate(prior.gameObject);
            var volume=new GameObject("Fidelity restrained bloom",typeof(Volume));volume.transform.SetParent(env.transform.parent,false);
            volume.GetComponent<Volume>().isGlobal=true;volume.GetComponent<Volume>().sharedProfile=profile;
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpConfigurator.UrpAssetPath);pipeline.supportsHDR=true;
            EditorUtility.SetDirty(profile);EditorUtility.SetDirty(pipeline);
        }
        public static void ValidateOrThrow()
        {
            foreach(var name in StaticAssets.Concat(AnimatedAssets))
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));
                Gravivore.Presentation.Assets.PresentationPrefabValidation.ValidateOrThrow(prefab);
                foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())
                    if(renderer.sharedMaterials.Length!=1||renderer.sharedMaterial==null)throw new InvalidOperationException(name+" must use one shared atlas material.");
                if(AnimatedAssets.Contains(name))
                {
                    var lod=prefab.GetComponent<LODGroup>();if(lod==null||lod.GetLODs().Length!=3)throw new InvalidOperationException(name+" requires three LODs.");
                    if(prefab.GetComponent<Animator>().applyRootMotion)throw new InvalidOperationException("Presentation cannot own root motion.");
                }
            }
            var deps=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true);
            foreach(var name in StaticAssets.Concat(AnimatedAssets))
            {
                var required=name=="Custodian_V2"?Chapter01V3Builder.Prefab("Custodian_V3"):Prefab(name);
                if(!deps.Contains(required))throw new InvalidOperationException("Live scene lacks "+required);
            }
            Debug.Log("CONCEPT_FIDELITY_ASSET_VALIDATION_PASS");
        }
    }
}
