using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CameraFollowSettings = Gravivore.Presentation.Camera.CameraFollowSettings;

namespace Gravivore.ArtSpike.Editor
{
    public static class ArtSpikeScene
    {
        public static readonly Vector3 AreaB = new Vector3(22, 0, 0);
        public static readonly Vector3 AreaC = new Vector3(22, 0, 22);
        public static readonly Vector3 AreaD = new Vector3(0, 0, 22);
        public const string SettingsPath = "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset";

        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.42f, .47f, .52f);
            RenderSettings.fog = false;
            var light = new GameObject("ArtReview_KeyLight_Only").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.2f;
            light.color = new Color(.90f, .95f, 1f);
            light.transform.rotation = Quaternion.Euler(48, -32, 0);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .75f;
            RenderSettings.sun = light;

            var profilePath = ArtSpikeBuilder.Root + "/Materials/ArtSpike_PostProcessing.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
                var bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(.16f);
                bloom.threshold.Override(1f);
                var tone = profile.Add<Tonemapping>(true);
                tone.mode.Override(TonemappingMode.Neutral);
                foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
            }
            var volume = new GameObject("ArtReview_ControlledCoreBloom").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            var composition = new ArtSpikeComposition();
            composition.CreateMaterials();
            Stage("AREA A - Player Evolution", Vector3.zero, composition, 24);
            for (var tier = 0; tier < 3; tier++)
                Place(ArtSpikeBuilder.CharacterPaths[tier], new Vector3((tier - 1) * 3.25f, 0, 0),
                    "A_G0_Tier" + tier, 24);
            ReviewCamera("Camera_A_Evolution_CloseReview", Vector3.zero, new Vector3(0, 7.4f, -5.48f), 16f / 9f).cullingMask = 1 << 24;

            Stage("AREA B - S20 Gameplay Scale", AreaB, composition, 25);
            var playerB = Place(ArtSpikeBuilder.CharacterPaths[1], AreaB + new Vector3(-.7f, 0, 0), "B_G0_Tier1", 25);
            Place(ArtSpikeBuilder.CharacterPaths[3], AreaB + new Vector3(1.3f, 0, 1f), "B_Cutter", 25).transform.rotation = Quaternion.Euler(0, 190, 0);
            GameplayCamera("Camera_B_S20", playerB.transform.position).cullingMask = 1 << 25;

            var environment = (GameObject)PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.Root + "/Prefabs/Environment/IndustrialBay_ArtSpike.prefab"));
            environment.name = "AREA C - Industrial Environment";
            environment.transform.position = AreaC;
            SetLayer(environment, 26);
            var playerC = Place(ArtSpikeBuilder.CharacterPaths[2], AreaC + new Vector3(-.4f, 0, -.8f), "C_G0_Tier2", 26);
            Place(ArtSpikeBuilder.CharacterPaths[3], AreaC + new Vector3(1.4f, 0, 1.4f), "C_Cutter", 26).transform.rotation = Quaternion.Euler(0, 205, 0);
            var mainCamera = GameplayCamera("Camera_C_S20", playerC.transform.position);
            mainCamera.cullingMask = 1 << 26;
            mainCamera.enabled = true;
            ReviewCamera("Camera_C_EnvironmentOverview", AreaC, new Vector3(10, 16, -13), 16f / 9f).cullingMask = 1 << 26;

            Stage("AREA D - Intended Scale Reference", AreaD, composition, 27);
            Place(ArtSpikeBuilder.CharacterPaths[2], AreaD + new Vector3(-1.5f, 0, 0), "D_G0_Tier2", 27);
            Place(ArtSpikeBuilder.CharacterPaths[3], AreaD + new Vector3(1.5f, 0, 0), "D_Cutter", 27);
            ReviewCamera("Camera_D_ScaleReference", AreaD, new Vector3(0, 7.4f, -5.48f), 16f / 9f).cullingMask = 1 << 27;

            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, ArtSpikeBuilder.ScenePath))
                throw new InvalidOperationException("Could not save comparison scene.");
        }

        private static void Stage(string name, Vector3 position, ArtSpikeComposition composition, int layer)
        {
            var root = composition.ReviewStage(name);
            root.transform.position = position;
            SetLayer(root, layer);
        }

        internal static GameObject Place(string path, Vector3 position, string name, int layer)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Missing review prefab: " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = name;
            instance.transform.position = position;
            SetLayer(instance, layer);
            return instance;
        }

        private static void SetLayer(GameObject root, int layer)
        {
            // Numeric review-only layers need no edits to the production TagManager.
            foreach (var child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer;
        }

        public static Camera GameplayCamera(string name, Vector3 playerPosition)
        {
            var settings = AssetDatabase.LoadAssetAtPath<CameraFollowSettings>(SettingsPath);
            if (settings == null) throw new InvalidOperationException("S20 camera settings unavailable.");
            // Copy a settled frame from PortraitFollowCamera.SnapToTarget, reading the asset without editing it.
            var camera = ReviewCamera(name, playerPosition, settings.Offset, 9f / 16f);
            camera.fieldOfView = settings.FieldOfView;
            camera.transform.rotation = Quaternion.LookRotation(
                playerPosition + Vector3.up * settings.LookAtHeight - camera.transform.position, Vector3.up);
            return camera;
        }

        private static Camera ReviewCamera(string name, Vector3 target, Vector3 offset, float aspect)
        {
            var camera = new GameObject(name, typeof(Camera), typeof(UniversalAdditionalCameraData)).GetComponent<Camera>();
            camera.enabled = false;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 100f;
            camera.fieldOfView = 46;
            camera.aspect = aspect;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.025f, .035f, .047f);
            camera.allowHDR = true;
            camera.transform.position = target + offset;
            camera.transform.LookAt(target + Vector3.up * .45f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            return camera;
        }
    }
}
