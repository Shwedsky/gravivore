using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Presentation.UI
{
    public static class HudTextFormatter
    {
        public static string Health(float current, float maximum)
        {
            return FormattableString.Invariant(
                $"HP {Math.Max(0f, current):0} / {Math.Max(0f, maximum):0}");
        }

        public static string Stats(PlayerStatLevels levels, PlayerDerivedStats derived, bool expanded)
        {
            var text =
                $"Power  L{levels.Power}\n" +
                $"Hull  L{levels.Hull}\n" +
                $"Armor  L{levels.Armor}\n" +
                $"Flux  L{levels.Flux}\n" +
                $"Mobility  L{levels.Mobility}";
            if (!expanded) return text;
            return text + FormattableString.Invariant(
                $"\n\nDamage  {derived.BaseDamage:0.#}\nMax HP  {derived.MaxHp:0.#}\nDefense  {derived.ArmorValue:0.#}\nAttack  {derived.AttackInterval:0.##}s\nSpeed  {derived.MoveSpeed:0.##}");
        }

        public static string OfflineDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1d)
            {
                return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }

            return $"{Math.Max(0, (int)duration.TotalMinutes)}m";
        }
    }
}
