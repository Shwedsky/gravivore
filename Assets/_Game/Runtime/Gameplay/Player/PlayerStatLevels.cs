using System;

namespace Gravivore.Gameplay.Player
{
    public readonly struct PlayerStatLevels
    {
        public PlayerStatLevels(int power, int hull, int armor, int flux, int mobility)
        {
            ValidateLevel(power, nameof(power));
            ValidateLevel(hull, nameof(hull));
            ValidateLevel(armor, nameof(armor));
            ValidateLevel(flux, nameof(flux));
            ValidateLevel(mobility, nameof(mobility));

            Power = power;
            Hull = hull;
            Armor = armor;
            Flux = flux;
            Mobility = mobility;
        }

        public int Power { get; }

        public int Hull { get; }

        public int Armor { get; }

        public int Flux { get; }

        public int Mobility { get; }

        public int GetLevel(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Power:
                    return Power;
                case PlayerStatType.Hull:
                    return Hull;
                case PlayerStatType.Armor:
                    return Armor;
                case PlayerStatType.Flux:
                    return Flux;
                case PlayerStatType.Mobility:
                    return Mobility;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown player stat.");
            }
        }

        public PlayerStatLevels WithLevel(PlayerStatType stat, int level)
        {
            ValidateLevel(level, nameof(level));
            switch (stat)
            {
                case PlayerStatType.Power:
                    return new PlayerStatLevels(level, Hull, Armor, Flux, Mobility);
                case PlayerStatType.Hull:
                    return new PlayerStatLevels(Power, level, Armor, Flux, Mobility);
                case PlayerStatType.Armor:
                    return new PlayerStatLevels(Power, Hull, level, Flux, Mobility);
                case PlayerStatType.Flux:
                    return new PlayerStatLevels(Power, Hull, Armor, level, Mobility);
                case PlayerStatType.Mobility:
                    return new PlayerStatLevels(Power, Hull, Armor, Flux, level);
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown player stat.");
            }
        }

        private static void ValidateLevel(int level, string parameterName)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Stat levels start at one.");
            }
        }
    }
}
