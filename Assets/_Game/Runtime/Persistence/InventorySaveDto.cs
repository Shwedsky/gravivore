using System;

namespace Gravivore.Persistence
{
    [Serializable]
    public sealed class InventorySaveDto
    {
        public string[] OwnedItemIds;
        public EquippedItemSaveDto[] EquippedItems;
        public ItemRankSaveDto[] ItemRanks;
    }

    [Serializable]
    public sealed class ItemRankSaveDto
    {
        public string ItemId;
        public int Rank;
    }
    [Serializable]
    public sealed class EquippedItemSaveDto
    {
        public int Slot;
        public string ItemId;
    }
}
