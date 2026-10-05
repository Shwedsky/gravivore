using System;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Progression;

namespace Gravivore.Persistence.Profile
{
    /// <summary>
    /// Owns the durable repeat-reward sequence:
    /// prepare state -> durable save -> apply reward -> durable applied save -> clear -> durable save.
    /// A Prepared transaction on disk always corresponds to the pre-reward progression snapshot.
    /// An Applied transaction is never applied again.
    /// </summary>
    public sealed class RepeatableRewardTransactionCoordinator
    {
        private readonly ProfileSession _session;
        private readonly RepeatableEncounterService _encounters;
        private readonly AuthoredRewardApplier _rewardApplier;
        private readonly Action<PendingEncounterReward> _completeFirstClear;

        public RepeatableRewardTransactionCoordinator(
            ProfileSession session,
            AuthoredRewardApplier rewardApplier,
            Action<PendingEncounterReward> completeFirstClear = null)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _rewardApplier = rewardApplier ?? throw new ArgumentNullException(nameof(rewardApplier));
            _completeFirstClear = completeFirstClear;
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
                // No reward has been granted. The transaction remains Prepared in memory and a retry
                // must establish the durable Prepared checkpoint before applying it.
                return false;
            }

            return RecoverPending(preparedCheckpointAlreadyDurable: true);
        }

        public bool RecoverPending() => RecoverPending(preparedCheckpointAlreadyDurable: false);

        private bool RecoverPending(bool preparedCheckpointAlreadyDurable)
        {
            var pending = _session.State.Repeatable.PendingReward;
            if (pending == null) return true;
            var appliedCheckpointDurable = false;

            if (pending.Phase == PendingRewardPhase.Prepared)
            {
                // On startup this is intentionally conservative: re-saving Prepared is harmless and
                // proves durable state is still available before reward application. In-session callers
                // that just completed the prepare flush can skip the redundant write.
                if (!preparedCheckpointAlreadyDurable && !_session.FlushNow()) return false;

                _rewardApplier.Apply(pending.Reward);
                pending.MarkApplied();
                _completeFirstClear?.Invoke(pending);
                if (!_session.FlushNow())
                {
                    // Memory is Applied. If this process crashes, disk is still Prepared with the
                    // pre-reward progression snapshot, so recovery applies exactly once. If it does
                    // not crash, an in-session retry sees Applied and will not grant again.
                    return false;
                }
                appliedCheckpointDurable = true;
            }

            if (pending.Phase != PendingRewardPhase.Applied)
            {
                throw new InvalidOperationException("Unknown pending reward phase.");
            }

            // Applied may be in memory after a failed Applied save. Establish that checkpoint
            // before clearing, including when recovering a previous in-session failure.
            if (!appliedCheckpointDurable)
            {
                _completeFirstClear?.Invoke(pending);
                if (!_session.FlushNow()) return false;
            }
            _session.State.Repeatable.ClearPending(pending.TransactionId);
            if (_session.FlushNow()) return true;
            // Keep Applied in memory so a failed final checkpoint remains retryable.
            _session.State.Repeatable.SetPending(pending);
            return false;
        }
    }
}
