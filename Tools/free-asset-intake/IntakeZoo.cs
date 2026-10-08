// Project-owned inspection code. Copied only to the isolated, ignored scratch project.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

public static class IntakeZoo
{
    [Serializable] public class Entry { public string sourceId, assetPath, originalPath, category; public bool render; }
    [Serializable] public class Catalog { public Entry[] entries; }
    [Serializable] public class Capture { public string sourceId, originalPath, category, renderPath, materialMode; public float sourceLongestDimension, previewScale; }
    [Serializable] public class CaptureList { public List<Capture> captures = new List<Capture>(); }
    static string Work => Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
    static Shader Lit;
    static Material Ground;
    static readonly Dictionary<string, Material> Converted = new Dictionary<string, Material>();

    public static void Build()
    {
        if (!Application.dataPath.Replace('\\','/').EndsWith("/_work_v2/unity_zoo/Assets"))
            throw new InvalidOperationException("Refusing to run outside isolated intake zoo");
        foreach (var path in Directory.GetFiles("Assets/Intake", "*", SearchOption.AllDirectories))
            if (new[] {".cs", ".dll", ".exe", ".ps1", ".bat", ".cmd", ".shader", ".shadergraph", ".blend", ".asmdef"}.Contains(Path.GetExtension(path).ToLowerInvariant()))
                throw new InvalidOperationException("Forbidden payload in art-only project: " + path);
        AssetDatabase.Refresh();
        var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
        Directory.CreateDirectory("Assets/Zoo");
        AssetDatabase.CreateAsset(renderer, "Assets/Zoo/Renderer.asset");
        var pipeline = UniversalRenderPipelineAsset.Create(renderer);
        pipeline.renderScale = 1; pipeline.msaaSampleCount = 1;
        AssetDatabase.CreateAsset(pipeline, "Assets/Zoo/Pipeline.asset");
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        Lit = Shader.Find("Universal Render Pipeline/Lit");
        if (Lit == null) throw new InvalidOperationException("URP Lit unavailable");
        Ground = new Material(Lit); Ground.color = new Color(.035f,.047f,.059f); Ground.SetFloat("_Smoothness", .1f);
        var catalog = JsonUtility.FromJson<Catalog>(File.ReadAllText("catalog.json"));
        var csv = new List<string> {"sourceId,originalPath,assetType,fileFormat,triangleCount,vertexCount,submeshCount,materialCount,textureCount,maxPreviewTextureResolution,shaderFamily,LOD,animationClips,rigged,boneCount,skinnedMeshCount,colliders,lights,particleSystems,scriptsComponents,mobileRisk,urpRisk,measurement"};
        var captures = new CaptureList();
        var zoo = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var categories = new Dictionary<string,Transform>();
        int sampleIndex = 0;
        Directory.CreateDirectory(Path.Combine(Work,"renders/models"));
        foreach (var entry in catalog.entries)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(entry.assetPath);
            if (model == null) throw new InvalidOperationException("Failed model import: " + entry.assetPath);
            var instance = Object.Instantiate(model);
            instance.name = entry.sourceId + " / " + model.name;
            var filters = instance.GetComponentsInChildren<MeshFilter>(true);
            var skin = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshes = filters.Select(x => x.sharedMesh).Concat(skin.Select(x => x.sharedMesh)).Where(x=>x!=null).Distinct().ToArray();
            long triangles = 0; int vertices = 0, submeshes = 0;
            foreach (var mesh in meshes) { vertices += mesh.vertexCount; submeshes += mesh.subMeshCount; for (int s=0;s<mesh.subMeshCount;s++) if(mesh.GetTopology(s)==MeshTopology.Triangles) triangles += (long)mesh.GetIndexCount(s)/3; }
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            var materials = renderers.SelectMany(x=>x.sharedMaterials).Where(x=>x!=null).Distinct().ToArray();
            var textures = new HashSet<Texture>();
            foreach(var mat in materials) foreach(var property in mat.GetTexturePropertyNames()) { var t=mat.GetTexture(property); if(t!=null)textures.Add(t); }
            var clips = AssetDatabase.LoadAllAssetsAtPath(entry.assetPath).OfType<AnimationClip>().Where(x=>!x.name.StartsWith("__preview__")).Select(x=>x.name).Distinct().ToArray();
            var bones = skin.SelectMany(x=>x.bones).Where(x=>x!=null).Distinct().Count();
            var risks = triangles>20000 || materials.Length>6 ? "HIGH_REDUCE_LOD_MATERIALS" : triangles>5000 || materials.Length>3 ? "MEDIUM_BATCH_ATLAS" : "LOW_GEOMETRY_ONLY";
            csv.Add(string.Join(",", new[] {entry.sourceId,entry.originalPath,"model",Path.GetExtension(entry.assetPath),triangles.ToString(),vertices.ToString(),submeshes.ToString(),materials.Length.ToString(),textures.Count.ToString(),textures.Count==0?"0":textures.Max(x=>Math.Max(x.width,x.height)).ToString(),string.Join(";",materials.Select(x=>x.shader==null?"missing":x.shader.name).Distinct()),instance.GetComponentInChildren<LODGroup>(true)!=null?"yes":"no",string.Join(";",clips),(skin.Length>0).ToString(),bones.ToString(),skin.Length.ToString(),instance.GetComponentsInChildren<Collider>(true).Length.ToString(),instance.GetComponentsInChildren<Light>(true).Length.ToString(),instance.GetComponentsInChildren<ParticleSystem>(true).Length.ToString(),string.Join(";",instance.GetComponentsInChildren<Component>(true).Where(x=>x!=null).Select(x=>x.GetType().Name).Distinct()),risks,"REVIEW_MATERIAL_CONVERSION","Unity6000.3.0f1_imported_mesh"}.Select(Escape)));
            if(!entry.render){Object.DestroyImmediate(instance);continue;}
            bool hadTexture = false;
            foreach (var r in renderers)
            {
                r.sharedMaterials = r.sharedMaterials.Select(m=>Convert(m,entry.sourceId,model.name)).ToArray();
                if(r.sharedMaterials.Any(m=>m!=null && m.GetTexture("_BaseMap")!=null)) hadTexture=true;
            }
            foreach(var animator in instance.GetComponentsInChildren<Animator>(true)) animator.enabled=false;
            var bounds = BoundsOf(instance);
            var longest = Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
            if(longest<=.0001f)throw new InvalidOperationException("Empty render sample: "+entry.assetPath);
            // Preview uses a documented 4m inspection plinth; original dimensions are recorded.
            float scale = 4f/longest;
            instance.transform.localScale *= scale;
            bounds=BoundsOf(instance);
            instance.transform.position -= new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            string file = entry.sourceId+"__"+Path.GetFileNameWithoutExtension(entry.assetPath)+".png";
            string output = Path.Combine(Work,"renders/models",file);
            Render(instance,output);
            captures.captures.Add(new Capture{sourceId=entry.sourceId,originalPath=entry.originalPath,category=entry.category,renderPath=output,materialMode=hadTexture?"source texture/color -> neutral URP Lit":"source colors or neutral geometry fallback; no texture detected",sourceLongestDimension=longest,previewScale=scale});
            if(!categories.TryGetValue(entry.category,out var parent)){parent=new GameObject(entry.category).transform;categories.Add(entry.category,parent);}
            instance.transform.SetParent(parent,true);
            instance.transform.position += new Vector3((sampleIndex%12)*6,0,(sampleIndex/12)*8);
            sampleIndex++;
        }
        File.WriteAllLines(Path.Combine(Work,"reports/unity_models.csv"),csv);
        File.WriteAllText(Path.Combine(Work,"reports/render_manifest.json"),JsonUtility.ToJson(captures,true));
        EditorSceneManager.SaveScene(zoo,"Assets/Zoo/AssetZoo.unity");
        AssetDatabase.SaveAssets();
        Debug.Log($"INTAKE_ZOO_COMPLETE models={catalog.entries.Length} captures={captures.captures.Count}");
    }

    static string Escape(string s)=>"\""+(s??"").Replace("\"","\"\"")+"\"";
    static Bounds BoundsOf(GameObject go){var rr=go.GetComponentsInChildren<Renderer>(true);var b=rr.Length>0?rr[0].bounds:new Bounds(go.transform.position,Vector3.zero);foreach(var r in rr)b.Encapsulate(r.bounds);return b;}

    static Material Convert(Material input,string source,string model)
    {
        string key=source+"/"+(input==null?"fallback":input.name);
        if(Converted.TryGetValue(key,out var cached))return cached;
        var mat=new Material(Lit){name=key.Replace('/','_')};
        var color=Color.gray; Texture texture=null,normal=null,metal=null; Color emission=Color.black;
        if(input!=null)
        {
            if(input.HasProperty("_BaseColor"))color=input.GetColor("_BaseColor");else if(input.HasProperty("_Color"))color=input.GetColor("_Color");
            if(input.HasProperty("_BaseMap"))texture=input.GetTexture("_BaseMap");if(texture==null&&input.HasProperty("_MainTex"))texture=input.GetTexture("_MainTex");
            if(input.HasProperty("_BumpMap"))normal=input.GetTexture("_BumpMap");if(input.HasProperty("_MetallicGlossMap"))metal=input.GetTexture("_MetallicGlossMap");
            if(input.HasProperty("_EmissionColor"))emission=input.GetColor("_EmissionColor");
        }
        if(texture==null)
        {
            var guids=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Intake/"+source});
            var candidates=guids.Select(AssetDatabase.GUIDToAssetPath).Where(p=>!p.EndsWith(".meta")&&!new[]{"normal","rough","metal","occlu","emissi","height","specular","preview","sample","cover"}.Any(x=>Path.GetFileName(p).ToLowerInvariant().Contains(x))).ToArray();
            var best=candidates.OrderByDescending(p=>TextureScore(p,input==null?model:input.name,model)).FirstOrDefault();
            if(best!=null && TextureScore(best,input==null?model:input.name,model)>0)texture=AssetDatabase.LoadAssetAtPath<Texture2D>(best);
        }
        mat.SetColor("_BaseColor",color);mat.SetTexture("_BaseMap",texture);mat.SetFloat("_Metallic",metal==null?.18f:.6f);mat.SetFloat("_Smoothness",.25f);
        if(normal!=null){mat.SetTexture("_BumpMap",normal);mat.EnableKeyword("_NORMALMAP");}
        if(metal!=null){mat.SetTexture("_MetallicGlossMap",metal);mat.EnableKeyword("_METALLICSPECGLOSSMAP");}
        mat.SetColor("_EmissionColor",emission*.2f);if(emission.maxColorComponent>0)mat.EnableKeyword("_EMISSION");
        string path="Assets/Zoo/"+Sanitize(key)+".mat"; AssetDatabase.CreateAsset(mat,path);Converted.Add(key,mat);return mat;
    }
    static string Sanitize(string s)=>string.Concat(s.Select(c=>char.IsLetterOrDigit(c)?c:'_'));
    static int TextureScore(string path,string material,string model)
    {
        string n=Path.GetFileNameWithoutExtension(path).ToLowerInvariant(),m=material.ToLowerInvariant(),o=model.ToLowerInvariant();
        int score=0;if(n.Contains(o))score+=10;if(n.Contains(m))score+=10;if(n.Contains("albedo")||n.Contains("basecolor")||n.Contains("base_color")||n.Contains("diffuse"))score+=4;if(n.Contains("atlas")||n.Contains("colormap"))score+=2;if(n.Contains("texture"))score+=1;return score;
    }

    static void Render(GameObject subject,string output)
    {
        // Hide previously placed zoo samples during each individual capture.
        var roots=subject.scene.GetRootGameObjects().Where(x=>x!=subject&&x.activeSelf).ToArray();foreach(var root in roots)root.SetActive(false);
        var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Neutral industrial plinth";floor.transform.position=new Vector3(0,-.06f,0);floor.transform.localScale=new Vector3(30,.1f,30);floor.GetComponent<Renderer>().sharedMaterial=Ground;
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.36f,.39f,.43f);
        var lightObj=new GameObject("Neutral key");var light=lightObj.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.6f;light.color=new Color(.94f,.97f,1);lightObj.transform.rotation=Quaternion.Euler(48,-30,0);
        var cameraObj=new GameObject("Elevated inspection camera");var cam=cameraObj.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.035f,.045f);cam.fieldOfView=35;cam.nearClipPlane=.05f;cam.farClipPlane=100;cam.allowHDR=false;
        var b=BoundsOf(subject);cam.transform.position=b.center+new Vector3(5,8,-6);cam.transform.LookAt(b.center);cam.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing=false;
        var rt=new RenderTexture(480,360,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;
        var tex=new Texture2D(480,360,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,480,360),0,0);tex.Apply();File.WriteAllBytes(output,tex.EncodeToPNG());RenderTexture.active=old;cam.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
        Object.DestroyImmediate(cameraObj);Object.DestroyImmediate(lightObj);Object.DestroyImmediate(floor);foreach(var root in roots)root.SetActive(true);
    }
}
