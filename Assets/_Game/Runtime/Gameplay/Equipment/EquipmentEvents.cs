using System;

namespace Gravivore.Gameplay.Equipment
{
    public enum EquipmentGrantSource
    {
        Development = 0,
        GameplayReward = 1
    }

    public readonly struct EquipmentGrantedEvent
    {
        public EquipmentGrantedEvent(string itemId, EquipmentGrantSource source)
        {
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Equipment id is required.", nameof(itemId));
            if (!Enum.IsDefined(typeof(EquipmentGrantSource), source)) throw new ArgumentOutOfRangeException(nameof(source));
            ItemId = itemId;
            Source = source;
        }

        public string ItemId { get; }
        public EquipmentGrantSource Source { get; }
    }

    public readonly struct EquipmentChangedEvent
    {
        public EquipmentChangedEvent(EquipmentSlot slot, string previousItemId, string currentItemId)
        {
            if (!Enum.IsDefined(typeof(EquipmentSlot), slot)) throw new ArgumentOutOfRangeException(nameof(slot));
            if (previousItemId == null && currentItemId == null)
            {
                throw new ArgumentException("An equipment change requires a previous or current item.");
            }

            Slot = slot;
            PreviousItemId = previousItemId;
            CurrentItemId = currentItemId;
        }

        public EquipmentSlot Slot { get; }
        public string PreviousItemId { get; }
        public string CurrentItemId { get; }
    }

    public enum InventoryChangeType
    {
        ItemGranted = 0,
        EquipmentChanged = 1
    }

    public readonly struct InventoryChangedEvent
    {
        public InventoryChangedEvent(InventoryChangeType changeType)
        {
            if (!Enum.IsDefined(typeof(InventoryChangeType), changeType)) throw new ArgumentOutOfRangeException(nameof(changeType));
            ChangeType = changeType;
        }

        public InventoryChangeType ChangeType { get; }
    }
}
