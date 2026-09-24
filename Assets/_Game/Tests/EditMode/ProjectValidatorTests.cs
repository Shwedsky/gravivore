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
    }
}
