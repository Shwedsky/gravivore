using System;
using Gravivore.Gameplay.Offline;
using UnityEngine;

namespace Gravivore.Persistence.Profile
{
    public readonly struct SaveOfflineConfiguration
    {
        public SaveOfflineConfiguration(
            int currentSchemaVersion,
            float autosaveDelaySeconds,
            OfflineRewardConfiguration offlineReward)
        {
            if (currentSchemaVersion < 1) throw new ArgumentOutOfRangeException(nameof(currentSchemaVersion));
            if (float.IsNaN(autosaveDelaySeconds) || float.IsInfinity(autosaveDelaySeconds) ||
                autosaveDelaySeconds <= 0f || autosaveDelaySeconds > 10f)
            {
                throw new ArgumentOutOfRangeException(nameof(autosaveDelaySeconds));
            }

            CurrentSchemaVersion = currentSchemaVersion;
            AutosaveDelaySeconds = autosaveDelaySeconds;
            OfflineReward = offlineReward;
        }

        public int CurrentSchemaVersion { get; }
        public float AutosaveDelaySeconds { get; }
        public OfflineRewardConfiguration OfflineReward { get; }
    }

    [CreateAssetMenu(fileName = "SaveOffline", menuName = "Gravivore/Persistence/Save and Offline")]
    public sealed class SaveOfflineDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int _currentSchemaVersion = SaveSchema.CurrentVersion;
        [SerializeField, Range(0.1f, 10f)] private float _autosaveDelaySeconds = 2f;
        [SerializeField, Min(0f)] private float _activeBaselineUnitsPerHour = 120f;
        [SerializeField, Range(0.01f, 1f)] private float _offlineEfficiency = 0.25f;
        [SerializeField, Min(0.1f)] private float _offlineCapHours = 2f;

        public SaveOfflineConfiguration Configuration => new SaveOfflineConfiguration(
            _currentSchemaVersion,
            _autosaveDelaySeconds,
            new OfflineRewardConfiguration(
                _activeBaselineUnitsPerHour,
                _offlineEfficiency,
                TimeSpan.FromHours(_offlineCapHours)));

        public void ValidateOrThrow()
        {
            var configuration = Configuration;
            if (configuration.CurrentSchemaVersion != SaveSchema.CurrentVersion)
            {
                throw new InvalidOperationException("Authored save schema must match the compiled current schema.");
            }
        }
    }
}
