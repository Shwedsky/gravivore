using System;

namespace Gravivore.Gameplay.Combat
{
    public sealed class AttackCadenceTimer
    {
        private float _remainingCooldown;

        public float RemainingCooldown => _remainingCooldown;

        public void Reset()
        {
            _remainingCooldown = 0f;
        }

        public bool Advance(float deltaTime, bool hasValidTarget, float attackInterval)
        {
            ValidateFiniteNonNegative(deltaTime, nameof(deltaTime));
            if (float.IsNaN(attackInterval) || float.IsInfinity(attackInterval) || attackInterval <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(attackInterval));
            }

            if (!hasValidTarget)
            {
                _remainingCooldown = 0f;
                return false;
            }

            _remainingCooldown = Math.Max(0f, _remainingCooldown - deltaTime);
            if (_remainingCooldown > 0f)
            {
                return false;
            }

            _remainingCooldown = attackInterval;
            return true;
        }

        private static void ValidateFiniteNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
