using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Encounters;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Feedback;
using UnityEngine;

namespace Gravivore.Presentation.AudioVfx
{
    /// <summary>Two phase-matched original stems. Observes combat and never writes it.</summary>
    public sealed class IndustrialMusicPresenter : MonoBehaviour
    {
        private S01SceneCompositionRoot _root;
        private GravityLashVfxPool _lash;
        private DeviceCorrectionDefinition _definition;
        private AudioSource _exploration, _combat;
        private float _combatHold, _warningHold, _intensity, _fade, _duck = 1;
        private bool _paused;
        public bool IsInitialized => _exploration != null && _combat != null;
        public float Intensity => _intensity;
        public AudioSource ExplorationSource => _exploration;
        public AudioSource CombatSource => _combat;

        public void Initialize(S01SceneCompositionRoot root, GravityLashVfxPool lash, DeviceCorrectionDefinition definition)
        {
            definition.ValidateOrThrow(); _root = root; _lash = lash; _definition = definition;
            _exploration = Stem("Industrial Exploration", definition.Exploration);
            _combat = Stem("Industrial Combat Layer", definition.CombatLayer);
            var start = AudioSettings.dspTime + .2;
            _exploration.PlayScheduled(start); _combat.PlayScheduled(start);
            lash.CuePlayed += Lash;
            root.PlayerHealth.Damaged += Hit;
            root.MagnetarGuard.TelegraphStarted += EliteWarning;
            root.CustodianBoss.TelegraphStarted += BossWarning;
        }
        private AudioSource Stem(string label, AudioClip clip)
        {
            var obj = new GameObject(label, typeof(AudioSource)); obj.transform.SetParent(transform, false);
            var source = obj.GetComponent<AudioSource>(); source.clip = clip; source.loop = true;
            source.playOnAwake = false; source.spatialBlend = 0; source.volume = 0; source.priority = 180;
            return source;
        }
        private void Lash(GravityLashCue cue, Vector3 position) { if (cue == GravityLashCue.Beam) _combatHold = 6; }
        private void Hit(DamageResult damage) => _combatHold = 6;
        private void EliteWarning(EliteShockwaveTelegraphEvent value) { _combatHold = 6; _warningHold = value.Duration + .25f; }
        private void BossWarning(BossTelegraphEvent value) { _combatHold = 6; _warningHold = value.Duration + .25f; }
        private void Update() => Tick(Time.unscaledDeltaTime);
        public void Tick(float deltaTime)
        {
            if (!IsInitialized || _paused) return;
            var dt = Mathf.Max(0, deltaTime);
            _combatHold = Mathf.Max(0, _combatHold - dt); _warningHold = Mathf.Max(0, _warningHold - dt);
            var fighting = _combatHold > 0 || _root.CustodianBoss.CanBeTargeted || _root.MagnetarGuard.IsEncounterActive;
            _intensity = Mathf.MoveTowards(_intensity, fighting ? 1 : 0, dt / _definition.MusicFadeSeconds);
            _fade = Mathf.MoveTowards(_fade, 1, dt / _definition.MusicFadeSeconds);
            _duck = Mathf.MoveTowards(_duck, _warningHold > 0 ? _definition.WarningMusicGain : 1, dt * 3);
            var volume = PresentationAudioSettings.IsMuted ? 0 : PresentationAudioSettings.Volume * _fade * _duck;
            _exploration.volume = volume * _definition.ExplorationVolume * Mathf.Lerp(1,.78f,_intensity);
            _combat.volume = volume * _definition.CombatVolume * _intensity;
        }
        private void OnApplicationPause(bool pause)
        {
            _paused = pause; if (!IsInitialized) return;
            if (pause) { _exploration.Pause(); _combat.Pause(); }
            else { _fade = 0; _exploration.UnPause(); _combat.UnPause(); }
        }
        private void OnDestroy()
        {
            if (_lash != null) _lash.CuePlayed -= Lash;
            if (_root != null)
            {
                _root.PlayerHealth.Damaged -= Hit;
                _root.MagnetarGuard.TelegraphStarted -= EliteWarning;
                _root.CustodianBoss.TelegraphStarted -= BossWarning;
            }
            if (_exploration != null) _exploration.Stop(); if (_combat != null) _combat.Stop();
        }
    }
}
