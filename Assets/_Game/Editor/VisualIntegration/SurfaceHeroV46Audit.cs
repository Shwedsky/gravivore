using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public sealed class SurfaceHeroV46Audit : IPreprocessBuildWithReport,IPostprocessBuildWithReport
    {
        public int callbackOrder=>700;
        [Serializable] private sealed class Row
        {public string renderer,material,mesh;public bool hero,baseMap,normal,metallicSmoothnessVariation,aoRecess,emissionMask,uniformFlatColorOnly;public string exemption;}
        [Serializable] private sealed class Pair{public string a,b;public Vector3 overlap;}
        [Serializable] private sealed class Report
        {public bool validated;public int flatColorOnlyHeroRenderers,primitiveOnlyHeroProps,heroRenderers,enabledRenderers,sharedMaterials,authoredLights;public long staticTriangles;public Row[] renderers;public Pair[] boundsOverlapCandidates;public string overlapMethod="Conservative bounds candidates, including hollow frames; actual production-camera inspection determines visible defects.";}
        public static void ValidateOrThrow()
        {
            if(!Directory.Exists(SurfaceHeroV46Builder.Root))return;
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
                var root=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<S01SceneCompositionRoot>(true)).Single();var env=root.VisualEnvironment;
                var layer=env.Floor.Find(SurfaceHeroV46Builder.Layer);if(layer==null)throw new InvalidOperationException("V46 layer missing");
                if(layer.GetComponentsInChildren<Collider>(true).Length!=0||layer.GetComponentsInChildren<MonoBehaviour>(true).Length!=0||layer.GetComponentsInChildren<Light>(true).Length!=0)throw new InvalidOperationException("V46 art owns gameplay authority or lights");
                if(VisualReplacementV3Builder.AuthorityFingerprint(env)!=File.ReadAllText(SurfaceHeroV46Builder.Output+"/v45_collision_fingerprint.txt"))throw new InvalidOperationException("V45 collision authority changed");
                var rs=root.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();var rows=new List<Row>();
                var report=new Report{enabledRenderers=rs.Length,sharedMaterials=rs.SelectMany(r=>r.sharedMaterials).Distinct().Count(),authoredLights=root.GetComponentsInChildren<Light>(true).Length};
                foreach(var r in rs)
                {
                    var f=r.GetComponent<MeshFilter>();var mesh=f!=null?f.sharedMesh:r is SkinnedMeshRenderer s?s.sharedMesh:null;
                    var own=r.transform.IsChildOf(layer);var support=r.name.StartsWith("Baked service detail ");var hero=!support&&(own&&!r.name.Contains("Apron")&&!SurfaceHeroV46Builder.Hierarchy(r.transform).Contains("DockApron")||r is SkinnedMeshRenderer||r.bounds.size.y>.65f&&Mathf.Max(r.bounds.size.x,r.bounds.size.z)>.85f);
                    if(mesh!=null&&r is MeshRenderer)for(var i=0;i<mesh.subMeshCount;i++)report.staticTriangles+=(long)mesh.GetIndexCount(i)/3;
                    if(own&&mesh!=null&&AssetDatabase.GetAssetPath(mesh).Contains("SurfaceHeroV46/Models/")&&mesh.subMeshCount!=1)throw new InvalidOperationException("Hero mesh must use one atlas material");
                    if(hero&&mesh!=null&&(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mesh))||new[]{"Cube","Cylinder","Sphere","Capsule"}.Contains(mesh.name)))report.primitiveOnlyHeroProps++;
                    foreach(var m in r.sharedMaterials)
                    {
                        Rendering.RenderingBuildAudit.ValidateMaterial(m,r.name);
                        var lit=m.shader.name=="Universal Render Pipeline/Lit";
                        var row=new Row{renderer=SurfaceHeroV46Builder.Hierarchy(r.transform),material=AssetDatabase.GetAssetPath(m),mesh=AssetDatabase.GetAssetPath(mesh),hero=hero&&lit,
                            baseMap=lit&&m.GetTexture("_BaseMap")!=null,normal=lit&&m.GetTexture("_BumpMap")!=null,
                            metallicSmoothnessVariation=lit&&m.GetTexture("_MetallicGlossMap")!=null,aoRecess=lit&&m.GetTexture("_OcclusionMap")!=null,
                            emissionMask=lit&&m.GetTexture("_EmissionMap")!=null&&m.IsKeywordEnabled("_EMISSION"),exemption=hero?"":support?"Baked low gratings, rails, small beacon pedestals and inaccessible utility trim; broad gantries replaced in Blender":"Small trim, floor, VFX or presentation helper"};
                        row.uniformFlatColorOnly=lit&&!row.baseMap;rows.Add(row);
                        if(row.hero){report.heroRenderers++;if(row.uniformFlatColorOnly)report.flatColorOnlyHeroRenderers++;if(own&&(!row.normal||!row.metallicSmoothnessVariation||!row.aoRecess))throw new InvalidOperationException("Incomplete hero PBR maps: "+row.renderer);}
                    }
                }
                report.renderers=rows.ToArray();var major=layer.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&!SurfaceHeroV46Builder.Hierarchy(r.transform).Contains("DockApron")&&!SurfaceHeroV46Builder.Hierarchy(r.transform).Contains("ServiceGantry")).ToArray();
                var pairs=new List<Pair>();
                for(var i=0;i<major.Length;i++)for(var j=i+1;j<major.Length;j++)
                {
                    var a=major[i].bounds;var b=major[j].bounds;var overlap=Vector3.Min(a.max,b.max)-Vector3.Max(a.min,b.min);
                    if(overlap.x>.12f&&overlap.y>.25f&&overlap.z>.12f)pairs.Add(new Pair{a=SurfaceHeroV46Builder.Hierarchy(major[i].transform)+" @ "+a.center,b=SurfaceHeroV46Builder.Hierarchy(major[j].transform)+" @ "+b.center,overlap=overlap});
                }
                report.boundsOverlapCandidates=pairs.ToArray();report.validated=report.flatColorOnlyHeroRenderers==0&&report.primitiveOnlyHeroProps==0;
                Directory.CreateDirectory(SurfaceHeroV46Builder.Output);File.WriteAllText(SurfaceHeroV46Builder.Output+"/material_quality_audit.json",JsonUtility.ToJson(report,true));
                File.WriteAllLines(SurfaceHeroV46Builder.Output+"/v46_renderers.tsv",rs.Select(r=>SurfaceHeroV46Builder.Hierarchy(r.transform)+"\t"+r.bounds.center.ToString("R")+"\t"+r.bounds.size.ToString("R")+"\t"+string.Join(",",r.sharedMaterials.Select(m=>AssetDatabase.GetAssetPath(m)))));
                if(!report.validated)throw new InvalidOperationException("V46 hero surface/geometry quality audit failed");
            }
            finally{if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);}
            Debug.Log("SURFACE_HERO_V46_MATERIAL_AUTHORITY_AUDIT_PASS");
        }
        public void OnPreprocessBuild(BuildReport report)=>ValidateOrThrow();
        public void OnPostprocessBuild(BuildReport report)
        {
            var packed=report.packedAssets.SelectMany(p=>p.contents).Select(c=>c.sourceAssetPath).Distinct().ToArray();
            var required=AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath,true).Where(p=>p.StartsWith(SurfaceHeroV46Builder.Root+"/Materials/")||p.StartsWith(SurfaceHeroV46Builder.Root+"/Textures/")).ToArray();
            foreach(var p in required)if(!packed.Contains(p))throw new BuildFailedException("V46 PBR resource missing in build: "+p);
            File.WriteAllLines(SurfaceHeroV46Builder.Output+"/apk_surface_dependencies.txt",required);
        }
    }
}
