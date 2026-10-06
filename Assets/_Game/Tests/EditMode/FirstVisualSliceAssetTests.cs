using System.Linq;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class FirstVisualSliceAssetTests
    {
        [Test] public void AllFiveVisualDeliverablesAreReachableFromProductionScene()
        {
            Assert.DoesNotThrow(FirstVisualSliceDependencies.ValidateOrThrow);
            var deps = AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath, true);
            foreach (var path in FirstVisualSliceDependencies.Required) Assert.Contains(path, deps);
            Assert.IsFalse(EditorBuildSettings.scenes.Any(s => s.path.Contains("ArtReview")));
        }
        [TestCase("Scout_V1")] [TestCase("Cutter_V1")] [TestCase("Magnetar_V1")]
        public void ProductionMachinesHaveRigidLodsSharedAtlasAndInPlaceStateCoverage(string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab(name));
            Assert.DoesNotThrow(() => PresentationPrefabValidation.ValidateOrThrow(prefab));
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true));
            var animator = prefab.GetComponent<Animator>(); Assert.IsFalse(animator.applyRootMotion);
            Assert.That(animator.runtimeAnimatorController.animationClips.Select(c => c.name),
                Is.EquivalentTo(new[] { "Idle", "Run", "Attack", "Hit", "Death" }));
            var lods = prefab.GetComponent<LODGroup>().GetLODs(); Assert.That(lods.Length, Is.EqualTo(3));
            var counts = lods.Select(l => ((SkinnedMeshRenderer)l.renderers[0]).sharedMesh.triangles.Length).ToArray();
            Assert.That(counts[1], Is.LessThan(counts[0])); Assert.That(counts[2], Is.LessThan(counts[1]));
            Assert.That(prefab.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Distinct().Count(), Is.EqualTo(1));
            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                Assert.That(clip.events, Is.Empty, "Animation cannot dispatch gameplay events.");
                Assert.That(clip.length, Is.GreaterThan(.3f));
                Assert.That(clip.isLooping, Is.EqualTo(clip.name == "Idle" || clip.name == "Run"));
            }
        }
        [Test] public void DenseIndustrialKitUsesOneOpaqueAtlasWithoutScriptsLightsOrCollision()
        {
            var environment = AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab("Environment_Slice"));
            Assert.That(environment.transform.childCount, Is.GreaterThan(55));
            Assert.IsEmpty(environment.GetComponentsInChildren<MonoBehaviour>(true));
            Assert.IsEmpty(environment.GetComponentsInChildren<Collider>(true));
            Assert.IsEmpty(environment.GetComponentsInChildren<Light>(true));
            var materials = environment.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Distinct().ToArray();
            Assert.That(materials.Length, Is.EqualTo(1)); Assert.That(materials[0].renderQueue, Is.LessThan(2501));
            foreach (var kit in new[] { "Deck_Module", "Bulkhead_Module", "Hero_Reactor", "Power_Bank", "Coolant_Pump", "Maintenance_Station", "Freight_Container", "Conduit_Rack", "Structural_Support" })
                Assert.IsTrue(environment.GetComponentsInChildren<Transform>().Any(t => t.name == kit), kit);
        }
        [TestCase("Scout_V1")] [TestCase("Cutter_V1")] [TestCase("Magnetar_V1")]
        public void ImportedMechanicalStatesActuallyAnimateWithoutMovingModelRoot(string name)
        {
            var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab(name)));
            try
            {
                var animator = obj.GetComponent<Animator>(); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                var body = obj.GetComponentsInChildren<Transform>().Single(t => t.name == "BODY");
                foreach (var state in new[] { "Idle", "Run", "Attack", "Hit", "Death" })
                {
                    animator.Play(state, 0, 0); animator.Update(0); var before = body.localRotation; var position = body.localPosition;
                    animator.Play(state, 0, .4f); animator.Update(0);
                    Assert.IsTrue(Quaternion.Angle(before, body.localRotation) > .001f || Vector3.Distance(position, body.localPosition) > .001f, state);
                    Assert.That(obj.transform.position, Is.EqualTo(Vector3.zero));
                }
            }
            finally { Object.DestroyImmediate(obj); }
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void AllProgressionTiersRetainApprovedG0GeometryAndFrozenPresentationFit(int tier)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab("G0_V3_Live_Tier" + tier));
            var model = prefab.transform.GetChild(0);
            Assert.That(model.localScale, Is.EqualTo(Vector3.one * G0ProductionV3Review.PresentationFit));
            Assert.That(prefab.GetComponentsInChildren<SkinnedMeshRenderer>().Length, Is.EqualTo(3));
            Assert.Contains(G0ProductionV3Review.Model, AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab), true));
            Assert.IsFalse(model.GetComponent<Animator>().applyRootMotion);
        }
    }
}
