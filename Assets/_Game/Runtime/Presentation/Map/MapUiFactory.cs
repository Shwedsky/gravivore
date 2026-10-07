using Gravivore.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Map
{
    internal static class MapUiFactory
    {
        public static readonly Color SurfaceColor = new Color(0.013f, 0.030f, 0.045f, 0.97f);
        public static readonly Color SurfaceColorExpanded = new Color(0.018f, 0.033f, 0.043f, 0.985f);
        public static readonly Color GridColor = new Color(.28f, .41f, .49f, .28f);
        public static readonly Color PlayerColor = new Color(0.25f, 0.88f, 0.88f, 1f);
        public static readonly Color OrdinaryColor = new Color(0.84f, 0.49f, 0.37f, 1f);
        public static readonly Color EliteColor = new Color(0.88f, 0.67f, 0.30f, 1f);
        public static readonly Color BossColor = new Color(0.94f, 0.38f, 0.25f, 1f);
        public static readonly Color SystemColor = new Color(0.35f, 0.78f, 0.72f, 1f);
        public static readonly Color DisabledColor = new Color(0.43f, 0.48f, 0.49f, 0.82f);

        public static RectTransform CreateRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image CreateImage(Transform parent, string name, Color color, bool raycastTarget = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = HudUiFactory.GetRuntimeFont();
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button AddButton(RectTransform rect)
        {
            var image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
                image.color = Color.clear;
            }

            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            return button;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color, bool raycastTarget = false)
        {
            var image = CreateImage(parent, name, color, raycastTarget);
            return image.rectTransform;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void PointAnchor(RectTransform rect, Vector2 normalized)
        {
            rect.anchorMin = normalized;
            rect.anchorMax = normalized;
            rect.anchoredPosition = Vector2.zero;
        }

        public static void SetSize(RectTransform rect, float width, float height)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public static void CreateGrid(RectTransform parent, int verticalLines, int horizontalLines)
        {
            for (var i = 1; i < verticalLines; i++)
            {
                var line = CreateImage(parent, $"GridV{i}", GridColor).rectTransform;
                var x = i / (float)verticalLines;
                line.anchorMin = new Vector2(x, 0f);
                line.anchorMax = new Vector2(x, 1f);
                line.pivot = new Vector2(0.5f, 0.5f);
                line.sizeDelta = new Vector2(1f, 0f);
            }

            for (var i = 1; i < horizontalLines; i++)
            {
                var line = CreateImage(parent, $"GridH{i}", GridColor).rectTransform;
                var y = i / (float)horizontalLines;
                line.anchorMin = new Vector2(0f, y);
                line.anchorMax = new Vector2(1f, y);
                line.pivot = new Vector2(0.5f, 0.5f);
                line.sizeDelta = new Vector2(0f, 1f);
            }
        }
        public static void TacticalFrame(RectTransform panel)
        {
            var color = new Color(.39f,.57f,.64f,.8f);
            for (var i = 0; i < 4; i++)
            {
                var anchor = new Vector2(i % 2, i / 2);
                var h = CreateImage(panel, "TacticalCornerH" + i, color).rectTransform;
                var v = CreateImage(panel, "TacticalCornerV" + i, color).rectTransform;
                PointAnchor(h, anchor); PointAnchor(v, anchor);
                h.pivot = v.pivot = anchor;
                SetSize(h, 22, 2); SetSize(v, 2, 22);
            }
            var rail = CreateImage(panel, "AmberStatusRail", EliteColor).rectTransform;
            rail.anchorMin = new Vector2(.08f,.985f); rail.anchorMax = new Vector2(.31f,.985f);
            rail.sizeDelta = new Vector2(0,2);
        }

        public static Color ColorFor(MapMarkerKind kind)
        {
            switch (kind)
            {
                case MapMarkerKind.Player: return PlayerColor;
                case MapMarkerKind.Ordinary: return OrdinaryColor;
                case MapMarkerKind.StrongOrdinary: return new Color(.86f,.62f,.30f,1);
                case MapMarkerKind.Elite: return EliteColor;
                case MapMarkerKind.Boss: return BossColor;
                case MapMarkerKind.RepairHub: return SystemColor;
                case MapMarkerKind.Gate: return SystemColor;
                default: return OrdinaryColor;
            }
        }
    }
}
