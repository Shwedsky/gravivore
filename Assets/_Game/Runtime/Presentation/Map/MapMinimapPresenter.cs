using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Map
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class MapMinimapPresenter : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float localRadius = MapProjection.DefaultLocalRadius;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.10f;
        [SerializeField] private bool autoRefresh = true;

        private sealed class MarkerEntry
        {
            public MapMarkerView Compact;
            public MapMarkerView Expanded;
            public int RefreshStamp;
        }

        private readonly Dictionary<string, MarkerEntry> _entries =
            new Dictionary<string, MarkerEntry>(StringComparer.Ordinal);

        private IMapMarkerSource _source;
        private IMapPoiSelectionSink _selectionSink;
        private MapProjection _projection;
        private RectTransform _compactSurface;
        private RectTransform _expandedRoot;
        private RectTransform _expandedSurface;
        private RectTransform _detailsPanel;
        private Text _detailsText;
        private bool _uiBuilt;
        private bool _expanded;
        private int _refreshStamp;
        private float _nextRefreshAt;
        private string _selectedId;
        private MapMarkerSnapshot? _selectedSnapshot;
        private int _selectedExactSecondBucket = int.MinValue;

        public event Action<bool> ExpandedChanged;
        public event Action<MapMarkerSnapshot> PoiSelected;
        public event Action PoiSelectionCleared;

        public bool IsExpanded => _expanded;
        public string SelectedMarkerId => _selectedId;
        public string SelectedDetailsText => _detailsText != null ? _detailsText.text : string.Empty;
        public RectTransform CompactSurface => _compactSurface;
        public RectTransform ExpandedSurface => _expandedSurface;
        public RectTransform ExpandedRoot => _expandedRoot;
        public int CachedMarkerCount => _entries.Count;

        public void Initialize(
            IMapMarkerSource source,
            MapWorldBounds? chapterBounds = null,
            float? compactLocalRadius = null,
            IMapPoiSelectionSink selectionSink = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _selectionSink = selectionSink;
            var bounds = chapterBounds ?? MapWorldBounds.FromCenterSize(
                Vector2.zero,
                new Vector2(MapProjection.DefaultChapterWidth, MapProjection.DefaultChapterDepth));
            var radius = compactLocalRadius ?? localRadius;
            _projection = new MapProjection(bounds, radius);
            BuildUiIfNeeded();
            RefreshNow();
        }

        public void SetAutoRefresh(bool enabled)
        {
            autoRefresh = enabled;
        }

        public void RefreshNow()
        {
            if (_source == null || _projection == null) return;
            BuildUiIfNeeded();
            _refreshStamp++;
            if (_refreshStamp == int.MaxValue) _refreshStamp = 1;

            FindPlayer(out var playerPosition);
            var count = _source.Count;
            var selectedFound = false;

            for (var i = 0; i < count; i++)
            {
                var marker = _source.GetMarker(i);
                if (string.IsNullOrEmpty(marker.Id))
                    throw new InvalidOperationException("Map marker source returned an empty marker id.");

                var entry = GetOrCreate(marker);
                if (entry.RefreshStamp == _refreshStamp)
                    throw new InvalidOperationException($"Map marker source returned duplicate id '{marker.Id}'.");
                entry.RefreshStamp = _refreshStamp;

                var expandedPosition = _projection.ProjectExpanded(marker.WorldPosition);
                var compactVisible = _projection.TryProjectCompact(marker.WorldPosition, playerPosition, out var compactPosition);
                entry.Compact.Apply(marker, compactPosition, compactVisible);
                entry.Expanded.Apply(marker, expandedPosition, marker.Visible);
                entry.Expanded.SetSelection(marker.Visible && _selectedId == marker.Id);

                if (_selectedId == marker.Id && marker.Visible)
                {
                    selectedFound = true;
                    UpdateSelectedDetails(marker, false);
                }
            }

            foreach (var pair in _entries)
            {
                if (pair.Value.RefreshStamp == _refreshStamp) continue;
                pair.Value.Compact.Root.gameObject.SetActive(false);
                pair.Value.Expanded.Root.gameObject.SetActive(false);
            }

            if (_selectedId != null && !selectedFound) ClearSelection();
        }

        public void OpenExpanded()
        {
            BuildUiIfNeeded();
            if (_expanded) return;
            _expanded = true;
            _expandedRoot.gameObject.SetActive(true);
            ExpandedChanged?.Invoke(true);
        }

        public void CloseExpanded()
        {
            if (!_expanded) return;
            _expanded = false;
            if (_expandedRoot != null) _expandedRoot.gameObject.SetActive(false);
            ClearSelection();
            ExpandedChanged?.Invoke(false);
        }

        public bool SelectMarker(string markerId)
        {
            if (!_expanded || string.IsNullOrEmpty(markerId)) return false;
            if (!_entries.TryGetValue(markerId, out var entry) ||
                !entry.Expanded.Root.gameObject.activeInHierarchy) return false;
            var marker = entry.Expanded.Snapshot;
            if (!marker.Visible || marker.Kind == MapMarkerKind.Player) return false;
            HandleSelection(markerId);
            return _selectedId == markerId;
        }

        public void ClearSelection()
        {
            if (_selectedId == null) return;
            if (_entries.TryGetValue(_selectedId, out var previous)) previous.Expanded.SetSelection(false);
            _selectedId = null;
            _selectedSnapshot = null;
            _selectedExactSecondBucket = int.MinValue;
            if (_detailsPanel != null) _detailsPanel.gameObject.SetActive(false);
            _selectionSink?.OnMapPoiSelectionCleared();
            PoiSelectionCleared?.Invoke();
        }

        public int GetCachedViewIdentity(string markerId, bool expanded)
        {
            if (!_entries.TryGetValue(markerId, out var entry)) return 0;
            return (expanded ? entry.Expanded.Root : entry.Compact.Root).gameObject.GetInstanceID();
        }

        public Vector2 GetMarkerAnchor(string markerId, bool expanded)
        {
            if (!_entries.TryGetValue(markerId, out var entry)) throw new KeyNotFoundException(markerId);
            var rect = expanded ? entry.Expanded.Root : entry.Compact.Root;
            return rect.anchorMin;
        }

        public MapMarkerVisualState GetMarkerVisualState(string markerId, bool expanded)
        {
            if (!_entries.TryGetValue(markerId, out var entry)) throw new KeyNotFoundException(markerId);
            return (expanded ? entry.Expanded : entry.Compact).VisualState;
        }

        private void Update()
        {
            if (!autoRefresh || _source == null || _projection == null) return;
            if (Time.unscaledTime < _nextRefreshAt) return;
            _nextRefreshAt = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
            RefreshNow();
        }

        private void BuildUiIfNeeded()
        {
            if (_uiBuilt) return;
            _uiBuilt = true;
            var root = GetComponent<RectTransform>();
            MapUiFactory.Stretch(root);

            BuildCompact(root);
            BuildExpanded(root);
        }

        private void BuildCompact(RectTransform root)
        {
            var compactPanel = MapUiFactory.CreatePanel(root, "CompactMinimap", MapUiFactory.SurfaceColor, true);
            compactPanel.anchorMin = new Vector2(0.70f, 0.765f);
            compactPanel.anchorMax = new Vector2(0.97f, 0.765f);
            compactPanel.pivot = Vector2.one;
            compactPanel.offsetMin = Vector2.zero;
            compactPanel.offsetMax = Vector2.zero;
            var compactFitter = compactPanel.gameObject.AddComponent<AspectRatioFitter>();
            compactFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            compactFitter.aspectRatio = 1f;
            var button = MapUiFactory.AddButton(compactPanel);
            button.onClick.AddListener(OpenExpanded);

            _compactSurface = MapUiFactory.CreatePanel(compactPanel, "CompactMapSurface", MapUiFactory.SurfaceColor);
            _compactSurface.anchorMin = new Vector2(0.055f, 0.055f);
            _compactSurface.anchorMax = new Vector2(0.945f, 0.945f);
            _compactSurface.offsetMin = Vector2.zero;
            _compactSurface.offsetMax = Vector2.zero;
            _compactSurface.gameObject.AddComponent<RectMask2D>();
            MapUiFactory.CreateGrid(_compactSurface, 4, 4);

            var north = MapUiFactory.CreateText(compactPanel, "North", "N", 15, TextAnchor.UpperCenter);
            north.color = new Color(0.72f, 0.86f, 0.87f, 0.86f);
            north.rectTransform.anchorMin = new Vector2(0.42f, 0.88f);
            north.rectTransform.anchorMax = new Vector2(0.58f, 0.99f);
            north.rectTransform.offsetMin = Vector2.zero;
            north.rectTransform.offsetMax = Vector2.zero;
        }

        private void BuildExpanded(RectTransform root)
        {
            _expandedRoot = MapUiFactory.CreatePanel(root, "ExpandedMap", new Color(0.008f, 0.016f, 0.022f, 0.97f), true);
            MapUiFactory.Stretch(_expandedRoot);

            var title = MapUiFactory.CreateText(_expandedRoot, "Title", "КАРТА", 28, TextAnchor.MiddleLeft);
            title.rectTransform.anchorMin = new Vector2(0.06f, 0.925f);
            title.rectTransform.anchorMax = new Vector2(0.55f, 0.985f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.color = new Color(0.72f, 0.86f, 0.87f, 1f);

            var closeRect = MapUiFactory.CreatePanel(_expandedRoot, "Close", new Color(0.10f, 0.19f, 0.21f, 0.96f), true);
            closeRect.anchorMin = new Vector2(0.82f, 0.92f);
            closeRect.anchorMax = new Vector2(0.95f, 0.985f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            var closeButton = MapUiFactory.AddButton(closeRect);
            closeButton.onClick.AddListener(CloseExpanded);
            var closeLabel = MapUiFactory.CreateText(closeRect, "Label", "X", 24, TextAnchor.MiddleCenter);
            MapUiFactory.Stretch(closeLabel.rectTransform);

            var fitRoot = MapUiFactory.CreateRect(_expandedRoot, "MapFitArea");
            fitRoot.anchorMin = new Vector2(0.08f, 0.17f);
            fitRoot.anchorMax = new Vector2(0.92f, 0.90f);
            fitRoot.offsetMin = Vector2.zero;
            fitRoot.offsetMax = Vector2.zero;

            _expandedSurface = MapUiFactory.CreatePanel(fitRoot, "ChapterMapSurface", MapUiFactory.SurfaceColorExpanded);
            MapUiFactory.Stretch(_expandedSurface);
            var fitter = _expandedSurface.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = _projection.ChapterBounds.Width / _projection.ChapterBounds.Depth;
            _expandedSurface.gameObject.AddComponent<RectMask2D>();
            var backgroundButton = MapUiFactory.AddButton(_expandedSurface);
            backgroundButton.onClick.AddListener(ClearSelection);
            MapUiFactory.CreateGrid(_expandedSurface, 6, 10);

            var north = MapUiFactory.CreateText(_expandedSurface, "North", "N", 18, TextAnchor.UpperCenter);
            north.rectTransform.anchorMin = new Vector2(0.42f, 0.955f);
            north.rectTransform.anchorMax = new Vector2(0.58f, 0.995f);
            north.rectTransform.offsetMin = Vector2.zero;
            north.rectTransform.offsetMax = Vector2.zero;
            north.color = new Color(0.72f, 0.86f, 0.87f, 0.86f);

            _detailsPanel = MapUiFactory.CreatePanel(_expandedRoot, "PoiDetails", new Color(0.035f, 0.075f, 0.085f, 0.98f));
            _detailsPanel.anchorMin = new Vector2(0.06f, 0.025f);
            _detailsPanel.anchorMax = new Vector2(0.94f, 0.145f);
            _detailsPanel.offsetMin = Vector2.zero;
            _detailsPanel.offsetMax = Vector2.zero;
            _detailsText = MapUiFactory.CreateText(_detailsPanel, "DetailsText", string.Empty, 17, TextAnchor.MiddleLeft);
            _detailsText.rectTransform.anchorMin = new Vector2(0.04f, 0.08f);
            _detailsText.rectTransform.anchorMax = new Vector2(0.96f, 0.92f);
            _detailsText.rectTransform.offsetMin = Vector2.zero;
            _detailsText.rectTransform.offsetMax = Vector2.zero;
            _detailsText.resizeTextForBestFit = true;
            _detailsText.resizeTextMinSize = 13;
            _detailsText.resizeTextMaxSize = 18;
            _detailsText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _detailsText.verticalOverflow = VerticalWrapMode.Truncate;
            _detailsPanel.gameObject.SetActive(false);
            _expandedRoot.gameObject.SetActive(false);
        }

        private MarkerEntry GetOrCreate(MapMarkerSnapshot marker)
        {
            if (_entries.TryGetValue(marker.Id, out var entry)) return entry;
            entry = new MarkerEntry
            {
                Compact = MapMarkerView.Create(_compactSurface, marker.Id, false, MapUiFactory.SurfaceColor, null),
                Expanded = MapMarkerView.Create(
                    _expandedSurface,
                    marker.Id,
                    true,
                    MapUiFactory.SurfaceColorExpanded,
                    marker.Kind == MapMarkerKind.Player ? null : HandleSelection)
            };
            _entries.Add(marker.Id, entry);
            return entry;
        }

        private void FindPlayer(out Vector3 playerPosition)
        {
            var center = _projection.ChapterBounds.Center;
            playerPosition = new Vector3(center.x, 0f, center.y);
            var count = _source.Count;
            for (var i = 0; i < count; i++)
            {
                var marker = _source.GetMarker(i);
                if (marker.Kind != MapMarkerKind.Player) continue;
                playerPosition = marker.WorldPosition;
                return;
            }
        }

        private void HandleSelection(string markerId)
        {
            if (!_expanded || !_entries.TryGetValue(markerId, out var entry)) return;
            var marker = entry.Expanded.Snapshot;
            if (!marker.Visible || marker.Kind == MapMarkerKind.Player) return;

            if (_selectedId != null && _entries.TryGetValue(_selectedId, out var previous))
                previous.Expanded.SetSelection(false);
            _selectedId = markerId;
            _selectedSnapshot = marker;
            entry.Expanded.SetSelection(true);
            _selectedExactSecondBucket = int.MinValue;
            UpdateSelectedDetails(marker, true);
            _selectionSink?.OnMapPoiSelected(marker);
            PoiSelected?.Invoke(marker);
        }

        private void UpdateSelectedDetails(MapMarkerSnapshot marker, bool force)
        {
            if (_detailsPanel == null || _detailsText == null) return;
            _detailsPanel.gameObject.SetActive(true);
            var exactBucket = marker.Availability == MapAvailabilityState.Cooldown
                ? MapTimerFormatter.ExactSecondBucket(marker.RemainingSeconds)
                : -1;
            if (!force && _selectedSnapshot.HasValue &&
                exactBucket == _selectedExactSecondBucket &&
                SameSelectedPresentationState(_selectedSnapshot.Value, marker))
            {
                _selectedSnapshot = marker;
                return;
            }

            _selectedSnapshot = marker;
            _selectedExactSecondBucket = exactBucket;
            _detailsText.text = BuildDetails(marker);
        }

        private static bool SameSelectedPresentationState(MapMarkerSnapshot a, MapMarkerSnapshot b) =>
            a.Id == b.Id &&
            a.DisplayName == b.DisplayName &&
            a.Availability == b.Availability &&
            a.ProgressionLocked == b.ProgressionLocked &&
            a.RewardState == b.RewardState;

        private static string BuildDetails(MapMarkerSnapshot marker)
        {
            var state = marker.ProgressionLocked ? "ЗАБЛОКИРОВАНО" : AvailabilityText(marker.Availability);
            var text = $"{marker.DisplayName}  ·  {state}";
            if (marker.Availability == MapAvailabilityState.Cooldown && marker.RemainingSeconds > 0f)
                text += $"  ·  {MapTimerFormatter.FormatExact(marker.RemainingSeconds)}";
            if (marker.RewardState == MapRewardState.Full)
                text += "  ·  НАГРАДА ДОСТУПНА";
            else if (marker.RewardState == MapRewardState.CappedFallback)
                text += "  ·  БАЗОВАЯ НАГРАДА";
            return text;
        }

        private static string AvailabilityText(MapAvailabilityState state)
        {
            switch (state)
            {
                case MapAvailabilityState.Available: return "ДОСТУПНО";
                case MapAvailabilityState.Active: return "АКТИВНО";
                case MapAvailabilityState.Cooldown: return "ВОССТАНОВЛЕНИЕ";
                case MapAvailabilityState.Defeated: return "ПОБЕЖДЕНО";
                default: return "НЕАКТИВНО";
            }
        }
    }
}
