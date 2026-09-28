using System;

namespace Gravivore.Persistence
{
    [Serializable]
    public sealed class InventorySaveDto
    {
        public string[] OwnedItemIds;
        public EquippedItemSaveDto[] EquippedItems;
    }

    [Serializable]
    public sealed class EquippedItemSaveDto
    {
        public int Slot;
        public string ItemId;
    }
}
