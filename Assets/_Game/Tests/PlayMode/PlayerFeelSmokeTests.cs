using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Feedback;
using Gravivore.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class PlayerFeelSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (_scene != null) yield return _scene.Cleanup();
            _scene = null;
        }

        private IEnumerator LoadControlled()
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load();
            _scene.Root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            _scene.Root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            _scene.Root.EnemyPopulation.enabled = false;
            foreach (var enemy in _scene.Root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            _scene.Root.PlayerObject.GetComponent<MechMotionPresenter>().enabled = false;
            _scene.Root.GetComponentInChildren<GravityLashVfxPool>().enabled = false;
        }

        [UnityTest]
        public IEnumerator ActualMovementAllDirectionsAndTiersWalkStopAndNeverMoveAuthority()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var player = root.PlayerObject.transform;
            var motion = player.GetComponent<MechMotionPresenter>();
            var view = player.GetComponent<PlayerEvolutionView>();
            var locomotion = player.GetComponent<PlayerLocomotion>();
            var input = new TestInput();
            locomotion.Initialize(input, Camera.main.transform, root.PlayerStats, 720f);
            foreach (var tier in new[] { EvolutionTier.Tier0, EvolutionTier.Tier1, EvolutionTier.Tier2 })
            {
                view.Apply(new EvolutionVisualState(tier, view.CurrentDominantStat));
                var form = view.GetTierForm(tier);
                Assert.That(form.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(form.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.IsFalse(form.GetComponentsInChildren<Component>(true).Any(c => c.GetType().FullName == "UnityEngine.Animator"));
                foreach (var direction in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right, Vector2.one.normalized })
                {
                    input.Movement = direction;
                    locomotion.Step(.04f);
                    var authority = player.position; var rotation = player.rotation;
                    motion.Tick(.04f);
                    Assert.IsTrue(motion.IsWalking);
                    Assert.That(motion.ObservedVelocity.magnitude, Is.GreaterThan(.1f));
                    Assert.That(player.position, Is.EqualTo(authority));
                    Assert.That(player.rotation, Is.EqualTo(rotation));
                    var hip = form.Find("02_TwoMechanicalLegs_Common/LeftLeg/HipPivot");
                    var movingPose = hip.localRotation;
                    Assert.That(Quaternion.Angle(movingPose, Quaternion.identity), Is.GreaterThan(1));
                    input.Movement = Vector2.zero; locomotion.Step(.04f); motion.Tick(.04f);
                    Assert.IsFalse(motion.IsWalking);
                    Assert.That(hip.localRotation, Is.EqualTo(Quaternion.identity));
                    Assert.That(player.position, Is.EqualTo(authority));
                }
            }
            var idleCore = view.GetTierForm(EvolutionTier.Tier2).Find("01_RobotBody_CommonIdentity/GravityCore_Common");
            var before = idleCore.localScale;
            motion.Tick(.2f);
            Assert.That(idleCore.localScale, Is.Not.EqualTo(before));
        }

        [UnityTest]
        public IEnumerator AngledAttacksUseActiveCoreSocketAndPoolChargeReleaseImpact()
        {
            yield return LoadControlled();
            var player = _scene.Root.PlayerObject.transform;
            var view = player.GetComponent<PlayerEvolutionView>();
            var motion = player.GetComponent<MechMotionPresenter>();
            var lash = _scene.Root.GetComponentInChildren<GravityLashVfxPool>();
            var cues = new List<GravityLashCue>(); lash.CuePlayed += (cue, _) => cues.Add(cue);
            var objects = lash.GetComponentsInChildren<Transform>(true).Length;
            foreach (var tier in new[] { EvolutionTier.Tier0, EvolutionTier.Tier1, EvolutionTier.Tier2 })
            {
                view.Apply(new EvolutionVisualState(tier, view.CurrentDominantStat));
                foreach (var direction in new[] { Vector3.forward, Vector3.right, Vector3.back, new Vector3(-1,0,1).normalized })
                {
                    var destination = player.position + direction * 3 + Vector3.up * .7f;
                    var authority = player.position;
                    cues.Clear(); lash.BeginCharge(authority, destination, null, .15f);
                    Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Windup }));
                    var beam = lash.LastPlayedObject.GetComponent<LineRenderer>();
                    Assert.IsFalse(beam.gameObject.activeSelf);
                    lash.Tick(.15f);
                    Assert.IsFalse(beam.gameObject.activeSelf, "Charge expiry cannot release an attack.");
                    lash.Play(authority, destination);
                    Assert.IsTrue(beam.gameObject.activeSelf);
                    Assert.That(Vector3.Distance(beam.GetPosition(0), motion.PresentationSocket.position), Is.LessThan(.001f));
                    var core = view.GetTierForm(tier).Find("01_RobotBody_CommonIdentity/GravityCore_Common");
                    Assert.That(Vector3.Distance(motion.PresentationSocket.position, core.position), Is.InRange(.1f,.14f));
                    Assert.That(Vector3.Dot(motion.PresentationSocket.forward, direction), Is.GreaterThan(.99f));
                    lash.Tick(.08f); lash.Tick(.12f);
                    Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Windup, GravityLashCue.Beam, GravityLashCue.Impact }));
                    Assert.That(player.position, Is.EqualTo(authority));
                }
            }
            Assert.That(lash.ActiveCount, Is.Zero);
            Assert.That(lash.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(objects));
        }

        [UnityTest]
        public IEnumerator ThrowingReleaseObserverCannotCancelImmediateDamageHitFeedbackOrPull()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.enabled = true; // TargetSelector intentionally ignores disabled gameplay actors.
            enemy.transform.position = root.PlayerObject.transform.position + Vector3.forward * 2.5f;
            Physics.SyncTransforms();
            var hp = enemy.CurrentHitPoints;
            var distance = Vector3.Distance(enemy.transform.position, root.PlayerObject.transform.position);
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            lash.CuePlayed += (cue, _) => { if (cue == GravityLashCue.Beam) throw new InvalidOperationException("release observer failed"); };
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: release observer failed");
            var attack = root.PlayerObject.GetComponent<GravityAttackController>();
            attack.ResetTransientState(); attack.Tick(0);
            Assert.That(enemy.CurrentHitPoints, Is.LessThan(hp));
            if (enemy.IsAlive) Assert.That(Vector3.Distance(enemy.transform.position, root.PlayerObject.transform.position), Is.LessThan(distance));
            Assert.IsTrue(lash.LastPlayedObject.activeSelf, "Beam must exist in the same call as HP/feedback.");
            Assert.That(root.CombatFeedback.EnemyHitPool.ActiveCount + root.CombatFeedback.EnemyDeathPool.ActiveCount, Is.GreaterThan(0));
        }

        [UnityTest]
        public IEnumerator DisablingGameplayCancelsItsPendingChargeImmediately()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.enabled = true;
            enemy.transform.position = root.PlayerObject.transform.position + Vector3.forward * 2.5f;
            Physics.SyncTransforms();
            var attack = root.PlayerObject.GetComponent<GravityAttackController>();
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            attack.ResetTransientState(); attack.Tick(0);
            lash.Tick(.08f); lash.Tick(.12f);
            attack.Tick(root.PlayerStats.DerivedStats.AttackInterval - .14f);
            Assert.That(lash.ActiveCount, Is.EqualTo(1));
            var hp = enemy.CurrentHitPoints;
            attack.enabled = true; attack.enabled = false;
            Assert.That(lash.ActiveCount, Is.Zero);
            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(hp));
        }

        [UnityTest]
        public IEnumerator ChargeExpiryCannotCommitDamageAndTargetLossClearsVisualPose()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.enabled = true;
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            var view = root.PlayerObject.GetComponent<PlayerEvolutionView>();
            var core = view.GetTierForm(view.CurrentTier).Find("01_RobotBody_CommonIdentity/GravityCore_Common");
            var scale = core.localScale;
            var hp = enemy.CurrentHitPoints;
            var cues = new List<GravityLashCue>(); lash.CuePlayed += (cue, _) => cues.Add(cue);
            lash.BeginCharge(root.PlayerObject.transform.position, enemy.TargetPoint.position, enemy, .15f);
            lash.Tick(1f);
            Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Windup }));
            Assert.IsFalse(lash.LastPlayedObject.activeSelf);
            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(hp));
            enemy.enabled = false;
            lash.Tick(0);
            lash.Tick(1f);
            Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Windup, GravityLashCue.Cancelled }));
            Assert.That(lash.ActiveCount, Is.Zero);
            Assert.That(core.localScale, Is.EqualTo(scale));
            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(hp));
        }

        [UnityTest]
        public IEnumerator ThrowingPrechargeObserverCannotBlockNextGameplayCommitOrLaterObservers()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            enemy.enabled = true;
            enemy.transform.position = root.PlayerObject.transform.position + Vector3.forward * 2.5f;
            Physics.SyncTransforms();
            var attack = root.PlayerObject.GetComponent<GravityAttackController>();
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            attack.ResetTransientState(); attack.Tick(0);
            var hp = enemy.CurrentHitPoints;
            var laterObserver = 0;
            lash.CuePlayed += (cue, _) => { if (cue == GravityLashCue.Windup) throw new InvalidOperationException("precharge observer failed"); };
            lash.CuePlayed += (cue, _) => { if (cue == GravityLashCue.Windup) laterObserver++; };
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: precharge observer failed");
            attack.Tick(root.PlayerStats.DerivedStats.AttackInterval - .14f);
            Assert.That(laterObserver, Is.EqualTo(1));
            Assert.That(enemy.CurrentHitPoints, Is.EqualTo(hp));
            attack.Tick(.141f);
            Assert.That(enemy.CurrentHitPoints, Is.LessThan(hp));
            Assert.IsTrue(lash.LastPlayedObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator RecycledEnemyLifeCancelsOldChargeAndReleasedImpactKeepsOldSnapshot()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var poolRoot = new GameObject("Timing Life Reuse Pool");
            poolRoot.transform.SetParent(root.transform);
            var pool = new OrdinaryEnemyPool(poolRoot.transform, 1, 9, TestMaterialFactory.Lit);
            var configuration = new EnemyRuntimeConfiguration("timing-reuse", 1000f, .0001f, 0f, .4f, .9f,
                new EnemyBehaviorParameters(5f, 7f, .1f, 10f));
            var player = root.PlayerObject.transform;
            var enemy = pool.Acquire(configuration, player, root.PlayerHealth, player.position + Vector3.forward * 3, pool.Return);
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            var cues = new List<GravityLashCue>();
            var impact = Vector3.zero;
            lash.CuePlayed += (cue, point) => { cues.Add(cue); if (cue == GravityLashCue.Impact) impact = point; };
            var oldLife = enemy.LifeId;
            lash.BeginCharge(player.position, enemy.TargetPoint.position, enemy, .15f);
            pool.Return(enemy);
            var reused = pool.Acquire(configuration, player, root.PlayerHealth, player.position + Vector3.right * 3, pool.Return);
            Assert.AreSame(enemy, reused);
            Assert.That(reused.LifeId, Is.Not.EqualTo(oldLife));
            lash.Tick(1f);
            Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Windup, GravityLashCue.Cancelled }));
            Assert.That(lash.ActiveCount, Is.Zero);
            Assert.That(reused.CurrentHitPoints, Is.EqualTo(1000f));
            var snapshot = reused.TargetPoint.position;
            lash.Play(player.position, snapshot, reused);
            pool.Return(reused);
            var next = pool.Acquire(configuration, player, root.PlayerHealth, player.position + Vector3.right * 30, pool.Return);
            lash.Tick(.08f);
            Assert.That(impact, Is.EqualTo(snapshot));
            Assert.That(next.CurrentHitPoints, Is.EqualTo(1000f));
            lash.Tick(.12f);
            Assert.That(lash.ActiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator AudioMuteVolumeAndIndependentDamageObserverRemainEffective()
        {
            yield return LoadControlled();
            var audio = _scene.Root.AudioPresenter;
            var oldMute = audio.IsMuted; var oldVolume = audio.Volume;
            try
            {
                var count = audio.PlayedCount; audio.SetMuted(true);
                foreach (var cue in new[] { S14AudioCue.Step, S14AudioCue.LashWindup, S14AudioCue.Release, S14AudioCue.LashImpact,
                    S14AudioCue.Hit, S14AudioCue.Death, S14AudioCue.PlayerHit, S14AudioCue.PlayerDeath }) audio.Play(cue);
                Assert.That(audio.PlayedCount, Is.EqualTo(count));
                var independentHaptics = new RecordingHaptics();
                _scene.Root.PlayerHealth.Damaged += _ => independentHaptics.Play(HapticCue.LightImpact);
                _scene.Root.PlayerHealth.Damaged += _ => throw new InvalidOperationException("audio observer failed");
                LogAssert.Expect(LogType.Exception, "InvalidOperationException: audio observer failed");
                var hp = _scene.Root.PlayerHealth.CurrentHitPoints;
                _scene.Root.PlayerHealth.ApplyDamage(new DamageRequest(5, DamageType.Gravity));
                Assert.That(_scene.Root.PlayerHealth.CurrentHitPoints, Is.LessThan(hp));
                Assert.That(independentHaptics.Count, Is.EqualTo(1));
                audio.SetMuted(false); audio.SetVolume(0); audio.Play(S14AudioCue.Release);
                Assert.That(audio.PlayedCount, Is.EqualTo(count));
                audio.SetVolume(.5f); audio.Play(S14AudioCue.Release);
                Assert.That(audio.PlayedCount, Is.EqualTo(count + 1));
                Assert.That(audio.SourceCount, Is.EqualTo(4));
                Assert.That(audio.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(4));
            }
            finally { audio.SetMuted(oldMute); audio.SetVolume(oldVolume); }
        }

        [UnityTest]
        public IEnumerator EnemyDeathAndPlayerDeathUseDistinctBoundedShutdownPools()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(0).GetLiveEnemy(0);
            var capacity = root.CombatFeedback.EnemyDeathPool.Capacity;
            enemy.ApplyDamage(new DamageRequest(100000, DamageType.Gravity));
            Assert.IsFalse(enemy.IsAlive);
            Assert.That(root.CombatFeedback.EnemyDeathPool.ActiveCount, Is.EqualTo(1));
            Assert.That(root.CombatFeedback.EnemyHitPool.ActiveCount, Is.Zero);
            Assert.That(root.CombatFeedback.EnemyDeathPool.Capacity, Is.EqualTo(capacity));
            var stats = root.PlayerStats.BaseLevels;
            root.PlayerHealth.ApplyDamage(new DamageRequest(100000, DamageType.Gravity));
            Assert.That(root.CombatFeedback.PlayerDeathPool.ActiveCount, Is.EqualTo(1));
            Assert.That(root.PlayerHealth.CurrentHitPoints, Is.EqualTo(root.PlayerHealth.MaximumHitPoints));
            Assert.That(root.PlayerStats.BaseLevels, Is.EqualTo(stats));
            Assert.That(root.PlayerObject.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RuntimeReviewCapturesAndPerformanceSnapshot()
        {
            yield return LoadControlled();
            var root = _scene.Root;
            var player = root.PlayerObject.transform;
            var motion = player.GetComponent<MechMotionPresenter>();
            var view = player.GetComponent<PlayerEvolutionView>();
            var camera = Camera.main;
            var tiers = new List<TierMetrics>();
            for (var tier = 0; tier < 3; tier++)
            {
                view.Apply(new EvolutionVisualState((EvolutionTier)tier, view.CurrentDominantStat)); motion.Tick(.016f);
                var form = view.GetTierForm((EvolutionTier)tier);
                var renderers = form.GetComponentsInChildren<Renderer>();
                var triangles = form.GetComponentsInChildren<MeshFilter>().Sum(f => (long)f.sharedMesh.triangles.Length / 3);
                tiers.Add(new TierMetrics { tier = tier, renderers = renderers.Length,
                    materialSlots = renderers.Sum(x => x.sharedMaterials.Length), triangles = triangles });
                Assert.That(renderers.Length, Is.LessThanOrEqualTo(40));
                Capture($"0{tier + 1}_Tier{tier}_Gameplay", false);
            }
            Capture("04_Tier2_Close", true);
            var locomotion = player.GetComponent<PlayerLocomotion>();
            var input = new TestInput { Movement = Vector2.up };
            locomotion.Initialize(input, camera.transform, root.PlayerStats, 720f);
            locomotion.Step(.085f); motion.Tick(.085f); Capture("05_WalkPose_A", false);
            locomotion.Step(.16f); motion.Tick(.16f); Capture("06_WalkPose_B", false);
            input.Movement = Vector2.zero; motion.Tick(.016f);
            var cutter = root.GetComponentsInChildren<OrdinaryEnemyController>().First(e =>
                e.transform.Find("Phase 3 Enemy Art Root/Phase3 Enemy [cutter-unit / accepted legacy presentation]/Cutter_ArtSpike") != null);
            cutter.transform.position = player.position + new Vector3(-1.1f,0,2.1f);
            var lash = root.GetComponentInChildren<GravityLashVfxPool>();
            cutter.enabled = true;
            Physics.SyncTransforms();
            var attack = player.GetComponent<GravityAttackController>();
            attack.ResetTransientState(); attack.Tick(0); // First acquisition releases directly.
            lash.Tick(.08f); lash.Tick(.12f);
            foreach (var pool in root.GetComponentsInChildren<PooledPulseVfx>()) pool.Tick(2f);
            var interval = root.PlayerStats.DerivedStats.AttackInterval;
            var hpBeforeCharge = cutter.CurrentHitPoints;
            attack.Tick(interval - .14f); motion.Tick(.016f);
            Assert.That(cutter.CurrentHitPoints, Is.EqualTo(hpBeforeCharge));
            Assert.IsFalse(lash.LastPlayedObject.activeSelf);
            Capture("07_Attack_Charge", false);
            attack.Tick(.141f);
            Assert.That(cutter.CurrentHitPoints, Is.LessThan(hpBeforeCharge));
            Assert.IsTrue(lash.LastPlayedObject.activeSelf);
            Capture("08_Attack_Release", false);
            lash.Tick(.08f); Capture("09_Attack_Impact", false);
            Capture("10_PlayerVsCutter_Combat", false);
            var snapshot = new Metrics
            {
                tiers = tiers.ToArray(), audioSources = root.AudioPresenter.SourceCount,
                activeParticleSystems = root.GetComponentsInChildren<Component>().Count(c => c.GetType().FullName == "UnityEngine.ParticleSystem"),
                lashCapacity = lash.Capacity,
                pulseCapacity = root.GetComponentsInChildren<PooledPulseVfx>(true).Sum(p => p.Capacity),
                activePulseObjects = root.GetComponentsInChildren<PooledPulseVfx>(true).Sum(p => p.ActiveCount),
                presentationMotionControllers = root.GetComponentsInChildren<MechMotionPresenter>().Length
            };
            Assert.That(snapshot.activeParticleSystems, Is.Zero);
            Assert.That(snapshot.presentationMotionControllers, Is.EqualTo(1));
            var directory = Environment.GetEnvironmentVariable("GRAVIVORE_PLAYER_FEEL_QA");
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "PERFORMANCE.json"), JsonUtility.ToJson(snapshot, true) + "\n");
            }
        }

        [Serializable] private sealed class TierMetrics { public int tier, renderers, materialSlots; public long triangles; }
        [Serializable] private sealed class Metrics
        {
            public TierMetrics[] tiers;
            public int audioSources, activeParticleSystems, lashCapacity, pulseCapacity, activePulseObjects, presentationMotionControllers;
        }
        private sealed class TestInput : IMovementInput { public Vector2 Movement { get; set; } }
        private sealed class RecordingHaptics : IHapticFeedback { public int Count; public void Play(HapticCue cue) => Count++; }

        private void Capture(string name, bool detail)
        {
            var directory = Environment.GetEnvironmentVariable("GRAVIVORE_PLAYER_FEEL_QA");
            if (string.IsNullOrEmpty(directory)) return;
            Directory.CreateDirectory(directory);
            var camera = Camera.main; var player = _scene.Root.PlayerObject.transform.position;
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var position = camera.transform.position; var rotation = camera.transform.rotation; var fov = camera.fieldOfView;
            var target = new RenderTexture(540,960,24);
            var pixels = new Texture2D(540,960,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture = target;
                // Same settled S20 gameplay camera; close capture is explicitly separate.
                camera.transform.position = player + (detail ? new Vector3(2.4f,1.8f,3.2f) : new Vector3(0,14.8f,-11.2f));
                camera.transform.LookAt(player + Vector3.up * (detail ? .8f : .9f)); camera.fieldOfView = detail ? 35 : 46;
                camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0,0,540,960),0,0); pixels.Apply();
                var rgb = pixels.GetRawTextureData();
                using (var file = File.Create(Path.Combine(directory,name + ".ppm")))
                {
                    var header = Encoding.ASCII.GetBytes("P6\n540 960\n255\n"); file.Write(header,0,header.Length);
                    for (var row = 959; row >= 0; row--) file.Write(rgb,row * 540 * 3,540 * 3);
                }
            }
            finally
            {
                camera.targetTexture = previousTarget; camera.transform.SetPositionAndRotation(position,rotation);
                camera.fieldOfView = fov; RenderTexture.active = previousActive; target.Release();
                Object.Destroy(target); Object.Destroy(pixels);
            }
        }
    }
}
