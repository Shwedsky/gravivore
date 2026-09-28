using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Equipment
{
    public readonly struct EquippedItemSnapshot
    {
        public EquippedItemSnapshot(EquipmentSlot slot, string itemId)
        {
            if (!Enum.IsDefined(typeof(EquipmentSlot), slot)) throw new ArgumentOutOfRangeException(nameof(slot));
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Equipped item id is required.", nameof(itemId));
            Slot = slot;
            ItemId = itemId;
        }

        public EquipmentSlot Slot { get; }
        public string ItemId { get; }
    }

    public sealed class InventoryStateSnapshot
    {
        public InventoryStateSnapshot(string[] ownedItemIds, EquippedItemSnapshot[] equippedItems)
        {
            OwnedItemIds = ownedItemIds != null ? (string[])ownedItemIds.Clone() : throw new ArgumentNullException(nameof(ownedItemIds));
            EquippedItems = equippedItems != null ? (EquippedItemSnapshot[])equippedItems.Clone() : throw new ArgumentNullException(nameof(equippedItems));
        }

        public string[] OwnedItemIds { get; }
        public EquippedItemSnapshot[] EquippedItems { get; }
    }

    public sealed class InventoryState
    {
        private const int SlotCount = 3;
        private readonly SortedSet<string> _ownedItemIds = new SortedSet<string>(StringComparer.Ordinal);
        private readonly string[] _equippedItemIds = new string[SlotCount];

        public int OwnedCount => _ownedItemIds.Count;

        public bool HasItem(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && _ownedItemIds.Contains(itemId);

        public bool TryGetEquipped(EquipmentSlot slot, out string itemId)
        {
            ValidateSlot(slot);
            itemId = _equippedItemIds[(int)slot];
            return itemId != null;
        }

        public InventoryStateSnapshot ExportSnapshot()
        {
            var owned = new string[_ownedItemIds.Count];
            _ownedItemIds.CopyTo(owned);
            var equippedCount = 0;
            for (var i = 0; i < _equippedItemIds.Length; i++)
            {
                if (_equippedItemIds[i] != null) equippedCount++;
            }

            var equipped = new EquippedItemSnapshot[equippedCount];
            var destination = 0;
            for (var i = 0; i < _equippedItemIds.Length; i++)
            {
                if (_equippedItemIds[i] == null) continue;
                equipped[destination++] = new EquippedItemSnapshot((EquipmentSlot)i, _equippedItemIds[i]);
            }

            return new InventoryStateSnapshot(owned, equipped);
        }

        public static InventoryState Restore(InventoryStateSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var state = new InventoryState();
            for (var i = 0; i < snapshot.OwnedItemIds.Length; i++)
            {
                var itemId = snapshot.OwnedItemIds[i];
                ValidateItemId(itemId);
                if (!state._ownedItemIds.Add(itemId)) throw new ArgumentException($"Duplicate owned equipment id: {itemId}.", nameof(snapshot));
            }

            for (var i = 0; i < snapshot.EquippedItems.Length; i++)
            {
                var equipped = snapshot.EquippedItems[i];
                ValidateSlot(equipped.Slot);
                ValidateItemId(equipped.ItemId);
                if (!state._ownedItemIds.Contains(equipped.ItemId))
                {
                    throw new ArgumentException($"Equipped item is not owned: {equipped.ItemId}.", nameof(snapshot));
                }

                if (state._equippedItemIds[(int)equipped.Slot] != null)
                {
                    throw new ArgumentException($"Multiple items are equipped in slot {equipped.Slot}.", nameof(snapshot));
                }

                state._equippedItemIds[(int)equipped.Slot] = equipped.ItemId;
            }

            return state;
        }

        internal bool Grant(string itemId)
        {
            ValidateItemId(itemId);
            return _ownedItemIds.Add(itemId);
        }

        internal string SetEquipped(EquipmentSlot slot, string itemId)
        {
            ValidateSlot(slot);
            ValidateItemId(itemId);
            if (!_ownedItemIds.Contains(itemId)) throw new InvalidOperationException($"Cannot equip unowned item: {itemId}.");
            var previous = _equippedItemIds[(int)slot];
            _equippedItemIds[(int)slot] = itemId;
            return previous;
        }

        internal string ClearEquipped(EquipmentSlot slot)
        {
            ValidateSlot(slot);
            var previous = _equippedItemIds[(int)slot];
            _equippedItemIds[(int)slot] = null;
            return previous;
        }

        private static void ValidateSlot(EquipmentSlot slot)
        {
            if (!Enum.IsDefined(typeof(EquipmentSlot), slot)) throw new ArgumentOutOfRangeException(nameof(slot));
        }

        private static void ValidateItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Equipment id is required.", nameof(itemId));
        }
    }
}
