using System;
using UnityEngine;

namespace Gravivore.Presentation.AudioVfx
{
    [CreateAssetMenu(menuName = "Gravivore/Presentation/Device correction")]
    public sealed class DeviceCorrectionDefinition : ScriptableObject
    {
        [SerializeField] private AudioClip _exploration, _combatLayer;
        [SerializeField, Range(0, 1)] private float _explorationVolume = .42f, _combatVolume = .32f;
        [SerializeField, Min(.1f)] private float _musicFadeSeconds = 2.8f;
        [SerializeField, Range(0, 1)] private float _warningMusicGain = .55f;
        [SerializeField, Min(.05f)] private float _pullDuration = .18f;
        [SerializeField, Min(.1f)] private float _pullMaximumOffset = 1.35f;
        public AudioClip Exploration => _exploration;
        public AudioClip CombatLayer => _combatLayer;
        public float ExplorationVolume => _explorationVolume;
        public float CombatVolume => _combatVolume;
        public float MusicFadeSeconds => _musicFadeSeconds;
        public float WarningMusicGain => _warningMusicGain;
        public float PullDuration => _pullDuration;
        public float PullMaximumOffset => _pullMaximumOffset;
        public void ValidateOrThrow()
        {
            if (_exploration == null || _combatLayer == null || _exploration.length < 30 || _combatLayer.length < 30 ||
                _musicFadeSeconds <= 0 || _pullDuration <= 0 || _pullDuration > .25f || _pullMaximumOffset <= 0)
                throw new InvalidOperationException("Device correction requires long music loops and bounded pull settings.");
        }
    }
}
