using System;
using System.Linq;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Presentation.UI;
using Gravivore.Presentation.Feedback;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Assets;
using Gravivore.Editor.VisualIntegration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter01ProductionTests
    {
        private static PlayerStatsConfiguration StatsConfig => AssetDatabase.LoadAssetAtPath<PlayerStatsDefinition>("Assets/_Game/Content/Definitions/S02_PlayerStats.asset").Configuration;
        private static ProgressionConfiguration ProgressionConfig => AssetDatabase.LoadAssetAtPath<PlayerProgressionDefinition>("Assets/_Game/Content/Definitions/S06_PlayerProgression.asset").Configuration;
        private static Chapter01WorldConfiguration WorldConfig => AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.World.Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset").Configuration;
        private static QuestState FreshQuests => new QuestState(AssetDatabase.LoadAssetAtPath<QuestDefinition>("Assets/_Game/Content/Definitions/S11_Chapter01OnboardingQuest.asset").Catalog);
        private sealed class EquipmentProbe : IPlayerDerivedStatsModifier
        {public PlayerDerivedStats Apply(in PlayerDerivedStats values)=>new PlayerDerivedStats(values.BaseDamage+7,values.MaxHp+13,values.ArmorValue+3,values.AttackInterval*.9f,values.MoveSpeed+.1f);}
        [TestCase(PlayerStatType.Power)] [TestCase(PlayerStatType.Hull)] [TestCase(PlayerStatType.Armor)] [TestCase(PlayerStatType.Flux)] [TestCase(PlayerStatType.Mobility)]
        public void RowsUseAuthoritativeXpThresholdCalculatorAndNonmutatingEquipmentPreview(PlayerStatType stat)
        {
            var config=StatsConfig;var levels=new PlayerStatLevels(4,4,4,4,4);var modifier=new EquipmentProbe();
            var stats=new PlayerStatsState(config,levels,new[]{modifier});
            var snapshot=new ProgressionSnapshot(7,6,5,4,3,42,Array.Empty<string>());
            using var service=new AssimilationProgressionService(stats,ProgressionState.Restore(snapshot),ProgressionConfig);
            var world=WorldConfig;var model=new CharacteristicsReadModel(stats,service,world.EliteRequirement,FreshQuests,new WorldUnlockState(world.EliteGate.Id,world.BossGate.Id,world.EliteEnemyId));
            var changeCount=0;stats.StatChanged+=_=>changeCount++;
            var read=model.Read(stat);var expected=PlayerStatsCalculator.Calculate(config,levels,new[]{modifier});
            var next=PlayerStatsCalculator.Calculate(config,levels.WithLevel(stat,5),new[]{modifier});
            Assert.That(read.Experience,Is.EqualTo(snapshot.GetStatExperience(stat)));
            Assert.That(read.Required,Is.EqualTo(ProgressionConfig.ThresholdCurve.Evaluate(4)));
            Assert.IsTrue(read.Current.HasSameValues(expected));Assert.IsTrue(read.Next.HasSameValues(next));
            Assert.That(read.Fraction,Is.EqualTo(Math.Min(1,read.Experience/read.Required)).Within(.00001f));
            Assert.That(stats.BaseLevels.GetLevel(stat),Is.EqualTo(4));Assert.That(changeCount,Is.Zero);
            StringAssert.Contains("ОП",model.Sources(stat));
        }
        [Test] public void AssimilationUsesConfiguredScoreAndObjectivesAndReportsGrantedAdmission()
        {
            var world=WorldConfig;var stats=new PlayerStatsState(StatsConfig,StatsConfig.StartingLevels);
            using var service=new AssimilationProgressionService(stats,ProgressionState.Restore(new ProgressionSnapshot(0,0,0,0,0,100,Array.Empty<string>())),ProgressionConfig);
            var state=new WorldUnlockState(world.EliteGate.Id,world.BossGate.Id,world.EliteEnemyId);
            var model=new CharacteristicsReadModel(stats,service,world.EliteRequirement,FreshQuests,state);
            Assert.That(model.AssimilationRequirement,Is.EqualTo(world.EliteRequirement.MinimumAssimilationScore));
            Assert.IsFalse(model.AdmissionGranted);StringAssert.Contains("цели 0/5",model.AssimilationText);
            state.TryUnlockEliteGate();StringAssert.Contains("ДОПУСК ПОЛУЧЕН",model.AssimilationText);
            StringAssert.DoesNotContain("До допуска",model.AssimilationText);
        }
        [TestCase("relay-yard")] [TestCase("cutting-floor")] [TestCase("shield-dump")] [TestCase("capacitor-field")] [TestCase("hauler-graveyard")]
        public void AllFivePackagesHaveAuthoredMachineryVerticalLandmarksAndOpaqueSharedMaterials(string id)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01ProductionBuilder.Prefab("Zone_"+id));Assert.IsNotNull(prefab);
            Assert.That(prefab.GetComponentsInChildren<Renderer>().Length,Is.GreaterThanOrEqualTo(10));
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true));Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true));
            Assert.IsTrue(prefab.GetComponentsInChildren<Renderer>().Any(r=>r.bounds.size.y>1.6f));
            Assert.IsTrue(prefab.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.renderQueue<2501));
            Assert.That(prefab.GetComponentsInChildren<Renderer>().Select(r=>r.sharedMaterial).Distinct().Count(),Is.LessThanOrEqualTo(2));
        }
        [TestCase("ArcDrone_V1")] [TestCase("Carrier_V1")] [TestCase("Warden_V1")] [TestCase("Custodian_V1")]
        public void NewMachinesUseBoundedRigidLodsInPlaceAnimationStatesAndOneOpaqueAtlas(string name)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01ProductionBuilder.Prefab(name));Assert.IsNotNull(prefab);
            Assert.DoesNotThrow(()=>PresentationPrefabValidation.ValidateOrThrow(prefab));
            var lods=prefab.GetComponent<LODGroup>().GetLODs();Assert.That(lods.Length,Is.EqualTo(3));
            var counts=lods.Select(l=>((SkinnedMeshRenderer)l.renderers[0]).sharedMesh.triangles.Length/3).ToArray();
            Assert.That(counts[0],Is.LessThanOrEqualTo(10000));Assert.That(counts[1],Is.LessThan(counts[0]));Assert.That(counts[2],Is.LessThan(counts[1]));
            Assert.IsFalse(prefab.GetComponent<Animator>().applyRootMotion);
            Assert.That(prefab.GetComponent<Animator>().runtimeAnimatorController.animationClips.Select(c=>c.name),Is.EquivalentTo(new[]{"Idle","Run","Attack","Hit","Death"}));
            Assert.That(prefab.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct().Count(),Is.EqualTo(1));
        }
        [Test] public void CompleteChapterAssetsAreReferencedByProductionScene()
        {Assert.DoesNotThrow(Chapter01ProductionDependencies.ValidateOrThrow);}
        [Test] public void FootstepGainAndGaitAreTunedRelativeToV38WithoutChangingCombatMix()
        {
            var audio=AssetDatabase.LoadAssetAtPath<S14PresentationDefinition>("Assets/_Game/Content/Definitions/S14_Presentation.asset");
            var motion=AssetDatabase.LoadAssetAtPath<PostDevicePresentationDefinition>("Assets/_Game/Content/VisualSlice/PostDevicePresentation.asset");
            Assert.That(audio.StepVolume/.24f,Is.InRange(.65f,.80f));
            Assert.That(.28f/audio.StepMinimumInterval,Is.InRange(.70f,.85f));
            Assert.That(1.8f/motion.RunCycleDistance,Is.InRange(.70f,.85f));
            Assert.IsNotNull(audio.StepClip);Assert.That(audio.StepClip.length,Is.LessThan(audio.StepMinimumInterval));
        }
    }
}
