using System;
using UnityEngine;

namespace Gravivore.Presentation.AudioVfx
{
    [CreateAssetMenu(menuName = "Gravivore/Presentation/Phase 6B Production Definition", fileName = "Phase6B_Production")]
    public sealed class Phase6BProductionDefinition : ScriptableObject
    {
        private const string DefinitionPath = "Assets/_Game/Content/Resources/Phase6B/Phase6B_Production.asset";

        [SerializeField] private Phase6BAudioBank _audioBank;
        [SerializeField] private Phase6BVfxInstance _playerCharge;
        [SerializeField] private Phase6BVfxInstance _playerReleaseFlash;
        [SerializeField] private Phase6BVfxInstance _gravityLashTravel;
        [SerializeField] private Phase6BVfxInstance _playerImpact;
        [SerializeField] private Phase6BVfxInstance _hostileImpact;
        [SerializeField] private Phase6BVfxInstance _mechanicalKillBurst;
        [SerializeField] private Phase6BVfxInstance _hostileTelegraphBase;
        [SerializeField] private Phase6BVfxInstance _bossConeTelegraph;
        [SerializeField] private Phase6BVfxInstance _bossLineTelegraph;
        [SerializeField] private Phase6BVfxInstance _bossCircleTelegraph;
        [SerializeField] private Phase6BVfxInstance _repairBeam;
        [SerializeField] private Phase6BVfxInstance _weldingSparks;
        [SerializeField] private Phase6BVfxInstance _scannerSweep;
        [SerializeField, Min(0.05f)] private float _repairPresentationInterval = 0.35f;

        public Phase6BAudioBank AudioBank => _audioBank;
        public float RepairPresentationInterval => _repairPresentationInterval;

        public Phase6BVfxPool.Binding[] CreateCombatBindings()
        {
            var bindings = new System.Collections.Generic.List<Phase6BVfxPool.Binding>(CreateCheckpoint2Bindings());
            bindings.Add(new Phase6BVfxPool.Binding(Phase6BVfxCue.HostileTelegraphBase, _hostileTelegraphBase, 1));
            bindings.Add(new Phase6BVfxPool.Binding(Phase6BVfxCue.BossConeTelegraph, _bossConeTelegraph, 1));
            bindings.Add(new Phase6BVfxPool.Binding(Phase6BVfxCue.BossLineTelegraph, _bossLineTelegraph, 1));
            bindings.Add(new Phase6BVfxPool.Binding(Phase6BVfxCue.BossCircleTelegraph, _bossCircleTelegraph, 1));
            return bindings.ToArray();
        }

        public Phase6BVfxPool.Binding[] CreateRepairBindings() => new[]
        {
            new Phase6BVfxPool.Binding(Phase6BVfxCue.RepairBeam, _repairBeam, 1),
            new Phase6BVfxPool.Binding(Phase6BVfxCue.WeldingSparks, _weldingSparks, 1),
            new Phase6BVfxPool.Binding(Phase6BVfxCue.ScannerSweep, _scannerSweep, 1)
        };

        public static Phase6BProductionDefinition LoadRequired()
        {
            // Editor-only compatibility for isolated presentation fixtures. Production injects
            // the serialized asset from its composition root on every platform.
#if UNITY_EDITOR
            var definition = UnityEditor.AssetDatabase.LoadAssetAtPath<Phase6BProductionDefinition>(DefinitionPath);
            if (definition == null)
                throw new InvalidOperationException($"Missing Phase6B production definition at {DefinitionPath}.");
            definition.ValidateOrThrow();
            return definition;
#else
            throw new InvalidOperationException("Inject the Phase6B production definition from composition.");
#endif
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
            ValidatePrefab(_hostileTelegraphBase, Phase6BVfxCue.HostileTelegraphBase);
            ValidatePrefab(_bossConeTelegraph, Phase6BVfxCue.BossConeTelegraph);
            ValidatePrefab(_bossLineTelegraph, Phase6BVfxCue.BossLineTelegraph);
            ValidatePrefab(_bossCircleTelegraph, Phase6BVfxCue.BossCircleTelegraph);
            ValidatePrefab(_repairBeam, Phase6BVfxCue.RepairBeam);
            ValidatePrefab(_weldingSparks, Phase6BVfxCue.WeldingSparks);
            ValidatePrefab(_scannerSweep, Phase6BVfxCue.ScannerSweep);
            if (_repairPresentationInterval <= 0f) throw new InvalidOperationException("Repair presentation interval must be positive.");
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
