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

        private FloatingJoystickSettings _settings;
        private IUiTouchExclusion _uiTouchExclusion;
        private FloatingJoystickView _view;
        private Vector2 _origin;
        private Vector2 _movement;
        private int _activePointerId = NoPointer;
        private bool _isInitialized;

        public Vector2 Movement => _movement;

        public void Initialize(
            FloatingJoystickSettings settings,
            IUiTouchExclusion uiTouchExclusion,
            FloatingJoystickView view)
        {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _uiTouchExclusion = uiTouchExclusion ?? throw new ArgumentNullException(nameof(uiTouchExclusion));
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
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

        private void OnDisable()
        {
            ResetPointer();
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
                _uiTouchExclusion.Blocks(screenPosition) ||
                !_view.ContainsScreenPoint(screenPosition) ||
                !_view.TryScreenToLocal(screenPosition, out _origin))
            {
                return false;
            }

            _activePointerId = pointerId;
            _view.Show(_origin);
            UpdatePointer(screenPosition);
            return true;
        }

        private void UpdatePointer(Vector2 screenPosition)
        {
            if (!_view.TryScreenToLocal(screenPosition, out var localPosition))
            {
                return;
            }

            var rawInput = MovementInputMath.NormalizeDrag(_origin, localPosition, _settings.Radius);
            _movement = MovementInputMath.ApplyRadialDeadZone(rawInput, _settings.DeadZone);
            _view.SetPositions(_origin, _origin + (rawInput * _settings.Radius));
        }

        private void ResetPointer()
        {
            _activePointerId = NoPointer;
            _movement = Vector2.zero;
            if (_view != null)
            {
                _view.Hide();
            }
        }
    }
}
