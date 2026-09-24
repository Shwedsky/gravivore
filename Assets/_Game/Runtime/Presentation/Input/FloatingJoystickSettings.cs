using UnityEngine;

namespace Gravivore.Presentation.Input
{
    [CreateAssetMenu(fileName = "FloatingJoystickSettings", menuName = "Gravivore/Input/Floating Joystick Settings")]
    public sealed class FloatingJoystickSettings : ScriptableObject
    {
        [SerializeField, Min(1f)] private float _radius = 110f;
        [SerializeField, Range(0f, 0.95f)] private float _deadZone = 0.12f;
        [SerializeField, Range(0.1f, 1f)] private float _activationMaxScreenHeight = 0.78f;
        [SerializeField, Min(1f)] private float _baseDiameter = 144f;
        [SerializeField, Min(1f)] private float _knobDiameter = 64f;
        [SerializeField] private Color _baseColor = new Color(0.08f, 0.12f, 0.16f, 0.58f);
        [SerializeField] private Color _knobColor = new Color(0.35f, 0.95f, 0.82f, 0.9f);

        public float Radius => _radius;
        public float DeadZone => _deadZone;
        public float ActivationMaxScreenHeight => _activationMaxScreenHeight;
        public float BaseDiameter => _baseDiameter;
        public float KnobDiameter => _knobDiameter;
        public Color BaseColor => _baseColor;
        public Color KnobColor => _knobColor;

        private void OnValidate()
        {
            _radius = Mathf.Max(1f, _radius);
            _deadZone = Mathf.Clamp(_deadZone, 0f, 0.95f);
            _activationMaxScreenHeight = Mathf.Clamp(_activationMaxScreenHeight, 0.1f, 1f);
            _baseDiameter = Mathf.Max(1f, _baseDiameter);
            _knobDiameter = Mathf.Clamp(_knobDiameter, 1f, _baseDiameter);
        }
    }
}
