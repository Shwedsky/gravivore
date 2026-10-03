using System;
using Gravivore.Editor.Balance;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Persistence.Profile;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class S20BalanceTests
    {
        [Test]
        public void CanonicalBalance_HasEarlyGrowthOrderedEvolutionAndFiveSpotEliteGate()
        {
            var progression = AssetDatabase.LoadAssetAtPath<PlayerProgressionDefinition>(
                "Assets/_Game/Content/Definitions/S06_PlayerProgression.asset").Configuration;
            var evolution = AssetDatabase.LoadAssetAtPath<EvolutionDefinition>(
                "Assets/_Game/Content/Definitions/S07_Evolution.asset").Catalog.Selection;
            var world = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(
                "Assets/_Game/Content/Definitions/S08_Chapter01World.asset").Configuration;
            var estimate = S20BalanceModel.Evaluate(progression, evolution, world.EliteRequirement);

            Assert.That(estimate.KillsToFirstStatLevel, Is.InRange(1, 4));
            Assert.That(world.EliteRequirement.RequiredObjectiveCount, Is.EqualTo(5));
            Assert.That(estimate.MinimumKillsForElite, Is.EqualTo(60));
            Assert.That(estimate.Tier1Assimilation / (double)world.EliteRequirement.MinimumAssimilationScore, Is.InRange(0.25d, 0.35d));
            Assert.That(estimate.Tier2Assimilation / (double)world.EliteRequirement.MinimumAssimilationScore, Is.InRange(0.65d, 0.75d));
            Assert.That(estimate.EstimatedBossReadyMinutes, Is.InRange(30d, 45d));
            Assert.That(estimate.EstimatedUpperBossReadyMinutes, Is.InRange(30d, 45d));
        }

        [Test]
        public void CanonicalOfflineAndCamera_StayInsideS20DesignBands()
        {
            var offline = AssetDatabase.LoadAssetAtPath<SaveOfflineDefinition>(
                "Assets/_Game/Content/Definitions/S12_SaveOffline.asset").Configuration.OfflineReward;
            var camera = AssetDatabase.LoadAssetAtPath<CameraFollowSettings>(
                "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");

            Assert.That(offline.Efficiency, Is.InRange(0.2d, 0.3d));
            Assert.That(offline.MaximumEligibleDuration, Is.EqualTo(TimeSpan.FromHours(2)));
            Assert.That(camera.Offset.y, Is.InRange(14f, 16f));
            Assert.That(camera.Offset.z, Is.InRange(-13f, -10f));
            Assert.That(camera.FieldOfView, Is.InRange(44f, 50f));
            Assert.That(camera.PositionDamping, Is.InRange(0f, 0.5f));
        }

        [Test]
        public void CanonicalWorld_HasSeparatedRegionsAndProgressiveEncounterApproach()
        {
            var world = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(
                "Assets/_Game/Content/Definitions/S08_Chapter01World.asset").Configuration;
            var elite = AssetDatabase.LoadAssetAtPath<MagnetarGuardDefinition>(
                "Assets/_Game/Content/Definitions/S09_MagnetarGuard.asset").Configuration;
            var boss = AssetDatabase.LoadAssetAtPath<CustodianBossDefinition>(
                "Assets/_Game/Content/Definitions/S09_CustodianM0.asset").CreateConfiguration(world);
            var nearestZoneToElite = float.MaxValue;
            var farthestZoneFromStart = 0f;

            for (var i = 0; i < world.ZoneCount; i++)
            {
                var zone = world.GetZone(i);
                Assert.That(world.Bounds.Contains(zone.Center), Is.True, zone.Id);
                nearestZoneToElite = Mathf.Min(nearestZoneToElite, Vector3.Distance(zone.Center, elite.SpawnPosition));
                farthestZoneFromStart = Mathf.Max(farthestZoneFromStart, Vector3.Distance(world.BasinCenter, zone.Center));

                for (var otherIndex = i + 1; otherIndex < world.ZoneCount; otherIndex++)
                {
                    Assert.That(
                        Vector3.Distance(zone.Center, world.GetZone(otherIndex).Center),
                        Is.GreaterThanOrEqualTo(28f),
                        $"{zone.Id} overlaps the readable region of {world.GetZone(otherIndex).Id}.");
                }
            }

            Assert.That(farthestZoneFromStart / 4.5f, Is.InRange(15f, 25f));
            Assert.That(nearestZoneToElite / 4.5f, Is.GreaterThanOrEqualTo(6f));
            Assert.That(world.Bounds.Contains(elite.SpawnPosition, elite.CollisionRadius), Is.True);
            Assert.That(world.Bounds.ContainsCircle(world.BossArenaCenter, world.BossArenaRadius), Is.True);
            Assert.That(world.BossGate.Position.z, Is.GreaterThan(elite.SpawnPosition.z));
            Assert.That(boss.StartPosition.z, Is.GreaterThan(world.BossGate.Position.z));
            Assert.That((boss.StartPosition.z - elite.SpawnPosition.z) / 4.5f, Is.GreaterThanOrEqualTo(5f));
        }

        [Test]
        public void FreshPlayer_UsesNormalCanonicalOrdinaryAggroRadius()
        {
            var playerDefinition = AssetDatabase.LoadAssetAtPath<PlayerStatsDefinition>(
                "Assets/_Game/Content/Definitions/S02_PlayerStats.asset");
            var player = new PlayerStatsState(
                playerDefinition.Configuration,
                playerDefinition.Configuration.StartingLevels);
            var enemyPaths = new[]
            {
                "S04_Enemy_ScoutDrone.asset", "S04_Enemy_CutterUnit.asset",
                "S04_Enemy_Warden.asset", "S04_Enemy_ArcDrone.asset", "S04_Enemy_Carrier.asset"
            };

            for (var i = 0; i < enemyPaths.Length; i++)
            {
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                    "Assets/_Game/Content/Definitions/" + enemyPaths[i]).CreateRuntimeConfiguration();
                Assert.That(
                    OrdinaryEnemyAggressionPolicy.ResolveProactiveAggroRadius(player.DerivedStats, enemy),
                    Is.EqualTo(enemy.Behavior.AggroRadius),
                    enemy.Id);
            }
        }
    }
}
