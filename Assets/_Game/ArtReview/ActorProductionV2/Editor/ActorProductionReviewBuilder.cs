using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.ArtReview;
using Gravivore.Presentation.Camera;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;
using Camera = UnityEngine.Camera;

namespace Gravivore.Editor.ActorProduction
{
    public static class ActorProductionReviewBuilder
    {
        public const string Root = "Assets/_Game/ArtReview/ActorProductionV2";
        public const string ReviewScene = Root + "/ActorProductionV2_Review.unity";
        public const string Evidence = "docs/history/implementation-passes/chapter01-actor-production-v2";
        public const string ProductionScene = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        public static readonly string[] Actors = { "Scout", "Cutter", "Warden", "ArcDrone", "Carrier", "Magnetar", "Custodian" };
        private static readonly float[] CollisionRadii = { .35f, .4f, .48f, .36f, .52f, .65f, 1f };
        private static readonly string[] RequiredStates = { "Idle", "Run", "Attack", "Hit", "Death" };
        [Serializable] public sealed class MeshInfo
        { public string name; public int triangles, vertices, bones, materials; public Vector3 dimensions; }
        [Serializable] public sealed class ActorInfo
        {
            public string actor; public MeshInfo[] lods; public string[] sockets, clips;
            public Vector3 renderDimensions; public float collisionRadius, visualPlanarRadius;
            public int gameplayAutoLod;
            public bool rootMotionDisabled, colliderFree, readyForChapter01Integration;
            public string envelopeNote;
        }
        [Serializable] public sealed class ValidationReport
        {
            public string engine; public ActorInfo[] actors; public bool productionDependencyIsolation;
            public float cameraFov, cameraLookAt; public Vector3 cameraOffset;
            public Vector3 g0RenderDimensions, g0RootScale;
            public string g0Reference = "Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab / unchanged";
            public string gate = "Isolated review only. Owner device visuals/performance and production encounter clearance still required.";
        }
        [MenuItem("Gravivore/Art Review/Actor Production V2/Build and Capture")]
        public static void BuildAndCapture()
        {
            Directory.CreateDirectory(Evidence + "/unity"); Directory.CreateDirectory(Root + "/Materials"); Directory.CreateDirectory(Root + "/Prefabs");
            AssetDatabase.Refresh();
            var canonicalBytes = File.ReadAllBytes(ProductionScene);
            var settings = AssetDatabase.LoadAssetAtPath<CameraFollowSettings>("Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            if (settings == null) throw new InvalidOperationException("Missing canonical camera settings");
            // Read main lighting and reuse existing world materials/prefabs. Main
            // is opened for inspection only and is never saved by this builder.
            var main = EditorSceneManager.OpenScene(ProductionScene);
            var mainLight = main.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Light>(true)).FirstOrDefault(x => x.type == LightType.Directional);
            var lightColor = mainLight != null ? mainLight.color : new Color(.85f, .91f, 1);
            var lightIntensity = mainLight != null ? mainLight.intensity : 1.2f;
            var lightRotation = mainLight != null ? mainLight.transform.rotation : Quaternion.Euler(48, -30, 0);
            var ambient = RenderSettings.ambientLight; var ambientIntensity = RenderSettings.ambientIntensity;
            var ambientMode = RenderSettings.ambientMode; var ambientSky = RenderSettings.ambientSkyColor;
            var ambientEquator = RenderSettings.ambientEquatorColor; var ambientGround = RenderSettings.ambientGroundColor;
            var reflectionMode = RenderSettings.defaultReflectionMode; var reflection = RenderSettings.customReflectionTexture;
            var reflectionIntensity = RenderSettings.reflectionIntensity; var ambientProbe = RenderSettings.ambientProbe;
            var prefabs = new List<GameObject>();
            foreach (var name in Actors) prefabs.Add(ImportAndMakePrefab(name));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = ambientMode; RenderSettings.ambientLight = ambient; RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientSkyColor = ambientSky; RenderSettings.ambientEquatorColor = ambientEquator; RenderSettings.ambientGroundColor = ambientGround;
            RenderSettings.defaultReflectionMode = reflectionMode; RenderSettings.customReflectionTexture = reflection; RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.ambientProbe = ambientProbe;
            var sun = new GameObject("Review copy of production directional light").AddComponent<Light>(); sun.type = LightType.Directional; sun.color = lightColor; sun.intensity = lightIntensity; sun.transform.rotation = lightRotation; sun.shadows = LightShadows.Soft;
            var cam = new GameObject("Production portrait camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();
            cam.fieldOfView = settings.FieldOfView; cam.aspect = 9f / 16f; cam.nearClipPlane = .1f; cam.farClipPlane = 100;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.025f, .033f, .045f);
            cam.transform.position = settings.Offset; cam.transform.LookAt(Vector3.up * settings.LookAtHeight);
            var environment = new GameObject("Read-only existing environment presentation");
            // Clone existing authored presentation onto a diagnostic stage.
            // This is not a new Chapter01 topology or a collision layout.
            for (var x = -1; x <= 1; x++) for (var z = -1; z <= 1; z++)
            {
                var floor = Spawn("Assets/_Game/Content/ConceptFidelityV2/Prefabs/Deck_V2_0.prefab", environment.transform);
                floor.transform.position = new Vector3(x * 6, -.08f, z * 6);
            }
            var reactor = Spawn("Assets/_Game/Content/ConceptFidelityV2/Prefabs/Reactor_V2.prefab", environment.transform); reactor.transform.position = new Vector3(4, 0, 5);
            var gate = Spawn("Assets/_Game/Content/VisualSlice/Prefabs/Containment_Gate.prefab", environment.transform); gate.transform.position = new Vector3(-4, 0, 5);
            // Remove any presentation colliders from the review copies only.
            foreach (var collider in environment.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            var g0 = Spawn("Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab", null);
            g0.name = "G-0 unchanged production reference"; g0.transform.position = new Vector3(-2.25f, 0, 0);
            var playerRenderers = g0.GetComponentsInChildren<Renderer>(true);
            var playerBounds = playerRenderers[0].bounds;
            foreach (var renderer in playerRenderers.Skip(1)) playerBounds.Encapsulate(renderer.bounds);
            var instances = prefabs.Select(x => (GameObject)PrefabUtility.InstantiatePrefab(x, scene)).ToArray();
            var reports = new List<ActorInfo>();
            for (var index = 0; index < instances.Length; index++)
            {
                foreach (var other in instances) other.SetActive(false);
                var go = instances[index]; go.SetActive(true); go.transform.position = Vector3.zero;
                var animator = go.GetComponent<Animator>(); var group = go.GetComponent<LODGroup>();
                var skins = go.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(x => x.name).ToArray();
                group.ForceLOD(0); Pose(go, "Idle", 0);
                var bounds = skins[0].bounds;
                if (bounds.size.y < .15f || bounds.size.y > 6) throw new InvalidOperationException(Actors[index] + " invalid import scale " + bounds);
                reports.Add(new ActorInfo { actor = Actors[index], collisionRadius = CollisionRadii[index], visualPlanarRadius = Mathf.Max(bounds.extents.x, bounds.extents.z),
                    renderDimensions = bounds.size, rootMotionDisabled = !animator.applyRootMotion, colliderFree = go.GetComponentsInChildren<Collider>(true).Length == 0,
                    gameplayAutoLod = CameraLod(group, cam),
                    sockets = go.GetComponentsInChildren<Transform>(true).Where(t => t.GetComponent<Renderer>() == null && t.childCount == 0).Select(t => t.name).ToArray(),
                    clips = animator.runtimeAnimatorController.animationClips.Select(x => x.name).Distinct().ToArray(),
                    lods = skins.Select(x => new MeshInfo { name = x.name, triangles = x.sharedMesh.triangles.Length / 3, vertices = x.sharedMesh.vertexCount, bones = x.bones.Length, materials = x.sharedMaterials.Length, dimensions = x.sharedMesh.bounds.size }).ToArray(),
                    readyForChapter01Integration = false, envelopeNote = "Body-center collider remains authoritative; outboard tools/supports are presentation reach. Production wall/turn/encounter clearance must be reviewed during separately authorized integration." });
                for (var heading = 0; heading < 4; heading++)
                {
                    go.transform.rotation = Quaternion.Euler(0, heading * 90, 0);
                    Capture(cam, go, Actors[index] + "_gameplay_" + heading + ".png", 1080, 1920, -1);
                }
                go.transform.rotation = Quaternion.identity;
                Capture(cam, go, Actors[index] + "_phone.png", 360, 640, -1);
                Capture(cam, go, Actors[index] + "_scale_G0.png", 1080, 1920, -1);
                var actorMaterial = skins[0].sharedMaterial; var emission = actorMaterial.GetColor("_EmissionColor");
                try { actorMaterial.SetColor("_EmissionColor", Color.black); Capture(cam, go, Actors[index] + "_emission_disabled.png", 360, 640, -1); }
                finally { actorMaterial.SetColor("_EmissionColor", emission); }
                for (var lod = 0; lod < 3; lod++) Capture(cam, go, Actors[index] + "_LOD" + lod + ".png", 360, 640, lod);
                foreach (var state in new[] { "Run", "Attack", "Hit", "Death", "Windup", "Block", "Cut", "Discharge", "Bank", "Charge", "Vent", "Telegraph", "AttackLine", "AttackCircle", "AttackCone" })
                {
                    if (!animator.HasState(0, Animator.StringToHash(state))) continue;
                    Pose(go, state, .5f); Capture(cam, go, Actors[index] + "_" + state + ".png", 540, 960, 0);
                }
                Pose(go, "Idle", 0); group.ForceLOD(-1);
                var diagnosticPosition = cam.transform.position; var diagnosticRotation = cam.transform.rotation;
                var diagnosticSpan = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                cam.transform.position = bounds.center + new Vector3(1, .7f, -1.5f).normalized * diagnosticSpan * 2.1f;
                cam.transform.LookAt(bounds.center); cam.fieldOfView = 40;
                g0.SetActive(false); Capture(cam, go, Actors[index] + "_in_environment.png", 1080, 1080, 0); g0.SetActive(true);
                cam.transform.position = diagnosticPosition; cam.transform.rotation = diagnosticRotation; cam.fieldOfView = settings.FieldOfView;
            }
            // Family and hierarchy captures use a wider diagnostic camera, explicitly labeled.
            for (var i = 0; i < instances.Length; i++)
            {
                instances[i].SetActive(true); Pose(instances[i], "Idle", 0);
                instances[i].transform.position = i < 5 ? new Vector3((i - 2) * 2.4f, 0, -1.7f) : new Vector3((i - 5.5f) * 5, 0, 3);
            }
            g0.transform.position = new Vector3(-5.8f, 0, 3); cam.transform.position = settings.Offset * 1.6f; cam.transform.LookAt(Vector3.up * settings.LookAtHeight);
            CaptureMany(cam, instances.Append(g0).ToArray(), "Complete_family.png", 1600, 1800);
            var playback = new GameObject("Review controls / no gameplay authority").AddComponent<ActorReviewPlayback>(); playback.Configure(instances, g0, cam, settings.Offset, settings.LookAtHeight);
            // Serialized scene starts with one actor at the real camera; playback
            // exposes the complete family and all animation/LOD controls on device.
            foreach (var go in instances) { go.SetActive(go == instances[0]); go.transform.position = Vector3.zero; }
            g0.transform.position = new Vector3(-2.25f, 0, 0); cam.transform.position = settings.Offset; cam.transform.LookAt(Vector3.up * settings.LookAtHeight);
            EditorSceneManager.SaveScene(scene, ReviewScene); AssetDatabase.SaveAssets();
            if (!canonicalBytes.SequenceEqual(File.ReadAllBytes(ProductionScene))) throw new InvalidOperationException("Production scene changed");
            var isolated = !AssetDatabase.GetDependencies(ProductionScene, true).Any(x => x.StartsWith(Root, StringComparison.Ordinal));
            if (!isolated) throw new InvalidOperationException("Production dependency isolation failed");
            var report = new ValidationReport { engine = Application.unityVersion, actors = reports.ToArray(), productionDependencyIsolation = isolated, cameraFov = settings.FieldOfView, cameraLookAt = settings.LookAtHeight, cameraOffset = settings.Offset, g0RenderDimensions = playerBounds.size, g0RootScale = g0.transform.localScale };
            File.WriteAllText(Evidence + "/UNITY_VALIDATION.json", JsonUtility.ToJson(report, true));
            ValidateAssets(); Debug.Log("ACTOR_V2_BUILD_CAPTURE_PASS");
        }
        public static GameObject ImportAndMakePrefab(string actor)
        {
            var modelPath = Root + "/Models/" + actor + "_V2.fbx";
            var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            if (importer == null) throw new InvalidOperationException("Missing model " + modelPath);
            importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.importAnimation = true; importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; importer.motionNodeName = "ROOT"; importer.optimizeGameObjects = false;
            importer.importCameras = false; importer.importLights = false; importer.importVisibility = false; importer.addCollider = false; importer.importBlendShapes = false;
            importer.isReadable = true; importer.importNormals = ModelImporterNormals.Import; importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.maxBonesPerVertex = 1; importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.materialImportMode = ModelImporterMaterialImportMode.None; importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.SaveAndReimport();
            var takes = importer.defaultClipAnimations;
            foreach (var take in takes)
            {
                var separator = take.name.LastIndexOf('|'); if (separator < 0) throw new InvalidOperationException("Unexpected take " + take.name);
                take.name = take.name.Substring(separator + 1); if (take.name == "Move") take.name = "Run";
                take.loopTime = take.name == "Idle" || take.name == "Run" || take.name == "Hover"; take.loopPose = false;
                take.keepOriginalOrientation = true; take.keepOriginalPositionXZ = true; take.keepOriginalPositionY = true;
                take.lockRootRotation = true; take.lockRootPositionXZ = true; take.lockRootHeightY = true;
            }
            importer.clipAnimations = takes; importer.SaveAndReimport();
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(x => !x.name.StartsWith("__preview__")).ToArray();
            foreach (var state in RequiredStates) if (!clips.Any(x => x.name == state)) throw new InvalidOperationException(actor + " lacks " + state);
            var path = Root + "/" + actor + "_V2.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path) ?? AnimatorController.CreateAnimatorControllerAtPath(path);
            var machine = controller.layers[0].stateMachine; foreach (var state in machine.states) machine.RemoveState(state.state);
            foreach (var clip in clips)
            {
                var state = machine.AddState(clip.name); state.motion = clip; state.writeDefaultValues = false;
                if (clip.name == "Idle") machine.defaultState = state;
            }
            // Existing presentation bridge state contracts; clips never apply damage.
            if (actor == "Custodian")
            {
                AddAlias(machine, "Windup", clips.Single(x => x.name == "Telegraph")); AddAlias(machine, "Release", clips.Single(x => x.name == "Attack")); AddAlias(machine, "Special", clips.Single(x => x.name == "AttackCircle"));
            }
            if (actor == "Magnetar") AddAlias(machine, "Windup", clips.Single(x => x.name == "Charge"));
            var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath)); model.name = actor + "_V2_Review";
            var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>(); animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var material = MakeMaterial(actor == "ArcDrone" ? "Blue" : actor == "Warden" || actor == "Carrier" || actor == "Magnetar" ? "Amber" : "Red");
            var skins = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(x => x.name).ToArray();
            if (skins.Length != 3) throw new InvalidOperationException(actor + " requires exactly three skins");
            foreach (var skin in skins) { skin.sharedMaterials = new[] { material }; skin.updateWhenOffscreen = false; skin.shadowCastingMode = ShadowCastingMode.On; }
            var lod = model.GetComponent<LODGroup>() ?? model.AddComponent<LODGroup>();
            lod.SetLODs(new[] { new LOD(.14f, new Renderer[] { skins[0] }), new LOD(.075f, new Renderer[] { skins[1] }), new LOD(.025f, new Renderer[] { skins[2] }) });
            lod.fadeMode = LODFadeMode.None; lod.RecalculateBounds();
            var saved = PrefabUtility.SaveAsPrefabAsset(model, Root + "/Prefabs/" + actor + "_V2_Review.prefab"); Object.DestroyImmediate(model); return saved;
        }
        private static void AddAlias(AnimatorStateMachine machine, string name, AnimationClip clip)
        { var state = machine.AddState(name); state.motion = clip; state.writeDefaultValues = false; }
        private static Material MakeMaterial(string color)
        {
            foreach (var channel in new[] { "BaseColor", "MetallicSmoothness", "Normal", "Occlusion", "Emission" + color })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(Root + "/Textures/HostileV2_" + channel + ".png");
                importer.sRGBTexture = channel == "BaseColor" || channel.StartsWith("Emission");
                importer.textureType = channel == "Normal" ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = false; importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp; importer.maxTextureSize = 1024; importer.textureCompression = TextureImporterCompression.Compressed;
                var android = importer.GetPlatformTextureSettings("Android"); android.overridden = true; android.maxTextureSize = 1024; android.format = TextureImporterFormat.ASTC_6x6; importer.SetPlatformTextureSettings(android); importer.SaveAndReimport();
            }
            var path = Root + "/Materials/HostileV2_" + color + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path); if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            Texture2D Tex(string channel) => AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/HostileV2_" + channel + ".png");
            mat.SetTexture("_BaseMap", Tex("BaseColor")); mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_MetallicGlossMap", Tex("MetallicSmoothness")); mat.EnableKeyword("_METALLICSPECGLOSSMAP"); mat.SetFloat("_Smoothness", 1); mat.SetFloat("_SmoothnessTextureChannel", 0);
            mat.SetTexture("_BumpMap", Tex("Normal")); mat.SetFloat("_BumpScale", .3f); mat.EnableKeyword("_NORMALMAP");
            mat.SetTexture("_OcclusionMap", Tex("Occlusion")); mat.SetFloat("_OcclusionStrength", .8f); mat.EnableKeyword("_OCCLUSIONMAP");
            mat.SetTexture("_EmissionMap", Tex("Emission" + color)); mat.SetColor("_EmissionColor", Color.white * 1.5f); mat.EnableKeyword("_EMISSION");
            mat.SetFloat("_Surface", 0); mat.SetFloat("_Cull", 2); EditorUtility.SetDirty(mat); return mat;
        }
        private static GameObject Spawn(string path, Transform parent)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (asset == null) throw new InvalidOperationException("Missing stage reference " + path);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset); if (parent != null) go.transform.SetParent(parent, true); return go;
        }
        public static void Pose(GameObject model, string state, float normalized)
        {
            var animator = model.GetComponentInChildren<Animator>(true); animator.enabled = true; animator.Rebind(); animator.Update(0);
            animator.Play(state, 0, normalized); animator.Update(0);
        }
        private static void Capture(Camera cam, GameObject go, string name, int width, int height, int lod)
        { CaptureMany(cam, new[] { go }, name, width, height, lod); }
        private static int CameraLod(LODGroup group, Camera cam)
        {
            if (group == null) return 0;
            var scale = group.transform.lossyScale;
            var size = group.size * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            var distance = Vector3.Distance(cam.transform.position, group.transform.TransformPoint(group.localReferencePoint));
            var coverage = size / (2 * Mathf.Max(.001f, distance) * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * .5f));
            coverage *= QualitySettings.lodBias;
            var levels = group.GetLODs();
            for (var i = 0; i < levels.Length; i++) if (coverage >= levels[i].screenRelativeTransitionHeight) return i;
            return -1;
        }
        private static void CaptureMany(Camera cam, GameObject[] actors, string name, int width, int height, int lod = 0)
        {
            var temporary = new List<GameObject>(); var meshes = new List<Mesh>(); var disabled = new List<(Renderer renderer, bool enabled)>(); var groups = new List<(LODGroup group, bool enabled)>();
            foreach (var actor in actors)
            {
                // G-0 keeps its production hierarchy/fit and normal skin path.
                // Baking beneath its scaled child can apply the fit twice in the
                // batch editor. Never substitute a shrunken player reference.
                if (actor.name.StartsWith("G-0", StringComparison.Ordinal)) continue;
                var group = actor.GetComponent<LODGroup>(); if (group != null) { groups.Add((group, group.enabled)); group.enabled = false; }
                var skins = actor.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(x => x.name).ToArray();
                if (skins.Length == 0) continue;
                foreach (var skin in skins) { disabled.Add((skin, skin.enabled)); skin.enabled = false; }
                var selected = lod < 0 ? CameraLod(group, cam) : Mathf.Min(lod, skins.Length - 1);
                if (selected < 0) continue;
                var source = skins[selected]; var mesh = new Mesh(); source.BakeMesh(mesh, false); meshes.Add(mesh);
                var posed = new GameObject("TEMP actual imported pose", typeof(MeshFilter), typeof(MeshRenderer)); posed.transform.SetParent(source.transform, false);
                posed.GetComponent<MeshFilter>().sharedMesh = mesh; posed.GetComponent<MeshRenderer>().sharedMaterials = source.sharedMaterials; temporary.Add(posed);
                var posedBounds = posed.GetComponent<MeshRenderer>().bounds;
                if (posedBounds.size.y < .15f || posedBounds.size.y > 6f || Vector3.Distance(posedBounds.center, actor.transform.position) > 5f)
                    throw new InvalidOperationException("Captured pose has invalid scale/location: " + actor.name + " " + posedBounds);
            }
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32); var prior = RenderTexture.active; var priorAspect = cam.aspect;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                cam.targetTexture = rt; cam.aspect = (float)width / height; for (var i = 0; i < 3; i++) cam.Render(); RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply(); File.WriteAllBytes(Evidence + "/unity/" + name, texture.EncodeToPNG());
            }
            finally
            {
                cam.targetTexture = null; cam.aspect = priorAspect; RenderTexture.active = prior; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(texture);
                foreach (var obj in temporary) Object.DestroyImmediate(obj); foreach (var mesh in meshes) Object.DestroyImmediate(mesh);
                foreach (var pair in disabled) pair.renderer.enabled = pair.enabled; foreach (var pair in groups) pair.group.enabled = pair.enabled;
            }
        }
        public static void ValidateAssets()
        {
            foreach (var actor in Actors)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/" + actor + "_V2_Review.prefab");
                if (prefab == null) throw new InvalidOperationException("Missing review prefab " + actor);
                var animator = prefab.GetComponent<Animator>(); var lods = prefab.GetComponent<LODGroup>().GetLODs();
                if (animator == null || animator.applyRootMotion || lods.Length != 3 || prefab.GetComponentsInChildren<Collider>(true).Length != 0) throw new InvalidOperationException(actor + " invalid review contract");
                foreach (var state in RequiredStates) if (!animator.runtimeAnimatorController.animationClips.Any(x => x.name == state)) throw new InvalidOperationException(actor + " missing state " + state);
                var names = prefab.GetComponentsInChildren<Transform>(true).Select(x => x.name).ToArray();
                foreach (var socket in new[] { "ROOT", "VisualCenter", "HitCenter", "GroundContact", "DamageNumberAnchor", "HealthBarAnchor", "TelegraphOrigin" }) if (!names.Contains(socket)) throw new InvalidOperationException(actor + " missing socket " + socket);
                if (!names.Any(x => x.StartsWith("AttackOrigin"))) throw new InvalidOperationException(actor + " missing attack origin");
                for (var i = 0; i < 3; i++) if (lods[i].renderers.Length != 1 || lods[i].renderers[0].sharedMaterials.Length != 1) throw new InvalidOperationException(actor + " unconsolidated LOD");
            }
            if (AssetDatabase.GetDependencies(ProductionScene, true).Any(x => x.StartsWith(Root, StringComparison.Ordinal))) throw new InvalidOperationException("Review leaked into production");
            Debug.Log("ACTOR_V2_ASSET_VALIDATION_PASS");
        }
        public static void ValidateProject()
        {
            ValidateAssets(); ProjectValidator.ValidateOrThrow();
            File.WriteAllText(Evidence + "/PROJECT_VALIDATION.txt", "Executed ProjectValidator.ValidateOrThrow and actor asset contract checks successfully.\n");
        }
        public static void BuildReviewAndroid()
        {
            ValidateAssets();
            var oldId = PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android); var oldVersion = PlayerSettings.bundleVersion; var oldCode = PlayerSettings.Android.bundleVersionCode;
            var oldPipeline = GraphicsSettings.defaultRenderPipeline; var oldQualityPipeline = QualitySettings.renderPipeline;
            var oldBackend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android); var oldArch = PlayerSettings.Android.targetArchitectures; var oldOrientation = PlayerSettings.defaultInterfaceOrientation;
            try
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android)) throw new InvalidOperationException("Android Build Support unavailable");
                PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.gravivore.actorreview"); PlayerSettings.bundleVersion = "0.1.0"; PlayerSettings.Android.bundleVersionCode = 1;
                PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP); PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpConfigurator.UrpAssetPath);
                if (pipeline == null) throw new InvalidOperationException("Canonical URP assets are missing; configure project first");
                GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
                Directory.CreateDirectory("Builds/Android");
                // Existing global build audits require canonical production
                // dependencies in the APK. Retain both unchanged scenes after
                // the isolated review entry point; no new actors are bound there.
                // Legacy APK packing audits inspect level1 explicitly. Keep
                // unchanged Chapter01 at index 1, review at 0, Bootstrap at 2.
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ReviewScene, ProductionScene, "Assets/_Game/Content/Scenes/Bootstrap.unity" }, locationPathName = "Builds/Android/gravivore-actor-review-v2.apk", target = BuildTarget.Android, options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.DetailedBuildReport });
                var packed = report.packedAssets.SelectMany(x => x.contents).Select(x => x.sourceAssetPath).Distinct().OrderBy(x => x).ToArray();
                var allPacked = Actors.All(actor => packed.Contains(Root + "/Models/" + actor + "_V2.fbx"));
                File.WriteAllText(Evidence + "/ANDROID_REVIEW_BUILD.json", JsonUtility.ToJson(new BuildRecord { result = report.summary.result.ToString(), errors = (int)report.summary.totalErrors, bytes = report.summary.totalSize, apk = "Builds/Android/gravivore-actor-review-v2.apk", packageId = "com.gravivore.actorreview", containsAllSeven = allPacked, productionIntegration = false, packedActorAssets = packed.Where(x => x.StartsWith(Root, StringComparison.Ordinal)).ToArray() }, true));
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Actor review APK failed: " + report.summary.result);
                if (!allPacked) throw new InvalidOperationException("Review APK omitted one or more of the seven actor models");
            }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, oldId); PlayerSettings.bundleVersion = oldVersion; PlayerSettings.Android.bundleVersionCode = oldCode;
                PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, oldBackend); PlayerSettings.Android.targetArchitectures = oldArch; PlayerSettings.defaultInterfaceOrientation = oldOrientation;
                GraphicsSettings.defaultRenderPipeline = oldPipeline; QualitySettings.renderPipeline = oldQualityPipeline; AssetDatabase.SaveAssets();
            }
        }
        [Serializable] private sealed class BuildRecord
        { public string result, apk, packageId; public int errors; public ulong bytes; public bool containsAllSeven, productionIntegration; public string[] packedActorAssets; }
    }
}
