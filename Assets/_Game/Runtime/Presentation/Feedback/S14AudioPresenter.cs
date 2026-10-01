using System;
using UnityEngine;

namespace Gravivore.Presentation.Feedback
{
    public enum S14AudioCue
    {
        LashWindup,
        LashImpact,
        Hit,
        Death,
        Assimilation,
        Evolution,
        Telegraph,
        BossImpact
    }

    public static class PresentationAudioSettings
    {
        private const string MutedKey = "gravivore.audio.muted";
        private const string VolumeKey = "gravivore.audio.volume";

        public static bool IsMuted => PlayerPrefs.GetInt(MutedKey, 0) != 0;
        public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.8f));

        public static void SetMuted(bool muted)
        {
            PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetVolume(float volume)
        {
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(volume));
            PlayerPrefs.Save();
        }
    }

    [DisallowMultipleComponent]
    public sealed class S14AudioPresenter : MonoBehaviour
    {
        private AudioSource[] _sources;
        private S14PresentationDefinition _definition;
        private int _cursor;
        private bool _isMuted;
        private float _volume;

        public S14AudioCue? LastCue { get; private set; }
        public bool IsMuted => _isMuted;
        public float Volume => _volume;

        public void Initialize(S14PresentationDefinition definition, int sourceCount = 3)
        {
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _definition.ValidateOrThrow();
            if (sourceCount < 1) throw new ArgumentOutOfRangeException(nameof(sourceCount));
            _isMuted = PresentationAudioSettings.IsMuted;
            _volume = PresentationAudioSettings.Volume;
            _sources = new AudioSource[sourceCount];
            for (var i = 0; i < sourceCount; i++)
            {
                var sourceObject = new GameObject($"Pooled Audio Source {i}", typeof(AudioSource));
                sourceObject.transform.SetParent(transform, false);
                var source = sourceObject.GetComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                _sources[i] = source;
            }
        }

        public void Play(S14AudioCue cue)
        {
            LastCue = cue;
            if (IsMuted || Volume <= 0f) return;
            var clip = Resolve(cue);
            var source = _sources[_cursor];
            _cursor = (_cursor + 1) % _sources.Length;
            source.volume = Volume;
            source.PlayOneShot(clip);
        }

        public void SetMuted(bool muted)
        {
            _isMuted = muted;
            PresentationAudioSettings.SetMuted(muted);
            if (muted) StopAll();
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            PresentationAudioSettings.SetVolume(_volume);
            for (var i = 0; i < _sources.Length; i++) _sources[i].volume = _volume;
        }

        private void StopAll()
        {
            for (var i = 0; i < _sources.Length; i++) _sources[i].Stop();
        }

        private AudioClip Resolve(S14AudioCue cue)
        {
            switch (cue)
            {
                case S14AudioCue.LashWindup: return _definition.LashWindupClip;
                case S14AudioCue.LashImpact: return _definition.LashImpactClip;
                case S14AudioCue.Hit: return _definition.HitClip;
                case S14AudioCue.Death: return _definition.DeathClip;
                case S14AudioCue.Assimilation: return _definition.AssimilationClip;
                case S14AudioCue.Evolution: return _definition.EvolutionClip;
                case S14AudioCue.Telegraph: return _definition.TelegraphClip;
                case S14AudioCue.BossImpact: return _definition.BossImpactClip;
                default: throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
            }
        }
    }
}
