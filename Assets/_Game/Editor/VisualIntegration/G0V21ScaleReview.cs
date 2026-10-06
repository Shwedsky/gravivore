using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using CameraSettings = Gravivore.Presentation.Camera.CameraFollowSettings;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Opt-in static review. Never binds a model into the runtime or saves Chapter01.</summary>
    public static class G0V21ScaleReview
    {
        public const string Root = "Assets/_Game/ArtReview/G0V21";
        public const string Model = Root + "/Models/G0_Bipedal_V21_Review.fbx";
        public const string ScenePath = Root + "/G0_V21_ScaleReview.unity";
        public const string Evidence = "docs/visual-production-v2/g0-bipedal-v2/evidence-v21/unity";
        public const string Data = "docs/visual-production-v2/g0-bipedal-v2/data-v21";
        public static readonly float[] Variants = { .9f, 1f, 1.1f };
        private static readonly Vector3 Stage = new Vector3(0, 0, 57);

        [Serializable] public sealed class Projection
        {
            public string name;
            public Vector3 dimensions;
            public Vector2 minPixels, maxPixels;
            public Vector2 pixels;
        }
        [Serializable] public sealed class Variant
        {
            public float multiplier, sourceToUnityScale, lateralGateMargin, overheadGateMargin, visualBeyondColliderPerSide;
            public Projection hero, scout, warden, magnetar;
        }
        [Serializable] public sealed class Report
        {
            public string engine = Application.unityVersion;
            public string scope = "Static Unity URP review of actual Chapter01 environment and production visual bindings; no AI, HUD, combat or prefab replacement.";
            public Vector3 productionSpawn, reviewPosition, cameraOffset, sourceDimensions, productionPlayerDimensions, gateSize, importedChestCenter;
            public float cameraFov, cameraLookAt, cameraDamping, colliderRadius, colliderHeight, baseFit;
            public Vector3 colliderCenter;
            public int importedTriangles, importedRenderers;
            public Variant[] variants;
        }

        [MenuItem("Gravivore/Art Review/G-0 V2.1/Build and Capture Scale Review")]
        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(Evidence); Directory.CreateDirectory(Data);
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh(); ConfigureImport();
            var canonical = EditorSceneManager.OpenScene("Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity", OpenSceneMode.Single);
            var composition = canonical.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var authoring = new SerializedObject(composition);
            var cameraSettings = (CameraSettings)authoring.FindProperty("_cameraSettings").objectReferenceValue;
            var catalog = (S15VisualCatalog)authoring.FindProperty("_s15VisualCatalog").objectReferenceValue;
            var evolution = (EvolutionDefinition)authoring.FindProperty("_evolutionDefinition").objectReferenceValue;
            var world = ((Chapter01WorldDefinition)authoring.FindProperty("_worldDefinition").objectReferenceValue).Configuration;
            var palette = (PresentationMaterialPalette)authoring.FindProperty("_materialPalette").objectReferenceValue;
            var report = new Report { productionSpawn = authoring.FindProperty("_playerSpawn").vector3Value,
                reviewPosition = Stage, cameraOffset = cameraSettings.Offset, cameraFov = cameraSettings.FieldOfView,
                cameraLookAt = cameraSettings.LookAtHeight, cameraDamping = cameraSettings.PositionDamping,
                colliderRadius = .42f, colliderHeight = 1.4f, colliderCenter = new Vector3(0,.7f,0), gateSize = world.EliteGate.Size };

            // Generate world through the same production presenter/data, in an unsaved temporary scene.
            var temporary = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(temporary);
            var tempRoot = new GameObject("TEMP Chapter01 source world");
            var envObject = UnityEngine.Object.Instantiate(composition.VisualEnvironment.gameObject, tempRoot.transform);
            var environment = envObject.GetComponent<ChapterVisualEnvironment>(); environment.Initialize();
            var worldObject = new GameObject("TEMP production world presenter"); worldObject.transform.SetParent(tempRoot.transform);
            var presenter = worldObject.AddComponent<Chapter01WorldPresenter>();
            presenter.Initialize(world, new WorldUnlockState(world.EliteGate.Id, world.BossGate.Id, "magnetar-guard"), palette.LitMaterial, catalog, environment);
            EditorSceneManager.SaveScene(temporary, Root + "/TemporaryWorld.unity");

            var review = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(review);
            var staticWorld = new GameObject("Chapter01 actual static environment - no gameplay authority");
            var materials = new Dictionary<Material, Material>();
            foreach (var renderer in tempRoot.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.enabled) continue;
                var filter = renderer.GetComponent<MeshFilter>(); if (filter == null) continue;
                var copy = new GameObject(renderer.name, typeof(MeshFilter), typeof(MeshRenderer));
                copy.transform.SetParent(staticWorld.transform);
                copy.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
                copy.transform.localScale = renderer.transform.lossyScale;
                copy.GetComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                var target = copy.GetComponent<MeshRenderer>(); EditorUtility.CopySerialized(renderer, target);
                target.sharedMaterials = renderer.sharedMaterials.Select(m => PersistMaterial(m, materials)).ToArray();
            }
            foreach (var light in tempRoot.GetComponentsInChildren<Light>())
            {
                var copy = new GameObject(light.name, typeof(Light)); copy.transform.SetParent(staticWorld.transform);
                copy.transform.SetPositionAndRotation(light.transform.position, light.transform.rotation);
                EditorUtility.CopySerialized(light, copy.GetComponent<Light>());
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.22f,.25f,.28f);

            var production = UnityEngine.Object.Instantiate(evolution.Catalog.TierPrefabs[0]);
            production.name = "Current production Tier0 reference (hidden in main comparison)";
            report.productionPlayerDimensions = BoundsOf(production).size;
            production.transform.position = new Vector3(-1.7f,0,57); production.SetActive(false);
            var scout = Enemy(catalog,"scout-drone",new Vector3(-2.5f,0,54.8f));
            var warden = Enemy(catalog,"warden",new Vector3(2.5f,0,54.8f));
            var guardRoot = new GameObject("Magnetar Guard production binding"); guardRoot.transform.position = new Vector3(2.1f,0,57f);
            var guard = environment.Definition.Elite.InstantiateUnder(guardRoot.transform);
            var hero = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
            hero.name = "G-0 V2.1 PRESENTATION ONLY";
            report.importedChestCenter=hero.GetComponentsInChildren<Renderer>().Single(r=>r.name.StartsWith("Integrated cyan")).bounds.center;
            // FBX probe: chest center Z=-0.29, rear containment Z>0. Imported forward is -Z.
            hero.transform.SetPositionAndRotation(Stage,Quaternion.identity);
            report.sourceDimensions = BoundsOf(hero).size;
            // Nominal height derives from authoritative player capsule. Authoring meters stay intact.
            report.baseFit = report.colliderHeight / report.sourceDimensions.y;
            report.importedRenderers = hero.GetComponentsInChildren<Renderer>().Length;
            report.importedTriangles = hero.GetComponentsInChildren<MeshFilter>().Sum(f => (int)(f.sharedMesh.GetIndexCount(0)/3));
            AssignHeroMaterials(hero);
            Footprint(Stage,report.colliderRadius);
            var camera = new GameObject("Chapter01 gameplay camera settings (static settled pose)",typeof(UnityEngine.Camera)).GetComponent<UnityEngine.Camera>();
            camera.fieldOfView = cameraSettings.FieldOfView; camera.nearClipPlane=.1f; camera.farClipPlane=100;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.035f,.047f,.06f);
            camera.aspect=9f/16f; camera.transform.position=Stage+cameraSettings.Offset;
            camera.transform.LookAt(Stage+Vector3.up*cameraSettings.LookAtHeight);
            var cameraData=camera.GetUniversalAdditionalCameraData(); cameraData.renderPostProcessing=false;
            var title=CameraLabel(camera,"G-0 V2.1  |  UNITY SCALE REVIEW",new Vector2(.5f,.95f),.004f);
            var heroLabel=CameraLabel(camera,"G-0",new Vector2(.5f,.43f),.0028f);
            var scoutLabel=CameraLabel(camera,"SCOUT",new Vector2(.21f,.32f),.0028f);
            var wardenLabel=CameraLabel(camera,"WARDEN",new Vector2(.81f,.32f),.0028f);
            var guardLabel=CameraLabel(camera,"MAGNETAR",new Vector2(.78f,.43f),.0028f);
            // Warm shader/light state before recording identical comparison conditions.
            var warm=new RenderTexture(1080,1920,24);camera.targetTexture=warm;
            for(var frame=0;frame<3;frame++)camera.Render();camera.targetTexture=null;warm.Release();UnityEngine.Object.DestroyImmediate(warm);
            report.variants=new Variant[3]; var images=new List<Texture2D>();
            for(var i=0;i<3;i++)
            {
                hero.transform.localScale=Vector3.one*(report.baseFit*Variants[i]);
                title.text=$"G-0 V2.1  |  {Variants[i]:0.00}x";
                var bounds=BoundsOf(hero);
                report.variants[i]=new Variant { multiplier=Variants[i],sourceToUnityScale=report.baseFit*Variants[i],
                    hero=Project(camera,hero),scout=Project(camera,scout),warden=Project(camera,warden),magnetar=Project(camera,guardRoot),
                    lateralGateMargin=(world.EliteGate.Size.x-bounds.size.x)/2,
                    overheadGateMargin=world.EliteGate.Size.y-.02f-bounds.size.y,
                    visualBeyondColliderPerSide=Mathf.Max(0,bounds.size.x/2-report.colliderRadius) };
                images.Add(Capture(camera,$"0{i+1}_G0_scale_{Mathf.RoundToInt(Variants[i]*100):000}_gameplay.png",1080,1920));
            }
            Board(images,"04_G0_scale_comparison_board.png");
            hero.transform.localScale=Vector3.one*report.baseFit;
            title.text="G-0 1.00x  |  ORDINARY ENEMIES";
            guardRoot.SetActive(false);
            guardLabel.gameObject.SetActive(false);
            Capture(camera,"05_G0_vs_ordinary_enemy.png",1080,1920);
            guardRoot.SetActive(true); scout.SetActive(false); warden.SetActive(false);
            guardLabel.gameObject.SetActive(true);scoutLabel.gameObject.SetActive(false);wardenLabel.gameObject.SetActive(false);
            title.text="G-0 1.00x  |  MAGNETAR GUARD";
            Capture(camera,"06_G0_vs_Magnetar.png",1080,1920);
            guardRoot.SetActive(false);
            var oldPosition=hero.transform.position;
            // Review at the actual gate plane; frame intact, cells hidden only for an open-clearance view.
            hero.transform.position=new Vector3(0,0,world.EliteGate.Position.z-2);
            title.text="G-0 1.00x  |  GATE APPROACH";
            guardLabel.gameObject.SetActive(false);
            var cells=staticWorld.GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith("Energy Cell")||r.name.StartsWith("Emitter ")).ToArray();
            foreach(var cell in cells) cell.enabled=false;
            Capture(camera,"07_G0_gate_clearance.png",1080,1920);
            hero.transform.position=new Vector3(0,0,world.EliteGate.Position.z);
            title.text="G-0 1.00x  |  GATE PLANE OCCLUSION";
            Capture(camera,"10_G0_actual_gate_plane_occlusion.png",1080,1920);
            foreach(var cell in cells) cell.enabled=true;
            hero.transform.position=oldPosition; scout.SetActive(true); warden.SetActive(true); guardRoot.SetActive(true);
            scoutLabel.gameObject.SetActive(true);wardenLabel.gameObject.SetActive(true);guardLabel.gameObject.SetActive(true);
            var phones=new List<Texture2D>();
            foreach(var scale in Variants) { title.text=$"G-0 V2.1  |  {scale:0.00}x  |  360x640";hero.transform.localScale=Vector3.one*report.baseFit*scale; phones.Add(Capture(camera,$"phone_{Mathf.RoundToInt(scale*100):000}.png",360,640)); }
            Board(phones,"08_G0_phone_size_readability.png");
            hero.transform.localScale=Vector3.one*report.baseFit;
            title.text="G-0 1.00x  |  CURRENT PLAYER AT SPAWN";
            var actorDelta=report.productionSpawn-Stage;
            hero.transform.position+=actorDelta;scout.transform.position+=actorDelta;warden.transform.position+=actorDelta;guardRoot.transform.position+=actorDelta;
            production.transform.position=new Vector3(-1.7f,0,report.productionSpawn.z);production.SetActive(true);
            camera.transform.position=report.productionSpawn+cameraSettings.Offset;
            Capture(camera,"09_G0_current_player_spawn_reference.png",1080,1920);
            hero.transform.position-=actorDelta;scout.transform.position-=actorDelta;warden.transform.position-=actorDelta;guardRoot.transform.position-=actorDelta;
            camera.transform.position=Stage+cameraSettings.Offset;production.SetActive(false);
            title.text="G-0 V2.1  |  1.00x  |  STATIC REVIEW";
            File.WriteAllText(Data+"/unity_scale_metrics.json",JsonUtility.ToJson(report,true)+"\n");
            if(hero.GetComponentsInChildren<Collider>(true).Length!=0||hero.GetComponentsInChildren<MonoBehaviour>(true).Length!=0)
                throw new InvalidOperationException("Review hero must be geometry only.");
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(review,ScenePath);
            AssetDatabase.DeleteAsset(Root + "/TemporaryWorld.unity");
            // Quit batch process without saving either canonical or temporary scenes.
            Debug.Log("G0_V21_UNITY_SCALE_REVIEW_CAPTURED "+JsonUtility.ToJson(report));
        }

        private static void ConfigureImport()
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Model);
            importer.globalScale=1; importer.useFileScale=true; importer.bakeAxisConversion=true;
            importer.importAnimation=false; importer.animationType=ModelImporterAnimationType.None;
            importer.importCameras=false; importer.importLights=false; importer.importBlendShapes=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard; importer.isReadable=true;
            importer.SaveAndReimport();
        }
        private static Material PersistMaterial(Material source,Dictionary<Material,Material> map)
        {
            if(AssetDatabase.Contains(source)) return source;
            if(map.TryGetValue(source,out var found)) return found;
            var path=$"{Root}/Materials/World_{map.Count:000}.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(source){hideFlags=HideFlags.None}; AssetDatabase.CreateAsset(material,path); }
            else EditorUtility.CopySerialized(source,material);
            material.hideFlags=HideFlags.None; map.Add(source,material); return material;
        }
        private static GameObject Enemy(S15VisualCatalog catalog,string id,Vector3 position)
        {
            if(!catalog.TryGetEnemy(id,out var recipe)) throw new InvalidOperationException(id);
            var root=new GameObject(id+" actual production binding"); root.transform.position=position;
            S15VisualFactory.Build(root.transform,recipe,catalog); root.transform.rotation=Quaternion.Euler(0,180,0); return root;
        }
        private static void AssignHeroMaterials(GameObject hero)
        {
            var cache=new Dictionary<string,Material>();
            foreach(var renderer in hero.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterials=renderer.sharedMaterials.Select(source=> {
                    var name=source.name;
                    if(cache.TryGetValue(name,out var old)) return old;
                    var cyan=name.Contains("cyan"); var pale=name.Contains("ceramic"); var steel=name.Contains("actuator"); var copper=name.Contains("copper"); var dark=name.Contains("graphite");
                    var color=cyan?new Color(.008f,.65f,.85f):pale?new Color(.29f,.38f,.41f):steel?new Color(.16f,.21f,.24f):copper?new Color(.45f,.19f,.075f):dark?new Color(.038f,.054f,.068f):new Color(.075f,.12f,.155f);
                    var path=Root+"/Materials/Hero_"+cache.Count+".mat";
                    var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(m==null) {m=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m,path);}
                    m.name=name; m.color=color; m.SetFloat("_Metallic",.72f); m.SetFloat("_Smoothness",.65f);
                    if(cyan) {m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*2.2f);}
                    EditorUtility.SetDirty(m); cache.Add(name,m); return m;
                }).ToArray();
            }
        }
        public static Bounds BoundsOf(GameObject root)
        {
            var renderers=root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();
            if(renderers.Length==0) throw new InvalidOperationException("No visible geometry: "+root.name);
            var bounds=renderers[0].bounds; foreach(var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds); return bounds;
        }
        private static Projection Project(UnityEngine.Camera camera,GameObject root)
        {
            var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                if(!filter.GetComponent<Renderer>().enabled) continue;
                var b=filter.sharedMesh.bounds;
                for(var i=0;i<8;i++) { var p=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    var v=camera.WorldToViewportPoint(filter.transform.TransformPoint(p));
                    var pixel=new Vector2(v.x*1080,(1-v.y)*1920);min=Vector2.Min(min,pixel);max=Vector2.Max(max,pixel); }
            }
            return new Projection{name=root.name,dimensions=BoundsOf(root).size,minPixels=min,maxPixels=max,pixels=max-min};
        }
        private static void Footprint(Vector3 position,float radius)
        {
            var go=new GameObject("Authoritative player footprint: radius 0.42, presentation guide only",typeof(LineRenderer));
            var line=go.GetComponent<LineRenderer>();line.loop=true;line.positionCount=64;line.startWidth=line.endWidth=.015f;
            var path=Root+"/Materials/Footprint.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Unlit")){color=new Color(.15f,.65f,.75f)};AssetDatabase.CreateAsset(m,path);}
            line.sharedMaterial=m;
            for(var i=0;i<64;i++)line.SetPosition(i,position+new Vector3(Mathf.Cos(i*Mathf.PI/32)*radius,.025f,Mathf.Sin(i*Mathf.PI/32)*radius));
        }
        private static Texture2D Capture(UnityEngine.Camera camera,string filename,int width,int height)
        {
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active; camera.targetTexture=target; camera.aspect=(float)width/height;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try { camera.Render(); RenderTexture.active=target; texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Evidence+"/"+filename,texture.EncodeToPNG());return texture; }
            finally {camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);}
        }
        private static TextMesh CameraLabel(UnityEngine.Camera camera,string text,Vector2 viewport,float size)
        {
            var go=new GameObject("Review annotation: "+text,typeof(TextMesh));
            go.transform.SetParent(camera.transform,false);
            go.transform.position=camera.ViewportToWorldPoint(new Vector3(viewport.x,viewport.y,2));
            go.transform.rotation=camera.transform.rotation;
            var label=go.GetComponent<TextMesh>();label.text=text;label.anchor=TextAnchor.MiddleCenter;
            label.alignment=TextAlignment.Center;label.fontSize=64;label.characterSize=size;
            label.color=new Color(.8f,.9f,.95f);return label;
        }
        private static void Board(List<Texture2D> images,string filename)
        {
            var w=images[0].width;var h=images[0].height;var board=new Texture2D(w*3,h,TextureFormat.RGB24,false);
            for(var i=0;i<3;i++)board.SetPixels(i*w,0,w,h,images[i].GetPixels());board.Apply();
            File.WriteAllBytes(Evidence+"/"+filename,board.EncodeToPNG());UnityEngine.Object.DestroyImmediate(board);
            foreach(var image in images)UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
