using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Player;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class VisualIntegrationSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        private readonly List<Object> _owned = new List<Object>();
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (_scene != null) yield return _scene.Cleanup(); _scene = null;
            foreach (var obj in _owned) if (obj != null) Object.Destroy(obj);
            _owned.Clear(); yield return null;
        }
        private IEnumerator Load(Action<Gravivore.Presentation.Composition.S01SceneCompositionRoot> configure = null)
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load(configure);
            _scene.Root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            _scene.Root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            _scene.Root.EnemyPopulation.enabled = false;
            foreach (var enemy in _scene.Root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            _scene.Root.MagnetarGuard.enabled = false; _scene.Root.CustodianBoss.enabled = false;
        }
        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field,BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target,value);
        private static T Get<T>(object target, string field) =>
            (T)target.GetType().GetField(field,BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);

        [UnityTest] public IEnumerator AuthoredRegionsHubAndVisualEnvironmentContainNoCollisionOrGameplayState()
        {
            yield return Load(); var root = _scene.Root; var environment = root.VisualEnvironment;
            Assert.DoesNotThrow(environment.ValidateOrThrow); Assert.That(environment.RegionCount,Is.EqualTo(11));
            Assert.That(environment.GetComponentsInChildren<Collider>(true),Is.Empty);
            Assert.That(environment.GetComponentsInChildren<MonoBehaviour>(true),Has.Length.EqualTo(1));
            Assert.That(environment.GetComponentsInChildren<Light>(true),Has.Length.EqualTo(1));
            Assert.That(environment.KeyLight.shadows,Is.EqualTo(LightShadows.None));
            var hub = environment.GetRegion("repair-hub").Root;
            Assert.That(hub.Find("PlayerDockPoint").position,Is.EqualTo(root.PlayerObject.transform.position));
            Assert.That(hub.GetComponentsInChildren<MonoBehaviour>(true),Is.Empty);
            foreach (var spot in Get<SpawnSpotDefinition[]>(root,"_spawnSpotDefinitions"))
            {
                var config = spot.CreateRuntimeConfiguration(); var region = environment.GetRegion(config.Id);
                Assert.That(region.Root.position,Is.EqualTo(config.WorldOrigin));
                Assert.That(region.EnemyId,Is.EqualTo(config.Enemy.Id)); Assert.IsNotNull(region.Landmark);
                foreach (var anchor in config.AnchorOffsets)
                    Assert.IsTrue(root.WorldPresenter.Bounds.Contains(config.WorldOrigin + anchor));
            }
            foreach (var collider in root.WorldPresenter.GetComponentsInChildren<Collider>(true))
                Assert.IsTrue(collider.transform.IsChildOf(root.WorldPresenter.GameplayRoot));
            var health = root.PlayerHealth.CurrentHitPoints;
            var eliteLocked = root.WorldPresenter.EliteGate.IsLocked;
            environment.gameObject.SetActive(false); yield return null;
            Assert.That(root.PlayerHealth.CurrentHitPoints,Is.EqualTo(health));
            Assert.That(root.WorldPresenter.EliteGate.IsLocked,Is.EqualTo(eliteLocked));
            Assert.IsTrue(root.WorldPresenter.EliteGate.BlockingCollider.enabled);
            Assert.That(root.WorldPresenter.PerimeterColliderCount,Is.EqualTo(4));
        }

        [UnityTest] public IEnumerator FullEliteBossAndPlayerOverridesKeepAuthorityDimensionsTargetsAndPrototypeTiming()
        {
            var model = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.Destroy(model.GetComponent<Collider>());
            Object.DontDestroyOnLoad(model);
            model.name = "Injected pure candidate"; _owned.Add(model); yield return null;
            var container = new GameObject("Presentation Sockets").transform; container.SetParent(model.transform,false);
            var origin = new GameObject("AttackOrigin").transform; origin.SetParent(container,false); origin.localPosition = new Vector3(0,1,.7f);
            var binding = new PresentationModelBinding(); Set(binding,"_prefab",model);
            Set(binding,"_localPosition",new Vector3(1,.3f,-.5f)); Set(binding,"_localEulerAngles",new Vector3(0,25,0));
            Set(binding,"_localScale",new Vector3(3,4,5));
            yield return Load(root =>
            {
                var definition = Object.Instantiate(Get<ChapterVisualIntegrationDefinition>(root.VisualEnvironment,"_definition")); _owned.Add(definition);
                Set(definition,"_elite",binding); Set(definition,"_boss",binding);
                Set(root.VisualEnvironment,"_definition",definition);
                var evolution = Object.Instantiate(Get<EvolutionDefinition>(root,"_evolutionDefinition")); _owned.Add(evolution);
                Set(evolution,"_tierOverrides",new[] { binding,binding,binding }); Set(root,"_evolutionDefinition",evolution);
            });
            var actual = _scene.Root;
            foreach (var authority in new[] { actual.MagnetarGuard.transform,actual.CustodianBoss.transform })
            {
                var controller = authority.GetComponent<CharacterController>();
                var visual = authority.GetComponent<CharacterVisualBinding>();
                Assert.That(visual.ActiveModel.localScale,Is.EqualTo(binding.LocalScale));
                Assert.That(visual.ActiveModel.localPosition,Is.EqualTo(binding.LocalPosition));
                Assert.That(visual.VisualRoot.GetComponentsInChildren<Collider>(true),Is.Empty);
                Assert.That(authority.localScale,Is.EqualTo(Vector3.one));
                var radius = authority == actual.MagnetarGuard.transform ? actual.MagnetarGuard.CollisionRadius : actual.CustodianBoss.CollisionRadius;
                Assert.That(controller.radius,Is.EqualTo(radius));
                var target = authority == actual.MagnetarGuard.transform ? actual.MagnetarGuard.TargetPoint : actual.CustodianBoss.TargetPoint;
                Assert.That(controller.height,Is.EqualTo(Mathf.Max(target.localPosition.y * 1.8f,radius * 2f)).Within(.001f));
            }
            var player = actual.PlayerObject.transform;
            Assert.That(player.GetComponent<CharacterController>().radius,Is.EqualTo(.42f));
            Assert.That(player.GetComponent<CharacterController>().height,Is.EqualTo(1.4f));
            var motion = player.GetComponent<MechMotionPresenter>(); motion.enabled = false;
            var view = player.GetComponent<PlayerEvolutionView>();
            foreach (var tier in new[] { EvolutionTier.Tier0,EvolutionTier.Tier1,EvolutionTier.Tier2 })
            {
                view.Apply(new EvolutionVisualState(tier,view.CurrentDominantStat));
                var before = player.position; motion.Tick(.016f);
                Assert.That(player.position,Is.EqualTo(before)); Assert.IsNotNull(player.GetComponent<CharacterVisualBinding>().ActiveModel);
                Assert.That(motion.PresentationSocket.position,Is.EqualTo(view.GetTierForm(tier).Find("Presentation Sockets/AttackOrigin").position));
            }
            Assert.That(Get<GravityAttackSettings>(actual,"_gravityAttackSettings").TargetScanInterval,
                Is.EqualTo(.1f).Within(.001f));
        }

        [UnityTest] public IEnumerator UnsafeDressingCannotBeInstantiatedOrIntroduceASecondRegenController()
        {
            yield return Load();
            var environment = _scene.Root.VisualEnvironment;
            var unsafeModel = GameObject.CreatePrimitive(PrimitiveType.Cube); _owned.Add(unsafeModel);
            var binding = new PresentationModelBinding(); Set(binding,"_prefab",unsafeModel);
            var entry = new EnvironmentDressingBinding(); Set(entry,"_anchor",environment.GetRegion("repair-hub").Root);
            Set(entry,"_model",binding); Set(environment,"_dressing",new[] { entry });
            var count = _scene.Root.GetComponentsInChildren<PlayerHealthController>(true).Length;
            Assert.Throws<InvalidOperationException>(environment.ValidateOrThrow);
            Assert.That(_scene.Root.GetComponentsInChildren<PlayerHealthController>(true),Has.Length.EqualTo(count));
        }

        [UnityTest] public IEnumerator CaptureStructuralFoundationWhenRequested()
        {
            var directory = Environment.GetEnvironmentVariable("GRAVIVORE_VISUAL_INTEGRATION_QA");
            if (string.IsNullOrEmpty(directory)) Assert.Ignore("Set GRAVIVORE_VISUAL_INTEGRATION_QA to export structural review captures.");
            Directory.CreateDirectory(directory); yield return Load();
            var environment = _scene.Root.VisualEnvironment;
            var camera = UnityEngine.Camera.main; camera.GetComponent<PortraitFollowCamera>().enabled = false;
            // Temporary markers make empty integration anchors visible in review; none are saved into Chapter01.
            for (var i = 0; i < environment.RegionCount; i++)
                Marker(environment.GetRegion(i).Root.position,Color.cyan,1);
            Capture(camera,directory,"01-chapter-roots",new Vector3(76,112,-40),new Vector3(0,0,29),
                "CHAPTER FOUNDATION\nGameplay Geometry: collision / gates\nVisual Environment: art only\n11 cyan region anchors; unchanged layout",900,1000,65);
            var hub = environment.GetRegion("repair-hub").Root;
            foreach (Transform anchor in hub) Marker(anchor.position,Color.yellow,.25f);
            Capture(camera,directory,"02-repair-hub-anchors",hub.position + new Vector3(6,8,-9),hub.position,
                "REPAIR HUB — ANCHORS ONLY\nMainPlatform / PlayerDockPoint at spawn\nLeft / Right / Rear A / Rear B\n4 beam origins / AmbientFxRoot",1000,750,46);
            Capture(camera,directory,"03-five-ordinary-spots",new Vector3(55,75,-35),new Vector3(0,0,18),
                "FIVE ORDINARY REGIONS\nrelay-yard / cutting-floor / shield-dump\ncapacitor-field / hauler-graveyard\nLandmarkAnchor + DressingAnchor per spot",1000,900,60);
            Capture(camera,directory,"04-elite-boss-roots",new Vector3(15,36,58),new Vector3(0,0,80),
                "ELITE / BOSS\nAuthority roots: original capsules / AI\nEncounter Visual Root: fallback / model slot\nApproach + Arena visual regions",1000,900,55);
#if UNITY_EDITOR
            _scene.Root.gameObject.SetActive(false);
            foreach (var obj in _owned) if (obj is GameObject marker) marker.SetActive(false);
            var review = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/_Game/ArtReview/Scenes/VisualIntegration_Review.unity",
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Additive));
            yield return null;
            UnityEngine.Camera overview = null;
            foreach (var root in review.GetRootGameObjects())
                foreach (var candidate in root.GetComponentsInChildren<UnityEngine.Camera>(true))
                    if (candidate.name == "Review Overview") overview = candidate;
            Assert.IsNotNull(overview);
            Capture(overview,directory,"05-isolated-art-review",overview.transform.position,Vector3.up,
                "ISOLATED ART REVIEW — CURRENT FALLBACKS\n1m reference | PlayerTier2 | Cutter | Elite | Boss\nOverview / gameplay camera option / close camera\nExcluded from gameplay Android build scenes",1400,750,46);
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(review);
#endif
        }

        private void Marker(Vector3 position, Color color, float size)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere); _owned.Add(marker);
            Object.Destroy(marker.GetComponent<Collider>()); marker.transform.position = position + Vector3.up * .25f;
            marker.transform.localScale = Vector3.one * size;
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); _owned.Add(material);
            material.color = color; marker.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void Capture(UnityEngine.Camera camera, string directory, string name, Vector3 position,
            Vector3 targetPoint, string caption, int width, int height, float fieldOfView)
        {
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var previousPosition = camera.transform.position; var previousRotation = camera.transform.rotation;
            var previousFov = camera.fieldOfView; var previousMask = camera.cullingMask; var previousFar = camera.farClipPlane;
            var target = new RenderTexture(width,height,24); var pixels = new Texture2D(width,height,TextureFormat.RGB24,false);
            var overlay = new GameObject("Temporary Structural Review Caption",typeof(Canvas));
            var canvas = overlay.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.sortingOrder = 30000;
            overlay.layer = LayerMask.NameToLayer("UI");
            var label = new GameObject("Hierarchy Summary",typeof(RectTransform),typeof(UnityEngine.UI.Text));
            label.transform.SetParent(overlay.transform,false); label.layer = overlay.layer;
            var rect = (RectTransform)label.transform; rect.anchorMin = new Vector2(0,1); rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0,1); rect.anchoredPosition = new Vector2(20,-15); rect.sizeDelta = new Vector2(-40,140);
            var text = label.GetComponent<UnityEngine.UI.Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24; text.color = Color.white; text.text = caption;
            try
            {
                camera.targetTexture = target; camera.transform.position = position; camera.transform.LookAt(targetPoint);
                camera.fieldOfView = fieldOfView; camera.farClipPlane = 300; camera.cullingMask = ~0;
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target; pixels.ReadPixels(new Rect(0,0,width,height),0,0); pixels.Apply(false);
                var rgb = pixels.GetRawTextureData();
                using (var file = File.Create(Path.Combine(directory,name + ".ppm")))
                {
                    var header = Encoding.ASCII.GetBytes("P6\n" + width + " " + height + "\n255\n"); file.Write(header,0,header.Length);
                    for (var row = height - 1; row >= 0; row--) file.Write(rgb,row * width * 3,width * 3);
                }
            }
            finally
            {
                camera.targetTexture = previousTarget; camera.transform.SetPositionAndRotation(previousPosition,previousRotation);
                camera.fieldOfView = previousFov; camera.farClipPlane = previousFar; camera.cullingMask = previousMask;
                RenderTexture.active = previousActive;
                target.Release(); Object.Destroy(target); Object.Destroy(pixels); Object.Destroy(overlay);
            }
        }
    }
}
