using System.Collections;
using Gravivore.Core.Stats;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Player;
using Gravivore.Persistence;
using Gravivore.Presentation.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gravivore.Tests.PlayMode
{
    public sealed class EquipmentSmokeTests
    {
        [UnityTest]
        public IEnumerator ChapterScene_EquipmentUpdatesCombatHealthAndMovementStats()
        {
            yield return LoadChapter();
            var root = FindCompositionRoot();
            Assert.IsNotNull(root.EquipmentCatalog);
            Assert.IsNotNull(root.Inventory);
            Assert.IsNotNull(root.Equipment);

            var baseDamage = root.PlayerStats.DerivedStats.BaseDamage;
            var baseInterval = root.PlayerStats.DerivedStats.AttackInterval;
            var baseMaxHp = root.PlayerHealth.MaximumHitPoints;
            var baseMoveSpeed = ((IMoveSpeedProvider)root.PlayerStats).MoveSpeed;

            GrantAndEquip(root.Equipment, "gravitic-fang", EquipmentSlot.Core);
            Assert.That(root.PlayerStats.DerivedStats.BaseDamage, Is.EqualTo(baseDamage + 4f).Within(0.0001f));

            GrantAndEquip(root.Equipment, "layered-carapace", EquipmentSlot.Chassis);
            Assert.That(root.PlayerHealth.MaximumHitPoints, Is.EqualTo(baseMaxHp + 30f).Within(0.0001f));

            GrantAndEquip(root.Equipment, "vector-fins", EquipmentSlot.Module);
            Assert.That(((IMoveSpeedProvider)root.PlayerStats).MoveSpeed, Is.EqualTo(baseMoveSpeed + 1f).Within(0.0001f));

            root.Equipment.GrantEquipment("pulse-capacitor");
            root.Equipment.Equip("pulse-capacitor", EquipmentSlot.Core);
            Assert.That(root.PlayerStats.DerivedStats.BaseDamage, Is.EqualTo(baseDamage).Within(0.0001f));
            Assert.That(root.PlayerStats.DerivedStats.AttackInterval, Is.EqualTo(baseInterval - 0.15f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator DeathRespawn_PreservesEquippedEffectsAndPermanentLevels()
        {
            yield return LoadChapter();
            var root = FindCompositionRoot();
            GrantAndEquip(root.Equipment, "layered-carapace", EquipmentSlot.Chassis);
            var expectedLevels = root.PlayerStats.BaseLevels;
            var expectedMaximumHp = root.PlayerHealth.MaximumHitPoints;

            var result = root.PlayerHealth.ApplyDamage(new DamageRequest(100000f, DamageType.Physical));

            Assert.IsTrue(result.WasLethal);
            Assert.That(root.PlayerHealth.CurrentHitPoints, Is.EqualTo(expectedMaximumHp).Within(0.0001f));
            Assert.That(root.PlayerHealth.MaximumHitPoints, Is.EqualTo(expectedMaximumHp).Within(0.0001f));
            Assert.That(root.PlayerStats.BaseLevels.Hull, Is.EqualTo(expectedLevels.Hull));
            Assert.IsTrue(root.Inventory.TryGetEquipped(EquipmentSlot.Chassis, out var itemId));
            Assert.That(itemId, Is.EqualTo("layered-carapace"));
        }

        [UnityTest]
        public IEnumerator SaveDtoReload_ReappliesSameDerivedStats()
        {
            var catalog = CreateCatalog();
            var sourceStats = CreateStats();
            var sourceState = new InventoryState();
            var sourceService = new EquipmentService(sourceStats, catalog, sourceState);
            GrantAndEquip(sourceService, "damage-core", EquipmentSlot.Core);
            GrantAndEquip(sourceService, "health-chassis", EquipmentSlot.Chassis);
            GrantAndEquip(sourceService, "speed-module", EquipmentSlot.Module);
            var expected = sourceStats.DerivedStats;

            var restoredState = InventorySaveMapper.Restore(InventorySaveMapper.Export(sourceState), catalog);
            var restoredStats = CreateStats();
            _ = new EquipmentService(restoredStats, catalog, restoredState);

            Assert.IsTrue(restoredStats.DerivedStats.HasSameValues(expected));
            yield return null;
        }

        private static IEnumerator LoadChapter()
        {
            var operation = SceneManager.LoadSceneAsync("Chapter01_ScrapExclusion", LoadSceneMode.Single);
            Assert.IsNotNull(operation);
            yield return operation;
            yield return null;
        }

        private static S01SceneCompositionRoot FindCompositionRoot()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i].TryGetComponent<S01SceneCompositionRoot>(out var root)) return root;
            }

            Assert.Fail("Chapter scene composition root was not found.");
            return null;
        }

        private static void GrantAndEquip(EquipmentService service, string itemId, EquipmentSlot slot)
        {
            service.GrantEquipment(itemId, EquipmentGrantSource.Development);
            service.Equip(itemId, slot);
        }

        private static EquipmentCatalog CreateCatalog() => new EquipmentCatalog(new[]
        {
            new EquipmentItem("damage-core", "Damage Core", EquipmentSlot.Core, new EquipmentFlatModifier(4f, 0f, 0f, 0f, 0f)),
            new EquipmentItem("health-chassis", "Health Chassis", EquipmentSlot.Chassis, new EquipmentFlatModifier(0f, 30f, 3f, 0f, 0f)),
            new EquipmentItem("speed-module", "Speed Module", EquipmentSlot.Module, new EquipmentFlatModifier(0f, 0f, 0f, 0f, 1f))
        });

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
    }
}
