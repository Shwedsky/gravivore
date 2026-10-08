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

    public readonly struct ItemRankSnapshot
    {
        public ItemRankSnapshot(string itemId, int rank) { ItemId = itemId; Rank = rank; }
        public string ItemId { get; }
        public int Rank { get; }
    }
    public sealed class InventoryStateSnapshot
    {
        public InventoryStateSnapshot(string[] ownedItemIds, EquippedItemSnapshot[] equippedItems, ItemRankSnapshot[] ranks = null)
        {
            Ranks = ranks != null ? (ItemRankSnapshot[])ranks.Clone() : Array.Empty<ItemRankSnapshot>();
            OwnedItemIds = ownedItemIds != null ? (string[])ownedItemIds.Clone() : throw new ArgumentNullException(nameof(ownedItemIds));
            EquippedItems = equippedItems != null ? (EquippedItemSnapshot[])equippedItems.Clone() : throw new ArgumentNullException(nameof(equippedItems));
        }

        public ItemRankSnapshot[] Ranks { get; }
        public string[] OwnedItemIds { get; }
        public EquippedItemSnapshot[] EquippedItems { get; }
    }

    public sealed class InventoryState
    {
        private const int SlotCount = 4;
        private readonly Dictionary<string,int> _ranks = new Dictionary<string,int>(StringComparer.Ordinal);
        public int GetRank(string itemId) => _ranks.TryGetValue(itemId, out var rank) ? rank : HasItem(itemId) ? 1 : 0;
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

            var ranks = new ItemRankSnapshot[owned.Length];
            for (var i = 0; i < owned.Length; i++) ranks[i] = new ItemRankSnapshot(owned[i], GetRank(owned[i]));
            return new InventoryStateSnapshot(owned, equipped, ranks);
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

            foreach (var entry in snapshot.Ranks)
            {
                if (!state.HasItem(entry.ItemId) || entry.Rank < 1 || entry.Rank > 5 || state._ranks.ContainsKey(entry.ItemId))
                    throw new ArgumentException("Invalid, duplicate or unowned equipment rank.", nameof(snapshot));
                state._ranks.Add(entry.ItemId, entry.Rank);
            }
            return state;
        }

        internal bool Grant(string itemId)
        {
            ValidateItemId(itemId);
            return _ownedItemIds.Add(itemId);
        }

        internal bool IncreaseRank(string itemId, int maximum)
        {
            if (!HasItem(itemId)) throw new InvalidOperationException("Ranked item must be owned.");
            var rank = GetRank(itemId); if (rank >= maximum) return false;
            _ranks[itemId] = rank + 1; return true;
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
