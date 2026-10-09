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
        [Test] public void LiveAtlasEmissionSurvivesUrpMaterialValidation()
        {
            foreach (var name in new[] { "Slice_IndustrialAtlas", "G0_V3_Tier0", "G0_V3_Tier1", "G0_V3_Tier2" })
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(FirstVisualSliceBuilder.Root + "/Materials/" + name + ".mat");
                Assert.IsNotNull(material.GetTexture("_EmissionMap"), name);
                Assert.IsTrue((material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive) != 0, name);
                Assert.IsTrue(material.IsKeywordEnabled("_EMISSION"), name);
            }
        }
        [Test] public void AllFiveVisualDeliverablesAreReachableFromProductionScene()
        {
            Assert.DoesNotThrow(FirstVisualSliceDependencies.ValidateOrThrow);
            var deps = AssetDatabase.GetDependencies(FirstVisualSliceBuilder.ScenePath, true);
            if(Chapter01BlueprintWorldBuilder.IsBlueprintWorld())
            {
                foreach(var sector in Chapter01BlueprintWorldBuilder.ReadLayout().sectors)
                    Assert.Contains(Chapter01BlueprintWorldBuilder.Root+"/Models/R1_"+sector.model+".fbx",deps);
                foreach(var path in FirstVisualSliceDependencies.Required.Where(p=>p!=FirstVisualSliceBuilder.Prefab("Environment_Slice") && !p.EndsWith("Deck_ServiceMarkings.fbx")))
                    Assert.Contains(path,deps);
                Assert.IsFalse(deps.Contains(FirstVisualSliceBuilder.Prefab("Environment_Slice")),"Historical world is not a production dependency.");
            }
            else foreach (var path in FirstVisualSliceDependencies.Required) Assert.Contains(path, deps);
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
        public void DeviceReviewedTiersGrowWithArticulatedProductionArmor(int tier)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab("G0_V3_Live_Tier" + tier));
            var model = prefab.transform.GetChild(0);
            Assert.That(model.localScale, Is.EqualTo(Vector3.one * G0ProductionV3Review.PresentationFit * FirstVisualSliceBuilder.G0TierVisualMultipliers[tier]));
            Assert.That(prefab.GetComponentsInChildren<SkinnedMeshRenderer>().Length, Is.EqualTo(tier == 0 ? 3 : 6));
            Assert.That(Vector3.Dot(model.Find("Visible Front (-Z imported)").forward, Vector3.forward), Is.GreaterThan(.99f));
            var lods = model.GetComponent<LODGroup>().GetLODs();
            Assert.That(prefab.GetComponentsInChildren<LODGroup>(true).Length,Is.EqualTo(1),
                "Base mech and armor must share one LOD group.");
            Assert.That(lods[0].renderers.Length, Is.EqualTo(tier == 0 ? 1 : 2));
            if (tier > 0)
            {
                var armor = (SkinnedMeshRenderer)lods[0].renderers[1];
                Assert.That(armor.sharedMaterial.name,Is.EqualTo("Slice_IndustrialAtlas"), "Armor UVs must keep their authored palette.");
                Assert.That(armor.sharedMesh.triangles.Length, Is.GreaterThan(600));
                Assert.IsTrue(armor.bones.All(b => b != null && b.IsChildOf(model)));
                Assert.IsFalse(armor.bones.Any(b => b.IsChildOf(armor.transform.parent)), "Armor must use the live base rig.");
                Assert.That(((SkinnedMeshRenderer)lods[2].renderers[1]).sharedMesh.triangles.Length, Is.LessThan(armor.sharedMesh.triangles.Length));
            }
            Assert.Contains(G0ProductionV3Review.Model, AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab), true));
            Assert.IsFalse(model.GetComponent<Animator>().applyRootMotion);
        }
        [Test] public void AuthoredDeckRemainsHorizontalAndUsesOnlyThePaletteUvChannel()
        {
            var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(FirstVisualSliceBuilder.Prefab("Deck_Module")));
            try
            {
                var renderer = obj.GetComponentInChildren<Renderer>();
                Assert.That(renderer.bounds.size.x, Is.InRange(3.8f,4.1f));
                Assert.That(renderer.bounds.size.z, Is.InRange(3.8f,4.1f));
                Assert.That(renderer.bounds.size.y, Is.LessThan(.2f));
                var mesh = obj.GetComponentInChildren<MeshFilter>().sharedMesh;
                Assert.That(mesh.uv2, Is.Empty, "Default primitive UVs must not displace the industrial palette from UV0.");
                Assert.IsFalse(mesh.uv.Any(v => v.x >= .545f && v.x < .875f), "Deck must not sample hostile/cyan emission strips.");
            }
            finally { Object.DestroyImmediate(obj); }
        }
    }
}
