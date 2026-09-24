using System;
using Gravivore.Core.Input;
using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Presentation.Input
{
    [DisallowMultipleComponent]
    public sealed class FloatingJoystickInput : MonoBehaviour, IMovementInput
    {
        private const int NoPointer = -1;
        private const int MousePointer = -2;
        private const int TextureSize = 64;

        private FloatingJoystickSettings _settings;
        private IUiTouchExclusion _uiTouchExclusion;
        private Texture2D _circleTexture;
        private Vector2 _origin;
        private Vector2 _pointerPosition;
        private Vector2 _movement;
        private int _activePointerId = NoPointer;
        private bool _isInitialized;

        public Vector2 Movement => _movement;

        public void Initialize(FloatingJoystickSettings settings, IUiTouchExclusion uiTouchExclusion)
        {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _uiTouchExclusion = uiTouchExclusion ?? throw new ArgumentNullException(nameof(uiTouchExclusion));
            _circleTexture = CreateCircleTexture();
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            UpdateTouchInput();

#if UNITY_EDITOR
            UpdateMouseInput();
#endif
        }

        private void OnGUI()
        {
            if (!_isInitialized || _activePointerId == NoPointer || _circleTexture == null)
            {
                return;
            }

            var guiOrigin = ToGuiPosition(_origin);
            var guiPointer = ToGuiPosition(_pointerPosition);
            DrawCircle(guiOrigin, _settings.BaseDiameter, _settings.BaseColor);
            DrawCircle(guiPointer, _settings.KnobDiameter, _settings.KnobColor);
        }

        private void OnDisable()
        {
            ResetPointer();
        }

        private void OnDestroy()
        {
            if (_circleTexture != null)
            {
                Destroy(_circleTexture);
            }
        }

        private void UpdateTouchInput()
        {
            if (_activePointerId >= 0)
            {
                for (var i = 0; i < UnityEngine.Input.touchCount; i++)
                {
                    var touch = UnityEngine.Input.GetTouch(i);
                    if (touch.fingerId != _activePointerId)
                    {
                        continue;
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        ResetPointer();
                    }
                    else
                    {
                        UpdatePointer(touch.position);
                    }

                    return;
                }

                ResetPointer();
                return;
            }

            if (_activePointerId != NoPointer)
            {
                return;
            }

            for (var i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began && TryBeginPointer(touch.fingerId, touch.position))
                {
                    return;
                }
            }
        }

#if UNITY_EDITOR
        private void UpdateMouseInput()
        {
            if (_activePointerId >= 0)
            {
                return;
            }

            if (_activePointerId == NoPointer && UnityEngine.Input.GetMouseButtonDown(0))
            {
                TryBeginPointer(MousePointer, UnityEngine.Input.mousePosition);
            }

            if (_activePointerId != MousePointer)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                ResetPointer();
            }
            else if (UnityEngine.Input.GetMouseButton(0))
            {
                UpdatePointer(UnityEngine.Input.mousePosition);
            }
        }
#endif

        private bool TryBeginPointer(int pointerId, Vector2 screenPosition)
        {
            if (screenPosition.y > Screen.height * _settings.ActivationMaxScreenHeight ||
                _uiTouchExclusion.Blocks(screenPosition))
            {
                return false;
            }

            _activePointerId = pointerId;
            _origin = screenPosition;
            UpdatePointer(screenPosition);
            return true;
        }

        private void UpdatePointer(Vector2 screenPosition)
        {
            var rawInput = MovementInputMath.NormalizeDrag(_origin, screenPosition, _settings.Radius);
            _movement = MovementInputMath.ApplyRadialDeadZone(rawInput, _settings.DeadZone);
            _pointerPosition = _origin + (rawInput * _settings.Radius);
        }

        private void ResetPointer()
        {
            _activePointerId = NoPointer;
            _movement = Vector2.zero;
            _pointerPosition = _origin;
        }

        private void DrawCircle(Vector2 center, float diameter, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(
                new Rect(center.x - (diameter * 0.5f), center.y - (diameter * 0.5f), diameter, diameter),
                _circleTexture);
            GUI.color = previousColor;
        }

        private static Vector2 ToGuiPosition(Vector2 screenPosition)
        {
            return new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        }

        private static Texture2D CreateCircleTexture()
        {
            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                name = "Floating Joystick Circle",
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
