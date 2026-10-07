using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Feedback;
using Gravivore.Presentation.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class DeviceCorrectionSmokeTests
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
        [UnityTest] public IEnumerator PullPresentationIsShortBoundedAndNeverMovesAuthorityOrSensors()
        {
            yield return Load(); var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            var motion = root.GetComponent<VisualSliceCombatMotion>();
            var lash = root.GetComponentInChildren<GravityLashVfxPool>(); motion.enabled = false;
            motion.Tick(0);
            var targetOffset = enemy.TargetPoint.position-enemy.transform.position;
            var destination = enemy.transform.position+Vector3.forward*.8f;
            lash.Play(root.PlayerObject.transform.position,enemy.TargetPoint.position,enemy);
            Assert.IsTrue(enemy.TryDisplace(destination,default));
            var authority = enemy.transform.position;
            var hp = enemy.CurrentHitPoints; motion.Tick(.01f);
            Assert.That(motion.ActivePullCount,Is.GreaterThan(0));
            Assert.That(motion.MaximumActivePullOffset,Is.InRange(.01f,1.35f));
            Assert.That(enemy.transform.position,Is.EqualTo(authority));
            Assert.That(Vector3.Distance(enemy.TargetPoint.position-authority,targetOffset),Is.LessThan(.001f));
            motion.Tick(.2f);
            Assert.That(motion.ActivePullCount,Is.Zero);
            Assert.That(motion.MaximumActivePullOffset,Is.Zero);
            Assert.That(enemy.transform.position,Is.EqualTo(authority)); Assert.That(enemy.CurrentHitPoints,Is.EqualTo(hp));
        }
        [UnityTest] public IEnumerator ThreePlayerVariantsReusePoolsAndNeverReplaceDangerZoneGeometry()
        {
            yield return Load(); var root = _scene.Root;
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            var count = lash.Phase6BCreatedVfxCount;
            var variants = new System.Collections.Generic.HashSet<int>();
            var pointCounts = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < 6; i++)
            {
                lash.Play(root.PlayerObject.transform.position,root.PlayerObject.transform.position+Vector3.forward*3);
                variants.Add(lash.LastAttackVariant);
                var effect = lash.LastPlayedObject.GetComponent<Phase6BVfxInstance>();
                Assert.That(effect.PlayerVariant,Is.EqualTo(lash.LastAttackVariant));
                pointCounts.Add(effect.GetComponentInChildren<LineRenderer>().positionCount);
                lash.Tick(.5f); lash.Tick(.5f);
            }
            Assert.That(variants,Is.EquivalentTo(new[] { 0,1,2 })); Assert.That(pointCounts.Count,Is.EqualTo(2));
            Assert.That(lash.Phase6BCreatedVfxCount,Is.EqualTo(count));
            var warning = lash.Vfx.GetComponentsInChildren<Phase6BVfxInstance>(true).First(f => f.Cue==Phase6BVfxCue.BossConeTelegraph);
            warning.ConfigurePlayerVariant(2); Assert.That(warning.PlayerVariant,Is.Zero);
            Assert.That(warning.Shape,Is.EqualTo(Phase6BVfxShape.Cone));
        }
        [UnityTest] public IEnumerator OriginalMusicFadesCombatAndRespectsMute_TacticalMapRetainsAuthority()
        {
            yield return Load(); var root = _scene.Root;
            var music = root.GetComponent<IndustrialMusicPresenter>();
            Assert.IsNotNull(music); Assert.IsTrue(music.IsInitialized);
            Assert.IsTrue(music.ExplorationSource.loop && music.CombatSource.loop);
            Assert.That(music.ExplorationSource.clip.length, Is.EqualTo(72).Within(.1));
            Assert.That(music.CombatSource.clip.length, Is.EqualTo(72).Within(.1));
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            lash.Play(root.PlayerObject.transform.position, root.PlayerObject.transform.position + Vector3.forward*3);
            music.Tick(.1f); Assert.That(music.Intensity, Is.InRange(.01f,.15f));
            music.Tick(2.9f); Assert.That(music.Intensity, Is.EqualTo(1));
            music.Tick(7); Assert.That(music.Intensity, Is.Zero);
            var muted = PresentationAudioSettings.IsMuted;
            try { PresentationAudioSettings.SetMuted(true); music.Tick(.1f); Assert.That(music.ExplorationSource.volume, Is.Zero); Assert.That(music.CombatSource.volume, Is.Zero); }
            finally { PresentationAudioSettings.SetMuted(muted); }
            var map = root.GetComponentInChildren<MapMinimapPresenter>(true);
            Assert.IsNotNull(map); map.RefreshNow();
            Assert.That(map.CachedMarkerCount, Is.EqualTo(root.MapMarkers.Count));
            Assert.IsNotEmpty(map.CurrentZoneText);
            Assert.IsNotNull(map.CompactSurface.parent.Find("OctagonalBorder"));
            Assert.That(root.MapMarkers.GetMarker(root.MapMarkers.Count-1).Kind, Is.EqualTo(MapMarkerKind.Gate));
            var anchor = map.GetMarkerAnchor("player",false);
            // The established local crop clamps at chapter boundaries, including spawn.
            Assert.That(anchor.x,Is.InRange(0,1)); Assert.That(anchor.y,Is.InRange(0,1));
            Assert.That(map.CompactSurface.localRotation,Is.EqualTo(Quaternion.identity));
        }
        [UnityTest] public IEnumerator MobileCameraShowsAllThreeEvolutionsAndRebuiltSlice()
        {
            yield return Load(); var root = _scene.Root; var camera = Camera.main;
            var body = root.PlayerObject.GetComponent<CharacterController>(); body.enabled = false;
            root.PlayerObject.transform.SetPositionAndRotation(new Vector3(0,0,66),Quaternion.Euler(0,180,0)); body.enabled = true;
            root.MagnetarGuard.ActivateEncounter();
            camera.GetComponent<Gravivore.Presentation.Camera.PortraitFollowCamera>().SnapToTarget();
            var map = root.GetComponentInChildren<MapMinimapPresenter>(true); map.RefreshNow();
            Assert.That(map.CurrentZoneText,Is.EqualTo("Контур Магнетара"));
            var view = root.PlayerObject.GetComponent<PlayerEvolutionView>();
            Directory.CreateDirectory("docs/device-correction/internal");
            foreach (var tier in new[] { EvolutionTier.Tier0,EvolutionTier.Tier1,EvolutionTier.Tier2 })
            {
                view.Apply(new EvolutionVisualState(tier,view.CurrentDominantStat));
                yield return new WaitForSeconds(.3f);
                var target = new RenderTexture(540,960,24); var texture = new Texture2D(540,960,TextureFormat.RGB24,false);
                var prior = RenderTexture.active;
                var canvas = root.GetComponentInChildren<Canvas>(); var mode = canvas.renderMode;
                var canvasCamera = canvas.worldCamera;
                try { camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera;
                    canvas.planeDistance = 1; Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                    texture.ReadPixels(new Rect(0,0,540,960),0,0); texture.Apply();
                    File.WriteAllBytes("docs/device-correction/internal/live_tier"+(int)tier+".png",texture.EncodeToPNG()); }
                finally { canvas.renderMode = mode; canvas.worldCamera = canvasCamera; camera.targetTexture = null;
                    RenderTexture.active = prior; target.Release(); Object.Destroy(target); Object.Destroy(texture); }
            }
            Assert.That(body.radius,Is.EqualTo(.42f)); Assert.That(body.height,Is.EqualTo(1.4f));
        }
    }
}
