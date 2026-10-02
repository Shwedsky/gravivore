using System;
using Gravivore.Editor.Balance;
using Gravivore.Gameplay.Offline;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Persistence.Profile;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;

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
            Assert.That(camera.Offset.y, Is.InRange(10f, 18f));
            Assert.That(camera.Offset.z, Is.InRange(-14f, -7f));
            Assert.That(camera.FieldOfView, Is.InRange(40f, 55f));
            Assert.That(camera.PositionDamping, Is.InRange(0f, 0.5f));
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
