using UnityEngine;

namespace Gravivore.Presentation.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaHudRoot : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplyCurrentSafeArea();
        }

        private void Update()
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.safeArea != _lastSafeArea || screenSize != _lastScreenSize)
            {
                ApplyCurrentSafeArea();
            }
        }

        public void ApplyCurrentSafeArea()
        {
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                return;
            }

            CalculateAnchors(Screen.safeArea, screenSize, out var anchorMin, out var anchorMax);
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = screenSize;
        }

        public static void CalculateAnchors(
            Rect safeArea,
            Vector2Int screenSize,
            out Vector2 anchorMin,
            out Vector2 anchorMax)
        {
            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
            anchorMin = Vector2.Max(Vector2.zero, Vector2.Min(Vector2.one, anchorMin));
            anchorMax = Vector2.Max(anchorMin, Vector2.Min(Vector2.one, anchorMax));
        }
    }
}
