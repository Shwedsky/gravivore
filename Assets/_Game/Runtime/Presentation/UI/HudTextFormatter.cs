using System;
using Gravivore.Gameplay.Player;

namespace Gravivore.Presentation.UI
{
    public static class HudTextFormatter
    {
        public static string Health(float current, float maximum)
        {
            return FormattableString.Invariant(
                $"{RussianUiText.Durability} {Math.Max(0f, current):0} / {Math.Max(0f, maximum):0}");
        }

        public static string Stats(PlayerStatLevels levels, PlayerDerivedStats derived, bool expanded)
        {
            var text =
                $"Мощность  ур. {levels.Power}\n" +
                $"Корпус  ур. {levels.Hull}\n" +
                $"Броня  ур. {levels.Armor}\n" +
                $"Поток  ур. {levels.Flux}\n" +
                $"Манёвренность  ур. {levels.Mobility}";
            if (!expanded) return text;
            return text + FormattableString.Invariant(
                $"\n\nУрон  {derived.BaseDamage:0.#}\nМакс. прочность  {derived.MaxHp:0.#}\nЗащита  {derived.ArmorValue:0.#}\nАтака  {derived.AttackInterval:0.##} с\nСкорость  {derived.MoveSpeed:0.##}");
        }

        public static string OfflineDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1d)
            {
                return $"{(int)duration.TotalHours} ч {duration.Minutes} мин";
            }

            return $"{Math.Max(0, (int)duration.TotalMinutes)} мин";
        }
    }
}
