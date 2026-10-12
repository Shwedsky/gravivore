using System;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Offline art installation. Gameplay identities remain in their existing definitions.</summary>
    public static class Chapter01CompleteConceptBuilder
    {
        public const string Root="Assets/_Game/Content/Chapter01CompleteConcept";
        public const string Output="docs/history/implementation-passes/chapter01-complete-concept/verification";
        public const string NetworkName="Chapter 01 connected industrial service spine";
        public static readonly string[] Models={"RepairHub","RelayYard","ShieldDump","HaulerGraveyard","MagnetarComplex","CustodianCore","ChapterServiceNetwork"};
        public static readonly string[] Sectors={"repair-hub","relay-yard","shield-dump","hauler-graveyard","elite-arena","boss-arena"};
        public static bool IsInstalled=>AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("CustodianCore"))!=null;
        public static string Model(string name)=>Root+"/Models/"+name+".fbx";
        public static string Prefab(string name)=>Root+"/Prefabs/"+name+".prefab";
        public static void Build()
        {
            Directory.CreateDirectory(Output);Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");
            AssetDatabase.Refresh();BuildMaterials();foreach(var model in Models)BuildPrefab(model);WriteGeometryAudit();
            var scene=EditorSceneManager.OpenScene(Chapter01BlueprintWorldBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            for(var i=0;i<Sectors.Length;i++)
            {
                var sector=root.VisualEnvironment.GetRegion(Sectors[i]).Root;
                var name=Models[i]+" / authored industrial sector";
                foreach(var old in sector.Cast<Transform>().Where(t=>t.name==name).ToArray())Object.DestroyImmediate(old.gameObject);
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(Models[i])),sector);
                instance.name=name;StaticPresentation(instance);
            }
            // All connections receive purpose-specific decks, utility trenches and
            // pressure-lock architecture. The approved electrical transfer is retained.
            var floor=root.VisualEnvironment.Floor;
            foreach(var name in new[]{"Authored cross-sector service network",NetworkName})
            {var old=floor.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);}
            var network=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab("ChapterServiceNetwork")),floor);
            network.name=NetworkName;StaticPresentation(network);
            RebuildFootprints();ConfigureLights(root);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            ValidateOrThrow();Chapter01BlueprintWorldBuilder.Audit();Debug.Log("CHAPTER01_COMPLETE_CONCEPT_AUTHORED");
        }
        private static void StaticPresentation(GameObject instance)
        {foreach(var node in instance.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(node.gameObject,StaticEditorFlags.BatchingStatic);}
        private static void WriteGeometryAudit()
        {
            var entries=Models.Select(name=>
            {
                var renderers=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name)).GetComponentsInChildren<MeshRenderer>();
                long triangles=0;foreach(var renderer in renderers){var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;for(var i=0;i<mesh.subMeshCount;i++)triangles+=(long)mesh.GetIndexCount(i)/3;}
                return new GeometryEntry{model=name,sourceRenderers=renderers.Length,sourceTriangles=triangles};
            }).ToArray();
            File.WriteAllText(Output+"/geometry-imported.json",JsonUtility.ToJson(new GeometryReport{models=entries},true));
        }
        [Serializable] private sealed class GeometryEntry{public string model;public int sourceRenderers;public long sourceTriangles;}
        [Serializable] private sealed class GeometryReport{public GeometryEntry[] models;}
        private static void BuildMaterials()
        {
            foreach(var path in Directory.GetFiles(Root+"/Textures","*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=path.EndsWith("_Normal.png")?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=path.EndsWith("_BaseColor.png");importer.maxTextureSize=512;importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Repeat;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=512;android.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            }
            foreach(var name in new[]{"ServicePaint","ArmorShell","Scorched"})
            {
                var path=Root+"/Materials/CH_"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
                foreach(var map in new[]{("_BaseMap","BaseColor"),("_BumpMap","Normal"),("_MetallicGlossMap","MetallicSmoothness"),("_OcclusionMap","Occlusion")})
                    mat.SetTexture(map.Item1,AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/CH_"+name+"_"+map.Item2+".png"));
                mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_BumpScale",.45f);mat.SetFloat("_Smoothness",.65f);mat.SetFloat("_OcclusionStrength",1);
                foreach(var key in new[]{"_NORMALMAP","_METALLICSPECGLOSSMAP","_OCCLUSIONMAP"})mat.EnableKeyword(key);
                mat.enableInstancing=true;EditorUtility.SetDirty(mat);
            }
        }
        private static void BuildPrefab(string name)
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Model(name));
            importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;importer.animationType=ModelImporterAnimationType.None;
            importer.bakeAxisConversion=true;importer.addCollider=false;importer.isReadable=false;importer.SaveAndReimport();
            var owner=new GameObject(name+" / original chapter installation");Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model(name)),owner.transform,false);
            foreach(var renderer in owner.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>
                {
                    var family=m.name.StartsWith("CF_")?CapacitorFieldProofBuilder.Root:m.name.StartsWith("CS_")?ConnectedConceptSectorsBuilder.Root:Root;
                    return AssetDatabase.LoadAssetAtPath<Material>(family+"/Materials/"+m.name+".mat")??throw new InvalidOperationException("Missing shared PBR surface "+m.name);
                }).ToArray();
            PrefabUtility.SaveAsPrefabAsset(owner,Prefab(name));Object.DestroyImmediate(owner);
        }
        private static void ConfigureLights(S01SceneCompositionRoot root)
        {
            for(var i=0;i<Sectors.Length;i++)
            {
                var sector=root.VisualEnvironment.GetRegion(Sectors[i]).Root;
                foreach(var light in root.VisualEnvironment.Lighting.GetComponentsInChildren<Light>().Where(l=>l.name.StartsWith(Sectors[i]+" /",StringComparison.Ordinal)))
                {
                    var work=light.name.Contains("machinery");var cold=i==0;
                    light.color=cold?new Color(.22f,.72f,1):work?new Color(1,.31f,.085f):new Color(.55f,.69f,.82f);
                    light.intensity=work?24:14;light.range=9;light.shadows=LightShadows.None;
                    var offset=i==4?new Vector3(-5,3.5f,4):i==5?new Vector3(0,4,6):new Vector3(0,3.1f,3.5f);
                    light.transform.position=sector.position+(work?offset:new Vector3(-4,3.2f,-2));
                }
            }
        }
        private static void RebuildFootprints()
        {
            var layout=AssetDatabase.LoadAssetAtPath<ChapterWorldLayoutDefinition>(Chapter01BlueprintWorldBuilder.Root+"/Chapter01WorldLayout.asset");
            var data=new SerializedObject(layout);var blockers=data.FindProperty("_blockers");
            var retired=new[]{"hub-rear-diagnostic-bank","west-row1-service-island","east-row1-service-island","custodian-west-containment-wall","custodian-east-containment-wall","custodian-north-reactor-wall"};
            for(var i=blockers.arraySize-1;i>=0;i--)if(retired.Contains(blockers.GetArrayElementAtIndex(i).FindPropertyRelative("_id").stringValue))blockers.DeleteArrayElementAtIndex(i);
            void Set(string id,Vector3 center,Vector3 size)
            {
                var index=Enumerable.Range(0,blockers.arraySize).Where(i=>blockers.GetArrayElementAtIndex(i).FindPropertyRelative("_id").stringValue==id).DefaultIfEmpty(-1).First();
                if(index<0){index=blockers.arraySize;blockers.arraySize++;}
                var p=blockers.GetArrayElementAtIndex(index);p.FindPropertyRelative("_id").stringValue=id;p.FindPropertyRelative("_center").vector3Value=center;p.FindPropertyRelative("_size").vector3Value=size;
            }
            Set("hub-west-maintenance-wall",new Vector3(-7.5f,2.4f,-30),new Vector3(3.9f,4.8f,15));
            Set("hub-east-maintenance-wall",new Vector3(7.5f,2.4f,-30),new Vector3(3.9f,4.8f,15));
            foreach(var s in new[]{-1,1})
            {
                Set("hub-diagnostic-"+s,new Vector3(s*4.8f,2.2f,-24),new Vector3(2.4f,4.4f,1.6f));
                Set("hub-manipulator-"+s,new Vector3(s*4.75f,1,-29),new Vector3(1.8f,2,1.7f));
                Set("hub-exit-jamb-"+s,new Vector3(s*3.5f,2.2f,-22),new Vector3(1.3f,4.4f,1.5f));
            }
            Set("relay-west-deployment-line",new Vector3(-7.1f,1.6f,-11),new Vector3(3.1f,3.2f,12));
            Set("relay-east-command-bay",new Vector3(8,2.4f,-8.5f),new Vector3(3.5f,4.8f,8));
            foreach(var s in new[]{-1,1})Set("relay-north-assembly-jamb-"+s,new Vector3(s*4.8f,2.1f,-4.1f),new Vector3(1.7f,4.2f,2.9f));
            Set("shield-west-armor-cells",new Vector3(-22.3f,2.1f,32.4f),new Vector3(3.5f,4.2f,15.3f));
            Set("shield-rear-shield-press",new Vector3(-15,2.9f,39),new Vector3(11.5f,5.8f,5.3f));
            Set("shield-east-sorting-service",new Vector3(-7.8f,2,41),new Vector3(2.2f,4,1.8f));
            Set("shield-east-reject-bin",new Vector3(-7.7f,1.2f,26.3f),new Vector3(2.9f,2.4f,3));
            Set("shield-southwest-reject-bin",new Vector3(-19,1.2f,26),new Vector3(2.9f,2.4f,3));
            Set("shield-east-sort-control",new Vector3(-9.2f,1.9f,24.2f),new Vector3(2.2f,3.8f,1.8f));
            Set("hauler-east-loading-gantry",new Vector3(22.4f,3.2f,33.1f),new Vector3(4,6.4f,14.5f));
            Set("hauler-rear-cargo-bank",new Vector3(16,1.5f,39.1f),new Vector3(6.5f,3,4));
            Set("hauler-west-ruptured-hull",new Vector3(8.1f,1.5f,35.8f),new Vector3(4,3,5.8f));
            Set("hauler-southwest-scrap",new Vector3(8.5f,.5f,27.2f),new Vector3(3.6f,1,3.5f));
            Set("hauler-foreground-cargo-plate",new Vector3(21,.5f,25),new Vector3(2.4f,1,3));
            Set("magnetar-reactor-footprint",new Vector3(-5,4,59.9f),new Vector3(8.9f,8,8.9f));
            Set("magnetar-west-power-conversion",new Vector3(-14,2.5f,60.5f),new Vector3(3,5,1.8f));
            Set("magnetar-east-power-conversion",new Vector3(8,2.5f,60.5f),new Vector3(3,5,1.8f));
            Set("magnetar-west-outer-support",new Vector3(-22,3,54),new Vector3(3.8f,6,18));
            Set("magnetar-east-outer-support",new Vector3(22,3,54),new Vector3(3.8f,6,18));
            Set("magnetar-deployment-cells",new Vector3(15.5f,2,62),new Vector3(12.5f,4,2.8f));
            Set("custodian-reactor-footprint",new Vector3(0,5,84.6f),new Vector3(10.1f,10,8.8f));
            foreach(var s in new[]{-1,1})
            {
                foreach(var pair in new[]{(-8f,2.2f),(-1.5f,4.4f),(5f,6.4f),(10f,7.7f)})
                    Set("custodian-containment-pier/"+s+"/"+pair.Item1.ToString(System.Globalization.CultureInfo.InvariantCulture),new Vector3(s*(pair.Item1==5?14:pair.Item1<7?11:13),pair.Item2/2,76+pair.Item1),new Vector3(4.1f,pair.Item2,3.4f));
                Set("custodian-service-console-"+s,new Vector3(s*7.7f,2.3f,83.4f),new Vector3(2.4f,4.6f,1.8f));
                foreach(var z in new[]{72f,78,84})Set("custodian-wing-service/"+s+"/"+z,new Vector3(s*22,2,z+1.5f),new Vector3(3,4,1.8f));
                foreach(var z in new[]{-.2f,20,39.5f,65})Set("spine-utility/"+s+"/"+z.ToString(System.Globalization.CultureInfo.InvariantCulture),new Vector3(s*3.35f,2,z),new Vector3(.8f,4,1.2f));
                foreach(var z in new[]{44f,64})Set("gate-architecture/"+s+"/"+z,new Vector3(s*5.5f,3.3f,z),new Vector3(3.3f,6.6f,2.9f));
            }
            var route=data.FindProperty("_route");
            var points=Enumerable.Range(0,19).Select(i=>route.GetArrayElementAtIndex(i).vector3Value).Concat(new[]{
                new Vector3(2,0,54),new Vector3(2,0,63),new Vector3(0,0,67),
                new Vector3(0,0,71.3f),new Vector3(-18,0,71.3f),new Vector3(-18,0,75),new Vector3(-18,0,71.3f),
                new Vector3(0,0,71.3f),new Vector3(18,0,71.3f),new Vector3(18,0,74),new Vector3(18,0,71.3f),new Vector3(0,0,71.3f),new Vector3(0,0,76)}).ToArray();
            route.arraySize=points.Length;for(var i=0;i<points.Length;i++)route.GetArrayElementAtIndex(i).vector3Value=points[i];
            // The same proxies feed production movement and minimap topology.
            data.ApplyModifiedPropertiesWithoutUndo();layout.ValidateOrThrow();EditorUtility.SetDirty(layout);
        }
        public static void ValidateOrThrow()
        {
            ConnectedConceptSectorsBuilder.ValidateOrThrow();
            foreach(var name in Models)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(name));
                if(prefab==null||prefab.GetComponentsInChildren<Collider>(true).Length!=0||prefab.GetComponentsInChildren<MonoBehaviour>(true).Length!=0||prefab.transform.localScale!=Vector3.one)
                    throw new InvalidOperationException("CHAPTER_ART separate presentation authority and metre scale required: "+name);
                foreach(var r in prefab.GetComponentsInChildren<Renderer>(true))Rendering.RenderingBuildAudit.ValidateRenderer(r);
            }
            var layout=AssetDatabase.LoadAssetAtPath<ChapterWorldLayoutDefinition>(Chapter01BlueprintWorldBuilder.Root+"/Chapter01WorldLayout.asset");
            for(var i=1;i<layout.RoutePointCount;i++)
            {
                var a=layout.GetRoutePoint(i-1);var b=layout.GetRoutePoint(i);var samples=Mathf.CeilToInt(Vector3.Distance(a,b)*3);
                for(var j=0;j<=samples;j++)if(!layout.IsClear(Vector3.Lerp(a,b,(float)j/Mathf.Max(1,samples)),.65f))
                    throw new InvalidOperationException("CHAPTER_ROUTE blocked segment "+i+" sample "+j);
            }
            Directory.CreateDirectory(Output);File.WriteAllText(Output+"/chapter-contract.json","{\"sharedCamera\":true,\"presentationColliders\":0,\"routeSegments\":"+(layout.RoutePointCount-1)+",\"blockers\":"+layout.BlockerCount+",\"ordinaryIdsAndProgressionPreserved\":true}");
        }
    }
}
