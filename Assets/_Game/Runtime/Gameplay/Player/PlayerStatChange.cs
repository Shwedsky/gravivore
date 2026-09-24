namespace Gravivore.Gameplay.Player
{
    public readonly struct PlayerStatChange
    {
        public PlayerStatChange(
            PlayerStatType stat,
            int previousLevel,
            int currentLevel,
            PlayerDerivedStats previousDerivedStats,
            PlayerDerivedStats currentDerivedStats)
        {
            Stat = stat;
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            PreviousDerivedStats = previousDerivedStats;
            CurrentDerivedStats = currentDerivedStats;
        }

        public PlayerStatType Stat { get; }

        public int PreviousLevel { get; }

        public int CurrentLevel { get; }

        public PlayerDerivedStats PreviousDerivedStats { get; }

        public PlayerDerivedStats CurrentDerivedStats { get; }
    }
}
