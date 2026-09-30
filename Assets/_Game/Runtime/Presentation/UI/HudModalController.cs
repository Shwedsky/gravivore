using System;
using Gravivore.Presentation.Input;
using UnityEngine;

namespace Gravivore.Presentation.UI
{
    public sealed class HudModalController : IDisposable
    {
        private readonly FloatingJoystickInput _movementInput;
        private object _owner;
        private float _previousTimeScale = 1f;

        public HudModalController(FloatingJoystickInput movementInput)
        {
            _movementInput = movementInput != null
                ? movementInput
                : throw new ArgumentNullException(nameof(movementInput));
        }

        public event Action Available;

        public bool IsBlocked => _owner != null;

        public bool TryOpen(object owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (_owner != null) return ReferenceEquals(_owner, owner);
            _owner = owner;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _movementInput.enabled = false;
            return true;
        }

        public void Close(object owner)
        {
            if (!ReferenceEquals(_owner, owner)) return;
            _owner = null;
            _movementInput.enabled = true;
            Time.timeScale = _previousTimeScale;
            Available?.Invoke();
        }

        public void Dispose()
        {
            if (_owner == null) return;
            _owner = null;
            _movementInput.enabled = true;
            Time.timeScale = _previousTimeScale;
        }
    }
}
