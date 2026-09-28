using System;
using UnityEngine;

namespace Gravivore.Presentation.Quests
{
    [CreateAssetMenu(fileName = "QuestOnboardingDefinition", menuName = "Gravivore/Quests/Onboarding Presentation Definition")]
    public sealed class QuestOnboardingDefinition : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _movementInputDeadZone = 0.2f;
        [SerializeField, Min(0.01f)] private float _movementInputSeconds = 0.25f;

        public float MovementInputDeadZone => _movementInputDeadZone;
        public float MovementInputSeconds => _movementInputSeconds;

        public void ValidateOrThrow()
        {
            if (float.IsNaN(_movementInputDeadZone) || float.IsInfinity(_movementInputDeadZone) ||
                _movementInputDeadZone < 0f || _movementInputDeadZone >= 1f ||
                float.IsNaN(_movementInputSeconds) || float.IsInfinity(_movementInputSeconds) ||
                _movementInputSeconds <= 0f)
            {
                throw new InvalidOperationException("Onboarding movement thresholds are invalid.");
            }
        }
    }
}
