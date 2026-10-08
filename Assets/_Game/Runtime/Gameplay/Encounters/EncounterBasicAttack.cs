using System;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    [Serializable]
    public sealed class EncounterBasicAttackSettings
    {
        [SerializeField, Min(0.1f)] private float _damage = 18f;
        [SerializeField, Min(0.1f)] private float _range = 8f;
        [SerializeField, Min(0.1f)] private float _interval = 1.25f;
        [SerializeField, Min(0.1f)] private float _windup = .35f;
        public EncounterBasicAttackConfiguration Configuration => new EncounterBasicAttackConfiguration(_damage, _range, _interval, _windup);
    }

    public readonly struct EncounterBasicAttackConfiguration
    {
        public EncounterBasicAttackConfiguration(float damage, float range, float interval, float windup)
        {
            foreach (var value in new[] { damage, range, interval, windup })
                if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (windup >= interval) throw new ArgumentException("Basic windup must be shorter than cadence.");
            Damage = damage; Range = range; Interval = interval; Windup = windup;
        }
        public float Damage { get; }
        public float Range { get; }
        public float Interval { get; }
        public float Windup { get; }
        public bool Enabled => Damage > 0;
    }

    public readonly struct EncounterBasicAttackEvent
    {
        public EncounterBasicAttackEvent(Vector3 source, Vector3 target, float duration, bool hit = false)
        { Source = source; Target = target; Duration = duration; Hit = hit; }
        public Vector3 Source { get; }
        public Vector3 Target { get; }
        public float Duration { get; }
        public bool Hit { get; }
    }

    /// <summary>One bounded basic attack, never a catch-up burst. Specials can cancel the windup.</summary>
    public sealed class EncounterBasicAttackCadence
    {
        private readonly EncounterBasicAttackConfiguration _configuration;
        private float _cooldown, _windup;
        public EncounterBasicAttackCadence(EncounterBasicAttackConfiguration configuration) => _configuration = configuration;
        public bool IsCharging { get; private set; }
        public bool Began { get; private set; }
        public bool Resolved { get; private set; }
        public bool Hit { get; private set; }
        public void Tick(float dt, float distance, bool allowed)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0 || float.IsNaN(distance) || float.IsInfinity(distance) || distance < 0)
                throw new ArgumentOutOfRangeException(nameof(dt));
            Began = Resolved = Hit = false;
            _cooldown = Mathf.Max(0, _cooldown - dt);
            if (!allowed || !_configuration.Enabled) { IsCharging = false; return; }
            if (IsCharging)
            {
                _windup = Mathf.Max(0, _windup - dt);
                if (_windup > 0) return;
                IsCharging = false; Resolved = true; Hit = distance <= _configuration.Range;
                return;
            }
            if (_cooldown > 0 || distance > _configuration.Range) return;
            IsCharging = Began = true; _windup = _configuration.Windup; _cooldown = _configuration.Interval;
        }
        public void Reset() { _cooldown = _windup = 0; IsCharging = Began = Resolved = Hit = false; }
    }
}
