using System;
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

            return new InventorySaveDto
            {
                OwnedItemIds = (string[])snapshot.OwnedItemIds.Clone(),
                EquippedItems = equipped
            };
        }

        public static InventoryState Restore(InventorySaveDto dto, EquipmentCatalog catalog)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (dto.OwnedItemIds == null || dto.EquippedItems == null)
            {
                throw new ArgumentException("Inventory save arrays are required.", nameof(dto));
            }

            var owned = (string[])dto.OwnedItemIds.Clone();
            for (var i = 0; i < owned.Length; i++) catalog.GetRequired(owned[i]);

            var equipped = new EquippedItemSnapshot[dto.EquippedItems.Length];
            for (var i = 0; i < equipped.Length; i++)
            {
                var entry = dto.EquippedItems[i] ??
                            throw new ArgumentException("Equipped save entries cannot be null.", nameof(dto));
                if (!Enum.IsDefined(typeof(EquipmentSlot), entry.Slot))
                {
                    throw new ArgumentOutOfRangeException(nameof(dto), $"Invalid equipment slot: {entry.Slot}.");
                }

                var slot = (EquipmentSlot)entry.Slot;
                var item = catalog.GetRequired(entry.ItemId);
                if (item.Slot != slot)
                {
                    throw new ArgumentException($"Equipment {item.Id} is stored in the wrong slot.", nameof(dto));
                }

                equipped[i] = new EquippedItemSnapshot(slot, item.Id);
            }

            return InventoryState.Restore(new InventoryStateSnapshot(owned, equipped));
        }
    }
}
