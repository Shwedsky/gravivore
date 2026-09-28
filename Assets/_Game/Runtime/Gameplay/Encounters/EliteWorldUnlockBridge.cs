using System;
using Gravivore.Gameplay.World;

namespace Gravivore.Gameplay.Encounters
{
    public sealed class EliteWorldUnlockBridge : IDisposable
    {
        private readonly IMagnetarGuardDefeatSource _elite;
        private readonly IEliteDefeatRecorder _worldUnlocks;
        private bool _handled;
        private bool _disposed;

        public EliteWorldUnlockBridge(IMagnetarGuardDefeatSource elite, IEliteDefeatRecorder worldUnlocks)
        {
            _elite = elite != null ? elite : throw new ArgumentNullException(nameof(elite));
            _worldUnlocks = worldUnlocks ?? throw new ArgumentNullException(nameof(worldUnlocks));
            _elite.Defeated += HandleDefeated;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _elite.Defeated -= HandleDefeated;
        }

        private void HandleDefeated(MagnetarGuardDefeatedEvent defeated)
        {
            if (_handled) return;
            _handled = true;
            _worldUnlocks.RecordEliteDefeated(defeated.EliteId);
        }
    }
}
