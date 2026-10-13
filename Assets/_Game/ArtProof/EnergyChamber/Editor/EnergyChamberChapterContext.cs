using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Gravivore.ArtProof.EnergyChamber
{
    /// <summary>Static authoring evidence only. Never saves the production scene.</summary>
    public static class EnergyChamberChapterContext
    {
        public const string Source = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        public const string ScenePath = EnergyChamberProofBuilder.Root + "/Scenes/EnergyChamber_Chapter01Context.unity";
        public static readonly Vector3 InstallationPosition = new Vector3(11,.08f,51.3f);
        public static readonly Vector3 PlayerPosition = InstallationPosition + new Vector3(3,0,-6.2f);
        public static void Audit()
        {
            var scene = EditorSceneManager.OpenScene(Source);
            var lines = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Renderer>(true))
                .Select(r => PathOf(r.transform) + " | active=" + r.gameObject.activeInHierarchy + " | bounds=" + r.bounds + " | mesh=" + AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>()?.sharedMesh)).ToArray();
            Directory.CreateDirectory(EnergyChamberProofBuilder.Evidence + "/verification");
            File.WriteAllLines(EnergyChamberProofBuilder.Evidence + "/verification/chapter-source-audit.txt", lines);
            Debug.Log("ENERGY_CHAMBER_CHAPTER_AUDITED renderers=" + lines.Length);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        public static void Build()
        {
            var hash=Hash(Source);
            var scene=EditorSceneManager.OpenScene(Source);
            var roots=scene.GetRootGameObjects();
            var sourceRenderers=roots.SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).ToArray();
            var activeCount=sourceRenderers.Count(r=>r.gameObject.activeInHierarchy && r.enabled);
            var sourcePositions=sourceRenderers.Select(r=>r.transform.localToWorldMatrix).ToArray();
            var sourceMaterials=sourceRenderers.Select(r=>r.sharedMaterials).ToArray();
            var sourceMeshes=sourceRenderers.Select(r=>r.GetComponent<MeshFilter>()?.sharedMesh).ToArray();
            var lights=roots.SelectMany(g=>g.GetComponentsInChildren<Light>(true)).ToArray();
            var volumes=roots.SelectMany(g=>g.GetComponentsInChildren<Volume>(true)).ToArray();
            // Unpack instance links in the isolated copy before stripping domain
            // components. Source prefab assets and Chapter scene are never saved.
            foreach(var instance in roots.SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
                .Select(t=>PrefabUtility.GetOutermostPrefabInstanceRoot(t.gameObject)).Where(g=>g!=null).Distinct().ToArray())
                if(PrefabUtility.IsPartOfPrefabInstance(instance))PrefabUtility.UnpackPrefabInstance(instance,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            foreach(var component in roots.SelectMany(g=>g.GetComponentsInChildren<Component>(true)).Where(c=>c!=null).Reverse().ToArray())
            {
                if(component is Transform || component is Renderer || component is MeshFilter || component is Light ||
                    component is ReflectionProbe || component is LODGroup || component is Volume || component is UniversalAdditionalLightData)continue;
                Object.DestroyImmediate(component);
            }
            for(var i=0;i<sourceRenderers.Length;i++)
            {
                var r=sourceRenderers[i];
                if(r.transform.localToWorldMatrix!=sourcePositions[i] || !r.sharedMaterials.SequenceEqual(sourceMaterials[i]) ||
                    r.GetComponent<MeshFilter>()?.sharedMesh!=sourceMeshes[i])throw new InvalidDataException("Context changed source presentation: "+r.name);
            }
            var installation=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnergyChamberProofBuilder.PrefabPath));
            installation.name="Energy Chamber / hybrid proof V1";installation.transform.position=InstallationPosition;
            var g0=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(EnergyChamberProofBuilder.G0Path));
            g0.name="G-0 / unchanged current Tier 0 scale reference";g0.transform.position=PlayerPosition;
            foreach(var behaviour in g0.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            var camera=new GameObject("Actual gameplay camera / Chapter 1 player anchor",typeof(UnityEngine.Camera)).GetComponent<UnityEngine.Camera>();
            camera.tag="MainCamera";EnergyChamberProofBuilder.ApplyGameplayCamera(camera,PlayerPosition);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var bounds=EnergyChamberProofBuilder.BoundsOf(installation);
            var overlap=sourceRenderers.Where(r=>r.enabled && r.gameObject.activeInHierarchy && r.bounds.min.y>.12f && r.bounds.Intersects(bounds)).Select(r=>PathOf(r.transform)).ToArray();
            if(overlap.Length!=0)throw new InvalidDataException("Installation intersects existing raised Chapter meshes: "+string.Join(", ",overlap));
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            if(Hash(Source)!=hash)throw new InvalidDataException("Production Chapter scene changed.");
            Directory.CreateDirectory(EnergyChamberProofBuilder.Evidence+"/verification");
            File.WriteAllText(EnergyChamberProofBuilder.Evidence+"/verification/chapter-context.json",JsonUtility.ToJson(new ContextRecord{
                source=Source,sourceSha256=hash,scene=ScenePath,placement=InstallationPosition,player=PlayerPosition,
                sourceRenderers=sourceRenderers.Length,activeRenderers=activeCount,sourceLights=lights.Length,
                sourceVolumeProfiles=volumes.Select(v=>AssetDatabase.GetAssetPath(v.sharedProfile)).ToArray(),
                sky=RenderSettings.ambientSkyColor,equator=RenderSettings.ambientEquatorColor,ground=RenderSettings.ambientGroundColor,
                reflection=AssetDatabase.GetAssetPath(RenderSettings.customReflectionTexture),reflectionIntensity=RenderSettings.reflectionIntensity,
                raisedMeshIntersections=overlap.Length,sourceGeometryMaterialsTransformsPreserved=true},true));
            Debug.Log("ENERGY_CHAMBER_CHAPTER_CONTEXT_BUILT sourcePresentationPreserved=true");
        }

        public static void Capture()
        {
            EditorSceneManager.OpenScene(ScenePath);
            var roots=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            var camera=roots.SelectMany(g=>g.GetComponentsInChildren<UnityEngine.Camera>()).Single();
            var g0=roots.Single(g=>g.name.StartsWith("G-0 /",StringComparison.Ordinal));
            var assembly=roots.Single(g=>g.name.StartsWith("Energy Chamber /",StringComparison.Ordinal));
            var pos=InstallationPosition;
            Directory.CreateDirectory(EnergyChamberProofBuilder.Evidence+"/screenshots");
            EnergyChamberProofBuilder.ApplyGameplayCamera(camera,PlayerPosition);
            g0.SetActive(false);EnergyChamberProofBuilder.Render(camera,"09_chapter01_gameplay",1080,1920);
            g0.SetActive(true);EnergyChamberProofBuilder.Render(camera,"10_chapter01_gameplay_G0",1080,1920);
            EnergyChamberProofBuilder.SetView(camera,pos+new Vector3(12,17,-13),pos+new Vector3(0,1.8f,0),48);
            EnergyChamberProofBuilder.Render(camera,"11_chapter01_high_three_quarter",1600,1300);
            EnergyChamberProofBuilder.SetView(camera,pos+new Vector3(7.8f,8.5f,-10),pos+new Vector3(.1f,2.8f,0),42);
            EnergyChamberProofBuilder.Render(camera,"12_chapter01_close_three_quarter",1600,1600);
            // Reduce only installation emission. Surrounding Chapter lighting,
            // grading, materials and emitters remain exactly as authored.
            var renderers=assembly.GetComponentsInChildren<Renderer>(true);
            var originals=renderers.Select(r=>r.sharedMaterials).ToArray();
            var replacements=originals.SelectMany(m=>m).Distinct().ToDictionary(m=>m,m=>new Material(m));
            try
            {
                foreach(var m in replacements.Values){m.DisableKeyword("_EMISSION");if(m.HasProperty("_EmissionColor"))m.SetColor("_EmissionColor",Color.black);}
                for(var i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i].Select(m=>replacements[m]).ToArray();
                EnergyChamberProofBuilder.ApplyGameplayCamera(camera,PlayerPosition);
                EnergyChamberProofBuilder.Render(camera,"13_chapter01_emission_disabled",1080,1920);
                EnergyChamberProofBuilder.SetView(camera,pos+new Vector3(7.8f,8.5f,-10),pos+new Vector3(.1f,2.8f,0),42);
                EnergyChamberProofBuilder.Render(camera,"14_chapter01_emission_disabled_detail",1600,1600);
            }
            finally
            {
                for(var i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=originals[i];
                foreach(var m in replacements.Values)Object.DestroyImmediate(m);
            }
            Debug.Log("ENERGY_CHAMBER_CHAPTER_CONTEXT_CAPTURED");
        }
        private static string Hash(string path)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
        [Serializable] private sealed class ContextRecord
        {
            public string source,sourceSha256,scene,reflection;public string[] sourceVolumeProfiles;
            public Vector3 placement,player;public Color sky,equator,ground;public float reflectionIntensity;
            public int sourceRenderers,activeRenderers,sourceLights,raisedMeshIntersections;
            public bool sourceGeometryMaterialsTransformsPreserved;
        }
        private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    }
}
