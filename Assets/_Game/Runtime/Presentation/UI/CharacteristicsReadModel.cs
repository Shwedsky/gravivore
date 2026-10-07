using System;
using System.Globalization;
using System.Text;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;

namespace Gravivore.Presentation.UI
{
    public readonly struct CharacteristicSnapshot
    {
        public CharacteristicSnapshot(PlayerStatType stat, int level, float xp, float required,
            PlayerDerivedStats current, PlayerDerivedStats next, bool maximum)
        { Stat = stat; Level = level; Experience = xp; Required = required; Current = current; Next = next; IsMaximum = maximum; }
        public PlayerStatType Stat { get; }
        public int Level { get; }
        public float Experience { get; }
        public float Required { get; }
        public PlayerDerivedStats Current { get; }
        public PlayerDerivedStats Next { get; }
        public bool IsMaximum { get; }
        public float Fraction => IsMaximum ? 1 : Math.Min(1, Experience / Required);
    }

    /// <summary>Presentation-only projection; formulas, caps, XP and gate authority stay in gameplay.</summary>
    public sealed class CharacteristicsReadModel
    {
        private readonly PlayerStatsState _stats;
        private readonly AssimilationProgressionService _progression;
        private readonly EliteGateRequirement _requirement;
        private readonly QuestState _quests;
        private readonly WorldUnlockState _world;
        private readonly string _admissionName;
        public CharacteristicsReadModel(PlayerStatsState stats, AssimilationProgressionService progression,
            EliteGateRequirement requirement, QuestState quests, WorldUnlockState world, string admissionName = "Магнетару")
        { _stats = stats; _progression = progression; _requirement = requirement; _quests = quests; _world = world; _admissionName = admissionName; }

        public CharacteristicSnapshot Read(PlayerStatType stat)
        {
            var level = _stats.BaseLevels.GetLevel(stat);
            var maximum = level >= _stats.GetMaximumLevel(stat);
            return new CharacteristicSnapshot(stat, level, _progression.State.GetStatExperience(stat),
                maximum ? 0 : _progression.Configuration.ThresholdCurve.Evaluate(level), _stats.DerivedStats,
                maximum ? _stats.DerivedStats : _stats.PreviewLevel(stat, level + 1), maximum);
        }
        public long AssimilationRequirement => _requirement.MinimumAssimilationScore;
        public long AssimilationScore => _progression.State.TotalAssimilationScore;
        public bool AdmissionGranted => _world.EliteGateUnlocked;
        public int CompletedObjectives
        {
            get { var count = 0; for(var i=0;i<_requirement.RequiredObjectiveCount;i++)
                if(_quests.IsObjectiveCompleted(_requirement.GetRequiredObjectiveId(i))) count++; return count; }
        }
        public string AssimilationText => $"АССИМИЛЯЦИЯ   {(AdmissionGranted ? AssimilationRequirement : Math.Min(AssimilationScore, AssimilationRequirement))} / {AssimilationRequirement}\n" +
            (AdmissionGranted ? "ДОПУСК ПОЛУЧЕН" : $"До допуска к {_admissionName} • цели {CompletedObjectives}/{_requirement.RequiredObjectiveCount}");
        public string AssimilationExplanation => $"Общий счёт: {AssimilationScore}. " + AssimilationDescription;
        public const string AssimilationDescription = "Общий прогресс поглощения технологий. Нужен для допуска к усиленным зонам и ключевым противникам. ОП повышают отдельные характеристики; ассимиляция учитывает общий прогресс.";
        public static string Description(PlayerStatType stat)
        {
            switch(stat)
            {
                case PlayerStatType.Power: return "Повышает базовый урон атак Г-0.";
                case PlayerStatType.Hull: return "Повышает максимальную прочность Г-0.";
                case PlayerStatType.Armor: return "Снижает входящий физический урон. После защиты остаётся минимум 1 урона.";
                case PlayerStatType.Flux: return "Сокращает интервал между атаками до установленного предела.";
                case PlayerStatType.Mobility: return "Повышает скорость движения до установленного предела.";
                default: return "Характеристика Г-0.";
            }
        }
        public static float Value(PlayerStatType stat, in PlayerDerivedStats values)
        {
            switch(stat)
            {
                case PlayerStatType.Power: return values.BaseDamage;
                case PlayerStatType.Hull: return values.MaxHp;
                case PlayerStatType.Armor: return values.ArmorValue;
                case PlayerStatType.Flux: return values.AttackInterval;
                case PlayerStatType.Mobility: return values.MoveSpeed;
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
        }
        public static string Effect(PlayerStatType stat, in PlayerDerivedStats values)
        {
            var value = FormatNumber(Value(stat, values));
            switch(stat)
            {
                case PlayerStatType.Power: return "Урон: " + value;
                case PlayerStatType.Hull: return "Прочность: " + value;
                case PlayerStatType.Armor: return "Защита: " + value;
                case PlayerStatType.Flux: return "Интервал атак: " + value + " с";
                case PlayerStatType.Mobility: return "Скорость: " + value + " м/с";
                default: throw new ArgumentOutOfRangeException(nameof(stat));
            }
        }
        public static string FormatNumber(float value)=>value.ToString("0.##",CultureInfo.GetCultureInfo("ru-RU"));
        public string Sources(PlayerStatType stat)
        {
            var text = new StringBuilder("Источники ОП:\n");
            var config = _progression.Configuration;
            for(var i=0;i<config.RewardCount;i++)
            {
                var reward = config.GetReward(i); if(reward.Stat != stat) continue;
                text.Append(EnemyName(reward.EnemyId)).Append(": +")
                    .Append(FormatNumber(reward.StatExperience)).Append(" ОП\n");
            }
            text.Append("Усиленные точки дают награду по своему множителю. Награда показана над противником.");
            return text.ToString();
        }
        private static string EnemyName(string id)
        {
            switch(id)
            {
                case "scout-drone": return "Разведчик";
                case "cutter-unit": return "Резчик";
                case "warden": return "Страж";
                case "arc-drone": return "Дуговой дрон";
                case "carrier": return "Носитель";
                case "magnetar-guard": return "Магнетар";
                case "custodian-m0": return "Кустодиан М-0";
                default: return "Технологическое ядро";
            }
        }
    }
}
