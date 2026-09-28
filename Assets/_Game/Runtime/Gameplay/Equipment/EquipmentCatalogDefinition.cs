using System;
using UnityEngine;

namespace Gravivore.Gameplay.Equipment
{
    [CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "Gravivore/Equipment/Catalog")]
    public sealed class EquipmentCatalogDefinition : ScriptableObject
    {
        [SerializeField] private EquipmentDefinition[] _items;

        public EquipmentCatalog Catalog
        {
            get
            {
                if (_items == null || _items.Length == 0) throw new InvalidOperationException("Equipment catalog definitions require items.");
                var items = new EquipmentItem[_items.Length];
                for (var i = 0; i < _items.Length; i++)
                {
                    if (_items[i] == null) throw new InvalidOperationException($"Equipment definition {i} is missing.");
                    items[i] = _items[i].Item;
                }

                return new EquipmentCatalog(items);
            }
        }

        public void ValidateOrThrow() => _ = Catalog;
    }
}
