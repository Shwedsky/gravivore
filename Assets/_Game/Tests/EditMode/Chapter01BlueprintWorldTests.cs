using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter01BlueprintWorldTests
    {
        [Test] public void RebuiltProductionWorldHasEightSectorsAndNoHistoricalLayers() => Chapter01BlueprintWorldBuilder.Audit();
        [Test] public void RouteHasContinuousGroundAndWideArchitecturalClearance()
        {
            var world = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            var layout = world.Layout; Assert.NotNull(layout); layout.ValidateOrThrow();
            for (var i=1;i<layout.RoutePointCount;i++)
            {
                var a=layout.GetRoutePoint(i-1); var b=layout.GetRoutePoint(i);
                var count=Mathf.CeilToInt(Vector3.Distance(a,b)/.15f);
                for(var j=0;j<=count;j++) Assert.IsTrue(layout.IsClear(Vector3.Lerp(a,b,(float)j/count),.65f),$"Route {i} sample {j}");
            }
        }
        [Test] public void FiveEncounterIdentitiesAndAllSpawnAnchorsStayOnOpenGround()
        {
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            var spots=AssetDatabase.FindAssets("t:SpawnSpotDefinition",new[]{"Assets/_Game/Content/Definitions"})
                .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<SpawnSpotDefinition>).ToArray();
            CollectionAssert.AreEquivalent(new[]{"relay-yard","cutting-floor","shield-dump","capacitor-field","hauler-graveyard"},spots.Select(s=>s.Id));
            foreach(var spot in spots)
            {
                var config=spot.CreateRuntimeConfiguration();
                Assert.That(config.Population.DesiredPopulation,Is.EqualTo(4));
                Assert.That(config.WaveCooldownSeconds,Is.EqualTo(120));
                for(var i=0;i<config.AnchorOffsets.Length;i++) Assert.IsTrue(world.Layout.IsClear(config.GetAnchorWorldPosition(i),.65f),spot.Id);
            }
            foreach(var position in world.Layout.StrongSpotPositions) Assert.IsTrue(world.Layout.IsClear(position,.8f));
        }
        [Test] public void EverySectorUsesRichAuthoredMeshesWithSeparateCollisionAuthority()
        {
            foreach(var sector in Chapter01BlueprintWorldBuilder.ReadLayout().sectors)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01BlueprintWorldBuilder.Root+"/Prefabs/R1_"+sector.model+".prefab");
                Assert.NotNull(prefab); Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));
                Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true));
                foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())
                    foreach(var material in renderer.sharedMaterials)
                        foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap"}) Assert.NotNull(material.GetTexture(map),sector.id+" "+map);
            }
        }
        [Test] public void RelocatedStrongSpotsPreserveTiersTimersRewardsAndGateAccess()
        {
            var world=AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>("Assets/_Game/Content/Definitions/S08_Chapter01World.asset");
            var baseline=Chapter1StrongOrdinarySpotCatalog.Create(world.Configuration);
            var relocated=Chapter1StrongOrdinarySpotCatalog.Create(world.Configuration,null,world.Layout.StrongSpotPositions);
            Assert.That(relocated.Count,Is.EqualTo(baseline.Count));
            for(var i=0;i<baseline.Count;i++)
            {
                Assert.That(relocated[i].Id,Is.EqualTo(baseline[i].Id));
                Assert.That(relocated[i].Region,Is.EqualTo(baseline[i].Region));
                Assert.That(relocated[i].HealthMultiplier,Is.EqualTo(baseline[i].HealthMultiplier));
                Assert.That(relocated[i].DamageMultiplier,Is.EqualTo(baseline[i].DamageMultiplier));
                Assert.That(relocated[i].RewardMultiplier,Is.EqualTo(baseline[i].RewardMultiplier));
                Assert.That(relocated[i].BaseRespawnSeconds,Is.EqualTo(baseline[i].BaseRespawnSeconds));
                Assert.That(relocated[i].PressurePolicy,Is.EqualTo(baseline[i].PressurePolicy));
                var gate=i<2?world.Configuration.EliteGate:world.Configuration.BossGate;
                Assert.That(relocated[i].Position.z<gate.Position.z,Is.EqualTo(baseline[i].Position.z<gate.Position.z));
            }
        }
    }
}
