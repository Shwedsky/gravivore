using System.Linq;
using Gravivore.Editor;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class Phase3CIntegrationTests
    {
        [TestCase("scout-drone", "Scout_V1")]
        [TestCase("cutter-unit", "Cutter_V1")]
        [TestCase("arc-drone", "ArcDrone_V1")]
        [TestCase("warden", "Warden_V1")]
        [TestCase("carrier", "Carrier_V1")]
        public void CanonicalOrdinaryBindingsRetainFallbackDataAndSafeMeshes(string id, string model)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(S15AssetConfigurator.CatalogPath);
            Assert.IsTrue(catalog.TryGetEnemy(id,out var recipe));
            Assert.That(recipe.PresentationPrefab.name, Is.EqualTo(model));
            Assert.That(recipe.PartCount, Is.GreaterThan(0));
            Assert.DoesNotThrow(() => PresentationPrefabValidation.ValidateOrThrow(recipe.PresentationPrefab));
            Assert.That(recipe.PresentationPrefab.GetComponentsInChildren<MeshFilter>(true).All(f => f.sharedMesh != null), Is.True);
        }

        [Test] public void PackIntakeHasNoBrokenReferencesAndEncounterBindingsAreCanonical()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { Phase3CIntegrationBuilder.PackRoot.TrimEnd('/') }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var report = ArtIntakeTool.Inspect(AssetDatabase.LoadAssetAtPath<GameObject>(path), ArtCandidateRole.Environment);
                Assert.That(report.runtimeSafe, Is.True, path);
                Assert.That(report.missingMeshes + report.missingMaterials + report.brokenTextureReferences, Is.Zero, path);
            }
            var definition = AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>(VisualIntegrationFoundationBuilder.DefinitionPath);
            Assert.That(definition.Elite.Prefab.name, Is.EqualTo("Magnetar_V1"));
            Assert.That(definition.Boss.Prefab.name, Is.EqualTo("Custodian_V2"));
            Assert.That(definition.RepairHub.Prefab.name, Is.EqualTo("Repair_Platform_V2"));
        }
    }
}
