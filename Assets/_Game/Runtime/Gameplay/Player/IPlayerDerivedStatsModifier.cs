namespace Gravivore.Gameplay.Player
{
    public interface IPlayerDerivedStatsModifier
    {
        PlayerDerivedStats Apply(in PlayerDerivedStats currentValues);
    }
}
