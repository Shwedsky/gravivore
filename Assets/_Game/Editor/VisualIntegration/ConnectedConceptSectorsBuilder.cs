using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    public static class ConnectedConceptSectorsBuilder
    {
        public const string Root="Assets/_Game/Content/ConnectedConceptSectors";
        public const string Output="docs/history/implementation-passes/connected-concept-sectors/verification";
        public const string TransferName="Industrial energy to fabrication transfer";
        public static string Model(string name)=>Root+"/Models/"+name+".fbx";
        public static string Prefab(string name)=>Root+"/Prefabs/"+name+".prefab";
        public static bool IsInstalled=>AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("CuttingFloor"))!=null;
        public static void Build()
        {
            Directory.CreateDirectory(Output);Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");
            AssetDatabase.Refresh();BuildMaterials();BuildPrefab("CuttingFloor");BuildPrefab("IndustrialTransfer");
            var scene=EditorSceneManager.OpenScene(Chapter01BlueprintWorldBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            var sector=root.VisualEnvironment.GetRegion("cutting-floor").Root;
            foreach(var node in sector.Cast<Transform>().Where(t=>t.name=="CuttingFloor / authored industrial sector").ToArray())Object.DestroyImmediate(node.gameObject);
            var installation=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("CuttingFloor")),sector);installation.name="CuttingFloor / authored industrial sector";
            var floor=root.VisualEnvironment.Floor;
            var old=floor.Find(TransferName);if(old!=null)Object.DestroyImmediate(old.gameObject);
            var transfer=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("IndustrialTransfer")),floor);
            transfer.name=TransferName;transfer.transform.localPosition=new Vector3(0,0,10);
            foreach(var node in installation.GetComponentsInChildren<Transform>().Concat(transfer.GetComponentsInChildren<Transform>()))GameObjectUtility.SetStaticEditorFlags(node.gameObject,StaticEditorFlags.BatchingStatic);
            ReplaceOldConnectorSurface(floor.Find("Authored cross-sector service network"));
            RebuildFootprints();
            BuildReflectionEnvironment();
            foreach(var light in root.VisualEnvironment.Lighting.GetComponentsInChildren<Light>())
                if(light.name.StartsWith("cutting-floor /",StringComparison.Ordinal))
                {
                    var work=light.name.Contains("machinery");light.color=work?new Color(1,.28f,.065f):new Color(.60f,.68f,.82f);
                    light.intensity=work?25:15;light.range=9;
                    light.transform.position=sector.position+(work?new Vector3(-2.5f,3.1f,4.5f):new Vector3(-5,3.2f,-2));
                }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            ValidateOrThrow();Chapter01BlueprintWorldBuilder.Audit();Debug.Log("CONNECTED_CONCEPT_SECTORS_AUTHORED");
        }
        private static void BuildMaterials()
        {
            foreach(var path in Directory.GetFiles(Root+"/Textures","*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=path.EndsWith("_Normal.png")?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=path.EndsWith("_BaseColor.png");importer.maxTextureSize=512;importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Repeat;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=512;android.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            }
            foreach(var name in new[]{"HeatSteel","PressPaint","FabricationDeck"})
            {
                var path=Root+"/Materials/CS_"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
                foreach(var map in new[]{("_BaseMap","BaseColor"),("_BumpMap","Normal"),("_MetallicGlossMap","MetallicSmoothness"),("_OcclusionMap","Occlusion")})
                    mat.SetTexture(map.Item1,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/CS_"+name+"_"+map.Item2+".png"));
                mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_BumpScale",.45f);mat.SetFloat("_Smoothness",.65f);mat.SetFloat("_OcclusionStrength",1);
                foreach(var keyword in new[]{"_NORMALMAP","_METALLICSPECGLOSSMAP","_OCCLUSIONMAP"})mat.EnableKeyword(keyword);
                mat.enableInstancing=true;EditorUtility.SetDirty(mat);
            }
        }
        private static void BuildPrefab(string name)
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Model(name));importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.animationType=ModelImporterAnimationType.None;importer.bakeAxisConversion=true;importer.addCollider=false;importer.isReadable=false;importer.SaveAndReimport();
            var owner=new GameObject(name+" / original authored installation");Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model(name)),owner.transform,false);
            foreach(var renderer in owner.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>AssetDatabase.LoadAssetAtPath<Material>((m.name.StartsWith("CF_")?CapacitorFieldProofBuilder.Root:Root)+"/Materials/"+m.name+".mat")??throw new InvalidOperationException("Missing shared physical surface "+m.name)).ToArray();
            PrefabUtility.SaveAsPrefabAsset(owner,Prefab(name));Object.DestroyImmediate(owner);
        }
        private static void BuildReflectionEnvironment()
        {
            // Original neutral industrial lighting environment. This is a small
            // precomputed cubemap, not an expensive realtime probe or a claimed bake.
            const int size=64;var path=Root+"/Textures/CS_IndustrialReflection.asset";
            var cube=AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if(cube==null){cube=new Cubemap(size,TextureFormat.RGBAHalf,true);cube.name="CS_IndustrialReflection";AssetDatabase.CreateAsset(cube,path);}
            var panel=new Vector3(-1,1,-.25f).normalized;
            for(var face=0;face<6;face++)
            {
                var colors=new Color[size*size];
                for(var y=0;y<size;y++)for(var x=0;x<size;x++)
                {
                    var u=(x+.5f)/size*2-1;var v=(y+.5f)/size*2-1;
                    var d=face==0?new Vector3(1,-v,-u):face==1?new Vector3(-1,-v,u):face==2?new Vector3(u,1,v):face==3?new Vector3(u,-1,-v):face==4?new Vector3(u,-v,1):new Vector3(-u,-v,-1);d.Normalize();
                    var horizon=new Color(.24f,.27f,.31f);var color=d.y>=0?Color.Lerp(horizon,new Color(.68f,.74f,.78f),d.y):Color.Lerp(horizon,new Color(.055f,.063f,.073f),-d.y);
                    var key=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,panel)),42)*1.15f;color+=new Color(.95f,.86f,.75f)*key;color.a=1;colors[y*size+x]=color;
                }
                cube.SetPixels(colors,(CubemapFace)face);
            }
            cube.Apply(true,false);cube.filterMode=FilterMode.Trilinear;EditorUtility.SetDirty(cube);
            RenderSettings.defaultReflectionMode=UnityEngine.Rendering.DefaultReflectionMode.Custom;RenderSettings.customReflectionTexture=cube;RenderSettings.reflectionIntensity=.65f;
        }
        private static void ReplaceOldConnectorSurface(Transform services)
        {
            if(services==null)throw new InvalidOperationException("Production service network missing.");
            if(PrefabUtility.IsPartOfPrefabInstance(services.gameObject))PrefabUtility.UnpackPrefabInstance(services.gameObject,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            var original=Chapter01BlueprintWorldBuilder.Root+"/Models/R1_ServiceNetwork.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(original);var readable=importer.isReadable;
            int removed=0;
            try
            {
                if(!readable){importer.isReadable=true;importer.SaveAndReimport();}
                foreach(var filter in services.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.Contains("_floor_")).ToArray())
                {
                    var mesh=Object.Instantiate(filter.sharedMesh);var vertices=mesh.vertices;var changed=0;var remaining=0;
                    bool Inside(int index){var v=filter.transform.TransformPoint(vertices[index]);return v.x>=-5.1f&&v.x<=5.1f&&v.z>=5.98f&&v.z<=14.02f;}
                    for(var sub=0;sub<mesh.subMeshCount;sub++)
                    {
                        var prior=mesh.GetTriangles(sub);var keep=new List<int>(prior.Length);
                        for(var i=0;i<prior.Length;i+=3)
                        {
                            if(Inside(prior[i])&&Inside(prior[i+1])&&Inside(prior[i+2])){changed++;continue;}
                            keep.Add(prior[i]);keep.Add(prior[i+1]);keep.Add(prior[i+2]);
                        }
                        mesh.SetTriangles(keep,sub);remaining+=keep.Count;
                    }
                    removed+=changed;
                    if(changed==0){Object.DestroyImmediate(mesh);continue;}
                    if(remaining==0){Object.DestroyImmediate(mesh);Object.DestroyImmediate(filter.gameObject);continue;}
                    mesh.RecalculateBounds();mesh.name="Retained service context "+filter.name;
                    var path=Root+"/Models/Retained_"+filter.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(existing==null){AssetDatabase.CreateAsset(mesh,path);existing=mesh;}else{EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(existing);}
                    filter.sharedMesh=existing;
                }
            }
            finally {if(importer.isReadable!=readable){importer.isReadable=readable;importer.SaveAndReimport();}}
            if(removed>0||!File.Exists(Output+"/connector-replacement.json"))
                File.WriteAllText(Output+"/connector-replacement.json","{\"removedOldTriangles\":"+removed+",\"region\":\"x[-5.1,5.1], z[5.98,14.02]\",\"sourceFbxPreserved\":true}");
        }
        private static void RebuildFootprints()
        {
            var layout=AssetDatabase.LoadAssetAtPath<ChapterWorldLayoutDefinition>(Chapter01BlueprintWorldBuilder.Root+"/Chapter01WorldLayout.asset");var serialized=new SerializedObject(layout);var blockers=serialized.FindProperty("_blockers");
            void Set(string id,Vector3 center,Vector3 size)
            {
                var index=Enumerable.Range(0,blockers.arraySize).Where(i=>blockers.GetArrayElementAtIndex(i).FindPropertyRelative("_id").stringValue==id).DefaultIfEmpty(-1).First();
                if(index<0){index=blockers.arraySize;blockers.arraySize++;}var entry=blockers.GetArrayElementAtIndex(index);entry.FindPropertyRelative("_id").stringValue=id;entry.FindPropertyRelative("_center").vector3Value=center;entry.FindPropertyRelative("_size").vector3Value=size;
            }
            Set("cutting-east-press-housing",new Vector3(22.3f,2.4f,9.5f),new Vector3(3.8f,4.8f,14.2f));
            Set("cutting-rear-furnace-line",new Vector3(15,3.4f,17.1f),new Vector3(12.6f,6.8f,6.2f));
            Set("cutting-west-hydraulic-pack",new Vector3(8.15f,1.7f,6),new Vector3(2.6f,3.4f,3.3f));
            Set("cutting-west-service-bank",new Vector3(8.15f,1.9f,13),new Vector3(2.5f,3.8f,3));
            foreach(var x in new[]{-7.15f,7.15f})foreach(var z in new[]{6.6f,13.4f})
            {var height=z<10?1.65f:4.6f;Set("transfer-portal/"+x.ToString(System.Globalization.CultureInfo.InvariantCulture)+"/"+z.ToString(System.Globalization.CultureInfo.InvariantCulture),new Vector3(x,height/2,z),new Vector3(1.35f,height,1.4f));}
            Set("transfer-isolation-station",new Vector3(-3.2f,1.85f,13),new Vector3(1.85f,3.7f,1.75f));
            Set("transfer-pressure-station",new Vector3(3.2f,1.85f,7),new Vector3(1.85f,3.7f,1.75f));
            serialized.ApplyModifiedPropertiesWithoutUndo();layout.ValidateOrThrow();EditorUtility.SetDirty(layout);
        }
        public static void ValidateOrThrow()
        {
            CapacitorFieldProofBuilder.ValidateOrThrow();
            foreach(var name in new[]{"CuttingFloor","IndustrialTransfer"})
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));
                if(prefab==null||prefab.GetComponentsInChildren<Collider>(true).Length>0||prefab.GetComponentsInChildren<MonoBehaviour>(true).Length>0)throw new InvalidOperationException("CS_ENV separate presentation authority required.");
                foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true))Rendering.RenderingBuildAudit.ValidateRenderer(renderer);
                if(prefab.transform.localScale!=Vector3.one)throw new InvalidOperationException("CS_ENV nonuniform fit forbidden.");
            }
            var world=AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.World.Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            foreach(var x in new[]{-15f,-8,-4,0,4,8,15})if(!world.Layout.IsClear(new Vector3(x,0,10),.65f))throw new InvalidOperationException("CS_ROUTE transfer blocked at "+x);
            if(!world.Layout.IsClear(new Vector3(15,0,12.8f),.65f))throw new InvalidOperationException("CS_ROUTE working centre approach blocked.");
            Directory.CreateDirectory(Output);File.WriteAllText(Output+"/shared-contract.json","{\"sharedCamera\":true,\"linear\":true,\"aces\":true,\"bloom\":0.12,\"actorV2Preserved\":true,\"presentationColliders\":0}");
        }
    }
}
