using System;
using UnityEngine;

namespace Gravivore.Presentation.Camera
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class PortraitFollowCamera : MonoBehaviour
    {
        private Transform _target;
        private CameraFollowSettings _settings;
        private Vector3 _velocity;
        private bool _isInitialized;

        public void Initialize(Transform target, CameraFollowSettings settings)
        {
            _target = target != null ? target : throw new ArgumentNullException(nameof(target));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));

            var cameraComponent = GetComponent<UnityEngine.Camera>();
            cameraComponent.fieldOfView = _settings.FieldOfView;
            SnapToTarget();
            _isInitialized = true;
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
            {
                return;
            }

            var targetPosition = _target.position + _settings.Offset;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                _settings.PositionDamping);
        }

        public void SnapToTarget()
        {
            if (_target == null || _settings == null)
            {
                throw new InvalidOperationException("PortraitFollowCamera must be initialized before snapping.");
            }

            transform.position = _target.position + _settings.Offset;
            transform.rotation = Quaternion.LookRotation(
                (_target.position + (Vector3.up * _settings.LookAtHeight)) - transform.position,
                Vector3.up);
            _velocity = Vector3.zero;
        }
    }
}
