using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;
using Camera = UnityEngine.Camera;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Environment-only authoring. No runtime gameplay ownership.</summary>
    public static class Chapter01ArchitectureProduction
    {
        public const string ScenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        public const string Root = "Assets/_Game/Content/Chapter01Architecture/EnergyChamber";
        public const string PrefabPath = Root + "/Prefabs/EnergyChamber_Production.prefab";
        public const string Evidence = "docs/history/implementation-passes/chapter01-architecture-production-v1";
        public static readonly Vector3 Placement = new Vector3(11, .08f, 50.65f);

        public static void Build()
        {
            UrpConfigurator.ConfigureUrp();
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var colliders = contents.transform.Find("Architectural collision");
                if (colliders != null) Object.DestroyImmediate(colliders.gameObject);
                var owner = new GameObject("Architectural collision").transform;
                owner.SetParent(contents.transform, false);
                // Four crossed rectangles approximate the circular stepped plinth
                // without detailed mesh collision or spherical capsule endcaps.
                for (var i = 0; i < 4; i++)
                    Box(owner, "Stepped base " + i, new Vector3(0, 1.075f, 0), new Vector3(5.30f, 2.12f, 2.15f), i * 45);
                Box(owner, "Central containment casing", new Vector3(0, 4.095f, 0), new Vector3(3.19f, 4.03f, 2.96f));
                for (var i = 0; i < 4; i++)
                {
                    var radial = Quaternion.Euler(0, i * 90, 0);
                    Box(owner, "Distribution machinery base " + i, radial * new Vector3(4.396f, .7225f, -.005f), new Vector3(2.6875f, 1.415f, 2.33f), i * 90);
                    Box(owner, "Vertical node support " + i, radial * new Vector3(4.275f, 2.305f, .10f), new Vector3(2.39f, 3.63f, 1.15f), i * 90);
                    Box(owner, "Core-to-node service bridge " + i, radial * new Vector3(2.875f, 1.45f, 0), new Vector3(3.13f, 1.68f, .93f), i * 90);
                }
                // Simple stair compound: solid risers follow the accepted diagonal
                // service stair, leaving the other diagonal service pockets open.
                for (var i = 0; i < 2; i++)
                for (var step = 0; step < 4; step++)
                {
                    var height = (step + 1) * .3275f;
                    var radial = Quaternion.Euler(0, 45 + i * 180, 0);
                    Box(owner, "Service stair " + i + " riser " + step,
                        radial * new Vector3(0, .015f + height * .5f, -4.55f + step * .60f),
                        new Vector3(1.0f, height, .61f), 45 + i * 180);
                }
                var all = contents.GetComponentsInChildren<Renderer>(true);
                var group = contents.GetComponent<LODGroup>();
                if (group == null) group = contents.AddComponent<LODGroup>();
                var lod1 = all.Where(r => !MeshPath(r).Contains("Polish") && !MeshPath(r).Contains("NodeVariant") && !MeshPath(r).Contains("Prop_Rail_")).ToArray();
                var lod2 = all.Where(r => new[] {"EC_Foundation.fbx", "EC_CentralChamber.fbx", "EC_CoreHeart.fbx", "EC_SideNode.fbx", "EC_VerticalDistribution.fbx", "EC_Connector.fbx"}.Any(s => MeshPath(r).EndsWith(s, StringComparison.Ordinal))).ToArray();
                group.fadeMode = LODFadeMode.None;
                group.SetLODs(new[] {new LOD(.38f, all), new LOD(.30f, lod1), new LOD(.08f, lod2)});
                group.RecalculateBounds();
                if (all.Length != 45 || Triangles(all) != 279694)
                    throw new InvalidDataException("Accepted LOD0 changed: " + all.Length + " renderers / " + Triangles(all) + " triangles");
                if (contents.GetComponentsInChildren<MonoBehaviour>(true).Length != 0 || contents.GetComponentsInChildren<Camera>(true).Length != 0)
                    throw new InvalidDataException("Production art contains behaviour or camera");
                PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }

            var scene = EditorSceneManager.OpenScene(ScenePath);
            var existing = scene.GetRootGameObjects().SingleOrDefault(g => g.name == "Energy Chamber / production");
            if (existing != null) Object.DestroyImmediate(existing);
            var installation = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            installation.name = "Energy Chamber / production";
            installation.transform.position = Placement;
            BindCollision(scene, installation);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Inspect();
            Debug.Log("CHAPTER_ARCHITECTURE_PRODUCTION_BUILT");
        }

        public static void TuneLod()
        {
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try {
                var group = contents.GetComponent<LODGroup>();
                var lods = group.GetLODs();
                var thresholds = new[] {.38f, .30f, .08f};
                for (var i = 0; i < lods.Length; i++) lods[i].screenRelativeTransitionHeight = thresholds[i];
                group.SetLODs(lods);
                var blockerLayer = LayerMask.NameToLayer("HardBlocker");
                if (blockerLayer < 0) throw new InvalidDataException("HardBlocker layer is required");
                foreach (var collider in contents.GetComponentsInChildren<BoxCollider>(true)) collider.gameObject.layer = blockerLayer;
                PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var installation = scene.GetRootGameObjects().Single(g => g.name == "Energy Chamber / production");
            installation.transform.position = Placement;
            BindCollision(scene, installation);
            EditorSceneManager.SaveScene(scene);
            Inspect();
        }

        private static void BindCollision(UnityEngine.SceneManagement.Scene scene, GameObject installation)
        {
            var environment = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<ChapterVisualEnvironment>(true)).Single(e => e.BlueprintWorldOnly);
            var serialized = new SerializedObject(environment);
            var roots = serialized.FindProperty("_architecturalCollisionRoots");
            roots.arraySize = 1;
            roots.GetArrayElementAtIndex(0).objectReferenceValue = installation.transform.Find("Architectural collision");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Box(Transform owner, string name, Vector3 center, Vector3 size, float yaw = 0)
        {
            var obj = new GameObject(name, typeof(BoxCollider));
            obj.layer = LayerMask.NameToLayer("HardBlocker");
            obj.transform.SetParent(owner, false);
            obj.transform.localPosition = center;
            obj.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            obj.GetComponent<BoxCollider>().size = size;
        }

        public static void Inspect()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var roots = scene.GetRootGameObjects();
            var groups = roots.SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .Where(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject) || t.name == "Energy Chamber / production")
                .Where(t => t.gameObject.activeInHierarchy && t.GetComponentsInChildren<Renderer>().Length > 0)
                .Select(t => new ArchitectureRecord {
                    name = PathOf(t), prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject),
                    position = t.position, bounds = BoundsOf(t.gameObject),
                    renderers = t.GetComponentsInChildren<Renderer>().Length,
                    triangles = Triangles(t.GetComponentsInChildren<Renderer>()),
                    materialSlots = t.GetComponentsInChildren<Renderer>().Sum(r => r.sharedMaterials.Length),
                    materials = t.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(AssetDatabase.GetAssetPath).Distinct().ToArray(),
                    colliderCount = t.GetComponentsInChildren<Collider>().Length
                }).ToArray();
            Write("architecture-inventory.json", new Inventory { items = groups });
            var installation = roots.Single(g => g.name == "Energy Chamber / production");
            var group = installation.GetComponent<LODGroup>();
            Write("energy-chamber-metrics.json", new LandmarkMetrics {
                position = installation.transform.position, bounds = BoundsOf(installation),
                colliders = installation.GetComponentsInChildren<Collider>().Length, lodSize = group.size,
                lods = group.GetLODs().Select((lod, i) => new LodRecord {
                    level = i, transition = lod.screenRelativeTransitionHeight,
                    renderers = lod.renderers.Length, triangles = Triangles(lod.renderers),
                    materialSlots = lod.renderers.Sum(r => r.sharedMaterials.Length),
                    uniqueMaterials = lod.renderers.SelectMany(r => r.sharedMaterials).Distinct().Count()
                }).ToArray(),
                dependencies = AssetDatabase.GetDependencies(PrefabPath, true)
            });
            File.WriteAllLines(Evidence + "/verification/chapter-hierarchy.txt", roots.SelectMany(g => g.GetComponentsInChildren<Transform>(true)).Select(t => PathOf(t)));
            Debug.Log("CHAPTER_ARCHITECTURE_INSPECTED " + groups.Length + " prefab groups");
        }

        public static void CaptureAudit()
        {
            UrpConfigurator.ConfigureUrp();
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var roots = scene.GetRootGameObjects();
            var camera = new GameObject("Temporary architecture audit camera", typeof(Camera)).GetComponent<Camera>();
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var g0 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab"));
            foreach (var b in g0.GetComponentsInChildren<MonoBehaviour>(true)) b.enabled = false;
            var landmarks = roots.SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .Where(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject) && t.gameObject.activeInHierarchy)
                .Where(t => t.name.Contains("authored industrial sector") || t.name.Contains("Energy Chamber") ||
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject).EndsWith("ChapterServiceNetwork.prefab") ||
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject).EndsWith("IndustrialTransfer.prefab"))
                .Where(t => t.GetComponentsInChildren<Renderer>().Length > 0).ToArray();
            var receipts = new List<CameraReceipt>();
            foreach (var t in landmarks)
            {
                var bounds = BoundsOf(t.gameObject);
                var target = new Vector3(bounds.center.x, .08f, bounds.center.z - Mathf.Min(4, bounds.extents.z));
                g0.transform.position = target;
                ApplyCamera(camera, target);
                var name = string.Concat(t.name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
                var path = Evidence + "/screenshots/audit/" + name + ".png";
                Render(camera, path, 720, 1280);
                receipts.Add(new CameraReceipt { objectPath = PathOf(t), screenshot = path, anchor = target, camera = camera.transform.position, fov = camera.fieldOfView });
            }
            foreach (var anchor in new[] {new Vector3(0, .08f, -20), new Vector3(0, .08f, 20), new Vector3(0, .08f, 43), new Vector3(0, .08f, 65)})
            {
                g0.transform.position = anchor; ApplyCamera(camera, anchor);
                var path = Evidence + "/screenshots/audit/transition_z" + anchor.z + ".png";
                Render(camera, path, 720, 1280);
                receipts.Add(new CameraReceipt { objectPath = "Chapter service spine / gate-transition sample", screenshot = path, anchor = anchor, camera = camera.transform.position, fov = camera.fieldOfView });
            }
            Write("audit-camera-receipts.json", new CameraReceipts { items = receipts.ToArray() });
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("CHAPTER_ARCHITECTURE_AUDIT_CAPTURED " + receipts.Count);
        }

        public static void ApplyCamera(Camera camera, Vector3 target)
        {
            var settings = AssetDatabase.LoadAssetAtPath<CameraFollowSettings>("Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            camera.orthographic = false; camera.fieldOfView = settings.FieldOfView; camera.aspect = 9f / 16;
            camera.nearClipPlane = .1f; camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.045f, .062f, .085f); camera.allowHDR = true;
            camera.transform.position = target + settings.Offset;
            camera.transform.rotation = Quaternion.LookRotation(target + Vector3.up * settings.LookAtHeight - camera.transform.position, Vector3.up);
        }

        public static void Render(Camera camera, string path, int width, int height)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var old = camera.targetTexture; var previous = RenderTexture.active;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            try {
                camera.targetTexture = target; for (var i = 0; i < 3; i++) camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply(); File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally { camera.targetTexture = old; RenderTexture.active = previous; Object.DestroyImmediate(target); Object.DestroyImmediate(pixels); }
        }
        public static Bounds BoundsOf(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            var bounds = renderers[0].bounds; foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds); return bounds;
        }
        public static int Triangles(IEnumerable<Renderer> renderers) => renderers.Sum(r => {
            var mesh = r.GetComponent<MeshFilter>()?.sharedMesh;
            return mesh == null ? 0 : Enumerable.Range(0, mesh.subMeshCount).Sum(s => (int)mesh.GetIndexCount(s) / 3);
        });
        private static string MeshPath(Renderer r) => AssetDatabase.GetAssetPath(r.GetComponent<MeshFilter>()?.sharedMesh);
        private static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
        private static void Write(string file, object record) {
            Directory.CreateDirectory(Evidence + "/verification"); File.WriteAllText(Evidence + "/verification/" + file, JsonUtility.ToJson(record, true));
        }
        [Serializable] private sealed class Inventory { public ArchitectureRecord[] items; }
        [Serializable] private sealed class ArchitectureRecord { public string name, prefab; public Vector3 position; public Bounds bounds; public int renderers, triangles, materialSlots, colliderCount; public string[] materials; }
        [Serializable] private sealed class LandmarkMetrics { public Vector3 position; public Bounds bounds; public int colliders; public float lodSize; public LodRecord[] lods; public string[] dependencies; }
        [Serializable] private sealed class LodRecord { public int level, renderers, triangles, materialSlots, uniqueMaterials; public float transition; }
        [Serializable] private sealed class CameraReceipt { public string objectPath, screenshot; public Vector3 anchor, camera; public float fov; }
        [Serializable] private sealed class CameraReceipts { public CameraReceipt[] items; }
    }
}
