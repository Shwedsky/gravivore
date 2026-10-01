using System;
using UnityEngine;

namespace Gravivore.Presentation.Feedback
{
    public enum HapticCue
    {
        LightImpact,
        HeavyImpact,
        Evolution
    }

    public interface IHapticFeedback
    {
        void Play(HapticCue cue);
    }

    public interface IHapticSettings
    {
        bool Enabled { get; }
    }

    public interface IUnscaledTimeSource
    {
        float Time { get; }
    }

    public sealed class PresentationHapticSettings : IHapticSettings
    {
        public const string EnabledKey = "gravivore.haptics.enabled";

        public PresentationHapticSettings()
        {
            Enabled = PlayerPrefs.GetInt(EnabledKey, 1) != 0;
        }

        public bool Enabled { get; private set; }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            PlayerPrefs.SetInt(EnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public sealed class UnityUnscaledTimeSource : IUnscaledTimeSource
    {
        public float Time => UnityEngine.Time.unscaledTime;
    }

    public sealed class ThrottledHapticFeedback : IHapticFeedback
    {
        private readonly IHapticFeedback _inner;
        private readonly IHapticSettings _settings;
        private readonly IUnscaledTimeSource _time;
        private readonly float _minimumInterval;
        private float _lastPlayedAt = float.NegativeInfinity;

        public ThrottledHapticFeedback(
            IHapticFeedback inner,
            IHapticSettings settings,
            IUnscaledTimeSource time,
            float minimumInterval = 0.12f)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            if (float.IsNaN(minimumInterval) || float.IsInfinity(minimumInterval) || minimumInterval < 0f)
                throw new ArgumentOutOfRangeException(nameof(minimumInterval));
            _minimumInterval = minimumInterval;
        }

        public void Play(HapticCue cue)
        {
            if (!_settings.Enabled) return;
            var now = _time.Time;
            if (now - _lastPlayedAt < _minimumInterval) return;
            _lastPlayedAt = now;
            _inner.Play(cue);
        }
    }

    public sealed class PlatformHapticFeedback : IHapticFeedback
    {
        public void Play(HapticCue cue)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
