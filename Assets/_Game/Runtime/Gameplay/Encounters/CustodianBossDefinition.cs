using Gravivore.Gameplay.World;
using UnityEngine;

namespace Gravivore.Gameplay.Encounters
{
    [CreateAssetMenu(fileName = "CustodianBossDefinition", menuName = "Gravivore/Encounters/Custodian Boss Definition")]
    public sealed class CustodianBossDefinition : ScriptableObject
    {
        [SerializeField] private string _id = "custodian-m0";
        [SerializeField] private Vector3 _startPosition = new Vector3(0f, 0f, 27f);
        [SerializeField, Min(1f)] private float _maximumHitPoints = 1000f;
        [SerializeField, Min(0f)] private float _armor = 20f;
        [SerializeField, Min(0.1f)] private float _collisionRadius = 1f;
        [SerializeField, Min(0.1f)] private float _targetPointHeight = 1.5f;
        [SerializeField, Min(0.1f)] private float _recoveryDuration = 1.5f;
        [SerializeField, Range(0.01f, 0.99f)] private float _lowHealthThreshold = 0.35f;
        [SerializeField, Range(0.01f, 0.99f)] private float _lowHealthCadenceMultiplier = 0.65f;
        [SerializeField, Min(0f)] private float _arenaExitResetGraceSeconds = 3f;
        [Header("Circle Pulse")]
        [SerializeField, Range(0.8f, 1.2f)] private float _circleTelegraphDuration = 1f;
        [SerializeField, Min(0.1f)] private float _circleDamage = 22f;
        [SerializeField, Min(0.1f)] private float _circleRadius = 4f;
        [Header("Cone Sweep")]
        [SerializeField, Range(0.8f, 1.2f)] private float _coneTelegraphDuration = 1f;
        [SerializeField, Min(0.1f)] private float _coneDamage = 24f;
        [SerializeField, Min(0.1f)] private float _coneRange = 6f;
        [SerializeField, Range(1f, 89f)] private float _coneHalfAngle = 35f;
        [Header("Line Charge")]
        [SerializeField, Range(0.8f, 1.2f)] private float _lineTelegraphDuration = 1.1f;
        [SerializeField, Min(0.1f)] private float _lineDamage = 28f;
        [SerializeField, Min(0.1f)] private float _lineLength = 7f;
        [SerializeField, Min(0.1f)] private float _lineWidth = 1.8f;
        [SerializeField, Min(0.1f)] private float _chargeDistance = 5f;

        [Header("V3 combat pressure")]
        [SerializeField] private EncounterBasicAttackSettings _basicAttack = new EncounterBasicAttackSettings();
        [SerializeField, Min(.1f)] private float _initialAggroRadius = 8f;
        [SerializeField, Min(.1f)] private float _combatLeashRadius = 21f;
        [SerializeField, Min(.1f)] private float _pursuitSpeed = 3.2f;
        public string Id => _id;

        public CustodianBossConfiguration CreateConfiguration(Chapter01WorldConfiguration world)
        {
            if (world == null) throw new System.ArgumentNullException(nameof(world));
            var attacks = new[]
            {
                new BossAttackConfiguration(BossAttackType.CirclePulse, _circleTelegraphDuration, _circleDamage, _circleRadius, 0f, 0f, 0f),
                new BossAttackConfiguration(BossAttackType.ConeSweep, _coneTelegraphDuration, _coneDamage, _coneRange, _coneHalfAngle, 0f, 0f),
                new BossAttackConfiguration(BossAttackType.LineCharge, _lineTelegraphDuration, _lineDamage, _lineLength, 0f, _lineWidth, _chargeDistance)
            };
            return new CustodianBossConfiguration(
                _id,
                _startPosition,
                world.BossArenaCenter,
                world.BossArenaRadius,
                world.Bounds,
                _maximumHitPoints,
                _armor,
                _collisionRadius,
                _targetPointHeight,
                _recoveryDuration,
                _lowHealthThreshold,
                _lowHealthCadenceMultiplier,
                attacks,
                new[] { BossAttackType.CirclePulse, BossAttackType.ConeSweep, BossAttackType.LineCharge },
                _arenaExitResetGraceSeconds, _basicAttack.Configuration, _initialAggroRadius, _combatLeashRadius, _pursuitSpeed);
        }

        public void ValidateOrThrow(Chapter01WorldConfiguration world) => _ = CreateConfiguration(world);
    }
}
