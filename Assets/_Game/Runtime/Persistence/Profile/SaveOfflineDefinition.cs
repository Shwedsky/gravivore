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
            OfflineRewardConfiguration offlineReward,
            TimeSpan minimumResumeAbsence = default)
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
            MinimumResumeAbsence = minimumResumeAbsence == default
                ? TimeSpan.FromSeconds(60d)
                : minimumResumeAbsence;
            if (MinimumResumeAbsence < TimeSpan.Zero ||
                MinimumResumeAbsence > offlineReward.MaximumEligibleDuration)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumResumeAbsence));
            }
        }

        public int CurrentSchemaVersion { get; }
        public float AutosaveDelaySeconds { get; }
        public OfflineRewardConfiguration OfflineReward { get; }
        public TimeSpan MinimumResumeAbsence { get; }
    }

    [CreateAssetMenu(fileName = "SaveOffline", menuName = "Gravivore/Persistence/Save and Offline")]
    public sealed class SaveOfflineDefinition : ScriptableObject
    {
        // Retained only for backward compatibility with existing authored assets. Runtime schema
        // currentness belongs to compiled persistence code, so stale serialized values cannot pin
        // a newer build to an older migration target.
        [SerializeField, HideInInspector] private int _currentSchemaVersion = SaveSchema.CurrentVersion;
        [SerializeField, Range(0.1f, 10f)] private float _autosaveDelaySeconds = 2f;
        [SerializeField, Min(0f)] private float _activeBaselineUnitsPerHour = 120f;
        [SerializeField, Range(0.01f, 1f)] private float _offlineEfficiency = 0.25f;
        [SerializeField, Min(0.1f)] private float _offlineCapHours = 2f;
        [SerializeField, Min(1f)] private float _minimumResumeAbsenceSeconds = 60f;

        public SaveOfflineConfiguration Configuration => new SaveOfflineConfiguration(
            SaveSchema.CurrentVersion,
            _autosaveDelaySeconds,
            new OfflineRewardConfiguration(
                _activeBaselineUnitsPerHour,
                _offlineEfficiency,
                TimeSpan.FromHours(_offlineCapHours)),
            TimeSpan.FromSeconds(_minimumResumeAbsenceSeconds > 0f
                ? _minimumResumeAbsenceSeconds
                : 60f));

        public void ValidateOrThrow()
        {
            _ = Configuration;
        }
    }
}
