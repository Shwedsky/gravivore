using System;
using System.Collections.Generic;

namespace Gravivore.Gameplay.Equipment
{
    public sealed class EquipmentCatalog
    {
        private readonly EquipmentItem[] _items;
        private readonly Dictionary<string, EquipmentItem> _itemsById;

        public EquipmentCatalog(IReadOnlyList<EquipmentItem> items)
        {
            if (items == null || items.Count == 0) throw new ArgumentException("Equipment catalog requires items.", nameof(items));
            _items = new EquipmentItem[items.Count];
            _itemsById = new Dictionary<string, EquipmentItem>(items.Count, StringComparer.Ordinal);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!_itemsById.TryAdd(item.Id, item)) throw new ArgumentException($"Duplicate equipment id: {item.Id}.", nameof(items));
                _items[i] = item;
            }
        }

        public int Count => _items.Length;
        public EquipmentItem GetAt(int index) => _items[index];
        public bool TryGet(string itemId, out EquipmentItem item)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                item = default;
                return false;
            }

            return _itemsById.TryGetValue(itemId, out item);
        }

        public EquipmentItem GetRequired(string itemId)
        {
            if (!TryGet(itemId, out var item)) throw new KeyNotFoundException($"Unknown equipment id: {itemId}.");
            return item;
        }
    }
}
