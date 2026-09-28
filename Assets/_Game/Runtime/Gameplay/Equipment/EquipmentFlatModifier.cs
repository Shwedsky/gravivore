using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Gameplay.Equipment
{
    [Serializable]
    public readonly struct EquipmentFlatModifier : IPlayerDerivedStatsModifier
    {
        public EquipmentFlatModifier(
            float baseDamage,
            float maxHp,
            float armorValue,
            float attackInterval,
            float moveSpeed)
        {
            ValidateFinite(baseDamage, nameof(baseDamage));
            ValidateFinite(maxHp, nameof(maxHp));
            ValidateFinite(armorValue, nameof(armorValue));
            ValidateFinite(attackInterval, nameof(attackInterval));
            ValidateFinite(moveSpeed, nameof(moveSpeed));
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

        public PlayerDerivedStats Apply(in PlayerDerivedStats currentValues)
        {
            return new PlayerDerivedStats(
                Math.Max(0f, currentValues.BaseDamage + BaseDamage),
                Math.Max(float.Epsilon, currentValues.MaxHp + MaxHp),
                Math.Max(0f, currentValues.ArmorValue + ArmorValue),
                Math.Max(float.Epsilon, currentValues.AttackInterval + AttackInterval),
                Math.Max(0f, currentValues.MoveSpeed + MoveSpeed));
        }

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentException("Equipment modifiers must be finite.", name);
        }
    }
}
