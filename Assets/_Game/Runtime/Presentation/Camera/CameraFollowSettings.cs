using UnityEngine;

namespace Gravivore.Presentation.Camera
{
    [CreateAssetMenu(fileName = "CameraFollowSettings", menuName = "Gravivore/Camera/Follow Settings")]
    public sealed class CameraFollowSettings : ScriptableObject
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -7f);
        [SerializeField, Min(0f)] private float _positionDamping = 0.16f;
        [SerializeField] private float _lookAtHeight = 0.8f;
        [SerializeField, Range(1f, 179f)] private float _fieldOfView = 42f;

        public Vector3 Offset => _offset;
        public float PositionDamping => _positionDamping;
        public float LookAtHeight => _lookAtHeight;
        public float FieldOfView => _fieldOfView;

        private void OnValidate()
        {
            _positionDamping = Mathf.Max(0f, _positionDamping);
            _fieldOfView = Mathf.Clamp(_fieldOfView, 1f, 179f);
        }
    }
}
