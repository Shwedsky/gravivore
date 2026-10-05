using System;
using Gravivore.Core.Time;

namespace Gravivore.Persistence.Profile
{
    /// <summary>
    /// Shared gameplay clock for persisted cooldown/window decisions.
    /// It accepts forward wall-clock movement but never reports an instant older than the
    /// persisted profile floor, so moving the device clock backwards cannot shorten cooldowns.
    /// </summary>
    public sealed class ProfileEffectiveUtcTimeProvider : ITimeProvider
    {
        private readonly ITimeProvider _source;
        private readonly ProfileRuntimeState _state;

        public ProfileEffectiveUtcTimeProvider(ITimeProvider source, ProfileRuntimeState state)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public DateTime UtcNow => _state.AdvanceEffectiveUtcFloor(_source.UtcNow);
    }
}
