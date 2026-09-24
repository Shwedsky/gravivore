using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class SafeAreaHudRootTests
    {
        [Test]
        public void CalculateAnchors_ConvertsSafeAreaToNormalizedPortraitAnchors()
        {
            SafeAreaHudRoot.CalculateAnchors(
                new Rect(0f, 100f, 1080f, 2200f),
                new Vector2Int(1080, 2400),
                out var anchorMin,
                out var anchorMax);

            Assert.That(anchorMin.x, Is.Zero.Within(0.0001f));
            Assert.That(anchorMin.y, Is.EqualTo(100f / 2400f).Within(0.0001f));
            Assert.That(anchorMax.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(anchorMax.y, Is.EqualTo(2300f / 2400f).Within(0.0001f));
        }

        [Test]
        public void CalculateAnchors_InvalidScreenSize_FallsBackToFullScreen()
        {
            SafeAreaHudRoot.CalculateAnchors(
                new Rect(0f, 0f, 100f, 100f),
                Vector2Int.zero,
                out var anchorMin,
                out var anchorMax);

            Assert.AreEqual(Vector2.zero, anchorMin);
            Assert.AreEqual(Vector2.one, anchorMax);
        }
    }
}
