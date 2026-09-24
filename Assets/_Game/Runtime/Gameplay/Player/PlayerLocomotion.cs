using System;
using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerLocomotion : MonoBehaviour
    {
        private CharacterController _characterController;
        private IMovementInput _movementInput;
        private Transform _cameraBasis;
        private PlayerMovementParameters _parameters;
        private bool _isInitialized;

        public void Initialize(
            IMovementInput movementInput,
            Transform cameraBasis,
            PlayerMovementParameters parameters)
        {
            _movementInput = movementInput ?? throw new ArgumentNullException(nameof(movementInput));
            _cameraBasis = cameraBasis != null ? cameraBasis : throw new ArgumentNullException(nameof(cameraBasis));
            _parameters = parameters;
            _characterController = GetComponent<CharacterController>();
            _isInitialized = true;
        }

        private void Update()
        {
            if (_isInitialized)
            {
                Step(Time.deltaTime);
            }
        }

        public void Step(float deltaTime)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("PlayerLocomotion must be initialized before stepping.");
            }

            if (deltaTime <= 0f)
            {
                return;
            }

            var input = Vector2.ClampMagnitude(_movementInput.Movement, 1f);
            if (input.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            var cameraForward = Vector3.ProjectOnPlane(_cameraBasis.forward, Vector3.up).normalized;
            var cameraRight = Vector3.ProjectOnPlane(_cameraBasis.right, Vector3.up).normalized;
            var movement = (cameraRight * input.x) + (cameraForward * input.y);

            _characterController.Move(movement * (_parameters.MoveSpeed * deltaTime));

            var targetRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _parameters.RotationDegreesPerSecond * deltaTime);
        }
    }
}
