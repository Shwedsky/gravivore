using System.IO;
using Gravivore.Presentation.Composition;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class ProjectValidatorTests
    {
        [OneTimeSetUp]
        public void ConfigureCleanCheckout()
        {
            Gravivore.Editor.ProjectConfigurator.ConfigureOrThrow();
        }

        [Test]
        public void ValidateOrThrow_DoesNotRepairInvalidOrientation()
        {
            Gravivore.Editor.ProjectValidator.ValidateOrThrow();
            var originalOrientation = PlayerSettings.defaultInterfaceOrientation;

            try
            {
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

                var exception = Assert.Throws<System.InvalidOperationException>(
                    () => Gravivore.Editor.ProjectValidator.ValidateOrThrow());

                StringAssert.Contains("orientation", exception.Message.ToLowerInvariant());
                Assert.AreEqual(UIOrientation.LandscapeLeft, PlayerSettings.defaultInterfaceOrientation);
            }
            finally
            {
                PlayerSettings.defaultInterfaceOrientation = originalOrientation;
            }
        }

        [Test]
        public void PresentationMaterialPalette_RejectsMissingMaterials()
        {
            var palette = ScriptableObject.CreateInstance<PresentationMaterialPalette>();
            try
            {
                Assert.Throws<System.InvalidOperationException>(() => palette.ValidateOrThrow());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(palette);
            }
        }

        [Test]
        public void CanonicalPresentationPalette_HasRequiredUrpMaterials()
        {
            var palette = AssetDatabase.LoadAssetAtPath<PresentationMaterialPalette>(
                Gravivore.Editor.PresentationMaterialAssetConfigurator.PalettePath);

            Assert.IsNotNull(palette);
            Assert.DoesNotThrow(() => palette.ValidateOrThrow());
            Assert.That(palette.LitMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
            Assert.That(palette.UnlitMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Unlit"));
        }

        [Test]
        public void ProductionRuntime_DoesNotUseShaderFind()
        {
            foreach (var path in Directory.GetFiles("Assets/_Game/Runtime", "*.cs", SearchOption.AllDirectories))
            {
                StringAssert.DoesNotContain("Shader.Find(", File.ReadAllText(path), path);
            }
        }

        [Test]
        public void ProductionRuntime_DoesNotUseResourcesLoad()
        {
            foreach (var path in Directory.GetFiles("Assets/_Game/Runtime", "*.cs", SearchOption.AllDirectories))
            {
                StringAssert.DoesNotContain("Resources.Load", File.ReadAllText(path), path);
            }
        }
    }
}
