using System;
using System.Collections;
using System.IO;
using System.Text;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class MechaRespawnMarkerSmokeTests
    {
        private CanonicalSceneTestScope _scene;

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (_scene != null) yield return _scene.Cleanup();
            _scene = null;
        }

        [UnityTest]
        public IEnumerator CanonicalMecha_AllTiersKeepLimbsAndOnlyAuthoritativeCollider()
        {
            _scene = new CanonicalSceneTestScope();
            yield return _scene.Load();
            var root = _scene.Root;
            var player = root.PlayerObject;
            var controller = player.GetComponent<CharacterController>();
            Assert.That(controller.height, Is.EqualTo(1.4f));
            Assert.That(controller.radius, Is.EqualTo(0.42f));
            Assert.That(controller.center, Is.EqualTo(new Vector3(0f, 0.7f, 0f)));
            var colliders = player.GetComponentsInChildren<Collider>(true);
            Assert.That(colliders.Length, Is.EqualTo(1));
            Assert.AreSame(controller, colliders[0]);

            var visual = player.transform.Find("Player Visual Root");
            var view = player.GetComponent<PlayerEvolutionView>();
            foreach (var tier in new[] { EvolutionTier.Tier0, EvolutionTier.Tier1, EvolutionTier.Tier2 })
            {
                view.Apply(new EvolutionVisualState(tier, root.EvolutionPresenter.CurrentDominantStat));
                var form = visual.Find($"G0_Tier{(int)tier}_ArtSpike");
                Assert.IsTrue(form.gameObject.activeInHierarchy);
                Assert.IsNotNull(form.Find("01_RobotBody_CommonIdentity/SensorHead"));
                Assert.IsNotNull(form.Find("01_RobotBody_CommonIdentity/Torso"));
                Assert.That(form.Find("02_TwoMechanicalLegs_Common").childCount, Is.EqualTo(2));
                Assert.That(form.Find("03_ArticulatedGravityArms_Common").childCount, Is.EqualTo(2));
                Assert.That(view.GetActiveTierModuleCount(), Is.EqualTo(1));
                CapturePreviewIfRequested(tier, false);
                CapturePreviewIfRequested(tier, true);
            }

            var vfx = root.GetComponentInChildren<GravityLashVfxPool>();
            var destination = player.transform.position + Vector3.forward * 3f;
            vfx.Play(player.transform.position, destination);
            vfx.Tick(.15f);
            var beam = vfx.GetComponentInChildren<LineRenderer>();
            Assert.IsNotNull(beam);
            Assert.That(beam.GetPosition(0), Is.EqualTo(visual.Find("Gravity Lash Presentation Origin").position));
            Assert.That(beam.GetPosition(1), Is.EqualTo(destination));

            var levels = root.PlayerStats.BaseLevels;
            root.PlayerHealth.ApplyDamage(new DamageRequest(100000f, DamageType.Physical));
            Assert.That(root.PlayerHealth.CurrentHitPoints, Is.EqualTo(root.PlayerHealth.MaximumHitPoints));
            Assert.That(root.PlayerStats.BaseLevels, Is.EqualTo(levels));
            Assert.That(player.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator CanonicalSpots_DelayDeathsOnceRecoverAndPublishLiveMarkerSnapshots()
        {
            _scene = new CanonicalSceneTestScope();
            yield return _scene.Load();
            var root = _scene.Root;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.EnemyPopulation.enabled = false;
            root.PlayerObject.transform.position = new Vector3(30f, 0f, -30f);
            root.EnemyPopulation.Tick(0f);
            var spot = root.EnemyPopulation.GetSpot(0);
            var untouched = root.EnemyPopulation.GetSpot(1);
            var markers = root.WorldMarkers;
            Assert.That(markers.Count, Is.EqualTo(8));
            Assert.That(markers.GetMarker(0).Position, Is.EqualTo(root.PlayerObject.transform.position));
            Assert.That(markers.GetMarker(6).Kind, Is.EqualTo(WorldMarkerKind.Elite));
            Assert.That(markers.GetMarker(7).Kind, Is.EqualTo(WorldMarkerKind.Boss));
            Assert.That(markers.GetMarker(6).Status, Is.EqualTo(WorldMarkerStatus.Locked));
            Assert.That(markers.GetMarker(7).Status, Is.EqualTo(WorldMarkerStatus.Locked));
            Assert.Throws<ArgumentOutOfRangeException>(() => markers.GetMarker(8));

            var initialScore = root.Progression.State.TotalAssimilationScore;
            var first = spot.GetLiveEnemy(0);
            first.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            var rewardedScore = root.Progression.State.TotalAssimilationScore;
            Assert.That(rewardedScore, Is.GreaterThan(initialScore));
            first.ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            Assert.That(root.Progression.State.TotalAssimilationScore, Is.EqualTo(rewardedScore));
            for (var i = 0; i < 3; i++) spot.GetLiveEnemy(0).ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            Assert.That(spot.LiveCount, Is.Zero);
            Assert.That(spot.RespawnPenaltySteps, Is.EqualTo(1));
            Assert.That(spot.AdditionalRespawnDelay, Is.EqualTo(8f));
            Assert.That(untouched.RespawnPenaltySteps, Is.Zero);
            var empty = markers.GetMarker(1);
            Assert.That(empty.Id, Is.EqualTo(spot.Id));
            Assert.That(empty.Position, Is.EqualTo(spot.Position));
            Assert.That(empty.Status, Is.EqualTo(WorldMarkerStatus.Respawning));
            Assert.That(empty.PendingRespawns, Is.EqualTo(4));
            Assert.That(empty.PenaltySteps, Is.EqualTo(1));

            // First three deaths stay at baseline; the fourth is scheduled at baseline + 8.
            spot.Tick(12f);
            Assert.That(spot.LiveCount, Is.EqualTo(3));
            Assert.That(spot.PendingRespawns, Is.EqualTo(1));
            Assert.That(spot.SecondsUntilNextRespawn, Is.InRange(4f, 8f));
            spot.Tick(8f);
            Assert.That(spot.LiveCount, Is.EqualTo(4));
            Assert.That(root.EnemyPopulation.LiveEnemyCount, Is.LessThanOrEqualTo(25));
            Assert.That(first.CurrentHitPoints, Is.EqualTo(first.MaximumHitPoints));
            spot.Tick(71f);
            Assert.That(spot.RespawnPenaltySteps, Is.Zero);
            Assert.That(markers.GetMarker(1).Status, Is.EqualTo(WorldMarkerStatus.Available));
            spot.GetLiveEnemy(0).ApplyDamage(new DamageRequest(10000f, DamageType.Gravity));
            Assert.That(spot.SecondsUntilNextRespawn, Is.InRange(8f, 12f));
            Assert.That(spot.AdditionalRespawnDelay, Is.Zero);
            var restored = WorldUnlockState.Restore("elite-gate", "boss-gate", root.MagnetarGuard.Id,
                new WorldUnlockSnapshot(true, true, true));
            var restoredMarkers = new WorldMarkerReadModel(root.PlayerObject.transform, root.EnemyPopulation,
                root.MagnetarGuard, root.CustodianBoss, restored);
            Assert.That(restoredMarkers.GetMarker(6).Status, Is.EqualTo(WorldMarkerStatus.Defeated));
        }

        private void CapturePreviewIfRequested(EvolutionTier tier, bool detail)
        {
            var directory = Environment.GetEnvironmentVariable("GRAVIVORE_VISUAL_QA_DIR");
            if (string.IsNullOrEmpty(directory)) return;
            Directory.CreateDirectory(directory);
            var camera = UnityEngine.Camera.main;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var previousPosition = camera.transform.position;
            var previousRotation = camera.transform.rotation;
            var previousFov = camera.fieldOfView;
            var target = new RenderTexture(540, 960, 24);
            var pixels = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                if (detail)
                {
                    var player = _scene.Root.PlayerObject.transform.position;
                    camera.transform.position = player + new Vector3(2.4f, 1.8f, 3.2f);
                    camera.transform.LookAt(player + Vector3.up * 0.8f);
                    camera.fieldOfView = 35f;
                }
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 540, 960), 0, 0);
                pixels.Apply();
                // Portable RGB output avoids adding an image-conversion module solely for QA.
                var rgb = pixels.GetRawTextureData();
                using (var file = File.Create(Path.Combine(directory,
                    $"mecha-{tier}-{(detail ? "detail" : "gameplay")}.ppm")))
                {
                    var header = Encoding.ASCII.GetBytes("P6\n540 960\n255\n");
                    file.Write(header, 0, header.Length);
                    for (var row = 959; row >= 0; row--) file.Write(rgb, row * 540 * 3, 540 * 3);
                }
            }
            finally
            {
                camera.targetTexture = previousTarget;
                camera.transform.SetPositionAndRotation(previousPosition, previousRotation);
                camera.fieldOfView = previousFov;
                RenderTexture.active = previousActive;
                target.Release();
                UnityEngine.Object.Destroy(target);
                UnityEngine.Object.Destroy(pixels);
            }
        }
    }
}
