using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Map
{
    internal sealed class MapMarkerView
    {
        private const int RingSegments = 8;
        private readonly bool _expanded;
        private readonly Color _surfaceColor;
        private readonly RectTransform _glyphRoot;
        private readonly Image _primary;
        private readonly Image _secondary;
        private readonly Image _cutout;
        private readonly Image _accentLeft;
        private readonly Image _accentRight;
        private readonly Image _lockBar;
        private readonly Image _lockStem;
        private readonly Image _slash;
        private readonly Image _completionStem;
        private readonly Image _completionTick;
        private readonly Image[] _activeBrackets;
        private readonly Image[] _cooldownRing;
        private readonly Image _rewardPip;
        private readonly Image _rewardBar;
        private readonly Image[] _selectionFrame;
        private readonly Text _label;
        private readonly Text _timer;
        private MapMarkerGlyph _configuredGlyph;
        private bool _glyphConfigured;
        private int _lastRoundedTimerBucket = int.MinValue;

        private MapMarkerView(RectTransform root, bool expanded, Color surfaceColor)
        {
            Root = root;
            _expanded = expanded;
            _surfaceColor = surfaceColor;
            _glyphRoot = MapUiFactory.CreateRect(root, "Glyph");
            MapUiFactory.PointAnchor(_glyphRoot, new Vector2(0.5f, 0.5f));
            MapUiFactory.SetSize(_glyphRoot, expanded ? 36f : 28f, expanded ? 36f : 28f);

            _primary = MapUiFactory.CreateImage(_glyphRoot, "Primary", Color.white);
            _secondary = MapUiFactory.CreateImage(_glyphRoot, "Secondary", Color.white);
            _cutout = MapUiFactory.CreateImage(_glyphRoot, "Cutout", surfaceColor);
            _accentLeft = MapUiFactory.CreateImage(_glyphRoot, "AccentLeft", Color.white);
            _accentRight = MapUiFactory.CreateImage(_glyphRoot, "AccentRight", Color.white);
            _lockBar = MapUiFactory.CreateImage(_glyphRoot, "LockBar", Color.white);
            _lockStem = MapUiFactory.CreateImage(_glyphRoot, "LockStem", Color.white);
            _slash = MapUiFactory.CreateImage(_glyphRoot, "InactiveSlash", Color.white);
            _completionStem = MapUiFactory.CreateImage(_glyphRoot, "CompletionStem", Color.white);
            _completionTick = MapUiFactory.CreateImage(_glyphRoot, "CompletionTick", Color.white);

            _activeBrackets = new Image[4];
            for (var i = 0; i < _activeBrackets.Length; i++)
                _activeBrackets[i] = MapUiFactory.CreateImage(_glyphRoot, $"ActiveBracket{i}", Color.white);

            _cooldownRing = new Image[RingSegments];
            for (var i = 0; i < RingSegments; i++)
            {
                var segment = MapUiFactory.CreateImage(_glyphRoot, $"CooldownSegment{i}", Color.white);
                var angle = i * 45f;
                var radians = angle * Mathf.Deg2Rad;
                var radius = expanded ? 21f : 17f;
                segment.rectTransform.anchorMin = segment.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                segment.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * radius;
                MapUiFactory.SetSize(segment.rectTransform, 2f, expanded ? 7f : 6f);
                segment.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
                _cooldownRing[i] = segment;
            }

            _rewardPip = MapUiFactory.CreateImage(_glyphRoot, "RewardPip", Color.white);
            _rewardPip.rectTransform.anchorMin = _rewardPip.rectTransform.anchorMax = new Vector2(1f, 1f);
            _rewardPip.rectTransform.anchoredPosition = new Vector2(-1f, -1f);
            MapUiFactory.SetSize(_rewardPip.rectTransform, 7f, 7f);
            _rewardBar = MapUiFactory.CreateImage(_rewardPip.rectTransform, "RewardBar", surfaceColor);
            MapUiFactory.PointAnchor(_rewardBar.rectTransform, new Vector2(0.5f, 0.5f));
            MapUiFactory.SetSize(_rewardBar.rectTransform, 9f, 2f);
            _rewardBar.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -45f);

            _selectionFrame = new Image[4];
            for (var i = 0; i < _selectionFrame.Length; i++)
                _selectionFrame[i] = MapUiFactory.CreateImage(root, $"Selection{i}", MapUiFactory.PlayerColor);
            ConfigureFrame(_selectionFrame, expanded ? 44f : 34f, expanded ? 44f : 34f, 2f);
            SetSelection(false);

            if (expanded)
            {
                _label = MapUiFactory.CreateText(root, "Label", string.Empty, 16, TextAnchor.MiddleLeft);
                var labelRect = _label.rectTransform;
                labelRect.anchorMin = labelRect.anchorMax = new Vector2(1f, 0.62f);
                labelRect.pivot = new Vector2(0f, 0.5f);
                labelRect.anchoredPosition = new Vector2(4f, 0f);
                MapUiFactory.SetSize(labelRect, 150f, 22f);

                _timer = MapUiFactory.CreateText(root, "Timer", string.Empty, 14, TextAnchor.MiddleLeft);
                var timerRect = _timer.rectTransform;
                timerRect.anchorMin = timerRect.anchorMax = new Vector2(1f, 0.20f);
                timerRect.pivot = new Vector2(0f, 0.5f);
                timerRect.anchoredPosition = new Vector2(4f, 0f);
                MapUiFactory.SetSize(timerRect, 120f, 20f);
                _timer.color = new Color(0.82f, 0.88f, 0.89f, 0.95f);
            }
        }

        public RectTransform Root { get; }
        public MapMarkerVisualState VisualState { get; private set; }
        public MapMarkerSnapshot Snapshot { get; private set; }

        public static MapMarkerView Create(
            RectTransform parent,
            string id,
            bool expanded,
            Color surfaceColor,
            Action<string> onSelected)
        {
            var root = MapUiFactory.CreateRect(parent, $"Marker_{id}");
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            MapUiFactory.SetSize(root, expanded ? 44f : 34f, expanded ? 44f : 34f);
            if (expanded && onSelected != null)
            {
                var image = root.gameObject.AddComponent<Image>();
                image.color = Color.clear;
                image.raycastTarget = true;
                var button = root.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => onSelected(id));
            }

            return new MapMarkerView(root, expanded, surfaceColor);
        }

        public void Apply(MapMarkerSnapshot snapshot, Vector2 normalizedPosition, bool visible)
        {
            Snapshot = snapshot;
            Root.gameObject.SetActive(visible && snapshot.Visible);
            if (!Root.gameObject.activeSelf) return;

            MapUiFactory.PointAnchor(Root, normalizedPosition);
            var visual = MapMarkerVisualResolver.Resolve(snapshot, _expanded);
            VisualState = visual;
            _glyphRoot.localScale = Vector3.one * (_expanded ? .9f : snapshot.Kind == MapMarkerKind.Player ? .95f :
                snapshot.Kind == MapMarkerKind.Boss ? 1f : snapshot.Kind == MapMarkerKind.Elite ? .95f : .85f);
            ConfigureGlyph(visual.Glyph);
            ApplyColorsAndState(snapshot, visual);
            if (snapshot.Kind == MapMarkerKind.Player)
                _glyphRoot.localRotation = Quaternion.Euler(0f, 0f, -snapshot.HeadingDegrees);
            else
                _glyphRoot.localRotation = Quaternion.identity;

            if (_expanded)
            {
                PositionOverviewText(_label, normalizedPosition.x > 0.5f, 0.62f);
                PositionOverviewText(_timer, normalizedPosition.x > 0.5f, 0.20f);
                _label.text = snapshot.DisplayName;
                UpdateRoundedTimer(snapshot);
            }
        }

        private static void PositionOverviewText(Text text, bool alignRight, float anchorY)
        {
            var rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(alignRight ? 0f : 1f, anchorY);
            rect.pivot = new Vector2(alignRight ? 1f : 0f, 0.5f);
            rect.anchoredPosition = new Vector2(alignRight ? -4f : 4f, 0f);
            text.alignment = alignRight ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        }

        public void SetSelection(bool selected)
        {
            for (var i = 0; i < _selectionFrame.Length; i++)
                _selectionFrame[i].gameObject.SetActive(selected);
        }

        private void ConfigureGlyph(MapMarkerGlyph glyph)
        {
            if (_glyphConfigured && _configuredGlyph == glyph) return;
            _glyphConfigured = true;
            _configuredGlyph = glyph;
            ResetGlyphPieces();

            switch (glyph)
            {
                case MapMarkerGlyph.Gate:
                    ConfigureBar(_primary.rectTransform, new Vector2(-7,0), 3, 17, 0);
                    ConfigureBar(_secondary.rectTransform, new Vector2(7,0), 3, 17, 0);
                    ConfigureBar(_accentLeft.rectTransform, new Vector2(0,7), 14, 2, 0);
                    _primary.gameObject.SetActive(true); _secondary.gameObject.SetActive(true); _accentLeft.gameObject.SetActive(true);
                    break;
                case MapMarkerGlyph.Chevron:
                    ConfigureBar(_primary.rectTransform, new Vector2(-4f, 0f), 4f, 18f, 34f);
                    ConfigureBar(_secondary.rectTransform, new Vector2(4f, 0f), 4f, 18f, -34f);
                    _primary.gameObject.SetActive(true);
                    _secondary.gameObject.SetActive(true);
                    break;
                case MapMarkerGlyph.Node:
                    ConfigureSquare(_primary.rectTransform, 13f, 0f);
                    ConfigureSquare(_cutout.rectTransform, 7f, 0f);
                    _primary.gameObject.SetActive(true);
                    break;
                case MapMarkerGlyph.BracketedNode:
                    ConfigureSquare(_primary.rectTransform, 13f, 0f);
                    ConfigureSquare(_cutout.rectTransform, 7f, 0f);
                    ConfigureBar(_accentLeft.rectTransform, new Vector2(-11f, 0f), 2f, 18f, 0f);
                    ConfigureBar(_accentRight.rectTransform, new Vector2(11f, 0f), 2f, 18f, 0f);
                    _primary.gameObject.SetActive(true);
                    _accentLeft.gameObject.SetActive(true);
                    _accentRight.gameObject.SetActive(true);
                    break;
                case MapMarkerGlyph.Diamond:
                    ConfigureSquare(_primary.rectTransform, 15f, 45f);
                    ConfigureSquare(_cutout.rectTransform, 8f, 45f);
                    _primary.gameObject.SetActive(true);
                    break;
                case MapMarkerGlyph.BossCore:
                    ConfigureSquare(_primary.rectTransform, 21f, 45f);
                    ConfigureSquare(_cutout.rectTransform, 13f, 45f);
                    ConfigureSquare(_secondary.rectTransform, 6f, 0f);
                    _primary.gameObject.SetActive(true);
                    _secondary.gameObject.SetActive(true);
                    break;
                case MapMarkerGlyph.RepairCross:
                    ConfigureBar(_primary.rectTransform, Vector2.zero, 5f, 19f, 0f);
                    ConfigureBar(_secondary.rectTransform, Vector2.zero, 19f, 5f, 0f);
                    _primary.gameObject.SetActive(true);
                    _secondary.gameObject.SetActive(true);
                    break;
            }
        }

        private void ApplyColorsAndState(MapMarkerSnapshot snapshot, MapMarkerVisualState visual)
        {
            var color = visual.LockOverlay || visual.InactiveSlash
                ? MapUiFactory.DisabledColor
                : MapUiFactory.ColorFor(snapshot.Kind);
            _primary.color = color;
            _secondary.color = color;
            _accentLeft.color = color;
            _accentRight.color = color;
            _cutout.color = _surfaceColor;
            _cutout.gameObject.SetActive(!visual.Filled &&
                                         visual.Glyph != MapMarkerGlyph.Chevron &&
                                         visual.Glyph != MapMarkerGlyph.Gate &&
                                         visual.Glyph != MapMarkerGlyph.RepairCross);

            _lockBar.gameObject.SetActive(visual.LockOverlay);
            _lockStem.gameObject.SetActive(visual.LockOverlay);
            if (visual.LockOverlay)
            {
                ConfigureBar(_lockBar.rectTransform, new Vector2(0f, -7f), 13f, 4f, 0f);
                ConfigureBar(_lockStem.rectTransform, new Vector2(0f, 1f), 3f, 10f, 0f);
                _lockBar.color = Color.white;
                _lockStem.color = Color.white;
            }

            _slash.gameObject.SetActive(visual.InactiveSlash);
            if (visual.InactiveSlash)
            {
                ConfigureBar(_slash.rectTransform, Vector2.zero, 2f, 27f, -45f);
                _slash.color = Color.white;
            }

            _completionStem.gameObject.SetActive(visual.CompletionMark);
            _completionTick.gameObject.SetActive(visual.CompletionMark);
            if (visual.CompletionMark)
            {
                ConfigureBar(_completionStem.rectTransform, new Vector2(-4f, -7f), 3f, 11f, -42f);
                ConfigureBar(_completionTick.rectTransform, new Vector2(4f, -3f), 3f, 18f, 42f);
                _completionStem.color = Color.white;
                _completionTick.color = Color.white;
            }

            ApplyActiveBrackets(visual.ActiveBrackets, color);
            ApplyCooldownRing(visual.CooldownRing, snapshot.CooldownProgress01, color);
            _rewardPip.gameObject.SetActive(visual.RewardPip);
            _rewardBar.gameObject.SetActive(visual.RewardPipBarred);
            if (visual.RewardPip)
                _rewardPip.color = visual.RewardPipBarred ? MapUiFactory.OrdinaryColor : Color.white;
        }

        private void ApplyActiveBrackets(bool visible, Color color)
        {
            for (var i = 0; i < _activeBrackets.Length; i++)
            {
                _activeBrackets[i].gameObject.SetActive(visible);
                _activeBrackets[i].color = color;
            }

            if (!visible) return;
            ConfigureBar(_activeBrackets[0].rectTransform, new Vector2(-13f, 8f), 7f, 2f, 0f);
            ConfigureBar(_activeBrackets[1].rectTransform, new Vector2(13f, 8f), 7f, 2f, 0f);
            ConfigureBar(_activeBrackets[2].rectTransform, new Vector2(-13f, -8f), 7f, 2f, 0f);
            ConfigureBar(_activeBrackets[3].rectTransform, new Vector2(13f, -8f), 7f, 2f, 0f);
        }

        private void ApplyCooldownRing(bool visible, float progress01, Color color)
        {
            var litSegments = progress01 < 0f
                ? RingSegments
                : Mathf.Clamp(Mathf.CeilToInt(progress01 * RingSegments), 1, RingSegments);
            for (var i = 0; i < _cooldownRing.Length; i++)
            {
                var enabled = visible && (progress01 < 0f || i < litSegments);
                _cooldownRing[i].gameObject.SetActive(enabled);
                var segmentColor = color;
                if (progress01 < 0f && (i & 1) == 1) segmentColor.a *= 0.35f;
                _cooldownRing[i].color = segmentColor;
            }
        }

        private void UpdateRoundedTimer(MapMarkerSnapshot snapshot)
        {
            var showTimer = snapshot.Availability == MapAvailabilityState.Cooldown &&
                            snapshot.RemainingSeconds > 0f &&
                            (snapshot.Kind == MapMarkerKind.Elite || snapshot.Kind == MapMarkerKind.Boss);
            _timer.gameObject.SetActive(showTimer);
            if (!showTimer)
            {
                _lastRoundedTimerBucket = int.MinValue;
                _timer.text = string.Empty;
                return;
            }

            var bucket = MapTimerFormatter.RoundedBucket(snapshot.RemainingSeconds);
            if (bucket == _lastRoundedTimerBucket) return;
            _lastRoundedTimerBucket = bucket;
            _timer.text = MapTimerFormatter.FormatRounded(snapshot.RemainingSeconds);
        }

        private void ResetGlyphPieces()
        {
            _primary.gameObject.SetActive(false);
            _secondary.gameObject.SetActive(false);
            _cutout.gameObject.SetActive(false);
            _accentLeft.gameObject.SetActive(false);
            _accentRight.gameObject.SetActive(false);
        }

        private static void ConfigureSquare(RectTransform rect, float size, float rotation)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            MapUiFactory.SetSize(rect, size, size);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static void ConfigureBar(RectTransform rect, Vector2 position, float width, float height, float rotation)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            MapUiFactory.SetSize(rect, width, height);
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static void ConfigureFrame(Image[] frame, float width, float height, float thickness)
        {
            var left = frame[0].rectTransform;
            var right = frame[1].rectTransform;
            var top = frame[2].rectTransform;
            var bottom = frame[3].rectTransform;
            left.anchorMin = left.anchorMax = new Vector2(0.5f, 0.5f);
            right.anchorMin = right.anchorMax = new Vector2(0.5f, 0.5f);
            top.anchorMin = top.anchorMax = new Vector2(0.5f, 0.5f);
            bottom.anchorMin = bottom.anchorMax = new Vector2(0.5f, 0.5f);
            left.anchoredPosition = new Vector2(-width * 0.5f, 0f);
            right.anchoredPosition = new Vector2(width * 0.5f, 0f);
            top.anchoredPosition = new Vector2(0f, height * 0.5f);
            bottom.anchoredPosition = new Vector2(0f, -height * 0.5f);
            MapUiFactory.SetSize(left, thickness, height);
            MapUiFactory.SetSize(right, thickness, height);
            MapUiFactory.SetSize(top, width, thickness);
            MapUiFactory.SetSize(bottom, width, thickness);
        }
    }
}
