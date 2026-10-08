using System;
using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Gameplay.Equipment
{
    public sealed class EquipmentService
    {
        private readonly PlayerStatsState _playerStats;
        private readonly EquipmentCatalog _catalog;
        private readonly InventoryState _inventory;
        private readonly IPlayerDerivedStatsModifier[] _equippedModifiers =
            new IPlayerDerivedStatsModifier[4];

        public EquipmentService(
            PlayerStatsState playerStats,
            EquipmentCatalog catalog,
            InventoryState inventory)
        {
            _playerStats = playerStats ?? throw new ArgumentNullException(nameof(playerStats));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            ValidateInventory();
            ApplyEquippedModifiers();
        }

        public event Action<EquipmentGrantedEvent> EquipmentGranted;
        public event Action<EquipmentChangedEvent> EquipmentChanged;
        public event Action<InventoryChangedEvent> InventoryChanged;

        public EquipmentCatalog Catalog => _catalog;
        public InventoryState Inventory => _inventory;

        public bool GrantEquipment(
            string itemId,
            EquipmentGrantSource source = EquipmentGrantSource.GameplayReward)
        {
            var item = _catalog.GetRequired(itemId);
            if (!Enum.IsDefined(typeof(EquipmentGrantSource), source)) throw new ArgumentOutOfRangeException(nameof(source));
            if (!_inventory.Grant(item.Id)) return false;

            Publish(EquipmentGranted, new EquipmentGrantedEvent(item.Id, source));
            Publish(InventoryChanged, new InventoryChangedEvent(InventoryChangeType.ItemGranted));
            return true;
        }

        /// <summary>Only called inside a durable encounter reward transaction for gameplay loot.</summary>
        public bool GrantRankedCopy(string itemId)
        {
            var item = _catalog.GetRequired(itemId);
            if (!_inventory.HasItem(itemId)) return GrantEquipment(itemId);
            if (!_inventory.IncreaseRank(itemId, item.MaximumRank)) return false;
            ApplyEquippedModifiers();
            Publish(EquipmentGranted, new EquipmentGrantedEvent(itemId, EquipmentGrantSource.GameplayReward));
            Publish(InventoryChanged, new InventoryChangedEvent(InventoryChangeType.ItemGranted));
            return true;
        }

        public bool Equip(string itemId, EquipmentSlot slot)
        {
            var item = _catalog.GetRequired(itemId);
            if (item.Slot != slot)
            {
                throw new ArgumentException($"Equipment {item.Id} belongs to {item.Slot}, not {slot}.", nameof(slot));
            }

            if (!_inventory.HasItem(item.Id)) throw new InvalidOperationException($"Cannot equip unowned item: {item.Id}.");
            if (_inventory.TryGetEquipped(slot, out var equippedId) &&
                string.Equals(equippedId, item.Id, StringComparison.Ordinal))
            {
                return false;
            }

            var previous = _inventory.SetEquipped(slot, item.Id);
            ApplyEquippedModifiers();
            Publish(EquipmentChanged, new EquipmentChangedEvent(slot, previous, item.Id));
            Publish(InventoryChanged, new InventoryChangedEvent(InventoryChangeType.EquipmentChanged));
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            if (!_inventory.TryGetEquipped(slot, out _)) return false;
            var previous = _inventory.ClearEquipped(slot);
            ApplyEquippedModifiers();
            Publish(EquipmentChanged, new EquipmentChangedEvent(slot, previous, null));
            Publish(InventoryChanged, new InventoryChangedEvent(InventoryChangeType.EquipmentChanged));
            return true;
        }

        private void ValidateInventory()
        {
            var snapshot = _inventory.ExportSnapshot();
            for (var i = 0; i < snapshot.OwnedItemIds.Length; i++)
            {
                var item = _catalog.GetRequired(snapshot.OwnedItemIds[i]);
                if (_inventory.GetRank(item.Id) > item.MaximumRank) throw new ArgumentException("Restored rank exceeds item cap.");
            }

            for (var i = 0; i < snapshot.EquippedItems.Length; i++)
            {
                var equipped = snapshot.EquippedItems[i];
                var item = _catalog.GetRequired(equipped.ItemId);
                if (item.Slot != equipped.Slot)
                {
                    throw new ArgumentException($"Restored equipment {item.Id} is in the wrong slot.", nameof(_inventory));
                }
            }
        }

        private void ApplyEquippedModifiers()
        {
            var count = 0;
            for (var slotIndex = 0; slotIndex < _equippedModifiers.Length; slotIndex++)
            {
                if (!_inventory.TryGetEquipped((EquipmentSlot)slotIndex, out var itemId)) continue;
                _equippedModifiers[count++] = _catalog.GetRequired(itemId).ModifierAtRank(_inventory.GetRank(itemId));
            }

            var active = new IPlayerDerivedStatsModifier[count];
            Array.Copy(_equippedModifiers, active, count);
            for (var i = count; i < _equippedModifiers.Length; i++) _equippedModifiers[i] = null;
            _playerStats.SetModifiers(PlayerStatsModifierSource.Equipment, active);
        }

        private static void Publish<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            var invocationList = handlers.GetInvocationList();
            for (var i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    ((Action<T>)invocationList[i]).Invoke(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}
