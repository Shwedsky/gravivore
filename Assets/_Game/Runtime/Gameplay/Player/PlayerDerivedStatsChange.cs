namespace Gravivore.Gameplay.Player
{
    public enum PlayerDerivedStatsChangeReason
    {
        LevelChanged = 0,
        ModifiersChanged = 1,
        Recalculated = 2
    }

    public readonly struct PlayerDerivedStatsChange
    {
        public PlayerDerivedStatsChange(
            PlayerDerivedStats previousValues,
            PlayerDerivedStats currentValues,
            PlayerDerivedStatsChangeReason reason)
        {
            PreviousValues = previousValues;
            CurrentValues = currentValues;
            Reason = reason;
        }

        public PlayerDerivedStats PreviousValues { get; }

        public PlayerDerivedStats CurrentValues { get; }

        public PlayerDerivedStatsChangeReason Reason { get; }
    }
}
