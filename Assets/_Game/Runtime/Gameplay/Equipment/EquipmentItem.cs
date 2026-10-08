using System;

namespace Gravivore.Gameplay.Equipment
{
    public readonly struct EquipmentItem
    {
        public EquipmentItem(
            string id,
            string displayName,
            EquipmentSlot slot,
            in EquipmentFlatModifier modifier, int maximumRank = 1, float damagePerRank = 0f)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Equipment id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Equipment display name is required.", nameof(displayName));
            if (!Enum.IsDefined(typeof(EquipmentSlot), slot)) throw new ArgumentOutOfRangeException(nameof(slot));
            Id = id;
            DisplayName = displayName;
            Slot = slot;
            if (maximumRank < 1 || maximumRank > 5 || float.IsNaN(damagePerRank) || float.IsInfinity(damagePerRank) || damagePerRank < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumRank));
            Modifier = modifier; MaximumRank = maximumRank; DamagePerRank = damagePerRank;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public EquipmentSlot Slot { get; }
        public EquipmentFlatModifier Modifier { get; }
        public int MaximumRank { get; }
        public float DamagePerRank { get; }
        public EquipmentFlatModifier ModifierAtRank(int rank)
        {
            if (rank < 1 || rank > MaximumRank) throw new ArgumentOutOfRangeException(nameof(rank));
            return new EquipmentFlatModifier(Modifier.BaseDamage + (rank - 1) * DamagePerRank,
                Modifier.MaxHp, Modifier.ArmorValue, Modifier.AttackInterval, Modifier.MoveSpeed);
        }
    }
}
