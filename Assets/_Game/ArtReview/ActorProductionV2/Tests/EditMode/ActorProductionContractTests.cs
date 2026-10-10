using System.Linq;
using Gravivore.Editor.ActorProduction;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.ArtReview.Tests
{
    public sealed class ActorProductionContractTests
    {
        private static GameObject Prefab(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(ActorProductionReviewBuilder.Root + "/Prefabs/" + name + "_V2_Review.prefab");
        private static Bounds WorldBounds(SkinnedMeshRenderer renderer)
        {
            var local = renderer.sharedMesh.bounds;
            var result = new Bounds(renderer.transform.TransformPoint(local.center), Vector3.zero);
            for (var x = -1; x <= 1; x += 2)
                for (var y = -1; y <= 1; y += 2)
                    for (var z = -1; z <= 1; z += 2)
                        result.Encapsulate(renderer.transform.TransformPoint(local.center + Vector3.Scale(local.extents, new Vector3(x, y, z))));
            return result;
        }
        [Test] public void CompleteFamilyHasConsolidatedLodsClipsSocketsAndNoProductionDependency()
        { Assert.DoesNotThrow(ActorProductionReviewBuilder.ValidateAssets); }
        [TestCase("Scout")][TestCase("Cutter")][TestCase("Warden")][TestCase("ArcDrone")][TestCase("Carrier")][TestCase("Magnetar")][TestCase("Custodian")]
        public void LodGeometryDecreasesWithoutLosingBoneBindings(string actor)
        {
            var lods = Prefab(actor).GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(x => x.name).ToArray();
            for (var i = 1; i < lods.Length; i++)
            {
                Assert.Less(lods[i].sharedMesh.triangles.Length, lods[i - 1].sharedMesh.triangles.Length);
                CollectionAssert.AreEquivalent(lods[0].bones.Select(x => x.name), lods[i].bones.Select(x => x.name));
                Assert.AreEqual(lods[i].sharedMesh.vertexCount, lods[i].sharedMesh.uv.Length);
            }
        }
        [Test] public void WardenShieldsFollowSeparateForearmControlChains()
        {
            var transforms = Prefab("Warden").GetComponentsInChildren<Transform>(true);
            foreach (var side in new[] { "L", "R" })
            {
                var pivot = transforms.Single(x => x.name == "ShieldPivot_" + side);
                Assert.AreEqual(side + "_TOOL", pivot.parent.name);
                Assert.AreEqual(side + "_ELBOW", pivot.parent.parent.name);
                Assert.AreEqual(side + "_SHOULDER", pivot.parent.parent.parent.name);
            }
        }
        [Test] public void ScoutHasTwoIntegratedDistalBladeSockets()
        {
            var transforms = Prefab("Scout").GetComponentsInChildren<Transform>(true);
            foreach (var side in new[] { "L", "R" })
            {
                var tip = transforms.Single(x => x.name == "BladeTip_" + side);
                var root = transforms.Single(x => x.name == "BladeRoot_" + side);
                Assert.AreEqual(side + "_TOOL", tip.parent.name);
                Assert.Greater(Vector3.Distance(tip.position, root.position), .35f);
            }
        }
        [Test] public void DroneHasFlightClearanceAndCarrierHasNoWalkingChains()
        {
            var drone = Prefab("ArcDrone").GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(x => x.name).First();
            Assert.Greater(WorldBounds(drone).min.y, .15f);
            var carrier = Prefab("Carrier"); var names = carrier.GetComponentsInChildren<Transform>(true).Select(x => x.name).ToArray();
            Assert.IsFalse(names.Any(x => x.Contains("HIP") || x.StartsWith("SUPPORT_")));
            var bounds = WorldBounds(carrier.GetComponentsInChildren<SkinnedMeshRenderer>(true).First());
            Assert.Greater(bounds.size.z, bounds.size.y * 2);
        }
    }
}
