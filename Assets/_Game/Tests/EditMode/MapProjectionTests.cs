using Gravivore.Presentation.Map;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class MapProjectionTests
    {
        private static MapProjection CreateProjection(float radius = 28f) =>
            new MapProjection(
                MapWorldBounds.FromCenterSize(Vector2.zero, new Vector2(72f, 140f)),
                radius);

        [Test]
        public void ExpandedProjection_MapsChapterCenterToCenter()
        {
            var projection = CreateProjection();
            var result = projection.ProjectExpanded(Vector3.zero);
            Assert.That(result.x, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void ExpandedProjection_MapsWorldEdgesDeterministically()
        {
            var projection = CreateProjection();
            var minimum = projection.ProjectExpanded(new Vector3(-36f, 0f, -70f));
            var maximum = projection.ProjectExpanded(new Vector3(36f, 0f, 70f));
            Assert.That(minimum.x, Is.Zero.Within(0.0001f));
            Assert.That(minimum.y, Is.Zero.Within(0.0001f));
            Assert.That(maximum.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(maximum.y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ExpandedProjection_ClampsOutsideWorldBounds()
        {
            var projection = CreateProjection();
            var result = projection.ProjectExpanded(new Vector3(-1000f, 0f, 1000f));
            Assert.That(result.x, Is.Zero.Within(0.0001f));
            Assert.That(result.y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void CompactProjection_UsesLocalWindowAndRejectsOutsideMarkers()
        {
            var projection = CreateProjection(20f);
            Assert.IsTrue(projection.TryProjectCompact(Vector3.zero, Vector3.zero, out var center));
            Assert.That(center.x, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(center.y, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.IsTrue(projection.TryProjectCompact(new Vector3(20f, 0f, 0f), Vector3.zero, out var edge));
            Assert.That(edge.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.IsFalse(projection.TryProjectCompact(new Vector3(20.01f, 0f, 0f), Vector3.zero, out _));
        }

        [Test]
        public void CompactWindow_ClampsToChapterEdgeInsteadOfShowingBlankSpace()
        {
            var projection = CreateProjection(20f);
            var player = new Vector3(36f, 0f, 70f);
            var window = projection.GetLocalWindow(player);
            Assert.That(window.MaxX, Is.EqualTo(36f).Within(0.0001f));
            Assert.That(window.MaxZ, Is.EqualTo(70f).Within(0.0001f));
            Assert.IsTrue(projection.TryProjectCompact(player, player, out var playerAnchor));
            Assert.That(playerAnchor.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(playerAnchor.y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void NorthUp_IsStableAndHeadingDoesNotRotateProjection()
        {
            var projection = CreateProjection(28f);
            var player = Vector3.zero;
            Assert.IsTrue(projection.TryProjectCompact(new Vector3(0f, 0f, 10f), player, out var north));
            Assert.IsTrue(projection.TryProjectCompact(new Vector3(10f, 0f, 0f), player, out var east));
            Assert.That(north.y, Is.GreaterThan(0.5f));
            Assert.That(north.x, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(east.x, Is.GreaterThan(0.5f));
            Assert.That(east.y, Is.EqualTo(0.5f).Within(0.0001f));
        }
    }
}
