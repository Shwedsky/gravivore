using Gravivore.Gameplay.Player;
using UnityEngine;

namespace Gravivore.Presentation.UI
{
    public static class RussianUiText
    {
        public const string CyrillicValidationSample = "ПРОЧНОСТЬ МАНЁВРЕННОСТЬ";
        public const string Durability = "ПРОЧНОСТЬ";
        public const string BossName = "ХРАНИТЕЛЬ М-0";
        public const string Paused = "ПАУЗА";
        public const string GameMenu = "МЕНЮ";
        public const string Characteristics = "ХАРАКТЕРИСТИКИ";
        public const string Equipment = "ЭКИПИРОВКА";
        public const string Settings = "НАСТРОЙКИ";
        public const string EmptyEquipment = "не установлено";
        public const string Resume = "ПРОДОЛЖИТЬ";
        public const string WelcomeBack = "С ВОЗВРАЩЕНИЕМ";
        public const string Claim = "ЗАБРАТЬ";
        public const string ChapterComplete = "ГЛАВА ЗАВЕРШЕНА";
        public const string BossDefeated = "Хранитель М-0 уничтожен";
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
                default: return "Характеристика";
            }
        }

        public static string StatIncreased(PlayerStatType stat, int level) =>
            $"{StatName(stat)}: уровень {level}";

        public static string StatsDetails(bool expanded) =>
            $"ХАРАКТЕРИСТИКИ: {(expanded ? "ПОДРОБНО" : "КРАТКО")}";

        public static string Audio(bool muted, int volumePercent) =>
            muted ? "ЗВУК: ВЫКЛ" : $"ЗВУК: {volumePercent}%";

        public static string Haptics(bool enabled) => enabled ? "ВИБРАЦИЯ: ВКЛ" : "ВИБРАЦИЯ: ВЫКЛ";
        public static string EquipmentSlotName(Gravivore.Gameplay.Equipment.EquipmentSlot slot)
        {
            switch (slot)
            {
                case Gravivore.Gameplay.Equipment.EquipmentSlot.Core: return "Ядро";
                case Gravivore.Gameplay.Equipment.EquipmentSlot.Chassis: return "Корпус";
                case Gravivore.Gameplay.Equipment.EquipmentSlot.Module: return "Модуль";
                default: return "Ячейка";
            }
        }
        public static string Away(string duration) => $"ВНЕ ИГРЫ {duration}";
        public static string Recovered(long amount) => $"ПОЛУЧЕНО: {amount} материала";
        public static string StoredMaterial(long amount) => $"ЗАПАС МАТЕРИАЛА: {amount}";
        public static string Assimilation(long current, long required) => $"АССИМИЛЯЦИЯ {current}/{required}";
        public static string IntroProgress(int current, int required) => $"Вводные цели {current}/{required}";
        public static string AssimilationReward(float experience, PlayerStatType stat) =>
            $"+{experience:0.#} {StatName(stat)}";

        public static string SpotName(string id)
        {
            switch (id)
            {
                case "relay-yard": return "Релейный двор";
                case "cutting-floor": return "Разделочный цех";
                case "shield-dump": return "Свалка щитов";
                case "capacitor-field": return "Поле конденсаторов";
                case "hauler-graveyard": return "Кладбище тягачей";
                default: return "Зона";
            }
        }

        public static string BossState(Gravivore.Gameplay.Encounters.CustodianBossState state)
        {
            switch (state)
            {
                case Gravivore.Gameplay.Encounters.CustodianBossState.Dormant: return "ожидание";
                case Gravivore.Gameplay.Encounters.CustodianBossState.Engaging: return "бой";
                case Gravivore.Gameplay.Encounters.CustodianBossState.Telegraphing: return "подготовка атаки";
                case Gravivore.Gameplay.Encounters.CustodianBossState.ExecutingAttack: return "атака";
                case Gravivore.Gameplay.Encounters.CustodianBossState.Recovery: return "восстановление";
                case Gravivore.Gameplay.Encounters.CustodianBossState.Resetting: return "возврат";
                default: return "уничтожен";
            }
        }

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
