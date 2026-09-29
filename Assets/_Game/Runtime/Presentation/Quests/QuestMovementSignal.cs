using System;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Quests;
using UnityEngine;

namespace Gravivore.Presentation.Quests
{
    [DisallowMultipleComponent]
    public sealed class QuestMovementSignal : MonoBehaviour
    {
        private IMovementInput _movementInput;
        private QuestService _quests;
        private float _deadZone;
        private float _requiredSeconds;
        private float _accumulatedSeconds;
        private bool _completed;

        public void Initialize(IMovementInput movementInput, QuestService quests, float deadZone, float requiredSeconds)
        {
            _movementInput = movementInput ?? throw new ArgumentNullException(nameof(movementInput));
            _quests = quests ?? throw new ArgumentNullException(nameof(quests));
            _deadZone = deadZone;
            _requiredSeconds = requiredSeconds;
            _completed = IsMovementObjectiveComplete();
        }

        public void Tick(float deltaTime)
        {
            if (_completed || deltaTime <= 0f) return;
            if (_movementInput.Movement.magnitude <= _deadZone) return;
            _accumulatedSeconds += deltaTime;
            if (_accumulatedSeconds < _requiredSeconds) return;
            _completed = true;
            _quests.RecordMovementPerformed();
        }

        private void Update() => Tick(Time.deltaTime);

        private bool IsMovementObjectiveComplete()
        {
            var active = _quests.ActiveObjective;
            return active.HasValue && active.Value.Type != QuestObjectiveType.MovementPerformed;
        }
    }
}
