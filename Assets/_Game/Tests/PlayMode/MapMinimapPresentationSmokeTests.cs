using System.Collections;
using Gravivore.Presentation.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gravivore.Tests.PlayMode
{
    public sealed class MapMinimapPresentationSmokeTests
    {
        [UnityTest]
        public IEnumerator PresentationModule_MovesPlayerReusesMarkersExpandsAndHandlesPortraitAspects()
        {
            var canvasObject = new GameObject("Map Test Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var host = new GameObject("Map Test Host", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(canvasObject.transform, false);
            host.anchorMin = host.anchorMax = new Vector2(0.5f, 0.5f);
            host.pivot = new Vector2(0.5f, 0.5f);
            host.sizeDelta = new Vector2(720f, 1280f);

            var presenterObject = new GameObject("Map Minimap Presentation", typeof(RectTransform), typeof(MapMinimapPresenter));
            presenterObject.transform.SetParent(host, false);
            var presenter = presenterObject.GetComponent<MapMinimapPresenter>();
            presenter.SetAutoRefresh(false);

            var source = new MutableMapSource(new[]
            {
                new MapMarkerSnapshot("player", MapMarkerKind.Player, Vector3.zero, MapAvailabilityState.Available, "Игрок", headingDegrees: 0f),
                new MapMarkerSnapshot("ordinary", MapMarkerKind.Ordinary, new Vector3(10f, 0f, 15f), MapAvailabilityState.Available, "Узел"),
                new MapMarkerSnapshot("elite", MapMarkerKind.Elite, new Vector3(-15f, 0f, 30f), MapAvailabilityState.Cooldown, "Элита", remainingSeconds: 754f, cooldownProgress01: 0.4f, rewardState: MapRewardState.Full),
                new MapMarkerSnapshot("boss", MapMarkerKind.Boss, new Vector3(0f, 0f, 60f), MapAvailabilityState.Inactive, "Босс", progressionLocked: true),
                new MapMarkerSnapshot("repair", MapMarkerKind.RepairHub, new Vector3(-10f, 0f, -20f), MapAvailabilityState.Available, "Ремонтный узел")
            });

            try
            {
                presenter.Initialize(source);
                Canvas.ForceUpdateCanvases();
                yield return null;

                Assert.That(presenter.CachedMarkerCount, Is.EqualTo(5));
                Assert.That(presenter.GetMarkerVisualState("ordinary", false).Glyph, Is.EqualTo(MapMarkerGlyph.Node));
                Assert.That(presenter.GetMarkerVisualState("elite", true).CooldownRing, Is.True);
                Assert.That(presenter.GetMarkerVisualState("boss", true).LockOverlay, Is.True);
                Assert.That(presenter.GetMarkerVisualState("boss", true).InactiveSlash, Is.False);
                Assert.IsFalse(presenter.CompactSurface.Find("Marker_elite").gameObject.activeSelf,
                    "Elite lies outside the compact local crop.");
                Assert.IsFalse(presenter.CompactSurface.Find("Marker_boss").gameObject.activeSelf,
                    "Boss lies outside the compact local crop.");
                Assert.That(presenter.GetMarkerVisualState("repair", false).Glyph, Is.EqualTo(MapMarkerGlyph.RepairCross));

                var playerExpandedBefore = presenter.GetMarkerAnchor("player", true);
                source.Markers[0] = new MapMarkerSnapshot(
                    "player", MapMarkerKind.Player, new Vector3(12f, 0f, 20f), MapAvailabilityState.Available, "Игрок", headingDegrees: 90f);
                presenter.RefreshNow();
                var playerExpandedAfter = presenter.GetMarkerAnchor("player", true);
                Assert.That(Mathf.Abs(playerExpandedAfter.x - playerExpandedBefore.x), Is.GreaterThan(0.0001f));
                Assert.That(Mathf.Abs(playerExpandedAfter.y - playerExpandedBefore.y), Is.GreaterThan(0.0001f));

                var ordinaryView = presenter.GetCachedViewIdentity("ordinary", false);
                source.Markers[1] = new MapMarkerSnapshot(
                    "ordinary", MapMarkerKind.Ordinary, new Vector3(10f, 0f, 15f), MapAvailabilityState.Cooldown, "Узел", remainingSeconds: 9f);
                presenter.RefreshNow();
                Assert.That(presenter.GetCachedViewIdentity("ordinary", false), Is.EqualTo(ordinaryView));
                Assert.IsTrue(presenter.GetMarkerVisualState("ordinary", false).CooldownRing);
                Assert.IsFalse(presenter.GetMarkerVisualState("ordinary", false).Filled);

                var ordinaryOverviewTimer = presenter.ExpandedSurface.Find("Marker_ordinary/Timer");
                Assert.NotNull(ordinaryOverviewTimer);
                Assert.IsTrue(ordinaryOverviewTimer.gameObject.activeSelf, "Ordinary overview exposes the authoritative wave timer.");

                presenter.OpenExpanded();
                Assert.IsTrue(presenter.IsExpanded);
                Assert.IsTrue(presenter.ExpandedRoot.gameObject.activeSelf);
                Assert.IsTrue(presenter.SelectMarker("elite"));
                StringAssert.Contains("12:34", presenter.SelectedDetailsText);

                var backgroundButton = presenter.ExpandedSurface.GetComponent<Button>();
                Assert.NotNull(backgroundButton);
                backgroundButton.onClick.Invoke();
                Assert.That(presenter.SelectedMarkerId, Is.Null, "Background tap must clear POI selection.");

                Assert.IsTrue(presenter.SelectMarker("elite"));
                source.Markers[2] = new MapMarkerSnapshot(
                    "elite", MapMarkerKind.Elite, new Vector3(-15f, 0f, 30f), MapAvailabilityState.Cooldown,
                    "Элита", visible: false, remainingSeconds: 754f, cooldownProgress01: 0.4f,
                    rewardState: MapRewardState.Full);
                presenter.RefreshNow();
                Assert.That(presenter.SelectedMarkerId, Is.Null, "A marker becoming invisible must clear selection.");

                source.Markers[2] = new MapMarkerSnapshot(
                    "elite", MapMarkerKind.Elite, new Vector3(-15f, 0f, 30f), MapAvailabilityState.Cooldown,
                    "Элита", remainingSeconds: 754f, cooldownProgress01: 0.4f,
                    rewardState: MapRewardState.Full);
                presenter.RefreshNow();

                presenter.CloseExpanded();
                Assert.IsFalse(presenter.IsExpanded);
                Assert.IsFalse(presenter.ExpandedRoot.gameObject.activeSelf);
                Assert.IsFalse(presenter.SelectMarker("elite"), "Collapsed map must not allow POI selection.");

                source.Markers[1] = new MapMarkerSnapshot(
                    "ordinary", MapMarkerKind.Ordinary, new Vector3(34f, 0f, 15f),
                    MapAvailabilityState.Available, "Правый узел");
                presenter.RefreshNow();
                presenter.OpenExpanded();
                Assert.IsTrue(presenter.SelectMarker("elite"));
                AssertPortraitLayout(host, presenter, new Vector2(540f, 960f));
                AssertPortraitLayout(host, presenter, new Vector2(720f, 1280f));
                AssertPortraitLayout(host, presenter, new Vector2(720f, 1600f));
                AssertPortraitLayout(host, presenter, new Vector2(1080f, 2400f));
            }
            finally
            {
                Object.Destroy(canvasObject);
            }

            yield return null;
        }

        private static void AssertPortraitLayout(RectTransform host, MapMinimapPresenter presenter, Vector2 size)
        {
            host.sizeDelta = size;
            host.ForceUpdateRectTransforms();
            presenter.GetComponent<RectTransform>().ForceUpdateRectTransforms();
            Canvas.ForceUpdateCanvases();

            var compactPanel = (RectTransform)presenter.CompactSurface.parent;
            Assert.That(compactPanel.anchorMin.x, Is.EqualTo(0.70f).Within(0.0001f));
            Assert.That(compactPanel.anchorMax.x, Is.EqualTo(0.97f).Within(0.0001f));
            Assert.That(compactPanel.anchorMin.y, Is.EqualTo(0.765f).Within(0.0001f));
            Assert.That(compactPanel.anchorMax.y, Is.EqualTo(0.765f).Within(0.0001f));
            Assert.That(compactPanel.pivot, Is.EqualTo(Vector2.one));
            var compactFitter = compactPanel.GetComponent<AspectRatioFitter>();
            Assert.NotNull(compactFitter);
            Assert.That(compactFitter.aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.WidthControlsHeight));
            Assert.That(compactFitter.aspectRatio, Is.EqualTo(.90f).Within(0.0001f));
            Assert.That(presenter.CompactSurface.rect.width/presenter.CompactSurface.rect.height,
                Is.EqualTo(1f).Within(.001f), "Zone footer must not stretch the established square projection.");

            var fitter = presenter.ExpandedSurface.GetComponent<AspectRatioFitter>();
            Assert.NotNull(fitter);
            Assert.That(fitter.aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.FitInParent));
            Assert.That(fitter.aspectRatio, Is.EqualTo(72f / 140f).Within(0.0001f));

            var mapCorners = new Vector3[4];
            var labelCorners = new Vector3[4];
            presenter.ExpandedSurface.GetWorldCorners(mapCorners);
            var label = presenter.ExpandedSurface.Find("Marker_ordinary/Label").GetComponent<Text>();
            label.rectTransform.GetWorldCorners(labelCorners);
            Assert.That(labelCorners[0].x, Is.GreaterThanOrEqualTo(mapCorners[0].x - 0.01f),
                "Right-edge POI label must stay inside the map.");
            Assert.That(labelCorners[2].x, Is.LessThanOrEqualTo(mapCorners[2].x + 0.01f));
            var details = presenter.ExpandedRoot.Find("PoiDetails/DetailsText").GetComponent<Text>();
            Assert.That(details.preferredHeight, Is.LessThanOrEqualTo(details.rectTransform.rect.height + 0.01f),
                "Selected timer/reward details must fit the small portrait panel.");
        }

        private sealed class MutableMapSource : IMapMarkerSource
        {
            public MutableMapSource(MapMarkerSnapshot[] markers) => Markers = markers;
            public MapMarkerSnapshot[] Markers { get; }
            public int Count => Markers.Length;
            public MapMarkerSnapshot GetMarker(int index) => Markers[index];
        }
    }
}
