using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Feedback;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class S14PresentationSmokeTests
    {
        [UnityTest]
        public IEnumerator GravityLashPool_ShowsBeamImmediatelyThenReusesForImpactTail()
        {
            var root = new GameObject("Lash Pool Test", typeof(GravityLashVfxPool));
            var settings = ScriptableObject.CreateInstance<GravityAttackSettings>();
            SetField(settings, "_targetLayers", (LayerMask)(1 << 9));
            SetField(settings, "_hardBlockerLayers", (LayerMask)(1 << 8));
            SetField(settings, "_vfxPoolSize", 1);
            var pool = root.GetComponent<GravityLashVfxPool>();
            pool.Initialize(settings, TestMaterialFactory.Lit);
            var cues = new System.Collections.Generic.List<GravityLashCue>();
            pool.CuePlayed += (_, _) => throw new System.InvalidOperationException("lash presentation failed");
            pool.CuePlayed += (cue, _) => cues.Add(cue);

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: lash presentation failed"));
            pool.Play(Vector3.zero, Vector3.forward);
            var first = pool.LastPlayedObject;
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: lash presentation failed"));
            pool.Tick(settings.VfxDuration);
            pool.Tick(settings.VfxDuration * 2f);
            Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Beam, GravityLashCue.Impact }));
            Assert.That(pool.ActiveCount, Is.Zero);

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: lash presentation failed"));
            pool.Play(Vector3.right, Vector3.forward * 2f);
            Assert.AreSame(first, pool.LastPlayedObject);
            Assert.That(pool.ActiveCount, Is.EqualTo(1));
            Object.Destroy(root);
            Object.Destroy(settings);
            yield return null;
        }

        [UnityTest]
        public IEnumerator HitImpactAndAssimilationPools_ReuseAndResetMotionTarget()
        {
            yield return VerifyPulseReuse("Hit", 0.1f, 0.2f, 0.5f, false);
            yield return VerifyPulseReuse("Impact", 0.12f, 0.3f, 1.1f, false);
            yield return VerifyPulseReuse("Assimilation", 0.2f, 0.2f, 0.05f, true);
        }

        [Test]
        public void AudioSettings_ClampVolumeAndPersistMute()
        {
            var previousMute = PresentationAudioSettings.IsMuted;
            var previousVolume = PresentationAudioSettings.Volume;
            try
            {
                PresentationAudioSettings.SetMuted(true);
                PresentationAudioSettings.SetVolume(2f);
                Assert.IsTrue(PresentationAudioSettings.IsMuted);
                Assert.That(PresentationAudioSettings.Volume, Is.EqualTo(1f));
                PresentationAudioSettings.SetVolume(-1f);
                Assert.That(PresentationAudioSettings.Volume, Is.Zero);
            }
            finally
            {
                PresentationAudioSettings.SetMuted(previousMute);
                PresentationAudioSettings.SetVolume(previousVolume);
            }
        }

        [Test]
        public void HapticContract_AcceptsPresentationCueWithoutGameplayDependency()
        {
            IHapticFeedback feedback = new RecordingHaptics();
            feedback.Play(HapticCue.Evolution);
            Assert.That(((RecordingHaptics)feedback).LastCue, Is.EqualTo(HapticCue.Evolution));
        }

        [Test]
        public void HapticThrottle_RapidEventsProduceOneVibration()
        {
            var recording = new RecordingHaptics();
            var settings = new FakeHapticSettings { Enabled = true };
            var time = new FakeUnscaledTimeSource { Time = 10f };
            var feedback = new ThrottledHapticFeedback(recording, settings, time, 0.12f);

            feedback.Play(HapticCue.LightImpact);
            time.Time = 10.05f;
            feedback.Play(HapticCue.HeavyImpact);
            time.Time = 10.13f;
            feedback.Play(HapticCue.HeavyImpact);

            Assert.That(recording.Count, Is.EqualTo(2));
        }

        [Test]
        public void HapticThrottle_DisabledSettingProducesNoVibration()
        {
            var recording = new RecordingHaptics();
            var feedback = new ThrottledHapticFeedback(
                recording,
                new FakeHapticSettings { Enabled = false },
                new FakeUnscaledTimeSource(),
                0.12f);

            feedback.Play(HapticCue.HeavyImpact);

            Assert.That(recording.Count, Is.Zero);
        }

        [Test]
        public void PlayerHealthDamage_IsSingleDamageHapticOwner()
        {
            var player = new GameObject("Haptic Owner Player", typeof(CharacterController), typeof(PlayerHealthController));
            try
            {
                var health = player.GetComponent<PlayerHealthController>();
                health.Initialize(player.GetComponent<CharacterController>(), CreateStats(), Vector3.zero, 0f);
                var recording = new RecordingHaptics();
                var feedback = new ThrottledHapticFeedback(
                    recording,
                    new FakeHapticSettings { Enabled = true },
                    new FakeUnscaledTimeSource { Time = 1f });
                health.Damaged += damage => feedback.Play(
                    damage.WasLethal ? HapticCue.HeavyImpact : HapticCue.LightImpact);

                health.ApplyDamage(new DamageRequest(5f, DamageType.Physical));

                Assert.That(recording.Count, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        private static IEnumerator VerifyPulseReuse(
            string name,
            float duration,
            float startScale,
            float endScale,
            bool moving)
        {
            var root = new GameObject($"{name} Pool Test", typeof(PooledPulseVfx));
            var target = new GameObject($"{name} Target");
            target.transform.position = Vector3.forward * 4f;
            var pool = root.GetComponent<PooledPulseVfx>();
            pool.Initialize(1, TestMaterialFactory.Lit);
            pool.Play(Vector3.zero, duration, startScale, endScale, moving ? target.transform : null);
            var first = pool.LastPlayedObject;
            pool.Tick(duration);
            Assert.That(pool.ActiveCount, Is.Zero);
            Assert.IsFalse(first.activeSelf);
            pool.Play(Vector3.right, duration, startScale, endScale);
            Assert.AreSame(first, pool.LastPlayedObject);
            Assert.That(first.transform.position, Is.EqualTo(Vector3.right));
            Object.Destroy(root);
            Object.Destroy(target);
            yield return null;
        }

        private static void SetField<T>(object target, string name, T value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private sealed class RecordingHaptics : IHapticFeedback
        {
            public HapticCue LastCue { get; private set; }
            public int Count { get; private set; }
            public void Play(HapticCue cue)
            {
                LastCue = cue;
                Count++;
            }
        }

        private static PlayerStatsState CreateStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 100f, 1f, 0f, 10f, 1000f),
                new StatCurve(10, 0f, 1f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, 0f, 0f, 0.2f, 10f),
                new StatCurve(10, 4f, 0f, 0f, 0f, 20f),
                0.2f,
                20f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private sealed class FakeHapticSettings : IHapticSettings
        {
            public bool Enabled { get; set; }
        }

        private sealed class FakeUnscaledTimeSource : IUnscaledTimeSource
        {
            public float Time { get; set; }
        }
    }
}
