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
        BossImpact,
        Step,
        Release,
        PlayerHit,
        PlayerDeath
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
        private float[] _sourceGains;
        private S14PresentationDefinition _definition;
        private int _cursor;
        private bool _isMuted;
        private float _volume;

        public S14AudioCue? LastCue { get; private set; }
        public bool IsMuted => _isMuted;
        public float Volume => _volume;
        public int SourceCount => _sources != null ? _sources.Length : 0;
        public int PlayedCount { get; private set; }

        public void Initialize(S14PresentationDefinition definition, int sourceCount = 4)
        {
            if (_sources != null) throw new InvalidOperationException("Audio presentation already initialized.");
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _definition.ValidateOrThrow();
            if (sourceCount < 1) throw new ArgumentOutOfRangeException(nameof(sourceCount));
            _isMuted = PresentationAudioSettings.IsMuted;
            _volume = PresentationAudioSettings.Volume;
            _sources = new AudioSource[sourceCount];
            _sourceGains = new float[sourceCount];
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
            var voiceCount = Mathf.Max(1, _sources.Length - 1);
            var index = cue == S14AudioCue.Step ? _sources.Length - 1 : _cursor;
            var source = _sources[index];
            if (cue != S14AudioCue.Step) _cursor = (_cursor + 1) % voiceCount;
            // One bounded voice per source. Footsteps never overlap a combat voice.
            source.clip = clip;
            _sourceGains[index] = cue == S14AudioCue.Step ? _definition.StepVolume :
                cue == S14AudioCue.Telegraph ? .65f : .45f;
            source.volume = Volume * _sourceGains[index];
            source.Play();
            PlayedCount++;
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
            for (var i = 0; i < _sources.Length; i++) _sources[i].volume = _volume * _sourceGains[i];
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
                case S14AudioCue.Step: return _definition.StepClip;
                case S14AudioCue.Release: return _definition.ReleaseClip;
                case S14AudioCue.PlayerHit: return _definition.PlayerHitClip;
                case S14AudioCue.PlayerDeath: return _definition.PlayerDeathClip;
                default: throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
            }
        }
    }
}
