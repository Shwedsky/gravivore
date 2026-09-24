using System;

namespace Gravivore.Gameplay.Player
{
    public readonly struct PlayerMovementParameters
    {
        public PlayerMovementParameters(float moveSpeed, float rotationDegreesPerSecond)
        {
            if (moveSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(moveSpeed));
            }

            if (rotationDegreesPerSecond < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rotationDegreesPerSecond));
            }

            MoveSpeed = moveSpeed;
            RotationDegreesPerSecond = rotationDegreesPerSecond;
        }

        public float MoveSpeed { get; }

        public float RotationDegreesPerSecond { get; }
    }
}
