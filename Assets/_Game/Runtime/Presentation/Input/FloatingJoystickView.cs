using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class FloatingJoystickView : MonoBehaviour
    {
        private const int TextureSize = 64;

        private RectTransform _interactionRect;
        private RectTransform _baseRect;
        private RectTransform _knobRect;
        private Texture2D _circleTexture;
        private Sprite _circleSprite;
        private bool _isInitialized;

        public void Initialize(RectTransform interactionRect, FloatingJoystickSettings settings)
        {
            _interactionRect = interactionRect != null
                ? interactionRect
                : throw new ArgumentNullException(nameof(interactionRect));
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _circleTexture = CreateCircleTexture();
            _circleSprite = Sprite.Create(
                _circleTexture,
                new Rect(0f, 0f, TextureSize, TextureSize),
                new Vector2(0.5f, 0.5f),
                TextureSize);
            _circleSprite.name = "Floating Joystick Circle";
            _circleSprite.hideFlags = HideFlags.HideAndDontSave;

            _baseRect = CreateImage("Joystick Base", settings.BaseDiameter, settings.BaseColor);
            _knobRect = CreateImage("Joystick Knob", settings.KnobDiameter, settings.KnobColor);
            _isInitialized = true;
            Hide();
        }

        public bool ContainsScreenPoint(Vector2 screenPosition)
        {
            return _isInitialized &&
                   RectTransformUtility.RectangleContainsScreenPoint(_interactionRect, screenPosition);
        }

        public bool TryScreenToLocal(Vector2 screenPosition, out Vector2 localPosition)
        {
            if (!_isInitialized)
            {
                localPosition = Vector2.zero;
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _interactionRect,
                screenPosition,
                null,
                out localPosition);
        }

        public void Show(Vector2 origin)
        {
            gameObject.SetActive(true);
            SetPositions(origin, origin);
        }

        public void SetPositions(Vector2 origin, Vector2 knobPosition)
        {
            if (!_isInitialized)
            {
                return;
            }

            _baseRect.anchoredPosition = origin;
            _knobRect.anchoredPosition = knobPosition;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_circleSprite != null)
            {
                Destroy(_circleSprite);
            }

            if (_circleTexture != null)
            {
                Destroy(_circleTexture);
            }
        }

        private RectTransform CreateImage(string objectName, float diameter, Color color)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.SetParent(transform, false);
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.sizeDelta = new Vector2(diameter, diameter);

            var image = imageObject.GetComponent<Image>();
            image.sprite = _circleSprite;
            image.color = color;
            image.raycastTarget = false;
            return imageRect;
        }

        private static Texture2D CreateCircleTexture()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = "Floating Joystick Circle Texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            var pixels = new Color32[TextureSize * TextureSize];
            var center = (TextureSize - 1) * 0.5f;
            var radiusSquared = center * center;

            for (var y = 0; y < TextureSize; y++)
            {
                for (var x = 0; x < TextureSize; x++)
                {
                    var deltaX = x - center;
                    var deltaY = y - center;
                    pixels[(y * TextureSize) + x] = deltaX * deltaX + deltaY * deltaY <= radiusSquared
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
