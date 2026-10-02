using System;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.World;

namespace Gravivore.Editor.Balance
{
    public readonly struct S20BalanceEstimate
    {
        public S20BalanceEstimate(
            int killsToFirstStatLevel,
            long tier1Assimilation,
            long tier2Assimilation,
            int minimumKillsForElite,
            double estimatedBossReadyMinutes,
            double estimatedUpperBossReadyMinutes)
        {
            KillsToFirstStatLevel = killsToFirstStatLevel;
            Tier1Assimilation = tier1Assimilation;
            Tier2Assimilation = tier2Assimilation;
            MinimumKillsForElite = minimumKillsForElite;
            EstimatedBossReadyMinutes = estimatedBossReadyMinutes;
            EstimatedUpperBossReadyMinutes = estimatedUpperBossReadyMinutes;
        }

        public int KillsToFirstStatLevel { get; }
        public long Tier1Assimilation { get; }
        public long Tier2Assimilation { get; }
        public int MinimumKillsForElite { get; }
        public double EstimatedBossReadyMinutes { get; }
        public double EstimatedUpperBossReadyMinutes { get; }
    }

    public static class S20BalanceModel
    {
        private static readonly string[] OrdinaryEnemyIds =
        {
            "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier"
        };

        public static S20BalanceEstimate Evaluate(
            ProgressionConfiguration progression,
            EvolutionConfiguration evolution,
            EliteGateRequirement elite,
            double ordinaryKillCycleSeconds = 24d,
            double encounterAndTravelMinutes = 10d)
        {
            if (progression == null) throw new ArgumentNullException(nameof(progression));
            if (evolution == null) throw new ArgumentNullException(nameof(evolution));
            if (elite == null) throw new ArgumentNullException(nameof(elite));
            if (ordinaryKillCycleSeconds <= 0d || encounterAndTravelMinutes < 0d)
                throw new ArgumentOutOfRangeException(nameof(ordinaryKillCycleSeconds));

            var minimumExperience = float.MaxValue;
            var minimumAssimilation = long.MaxValue;
            for (var i = 0; i < OrdinaryEnemyIds.Length; i++)
            {
                if (!progression.TryGetReward(OrdinaryEnemyIds[i], out var reward))
                    throw new InvalidOperationException($"Missing reward route: {OrdinaryEnemyIds[i]}.");
                minimumExperience = Math.Min(minimumExperience, reward.StatExperience);
                minimumAssimilation = Math.Min(minimumAssimilation, reward.AssimilationScore);
            }

            var firstLevelKills = (int)Math.Ceiling(
                progression.ThresholdCurve.Evaluate(1) / minimumExperience);
            var eliteKills = (int)Math.Ceiling(
                elite.MinimumAssimilationScore / (double)minimumAssimilation);
            var readyMinutes = eliteKills * ordinaryKillCycleSeconds / 60d + encounterAndTravelMinutes;
            var upperMinutes = (eliteKills + 20) * ordinaryKillCycleSeconds / 60d + encounterAndTravelMinutes;
            return new S20BalanceEstimate(
                firstLevelKills,
                evolution.Tier1Threshold,
                evolution.Tier2Threshold,
                eliteKills,
                readyMinutes,
                upperMinutes);
        }
    }
}
