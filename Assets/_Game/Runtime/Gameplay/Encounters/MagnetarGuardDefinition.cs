using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    [CreateAssetMenu(fileName = "MagnetarGuardDefinition", menuName = "Gravivore/Encounters/Magnetar Guard Definition")]
    public sealed class MagnetarGuardDefinition : ScriptableObject
    {
        [SerializeField] private string _id = "magnetar-guard";
        [SerializeField] private Vector3 _spawnPosition = new Vector3(0f, 0f, 18f);
        [SerializeField, Min(1f)] private float _maximumHitPoints = 250f;
        [SerializeField, Min(0f)] private float _armor = 10f;
        [SerializeField, Min(0.1f)] private float _moveSpeed = 1.8f;
        [SerializeField, Min(0.1f)] private float _attackDamage = 14f;
        [SerializeField, Min(0.1f)] private float _collisionRadius = 0.65f;
        [SerializeField, Min(0.1f)] private float _targetPointHeight = 1.2f;
        [SerializeField, Min(0.1f)] private float _aggroRadius = 8f;
        [SerializeField, Min(0.1f)] private float _attackRange = 2.4f;
        [SerializeField, Min(0.1f)] private float _shockwaveRadius = 3.2f;
        [SerializeField, Min(0.1f)] private float _telegraphDuration = 1f;
        [SerializeField, Min(0.1f)] private float _recoveryDuration = 1.4f;

        public string Id => _id;

        public MagnetarGuardConfiguration Configuration => new MagnetarGuardConfiguration(
            _id,
            _spawnPosition,
            _maximumHitPoints,
            _armor,
            _moveSpeed,
            _attackDamage,
            _collisionRadius,
            _targetPointHeight,
            _aggroRadius,
            _attackRange,
            _shockwaveRadius,
            _telegraphDuration,
            _recoveryDuration);

        public void ValidateOrThrow() => _ = Configuration;
    }
}
