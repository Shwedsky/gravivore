using System;
using UnityEngine;

namespace Gravivore.Presentation.Combat
{
    [CreateAssetMenu(menuName = "Gravivore/Presentation/Post device combat")]
    public sealed class PostDevicePresentationDefinition : ScriptableObject
    {
        [SerializeField, Range(1,8)] private int _visibleBars = 6;
        [SerializeField, Range(4,16)] private int _damageTextCapacity = 10;
        [SerializeField, Range(2,8)] private int _rewardTextCapacity = 4;
        [SerializeField] private Vector2 _plateSize = new Vector2(240,78);
        [SerializeField, Min(.1f)] private float _relevanceRange = 7f, _recentDamageSeconds = 2.5f;
        [SerializeField, Min(.1f)] private float _damageLifetime = .75f, _rewardLifetime = 1.1f, _textTravel = .6f;
        [SerializeField, Min(.1f)] private float _runCycleDistance = 1.8f, _locomotionBlendSeconds = .09f;
        [SerializeField, Range(0,.3f)] private float _footContactCorrection = .18f;
        [SerializeField, Min(.05f)] private float _hitchThreshold = .25f;
        [SerializeField, Min(1)] private float _hitchCooldown = 20f;
        [SerializeField, Range(1,8)] private int _hitchSnapshots = 4;
        public int VisibleBars => _visibleBars;
        public int DamageTextCapacity => _damageTextCapacity;
        public int RewardTextCapacity => _rewardTextCapacity;
        public Vector2 PlateSize => _plateSize;
        public float RelevanceRange => _relevanceRange;
        public float RecentDamageSeconds => _recentDamageSeconds;
        public float DamageLifetime => _damageLifetime;
        public float RewardLifetime => _rewardLifetime;
        public float TextTravel => _textTravel;
        public float RunCycleDistance => _runCycleDistance;
        public float LocomotionBlendSeconds => _locomotionBlendSeconds;
        public float FootContactCorrection => _footContactCorrection;
        public float HitchThreshold => _hitchThreshold;
        public float HitchCooldown => _hitchCooldown;
        public int HitchSnapshots => _hitchSnapshots;
        public void ValidateOrThrow()
        {
            if (_visibleBars < 1 || _visibleBars > 8 || _damageTextCapacity < 4 || _damageTextCapacity > 16 ||
                _rewardTextCapacity < 2 || _rewardTextCapacity > 8 || _plateSize.x < 180 || _plateSize.y < 60 ||
                _relevanceRange <= 0 || _damageLifetime <= 0 || _rewardLifetime <= 0 || _runCycleDistance <= 0 ||
                _locomotionBlendSeconds <= 0 || _hitchThreshold < .05f || _hitchCooldown < 1 || _hitchSnapshots < 1 || _hitchSnapshots > 8)
                throw new InvalidOperationException("Post-device presentation settings must remain readable and bounded.");
        }
    }
}
