using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    internal static class HudUiFactory
    {
        private static Font _font;

        public static readonly Color PanelColor = new Color(0.025f, 0.045f, 0.055f, 0.88f);
        public static readonly Color ModalBackdropColor = new Color(0.01f, 0.018f, 0.025f, 0.82f);
        public static readonly Color AccentColor = new Color(0.18f, 0.69f, 0.80f, 1f);
        public static readonly Color WarningColor = new Color(0.95f, 0.32f, 0.28f, 1f);

        public static RectTransform CreatePanel(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color,
            bool raycastTarget = false)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetRect(rect, anchorMin, anchorMax);
            var image = gameObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            ProductionUiSkinScope.Frame(rect,color);
            return rect;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string value,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetRect(rect, anchorMin, anchorMax);
            var text = gameObject.GetComponent<Text>();
            text.font = GetRuntimeFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string label,
            Action onClick)
        {
            var rect = CreatePanel(
                parent,
                name,
                anchorMin,
                anchorMax,
                new Color(0.08f, 0.22f, 0.24f, 0.96f),
                true);
            var button = rect.gameObject.AddComponent<Button>();
            var skin=rect.GetComponentInParent<ProductionUiSkinScope>()?.Definition;
            if(skin?.Button!=null){var surface=rect.GetComponent<Image>();surface.sprite=skin.Button;surface.type=Image.Type.Sliced;}
            var colors = button.colors;
            colors.highlightedColor = new Color(0.16f, 0.48f, 0.46f, 1f);
            colors.pressedColor = new Color(0.08f, 0.62f, 0.54f, 1f);
            button.colors = colors;
            var text = CreateText(
                rect,
                "Label",
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.95f),
                label,
                30,
                TextAnchor.MiddleCenter,
                Color.white);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 22;
            text.resizeTextMaxSize = 30;
            if (onClick != null) button.onClick.AddListener(() => onClick());
            return button;
        }

        public static Button CreateCompactButton(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            string label,
            Action onClick,
            out RectTransform visualRect)
        {
            var hitRect = CreatePanel(parent, name, anchorMin, anchorMax, Color.clear, true);
            var button = hitRect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            visualRect = CreatePanel(
                hitRect,
                "Visual",
                new Vector2(0.18f, 0.18f),
                new Vector2(0.82f, 0.82f),
                new Color(0.08f, 0.22f, 0.24f, 0.96f));
            var text = CreateText(
                visualRect,
                "Label",
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.95f),
                label,
                26,
                TextAnchor.MiddleCenter,
                Color.white);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 18;
            text.resizeTextMaxSize = 26;
            if (onClick != null) button.onClick.AddListener(() => onClick());
            return button;
        }

        public static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal static Font GetRuntimeFont()
        {
            if (_font != null) return _font;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
            {
                throw new InvalidOperationException("Unity built-in font LegacyRuntime.ttf is unavailable.");
            }

            if (!RussianUiText.FontSupportsCyrillic(_font))
            {
                throw new InvalidOperationException("Unity built-in font LegacyRuntime.ttf does not support required Cyrillic glyphs.");
            }

            return _font;
        }
    }
}
