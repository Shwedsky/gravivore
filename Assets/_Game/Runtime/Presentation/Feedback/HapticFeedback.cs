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
