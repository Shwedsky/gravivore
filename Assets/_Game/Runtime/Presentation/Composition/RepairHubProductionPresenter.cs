using System;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.AudioVfx;
using UnityEngine;

namespace Gravivore.Presentation.Composition
{
    [DisallowMultipleComponent]
    public sealed class RepairHubProductionPresenter : MonoBehaviour
    {
        private PlayerHealthController _health;
        private Vector3 _repairPosition;
        private float _radiusSquared;
        private float _interval;
        private float _groundEffectVisualLift;
        private float _remaining;
        private Phase6BAudioPlayer _audio;
        private Phase6BVfxPool _vfx;
        public bool IsRepairing { get; private set; }
        public Phase6BVfxPool Vfx => _vfx;

        public void Initialize(PlayerHealthController health, Vector3 repairPosition,
            PlayerRecoveryConfiguration recovery, Phase6BProductionDefinition definition)
        {
            _health = health != null ? health : throw new ArgumentNullException(nameof(health));
            _repairPosition = repairPosition;
            _radiusSquared = recovery.RespawnZoneRadius * recovery.RespawnZoneRadius;
            _interval = definition.RepairPresentationInterval;
            _groundEffectVisualLift = definition.GroundEffectVisualLift;
            _audio = gameObject.AddComponent<Phase6BAudioPlayer>();
            _audio.Initialize(definition.AudioBank, 1);
            _vfx = gameObject.AddComponent<Phase6BVfxPool>();
            _vfx.Initialize(definition.CreateRepairBindings());
            _health.Healed += HandleHealed;
            _health.Died += HandleDeath;
        }

        private bool InRepairZone()
        {
            var offset = _health.transform.position - _repairPosition;
            offset.y = 0f;
            return offset.sqrMagnitude <= _radiusSquared;
        }

        private void HandleHealed(float amount)
        {
            if (amount <= 0f || !InRepairZone()) return;
            if (!IsRepairing)
            {
                IsRepairing = true;
                _remaining = 0f;
                _audio.TryPlay(Phase6BAudioCue.RepairHub, _repairPosition);
            }
            if (_remaining > 0f) return;
            var target = _health.transform.position + Vector3.up * 0.8f;
            _vfx.TryPlay(Phase6BVfxCue.RepairBeam, _repairPosition + Vector3.up, target, _interval);
            _vfx.TryPlay(Phase6BVfxCue.WeldingSparks, target, target, _interval);
            _vfx.TryPlay(Phase6BVfxCue.ScannerSweep, _repairPosition + Vector3.up * _groundEffectVisualLift, target, _interval);
            _remaining = _interval;
        }

        private void Update()
        {
            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
            if (IsRepairing && (!_health.IsAlive || !InRepairZone() || _health.CombatSecondsRemaining > 0f ||
                _health.CurrentHitPoints >= _health.MaximumHitPoints)) StopPresentation();
        }

        private void HandleDeath(PlayerDeathEvent value) => StopPresentation();
        public void StopPresentation()
        {
            IsRepairing = false;
            _remaining = 0f;
            _audio?.StopAll();
            _vfx?.StopAll();
        }
        private void OnDisable() => StopPresentation();
        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Healed -= HandleHealed;
                _health.Died -= HandleDeath;
            }
            StopPresentation();
        }
    }
}
