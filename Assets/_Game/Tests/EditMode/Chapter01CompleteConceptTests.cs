using System;
using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Gameplay.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Chapter01CompleteConceptTests
    {
        [Test] public void CompleteInstallationsUseMetresOpaquePbrAndSeparateGameplayAuthority()
        {
            Assert.DoesNotThrow(Chapter01CompleteConceptBuilder.ValidateOrThrow);
            foreach(var name in Chapter01CompleteConceptBuilder.Models)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01CompleteConceptBuilder.Prefab(name));
                Assert.That(prefab.transform.localScale,Is.EqualTo(Vector3.one));
                Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true));Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true));
                foreach(var material in prefab.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Distinct())
                {
                    Assert.That(material.renderQueue,Is.LessThanOrEqualTo(2500));Assert.AreEqual("Universal Render Pipeline/Lit",material.shader.name);
                    foreach(var map in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap","_OcclusionMap"})Assert.NotNull(material.GetTexture(map),name+" "+map);
                }
            }
        }
        [Test] public void EveryChapterRouteSegmentAndStrongObjectiveHasPhysicalClearance()
        {
            var layout=AssetDatabase.LoadAssetAtPath<ChapterWorldLayoutDefinition>(Chapter01BlueprintWorldBuilder.Root+"/Chapter01WorldLayout.asset");
            for(var i=1;i<layout.RoutePointCount;i++)
            {
                var a=layout.GetRoutePoint(i-1);var b=layout.GetRoutePoint(i);var count=Mathf.CeilToInt(Vector3.Distance(a,b)*4);
                for(var j=0;j<=count;j++)Assert.IsTrue(layout.IsClear(Vector3.Lerp(a,b,(float)j/Mathf.Max(1,count)),.65f),"segment "+i+" sample "+j);
            }
            foreach(var p in layout.StrongSpotPositions)Assert.IsTrue(layout.IsClear(p,1));
        }
        [Test] public void FunctionalMajorMassesAreDistinctAndReachableInProductionScene()
        {
            var deps=AssetDatabase.GetDependencies(Chapter01BlueprintWorldBuilder.ScenePath,true);
            var functions=new[]{"repair_mechanisms","deployment_dock","armor_transfer","transport_wrecks","magnetic_installation","containment_core"};
            for(var i=0;i<6;i++)
            {
                var name=Chapter01CompleteConceptBuilder.Models[i];Assert.Contains(Chapter01CompleteConceptBuilder.Model(name),deps);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Chapter01CompleteConceptBuilder.Prefab(name));
                Assert.IsTrue(prefab.GetComponentsInChildren<MeshRenderer>().Any(r=>r.name.Contains(functions[i])),name+" functional working centre");
            }
            Assert.Contains(CapacitorFieldProofBuilder.Model,deps);Assert.Contains(ConnectedConceptSectorsBuilder.Model("CuttingFloor"),deps);
            Assert.Contains(Chapter01CompleteConceptBuilder.Model("ChapterServiceNetwork"),deps);
        }
    }
}
