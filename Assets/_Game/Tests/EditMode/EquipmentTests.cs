using System;
using System.Text.RegularExpressions;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Player;
using Gravivore.Persistence;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gravivore.Tests.EditMode
{
    public sealed class EquipmentTests
    {
        [Test]
        public void Catalog_RequiresUniqueIdsAndSupportsExactlyThreeSlotKinds()
        {
            var catalog = CreateCatalog();

            Assert.That(catalog.Count, Is.EqualTo(6));
            Assert.That(Enum.GetValues(typeof(EquipmentSlot)).Length, Is.EqualTo(3));
            Assert.Throws<ArgumentException>(() => new EquipmentCatalog(new[]
            {
                Item("same", EquipmentSlot.Core, damage: 1f),
                Item("same", EquipmentSlot.Module, speed: 1f)
            }));
        }

        [Test]
        public void Grant_IsOwnedImmediatelyAndDuplicateIsIdempotent()
        {
            var rig = CreateRig();
            var grantedCount = 0;
            rig.Service.EquipmentGranted += _ => grantedCount++;

            Assert.IsTrue(rig.Service.GrantEquipment("damage-core"));
            Assert.IsTrue(rig.State.HasItem("damage-core"));
            Assert.IsFalse(rig.Service.GrantEquipment("damage-core"));
            Assert.That(rig.State.OwnedCount, Is.EqualTo(1));
            Assert.That(grantedCount, Is.EqualTo(1));
        }

        [Test]
        public void Equip_RejectsUnownedItemAndWrongSlot()
        {
            var rig = CreateRig();
            Assert.Throws<InvalidOperationException>(() => rig.Service.Equip("damage-core", EquipmentSlot.Core));

            rig.Service.GrantEquipment("damage-core");
            Assert.Throws<ArgumentException>(() => rig.Service.Equip("damage-core", EquipmentSlot.Module));
            Assert.IsFalse(rig.State.TryGetEquipped(EquipmentSlot.Core, out _));
        }

        [Test]
        public void EquipSwapAndUnequip_UpdateDerivedStatsWithoutMutatingLevels()
        {
            var rig = CreateRig();
            var levels = rig.Stats.BaseLevels;
            var baselineDamage = rig.Stats.DerivedStats.BaseDamage;
            rig.Service.GrantEquipment("damage-core");
            rig.Service.GrantEquipment("flux-core");

            Assert.IsTrue(rig.Service.Equip("damage-core", EquipmentSlot.Core));
            Assert.That(rig.Stats.DerivedStats.BaseDamage, Is.EqualTo(baselineDamage + 4f).Within(0.0001f));
            Assert.IsTrue(rig.Service.Equip("flux-core", EquipmentSlot.Core));
            Assert.That(rig.Stats.DerivedStats.BaseDamage, Is.EqualTo(baselineDamage).Within(0.0001f));
            Assert.That(rig.Stats.DerivedStats.AttackInterval, Is.EqualTo(0.85f).Within(0.0001f));
            Assert.IsTrue(rig.State.HasItem("damage-core"));

            Assert.IsTrue(rig.Service.Unequip(EquipmentSlot.Core));
            Assert.That(rig.Stats.DerivedStats.AttackInterval, Is.EqualTo(1f).Within(0.0001f));
            AssertLevels(levels, rig.Stats.BaseLevels);
            Assert.IsFalse(rig.Service.Unequip(EquipmentSlot.Core));
        }

        [Test]
        public void MultipleSlotsComposeAndHardCapsApplyAfterEquipment()
        {
            var rig = CreateRig();
            GrantAndEquip(rig, "damage-core", EquipmentSlot.Core);
            GrantAndEquip(rig, "health-chassis", EquipmentSlot.Chassis);
            GrantAndEquip(rig, "speed-module", EquipmentSlot.Module);

            Assert.That(rig.Stats.DerivedStats.BaseDamage, Is.EqualTo(14f).Within(0.0001f));
            Assert.That(rig.Stats.DerivedStats.MaxHp, Is.EqualTo(130f).Within(0.0001f));
            Assert.That(rig.Stats.DerivedStats.ArmorValue, Is.EqualTo(8f).Within(0.0001f));
            Assert.That(rig.Stats.DerivedStats.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));

            rig.Service.GrantEquipment("cap-module");
            rig.Service.Equip("cap-module", EquipmentSlot.Module);
            rig.Service.GrantEquipment("flux-core");
            rig.Service.Equip("flux-core", EquipmentSlot.Core);
            Assert.That(rig.Stats.DerivedStats.AttackInterval, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(rig.Stats.DerivedStats.MoveSpeed, Is.EqualTo(6f).Within(0.0001f));
        }

        [Test]
        public void EquipmentSource_DoesNotEraseExternalModifiersAndSurvivesLevelChange()
        {
            var rig = CreateRig(new AddDamageModifier(3f));
            GrantAndEquip(rig, "damage-core", EquipmentSlot.Core);
            Assert.That(rig.Stats.DerivedStats.BaseDamage, Is.EqualTo(17f).Within(0.0001f));

            rig.Service.Unequip(EquipmentSlot.Core);
            Assert.That(rig.Stats.DerivedStats.BaseDamage, Is.EqualTo(13f).Within(0.0001f));
            rig.Service.Equip("damage-core", EquipmentSlot.Core);
            rig.Stats.SetLevel(PlayerStatType.Power, 2);
            Assert.That(rig.Stats.DerivedStats.BaseDamage, Is.EqualTo(19f).Within(0.0001f));
        }

        [Test]
        public void Events_ArePostMutationAndFailingListenerDoesNotBlockFollowingListener()
        {
            var rig = CreateRig();
            rig.Service.GrantEquipment("damage-core");
            var observedMutation = false;
            var laterListenerCalled = false;
            rig.Service.EquipmentChanged += _ =>
            {
                observedMutation = rig.State.TryGetEquipped(EquipmentSlot.Core, out var id) && id == "damage-core";
                throw new InvalidOperationException("expected listener failure");
            };
            rig.Service.EquipmentChanged += _ => laterListenerCalled = true;
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: expected listener failure"));

            Assert.IsTrue(rig.Service.Equip("damage-core", EquipmentSlot.Core));
            Assert.IsTrue(observedMutation);
            Assert.IsTrue(laterListenerCalled);
        }

        [Test]
        public void SaveMapper_RoundTripsOwnedAndEquippedStateWithoutReplayingEvents()
        {
            var source = CreateRig();
            GrantAndEquip(source, "damage-core", EquipmentSlot.Core);
            GrantAndEquip(source, "health-chassis", EquipmentSlot.Chassis);
            var expected = source.Stats.DerivedStats;

            var restoredState = InventorySaveMapper.Restore(
                InventorySaveMapper.Export(source.State),
                source.Catalog);
            var restoredStats = CreateStats();
            var restoredService = new EquipmentService(restoredStats, source.Catalog, restoredState);
            var eventCount = 0;
            restoredService.EquipmentChanged += _ => eventCount++;

            Assert.IsTrue(restoredState.HasItem("damage-core"));
            Assert.IsTrue(restoredState.TryGetEquipped(EquipmentSlot.Chassis, out var chassis));
            Assert.That(chassis, Is.EqualTo("health-chassis"));
            Assert.That(restoredStats.DerivedStats.HasSameValues(expected), Is.True);
            Assert.That(eventCount, Is.Zero);
        }

        [Test]
        public void SaveMapper_RejectsUnknownDuplicateUnownedAndWrongSlotData()
        {
            var catalog = CreateCatalog();
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => InventorySaveMapper.Restore(
                Dto(new[] { "unknown" }), catalog));
            Assert.Throws<ArgumentException>(() => InventorySaveMapper.Restore(
                Dto(new[] { "damage-core", "damage-core" }), catalog));
            Assert.Throws<ArgumentException>(() => InventorySaveMapper.Restore(
                Dto(Array.Empty<string>(), new EquippedItemSaveDto { Slot = 0, ItemId = "damage-core" }), catalog));
            Assert.Throws<ArgumentException>(() => InventorySaveMapper.Restore(
                Dto(new[] { "damage-core" }, new EquippedItemSaveDto { Slot = 2, ItemId = "damage-core" }), catalog));
        }

        private static InventorySaveDto Dto(string[] owned, params EquippedItemSaveDto[] equipped) =>
            new InventorySaveDto { OwnedItemIds = owned, EquippedItems = equipped };

        private static void GrantAndEquip(Rig rig, string itemId, EquipmentSlot slot)
        {
            rig.Service.GrantEquipment(itemId);
            rig.Service.Equip(itemId, slot);
        }

        private static Rig CreateRig(IPlayerDerivedStatsModifier external = null)
        {
            var stats = CreateStats();
            if (external != null) stats.SetModifiers(new[] { external });
            var catalog = CreateCatalog();
            var state = new InventoryState();
            return new Rig(stats, catalog, state, new EquipmentService(stats, catalog, state));
        }

        private static EquipmentCatalog CreateCatalog() => new EquipmentCatalog(new[]
        {
            Item("damage-core", EquipmentSlot.Core, damage: 4f),
            Item("flux-core", EquipmentSlot.Core, interval: -10f),
            Item("health-chassis", EquipmentSlot.Chassis, hp: 30f, armor: 3f),
            Item("other-chassis", EquipmentSlot.Chassis, hp: 10f),
            Item("speed-module", EquipmentSlot.Module, speed: 20f),
            Item("cap-module", EquipmentSlot.Module, interval: -10f, speed: 20f)
        });

        private static EquipmentItem Item(
            string id,
            EquipmentSlot slot,
            float damage = 0f,
            float hp = 0f,
            float armor = 0f,
            float interval = 0f,
            float speed = 0f) =>
            new EquipmentItem(id, id, slot, new EquipmentFlatModifier(damage, hp, armor, interval, speed));

        private static PlayerStatsState CreateStats()
        {
            var configuration = new PlayerStatsConfiguration(
                new StatCurve(10, 10f, 2f, 0.5f, 0f, 1000f),
                new StatCurve(10, 100f, 10f, 1f, 1f, 10000f),
                new StatCurve(10, 5f, 3f, 0f, 0f, 1000f),
                new StatCurve(10, 1f, -0.1f, 0f, 0.05f, 10f),
                new StatCurve(10, 4f, 0.5f, 0f, 0f, 20f),
                0.4f,
                6f,
                new PlayerStatLevels(1, 1, 1, 1, 1));
            return new PlayerStatsState(configuration, configuration.StartingLevels);
        }

        private static void AssertLevels(PlayerStatLevels expected, PlayerStatLevels actual)
        {
            Assert.That(actual.Power, Is.EqualTo(expected.Power));
            Assert.That(actual.Hull, Is.EqualTo(expected.Hull));
            Assert.That(actual.Armor, Is.EqualTo(expected.Armor));
            Assert.That(actual.Flux, Is.EqualTo(expected.Flux));
            Assert.That(actual.Mobility, Is.EqualTo(expected.Mobility));
        }

        private sealed class AddDamageModifier : IPlayerDerivedStatsModifier
        {
            private readonly float _value;
            public AddDamageModifier(float value) => _value = value;
            public PlayerDerivedStats Apply(in PlayerDerivedStats currentValues) =>
                new PlayerDerivedStats(
                    currentValues.BaseDamage + _value,
                    currentValues.MaxHp,
                    currentValues.ArmorValue,
                    currentValues.AttackInterval,
                    currentValues.MoveSpeed);
        }

        private readonly struct Rig
        {
            public Rig(PlayerStatsState stats, EquipmentCatalog catalog, InventoryState state, EquipmentService service)
            {
                Stats = stats;
                Catalog = catalog;
                State = state;
                Service = service;
            }

            public PlayerStatsState Stats { get; }
            public EquipmentCatalog Catalog { get; }
            public InventoryState State { get; }
            public EquipmentService Service { get; }
        }
    }
}
