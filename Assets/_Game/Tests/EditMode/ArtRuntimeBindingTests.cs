using System;
using System.Linq;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Evolution;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ArtRuntimeBindingTests
    {
        private const string Art = "Assets/_Game/ArtSpike/Prefabs/";
        private static EvolutionDefinition Definition => AssetDatabase.LoadAssetAtPath<EvolutionDefinition>(
            "Assets/_Game/Content/Definitions/S07_Evolution.asset");

        [Test]
        public void ExistingEvolutionDefinitionBindsThreeDistinctPresentationOnlyForms()
        {
            var catalog = Definition.Catalog;
            Assert.That(catalog.Selection.Tier1Threshold, Is.EqualTo(20));
            Assert.That(catalog.Selection.Tier2Threshold, Is.EqualTo(42));
            Assert.That(catalog.TierPrefabs.Length, Is.EqualTo(3));
            for (var tier = 0; tier < 3; tier++)
            {
                var prefab = catalog.TierPrefabs[tier];
                Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(Art + $"Player/G0_Tier{tier}_ArtSpike.prefab"));
                Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.That(prefab.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(prefab.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            }
        }

        [Test]
        public void OnlyCutterRecipeOverridesTheExistingS15PartsAndLandmarks()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(Gravivore.Editor.S15AssetConfigurator.CatalogPath);
            foreach (var id in new[] { "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier" })
            {
                Assert.IsTrue(catalog.TryGetEnemy(id, out var recipe));
                if (id == "cutter-unit")
                    Assert.That(AssetDatabase.GetAssetPath(recipe.PresentationPrefab), Is.EqualTo(Art + "Enemies/Cutter_ArtSpike.prefab"));
                else
                    Assert.That(recipe.PresentationPrefab, Is.Null, id);
                for (var i = 0; i < recipe.PartCount; i++)
                    Assert.That(AssetDatabase.GetAssetPath(recipe.GetPart(i).SourceModel),
                        Does.StartWith(Gravivore.Editor.S15AssetConfigurator.ModelRoot));
            }
            foreach (var id in new[] { "relay-yard", "cutting-floor", "shield-dump", "capacitor-field", "hauler-graveyard" })
                Assert.That(catalog.GetLandmark(id).PresentationPrefab, Is.Null, id);
            foreach (var name in new[] { "S09_MagnetarGuard", "S09_CustodianM0", "S08_Chapter01World" })
                Assert.IsFalse(AssetDatabase.GetDependencies("Assets/_Game/Content/Definitions/" + name + ".asset", true)
                    .Any(p => p.StartsWith("Assets/_Game/ArtSpike/")));
        }

        [Test]
        public void CurrentS20CameraAndGameplayBuildScenesExcludeTheArtBay()
        {
            var camera = AssetDatabase.LoadAssetAtPath<CameraFollowSettings>("Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
            Assert.That(camera.Offset, Is.EqualTo(new Vector3(0, 14.8f, -11.2f)));
            Assert.That(camera.FieldOfView, Is.EqualTo(46));
            CollectionAssert.AreEqual(new[] { "Assets/_Game/Content/Scenes/Bootstrap.unity",
                "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity" }, Gravivore.Editor.Build.AndroidBuild.BuildScenes);
        }

        [TestCase("incomplete")]
        [TestCase("duplicate")]
        [TestCase("physics")]
        [TestCase("authority")]
        public void WholeFormCatalogRejectsInvalidOrAuthoritativePrefabs(string invalid)
        {
            var original = Definition.Catalog;
            var forms = (GameObject[])original.TierPrefabs.Clone();
            GameObject unsafeForm = null;
            try
            {
                if (invalid == "incomplete") forms = new[] { forms[0] };
                if (invalid == "duplicate") forms[1] = forms[0];
                if (invalid == "physics" || invalid == "authority")
                {
                    unsafeForm = UnityEngine.Object.Instantiate(forms[0]);
                    if (invalid == "physics") unsafeForm.AddComponent<BoxCollider>();
                    else unsafeForm.AddComponent<PlayerEvolutionView>();
                    forms[0] = unsafeForm;
                }
                Assert.Throws<InvalidOperationException>(() => new EvolutionVisualCatalog(
                    original.Selection, original.TierModuleSets, original.AccentModules, forms));
            }
            finally { if (unsafeForm != null) UnityEngine.Object.DestroyImmediate(unsafeForm); }
        }
    }
}
