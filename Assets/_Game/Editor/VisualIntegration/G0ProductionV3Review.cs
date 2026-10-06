using System;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Explicit isolated asset intake and evidence. Never saves the canonical scene.</summary>
    public static class G0ProductionV3Review
    {
        public const string Root = "Assets/_Game/ArtReview/G0ProductionV3";
        public const string Model = Root + "/Models/G0_Production_V3.fbx";
        public const string Prefab = Root + "/G0_Production_V3_ArtReview.prefab";
        public const string ScenePath = Root + "/G0_Production_V3_Review.unity";
        public const string Evidence = "docs/g0-production-v3/evidence/unity";
        public const string Data = "docs/g0-production-v3/data";
        // Frozen accepted V2.1 normalization (1.4 / 3.358424902 * 1.10).
        public const float PresentationFit = .4585482776f;
        public static readonly string[] ClipNames = { "Idle", "Run", "Attack", "Hit", "Death" };

        [Serializable] public sealed class MeshReport
        {
            public string name;
            public int vertices, triangles, bones, uvCount, normalCount, tangentCount, submeshes;
            public Vector3 dimensions;
        }
        [Serializable] public sealed class ClipReport
        {
            public string name;
            public float duration, fps;
            public bool loop;
        }
        [Serializable] public sealed class Report
        {
            public string engine, scope;
            public float fit, cameraFov, cameraLookAt;
            public Vector3 cameraOffset, modelDimensions, reviewPosition;
            public MeshReport[] meshes;
            public ClipReport[] clips;
            public string[] hierarchy;
            public bool productionDependenciesUnchanged, rootMotionDisabled;
        }

        [MenuItem("Gravivore/Art Review/G-0 Production V3/Build and Capture")]
        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(Evidence); Directory.CreateDirectory(Data);
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh(); ConfigureImport();
            var material = MakeMaterial();
            var clips = AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
            foreach (var name in ClipNames)
                if (!clips.Any(c => c.name == name)) throw new InvalidOperationException("Missing imported clip " + name);
            var controllerPath = Root + "/G0_Production_V3_Review.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var state in machine.states) machine.RemoveState(state.state);
            foreach (var name in ClipNames)
            {
                var state = machine.AddState(name); state.motion = clips.Single(c => c.name == name); state.writeDefaultValues = false;
                if (name == "Idle") machine.defaultState = state;
            }
            var imported = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            var asset = UnityEngine.Object.Instantiate(imported); asset.name = "G0_Production_V3_ArtReview";
            var animator = asset.GetComponent<Animator>();
            if (animator == null) animator = asset.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var renderers = asset.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length != 3) throw new InvalidOperationException("Exactly three imported LOD skins required");
            foreach (var r in renderers) { r.sharedMaterials = new[] { material }; r.updateWhenOffscreen = true; }
            var lod = asset.GetComponent<LODGroup>(); if (lod == null) lod = asset.AddComponent<LODGroup>();
            var sorted = renderers.OrderBy(r => r.name).ToArray();
            lod.SetLODs(new[] { new LOD(.13f, new Renderer[]{sorted[0]}), new LOD(.065f,new Renderer[]{sorted[1]}), new LOD(.02f,new Renderer[]{sorted[2]}) });
            lod.fadeMode = LODFadeMode.None; lod.RecalculateBounds();
            PrefabUtility.SaveAsPrefabAsset(asset, Prefab); UnityEngine.Object.DestroyImmediate(asset);

            // Read canonical camera settings through its composition reference.
            var canonical = EditorSceneManager.OpenScene("Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity");
            var composition = canonical.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var serialized = new SerializedObject(composition);
            var settings = (CameraFollowSettings)serialized.FindProperty("_cameraSettings").objectReferenceValue;
            var offset = settings.Offset; var fov = settings.FieldOfView; var lookAt = settings.LookAtHeight;
            // The committed V2.1 stage already contains the exact main world and
            // existing enemy bindings. Reuse it without modifying its source.
            var scene = EditorSceneManager.OpenScene(G0V21ScaleReview.ScenePath);
            foreach (var g in scene.GetRootGameObjects().Where(g => g.name.StartsWith("G-0 V2.1") || g.name.StartsWith("Current production Tier0") || g.name.StartsWith("Authoritative player footprint")))
                UnityEngine.Object.DestroyImmediate(g);
            var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<UnityEngine.Camera>(true)).Single();
            foreach (var label in camera.GetComponentsInChildren<TextMesh>(true)) UnityEngine.Object.DestroyImmediate(label.gameObject);
            var stage = new Vector3(0,0,57);
            camera.fieldOfView = fov; camera.transform.position = stage + offset;
            camera.transform.LookAt(stage + Vector3.up * lookAt); camera.aspect = 9f/16f;
            var hero = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab),scene);
            hero.name = "G-0 V3 PRESENTATION ONLY"; hero.transform.position = stage; hero.transform.localScale = Vector3.one * PresentationFit;
            var heroAnimator = hero.GetComponent<Animator>(); heroAnimator.enabled = true;heroAnimator.Rebind();heroAnimator.Update(0);
            var reviewSkins=hero.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r=>r.name).ToArray();
            var heroLod = hero.GetComponent<LODGroup>(); heroLod.ForceLOD(0);
            var references = scene.GetRootGameObjects().Where(g => g.name.Contains("actual production binding") || g.name.Contains("Magnetar Guard production binding")).ToArray();
            foreach(var reference in references) reference.SetActive(false);
            Sample(clips, "Idle", hero, 0);
            Capture(camera,hero,"06_gameplay_camera.png",1080,1920);
            Capture(camera,hero,"07_phone_size.png",360,640);
            foreach(var reference in references.Where(g => g.name.StartsWith("scout-drone") || g.name.StartsWith("warden"))) reference.SetActive(true);
            Capture(camera,hero,"08_ordinary_enemy.png",1080,1920);
            foreach(var reference in references) reference.SetActive(reference.name.Contains("Magnetar"));
            Capture(camera,hero,"09_magnetar.png",1080,1920);
            foreach(var reference in references) reference.SetActive(false);
            Sample(clips,"Run",hero,.20f); Capture(camera,hero,"10_movement.png",1080,1920);
            Sample(clips,"Attack",hero,.333333f); Capture(camera,hero,"11_attack.png",1080,1920);
            Sample(clips,"Death",hero,1.5f); Capture(camera,hero,"12_death.png",1080,1920);
            // LOD evidence uses the same camera/settings and atlas, with no zoom.
            Sample(clips,"Idle",hero,0);
            for(var i=0;i<3;i++){heroLod.ForceLOD(i);Capture(camera,hero,"LOD"+i+"_phone.png",360,640,i);}
            heroLod.ForceLOD(-1);
            Capture(camera,hero,"07b_phone_automatic_lod.png",360,640,0,false);
            // Deterministic contact sheet samples actual imported clips.
            foreach(var name in ClipNames)
            {
                heroLod.ForceLOD(0); var clip=clips.Single(c=>c.name==name);
                for(var i=0;i<5;i++){Sample(clips,name,hero,clip.length*i/4);Capture(camera,hero,name+"_"+i+".png",360,640);}
            }
            Sample(clips,"Idle",hero,0); heroLod.ForceLOD(-1); heroAnimator.enabled=true;
            foreach(var reference in references) reference.SetActive(true);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene,ScenePath);
            var report=new Report {engine=Application.unityVersion,scope="Isolated V3 production asset with unchanged Chapter01 camera and committed world/reference bindings",fit=PresentationFit,
                cameraFov=fov,cameraOffset=offset,cameraLookAt=lookAt,reviewPosition=stage,
                modelDimensions=reviewSkins[0].sharedMesh.bounds.size,
                hierarchy=hero.GetComponentsInChildren<Transform>().Select(t=>t.name).ToArray(),
                meshes=reviewSkins.Select(r=>new MeshReport {name=r.name,vertices=r.sharedMesh.vertexCount,triangles=(int)r.sharedMesh.GetIndexCount(0)/3,
                    bones=r.bones.Length,uvCount=r.sharedMesh.uv.Length,normalCount=r.sharedMesh.normals.Length,tangentCount=r.sharedMesh.tangents.Length,submeshes=r.sharedMesh.subMeshCount,dimensions=r.sharedMesh.bounds.size}).ToArray(),
                clips=clips.Select(c=>new ClipReport {name=c.name,duration=c.length,fps=c.frameRate,loop=c.isLooping}).ToArray(),
                productionDependenciesUnchanged=!AssetDatabase.GetDependencies("Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity",true).Contains(Model),rootMotionDisabled=!heroAnimator.applyRootMotion};
            File.WriteAllText(Data+"/unity_import_validation.json",JsonUtility.ToJson(report,true)+"\n");
            if(!report.productionDependenciesUnchanged || hero.GetComponentsInChildren<Collider>(true).Length!=0) throw new InvalidOperationException("Review isolation violated");
            Debug.Log("G0_V3_CAPTURE_AND_IMPORT_PASS "+JsonUtility.ToJson(report));
        }

        private static void ConfigureImport()
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Model);
            importer.globalScale=1; importer.useFileScale=true; importer.bakeAxisConversion=true;
            importer.importAnimation=true; importer.animationType=ModelImporterAnimationType.Generic; importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.motionNodeName="ROOT"; importer.optimizeGameObjects=false; importer.importCameras=false; importer.importLights=false;
            importer.importBlendShapes=false; importer.importVisibility=false; importer.addCollider=false; importer.isReadable=true;
            importer.importNormals=ModelImporterNormals.Import; importer.importTangents=ModelImporterTangents.CalculateMikk;
            importer.animationCompression=ModelImporterAnimationCompression.Off; importer.materialImportMode=ModelImporterMaterialImportMode.None;
            importer.maxBonesPerVertex=1; importer.skinWeights=ModelImporterSkinWeights.Custom;
            importer.SaveAndReimport();
            var takes=importer.defaultClipAnimations;
            if(takes.Length!=5) throw new InvalidOperationException("Expected five FBX takes, received "+takes.Length);
            foreach(var take in takes)
            {
                var name=ClipNames.SingleOrDefault(n=>take.name.EndsWith(n,StringComparison.Ordinal));
                if(name==null) throw new InvalidOperationException("Unexpected FBX take "+take.name);
                take.name=name;take.loopTime=name=="Idle"||name=="Run";take.loopPose=false;
                take.keepOriginalOrientation=true;take.keepOriginalPositionXZ=true;take.keepOriginalPositionY=true;
                take.lockRootRotation=true;take.lockRootPositionXZ=true;take.lockRootHeightY=true;
            }
            importer.clipAnimations=takes;importer.SaveAndReimport();
        }

        private static Material MakeMaterial()
        {
            foreach(var name in new[]{"BaseColor","MetallicSmoothness","Emission"})
            {
                var path=Root+"/Textures/G0_V3_"+name+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.sRGBTexture=name!="MetallicSmoothness";importer.alphaSource=TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency=false;importer.mipmapEnabled=true;importer.maxTextureSize=2048;
                importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Compressed;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=1024;android.format=TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            }
            var pathMat=Root+"/Materials/G0_V3_IndustrialAtlas.mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(pathMat);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,pathMat);}
            mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/G0_V3_BaseColor.png"));mat.SetColor("_BaseColor",Color.white);
            mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/G0_V3_MetallicSmoothness.png"));mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            mat.SetFloat("_Smoothness",1);mat.SetFloat("_SmoothnessTextureChannel",0);
            mat.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/G0_V3_Emission.png"));mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.white*1.4f);
            mat.SetFloat("_Surface",0);mat.SetFloat("_Cull",2);EditorUtility.SetDirty(mat);return mat;
        }

        private static void Sample(AnimationClip[] clips,string name,GameObject hero,float seconds)
        {
            var clip=clips.Single(c=>c.name==name);var animator=hero.GetComponent<Animator>();
            animator.enabled=true;animator.Rebind();animator.Update(0);animator.Play(name,0,Mathf.Clamp01(seconds/clip.length));animator.Update(0);
            Debug.Log("G0_V3_POSE "+name+" "+seconds+" torso="+hero.GetComponentsInChildren<Transform>().Single(t=>t.name=="TORSO").localRotation+" shoulder="+hero.GetComponentsInChildren<Transform>().Single(t=>t.name=="R_SHOULDER").localRotation+" bindings="+AnimationUtility.GetCurveBindings(clip).Length);
        }
        private static void Capture(UnityEngine.Camera camera,GameObject hero,string name,int width,int height,int lodIndex=0,bool bakePose=true)
        {
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var prior=RenderTexture.active;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            var skins=hero.GetComponentsInChildren<SkinnedMeshRenderer>().OrderBy(r=>r.name).ToArray();
            var states=skins.Select(r=>r.enabled).ToArray();var group=hero.GetComponent<LODGroup>();var groupEnabled=group.enabled;
            GameObject posed=null;Mesh posedMesh=null;
            // Batch editor Camera.Render does not advance Unity's skinned draw
            // cache. BakeMesh uses the actual sampled imported bone matrices;
            // this temporary renderer makes those poses visible in that same
            // URP camera. It is deleted before saving the review scene.
            if(bakePose)
            {
                group.enabled=false;foreach(var skin in skins)skin.enabled=false;
                var source=skins[lodIndex];posedMesh=new Mesh();source.BakeMesh(posedMesh,false);
                posed=new GameObject("TEMP imported clip pose",typeof(MeshFilter),typeof(MeshRenderer));
                posed.transform.SetParent(source.transform,false);posed.GetComponent<MeshFilter>().sharedMesh=posedMesh;
                posed.GetComponent<MeshRenderer>().sharedMaterials=source.sharedMaterials;
                if(name=="06_gameplay_camera.png")
                {
                    var points=posedMesh.vertices.Select(v=>posed.transform.TransformPoint(v)).ToArray();
                    var height=points.Max(v=>v.y)-points.Min(v=>v.y);
                    var ground=points.Min(v=>v.y);
                    if(Mathf.Abs(height-1.54f)>.002f||Mathf.Abs(ground)>.002f)throw new InvalidOperationException("Captured rest scale/ground mismatch: height="+height+" ground="+ground);
                    Debug.Log("G0_V3_CAMERA_REST_SCALE_PASS height="+height+" ground="+ground);
                }
            }
            try {camera.targetTexture=target;camera.aspect=(float)width/height;for(var i=0;i<3;i++)camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Evidence+"/"+name,texture.EncodeToPNG());}
            finally {camera.targetTexture=null;RenderTexture.active=prior;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);
                if(posed!=null)UnityEngine.Object.DestroyImmediate(posed);if(posedMesh!=null)UnityEngine.Object.DestroyImmediate(posedMesh);
                for(var i=0;i<skins.Length;i++)skins[i].enabled=states[i];group.enabled=groupEnabled;}
        }
    }
}
