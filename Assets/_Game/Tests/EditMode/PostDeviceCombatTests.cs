using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Development;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class PostDeviceCombatTests
    {
        [Test] public void EveryStrongRewardPreviewUsesTheSameConfigurationScalingAsTheGrant()
        {
            var progression=AssetDatabase.LoadAssetAtPath<PlayerProgressionDefinition>("Assets/_Game/Content/Definitions/S06_PlayerProgression.asset");
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            Assert.IsNotNull(progression); Assert.IsNotNull(world);
            foreach(var strong in Chapter1StrongOrdinarySpotCatalog.Create(world.Configuration))
            {
                var id=strong.Region==StrongOrdinaryRegion.Elite?"cutter-unit":"carrier";
                // Use an actual configured route; the test checks every strong multiplier.
                if(!progression.Configuration.TryGetReward(id,out var unit))
                    Assert.Fail("Expected canonical reward route: "+id);
                progression.Configuration.TryGetReward(id,strong.RewardMultiplier,out var scaled);
                Assert.That(scaled.StatExperience,Is.EqualTo(unit.StatExperience*strong.RewardMultiplier));
                Assert.That(scaled.AssimilationScore,Is.EqualTo(System.Math.Round(unit.AssimilationScore*(double)strong.RewardMultiplier,System.MidpointRounding.AwayFromZero)));
                var configuration=AssetDatabase.LoadAssetAtPath<PlayerStatsDefinition>("Assets/_Game/Content/Definitions/S02_PlayerStats.asset").Configuration;
                using var service=new AssimilationProgressionService(new PlayerStatsState(configuration,configuration.StartingLevels),new ProgressionState(),progression.Configuration);
                CoreRewardGrantedEvent granted=default; service.RewardGranted+=value=>granted=value;
                Assert.IsTrue(service.TryGrant(new EnemyDeathEvent(new EnemyLifeId(System.Guid.NewGuid()),id,Vector3.zero,strong.RewardMultiplier)));
                Assert.That(granted.GrantedExperience,Is.EqualTo(scaled.StatExperience));
                Assert.That(granted.GrantedAssimilationScore,Is.EqualTo(scaled.AssimilationScore));
            }
        }
        [Test] public void HitchWindowIsFixedSizeAndThresholdCooldownSuppressRepeatedStalls()
        {
            var window=new HitchSampleWindow(.25f,20,5);
            Assert.IsFalse(window.Record(1,1)); Assert.IsFalse(window.Record(.016f,6));
            Assert.IsTrue(window.Record(.3f,7)); Assert.IsFalse(window.Record(1,8)); Assert.IsTrue(window.Record(.3f,27));
            var samples=new float[64]; for(var i=0;i<100;i++) window.Record(.016f,i+28);
            window.CopyChronological(samples); Assert.That(samples,Has.Length.EqualTo(64));
            foreach(var sample in samples) Assert.That(sample,Is.EqualTo(.016f));
        }
        [Test] public void PresentationSettingsRemainBoundedAndSceneDependencyIsPresent()
        {
            var settings=AssetDatabase.LoadAssetAtPath<PostDevicePresentationDefinition>(Gravivore.Editor.VisualIntegration.PostDeviceCombatBuilder.SettingsPath);
            Assert.IsNotNull(settings); Assert.DoesNotThrow(settings.ValidateOrThrow);
            Assert.That(settings.VisibleBars,Is.LessThanOrEqualTo(8)); Assert.That(settings.DamageTextCapacity+settings.RewardTextCapacity,Is.LessThanOrEqualTo(24));
            Assert.That(AssetDatabase.GetDependencies(Gravivore.Editor.VisualIntegration.FirstVisualSliceBuilder.ScenePath,true),
                Does.Contain(Gravivore.Editor.VisualIntegration.PostDeviceCombatBuilder.SettingsPath));
        }
    }
}
