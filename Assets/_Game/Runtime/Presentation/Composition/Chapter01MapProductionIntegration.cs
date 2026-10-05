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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachInAlreadyLoadedScene()
        {
            AttachToProductionRoots();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToProductionRoots();
        }

        private static void AttachToProductionRoots()
        {
            var roots = Object.FindObjectsByType<S01SceneCompositionRoot>(FindObjectsSortMode.None);
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.GetComponent<Chapter01MapProductionIntegration>() != null) continue;
                root.gameObject.AddComponent<Chapter01MapProductionIntegration>();
            }
        }

        private IEnumerator Start()
        {
            _root = GetComponent<S01SceneCompositionRoot>();
            if (_root == null) yield break;

            // S01SceneCompositionRoot composes in Start. Wait until the authoritative Chapter01
            // read model and the production HUD are fully available before attaching presentation.
            while (_root.WorldMarkers == null ||
                   _root.PlayerObject == null ||
                   _root.PauseMenu == null ||
                   _root.OfflineRewardPanel == null ||
                   _root.ChapterCompletion == null)
            {
                yield return null;
            }

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
                    new CurrentWorldMarkerMapAdapter(
                        _root.WorldMarkers,
                        () => _root.PlayerObject != null ? _root.PlayerObject.transform.eulerAngles.y : 0f));
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
