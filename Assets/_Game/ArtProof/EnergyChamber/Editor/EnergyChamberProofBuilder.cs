using System;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Camera;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Gravivore.ArtProof.EnergyChamber
{
    /// <summary>Editor-only authoring and evidence; never a gameplay component.</summary>
    public static class EnergyChamberProofBuilder
    {
        public const string Root = "Assets/_Game/ArtProof/EnergyChamber";
        public const string ScenePath = Root + "/Scenes/EnergyChamber_HybridProof.unity";
        public const string PrefabPath = Root + "/Prefabs/EnergyChamber_HybridProof.prefab";
        public const string NodePath = Root + "/Prefabs/DistributionNode.prefab";
        public const string CameraSettingsPath = "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset";
        public const string G0Path = "Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab";
        public const string Evidence = "docs/history/implementation-passes/energy-chamber-vertical-nodes-v1";
        private const string Vr3 = "Assets/_Game/Content/VisualReplacementV3";
        private static readonly Vector3 GameplayTarget = new Vector3(0,0,-1.6f);
        private static readonly Vector3 PlayerPosition = new Vector3(2.7f,0,-3.65f);

        [MenuItem("Gravivore/Art Proof/Energy Chamber/Build isolated proof")]
        public static void Build()
        {
            foreach (var folder in new[] {"Materials","Prefabs","Scenes"}) Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh(); BuildMaterials();
            foreach (var model in Directory.GetFiles(Root+"/Models","*.fbx").Concat(Directory.GetFiles(Root+"/Donors","*.fbx")))
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(model);
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;
                importer.bakeAxisConversion=true;importer.animationType=ModelImporterAnimationType.None;
                importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();
            }
            BuildNode();
            var owner=new GameObject("Energy Chamber / hybrid proof V1");
            Custom("Foundation",owner.transform); Custom("CentralChamber",owner.transform);
            Custom("FoundationPolish",owner.transform);Custom("ChamberPolish",owner.transform);
            Custom("CoreHeart",owner.transform);
            for(var i=0;i<4;i++)
            {
                var angle=i*90f;
                var radial=Quaternion.Euler(0,angle,0);
                var node=(GameObject)PrefabUtility.InstantiatePrefab(Required<GameObject>(NodePath),owner.transform);
                node.name="Distribution node "+(i+1);node.transform.localPosition=radial*new Vector3(4.40f,0,0);node.transform.localRotation=radial;
                if(i==1 || i==3)Custom("NodeVariant",node.transform);
                var connector=Custom("Connector",owner.transform);connector.name="Core-to-node route "+(i+1);connector.transform.localRotation=radial;
                Custom("ConnectorPolish",connector.transform);
                var holder=Existing("VR3_proppipeholder",connector.transform,new Vector3(3.48f,.84f,0),.68f,90);
                holder.name="Existing pipe-holder / carrier bridge support";
            }
            BuildService(owner.transform);
            foreach(var renderer in owner.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode=ShadowCastingMode.On;
            PrefabUtility.SaveAsPrefabAsset(owner,PrefabPath);Object.DestroyImmediate(owner);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var installation=(GameObject)PrefabUtility.InstantiatePrefab(Required<GameObject>(PrefabPath));installation.name="Energy Chamber / hybrid proof V1";
            var g0=(GameObject)PrefabUtility.InstantiatePrefab(Required<GameObject>(G0Path));
            g0.name="G-0 / unchanged current Tier 0 scale reference";g0.transform.position=PlayerPosition;
            // The accepted presentation prefab is referenced at its production
            // transform. No geometry, material, scale or prefab bytes are changed.
            foreach(var behaviour in g0.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled=false;
            BuildStage();BuildLighting();
            var camera=new GameObject("Actual gameplay camera / centre inspection anchor",typeof(UnityEngine.Camera)).GetComponent<UnityEngine.Camera>();
            camera.tag="MainCamera";ApplyGameplayCamera(camera,GameplayTarget);
            var extra=camera.GetUniversalAdditionalCameraData();extra.renderPostProcessing=true;
            var volume=new GameObject("Restrained proof grading",typeof(Volume)).GetComponent<Volume>();volume.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(1.3f);bloom.intensity.Override(.075f);bloom.scatter.Override(.35f);
            var profilePath=Root+"/Materials/EC_ReviewVolume.asset";
            if(AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath)!=null)AssetDatabase.DeleteAsset(profilePath);
            AssetDatabase.CreateAsset(profile,profilePath);
            foreach(var component in profile.components)AssetDatabase.AddObjectToAsset(component,profile);
            volume.sharedProfile=profile;
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();Validate();
            Debug.Log("ENERGY_CHAMBER_PROOF_BUILT");
        }

        private static void BuildMaterials()
        {
            const string maps=Root+"/Textures/PolishV1/";
            foreach(var path in Directory.GetFiles(Root+"/Textures","*.png",SearchOption.AllDirectories))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=path.Contains("_Normal")?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=path.Contains("BaseColor") || path.Contains("Palette");
                importer.mipmapEnabled=true;importer.maxTextureSize=1024;
                importer.wrapMode=path.Contains("Markings")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
                importer.textureCompression=TextureImporterCompression.CompressedHQ;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=1024;
                android.format=path.Contains("Markings")?TextureImporterFormat.ASTC_4x4:TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            }
            foreach(var name in new[]{"Graphite","Shell","Steel","Deck","Ceramic","Warning","Coil","Cold","Copper","Gasket","Plate"})
            {
                var path=Root+"/Materials/EC_"+name+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
                foreach(var map in new[]{("_BaseMap","BaseColor"),("_BumpMap","Normal"),("_MetallicGlossMap","MetallicSmoothness"),("_OcclusionMap","Occlusion")})
                    material.SetTexture(map.Item1,Required<Texture2D>(maps+"EC_"+name+"_"+map.Item2+".png"));
                material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",.90f);material.SetFloat("_BumpScale",.40f);
                material.EnableKeyword("_NORMALMAP");material.EnableKeyword("_METALLICSPECGLOSSMAP");material.EnableKeyword("_OCCLUSIONMAP");material.enableInstancing=true;
                if(name=="Cold"){material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(.06f,.48f,.62f));}
                else {material.DisableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.black);}
                EditorUtility.SetDirty(material);
            }
            var donorPath=Root+"/Materials/EC_DonorMachinery.mat";
            var donor=AssetDatabase.LoadAssetAtPath<Material>(donorPath);
            if(donor==null){donor=new Material(Required<Material>(Vr3+"/Materials/VR3_WornIndustrialAtlas.mat"));AssetDatabase.CreateAsset(donor,donorPath);}
            donor.SetTexture("_BaseMap",Required<Texture2D>(maps+"EC_DonorPalette.png"));
            donor.SetTexture("_MetallicGlossMap",null);donor.DisableKeyword("_METALLICSPECGLOSSMAP");
            donor.SetFloat("_Metallic",.38f);donor.SetFloat("_Smoothness",.43f);donor.SetFloat("_OcclusionStrength",.55f);
            donor.SetColor("_EmissionColor",new Color(.12f,.16f,.16f));EditorUtility.SetDirty(donor);
            var markings=MaterialAt("Markings");markings.SetTexture("_BaseMap",Required<Texture2D>(maps+"EC_Markings_BaseColor.png"));
            markings.SetFloat("_Metallic",.22f);markings.SetFloat("_Smoothness",.35f);EditorUtility.SetDirty(markings);
            var ground=MaterialAt("ReviewGround");ground.SetTexture("_BaseMap",Required<Texture2D>(maps+"EC_Deck_BaseColor.png"));
            ground.SetColor("_BaseColor",new Color(.46f,.49f,.48f));ground.SetFloat("_Metallic",.12f);ground.SetFloat("_Smoothness",.25f);
            ground.DisableKeyword("_EMISSION");EditorUtility.SetDirty(ground);
            // Owner's explicit concept correction: warm central containment,
            // cool smaller peripheral stabilizers. Scope stays in proof materials.
            foreach(var preset in new[]{
                (name:"CoreWarm",map:"CoreWarm",emission:new Color(2.6f,.48f,.045f)),
                (name:"CoreInnerWarm",map:"CoreWarm",emission:new Color(1.8f,.29f,.025f)),
                (name:"NodeCool",map:"NodeCool",emission:new Color(.055f,.90f,1.8f))})
            {
                var m=MaterialAt(preset.name);const string energy=Root+"/Textures/VerticalNodesV1/EC_";
                foreach(var map in new[]{("_BaseMap","BaseColor"),("_BumpMap","Normal"),("_MetallicGlossMap","MetallicSmoothness"),("_OcclusionMap","Occlusion"),("_EmissionMap","Emission")})
                    m.SetTexture(map.Item1,Required<Texture2D>(energy+preset.map+"_"+map.Item2+".png"));
                m.SetColor("_BaseColor",Color.white);m.SetColor("_EmissionColor",preset.emission);
                m.SetFloat("_Smoothness",.80f);m.SetFloat("_BumpScale",.30f);
                // This installed URP validates _EMISSION using AnyEmissive.
                // BakedEmissive keeps the surface keyword through import/reload;
                // this review scene does not bake GI or add realtime local lights.
                m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
                m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_METALLICSPECGLOSSMAP");m.EnableKeyword("_OCCLUSIONMAP");m.EnableKeyword("_EMISSION");
                EditorUtility.SetDirty(m);
            }
        }

        private static Material MaterialAt(string name)
        {
            var path=Root+"/Materials/EC_"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.enableInstancing=true;return material;
        }

        private static GameObject Custom(string name,Transform parent)
        {
            var obj=ModelUnder(Root+"/Models/EC_"+name+".fbx",parent,"EC_"+name);
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>
                {
                    var materialName=m.name;
                    if(name=="CentralChamber" || name=="ChamberPolish")
                    {
                        if(materialName=="EC_Cold")materialName="EC_CoreWarm";
                        if(materialName=="EC_Coil")materialName="EC_CoreInnerWarm";
                    }
                    return Required<Material>(Root+"/Materials/"+materialName+".mat");
                }).ToArray();
            return obj;
        }

        private static void BuildNode()
        {
            var owner=new GameObject("Distribution node / master assembly");Custom("SideNode",owner.transform);
            Custom("NodePolish",owner.transform);
            Custom("VerticalDistribution",owner.transform);
            Existing("VR3_generator",owner.transform,new Vector3(.04f,.49f,-.10f),1.96f,90);
            Existing("VR3_centrifuge",owner.transform,new Vector3(.78f,.49f,.78f),.70f,0);
            PrefabUtility.SaveAsPrefabAsset(owner,NodePath);Object.DestroyImmediate(owner);
        }

        private static GameObject Existing(string name,Transform parent,Vector3 bottomCenter,float maxWidth,float yaw)
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(Required<GameObject>(Vr3+"/Prefabs/"+name+".prefab"),parent);
            obj.transform.localRotation=Quaternion.identity;obj.transform.localPosition=Vector3.zero;
            var bounds=LocalBoundsOf(obj);var scale=maxWidth/Mathf.Max(bounds.size.x,bounds.size.z);
            obj.transform.localScale=Vector3.one*scale;
            var rotation=Quaternion.Euler(0,yaw,0);obj.transform.localRotation=rotation;
            obj.transform.localPosition=bottomCenter-rotation*new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>Required<Material>(Root+"/Materials/EC_DonorMachinery.mat")).ToArray();
            return obj;
        }

        private static void BuildService(Transform parent)
        {
            var service=new GameObject("Service architecture / source stairs and rails");service.transform.SetParent(parent,false);
            // The new stair donor has 2 m rise and 4.08 m run. Uniform scaling
            // preserves human proportions; upper tread meets the 1.32 m deck.
            for(var i=0;i<2;i++)
            {
                var stair=ModelUnder(Root+"/Donors/Platform_Stairs_4.fbx",service.transform,"Service stair "+(i+1));MapTrim(stair);
                var bounds=LocalBoundsOf(stair);var scale=1.31f/bounds.size.y;stair.transform.localScale=Vector3.one*scale;
                var rotation=Quaternion.Euler(0,45+i*180,0);stair.transform.localRotation=rotation;
                stair.transform.localPosition=rotation*new Vector3(0,.015f,-3.15f)-rotation*new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
                var vertices=stair.GetComponentsInChildren<MeshFilter>().SelectMany(f=>f.sharedMesh.vertices.Select(v=>f.transform.TransformPoint(v))).ToArray();
                var top=vertices.Where(v=>v.y>vertices.Max(p=>p.y)-.035f).Aggregate(Vector3.zero,(sum,v)=>sum+v);
                top/=vertices.Count(v=>v.y>vertices.Max(p=>p.y)-.035f);
                if(new Vector2(top.x,top.z).magnitude>3.15f)
                {
                    rotation=Quaternion.Euler(0,225+i*180,0);stair.transform.localRotation=rotation;
                    stair.transform.localPosition=Quaternion.Euler(0,45+i*180,0)*new Vector3(0,.015f,-3.15f)-rotation*new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
                }
            }
            for(var i=0;i<4;i++)
            {
                var rail=ModelUnder(Root+"/Donors/Prop_Rail_Round_Big.fbx",service.transform,"Service quadrant safety rail "+(i+1));MapTrim(rail);
                rail.transform.localScale=Vector3.one*.63f;rail.transform.localRotation=Quaternion.Euler(0,45+i*90,0);
                rail.transform.localPosition=Vector3.up*1.37f;
            }
        }
        private static void MapTrim(GameObject obj)
        {
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>Required<Material>("Assets/_Game/Content/BlueprintWorldR1/Materials/R1_Trim"+(m.name.Contains("Trim_01") ? "1" : "2")+".mat")).ToArray();
        }
        private static GameObject ModelUnder(string path,Transform parent,string name)
        {
            // FBX roots may encode both axis conversion and unit scale. Keep
            // those intact; assembly transforms belong on a neutral wrapper.
            var wrapper=new GameObject(name);wrapper.transform.SetParent(parent,false);
            Object.Instantiate(Required<GameObject>(path),wrapper.transform,false);return wrapper;
        }

        private static void BuildStage()
        {
            // Review backdrop only: the judged installation is its own prefab.
            var stage=new GameObject("Inspection ground / existing authored floor donor");
            var floor=Required<GameObject>(Vr3+"/Prefabs/VR3_platformmetal.prefab");
            for(var x=-3;x<=3;x++)for(var z=-3;z<=3;z++)
            {
                var tile=(GameObject)PrefabUtility.InstantiatePrefab(floor,stage.transform);
                var bounds=BoundsOf(tile);var scale=2.45f/Mathf.Max(bounds.size.x,bounds.size.z);
                tile.transform.localScale=Vector3.one*scale;
                tile.transform.localPosition=new Vector3(x*2.48f,-.015f,z*2.48f)-new Vector3(bounds.center.x,bounds.max.y,bounds.center.z)*scale;
                foreach(var renderer in tile.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>Required<Material>(Root+"/Materials/EC_ReviewGround.mat")).ToArray();
            }
        }
        private static void BuildLighting()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.32f,.37f,.44f);
            RenderSettings.ambientEquatorColor=new Color(.16f,.20f,.25f);RenderSettings.ambientGroundColor=new Color(.08f,.10f,.13f);
            RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture=Required<Cubemap>("Assets/_Game/Content/BlueprintWorldR1/Textures/R1_IndustrialReflection.asset");RenderSettings.reflectionIntensity=.70f;
            var key=new GameObject("Inspection directional key",typeof(Light)).GetComponent<Light>();key.type=LightType.Directional;key.intensity=1.7f;key.color=new Color(.83f,.91f,1);key.shadows=LightShadows.Soft;key.transform.rotation=Quaternion.Euler(45,-35,0);RenderSettings.sun=key;
            var rim=new GameObject("Inspection unshadowed fill",typeof(Light)).GetComponent<Light>();rim.type=LightType.Directional;rim.intensity=.32f;rim.color=new Color(.62f,.72f,.86f);rim.shadows=LightShadows.None;rim.transform.rotation=Quaternion.Euler(30,135,0);
        }
        public static void ApplyGameplayCamera(UnityEngine.Camera camera,Vector3 target)
        {
            var settings=Required<CameraFollowSettings>(CameraSettingsPath);
            camera.orthographic=false;camera.fieldOfView=settings.FieldOfView;camera.aspect=9f/16;camera.nearClipPlane=.1f;camera.farClipPlane=100;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.045f,.062f,.085f);camera.allowHDR=true;
            camera.transform.position=target+settings.Offset;
            camera.transform.rotation=Quaternion.LookRotation(target+Vector3.up*settings.LookAtHeight-camera.transform.position,Vector3.up);
        }

        [MenuItem("Gravivore/Art Proof/Energy Chamber/Capture review views")]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(ScenePath);Directory.CreateDirectory(Evidence+"/screenshots");
            var roots=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            var camera=roots.SelectMany(g=>g.GetComponentsInChildren<UnityEngine.Camera>()).Single();
            var g0=roots.Single(g=>g.name.StartsWith("G-0 /",StringComparison.Ordinal));
            var originalPos=camera.transform.position;var originalRot=camera.transform.rotation;var originalFov=camera.fieldOfView;
            g0.SetActive(false);
            SetView(camera,new Vector3(11,5,-15),new Vector3(0,2.7f,0),46);Render(camera,"01_low_technical",1600,1100);
            SetView(camera,new Vector3(12,17,-13),new Vector3(0,1.8f,0),48);Render(camera,"02_high_three_quarter",1600,1300);
            ApplyGameplayCamera(camera,GameplayTarget);Render(camera,"03_gameplay_camera",1080,1920);
            g0.SetActive(true);Render(camera,"04_gameplay_G0",1080,1920);
            // Additional true player-follow anchor. FOV, offset, look-at height
            // remain exact; centring uses G-0's position rather than the machine.
            ApplyGameplayCamera(camera,PlayerPosition);Render(camera,"04b_player_follow_anchor",1080,1920);
            ApplyGameplayCamera(camera,GameplayTarget);
            var renderers=roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).ToArray();
            var originalMaterials=renderers.Select(r=>r.sharedMaterials).ToArray();var diagnostic=originalMaterials.SelectMany(m=>m).Distinct().ToDictionary(m=>m,m=>new Material(m));
            try
            {
                foreach(var m in diagnostic.Values){m.DisableKeyword("_EMISSION");if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.black);}
                for(var i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originalMaterials[i].Select(m=>diagnostic[m]).ToArray();
                Render(camera,"05_emission_disabled",1080,1920);
                SetView(camera,new Vector3(6.4f,6.8f,-8.2f),new Vector3(.1f,3.2f,0),42);Render(camera,"05b_emission_disabled_detail",1600,1600);
            }
            finally {for(var i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originalMaterials[i];foreach(var m in diagnostic.Values)Object.DestroyImmediate(m);}
            SetView(camera,new Vector3(9,9,-13),new Vector3(0,2.1f,0),46);Render(camera,"06_normal_material_lighting",1600,1400);
            SetView(camera,new Vector3(6.4f,6.8f,-8.2f),new Vector3(.1f,3.2f,0),42);Render(camera,"07_close_three_quarter",1600,1600);
            var installation=roots.Single(g=>g.name.StartsWith("Energy Chamber",StringComparison.Ordinal));
            installation.SetActive(false);g0.SetActive(false);
            var nodeDetail=Object.Instantiate(Required<GameObject>(NodePath));
            try
            {
                SetView(camera,new Vector3(4.8f,4.6f,-6.4f),new Vector3(0,1.9f,0),40);Render(camera,"08_vertical_node_detail",1400,1600);
            }
            finally {Object.DestroyImmediate(nodeDetail);installation.SetActive(true);g0.SetActive(true);}
            camera.transform.SetPositionAndRotation(originalPos,originalRot);camera.fieldOfView=originalFov;
            var settings=Required<CameraFollowSettings>(CameraSettingsPath);var assembly=roots.Single(g=>g.name.StartsWith("Energy Chamber",StringComparison.Ordinal));
            File.WriteAllText(Evidence+"/camera-and-scale.json",JsonUtility.ToJson(new CameraRecord{settings=CameraSettingsPath,offset=settings.Offset,fieldOfView=settings.FieldOfView,lookAtHeight=settings.LookAtHeight,inspectionAnchor=GameplayTarget,playerAnchor=PlayerPosition,g0Source=G0Path,g0Scale=g0.transform.localScale,g0Bounds=BoundsOf(g0).size,installationBounds=BoundsOf(assembly).size},true));
            File.WriteAllText(Evidence+"/component-bounds.json",JsonUtility.ToJson(new BoundRecords{items=assembly.GetComponentsInChildren<Renderer>().Select(r=>new BoundRecord{name=r.name,position=r.transform.position,size=r.bounds.size,center=r.bounds.center}).ToArray()},true));
            Debug.Log("ENERGY_CHAMBER_REVIEW_CAPTURED");
        }
        private static void SetView(UnityEngine.Camera camera,Vector3 position,Vector3 look,float fov)
        {camera.transform.position=position;camera.transform.LookAt(look);camera.fieldOfView=fov;}
        private static void Render(UnityEngine.Camera camera,string name,int width,int height)
        {
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var texture=new Texture2D(width,height,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try {camera.targetTexture=target;camera.aspect=(float)width/height;for(var i=0;i<3;i++)camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Evidence+"/screenshots/"+name+".png",texture.EncodeToPNG());}
            finally {camera.targetTexture=null;RenderTexture.active=previous;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(texture);}
        }
        [Serializable] private sealed class CameraRecord {public string settings,g0Source;public Vector3 offset,inspectionAnchor,playerAnchor,g0Scale,g0Bounds,installationBounds;public float fieldOfView,lookAtHeight;}
        [Serializable] private sealed class BoundRecords {public BoundRecord[] items;}
        [Serializable] private sealed class BoundRecord {public string name;public Vector3 position,size,center;}

        public static void Validate()
        {
            var prefab=Required<GameObject>(PrefabPath);
            if(prefab.GetComponentsInChildren<Collider>(true).Length!=0 || prefab.GetComponentsInChildren<MonoBehaviour>(true).Length!=0)throw new InvalidDataException("Proof prefab must contain presentation only.");
            if(prefab.transform.Cast<Transform>().Count(t=>t.name.StartsWith("Distribution node ",StringComparison.Ordinal))!=4 || prefab.transform.Cast<Transform>().Count(t=>t.name.StartsWith("Core-to-node route ",StringComparison.Ordinal))!=4)throw new InvalidDataException("Four master nodes and four connected routes required.");
            foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                if(renderer.sharedMaterials.Any(m=>m==null || m.shader==null || m.shader.name.Contains("Error")))throw new InvalidDataException("Missing or unsupported proof material: "+renderer.name);
            if(!File.Exists(ScenePath))throw new InvalidDataException("Proof scene missing.");
            var meshes=prefab.GetComponentsInChildren<MeshFilter>(true);var triangles=meshes.Sum(f=>f.sharedMesh.triangles.Length/3);
            Directory.CreateDirectory(Evidence+"/verification");
            File.WriteAllText(Evidence+"/verification/assembly.json",JsonUtility.ToJson(new AssemblyRecord{renderers=prefab.GetComponentsInChildren<Renderer>(true).Length,triangles=triangles,nodes=4,routes=4,gameplayComponents=0,colliders=0},true));
            Debug.Log("ENERGY_CHAMBER_VALIDATED triangles="+triangles);
        }
        [Serializable] private sealed class AssemblyRecord {public int renderers,triangles,nodes,routes,gameplayComponents,colliders;}
        public static Bounds BoundsOf(GameObject root)
        {var renderers=root.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0)throw new InvalidDataException("No renderer in "+root.name);var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);return bounds;}
        private static Bounds LocalBoundsOf(GameObject root)
        {
            var first=true;var result=new Bounds();
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var bounds=renderer.localBounds;var matrix=root.transform.worldToLocalMatrix*renderer.transform.localToWorldMatrix;
                foreach(var x in new[]{bounds.min.x,bounds.max.x})foreach(var y in new[]{bounds.min.y,bounds.max.y})foreach(var z in new[]{bounds.min.z,bounds.max.z})
                {var point=matrix.MultiplyPoint3x4(new Vector3(x,y,z));if(first){result=new Bounds(point,Vector3.zero);first=false;}else result.Encapsulate(point);}
            }
            if(first)throw new InvalidDataException("No local renderer bounds in "+root.name);return result;
        }
        private static T Required<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidDataException("Missing proof dependency: "+path);

    }
}
