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

            StringAssert.Contains("Мощность  ур. 2", text);
            StringAssert.Contains("Корпус  ур. 3", text);
            StringAssert.Contains("Броня  ур. 4", text);
            StringAssert.Contains("Поток  ур. 5", text);
            StringAssert.Contains("Манёвренность  ур. 6", text);
            StringAssert.DoesNotContain("Урон", text);
        }

        [Test]
        public void Stats_ExpandedAddsReadableDerivedValuesWithoutFormula()
        {
            var text = HudTextFormatter.Stats(
                new PlayerStatLevels(1, 1, 1, 1, 1),
                new PlayerDerivedStats(12f, 130f, 4f, 0.8f, 5f),
                true);

            StringAssert.Contains("Урон  12", text);
            StringAssert.Contains("Макс. прочность  130", text);
            StringAssert.Contains("Атака  0.8 с", text);
            StringAssert.DoesNotContain("=", text);
        }

        [TestCase(30, "0 мин")]
        [TestCase(600, "10 мин")]
        [TestCase(7500, "2 ч 5 мин")]
        public void OfflineDuration_UsesCompactPhoneFriendlyText(int seconds, string expected)
        {
            Assert.That(HudTextFormatter.OfflineDuration(TimeSpan.FromSeconds(seconds)), Is.EqualTo(expected));
        }
    }
}
