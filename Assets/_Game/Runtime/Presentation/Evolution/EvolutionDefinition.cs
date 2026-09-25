using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using UnityEngine;

namespace Gravivore.Presentation.Evolution
{
    public sealed class EvolutionVisualCatalog
    {
        public EvolutionVisualCatalog(
            EvolutionConfiguration selection,
            EvolutionTierModuleSet[] tierModuleSets,
            EvolutionAccentModule[] accentModules)
        {
            Selection = selection ?? throw new ArgumentNullException(nameof(selection));
            TierModuleSets = tierModuleSets ?? throw new ArgumentNullException(nameof(tierModuleSets));
            AccentModules = accentModules ?? throw new ArgumentNullException(nameof(accentModules));
            EvolutionDefinition.ValidateCatalog(this);
        }

        public EvolutionConfiguration Selection { get; }
        public EvolutionTierModuleSet[] TierModuleSets { get; }
        public EvolutionAccentModule[] AccentModules { get; }
    }

    [CreateAssetMenu(fileName = "EvolutionDefinition", menuName = "Gravivore/Progression/Evolution")]
    public sealed class EvolutionDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private long _tier1Threshold = 10;
        [SerializeField, Min(2)] private long _tier2Threshold = 30;
        [SerializeField] private PlayerStatType[] _dominancePriority =
        {
            PlayerStatType.Power,
            PlayerStatType.Hull,
            PlayerStatType.Armor,
            PlayerStatType.Flux,
            PlayerStatType.Mobility
        };
        [SerializeField] private EvolutionTierModuleSet[] _tierModuleSets = Array.Empty<EvolutionTierModuleSet>();
        [SerializeField] private EvolutionAccentModule[] _accentModules = Array.Empty<EvolutionAccentModule>();

        public EvolutionVisualCatalog Catalog => new EvolutionVisualCatalog(
            new EvolutionConfiguration(_tier1Threshold, _tier2Threshold, _dominancePriority),
            _tierModuleSets,
            _accentModules);

        public void ValidateOrThrow()
        {
            _ = Catalog;
        }

        internal static void ValidateCatalog(EvolutionVisualCatalog catalog)
        {
            const int tierCount = 3;
            const int statCount = 5;
            var seenTiers = new bool[tierCount];
            var seenStats = new bool[statCount];
            var seenSockets = new bool[4];
            var moduleIds = new HashSet<string>(StringComparer.Ordinal);

            if (catalog.TierModuleSets.Length != tierCount)
            {
                throw new InvalidOperationException("Evolution requires exactly Tier0, Tier1, and Tier2 module sets.");
            }

            for (var i = 0; i < catalog.TierModuleSets.Length; i++)
            {
                var set = catalog.TierModuleSets[i] ??
                          throw new InvalidOperationException($"Evolution tier module set {i} is not assigned.");
                var tierIndex = (int)set.Tier;
                if (tierIndex < 0 || tierIndex >= tierCount || seenTiers[tierIndex])
                {
                    throw new InvalidOperationException("Evolution tier module sets must contain each tier exactly once.");
                }

                seenTiers[tierIndex] = true;
                if (set.Modules == null || (set.Tier != EvolutionTier.Tier0 && set.Modules.Length == 0))
                {
                    throw new InvalidOperationException($"{set.Tier} requires configured silhouette modules.");
                }

                for (var moduleIndex = 0; moduleIndex < set.Modules.Length; moduleIndex++)
                {
                    ValidateModule(set.Modules[moduleIndex], moduleIds, seenSockets);
                }
            }

            if (catalog.AccentModules.Length != statCount)
            {
                throw new InvalidOperationException("Evolution requires one dominant accent for each player stat.");
            }

            for (var i = 0; i < catalog.AccentModules.Length; i++)
            {
                var accent = catalog.AccentModules[i] ??
                             throw new InvalidOperationException($"Evolution accent {i} is not assigned.");
                var statIndex = (int)accent.Stat;
                if (statIndex < 0 || statIndex >= statCount || seenStats[statIndex])
                {
                    throw new InvalidOperationException("Evolution requires one unique accent for each player stat.");
                }

                seenStats[statIndex] = true;
                ValidateModule(accent.Module, moduleIds, seenSockets);
            }

            for (var i = 0; i < seenSockets.Length; i++)
            {
                if (!seenSockets[i])
                {
                    throw new InvalidOperationException($"Evolution socket {(EvolutionSocketId)i} has no configured module.");
                }
            }
        }

        private static void ValidateModule(
            EvolutionModuleConfiguration module,
            HashSet<string> moduleIds,
            bool[] seenSockets)
        {
            if (module == null)
            {
                throw new InvalidOperationException("Evolution module is not assigned.");
            }

            module.ValidateOrThrow();
            if (!moduleIds.Add(module.Id))
            {
                throw new InvalidOperationException($"Duplicate evolution module id: {module.Id}.");
            }

            seenSockets[(int)module.Socket] = true;
        }
    }
}
