using System.Linq;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Phase3VisualWorldBaselineTests
    {
        private const string WorldDefinitionPath = "Assets/_Game/Content/Definitions/S08_Chapter01World.asset";
        private const string CatalogPath = "Assets/_Game/Content/Definitions/S15_VisualCatalog.asset";

        [Test]
        public void ChapterBaselineBuildsFiveLandmarksAndNoPresentationColliderAuthority()
        {
            var parent = new GameObject("Phase3 World Test Root");
            var material = CreateTestMaterial();
            try
            {
                var world = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(WorldDefinitionPath);
                Assert.That(world, Is.Not.Null);

                var report = Chapter01VisualWorldBaseline.Build(parent.transform, world.Configuration, material);

                Assert.That(report.ZoneLandmarkCount, Is.EqualTo(5));
                Assert.That(report.RendererCount, Is.InRange(120, 180));
                Assert.That(report.EnabledColliderCount, Is.Zero);
                Assert.That(report.Root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.That(report.Root.transform.Find("Repair Hub"), Is.Not.Null);
                Assert.That(report.Root.transform.Find("Elite Approach"), Is.Not.Null);
                Assert.That(report.Root.transform.Find("Boss Destination"), Is.Not.Null);

                foreach (var id in new[]
                         {
                             "relay-yard", "cutting-floor", "shield-dump",
                             "capacitor-field", "hauler-graveyard"
                         })
                {
                    Assert.That(report.Root.transform.Find($"Zone Landmark [{id}]"), Is.Not.Null, id);
                }
            }
            finally
            {
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void OrdinaryEnemyFamiliesHaveDistinctHardSurfacePresentationsWithoutEnabledColliders()
        {
            var parent = new GameObject("Phase3 Enemy Test Root");
            var material = CreateTestMaterial();
            try
            {
                var ids = new[] { "scout-drone", "cutter-unit", "arc-drone", "warden", "carrier" };
                var rendererCounts = ids
                    .Select(id =>
                    {
                        var visual = Phase3EnemyVisualFactory.BuildPreview(parent.transform, id, material);
                        Assert.That(IndustrialPrimitiveFactory.CountEnabledColliders(visual), Is.Zero, id);
                        Assert.That(visual.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty, id);
                        var renderers = visual.GetComponentsInChildren<Renderer>(true);
                        Assert.That(renderers.Length, Is.InRange(7, 20), id);
                        return renderers.Length;
                    })
                    .ToArray();

                Assert.That(rendererCounts.Distinct().Count(), Is.GreaterThanOrEqualTo(3),
                    "Enemy families should not collapse to one repeated body recipe.");
            }
            finally
            {
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void RuntimeFactoryPreservesAcceptedCutterArtV3Override()
        {
            var parent = new GameObject("Phase3 Runtime Factory Test Root");
            var material = CreateTestMaterial();
            try
            {
                var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(CatalogPath);
                Assert.That(catalog, Is.Not.Null);
                var state = new Phase3EnemyVisualState(parent.transform, catalog, material);

                state.Apply("cutter-unit");

                Assert.That(state.ActiveId, Is.EqualTo("cutter-unit"));
                Assert.That(parent.GetComponentsInChildren<Transform>(true)
                    .Any(t => t.name.Contains("accepted ART V3")), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void EliteAndBossArePresentationOnlyAndBossHasDominantIndustrialMass()
        {
            var parent = new GameObject("Phase3 Encounter Test Root");
            var material = CreateTestMaterial();
            try
            {
                var elite = Phase3EncounterVisualFactory.BuildMagnetarGuard(parent.transform, material);
                var boss = Phase3EncounterVisualFactory.BuildCustodianM0(parent.transform, material);

                Assert.That(IndustrialPrimitiveFactory.CountEnabledColliders(elite), Is.Zero);
                Assert.That(IndustrialPrimitiveFactory.CountEnabledColliders(boss), Is.Zero);
                Assert.That(elite.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.That(boss.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);

                var eliteRenderers = elite.GetComponentsInChildren<Renderer>(true);
                var bossRenderers = boss.GetComponentsInChildren<Renderer>(true);
                Assert.That(eliteRenderers.Length, Is.GreaterThanOrEqualTo(12));
                Assert.That(bossRenderers.Length, Is.GreaterThan(eliteRenderers.Length));

                var eliteBounds = CombineBounds(eliteRenderers);
                var bossBounds = CombineBounds(bossRenderers);
                Assert.That(bossBounds.size.x, Is.GreaterThan(eliteBounds.size.x * 1.5f));
                Assert.That(bossBounds.size.z, Is.GreaterThan(eliteBounds.size.z * 1.35f));
            }
            finally
            {
                Object.DestroyImmediate(parent);
                Object.DestroyImmediate(material);
            }
        }

        private static Material CreateTestMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Assert.That(shader, Is.Not.Null, "A lit shader is required for presentation tests.");
            return new Material(shader);
        }

        private static Bounds CombineBounds(Renderer[] renderers)
        {
            Assert.That(renderers, Is.Not.Empty);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}
