using System;
using Gravivore.Gameplay.World;

namespace Gravivore.Gameplay.Encounters
{
    public sealed class EliteEncounterActivationBridge : IDisposable
    {
        private readonly WorldUnlockState _worldState;
        private readonly IMagnetarGuardActivationTarget _elite;
        private bool _disposed;

        public EliteEncounterActivationBridge(
            WorldUnlockState worldState,
            IMagnetarGuardActivationTarget elite)
        {
            _worldState = worldState ?? throw new ArgumentNullException(nameof(worldState));
            _elite = elite ?? throw new ArgumentNullException(nameof(elite));
            _worldState.GateUnlocked += HandleGateUnlocked;
            Synchronize();
        }

        public bool Synchronize()
        {
            return !_disposed && _worldState.EliteGateUnlocked && _elite.ActivateEncounter();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _worldState.GateUnlocked -= HandleGateUnlocked;
        }

        private void HandleGateUnlocked(WorldGateUnlockedEvent unlocked) => Synchronize();
    }
}
