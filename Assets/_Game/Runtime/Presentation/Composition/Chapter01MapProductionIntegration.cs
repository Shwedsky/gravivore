using System;
using System.Collections;
using System.Collections.Generic;
using Gravivore.Presentation.Input;
using Gravivore.Presentation.Map;
using Gravivore.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravivore.Presentation.Composition
{
    /// <summary>
    /// Production-only bridge that attaches the already-merged Map/Minimap presenter to the
    /// real S01 Chapter01 composition root after its authoritative runtime state is composed.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Chapter01MapProductionIntegration : MonoBehaviour
    {
        private S01SceneCompositionRoot _root;

        public MapMinimapPresenter MapPresenter { get; private set; }

        public void Initialize(S01SceneCompositionRoot root)
        {
            if (_root != null) throw new InvalidOperationException("Production map integration is already initialized.");
            _root = root != null ? root : throw new ArgumentNullException(nameof(root));
            InitializeProductionMap();
        }

        private void InitializeProductionMap()
        {
            var safeArea = GetComponentInChildren<SafeAreaHudRoot>(true);
            if (safeArea == null)
            {
                throw new InvalidOperationException("Chapter01 production map requires the composed Safe Area HUD root.");
            }

            var safeAreaRect = safeArea.GetComponent<RectTransform>();
            MapPresenter = safeArea.GetComponentInChildren<MapMinimapPresenter>(true);
            if (MapPresenter == null)
            {
                var mapObject = new GameObject(
                    "Chapter01 Production Map Minimap",
                    typeof(RectTransform),
                    typeof(MapMinimapPresenter));
                mapObject.transform.SetParent(safeAreaRect, false);
                var mapRect = mapObject.GetComponent<RectTransform>();
                mapRect.anchorMin = Vector2.zero;
                mapRect.anchorMax = Vector2.one;
                mapRect.offsetMin = Vector2.zero;
                mapRect.offsetMax = Vector2.zero;

                MapPresenter = mapObject.GetComponent<MapMinimapPresenter>();
                MapPresenter.Initialize(
                    _root.MapMarkers, _root.MapBounds);
                MapPresenter.InitializeTopology(new TacticalMapTopology(_root.WorldPresenter, _root.EnemyPopulation,
                    _root.RepairHub.RepairPosition, _root.RepairHub.RepairRadius));
            }

            RebuildProductionTouchExclusion(safeAreaRect);
        }

        private void RebuildProductionTouchExclusion(RectTransform safeArea)
        {
            var exclusion = _root.GetComponent<UiTouchExclusion>();
            if (exclusion == null || MapPresenter == null) return;

            var regions = new List<RectTransform>(10);
            AddRegion(regions, safeArea.Find("HUD Touch Exclusion") as RectTransform);
            AddRegion(regions, _root.PauseMenu != null ? _root.PauseMenu.PauseButtonRect : null);
            AddRegion(regions, _root.PauseMenu != null ? _root.PauseMenu.ModalRect : null);
            AddRegion(regions, _root.OfflineRewardPanel != null ? _root.OfflineRewardPanel.ModalRect : null);
            AddRegion(regions, _root.ChapterCompletion != null ? _root.ChapterCompletion.ModalRect : null);

            // Compact panel is always active; expanded root becomes a full-screen exclusion only
            // while the expanded Chapter map is open.
            AddRegion(regions, MapPresenter.CompactSurface != null
                ? MapPresenter.CompactSurface.parent as RectTransform
                : null);
            AddRegion(regions, MapPresenter.ExpandedRoot);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_root.DevelopmentOverlay != null)
            {
                AddRegion(regions, _root.DevelopmentOverlay.ToggleRect);
                AddRegion(regions, _root.DevelopmentOverlay.PanelRect);
            }
#endif

            exclusion.Initialize(regions.ToArray());
        }

        private static void AddRegion(List<RectTransform> regions, RectTransform region)
        {
            if (region == null || regions.Contains(region)) return;
            regions.Add(region);
        }
    }
}
