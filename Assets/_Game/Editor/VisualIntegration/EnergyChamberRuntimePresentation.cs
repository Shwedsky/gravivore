using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public static class EnergyChamberRuntimePresentation
    {
        public const string Evidence = "docs/history/implementation-passes/energy-chamber-runtime-presentation-v1";
        public const string Root = Chapter01ArchitectureProduction.Root;
        public const string Prefab = Chapter01ArchitectureProduction.PrefabPath;
        public static void Inspect()
        {
            var scene = EditorSceneManager.OpenScene(Chapter01ArchitectureProduction.ScenePath);
            var installation = scene.GetRootGameObjects().Single(g => g.name == "Energy Chamber / production");
            var group = installation.GetComponent<LODGroup>();
            var lods = group.GetLODs();
            Write("renderer-inventory.json", new Inventory {
                size=group.size, referencePoint=group.localReferencePoint, nestedLodGroups=installation.GetComponentsInChildren<LODGroup>(true).Length-1,
                items=installation.GetComponentsInChildren<Renderer>(true).Select(r => new Record {
                    name=PathOf(r.transform,installation.transform), mesh=AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>()?.sharedMesh),
                    bounds=r.bounds, localBounds=r.localBounds, enabled=r.enabled, active=r.gameObject.activeInHierarchy,
                    transformScale=r.transform.lossyScale, lods=Enumerable.Range(0,lods.Length).Where(i=>lods[i].renderers.Contains(r)).ToArray(),
                    triangles=Chapter01ArchitectureProduction.Triangles(new[]{r}),
                    materials=r.sharedMaterials.Select(m=>new MaterialRecord {name=m.name,shader=m.shader.name,queue=m.renderQueue,opaque=!m.HasProperty("_Surface")||m.GetFloat("_Surface")==0}).ToArray()
                }).ToArray(),
                levels=lods.Select((l,i)=>new Level {level=i,threshold=l.screenRelativeTransitionHeight,renderers=l.renderers.Length,
                    triangles=Chapter01ArchitectureProduction.Triangles(l.renderers),slots=l.renderers.Sum(r=>r.sharedMaterials.Length),materials=l.renderers.SelectMany(r=>r.sharedMaterials).Distinct().Count()}).ToArray()
            });
        }
        public static void FixVisibility()
        {
            var contents=PrefabUtility.LoadPrefabContents(Prefab);
            try {
                var all=contents.GetComponentsInChildren<Renderer>(true);
                string MeshName(Renderer r)=>Path.GetFileName(AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>().sharedMesh));
                var near=all.Where(r=>MeshName(r)!="EC_FoundationPolish.fbx"&&MeshName(r)!="EC_ConnectorPolish.fbx").ToArray();
                var silhouette=new[]{"EC_Foundation.fbx","EC_CentralChamber.fbx","EC_CoreHeart.fbx","EC_SideNode.fbx","EC_VerticalDistribution.fbx","EC_Connector.fbx",
                    "EC_ChamberPolish.fbx","EC_NodePolish.fbx","Prop_Rail_Round_Big.fbx","Platform_Stairs_4.fbx"};
                var far=all.Where(r=>silhouette.Contains(MeshName(r))).ToArray();
                var group=contents.GetComponent<LODGroup>();
                group.fadeMode=LODFadeMode.None;
                group.SetLODs(new[]{new LOD(.26f,all),new LOD(.13f,near),new LOD(.035f,far)});
                // Bounds already match the accepted meshes; do not inflate them to hide culling.
                if(all.Length!=45||Chapter01ArchitectureProduction.Triangles(all)!=279694)
                    throw new InvalidDataException("Frozen near architecture changed.");
                PrefabUtility.SaveAsPrefabAsset(contents,Prefab);
            }
            finally {PrefabUtility.UnloadPrefabContents(contents);}
            AssetDatabase.SaveAssets();Inspect();
            Debug.Log("ENERGY_CHAMBER_VISIBILITY_FIXED");
        }
        public static void SurveySurfaces()
        {
            var contents=PrefabUtility.LoadPrefabContents(Prefab);
            try {
                var records=new System.Collections.Generic.List<SurfaceSamples>();
                foreach(var r in contents.GetComponentsInChildren<MeshRenderer>(true)) {
                    var mesh=r.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;
                    for(var sub=0;sub<mesh.subMeshCount;sub++) {
                        var indices=mesh.GetTriangles(sub);var points=new System.Collections.Generic.List<Vector3>();
                        var stride=Math.Max(3,indices.Length/1800/3*3);
                        for(var t=0;t<indices.Length;t+=stride) {
                            var center=(vertices[indices[t]]+vertices[indices[t+1]]+vertices[indices[t+2]])/3;
                            points.Add(contents.transform.InverseTransformPoint(r.transform.TransformPoint(center)));
                        }
                        records.Add(new SurfaceSamples {name=PathOf(r.transform,contents.transform),material=r.sharedMaterials[sub].name,points=points.ToArray()});
                    }
                }
                Directory.CreateDirectory("Builds/Logs");File.WriteAllText("Builds/Logs/energy-surface-samples.json",JsonUtility.ToJson(new SurfaceSampleRecords {items=records.ToArray()},true));
            }
            finally {PrefabUtility.UnloadPrefabContents(contents);}
        }
        public static void AuthorLookDev()
        {
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/Shaders/EnergyChamberRuntimeLit.shader");
            if(shader==null)throw new InvalidDataException("Runtime PBR shader missing.");
            var textureRoot=Root+"/Textures/RuntimePresentationV1";
            foreach(var file in Directory.GetFiles(textureRoot,"*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                var normal=file.EndsWith("_Normal.png",StringComparison.Ordinal);
                importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=file.EndsWith("_BaseColor.png",StringComparison.Ordinal);
                importer.alphaIsTransparency=false;importer.mipmapEnabled=true;importer.isReadable=false;
                importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=2;
                importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Compressed;
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings {name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ASTC_6x6,compressionQuality=50});
                importer.SaveAndReimport();
            }
            var materials=Directory.GetFiles(Root+"/Materials","*.mat").Select(AssetDatabase.LoadAssetAtPath<Material>).ToArray();
            foreach(var material in materials)ConfigureMaterial(material,shader,Role(material.name));
            // Production-only copies of two shared trim materials keep every other
            // Chapter facility's material response frozen.
            var contents=PrefabUtility.LoadPrefabContents(Prefab);
            try {
                foreach(var renderer in contents.GetComponentsInChildren<Renderer>(true)) {
                    var slots=renderer.sharedMaterials;
                    for(var i=0;i<slots.Length;i++) {
                        if(AssetDatabase.GetAssetPath(slots[i]).StartsWith(Root,StringComparison.Ordinal))continue;
                        var path=Root+"/Materials/EC_Runtime_"+slots[i].name+".mat";
                        var copy=AssetDatabase.LoadAssetAtPath<Material>(path);
                        if(copy==null){copy=new Material(slots[i]);AssetDatabase.CreateAsset(copy,path);}
                        ConfigureMaterial(copy,shader,slots[i].name.Contains("Trim")?"Machined":"Deck");slots[i]=copy;
                    }
                    renderer.sharedMaterials=slots;
                }
                PrefabUtility.SaveAsPrefabAsset(contents,Prefab);
            }
            finally{PrefabUtility.UnloadPrefabContents(contents);}
            AssetDatabase.SaveAssets();Inspect();
            var all=AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}).Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            Write("material-receipts.json",new Materials {items=all.Select(m=>new SurfaceRecord {name=m.name,role=Role(m.name),shader=m.shader.name,
                smoothness=m.GetFloat("_Smoothness"),normalStrength=m.GetFloat("_BumpScale"),emission=m.GetColor("_EmissionColor"),
                energyRole=m.GetFloat("_EnergyRole"),spill=m.GetFloat("_SpillStrength"),practical=m.GetFloat("_PracticalStrength"),
                baseMap=AssetDatabase.GetAssetPath(m.GetTexture("_BaseMap")),metallicMap=AssetDatabase.GetAssetPath(m.GetTexture("_MetallicGlossMap")),
                normalMap=AssetDatabase.GetAssetPath(m.GetTexture("_BumpMap")),occlusionMap=AssetDatabase.GetAssetPath(m.GetTexture("_OcclusionMap"))}).ToArray()});
            if(ShaderUtil.ShaderHasError(shader))throw new InvalidDataException("Energy Chamber shader import error.");
            Debug.Log("ENERGY_CHAMBER_LOOKDEV_AUTHORED "+all.Length+" material assets");
        }
        private static string Role(string name)
        {
            switch(name) {
                case "EC_BaseIron":case "EC_Frame":return "Graphite";
                case "EC_Cold":case "EC_Steel":return "Machined";
                case "EC_ChamberCasing":case "EC_NodeCasing":case "EC_BaseCassette":case "EC_Shell":return "Casing";
                case "EC_PipeSteel":return "Pipe";
                case "EC_Gasket":return "Recess";
                case "EC_Copper":return "Copper";
                case "EC_Interface":return "Machined";
                case "EC_Deck":return "Deck";
                case "EC_Ceramic":return "ColdPanel";
                default:return "PreservedMask";
            }
        }
        private static void ConfigureMaterial(Material material,Shader shader,string role)
        {
            material.shader=shader;material.renderQueue=2000;material.enableInstancing=true;
            material.SetVector("_ChamberOrigin",new Vector4(11,.08f,50.65f,0));
            material.SetFloat("_Surface",0);material.SetFloat("_Cull",2);material.SetFloat("_EmissionScale",1);material.SetFloat("_PreviewTime",-1);
            material.SetFloat("_SpillStrength",role=="Recess"?.18f:1);
            material.SetFloat("_PracticalStrength",role=="Recess"?0:1);
            material.SetFloat("_OperationStrength",.12f);material.SetFloat("_EnergyRole",0);
            material.SetFloat("_SurfaceClass",material.name=="EC_ChamberCasing"?1:0);
            material.SetFloat("_OcclusionStrength",1);
            if(role!="PreservedMask") {
                var directory=Root+"/Textures/RuntimePresentationV1/EC_"+role;
                material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(directory+"_BaseColor.png"));
                material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(directory+"_MetallicSmoothness.png"));
                material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(directory+"_Normal.png"));
                material.SetTexture("_OcclusionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(directory+"_Occlusion.png"));
                material.SetColor("_BaseColor",Color.white);material.SetColor("_EmissionColor",Color.black);
                material.SetFloat("_Smoothness",1);material.SetFloat("_BumpScale",.8f);
            }
            if(material.name=="EC_CoreWarm") {material.SetColor("_EmissionColor",new Color(4.3f,.88f,.07f));material.SetFloat("_EnergyRole",1);}
            if(material.name=="EC_CoreInnerWarm") {material.SetColor("_EmissionColor",new Color(2.8f,.62f,.045f));material.SetFloat("_EnergyRole",1);}
            if(material.name=="EC_NodeCool") {material.SetColor("_EmissionColor",new Color(.035f,1.0f,1.9f));material.SetFloat("_EnergyRole",2);}
            if(material.GetTexture("_BumpMap")!=null)material.EnableKeyword("_NORMALMAP");
            if(material.GetTexture("_MetallicGlossMap")!=null)material.EnableKeyword("_METALLICSPECGLOSSMAP");
            if(material.GetColor("_EmissionColor").maxColorComponent>0)material.EnableKeyword("_EMISSION");else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
        }
        public static string PathOf(Transform t,Transform root) => t==root?t.name:PathOf(t.parent,root)+"/"+t.name;
        private static void Write(string name,object record) {
            var stage=Environment.GetEnvironmentVariable("GRAVIVORE_ENERGY_PRESENTATION_STAGE")??"diagnosis";
            Directory.CreateDirectory(Evidence+"/verification/"+stage);
            File.WriteAllText(Evidence+"/verification/"+stage+"/"+name,JsonUtility.ToJson(record,true));
        }
        [Serializable] private sealed class Inventory {public float size;public Vector3 referencePoint;public int nestedLodGroups;public Record[] items;public Level[] levels;}
        [Serializable] private sealed class Record {public string name,mesh;public Bounds bounds,localBounds;public bool enabled,active;public Vector3 transformScale;public int[] lods;public int triangles;public MaterialRecord[] materials;}
        [Serializable] private sealed class MaterialRecord {public string name,shader;public int queue;public bool opaque;}
        [Serializable] private sealed class Level {public int level,renderers,triangles,slots,materials;public float threshold;}
        [Serializable] private sealed class Materials {public SurfaceRecord[] items;}
        [Serializable] private sealed class SurfaceRecord {public string name,role,shader,baseMap,metallicMap,normalMap,occlusionMap;public float smoothness,normalStrength,energyRole,spill,practical;public Color emission;}
        [Serializable] private sealed class SurfaceSamples {public string name,material;public Vector3[] points;}
        [Serializable] private sealed class SurfaceSampleRecords {public SurfaceSamples[] items;}
    }
}
