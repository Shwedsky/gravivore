using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    public static class SurfaceHeroV46Builder
    {
        public const string Root="Assets/_Game/Content/SurfaceHeroV46";
        public const string Output="docs/surface-hero-v46/verification";
        public const string Layer="Chapter 01 Production Machinery V46";
        public static string Hierarchy(Transform t)=>t.parent==null?t.name:Hierarchy(t.parent)+"/"+t.name;
        private static string MeshPath(Renderer r){var f=r.GetComponent<MeshFilter>();return AssetDatabase.GetAssetPath(f!=null?f.sharedMesh:r is SkinnedMeshRenderer s?s.sharedMesh:null);}
        public static void Baseline()
        {
            Directory.CreateDirectory(Output);
            var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();
            File.WriteAllText(Output+"/v45_collision_fingerprint.txt",VisualReplacementV3Builder.AuthorityFingerprint(root.VisualEnvironment));
            File.WriteAllLines(Output+"/v45_renderers.tsv",root.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).Select(r=>Hierarchy(r.transform)+"\t"+r.bounds.center.ToString("R")+"\t"+r.bounds.size.ToString("R")+"\t"+string.Join(",",r.sharedMaterials.Select(m=>AssetDatabase.GetAssetPath(m)))+"\t"+MeshPath(r)));
            Debug.Log("SURFACE_HERO_V46_BASELINE_RECORDED");
        }
        private static readonly string[] Families={"PressureVessel","EnergyCylinder","ArcMachine","GateModule","LightDock","HeavyDock","SpecialDock","DockApron","ServiceGantry"};
        private static readonly List<(GameObject obj,Bounds envelope,string family)> Placements=new List<(GameObject,Bounds,string)>();
        private static readonly Dictionary<string,string> Replacements=new Dictionary<string,string>
        {
            {"VR3_generatorpilelarge","PressureVessel"},{"VR3_centrifuge","ArcMachine"},{"VR3_cryotube","EnergyCylinder"},
            {"Hero_Reactor","EnergyCylinder"},{"Primary_Containment","EnergyCylinder"},{"Reactor_V2","ArcMachine"},
            {"Containment_Vessel_V2","EnergyCylinder"},{"Pressure_Wreck_V2","PressureVessel"},{"VR3_generator","ArcMachine"},
            {"Bulkhead_V2","GateModule"}
        };
        private static Texture2D Texture(string name)=>AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/V46_"+name+".png");
        private static Material NewMaterial(string name,string prefix,Color tint,Color emission)
        {
            var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            ApplyMaps(m,prefix,tint,emission);return m;
        }
        private static void ApplyMaps(Material m,string prefix,Color tint,Color emission,bool preserveBase=false)
        {
            if(!preserveBase){m.SetTexture("_BaseMap",Texture(prefix+"BaseColor"));m.SetColor("_BaseColor",tint);m.SetTexture("_MetallicGlossMap",Texture(prefix+"MetallicSmoothness"));}
            m.SetTexture("_BumpMap",Texture(prefix+"Normal"));m.SetTexture("_OcclusionMap",Texture(prefix+"Occlusion"));
            if(!preserveBase){m.SetTexture("_EmissionMap",Texture(prefix+"Emission"));m.SetColor("_EmissionColor",emission);}
            m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_OCCLUSIONMAP");m.EnableKeyword("_METALLICSPECGLOSSMAP");
            if(emission.maxColorComponent>0||preserveBase&&m.IsKeywordEnabled("_EMISSION"))m.EnableKeyword("_EMISSION");else m.DisableKeyword("_EMISSION");
            m.SetFloat("_BumpScale",.72f);m.SetFloat("_Smoothness",.85f);m.SetFloat("_OcclusionStrength",.75f);
            m.SetFloat("_EnvironmentReflections",1);m.SetFloat("_SpecularHighlights",1);m.enableInstancing=true;
            m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;EditorUtility.SetDirty(m);
        }
        private static void Prepare()
        {
            Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Meshes");AssetDatabase.Refresh();
            foreach(var p in Directory.GetFiles(Root+"/Textures","*.png"))
            {
                var i=(TextureImporter)AssetImporter.GetAtPath(p);i.textureType=p.Contains("Normal")?TextureImporterType.NormalMap:TextureImporterType.Default;
                i.sRGBTexture=p.Contains("BaseColor")||p.Contains("Emission");i.mipmapEnabled=true;i.maxTextureSize=p.Contains("Legacy")||p.Contains("Support")||p.Contains("G0")||p.Contains("Slice")?1024:2048;
                i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Trilinear;i.textureCompression=TextureImporterCompression.CompressedHQ;
                var android=i.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=i.maxTextureSize;android.format=TextureImporterFormat.ASTC_6x6;i.SetPlatformTextureSettings(android);i.SaveAndReimport();
            }
            var hero=NewMaterial("V46_HeroAmber","",Color.white,Color.white*1.75f);
            var cyan=NewMaterial("V46_HeroCyan","",Color.white,Color.white*1.9f);cyan.SetTexture("_EmissionMap",Texture("EmissionCyan"));
            var red=NewMaterial("V46_HeroRed","",Color.white,Color.white*1.85f);red.SetTexture("_EmissionMap",Texture("EmissionRed"));
            foreach(var family in Families)
            {
                var p=Root+"/Models/V46_"+family+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(p);
                if(importer==null)throw new InvalidOperationException("Missing Blender export "+p);
                importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;
                importer.animationType=ModelImporterAnimationType.None;importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;importer.isReadable=false;importer.SaveAndReimport();
                var obj=new GameObject("V46_"+family);Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(p),obj.transform,false);
                foreach(var r in obj.GetComponentsInChildren<Renderer>())r.sharedMaterial=hero;
                PrefabUtility.SaveAsPrefabAsset(obj,Root+"/Prefabs/V46_"+family+".prefab");Object.DestroyImmediate(obj);
            }
            // Keep accepted meshes, rig/bone/clip names and UVs. Only shared surfaces change.
            foreach(var path in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game/Content","Assets/_Game/ArtReview/G0ProductionV3"}).Select(AssetDatabase.GUIDToAssetPath))
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m.shader.name!="Universal Render Pipeline/Lit"||path.StartsWith(Root))continue;
                if(m.name=="VR3_WornIndustrialAtlas"||m.name=="V45_WornHousing")ApplyMaps(m,"Legacy_",Color.white,Color.white*1.35f);
                else if(m.name=="Slice_IndustrialAtlas")ApplyMaps(m,"Slice_",Color.white,Color.white*1.25f);
                else if(m.name=="Chapter01_IndustrialAtlas"||m.name=="Fidelity_IndustrialAtlas")ApplyMaps(m,"Legacy_",Color.white,Color.white,true);
                else if(m.name=="VR3_IndustrialTrimAtlas"){m.SetTexture("_OcclusionMap",Texture("Trim_Occlusion"));m.EnableKeyword("_OCCLUSIONMAP");m.SetFloat("_OcclusionStrength",.75f);EditorUtility.SetDirty(m);}
                else if(m.name.StartsWith("G0_V3_Tier"))
                {m.SetTexture("_BumpMap",Texture("G0_Normal"));m.SetTexture("_OcclusionMap",Texture("G0_Occlusion"));m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_OCCLUSIONMAP");m.SetFloat("_BumpScale",.5f);m.SetFloat("_OcclusionStrength",.55f);EditorUtility.SetDirty(m);}
                else if(m.name.StartsWith("V45_"))
                {
                    var tint=m.name.Contains("Graphite")?new Color(.30f,.38f,.43f):m.name.Contains("Ochre")?new Color(1.6f,.88f,.26f):Color.white;
                    var glow=m.name.Contains("Amber")?new Color(1.55f,.47f,.055f)*.15f:m.name.Contains("Cyan")?new Color(.10f,1.3f,1.8f)*.15f:m.name.Contains("Red")?new Color(2.2f,.16f,.035f)*.15f:m.name.Contains("White")?new Color(1.15f,1.35f,1.25f)*.15f:Color.black;
                    ApplyMaps(m,"Support_",tint,glow); // PBR trim; broad V45 neon recedes.
                }
            }
            AssetDatabase.SaveAssets();
        }
        private static void Disable(Renderer r){r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        private static Bounds BoundsOf(Renderer[] rs){var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
        private static GameObject Place(Transform layer,string family,Bounds envelope,string role="Amber",float yaw=0)
        {
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/V46_"+family+".prefab"),layer);
            obj.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            var rs=obj.GetComponentsInChildren<Renderer>();var native=BoundsOf(rs);var size=native.size;
            var rotated=Mathf.Abs(yaw)>1;if(rotated)size=new Vector3(size.z,size.y,size.x);
            var planar=Mathf.Min(envelope.size.x/size.x,envelope.size.z/size.z)*.96f;
            obj.transform.localScale=new Vector3(planar,envelope.size.y/native.size.y,planar);
            var rotation=Quaternion.Euler(0,yaw,0);var offset=Vector3.Scale(new Vector3(native.center.x,native.min.y,native.center.z),obj.transform.localScale);
            obj.transform.SetPositionAndRotation(new Vector3(envelope.center.x,envelope.min.y,envelope.center.z)-rotation*offset,rotation);
            foreach(var r in rs)r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/V46_Hero"+role+".mat");
            foreach(var t in obj.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,StaticEditorFlags.BatchingStatic);
            Placements.Add((obj,envelope,family));
            return obj;
        }
        public static void Build()
        {
            if(!File.Exists(Output+"/v45_collision_fingerprint.txt"))throw new InvalidOperationException("Record V45 baseline before art integration.");
            Prepare();Placements.Clear();var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();var env=root.VisualEnvironment;
            var fingerprint=VisualReplacementV3Builder.AuthorityFingerprint(env);
            if(fingerprint!=File.ReadAllText(Output+"/v45_collision_fingerprint.txt"))throw new InvalidOperationException("V45 authority changed before integration.");
            var previous=env.Floor.Find(Layer);if(previous!=null)Object.DestroyImmediate(previous.gameObject);
            var layer=new GameObject(Layer).transform;layer.SetParent(env.Floor,false);
            var baseline=new HashSet<string>(File.ReadAllLines(Output+"/v45_renderers.tsv").Select(l=>l.Split('\t')[0]));
            foreach(var r in env.Floor.GetComponentsInChildren<Renderer>(true).Where(r=>!r.transform.IsChildOf(layer)))
            {r.enabled=baseline.Contains(Hierarchy(r.transform));PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
            var receipt=new List<string>();
            foreach(var node in env.Floor.GetComponentsInChildren<Transform>(true).Where(t=>Replacements.ContainsKey(t.name)&&!t.IsChildOf(layer)).ToArray())
            {
                var rs=node.GetComponentsInChildren<Renderer>(true);if(rs.Length==0||!rs.Any(r=>baseline.Contains(Hierarchy(r.transform))))continue;
                var b=BoundsOf(rs);foreach(var r in rs)Disable(r);
                var family=Replacements[node.name];var role=b.center.z>57?"Red":Hierarchy(node).Contains("Repair Hub")?"Cyan":"Amber";
                var yaw=family=="GateModule"&&b.size.z>b.size.x?90:0;
                var replacement=Place(layer,family,b,role,yaw);receipt.Add(Hierarchy(node)+" -> "+family+" | "+BoundsOf(replacement.GetComponentsInChildren<Renderer>()));
            }
            var v45=env.Floor.Find(ConceptCorrectiveV45Builder.Layer);
            foreach(var dock in v45.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Serviced enemy dock ")||t.name.StartsWith("Heavy fabrication nest ")).ToArray())
            {
                var heavy=dock.name.StartsWith("Heavy");var special=dock.name.Contains("capacitor")||dock.name.Contains("relay");
                var back=dock.GetComponentsInChildren<Transform>(true).First(t=>t.name==(heavy?"VR3_generatorpilelarge":"VR3_command"));
                var b=BoundsOf(back.GetComponentsInChildren<Renderer>(true));foreach(var r in back.GetComponentsInChildren<Renderer>(true))Disable(r);
                // A heavy rear pile already received a vessel replacement. Replace that art
                // inside this exact authority envelope with the robot service facility.
                foreach(var r in layer.GetComponentsInChildren<Renderer>().Where(r=>Vector3.Distance(r.bounds.center,b.center)<.15f))Disable(r);
                var family=special?"SpecialDock":heavy?"HeavyDock":"LightDock";var obj=Place(layer,family,b,"Red");
                receipt.Add(dock.name+" -> "+family+" opening faces patrol area");
                foreach(var r in dock.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Baked service detail ")))Disable(r);
                // Low berth guides/feed remain exactly on the accepted four origin anchors.
                var apron=Place(layer,"DockApron",new Bounds(dock.position+new Vector3(0,.21f,.75f),new Vector3(4.9f,.11f,5.5f)),"Red");
                apron.name="V46_DockApron "+dock.name;
            }
            ReplaceGantryCubes(v45,layer);
            ResolveMajorIntersections(receipt);
            ClearAncillaryIntersections(env.Floor,layer,receipt);
            if(VisualReplacementV3Builder.AuthorityFingerprint(env)!=fingerprint)throw new InvalidOperationException("Presentation authoring changed collision authority.");
            File.WriteAllLines(Output+"/replacement_manifest.txt",receipt);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            var used=new HashSet<string>(AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true));
            // Remove only this pass's unreferenced generated mesh aliases.
            foreach(var path in Directory.GetFiles(Root+"/Meshes","*.asset").Select(p=>p.Replace('\\','/')))
                if(!used.Contains(path))AssetDatabase.DeleteAsset(path);
            Debug.Log("SURFACE_HERO_V46_AUTHOR_PASS");
        }
        private static bool Overlap(Bounds a,Bounds b,float gap=.06f)
        {
            var v=Vector3.Min(a.max,b.max)-Vector3.Max(a.min,b.min);
            return v.x>-gap&&v.z>-gap&&v.y>.25f;
        }
        private static void ClearAncillaryIntersections(Transform floor,Transform layer,List<string> receipt)
        {
            var major=layer.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&!Hierarchy(r.transform).Contains("DockApron")&&!Hierarchy(r.transform).Contains("ServiceGantry")).ToArray();
            foreach(var r in floor.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&!r.transform.IsChildOf(layer)))
            {
                var path=Hierarchy(r.transform);
                if(!new[]{"Conduit_Rack","VR3_columnpipes","VR3_proppipeholder","VR3_propventbig","Containment_Frame_V2"}.Any(path.Contains))continue;
                var hit=major.FirstOrDefault(m=>{var v=Vector3.Min(m.bounds.max,r.bounds.max)-Vector3.Max(m.bounds.min,r.bounds.min);return v.x>.15f&&v.y>.20f&&v.z>.15f;});
                if(hit==null)continue;
                Disable(r);receipt.Add("Intersection cleanup: legacy service cladding crosses authored machine; suppressed "+path+" at "+r.bounds.center+"; new asset includes attached services.");
            }
        }
        private static void ResolveMajorIntersections(List<string> receipt)
        {
            var major=Placements.Where(p=>p.family!="DockApron"&&p.family!="ServiceGantry"&&p.obj.GetComponentsInChildren<Renderer>().Any(r=>r.enabled)).ToList();
            var removed=new HashSet<GameObject>();
            // Several legacy layers cladded the same proxy. Retain the largest
            // authored machine and suppress its redundant overlapping skin.
            for(var i=0;i<major.Count;i++)for(var j=i+1;j<major.Count;j++)
            {
                if(removed.Contains(major[i].obj)||removed.Contains(major[j].obj))continue;
                var a=BoundsOf(major[i].obj.GetComponentsInChildren<Renderer>());var b=BoundsOf(major[j].obj.GetComponentsInChildren<Renderer>());
                var delta=a.center-b.center;delta.y=0;
                if(delta.magnitude>.85f||!Overlap(a,b)||major[i].family=="GateModule"||major[j].family=="GateModule")continue;
                var discard=a.size.x*a.size.z*a.size.y>=b.size.x*b.size.z*b.size.y?major[j].obj:major[i].obj;
                foreach(var r in discard.GetComponentsInChildren<Renderer>())Disable(r);removed.Add(discard);
                receipt.Add("Intersection cleanup: redundant legacy skin suppressed at "+(discard==major[i].obj?a.center:b.center));
            }
            var accepted=new List<Bounds>();
            foreach(var p in major.Where(p=>!removed.Contains(p.obj)).OrderBy(p=>p.family=="GateModule"?0:p.family.EndsWith("Dock")?1:2).ThenByDescending(p=>p.envelope.size.x*p.envelope.size.z*p.envelope.size.y))
            {
                var original=BoundsOf(p.obj.GetComponentsInChildren<Renderer>());var scale=p.obj.transform.localScale;var position=p.obj.transform.position;
                if(accepted.All(b=>!Overlap(original,b))){accepted.Add(original);continue;}
                var placed=false;
                foreach(var fraction in new[]{1f,.92f,.84f,.76f,.68f,.60f,.52f,.44f,.36f})
                {
                    var ext=original.extents*fraction;var room=p.envelope.extents-ext-Vector3.one*.025f;
                    foreach(var x in new[]{0f,-1f,1f})
                    {
                    foreach(var z in new[]{0f,-1f,1f})
                    {
                        var center=new Vector3(p.envelope.center.x+x*Mathf.Max(0,room.x),p.envelope.min.y+ext.y,p.envelope.center.z+z*Mathf.Max(0,room.z));
                        var candidate=new Bounds(center,ext*2);
                        if(accepted.Any(b=>Overlap(candidate,b)))continue;
                        p.obj.transform.localScale=scale*fraction;
                        var after=BoundsOf(p.obj.GetComponentsInChildren<Renderer>());p.obj.transform.position+=center-after.center;
                        accepted.Add(BoundsOf(p.obj.GetComponentsInChildren<Renderer>()));placed=true;
                        receipt.Add("Intersection cleanup: "+p.family+" remains inside original proxy; scale="+fraction+" center="+center);break;
                    }
                    if(placed)break;
                    }
                    if(placed)break;
                }
                if(!placed)throw new InvalidOperationException("Cannot clear overlapping art within its V45 proxy: "+p.family+" "+original.center);
            }
        }
        public static void Gallery()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.43f,.46f,.50f);RenderSettings.ambientEquatorColor=new Color(.28f,.30f,.34f);RenderSettings.ambientGroundColor=new Color(.14f,.15f,.18f);
            var key=new GameObject("Neutral gallery key").AddComponent<Light>();key.type=LightType.Directional;key.color=Color.white;key.intensity=1.4f;key.transform.rotation=Quaternion.Euler(40,-30,0);
            var fill=new GameObject("Neutral gallery fill").AddComponent<Light>();fill.type=LightType.Directional;fill.color=Color.white;fill.intensity=.45f;fill.transform.rotation=Quaternion.Euler(25,135,0);
            var camera=new GameObject("Neutral gallery camera").AddComponent<UnityEngine.Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.06f,.07f,.085f);camera.fieldOfView=38;camera.nearClipPlane=.05f;
            Directory.CreateDirectory("docs/surface-hero-v46/internal/neutral");
            foreach(var family in Families)
            {
                var obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/V46_"+family+".prefab"));var b=BoundsOf(obj.GetComponentsInChildren<Renderer>());
                camera.transform.position=b.center+new Vector3(.60f,.56f,-1).normalized*Mathf.Max(b.size.x,b.size.y,b.size.z)*2.3f;camera.transform.LookAt(b.center);
                var rt=new RenderTexture(960,960,24,RenderTextureFormat.ARGBHalf);var tex=new Texture2D(960,960,TextureFormat.RGB24,false);
                var prior=RenderTexture.active;camera.targetTexture=rt;
                try{camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,960,960),0,0);tex.Apply();File.WriteAllBytes("docs/surface-hero-v46/internal/neutral/"+family+".png",tex.EncodeToPNG());}
                finally{camera.targetTexture=null;RenderTexture.active=prior;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);Object.DestroyImmediate(obj);}
            }
            Debug.Log("SURFACE_HERO_V46_NEUTRAL_GALLERY_CAPTURED");
        }
        private static void ReplaceGantryCubes(Transform v45,Transform layer)
        {
            var sector=v45.Find("Connected corridor machinery and maintenance alcoves");var zs=new[]{-12f,4f,22f,34f};
            var originals=File.ReadAllLines(Output+"/v45_renderers.tsv").Select(l=>l.Split('\t')).GroupBy(a=>a[0]).ToDictionary(g=>g.Key,g=>g.First()[4]);
            var equivalence=new List<string>();
            foreach(var f in sector.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Baked service detail ")&&f.sharedMesh.vertexCount%24==0))
            {
                var before=f.sharedMesh;var source=AssetDatabase.LoadAssetAtPath<Mesh>(originals[Hierarchy(f.transform)]);
                var mesh=Object.Instantiate(source);var vertices=mesh.vertices;var triangles=mesh.triangles;var keep=new List<int>();
                for(var start=0;start<vertices.Length;start+=24)
                {
                    var center=Vector3.zero;for(var j=0;j<24;j++)center+=f.transform.TransformPoint(vertices[start+j]);center/=24;
                    var gantry=center.y>.2f&&zs.Any(z=>Mathf.Abs(center.z-z)<.65f);
                    if(!gantry)keep.AddRange(triangles.Where(t=>t>=start&&t<start+24));
                }
                mesh.triangles=keep.ToArray();mesh.RecalculateBounds();mesh.name="V46 cleaned service support "+f.GetComponent<Renderer>().sharedMaterial.name;
                if(AssetDatabase.GetAssetPath(before).StartsWith(Root+"/Meshes/"))
                {
                    if(!before.vertices.SequenceEqual(mesh.vertices)||!before.triangles.SequenceEqual(mesh.triangles)||!before.normals.SequenceEqual(mesh.normals)||!before.uv.SequenceEqual(mesh.uv))throw new InvalidOperationException("Mesh alias cleanup changed reviewed support geometry");
                    equivalence.Add(f.name+": vertices, triangles, normals and UVs identical to full-suite tested mesh");
                }
                var path=Root+"/Meshes/Service_"+f.GetComponent<Renderer>().sharedMaterial.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing!=null){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
                f.sharedMesh=mesh;
            }
            if(equivalence.Count>0)File.WriteAllLines(Output+"/mesh_alias_equivalence.txt",equivalence);
            foreach(var z in zs)Place(layer,"ServiceGantry",new Bounds(new Vector3(0,2.045f,z),new Vector3(7.4f,4.09f,.98f)));
        }
    }
}
