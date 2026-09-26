using System;
using Gravivore.Gameplay.World;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class WorldBoundsTests
    {
        [Test]
        public void CanonicalLayout_ZonesAnchorsGatesAndBossArenaFitPlayableBounds()
        {
            var configuration = CreateConfiguration();
            var offsets = new[]
            {
                new Vector3(-1.5f, 0f, -1.5f),
                new Vector3(1.5f, 0f, -1.5f),
                new Vector3(-1.5f, 0f, 1.5f),
                new Vector3(1.5f, 0f, 1.5f)
            };

            for (var i = 0; i < configuration.ZoneCount; i++)
            {
                var center = configuration.GetZone(i).Center;
                Assert.IsTrue(configuration.Bounds.Contains(center));
                for (var anchorIndex = 0; anchorIndex < offsets.Length; anchorIndex++)
                {
                    Assert.IsTrue(configuration.Bounds.Contains(center + offsets[anchorIndex], 0.1f));
                }
            }

            Assert.IsTrue(configuration.Bounds.ContainsRectangle(
                configuration.EliteGate.Position,
                new Vector2(configuration.EliteGate.Size.x, configuration.EliteGate.Size.z)));
            Assert.IsTrue(configuration.Bounds.ContainsRectangle(
                configuration.BossGate.Position,
                new Vector2(configuration.BossGate.Size.x, configuration.BossGate.Size.z)));
            Assert.IsTrue(configuration.Bounds.ContainsCircle(
                configuration.BossArenaCenter,
                configuration.BossArenaRadius));
        }

        [Test]
        public void Bounds_UseGroundCenterAndRejectOutsidePoints()
        {
            var bounds = new WorldBounds(new Vector3(4f, 0f, 8f), new Vector2(20f, 30f));

            Assert.That(bounds.MinX, Is.EqualTo(-6f));
            Assert.That(bounds.MaxX, Is.EqualTo(14f));
            Assert.That(bounds.MinZ, Is.EqualTo(-7f));
            Assert.That(bounds.MaxZ, Is.EqualTo(23f));
            Assert.IsTrue(bounds.Contains(new Vector3(14f, 0f, 23f)));
            Assert.IsFalse(bounds.Contains(new Vector3(14.01f, 0f, 8f)));
            Assert.IsFalse(bounds.Contains(new Vector3(4f, 0f, 23f), 0.1f));
        }

        [Test]
        public void InvalidOrNonFiniteBoundaryGeometry_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorldBoundaryConfiguration(float.NaN, 2.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorldBoundaryConfiguration(0.6f, float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new WorldBounds(Vector3.zero, new Vector2(float.PositiveInfinity, 48f)));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new WorldGateConfiguration("gate", Vector3.zero, new Vector3(5f, float.NaN, 0.6f)));

            Assert.Throws<ArgumentException>(() => CreateConfiguration(
                eliteGate: new WorldGateConfiguration(
                    "elite-gate",
                    new Vector3(20f, 0f, 15f),
                    new Vector3(5f, 2.5f, 0.6f))));
            Assert.Throws<ArgumentException>(() => CreateConfiguration(
                bossArenaCenter: new Vector3(0f, 0f, 30f)));
        }

        private static Chapter01WorldConfiguration CreateConfiguration(
            WorldGateConfiguration? eliteGate = null,
            Vector3? bossArenaCenter = null)
        {
            var zones = new[]
            {
                Zone("relay-yard", -8f, 6f, -11f, 6f, Color.cyan),
                Zone("cutting-floor", 0f, 10f, 0f, 13.5f, Color.red),
                Zone("shield-dump", 8f, 6f, 11f, 6f, Color.green),
                Zone("capacitor-field", -7f, -7f, -10f, -9f, Color.yellow),
                Zone("hauler-graveyard", 7f, -7f, 10f, -9f, Color.magenta)
            };
            return new Chapter01WorldConfiguration(
                "chapter01-scrap-exclusion",
                Vector3.zero,
                new Vector3(0f, 0f, 8f),
                new Vector2(40f, 48f),
                new WorldBoundaryConfiguration(0.6f, 2.5f),
                zones,
                eliteGate ?? new WorldGateConfiguration(
                    "elite-gate", new Vector3(0f, 0f, 15f), new Vector3(5f, 2.5f, 0.6f)),
                new WorldGateConfiguration(
                    "boss-gate", new Vector3(0f, 0f, 21f), new Vector3(5f, 2.5f, 0.6f)),
                bossArenaCenter ?? new Vector3(0f, 0f, 27f),
                5f,
                "magnetar-guard",
                new EliteGateRequirement(new[] { "a", "b", "c", "d", "e" }, 5));
        }

        private static WorldZoneConfiguration Zone(
            string id, float x, float z, float landmarkX, float landmarkZ, Color color)
        {
            return new WorldZoneConfiguration(
                id,
                new Vector3(x, 0f, z),
                new Vector3(landmarkX, 0f, landmarkZ),
                color);
        }
    }
}
