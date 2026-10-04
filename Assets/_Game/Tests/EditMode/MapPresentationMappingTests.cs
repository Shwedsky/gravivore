using Gravivore.Presentation.Map;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class MapPresentationMappingTests
    {
        [TestCase(0f, "00:00")]
        [TestCase(59f, "00:59")]
        [TestCase(60f, "01:00")]
        [TestCase(3599f, "59:59")]
        [TestCase(3600f, "1:00:00")]
        [TestCase(3754f, "1:02:34")]
        public void ExactTimer_FormatsCountdownWithoutNegativeValues(float seconds, string expected)
        {
            Assert.That(MapTimerFormatter.FormatExact(seconds), Is.EqualTo(expected));
        }

        [TestCase(-1f, "готово")]
        [TestCase(0f, "готово")]
        [TestCase(59f, "<1 мин")]
        [TestCase(60f, "1 мин")]
        [TestCase(754f, "12 мин")]
        [TestCase(3900f, "1 ч 5 мин")]
        public void RoundedTimer_UsesCoarseExpandedMapText(float seconds, string expected)
        {
            Assert.That(MapTimerFormatter.FormatRounded(seconds), Is.EqualTo(expected));
        }

        [Test]
        public void IconAndStatusMapping_UsesShapeAndOverlayNotColorAlone()
        {
            var ordinaryCooldown = new MapMarkerSnapshot(
                "spot", MapMarkerKind.Ordinary, Vector3.zero, MapAvailabilityState.Cooldown,
                remainingSeconds: 30f);
            var lockedElite = new MapMarkerSnapshot(
                "elite", MapMarkerKind.Elite, Vector3.zero, MapAvailabilityState.Inactive,
                progressionLocked: true);
            var strong = new MapMarkerSnapshot(
                "strong", MapMarkerKind.StrongOrdinary, Vector3.zero, MapAvailabilityState.Available);
            var rewardBoss = new MapMarkerSnapshot(
                "boss", MapMarkerKind.Boss, Vector3.zero, MapAvailabilityState.Available,
                rewardState: MapRewardState.CappedFallback);

            var cooldownVisual = MapMarkerVisualResolver.Resolve(ordinaryCooldown, false);
            Assert.That(cooldownVisual.Glyph, Is.EqualTo(MapMarkerGlyph.Node));
            Assert.IsTrue(cooldownVisual.CooldownRing);
            Assert.IsFalse(cooldownVisual.Filled);

            var lockedVisual = MapMarkerVisualResolver.Resolve(lockedElite, false);
            Assert.That(lockedVisual.Glyph, Is.EqualTo(MapMarkerGlyph.Diamond));
            Assert.IsTrue(lockedVisual.LockOverlay);
            Assert.IsFalse(lockedVisual.InactiveSlash, "Locked is a distinct overlay state, not generic inactive.");

            var defeatedBoss = new MapMarkerSnapshot(
                "defeated-boss", MapMarkerKind.Boss, Vector3.zero, MapAvailabilityState.Defeated);
            var defeatedBossVisual = MapMarkerVisualResolver.Resolve(defeatedBoss, true);
            Assert.IsTrue(defeatedBossVisual.CompletionMark);
            Assert.IsFalse(defeatedBossVisual.InactiveSlash);

            Assert.That(MapMarkerVisualResolver.Resolve(strong, false).Glyph, Is.EqualTo(MapMarkerGlyph.BracketedNode));
            Assert.IsFalse(MapMarkerVisualResolver.Resolve(rewardBoss, false).RewardPip);
            Assert.IsTrue(MapMarkerVisualResolver.Resolve(rewardBoss, true).RewardPipBarred);
        }

        [Test]
        public void CurrentAdapter_MapsOnlyExistingAuthorityAndPreservesWorldCoordinates()
        {
            var source = new FakeWorldMarkerSource(new[]
            {
                new WorldMarkerSnapshot("player", WorldMarkerKind.Player, new Vector3(1f, 2f, 3f), WorldMarkerStatus.Available),
                new WorldMarkerSnapshot("spot-a", WorldMarkerKind.RegularSpot, new Vector3(4f, 5f, 6f), WorldMarkerStatus.Respawning, nextRespawnSeconds: 12.5f),
                new WorldMarkerSnapshot("elite", WorldMarkerKind.Elite, new Vector3(7f, 8f, 9f), WorldMarkerStatus.Locked),
                new WorldMarkerSnapshot("boss", WorldMarkerKind.Boss, new Vector3(10f, 11f, 12f), WorldMarkerStatus.Defeated)
            });
            var adapter = new CurrentWorldMarkerMapAdapter(source, () => 135f);

            Assert.That(adapter.GetMarker(0).Kind, Is.EqualTo(MapMarkerKind.Player));
            Assert.That(adapter.GetMarker(0).HeadingDegrees, Is.EqualTo(135f));
            Assert.That(adapter.GetMarker(1).Kind, Is.EqualTo(MapMarkerKind.Ordinary));
            Assert.That(adapter.GetMarker(1).Availability, Is.EqualTo(MapAvailabilityState.Cooldown));
            Assert.That(adapter.GetMarker(1).RemainingSeconds, Is.EqualTo(12.5f));
            Assert.That(adapter.GetMarker(2).ProgressionLocked, Is.True);
            Assert.That(adapter.GetMarker(3).Availability, Is.EqualTo(MapAvailabilityState.Defeated));
            Assert.That(adapter.GetMarker(1).RewardState, Is.EqualTo(MapRewardState.Unknown));
        }

        [Test]
        public void CurrentAdapter_RejectsUnknownAuthorityValuesInsteadOfFabricatingPresentationState()
        {
            var unknownKind = new FakeWorldMarkerSource(new[]
            {
                new WorldMarkerSnapshot("unknown-kind", (WorldMarkerKind)999, Vector3.zero, WorldMarkerStatus.Available)
            });
            var unknownStatus = new FakeWorldMarkerSource(new[]
            {
                new WorldMarkerSnapshot("unknown-status", WorldMarkerKind.RegularSpot, Vector3.zero, (WorldMarkerStatus)999)
            });

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new CurrentWorldMarkerMapAdapter(unknownKind).GetMarker(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new CurrentWorldMarkerMapAdapter(unknownStatus).GetMarker(0));
        }

        [Test]
        public void Presenter_FindsPlayerByKindRatherThanAssumingIndexZero()
        {
            var source = new MutableMapMarkerSource(new[]
            {
                new MapMarkerSnapshot("spot-first", MapMarkerKind.Ordinary, new Vector3(20f, 0f, 0f), MapAvailabilityState.Available),
                new MapMarkerSnapshot("player-second", MapMarkerKind.Player, Vector3.zero, MapAvailabilityState.Available)
            });
            var go = new GameObject("Map Presenter Player Order", typeof(RectTransform), typeof(MapMinimapPresenter));
            try
            {
                var presenter = go.GetComponent<MapMinimapPresenter>();
                presenter.SetAutoRefresh(false);
                presenter.Initialize(source);
                var anchor = presenter.GetMarkerAnchor("player-second", false);
                Assert.That(anchor.x, Is.EqualTo(0.5f).Within(0.0001f));
                Assert.That(anchor.y, Is.EqualTo(0.5f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CurrentAdapter_DoesNotDependOnUnrelatedVisualTransformPositions()
        {
            var gameplayPosition = new Vector3(9f, 0f, -17f);
            var source = new FakeWorldMarkerSource(new[]
            {
                new WorldMarkerSnapshot("spot", WorldMarkerKind.RegularSpot, gameplayPosition, WorldMarkerStatus.Available)
            });
            var adapter = new CurrentWorldMarkerMapAdapter(source);
            var visual = new GameObject("Unrelated Visual Prefab Position");
            try
            {
                visual.transform.position = new Vector3(999f, 999f, 999f);
                Assert.That(adapter.GetMarker(0).WorldPosition, Is.EqualTo(gameplayPosition));
                visual.transform.position = new Vector3(-999f, -999f, -999f);
                Assert.That(adapter.GetMarker(0).WorldPosition, Is.EqualTo(gameplayPosition));
            }
            finally
            {
                Object.DestroyImmediate(visual);
            }
        }

        [Test]
        public void Presenter_ReusesMarkerViewsForStableIds()
        {
            var source = new MutableMapMarkerSource(new[]
            {
                new MapMarkerSnapshot("player", MapMarkerKind.Player, Vector3.zero, MapAvailabilityState.Available),
                new MapMarkerSnapshot("spot", MapMarkerKind.Ordinary, new Vector3(5f, 0f, 5f), MapAvailabilityState.Available)
            });
            var go = new GameObject("Map Presenter", typeof(RectTransform), typeof(MapMinimapPresenter));
            try
            {
                var presenter = go.GetComponent<MapMinimapPresenter>();
                presenter.SetAutoRefresh(false);
                presenter.Initialize(source);
                var compactIdentity = presenter.GetCachedViewIdentity("spot", false);
                var expandedIdentity = presenter.GetCachedViewIdentity("spot", true);

                source.Markers[1] = new MapMarkerSnapshot(
                    "spot", MapMarkerKind.Ordinary, new Vector3(7f, 0f, 9f), MapAvailabilityState.Cooldown,
                    remainingSeconds: 8f);
                presenter.RefreshNow();

                Assert.That(presenter.GetCachedViewIdentity("spot", false), Is.EqualTo(compactIdentity));
                Assert.That(presenter.GetCachedViewIdentity("spot", true), Is.EqualTo(expandedIdentity));
                Assert.That(presenter.GetMarkerVisualState("spot", false).CooldownRing, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private sealed class FakeWorldMarkerSource : IWorldMarkerSource
        {
            private readonly WorldMarkerSnapshot[] _markers;
            public FakeWorldMarkerSource(WorldMarkerSnapshot[] markers) => _markers = markers;
            public int Count => _markers.Length;
            public WorldMarkerSnapshot GetMarker(int index) => _markers[index];
        }

        private sealed class MutableMapMarkerSource : IMapMarkerSource
        {
            public MutableMapMarkerSource(MapMarkerSnapshot[] markers) => Markers = markers;
            public MapMarkerSnapshot[] Markers { get; }
            public int Count => Markers.Length;
            public MapMarkerSnapshot GetMarker(int index) => Markers[index];
        }
    }
}
