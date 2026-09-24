using System;

namespace Gravivore.Gameplay.Player
{
    public readonly struct PlayerDerivedStats
    {
        public PlayerDerivedStats(
            float baseDamage,
            float maxHp,
            float armorValue,
            float attackInterval,
            float moveSpeed)
        {
            ValidateNonNegative(baseDamage, nameof(baseDamage));
            ValidatePositive(maxHp, nameof(maxHp));
            ValidateNonNegative(armorValue, nameof(armorValue));
            ValidatePositive(attackInterval, nameof(attackInterval));
            ValidateNonNegative(moveSpeed, nameof(moveSpeed));

            BaseDamage = baseDamage;
            MaxHp = maxHp;
            ArmorValue = armorValue;
            AttackInterval = attackInterval;
            MoveSpeed = moveSpeed;
        }

        public float BaseDamage { get; }

        public float MaxHp { get; }

        public float ArmorValue { get; }

        public float AttackInterval { get; }

        public float MoveSpeed { get; }

        public bool HasSameValues(PlayerDerivedStats other)
        {
            return BaseDamage == other.BaseDamage &&
                   MaxHp == other.MaxHp &&
                   ArmorValue == other.ArmorValue &&
                   AttackInterval == other.AttackInterval &&
                   MoveSpeed == other.MoveSpeed;
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Derived stat values must be finite.", parameterName);
            }
        }
    }
}
