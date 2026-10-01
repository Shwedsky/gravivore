using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Gravivore.Gameplay.Combat;
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
        public IEnumerator GravityLashPool_ReusesSequenceAndSeparatesWindupBeamImpact()
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
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: lash presentation failed"));
            pool.Tick(settings.VfxDuration);
            pool.Tick(settings.VfxDuration * 2f);
            Assert.That(cues, Is.EqualTo(new[] { GravityLashCue.Windup, GravityLashCue.Beam, GravityLashCue.Impact }));
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
            public void Play(HapticCue cue) => LastCue = cue;
        }
    }
}
