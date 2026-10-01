using System.IO;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class RussianUiTextTests
    {
        [Test]
        public void LegacyRuntimeFont_ContainsRequiredCyrillicGlyphs()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Assert.IsNotNull(font);
            Assert.IsTrue(RussianUiText.FontSupportsCyrillic(font));
        }

        [Test]
        public void RuntimeUi_DoesNotContainOldMajorEnglishPlayerFacingLabels()
        {
            var roots = new[]
            {
                "Assets/_Game/Runtime/Presentation/UI",
                "Assets/_Game/Runtime/Presentation/Quests"
            };
            var forbidden = new[]
            {
                "\"PAUSED\"",
                "\"WELCOME BACK\"",
                "\"VERTICAL SLICE COMPLETE\"",
                "\"CONTINUE EXPLORING\"",
                "\"CUSTODIAN M-0\"",
                "STAT DETAILS:",
                "Power  L",
                "Hull  L",
                "Assimilation {"
            };

            for (var rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                var files = Directory.GetFiles(roots[rootIndex], "*.cs", SearchOption.AllDirectories);
                for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
                {
                    var source = File.ReadAllText(files[fileIndex]);
                    for (var labelIndex = 0; labelIndex < forbidden.Length; labelIndex++)
                    {
                        StringAssert.DoesNotContain(forbidden[labelIndex], source, files[fileIndex]);
                    }
                }
            }
        }
    }
}
