using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Equipment;

namespace Gravivore.Persistence
{
    public static class InventorySaveMapper
    {
        public static InventorySaveDto Export(InventoryState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var snapshot = state.ExportSnapshot();
            var equipped = new EquippedItemSaveDto[snapshot.EquippedItems.Length];
            for (var i = 0; i < equipped.Length; i++)
            {
                equipped[i] = new EquippedItemSaveDto
                {
                    Slot = (int)snapshot.EquippedItems[i].Slot,
                    ItemId = snapshot.EquippedItems[i].ItemId
                };
            }

            var ranks = new ItemRankSaveDto[snapshot.Ranks.Length];
            for (var i = 0; i < ranks.Length; i++) ranks[i] = new ItemRankSaveDto { ItemId = snapshot.Ranks[i].ItemId, Rank = snapshot.Ranks[i].Rank };
            return new InventorySaveDto
            {
                OwnedItemIds = (string[])snapshot.OwnedItemIds.Clone(),
                EquippedItems = equipped, ItemRanks = ranks
            };
        }

        public static InventoryState Restore(
            InventorySaveDto dto,
            EquipmentCatalog catalog,
            Action<string> optionalContentWarning = null)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (dto.OwnedItemIds == null || dto.EquippedItems == null)
            {
                throw new ArgumentException("Inventory save arrays are required.", nameof(dto));
            }

            var rawOwned = new HashSet<string>(StringComparer.Ordinal);
            var owned = new List<string>(dto.OwnedItemIds.Length);
            for (var i = 0; i < dto.OwnedItemIds.Length; i++)
            {
                var itemId = dto.OwnedItemIds[i];
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    throw new ArgumentException("Owned equipment ids must be non-empty.", nameof(dto));
                }

                if (!rawOwned.Add(itemId))
                {
                    throw new ArgumentException($"Duplicate owned equipment id: {itemId}.", nameof(dto));
                }

                if (catalog.TryGet(itemId, out _))
                {
                    owned.Add(itemId);
                }
                else
                {
                    optionalContentWarning?.Invoke($"Dropped removed equipment ownership reference '{itemId}'.");
                }
            }

            var usedSlots = new HashSet<EquipmentSlot>();
            var equipped = new List<EquippedItemSnapshot>(dto.EquippedItems.Length);
            for (var i = 0; i < dto.EquippedItems.Length; i++)
            {
                var entry = dto.EquippedItems[i] ??
                            throw new ArgumentException("Equipped save entries cannot be null.", nameof(dto));
                if (!Enum.IsDefined(typeof(EquipmentSlot), entry.Slot))
                {
                    throw new ArgumentOutOfRangeException(nameof(dto), $"Invalid equipment slot: {entry.Slot}.");
                }

                if (string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    throw new ArgumentException("Equipped equipment ids must be non-empty.", nameof(dto));
                }

                var slot = (EquipmentSlot)entry.Slot;
                if (!usedSlots.Add(slot))
                {
                    throw new ArgumentException($"Multiple items are equipped in slot {slot}.", nameof(dto));
                }

                if (!rawOwned.Contains(entry.ItemId))
                {
                    throw new ArgumentException($"Equipped item is not owned: {entry.ItemId}.", nameof(dto));
                }

                if (!catalog.TryGet(entry.ItemId, out var item))
                {
                    optionalContentWarning?.Invoke(
                        $"Dropped removed equipped item reference '{entry.ItemId}' from slot {slot}.");
                    continue;
                }

                if (item.Slot != slot)
                {
                    throw new ArgumentException($"Equipment {item.Id} is stored in the wrong slot.", nameof(dto));
                }

                equipped.Add(new EquippedItemSnapshot(slot, item.Id));
            }

            var ranks = new List<ItemRankSnapshot>();
            var rankedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var rank in dto.ItemRanks ?? Array.Empty<ItemRankSaveDto>())
            {
                if (rank == null || string.IsNullOrWhiteSpace(rank.ItemId) || !rawOwned.Contains(rank.ItemId) ||
                    !rankedIds.Add(rank.ItemId) || rank.Rank < 1 || rank.Rank > 5)
                    throw new ArgumentException("Invalid equipment rank save.", nameof(dto));
                if (!catalog.TryGet(rank.ItemId, out var item)) continue;
                if (rank.Rank > item.MaximumRank) throw new ArgumentException("Rank exceeds catalog cap.", nameof(dto));
                ranks.Add(new ItemRankSnapshot(rank.ItemId, rank.Rank));
            }
            return InventoryState.Restore(new InventoryStateSnapshot(owned.ToArray(), equipped.ToArray(), ranks.ToArray()));
        }
    }
}
