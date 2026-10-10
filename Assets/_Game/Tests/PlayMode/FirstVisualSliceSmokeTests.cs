using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class FirstVisualSliceSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup() { if (_scene != null) yield return _scene.Cleanup(); _scene = null; }
        private IEnumerator Load()
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load();
            _scene.Root.EnemyPopulation.enabled = false;
            foreach (var enemy in _scene.Root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            _scene.Root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            _scene.Root.MagnetarGuard.enabled = false; _scene.Root.CustodianBoss.enabled = false;
        }
        [UnityTest] public IEnumerator CanonicalGameplayUsesAllNewVisualsAndAuthorityCollision()
        {
            yield return Load(); var root = _scene.Root;
            Assert.That(root.PlayerObject.GetComponent<CharacterVisualBinding>().ActiveModel.name, Does.StartWith("G0_V3_Live"));
            Assert.That(root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0).GetComponent<CharacterVisualBinding>().ActiveModel.name, Is.EqualTo("Scout_V1"));
            Assert.That(root.EnemyPopulation.GetSpot(1).GetLiveEnemy(0).GetComponent<CharacterVisualBinding>().ActiveModel.name, Is.EqualTo("Cutter_V1"));
            Assert.That(root.MagnetarGuard.GetComponent<CharacterVisualBinding>().ActiveModel.name, Is.EqualTo("Magnetar_V1"));
            if(root.VisualEnvironment.BlueprintWorldOnly)
            {
                foreach(var id in new[]{"repair-hub","relay-yard","capacitor-field","cutting-floor","shield-dump","hauler-graveyard","elite-arena","boss-arena"})
                    Assert.IsNotEmpty(root.VisualEnvironment.GetRegion(id).Root.GetComponentsInChildren<Renderer>());
                Assert.IsNull(root.VisualEnvironment.Floor.Find("Phase3C Routes"));
            }
            else
            {
                Assert.IsNotNull(root.VisualEnvironment.Floor.Find("First Visual Slice Industrial Containment"));
                var oldRoutes = root.VisualEnvironment.Floor.Find("Phase3C Routes");
                foreach (Transform panel in oldRoutes)
                    Assert.IsTrue(panel.GetComponentsInChildren<Renderer>(true).All(r=>!r.enabled),
                        "Old corridor surface must not cover the rebuilt floor: " + panel.name);
                Assert.IsNotNull(root.VisualEnvironment.Floor.Find("Chapter 01 Full Production/Facility deck segmentation"));
            }
            Assert.IsTrue(root.VisualEnvironment.FullChapterProduction);
            Assert.IsEmpty(root.VisualEnvironment.GetComponentsInChildren<Collider>(true));
            Assert.That(root.PlayerObject.GetComponent<CharacterController>().radius, Is.EqualTo(.42f));
            Assert.That(root.PlayerObject.GetComponent<CharacterController>().height, Is.EqualTo(1.4f));
            Assert.That(root.WorldPresenter.EnvironmentBlockerCount, Is.EqualTo(root.WorldPresenter.Layout != null ? root.WorldPresenter.Layout.BlockerCount : 2 + root.VisualEnvironment.SliceObstacleCount));
            // The main approach, strong-spot and elite centres remain reachable when the normal gate unlocks.
            root.WorldPresenter.EliteGate.SetLocked(false); root.WorldPresenter.BossGate.SetLocked(false); Physics.SyncTransforms();
            for (var z = Mathf.CeilToInt(root.WorldPresenter.Configuration.EliteGate.Position.z-1); z < root.WorldPresenter.Configuration.BossGate.Position.z-1; z++)
                Assert.IsFalse(Physics.CheckCapsule(new Vector3(0,.45f,z), new Vector3(0,1.05f,z), .42f,
                    LayerMask.GetMask("HardBlocker"), QueryTriggerInteraction.Ignore), "central route at " + z);
            foreach (var spot in root.StrongSpots)
                Assert.IsFalse(Physics.CheckCapsule(spot.Position + Vector3.up*.45f, spot.Position + Vector3.up*1.05f, .42f,
                    LayerMask.GetMask("HardBlocker"), QueryTriggerInteraction.Ignore), spot.Id);
            var body=root.PlayerObject.GetComponent<CharacterController>();
            foreach(var other in root.GetComponentsInChildren<CharacterController>(true))if(other!=body)other.enabled=false;
            foreach(var strong in root.StrongSpots)
                Chapter1DeviceStabilizationSmokeTests.WalkCapsulePath(root,body,strong.Position);
        }
        [UnityTest] public IEnumerator OrdinaryShutdownAnimationOutlivesImmediateRecycleWithoutDelayingReward()
        {
            yield return Load(); var root = _scene.Root;
            var bridge = root.GetComponent<VisualSliceAnimationBridge>();
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            var objects = bridge.GetComponentsInChildren<Transform>(true).Length;
            var deaths = root.Phase6BCombat.EnemyDeathCount;
            enemy.ApplyDamage(new DamageRequest(10000, DamageType.Physical)); bridge.Tick();
            Assert.IsFalse(enemy.IsAlive); Assert.That(root.Phase6BCombat.EnemyDeathCount, Is.EqualTo(deaths + 1));
            Assert.That(bridge.ActiveShutdownCount, Is.EqualTo(1));
            yield return new WaitForSeconds(1.05f); bridge.Tick();
            Assert.That(bridge.ActiveShutdownCount, Is.Zero);
            Assert.That(bridge.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(objects));
        }
        [UnityTest] public IEnumerator InternalGameplayCameraCaptureShowsRebuiltApproachAndElite()
        {
            yield return Load(); var root = _scene.Root;
            var body = root.PlayerObject.GetComponent<CharacterController>();
            body.enabled = false; root.PlayerObject.transform.position = root.MagnetarGuard.transform.position+Vector3.back*4; body.enabled = true;
            root.MagnetarGuard.ActivateEncounter();
            yield return new WaitForSeconds(.5f);
            var camera = Camera.main;
            var previous = RenderTexture.active;
            var target = new RenderTexture(540, 960, 24); var texture = new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                texture.ReadPixels(new Rect(0,0,540,960),0,0); texture.Apply();
                Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/first-visual-slice/internal");
                File.WriteAllLines("docs/history/visual-stages/chapter01-visual-passes/first-visual-slice/internal/arena_surface_bounds.txt",
                    root.GetComponentsInChildren<Renderer>().Where(r => r.enabled && r.bounds.min.x <= 0 && r.bounds.max.x >= 0 &&
                        r.bounds.min.z <= 66 && r.bounds.max.z >= 66 && r.bounds.max.y < 1)
                        .Select(r => r.name + " | " + r.bounds + " | " + r.sharedMaterial.name));
                File.WriteAllBytes("docs/history/visual-stages/chapter01-visual-passes/first-visual-slice/internal/live_elite_area.png", texture.EncodeToPNG());
                Assert.That(root.PlayerHealth.IsAlive, Is.True);
                Assert.IsTrue(root.MagnetarGuard.IsAlive);
            }
            finally { camera.targetTexture = null; RenderTexture.active = previous; target.Release(); Object.Destroy(target); Object.Destroy(texture); }
        }
    }
}
