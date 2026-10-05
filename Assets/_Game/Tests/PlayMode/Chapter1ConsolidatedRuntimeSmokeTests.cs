using System;
using System.Collections;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.AudioVfx;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter1ConsolidatedRuntimeSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (_scene != null) yield return _scene.Cleanup();
        }
        private IEnumerator Load()
        {
            _scene = new CanonicalSceneTestScope();
            yield return _scene.Load();
            Control(_scene.Root);
        }
        private static void Control(S01SceneCompositionRoot root)
        {
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            root.EnemyPopulation.enabled = false;
            root.MagnetarGuard.enabled = false;
            root.CustodianBoss.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
        }

        [UnityTest]
        public IEnumerator ProductionMapStrongSpotsAndRepair_UseAuthoritativeStateAndBoundedPools()
        {
            yield return Load();
            var root = _scene.Root;
            Assert.That(root.SaveSchemaVersion, Is.EqualTo(2));
            Assert.That(root.EnemyPopulation.SpotCount, Is.EqualTo(9));
            Assert.That(root.StrongSpots.Count, Is.EqualTo(4));
            Assert.That(root.MapMarkers.Count, Is.EqualTo(13));
            var map = root.MapIntegration.MapPresenter;
            Assert.That(map.CompactSurface.gameObject.activeInHierarchy, Is.True);
            Capture(root, "01_chapter_minimap.png");
            root.PlayerObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Assert.That(root.MapMarkers.GetMarker(0).HeadingDegrees, Is.EqualTo(90f).Within(.001f));
            map.OpenExpanded(); map.RefreshNow();
            Assert.That(map.CachedMarkerCount, Is.EqualTo(13));
            for (var i = 5; i < 9; i++)
            {
                Assert.That(root.EnemyPopulation.GetSpot(i).LiveCount, Is.GreaterThan(0), "Strong spots cannot starve behind the global cap.");
                Assert.That(root.MapMarkers.GetMarker(i + 1).Kind, Is.EqualTo(MapMarkerKind.StrongOrdinary));
            }
            Assert.That(map.SelectMarker("strong-elite-a"), Is.True);
            Capture(root, "02_expanded_map.png");
            map.CloseExpanded();
            var strong = root.EnemyPopulation.GetSpot(5);
            var independent = root.EnemyPopulation.GetSpot(6);
            var baseline = root.EnemyPopulation.GetSpot(3).GetLiveEnemy(0);
            Assert.That(strong.GetLiveEnemy(0).CurrentHitPoints, Is.GreaterThan(baseline.CurrentHitPoints));
            var rewards = 0f;
            root.EnemyPopulation.EnemyDied += death => rewards = death.RewardMultiplier;
            while (strong.LiveCount > 0) strong.GetLiveEnemy(0).ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            Assert.That(rewards, Is.EqualTo(root.StrongSpots[0].RewardMultiplier));
            Assert.That(strong.RespawnPenaltySteps, Is.EqualTo(1));
            Assert.That(independent.RespawnPenaltySteps, Is.Zero);
            root.EnemyPopulation.Tick(20f);
            Assert.That(strong.LiveCount, Is.GreaterThan(0));
            Assert.That(root.EnemyPopulation.LiveEnemyCount, Is.LessThanOrEqualTo(25));
            Move(root, strong.Position + Vector3.back * 3f);
            Capture(root, "05_strong_ordinary.png");
            Move(root, root.MapMarkers.GetMarker(12).WorldPosition);
            root.PlayerHealth.enabled = false;
            root.PlayerHealth.ApplyDamage(new DamageRequest(15f, DamageType.Physical));
            var damagedHp = root.PlayerHealth.CurrentHitPoints;
            root.PlayerHealth.Tick(3.1f);
            Assert.That(root.PlayerHealth.CurrentHitPoints, Is.GreaterThan(damagedHp));
            Assert.That(root.RepairHub.IsRepairing, Is.True);
            Capture(root, "10_repair_hub.png");
            Move(root, root.PlayerObject.transform.position + Vector3.right * 10f);
            yield return null;
            Assert.That(root.RepairHub.IsRepairing, Is.False);
            Assert.That(root.RepairHub.Vfx.ActiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator MagnetarAndCustodian_FirstClearRepeatCooldownAndReload_PreservePermanentProgression()
        {
            yield return Load();
            var root = _scene.Root;
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();
            root.Chapter1Encounters.Tick();
            var elite = root.MagnetarGuard;
            Assert.That(elite.IsAlive, Is.True);
            Move(root, elite.transform.position + Vector3.back);
            elite.Tick(0f);
            Assert.That(root.Phase6BCombat.EliteAttackCount, Is.EqualTo(1));
            Assert.That(root.EncounterTelegraphs.EliteTelegraphVisible, Is.False);
            Capture(root, "06_magnetar.png");
            elite.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            var first = root.MapMarkers.GetMarker(10);
            Assert.That(first.FirstClearCompleted, Is.True);
            Assert.That(first.Availability, Is.EqualTo(MapAvailabilityState.Cooldown));
            Assert.That(first.RemainingSeconds, Is.EqualTo(900f));
            Assert.That(first.PremiumRewardsRemaining, Is.EqualTo(3));
            _scene.AdvanceTime(TimeSpan.FromMinutes(15)); root.Chapter1Encounters.Tick();
            Assert.That(elite.IsAlive, Is.True);
            elite.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            Assert.That(root.MapMarkers.GetMarker(10).PremiumRewardsRemaining, Is.EqualTo(2));
            var completions = 0;
            root.BossCompletion.Defeated += _ => completions++;
            var boss = root.CustodianBoss;
            Move(root, boss.transform.position + Vector3.forward * 4.5f);
            boss.Tick(0f);
            Assert.That(boss.CanBeTargeted, Is.True);
            boss.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            Assert.That(completions, Is.EqualTo(1));
            Assert.That(root.MapMarkers.GetMarker(11).RemainingSeconds, Is.EqualTo(1800f));
            _scene.AdvanceTime(TimeSpan.FromMinutes(30)); root.Chapter1Encounters.Tick();
            Move(root, boss.transform.position + Vector3.forward * 4.5f); boss.Tick(0f);
            boss.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            Assert.That(completions, Is.EqualTo(1), "Repeats must never complete the Chapter again.");
            Assert.That(root.MapMarkers.GetMarker(11).PremiumRewardsRemaining, Is.EqualTo(1));
            Assert.That(root.FlushNow(), Is.True);
            var score = root.Progression.State.TotalAssimilationScore;
            yield return _scene.Load();
            root = _scene.Root; Control(root);
            Assert.That(root.Progression.State.TotalAssimilationScore, Is.EqualTo(score));
            Assert.That(root.MapMarkers.GetMarker(11).RemainingSeconds, Is.EqualTo(1800f));
            Assert.That(root.MapMarkers.GetMarker(11).PremiumRewardsRemaining, Is.EqualTo(1));
            Assert.That(root.BossCompletion.IsDefeated, Is.True);
            Assert.That(root.CustodianBoss.CanBeTargeted, Is.False);
            Assert.That(root.MapMarkers.GetMarker(10).FirstClearCompleted, Is.True);
        }

        [UnityTest]
        public IEnumerator CustodianPresentation_UsesExactGameplayGeometryTimingAndCancelsOnReset()
        {
            yield return Load();
            var root = _scene.Root;
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment(); root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            var boss = root.CustodianBoss;
            Move(root, boss.transform.position + Vector3.forward * 4.5f);
            boss.Tick(0f);
            var expected = new[] { Phase6BVfxCue.BossCircleTelegraph, Phase6BVfxCue.BossConeTelegraph, Phase6BVfxCue.BossLineTelegraph };
            var evidence = new[] { "09_boss_circle.png", "07_boss_cone.png", "08_boss_line.png" };
            for (var i = 0; i < expected.Length; i++)
            {
                var bridge = root.Phase6BCombat;
                Assert.That(bridge.LastTelegraphCue, Is.EqualTo(expected[i]));
                Assert.That(bridge.LastTelegraphDuration, Is.EqualTo(i == 2 ? 1.1f : 1f));
                Assert.That(root.EncounterTelegraphs.BossTelegraphVisible, Is.False);
                var instance = bridge.Vfx.GetComponentsInChildren<Phase6BVfxInstance>(true).Single(x => x.Cue == expected[i]);
                Assert.That(instance.IsPlaying, Is.True);
                Assert.That(instance.PresentedRange, Is.EqualTo(i == 0 ? 4f : i == 1 ? 6f : 7f));
                var hp = root.PlayerHealth.CurrentHitPoints;
                boss.Tick(.25f);
                Assert.That(root.PlayerHealth.CurrentHitPoints, Is.EqualTo(hp), "Warning presentation cannot commit damage.");
                Capture(root, evidence[i]);
                boss.Tick(2f); boss.Tick(0f); boss.Tick(2f);
                if (i < 2) boss.Tick(0f);
            }
            boss.ResetEncounter();
            foreach (var instance in root.Phase6BCombat.Vfx.GetComponentsInChildren<Phase6BVfxInstance>(true))
                if (expected.Contains(instance.Cue)) Assert.That(instance.IsPlaying, Is.False);
        }

        [UnityTest]
        public IEnumerator PlayerAndEnemyFeedback_UseOneProductionObserverAndResetPooledLives()
        {
            yield return Load();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.enabled = true;
            enemy.transform.position = root.PlayerObject.transform.position + Vector3.forward * 2.5f;
            Physics.SyncTransforms();
            var attack = root.PlayerObject.GetComponent<GravityAttackController>();
            var hp = enemy.CurrentHitPoints;
            attack.ResetTransientState(); attack.Tick(0f);
            Assert.That(enemy.CurrentHitPoints, Is.LessThan(hp));
            Assert.That(root.Phase6BCombat.EnemyHitCount + root.Phase6BCombat.EnemyDeathCount, Is.EqualTo(1));
            Capture(root, "03_player_attack.png");
            if (enemy.IsAlive) enemy.ApplyDamage(new DamageRequest(100000f, DamageType.Gravity));
            Assert.That(root.Phase6BCombat.EnemyDeathCount, Is.EqualTo(1));
            Assert.That(root.CombatFeedback.EnemyHitPool.ActiveCount, Is.Zero);
            Assert.That(root.CombatFeedback.EnemyDeathPool.ActiveCount, Is.Zero);
            Capture(root, "04_enemy_death.png");
            Move(root, root.PlayerObject.transform.position + Vector3.back * 10f);
            root.EnemyPopulation.Tick(20f);
            Assert.That(root.EnemyPopulation.GetSpot(0).LiveCount, Is.GreaterThan(0));
        }

        private static void Move(S01SceneCompositionRoot root, Vector3 position)
        {
            var body = root.PlayerObject.GetComponent<CharacterController>();
            body.enabled = false; root.PlayerObject.transform.position = position; body.enabled = true;
            Physics.SyncTransforms();
            root.MapIntegration.MapPresenter.RefreshNow();
        }

        private static void Capture(S01SceneCompositionRoot root, string name)
        {
            if (Environment.GetEnvironmentVariable("GRAVIVORE_CAPTURE_EVIDENCE") != "1") return;
            var camera = UnityEngine.Camera.main;
            camera.GetComponent<PortraitFollowCamera>().enabled = false;
            camera.transform.position = root.PlayerObject.transform.position + new Vector3(0f, 14f, -10f);
            camera.transform.LookAt(root.PlayerObject.transform.position);
            var canvas = root.GetComponentInChildren<Canvas>();
            var mode = canvas.renderMode;
            var priorCamera = canvas.worldCamera;
            var priorTarget = camera.targetTexture;
            var priorActive = RenderTexture.active;
            var target = new RenderTexture(540, 960, 24);
            var texture = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f;
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); texture.Apply();
                var directory = Path.Combine(Application.dataPath, "../Builds/Evidence");
                Directory.CreateDirectory(directory); File.WriteAllBytes(Path.Combine(directory, name), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = priorActive; camera.targetTexture = priorTarget;
                canvas.renderMode = mode; canvas.worldCamera = priorCamera;
                UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(texture);
            }
        }
    }
}
