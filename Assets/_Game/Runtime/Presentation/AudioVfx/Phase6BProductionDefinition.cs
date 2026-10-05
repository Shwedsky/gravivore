using System;
using UnityEngine;

namespace Gravivore.Presentation.AudioVfx
{
    [CreateAssetMenu(menuName = "Gravivore/Presentation/Phase 6B Production Definition", fileName = "Phase6B_Production")]
    public sealed class Phase6BProductionDefinition : ScriptableObject
    {
        private const string ResourcesPath = "Phase6B/Phase6B_Production";

        [SerializeField] private Phase6BAudioBank _audioBank;
        [SerializeField] private Phase6BVfxInstance _playerCharge;
        [SerializeField] private Phase6BVfxInstance _playerReleaseFlash;
        [SerializeField] private Phase6BVfxInstance _gravityLashTravel;
        [SerializeField] private Phase6BVfxInstance _playerImpact;
        [SerializeField] private Phase6BVfxInstance _hostileImpact;
        [SerializeField] private Phase6BVfxInstance _mechanicalKillBurst;

        public Phase6BAudioBank AudioBank => _audioBank;

        public static Phase6BProductionDefinition LoadRequired()
        {
            var definition = Resources.Load<Phase6BProductionDefinition>(ResourcesPath);
            if (definition == null)
                throw new InvalidOperationException($"Missing Phase6B production definition at Resources/{ResourcesPath}.");
            definition.ValidateOrThrow();
            return definition;
        }

        public Phase6BVfxPool.Binding[] CreateCheckpoint2Bindings()
        {
            ValidateOrThrow();
            return new[]
            {
                new Phase6BVfxPool.Binding(Phase6BVfxCue.PlayerCharge, _playerCharge, 2),
                new Phase6BVfxPool.Binding(Phase6BVfxCue.PlayerReleaseFlash, _playerReleaseFlash, 3),
                new Phase6BVfxPool.Binding(Phase6BVfxCue.GravityLashTravel, _gravityLashTravel, 4),
                new Phase6BVfxPool.Binding(Phase6BVfxCue.PlayerImpact, _playerImpact, 4),
                new Phase6BVfxPool.Binding(Phase6BVfxCue.HostileImpact, _hostileImpact, 6),
                new Phase6BVfxPool.Binding(Phase6BVfxCue.MechanicalKillBurst, _mechanicalKillBurst, 4)
            };
        }

        public void ValidateOrThrow()
        {
            if (_audioBank == null) throw new InvalidOperationException("Phase6B production audio bank is missing.");
            _audioBank.ValidateOrThrow();
            ValidatePrefab(_playerCharge, Phase6BVfxCue.PlayerCharge);
            ValidatePrefab(_playerReleaseFlash, Phase6BVfxCue.PlayerReleaseFlash);
            ValidatePrefab(_gravityLashTravel, Phase6BVfxCue.GravityLashTravel);
            ValidatePrefab(_playerImpact, Phase6BVfxCue.PlayerImpact);
            ValidatePrefab(_hostileImpact, Phase6BVfxCue.HostileImpact);
            ValidatePrefab(_mechanicalKillBurst, Phase6BVfxCue.MechanicalKillBurst);
        }

        private static void ValidatePrefab(Phase6BVfxInstance prefab, Phase6BVfxCue expected)
        {
            if (prefab == null) throw new InvalidOperationException($"Phase6B production VFX {expected} is missing.");
            if (prefab.Cue != expected)
                throw new InvalidOperationException($"Phase6B production VFX expected {expected} but prefab exposes {prefab.Cue}.");
            prefab.ValidateOrThrow();
        }
    }
}
