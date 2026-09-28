using System;

namespace Gravivore.Gameplay.Equipment
{
    public readonly struct EquipmentItem
    {
        public EquipmentItem(
            string id,
            string displayName,
            EquipmentSlot slot,
            in EquipmentFlatModifier modifier)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Equipment id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Equipment display name is required.", nameof(displayName));
            if (!Enum.IsDefined(typeof(EquipmentSlot), slot)) throw new ArgumentOutOfRangeException(nameof(slot));
            Id = id;
            DisplayName = displayName;
            Slot = slot;
            Modifier = modifier;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public EquipmentSlot Slot { get; }
        public EquipmentFlatModifier Modifier { get; }
    }
}
