using System;
using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Phase3DReviewCaptureTests
    {
        private CanonicalSceneTestScope _scene;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (_scene != null) yield return _scene.Cleanup();
            _scene = null;
        }

        [UnityTest]
        public IEnumerator RealGameplayCameraProducesRequiredReviewSetWhenOptedIn()
        {
            _scene = new CanonicalSceneTestScope();
            yield return _scene.Load();
            var output = Environment.GetEnvironmentVariable("GRAVIVORE_PHASE3D_QA");
            if (string.IsNullOrEmpty(output)) yield break;
            Directory.CreateDirectory(output);

            var root = _scene.Root;
            root.EnemyPopulation.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            root.MagnetarGuard.enabled = false;
            root.CustodianBoss.enabled = false;

            var camera = Camera.main;
            Assert.IsNotNull(camera);
            camera.GetComponent<PortraitFollowCamera>().enabled = false;
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<CameraFollowSettings>(
                "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            Assert.That(settings.Offset, Is.EqualTo(new Vector3(0f, 14.8f, -11.2f)));
            Assert.That(settings.FieldOfView, Is.EqualTo(46f));

            CapturePair(camera, settings, output, "01_RepairHub_BeforeAfter",
                root.VisualEnvironment.GetRegion("repair-hub").Root.position,
                "01_RepairHub.png");

            var carrier = root.GetComponentsInChildren<OrdinaryEnemyController>()
                .First(e => e.GetComponent<CharacterVisualBinding>()?.ActiveModel?.name == "Carrier_Phase3D");
            CapturePair(camera, settings, output, "02_Carrier_BeforeAfter",
                carrier.transform.position, "13_AllEnemyFamilies_Review.png");

            CapturePair(camera, settings, output, "03_CapacitorField_BeforeAfter",
                root.VisualEnvironment.GetRegion("capacitor-field").Root.position + Vector3.forward * 4.5f,
                "02_CapacitorField.png");
            CapturePair(camera, settings, output, "04_ShieldDump_BeforeAfter",
                root.VisualEnvironment.GetRegion("shield-dump").Root.position + Vector3.forward * 3.5f,
                "05_ShieldDump.png");
            CapturePair(camera, settings, output, "05_HaulerGraveyard_BeforeAfter",
                root.VisualEnvironment.GetRegion("hauler-graveyard").Root.position + Vector3.forward * 3.5f,
                "03_HaulerGraveyard.png");
            CapturePair(camera, settings, output, "06_EliteArena_BeforeAfter",
                root.VisualEnvironment.GetRegion("elite-arena").Root.position,
                "08_MagnetarGuard.png");

            Capture(camera, settings, output, "07_Magnetar_NewWorld", root.MagnetarGuard.transform.position);
            Capture(camera, settings, output, "08_Custodian_NewWorld", root.CustodianBoss.transform.position);

            var actors = root.GetComponentsInChildren<OrdinaryEnemyController>()
                .Where(e => e.GetComponent<CharacterVisualBinding>()?.ActiveModel != null)
                .GroupBy(e => e.GetComponent<CharacterVisualBinding>().ActiveModel.name)
                .Select(g => g.First()).Take(5).ToArray();
            var saved = actors.Select(a => a.transform.position).ToArray();
            var familyCenter = new Vector3(-20f, 0f, -15f);
            for (var i = 0; i < actors.Length; i++)
                actors[i].transform.position = familyCenter + new Vector3((i % 3 - 1) * 2.2f, 0f, 2.2f + i / 3 * 2.3f);
            Physics.SyncTransforms();
            Capture(camera, settings, output, "09_OrdinaryFamilies_NewWorld", familyCenter);
            for (var i = 0; i < actors.Length; i++) actors[i].transform.position = saved[i];

            Capture(camera, settings, output, "10_Traversal_NewWorld",
                root.WorldPresenter.Bounds.Center);
            Capture(camera, settings, output, "11_G0_In_NewWorld", root.PlayerObject.transform.position);
            Capture(camera, settings, output, "12_IndustrialGate", root.WorldPresenter.EliteGate.transform.position);

            Assert.That(Directory.GetFiles(output, "*.png"), Has.Length.GreaterThanOrEqualTo(12));
        }

        private static void CapturePair(
            Camera camera,
            CameraFollowSettings settings,
            string output,
            string name,
            Vector3 target,
            string baselineFile)
        {
            var after = Render(camera, settings, target);
            // A freshly captured Phase 3C directory permits matching production-camera
            // comparisons; committed historical shots remain the default reference.
            var baselineDirectory = Environment.GetEnvironmentVariable("GRAVIVORE_PHASE3D_BASELINE_QA");
            if (string.IsNullOrEmpty(baselineDirectory))
                baselineDirectory = Path.Combine(Application.dataPath, "../docs/phase3c/screenshots/");
            var baselinePath = Path.GetFullPath(Path.Combine(baselineDirectory, baselineFile));
            Assert.IsTrue(File.Exists(baselinePath), baselinePath);
            var before = new Texture2D(2, 2, TextureFormat.RGB24, false);
            Assert.IsTrue(before.LoadImage(File.ReadAllBytes(baselinePath), false));

            var combined = new Texture2D(before.width + after.width,
                Mathf.Max(before.height, after.height), TextureFormat.RGB24, false);
            try
            {
                var fill = Enumerable.Repeat(Color.black, combined.width * combined.height).ToArray();
                combined.SetPixels(fill);
                combined.SetPixels(0, 0, before.width, before.height, before.GetPixels());
                combined.SetPixels(before.width, 0, after.width, after.height, after.GetPixels());
                combined.Apply(false);
                File.WriteAllBytes(Path.Combine(output, name + ".png"), combined.EncodeToPNG());
            }
            finally
            {
                Object.Destroy(before);
                Object.Destroy(after);
                Object.Destroy(combined);
            }
        }

        private static void Capture(
            Camera camera,
            CameraFollowSettings settings,
            string output,
            string name,
            Vector3 target)
        {
            var image = Render(camera, settings, target);
            try { File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG()); }
            finally { Object.Destroy(image); }
        }

        private static Texture2D Render(Camera camera, CameraFollowSettings settings, Vector3 target)
        {
            camera.transform.position = target + settings.Offset;
            camera.transform.LookAt(target + Vector3.up * settings.LookAtHeight);
            camera.fieldOfView = settings.FieldOfView;
            Physics.SyncTransforms();

            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var targetTexture = new RenderTexture(540, 960, 24);
            var pixels = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = targetTexture;
                camera.Render();
                RenderTexture.active = targetTexture;
                pixels.ReadPixels(new Rect(0, 0, 540, 960), 0, 0);
                pixels.Apply(false);
                return pixels;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                targetTexture.Release();
                Object.Destroy(targetTexture);
            }
        }
    }
}
