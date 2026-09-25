using System;

namespace Gravivore.Gameplay.Enemies
{
    public readonly struct EnemyLifeId : IEquatable<EnemyLifeId>
    {
        public EnemyLifeId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Enemy life id cannot be empty.", nameof(value));
            }

            Value = value;
        }

        public Guid Value { get; }

        public bool IsValid => Value != Guid.Empty;

        public bool Equals(EnemyLifeId other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is EnemyLifeId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString("N");

        public static bool operator ==(EnemyLifeId left, EnemyLifeId right) => left.Equals(right);

        public static bool operator !=(EnemyLifeId left, EnemyLifeId right) => !left.Equals(right);
    }
}
