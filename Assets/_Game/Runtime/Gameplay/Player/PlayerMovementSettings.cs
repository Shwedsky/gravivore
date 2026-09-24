using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Gravivore/Player/Movement Settings")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _rotationDegreesPerSecond = 540f;

        public float RotationDegreesPerSecond => _rotationDegreesPerSecond;

        private void OnValidate()
        {
            _rotationDegreesPerSecond = Mathf.Max(0f, _rotationDegreesPerSecond);
        }
    }
}
