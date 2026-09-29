using System;
using UnityEngine;

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
            var finalHitPoints = CurrentHitPoints - appliedDamage;
            var wasLethal = finalHitPoints <= 0f && !_deathSignaled;
            CurrentHitPoints = finalHitPoints;
            if (wasLethal)
            {
                _deathSignaled = true;
            }

            Publish(Changed, new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
            if (wasLethal) Publish(Died);

            return new DamageResult(appliedDamage, wasLethal);
        }

        public void Reset(float maximumHitPoints)
        {
            ValidatePositive(maximumHitPoints, nameof(maximumHitPoints));
            var previousHitPoints = CurrentHitPoints;
            MaximumHitPoints = maximumHitPoints;
            CurrentHitPoints = maximumHitPoints;
            _deathSignaled = false;
            Publish(Changed, new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
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
            Publish(Changed, new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
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
                Publish(Changed, new HealthChangedEvent(previousHitPoints, CurrentHitPoints, MaximumHitPoints));
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

        private static void Publish(Action handlers)
        {
            if (handlers == null) return;
            var invocationList = handlers.GetInvocationList();
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action)invocationList[i])();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static void Publish<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            var invocationList = handlers.GetInvocationList();
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i])(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}
