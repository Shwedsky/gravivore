using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Presentation.UI
{
    public static class RussianUiText
    {
        public const string CyrillicValidationSample = "ПРОЧНОСТЬ МАНЁВРЕННОСТЬ";
        public const string Durability = "ПРОЧНОСТЬ";
        public const string BossName = "ХРАНИТЕЛЬ M-0";
        public const string Paused = "ПАУЗА";
        public const string Resume = "ПРОДОЛЖИТЬ";
        public const string WelcomeBack = "С ВОЗВРАЩЕНИЕМ";
        public const string Claim = "ЗАБРАТЬ";
        public const string ChapterComplete = "ГЛАВА ЗАВЕРШЕНА";
        public const string BossDefeated = "Хранитель M-0 уничтожен";
        public const string ContinueExploring = "ПРОДОЛЖИТЬ ИГРУ";
        public const string PrimarySequenceComplete = "ОСНОВНАЯ ЦЕПОЧКА ЗАВЕРШЕНА";

        public static string StatName(PlayerStatType stat)
        {
            switch (stat)
            {
                case PlayerStatType.Power: return "Мощность";
                case PlayerStatType.Hull: return "Корпус";
                case PlayerStatType.Armor: return "Броня";
                case PlayerStatType.Flux: return "Поток";
                case PlayerStatType.Mobility: return "Манёвренность";
                default: return stat.ToString();
            }
        }

        public static string StatIncreased(PlayerStatType stat, int level) =>
            $"{StatName(stat)}: уровень {level}";

        public static string StatsDetails(bool expanded) =>
            $"ХАРАКТЕРИСТИКИ: {(expanded ? "ПОДРОБНО" : "КРАТКО")}";

        public static string Audio(bool muted, int volumePercent) =>
            muted ? "ЗВУК: ВЫКЛ" : $"ЗВУК: {volumePercent}%";

        public static string Haptics(bool enabled) => enabled ? "ВИБРАЦИЯ: ВКЛ" : "ВИБРАЦИЯ: ВЫКЛ";
        public static string Away(string duration) => $"ВНЕ ИГРЫ {duration}";
        public static string Recovered(long amount) => $"ПОЛУЧЕНО: {amount} материала";
        public static string StoredMaterial(long amount) => $"ЗАПАС МАТЕРИАЛА: {amount}";
        public static string Assimilation(long current, long required) => $"АССИМИЛЯЦИЯ {current}/{required}";
        public static string IntroProgress(int current, int required) => $"Вводные цели {current}/{required}";
        public static string AssimilationReward(float experience, PlayerStatType stat) =>
            $"+{experience:0.#} {StatName(stat)}";

        public static bool FontSupportsCyrillic(Font font)
        {
            if (font == null) return false;
            for (var i = 0; i < CyrillicValidationSample.Length; i++)
            {
                var character = CyrillicValidationSample[i];
                if (character != ' ' && !font.HasCharacter(character)) return false;
            }

            return true;
        }
    }
}
