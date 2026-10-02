using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    public static class ArtSpikeCapture
    {
        [MenuItem("Gravivore/Art Spike/Render Review Images")]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(ArtSpikeBuilder.ScenePath, OpenSceneMode.Single);
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            Camera Camera(string name) => roots.Single(r => r.name == name).GetComponent<Camera>();
            Directory.CreateDirectory("docs/art-spike/images");
            Render(Camera("Camera_A_Evolution_CloseReview"), "01_G0_Evolution.png", 1920, 1080);
            var enemy = roots.Single(r => r.name == "B_Cutter");
            enemy.SetActive(false);
            try { Render(Camera("Camera_B_S20"), "02_G0_GameplayScale.png", 1080, 1920); }
            finally { enemy.SetActive(true); }
            Render(Camera("Camera_B_S20"), "03_Enemy_GameplayScale.png", 1080, 1920);
            Render(Camera("Camera_C_EnvironmentOverview"), "04_Environment_Overview.png", 1920, 1080);
            Render(Camera("Camera_C_S20"), "05_Gameplay_Mock.png", 1080, 1920);
            Render(Camera("Camera_D_ScaleReference"), "06_ScaleReference.png", 1920, 1080);
            // A supplementary settled-camera comparison of every tier; each panel is an actual camera render.
            var triptych = new Texture2D(3240, 1920, TextureFormat.RGB24, false);
            var instances = roots.Where(r => r.name.StartsWith("A_G0_", StringComparison.Ordinal)).ToArray();
            try
            {
                for (var tier = 0; tier < 3; tier++)
                {
                    var instance = instances.Single(r => r.name == "A_G0_Tier" + tier);
                    foreach (var other in instances) other.SetActive(other == instance);
                    var camera = ArtSpikeScene.GameplayCamera("Capture_Temporary", instance.transform.position);
                    camera.cullingMask = 1 << 24;
                    try
                    {
                        var panel = ReadCamera(camera, 1080, 1920);
                        try { triptych.SetPixels(tier * 1080, 0, 1080, 1920, panel.GetPixels()); }
                        finally { Object.DestroyImmediate(panel); }
                    }
                    finally { Object.DestroyImmediate(camera.gameObject); }
                }
                triptych.Apply();
                File.WriteAllBytes("docs/art-spike/images/07_Evolution_S20Scale.png", triptych.EncodeToPNG());
            }
            finally
            {
                foreach (var instance in instances) instance.SetActive(true);
                Object.DestroyImmediate(triptych);
            }
            // Restore the saved authoring scene; capture-only visibility never persists.
            EditorSceneManager.OpenScene(ArtSpikeBuilder.ScenePath, OpenSceneMode.Single);
            Debug.Log("ART SPIKE actual Unity renders completed on " + SystemInfo.graphicsDeviceType + ": " + SystemInfo.graphicsDeviceName);
        }

        public static void BuildAndCapture()
        {
            ArtSpikeBuilder.Build();
            Capture();
        }

        private static void Render(Camera camera, string fileName, int width, int height)
        {
            var texture = ReadCamera(camera, width, height);
            try { File.WriteAllBytes("docs/art-spike/images/" + fileName, texture.EncodeToPNG()); }
            finally { Object.DestroyImmediate(texture); }
        }

        private static Texture2D ReadCamera(Camera camera, int width, int height)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("Image capture requires a graphics device. Omit -nographics.");
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            var previousActive = RenderTexture.active;
            var previousAspect = camera.aspect;
            Texture2D texture = null;
            try
            {
                target.Create();
                camera.aspect = (float)width / height;
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                // This initializes the active URP as needed; every delivered pixel comes from this scene.
                RenderPipeline.SubmitRenderRequest(camera, request);
                // URP uploads shared material constants on the first request after an editor reload.
                // Warm the scene before readback so the first image has the same material state as later ones.
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = target;
                texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                return texture;
            }
            catch
            {
                if (texture != null) Object.DestroyImmediate(texture);
                throw;
            }
            finally
            {
                camera.aspect = previousAspect;
                RenderTexture.active = previousActive;
                target.Release();
                Object.DestroyImmediate(target);
            }
        }
    }
}
