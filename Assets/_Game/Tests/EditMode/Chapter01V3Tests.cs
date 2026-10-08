using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Player;
using Gravivore.Persistence;
using Gravivore.Persistence.Profile;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter01V3Tests
    {
        private static T Asset<T>(string name) where T:UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>("Assets/_Game/Content/Definitions/"+name+".asset");
        [Test] public void EveryStrongSpotExceedsItsActualOrdinaryArchetypeInHealthDamageCadenceSpeedAndReward()
        {
            var world=Asset<Chapter01WorldDefinition>("S08_Chapter01World").Configuration;
            foreach(var spot in Chapter1StrongOrdinarySpotCatalog.Create(world))
            {
                var ordinary=Asset<SpawnSpotDefinition>(spot.Region==StrongOrdinaryRegion.Elite?"S04_SpawnSpot_CapacitorField":"S04_SpawnSpot_HaulerGraveyard").CreateRuntimeConfiguration();
                var strong=spot.CreateSpawnConfiguration(ordinary);
                Assert.That(strong.Enemy.Id,Is.EqualTo(ordinary.Enemy.Id));
                Assert.That(strong.Enemy.MaximumHitPoints/ordinary.Enemy.MaximumHitPoints,Is.InRange(1.5f,2.2f));
                Assert.That(strong.Enemy.AttackDamage/ordinary.Enemy.AttackDamage,Is.InRange(1.2f,1.6001f));
                Assert.That(strong.Enemy.MoveSpeed,Is.GreaterThan(ordinary.Enemy.MoveSpeed));
                Assert.That(strong.Enemy.Behavior.AttackInterval,Is.LessThan(ordinary.Enemy.Behavior.AttackInterval));
                Assert.That(strong.RewardMultiplier,Is.GreaterThan(ordinary.RewardMultiplier));
            }
        }
        [TestCase(false)] [TestCase(true)]
        public void EliteAndBossAuthoredBasicsRespectWindupCadenceCancellationAndNoCatchupBurst(bool boss)
        {
            var config=boss?Asset<CustodianBossDefinition>("S09_CustodianM0").CreateConfiguration(Asset<Chapter01WorldDefinition>("S08_Chapter01World").Configuration).BasicAttack:
                Asset<MagnetarGuardDefinition>("S09_MagnetarGuard").Configuration.BasicAttack;
            Assert.IsTrue(config.Enabled);var cadence=new EncounterBasicAttackCadence(config);
            cadence.Tick(0,config.Range-1,true);Assert.IsTrue(cadence.Began);Assert.IsFalse(cadence.Resolved);
            cadence.Tick(config.Windup-.01f,config.Range-1,true);Assert.IsFalse(cadence.Resolved);
            cadence.Tick(.011f,config.Range-1,true);Assert.IsTrue(cadence.Resolved);Assert.IsTrue(cadence.Hit);
            cadence.Tick(.01f,config.Range-1,true);Assert.IsFalse(cadence.Began);
            cadence.Tick(30,config.Range-1,true);Assert.IsTrue(cadence.Began);Assert.IsFalse(cadence.Resolved);
            cadence.Tick(.1f,config.Range-1,false);Assert.IsFalse(cadence.IsCharging);
            cadence.Reset();cadence.Tick(0,config.Range-1,true);cadence.Tick(config.Windup,config.Range+1,true);
            Assert.IsTrue(cadence.Resolved);Assert.IsFalse(cadence.Hit);
        }
        [Test] public void WeaponRankSaveRoundTripEquipUnequipAndCapPreservePermanentLevels()
        {
            var statsConfig=Asset<PlayerStatsDefinition>("S02_PlayerStats").Configuration;
            var stats=new PlayerStatsState(statsConfig,statsConfig.StartingLevels);var baseline=stats.DerivedStats.BaseDamage;
            var catalog=Asset<EquipmentCatalogDefinition>("S10_EquipmentCatalog").Catalog;
            var state=new InventoryState();var service=new EquipmentService(stats,catalog,state);
            for(var rank=1;rank<=5;rank++)
            {
                Assert.IsTrue(service.GrantRankedCopy(Chapter01Weapon.ItemId));
                service.Equip(Chapter01Weapon.ItemId,EquipmentSlot.Weapon);
                Assert.That(state.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(rank));
                Assert.That(stats.DerivedStats.BaseDamage,Is.EqualTo(baseline+12+(rank-1)*4));
            }
            Assert.IsFalse(service.GrantRankedCopy(Chapter01Weapon.ItemId));Assert.That(state.OwnedCount,Is.EqualTo(1));
            var restored=InventorySaveMapper.Restore(InventorySaveMapper.Export(state),catalog);
            var reloadedStats=new PlayerStatsState(statsConfig,statsConfig.StartingLevels);
            var reloaded=new EquipmentService(reloadedStats,catalog,restored);
            Assert.That(restored.GetRank(Chapter01Weapon.ItemId),Is.EqualTo(5));
            Assert.That(reloadedStats.DerivedStats.BaseDamage,Is.EqualTo(baseline+28));
            reloaded.Unequip(EquipmentSlot.Weapon);Assert.That(reloadedStats.DerivedStats.BaseDamage,Is.EqualTo(baseline));
            Assert.That(reloadedStats.BaseLevels,Is.EqualTo(statsConfig.StartingLevels));
            var dto=InventorySaveMapper.Export(state);dto.ItemRanks[0].Rank=6;
            Assert.Throws<ArgumentException>(()=>InventorySaveMapper.Restore(dto,catalog));
        }
        [Test] public void SchemaTwoMigrationPreservesInventoryAndProgressionWithoutInventingHistoricalBossLoot()
        {
            var serializer=new UnityJsonSaveSerializer();var defaults=new SaveRootDto();
            var source=new SaveRootDto{schemaVersion=2,profileId="existing-owner",inventory=new InventorySaveDto{OwnedItemIds=new[]{"old-core"},EquippedItems=Array.Empty<EquippedItemSaveDto>()}};
            var migrated=new SaveMigrationPipeline(3,serializer,Array.Empty<ISaveMigration>()).MigrateToCurrent(serializer.Serialize(source),defaults);
            Assert.IsTrue(migrated.WasMigrated);Assert.That(migrated.Save.schemaVersion,Is.EqualTo(3));
            Assert.That(migrated.Save.profileId,Is.EqualTo(source.profileId));
            Assert.That(migrated.Save.inventory.OwnedItemIds,Is.EqualTo(source.inventory.OwnedItemIds));
            Assert.That(migrated.Save.inventory.ItemRanks,Is.Empty);
        }
    }
}
