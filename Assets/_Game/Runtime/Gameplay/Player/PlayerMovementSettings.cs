using UnityEngine;

namespace Gravivore.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Gravivore/Player/Movement Settings")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 4.5f;
        [SerializeField, Min(0f)] private float _rotationDegreesPerSecond = 540f;

        public PlayerMovementParameters Parameters =>
            new PlayerMovementParameters(_moveSpeed, _rotationDegreesPerSecond);

        private void OnValidate()
        {
            _moveSpeed = Mathf.Max(0f, _moveSpeed);
            _rotationDegreesPerSecond = Mathf.Max(0f, _rotationDegreesPerSecond);
        }
    }
}
