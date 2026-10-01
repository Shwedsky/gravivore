using UnityEngine;

namespace Gravivore.Presentation.UI
{
    internal sealed class HealthBarView
    {
        private HealthBarView(RectTransform fillRect)
        {
            FillRect = fillRect;
        }

        public RectTransform FillRect { get; }

        public float NormalizedValue => FillRect.anchorMax.x;

        public static HealthBarView Create(
            RectTransform parent,
            string trackName,
            string fillName,
            Vector2 trackAnchorMin,
            Vector2 trackAnchorMax,
            Color trackColor,
            Color fillColor)
        {
            var track = HudUiFactory.CreatePanel(
                parent,
                trackName,
                trackAnchorMin,
                trackAnchorMax,
                trackColor);
            var fill = HudUiFactory.CreatePanel(
                track,
                fillName,
                Vector2.zero,
                Vector2.one,
                fillColor);
            return new HealthBarView(fill);
        }

        public void SetNormalizedValue(float value)
        {
            var normalized = Mathf.Clamp01(value);
            FillRect.anchorMin = Vector2.zero;
            FillRect.anchorMax = new Vector2(normalized, 1f);
            FillRect.offsetMin = Vector2.zero;
            FillRect.offsetMax = Vector2.zero;
        }
    }
}
