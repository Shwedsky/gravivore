using System;
using System.Collections.Generic;
using Gravivore.Presentation.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class S15AssetIntegrationTests
    {
        [OneTimeSetUp]
        public void ConfigureAssets()
        {
            Gravivore.Editor.S15AssetConfigurator.ConfigureOrThrow();
        }

        [Test]
        public void CanonicalCatalog_HasPlayerFiveDistinctEnemiesAndFiveLandmarks()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(
                Gravivore.Editor.S15AssetConfigurator.CatalogPath);
            Assert.IsNotNull(catalog);
            Assert.DoesNotThrow(catalog.ValidateOrThrow);
            Assert.That(catalog.Player.PartCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(catalog.EnemyCount, Is.EqualTo(5));
            Assert.That(catalog.LandmarkCount, Is.EqualTo(5));

            var expected = new HashSet<string>
            {
                "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier"
            };
            var signatures = new HashSet<string>();
            foreach (var id in expected)
            {
                Assert.IsTrue(catalog.TryGetEnemy(id, out var recipe), id);
                Assert.That(recipe.PartCount, Is.GreaterThanOrEqualTo(2), id);
                var signature = recipe.PartCount.ToString();
                for (var i = 0; i < recipe.PartCount; i++)
                {
                    var part = recipe.GetPart(i);
                    signature += $"|{part.SourceModel.name}:{part.LocalPosition}:{part.LocalScale}";
                }
                Assert.IsTrue(signatures.Add(signature), $"Enemy {id} duplicates another visual shape.");
            }
        }

        [Test]
        public void EnemyVisualState_RejectsUnknownEnemyInsteadOfUsingFallback()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(
                Gravivore.Editor.S15AssetConfigurator.CatalogPath);
            var root = new GameObject("S15 Missing Visual Test");
            try
            {
                var state = new S15EnemyVisualState(root.transform, catalog);
                Assert.Throws<InvalidOperationException>(() => state.Apply("missing-enemy"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ImportedModels_UseAuditedMobileSettings()
        {
            var guids = AssetDatabase.FindAssets("t:Model", new[]
            {
                Gravivore.Editor.S15AssetConfigurator.ModelRoot.TrimEnd('/')
            });
            Assert.That(guids, Has.Length.EqualTo(14));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);
                Assert.IsFalse(importer.importAnimation, path);
                Assert.IsFalse(importer.isReadable, path);
                Assert.That(importer.materialImportMode, Is.EqualTo(ModelImporterMaterialImportMode.None), path);
                Assert.That(importer.meshCompression, Is.EqualTo(ModelImporterMeshCompression.Medium), path);
            }
        }

        [TestCase("piston-round.fbx")]
        [TestCase("robot-arm-a.fbx")]
        public void VisualFactory_PreservesEveryRenderableMesh(string modelName)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(
                Gravivore.Editor.S15AssetConfigurator.CatalogPath);
            var part = FindPart(catalog, modelName);
            var sourceMeshCount = part.SourceModel.GetComponentsInChildren<MeshFilter>(true).Length;
            var root = new GameObject($"S15 {modelName} Hierarchy Test");
            try
            {
                var built = S15VisualFactory.BuildPart(root.transform, part, catalog, 0);
                Assert.That(
                    built.GetComponentsInChildren<MeshFilter>(true),
                    Has.Length.EqualTo(sourceMeshCount));
                Assert.That(built.GetComponentsInChildren<Collider>(true), Is.Empty);
                foreach (var renderer in built.GetComponentsInChildren<MeshRenderer>(true))
                {
                    Assert.That(renderer.sharedMaterials, Is.Not.Empty);
                    Assert.That(renderer.sharedMaterials, Has.None.Null);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CatalogCoverage_RejectsMissingEnemyAndLandmarkIds()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(
                Gravivore.Editor.S15AssetConfigurator.CatalogPath);
            var enemies = new[] { "scout-drone", "cutter-unit", "warden", "arc-drone", "missing-enemy" };
            var landmarks = new[] { "relay-yard", "cutting-floor", "shield-dump", "capacitor-field", "hauler-graveyard" };
            Assert.Throws<InvalidOperationException>(() => catalog.ValidateCoverageOrThrow(enemies, landmarks));

            enemies[4] = "carrier";
            landmarks[4] = "missing-landmark";
            Assert.Throws<InvalidOperationException>(() => catalog.ValidateCoverageOrThrow(enemies, landmarks));
        }

        private static S15VisualPart FindPart(S15VisualCatalog catalog, string modelName)
        {
            var assetName = System.IO.Path.GetFileNameWithoutExtension(modelName);
            for (var i = 0; i < catalog.Player.PartCount; i++)
            {
                var part = catalog.Player.GetPart(i);
                if (string.Equals(part.SourceModel.name, assetName, StringComparison.OrdinalIgnoreCase)) return part;
            }

            var enemyIds = new[] { "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier" };
            for (var i = 0; i < enemyIds.Length; i++)
            {
                catalog.TryGetEnemy(enemyIds[i], out var recipe);
                for (var j = 0; j < recipe.PartCount; j++)
                {
                    var part = recipe.GetPart(j);
                    if (string.Equals(part.SourceModel.name, assetName, StringComparison.OrdinalIgnoreCase)) return part;
                }
            }

            throw new InvalidOperationException($"Canonical S15 part not found: {modelName}");
        }
    }
}
