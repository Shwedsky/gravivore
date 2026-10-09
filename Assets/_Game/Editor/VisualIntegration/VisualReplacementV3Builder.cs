using System;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.World;
using Gravivore.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Offline presentation authoring. Never edits gameplay proxies or content balance.</summary>
    public static class VisualReplacementV3Builder
    {
        public const string Root = "Assets/_Game/Content/VisualReplacementV3";
        public const string Layer = "Chapter 01 Visual Replacement V3";
        public static string Prefab(string name) => Root + "/Prefabs/" + name + ".prefab";
        public static void BuildRepair() => Environment(false);
        public static void BuildEnvironment() => Environment(true);
        public static void BuildActors()
        {
            AssetDatabase.Refresh();
            var worn=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/VR3_WornIndustrialAtlas.mat");
            foreach(var pair in new[]{("VisualSlice","Scout_V1"),("VisualSlice","Cutter_V1"),("VisualSlice","Magnetar_V1"),("Chapter01Production","Warden_V1"),("Chapter01Production","ArcDrone_V1"),("Chapter01Production","Carrier_V1"),("Chapter01V3","Custodian_V3")})
            {
                var path="Assets/_Game/Content/"+pair.Item1+"/Prefabs/"+pair.Item2+".prefab";
                var obj=PrefabUtility.LoadPrefabContents(path);
                foreach(var renderer in obj.GetComponentsInChildren<Renderer>(true))renderer.sharedMaterials=new[]{worn};
                PrefabUtility.SaveAsPrefabAsset(obj,path);PrefabUtility.UnloadPrefabContents(obj);
            }
            AssetDatabase.SaveAssets();Chapter01V3Builder.ValidateOrThrow();
            Debug.Log("VISUAL_REPLACEMENT_V3_ACTORS_PASS");
        }
        public static void BuildEquipment()
        {
            AssetDatabase.Refresh();
            var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/VR3_WornIndustrialAtlas.mat");
            var obj=new GameObject("Emitter_M0");
            // R_TOOL's local right points inward on the approved rig. Lift and
            // move the housing outward so the shoulder cannot hide the emitter.
            obj.transform.localPosition=new Vector3(-.36f,.28f,.16f);
            obj.transform.localScale=Vector3.one*1.5f;
            var muzzle=Group(obj.transform,"Muzzle");muzzle.localPosition=new Vector3(0,.025f,1.03f);
            for(var rank=1;rank<=5;rank++)
            {
                var path=Root+"/Models/M0_Rank"+rank+".fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.materialImportMode=ModelImporterMaterialImportMode.None;
                importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.None;
                importer.bakeAxisConversion=true;importer.isReadable=false;importer.SaveAndReimport();
                var group=Group(obj.transform,"Rank"+rank);
                var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),group,false);
                foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=new[]{material};
                Group(group,"RankMuzzle").localPosition=new Vector3(0,.025f,1.03f+(rank-1)*.16f);
                RenderThumbnail(model,rank);group.gameObject.SetActive(rank==1);
            }
            PrefabUtility.SaveAsPrefabAsset(obj,Chapter01V3Builder.Prefab("Emitter_M0"));Object.DestroyImmediate(obj);
            AssetDatabase.Refresh();AssetDatabase.SaveAssets();
            Debug.Log("VISUAL_REPLACEMENT_V3_EQUIPMENT_PASS");
        }
        private static void RenderThumbnail(GameObject model,int rank)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                var copy=Object.Instantiate(model);preview.AddSingleGO(copy);
                var rs=copy.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
                preview.camera.orthographic=true;preview.camera.orthographicSize=bounds.extents.magnitude*.68f;
                preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=30;
                preview.camera.transform.position=bounds.center+new Vector3(2.3f,1.8f,1.4f);preview.camera.transform.LookAt(bounds.center);
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.035f,.05f,.063f,1);
                preview.lights[0].intensity=1.8f;preview.lights[0].transform.rotation=Quaternion.Euler(35,-25,0);
                preview.lights[1].intensity=.9f;preview.lights[1].transform.rotation=Quaternion.Euler(315,130,0);
                preview.BeginStaticPreview(new Rect(0,0,512,320));preview.Render(true);
                var texture=preview.EndStaticPreview();File.WriteAllBytes(Root+"/Textures/M0_Thumbnail"+rank+".png",texture.EncodeToPNG());Object.DestroyImmediate(texture);
            }
            finally{preview.Cleanup();}
        }
        private static Sprite UiSprite(string name,bool sliced=false)
        {
            var path=Root+"/Textures/"+name+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=512;
            importer.spriteBorder=sliced?new Vector4(12,12,12,12):Vector4.zero;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public static void BuildUI()
        {
            AssetDatabase.Refresh();var path=Root+"/ProductionUiSkin.asset";
            var worn=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/VR3_WornIndustrialAtlas.mat");
            worn.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;worn.EnableKeyword("_EMISSION");EditorUtility.SetDirty(worn);
            var skin=AssetDatabase.LoadAssetAtPath<ProductionUiSkinDefinition>(path);
            if(skin==null){skin=ScriptableObject.CreateInstance<ProductionUiSkinDefinition>();AssetDatabase.CreateAsset(skin,path);}
            var data=new SerializedObject(skin);
            data.FindProperty("_frame").objectReferenceValue=UiSprite("UI_inventoryselectedfalse",true);
            data.FindProperty("_selected").objectReferenceValue=UiSprite("UI_inventoryselectedtrue",true);
            data.FindProperty("_utility").objectReferenceValue=UiSprite("UtilityBar",true);
            data.FindProperty("_button").objectReferenceValue=UiSprite("UI_buttonscolorblack",true);
            data.FindProperty("_divider").objectReferenceValue=UiSprite("UI_dividersborderstriped");
            var thumbnails=data.FindProperty("_weaponThumbnails");thumbnails.arraySize=5;
            for(var i=0;i<5;i++)thumbnails.GetArrayElementAtIndex(i).objectReferenceValue=UiSprite("M0_Thumbnail"+(i+1));
            data.ApplyModifiedPropertiesWithoutUndo();skin.ValidateOrThrow();
            var definition=AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset");
            data=new SerializedObject(definition);data.FindProperty("_uiSkin").objectReferenceValue=skin;data.ApplyModifiedPropertiesWithoutUndo();
            Directory.CreateDirectory("Assets/StreamingAssets");File.Copy("ThirdPartyNotices.md","Assets/StreamingAssets/ThirdPartyNotices.txt",true);
            foreach(var name in new[]{"HostileSoft","PlayerSoft","PlayerStreak","RepairSoft","RepairStreak"})
            {
                var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Content/Presentation/Phase6B/VFX/Materials/M_Phase6B_"+name+".mat");
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/VR3_"+(name.Contains("Streak")||name=="HostileSoft"?"Beam":"Energy")+"Mask.png");
                material.SetTexture("_BaseMap",texture);material.SetTexture("_MainTex",texture);EditorUtility.SetDirty(material);
            }
            var sparkPath=Root+"/Materials/VR3_MetalSpark.mat";
            var spark=AssetDatabase.LoadAssetAtPath<Material>(sparkPath);
            if(spark==null){spark=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Content/Presentation/Phase6B/VFX/Materials/M_Phase6B_HostileSoft.mat"));AssetDatabase.CreateAsset(spark,sparkPath);}
            var sparkTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/VR3_SparkMask.png");spark.SetTexture("_BaseMap",sparkTexture);spark.SetTexture("_MainTex",sparkTexture);EditorUtility.SetDirty(spark);
            const string impactPath="Assets/_Game/Content/Presentation/Phase6B/VFX/Prefabs/PFX_HostileImpact.prefab";
            var impact=PrefabUtility.LoadPrefabContents(impactPath);var effectData=new SerializedObject(impact.GetComponent<Gravivore.Presentation.AudioVfx.Phase6BVfxInstance>());
            effectData.FindProperty("_material").objectReferenceValue=spark;effectData.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(impact,impactPath);PrefabUtility.UnloadPrefabContents(impact);
            AssetDatabase.Refresh();AssetDatabase.SaveAssets();Debug.Log("VISUAL_REPLACEMENT_V3_UI_PASS");
        }
        private static void Environment(bool full)
        {
            PrepareStatic();
            var scene = EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var composition = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var env = composition.VisualEnvironment;
            var before = AuthorityFingerprint(env);
            var prior = env.Floor.Find(Layer); if (prior != null) Object.DestroyImmediate(prior.gameObject);
            var layer = Group(env.Floor, Layer);
            var deck = Group(layer, "Layered worn deck");
            var tiles = env.Floor.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("Deck_V2_", StringComparison.Ordinal) && !t.parent.name.StartsWith("Deck_V2_",StringComparison.Ordinal) && !t.IsChildOf(layer)).ToArray();
            foreach (var tile in tiles)
            {
                var replace = full || tile.position.z < -18 && tile.position.z > -40 && Mathf.Abs(tile.position.x) < 7;
                foreach (var renderer in tile.GetComponentsInChildren<Renderer>(true)) renderer.enabled = !replace;
                if (!replace) continue;
                var index = Mathf.Abs(Mathf.RoundToInt(tile.position.z))*7+(tile.position.x>0?3:0);
                var name = index % 5 == 0 ? "VR3_platformdarkplates" : index % 5 == 1 || index % 5 == 3 ? "VR3_platformmetal" : "VR3_FracturedDeck";
                Fit(deck, name, new Vector3(tile.position.x,.12f,tile.position.z), new Vector3(5.96f, .075f, 5.96f), index % 4 * 90);
                // Raised seam and grated construction are small enough to preserve floor readability.
                if (index % 3 != 1) Fit(deck,"VR3_ServiceGrate",new Vector3(tile.position.x+2.60f,.09f,tile.position.z),new Vector3(.52f,.09f,5.82f));
            }
            var hub = Group(layer,"Repair Hub connected service machinery");
            // These donors fit inside the existing service machinery authority footprint.
            ReplaceProxyArt(env, hub, full);
            Fit(hub,"VR3_ServiceRun",new Vector3(-3.8f,.11f,-29.8f),new Vector3(.65f,.18f,4),90);
            Fit(hub,"VR3_ServiceRun",new Vector3(-5.7f,.11f,-27.8f),new Vector3(.65f,.18f,4));
            Fit(hub,"VR3_ServiceGrate",new Vector3(3.3f,.14f,-31),new Vector3(.65f,.1f,3.8f));
            if (full)
            {
                var services=Group(layer,"Connected sector edge services");
                // Reuse occupied conduit proxies along the main service route.
                foreach(var z in new[]{-32,-24,-16,-8,0,8,16,24,32})foreach(var side in new[]{-1,1})
                    Fit(services,"VR3_ServiceRun",new Vector3(side*4.9f,.18f,z+1.5f),new Vector3(.68f,.21f,3.2f));
                var edges=Group(layer,"Damaged outside-wall industrial depth");
                for(var z=-34;z<=96;z+=8)foreach(var side in new[]{-1,1})
                {
                    Fit(edges,"VR3_topcablesstraight",new Vector3(side*37f,-.55f,z),new Vector3(.4f,1.35f,7.8f));
                    if((z+34)%16==0)Fit(edges,"VR3_generatorpilelarge",new Vector3(side*39.3f,-.35f,z+2),new Vector3(2.5f,2.7f,3.2f),side*12);
                }
                var thresholds=Group(layer,"Industrial threshold cladding");
                // Match existing posts; do not add visible closed doors to open lanes.
                foreach(var z in new[]{-24,0,24,80})foreach(var side in new[]{-1,1})
                    Fit(thresholds,"VR3_propventbig",new Vector3(side*(z==80?4.7f:5.2f),1.4f,z),new Vector3(.6f,.15f,1.7f),side*90);
            }
            if (AuthorityFingerprint(env) != before) throw new InvalidOperationException("Visual authoring changed collision authority.");
            env.ValidateOrThrow();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification");
            File.WriteAllText("docs/history/implementation-passes/chapter01-visual-replacement-v3/v44/verification/collision_fingerprint.txt",before);
            Debug.Log(full?"VISUAL_REPLACEMENT_V3_ENVIRONMENT_PASS":"VISUAL_REPLACEMENT_V3_REPAIR_PASS");
        }
        private static void ReplaceProxyArt(ChapterVisualEnvironment env,Transform parent,bool full)
        {
            var nodes=env.Floor.GetComponentsInChildren<Transform>(true).Where(t=>!t.IsChildOf(parent.parent)).ToArray();
            foreach(var node in nodes)
            {
                if(!full && (node.position.z < -34 || node.position.z > -21 || Mathf.Abs(node.position.x)>10))continue;
                var name=node.name;
                string donor=null;
                if(name=="Maintenance_Station")donor="VR3_command";
                else if(name=="Structural_Support")donor="VR3_columnpipes";
                else if(name=="Capacitor_Bank")donor="VR3_generatorpilelarge";
                else if(name=="Shield_Stack")donor="VR3_propcrate";
                else if(name=="Freight_Container")donor="VR3_propcrate";
                else if(full && name=="Coolant_Pump")donor="VR3_centrifuge";
                else if(full && name=="Transformer")donor="VR3_generator";
                else if(full && name=="Containment_Buttress")donor="VR3_cryotube";
                else if(full && name=="Bulkhead_Module")donor="VR3_shortwallmetalplatesstraight";
                else if(full && name=="Service_Arch")donor="VR3_doorframesquare";
                if(donor==null || !node.gameObject.activeInHierarchy)continue;
                var renderers=node.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0)continue;
                var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                // Exact occupied world AABB fit; this remains presentation only.
                foreach(var r in renderers)r.enabled=false;
                Fit(parent,donor,new Vector3(bounds.center.x,bounds.min.y,bounds.center.z),bounds.size);
            }
        }
        public static string AuthorityFingerprint(ChapterVisualEnvironment env)
        {
            return string.Join("\n",Enumerable.Range(0,env.SliceObstacleCount).Select(i=>{
                var o=env.GetSliceObstacle(i);return o.Name+"|"+o.Center.ToString("R")+"|"+o.Size.ToString("R");}));
        }
        private static Transform Group(Transform parent,string name)
        {var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
        public static GameObject Fit(Transform parent,string name,Vector3 position,Vector3 size,float yaw=0)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));if(prefab==null)throw new InvalidOperationException("Missing V3 module "+name);
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);obj.transform.position=Vector3.zero;
            var rs=obj.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            // Keep cladding inside the proxy footprint without extreme donor stretch.
            obj.transform.localScale=new Vector3(Mathf.Min(8,size.x/Mathf.Max(.001f,b.size.x)),b.size.y<.001f?1:Mathf.Min(8,size.y/b.size.y),Mathf.Min(8,size.z/Mathf.Max(.001f,b.size.z)));
            var scale=obj.transform.localScale;var offset=Vector3.Scale(new Vector3(b.center.x,b.min.y,b.center.z),scale);
            var rotation=Quaternion.Euler(0,yaw,0);obj.transform.SetPositionAndRotation(position-rotation*offset,rotation);
            foreach(var node in obj.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(node.gameObject,StaticEditorFlags.BatchingStatic);
            return obj;
        }
        private static void PrepareStatic()
        {
            Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");AssetDatabase.Refresh();UrpConfigurator.ConfigureUrp();
            var worn=Material("VR3_WornIndustrialAtlas","VR3_",true);
            var trim=Material("VR3_IndustrialTrimAtlas","VR3_Trim_",false);
            foreach(var path in Directory.GetFiles(Root+"/Models","*.fbx"))
            {
                var name=Path.GetFileNameWithoutExtension(path);
                if(name.StartsWith("M0_")||name.EndsWith("_Actor"))continue;
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.materialImportMode=ModelImporterMaterialImportMode.None;
                importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.animationType=ModelImporterAnimationType.None;
                importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.isReadable=false;importer.SaveAndReimport();
                var obj=new GameObject(name);Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),obj.transform,false);
                var isTrim=name.Contains("platform")||name.Contains("shortwall")||name.Contains("door")||name.Contains("topcables")||name.Contains("columnpipes")||name.Contains("proppipeholder")||name.Contains("propvent");
                foreach(var renderer in obj.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=new[]{isTrim?trim:worn};
                PrefabUtility.SaveAsPrefabAsset(obj,Prefab(name));Object.DestroyImmediate(obj);
            }
            AssetDatabase.SaveAssets();
        }
        private static Material Material(string name,string prefix,bool emission)
        {
            var path=Root+"/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            foreach(var label in new[]{"BaseColor","MetallicSmoothness"}.Concat(emission?new[]{"Emission"}:new[]{"Normal"}))
            {
                var tp=Root+"/Textures/"+prefix+label+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(tp);
                importer.textureType=label=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=label=="BaseColor";
                importer.maxTextureSize=emission?1024:2048;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
                material.SetTexture(label=="BaseColor"?"_BaseMap":label=="Emission"?"_EmissionMap":label=="Normal"?"_BumpMap":"_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tp));
            }
            material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",.65f);material.SetFloat("_Metallic",.7f);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");material.EnableKeyword(emission?"_EMISSION":"_NORMALMAP");
            if(emission){material.SetColor("_EmissionColor",Color.white*1.3f);material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;}
            material.enableInstancing=true;EditorUtility.SetDirty(material);return material;
        }
    }
}
