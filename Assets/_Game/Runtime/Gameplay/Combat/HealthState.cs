using System;

namespace Gravivore.Gameplay.Combat
{
    public readonly struct HealthChangedEvent
    {
        public HealthChangedEvent(float previousHitPoints, float currentHitPoints, float maximumHitPoints)
        {
            PreviousHitPoints = previousHitPoints;
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
        }

        public float PreviousHitPoints { get; }

        public float CurrentHitPoints { get; }

        public float MaximumHitPoints { get; }
    }

    public sealed class HealthState
    {
        private bool _deathSignaled;

        public event Action<HealthChangedEvent> Changed;

        public event Action Died;

        public float CurrentHitPoints { get; private set; }

        public float MaximumHitPoints { get; private set; }

        public bool IsAlive => CurrentHitPoints > 0f;

        public DamageResult ApplyDamage(in DamageRequest request, float armor)
        {
            var resolvedDamage = DamageResolver.ResolvePhysicalDamage(request.RawDamage, armor);
            if (!IsAlive)
            {
                return new DamageResult(0f, false);
            }

            var previousHitPoints = CurrentHitPoints;
            var appliedDamage = Math.Min(CurrentHitPoints, resolvedDamage);
            CurrentHitPoints -= appliedDamage;
            Changed?.Invoke(new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));

            var wasLethal = !IsAlive && !_deathSignaled;
            if (wasLethal)
            {
                _deathSignaled = true;
                Died?.Invoke();
            }

            return new DamageResult(appliedDamage, wasLethal);
        }

        public void Reset(float maximumHitPoints)
        {
            ValidatePositive(maximumHitPoints, nameof(maximumHitPoints));
            var previousHitPoints = CurrentHitPoints;
            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = maximumHitPoints;
            _deathSignaled = false;
            Changed?.Invoke(new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
        }

        public void SetMaximum(float maximumHitPoints)
        {
            ValidatePositive(maximumHitPoints, nameof(maximumHitPoints));
            if (MaximumHitPoints == maximumHitPoints)
            {
                return;
            }

            var previousHitPoints = CurrentHitPoints;
            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = Math.Min(CurrentHitPoints, MaximumHitPoints);
            Changed?.Invoke(new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
        }

        public void HealToFull()
        {
            if (MaximumHitPoints <= 0f)
            {
                throw new InvalidOperationException("Health must be initialized before healing.");
            }

            var previousHitPoints = CurrentHitPoints;
            CurrentHitPoints = MaximumHitPoints;
            if (previousHitPoints != CurrentHitPoints)
            {
                Changed?.Invoke(new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
            }
        }

        public void MarkInactive()
        {
            CurrentHitPoints = 0f;
            _deathSignaled = true;
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
