using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Persistence.Profile
{
    /// <summary>
    /// Owns the durable repeat-reward sequence:
    /// prepare state -> save -> apply reward -> save applied phase -> clear -> save.
    /// A Prepared transaction is safe to re-apply only after reload because its durable profile
    /// still contains the pre-reward progression snapshot. An Applied transaction is never applied again.
    /// </summary>
    public sealed class RepeatableRewardTransactionCoordinator
    {
        private readonly ProfileSession _session;
        private readonly RepeatableEncounterService _encounters;
        private readonly AuthoredRewardApplier _rewardApplier;

        public RepeatableRewardTransactionCoordinator(
            ProfileSession session,
            AuthoredRewardApplier rewardApplier)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _rewardApplier = rewardApplier ?? throw new ArgumentNullException(nameof(rewardApplier));
            _encounters = new RepeatableEncounterService(session.State.Repeatable, session.EffectiveTime);
        }

        public RepeatableEncounterService Encounters => _encounters;

        public bool PrepareAndCommit(
            RepeatableEncounterKind encounterKind,
            bool firstClearCompletedBeforeKill,
            CoreReward reward)
        {
            if (_session.State.Repeatable.PendingReward != null)
            {
                throw new InvalidOperationException("Resolve the pending repeat reward before starting another transaction.");
            }

            _encounters.PrepareDefeat(encounterKind, firstClearCompletedBeforeKill, reward);
            if (!_session.FlushNow())
            {
                // Nothing has been granted yet. Keep the Prepared transaction in memory so a later
                // save/recovery can continue without silently losing the kill entitlement.
                return false;
            }

            return RecoverPending();
        }

        public bool RecoverPending()
        {
            var pending = _session.State.Repeatable.PendingReward;
            if (pending == null) return true;

            if (pending.Phase == PendingRewardPhase.Prepared)
            {
                _rewardApplier.Apply(pending.Reward);
                pending.MarkApplied();
                if (!_session.FlushNow())
                {
                    // In-memory progression and phase are Applied. Disk remains Prepared with the
                    // pre-reward progression snapshot; a crash therefore reloads a state where a
                    // single application is still required, while an in-session retry cannot double grant.
                    return false;
                }
            }

            if (pending.Phase != PendingRewardPhase.Applied)
            {
                throw new InvalidOperationException("Unknown pending reward phase.");
            }

            _session.State.Repeatable.ClearPending(pending.TransactionId);
            return _session.FlushNow();
        }
    }
}
