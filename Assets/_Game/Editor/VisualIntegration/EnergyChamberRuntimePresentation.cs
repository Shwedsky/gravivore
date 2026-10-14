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
        public static void FixVisibility() => throw new NotImplementedException("Diagnosis checkpoint must be pushed first.");
        public static void AuthorLookDev() => throw new NotImplementedException("Visibility checkpoint precedes LookDev.");
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
    }
}
