using System;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.UI;
using NUnit.Framework;

namespace Gravivore.Tests.EditMode
{
    public sealed class HudTextFormatterTests
    {
        [Test]
        public void Stats_CompactListsExactlyFivePermanentStats()
        {
            var text = HudTextFormatter.Stats(
                new PlayerStatLevels(2, 3, 4, 5, 6),
                new PlayerDerivedStats(12f, 130f, 4f, 0.8f, 5f),
                false);

            StringAssert.Contains("Power  L2", text);
            StringAssert.Contains("Hull  L3", text);
            StringAssert.Contains("Armor  L4", text);
            StringAssert.Contains("Flux  L5", text);
            StringAssert.Contains("Mobility  L6", text);
            StringAssert.DoesNotContain("Damage", text);
        }

        [Test]
        public void Stats_ExpandedAddsReadableDerivedValuesWithoutFormula()
        {
            var text = HudTextFormatter.Stats(
                new PlayerStatLevels(1, 1, 1, 1, 1),
                new PlayerDerivedStats(12f, 130f, 4f, 0.8f, 5f),
                true);

            StringAssert.Contains("Damage  12", text);
            StringAssert.Contains("Max HP  130", text);
            StringAssert.Contains("Attack  0.8s", text);
            StringAssert.DoesNotContain("=", text);
        }

        [TestCase(30, "0m")]
        [TestCase(600, "10m")]
        [TestCase(7500, "2h 5m")]
        public void OfflineDuration_UsesCompactPhoneFriendlyText(int seconds, string expected)
        {
            Assert.That(HudTextFormatter.OfflineDuration(TimeSpan.FromSeconds(seconds)), Is.EqualTo(expected));
        }
    }
}
