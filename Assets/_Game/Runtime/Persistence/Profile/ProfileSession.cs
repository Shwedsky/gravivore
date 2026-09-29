using System;
using Gravivore.Core.Time;
using Gravivore.Gameplay.Offline;

namespace Gravivore.Persistence.Profile
{
    public sealed class ProfileSession
    {
        private readonly IProfileRepository _repository;
        private readonly ProfileRestoreContext _context;
        private readonly ITimeProvider _time;
        private readonly ISaveDiagnostics _diagnostics;
        private readonly TimeSpan _minimumResumeAbsence;

        private ProfileSession(
            IProfileRepository repository,
            ProfileRestoreContext context,
            ITimeProvider time,
            ISaveDiagnostics diagnostics,
            ProfileRuntimeState state,
            OfflineRewardService offlineRewards,
            OfflineReturnSummary returnSummary,
            ProfileLoadResult loadResult,
            bool persistenceSuspended,
            TimeSpan minimumResumeAbsence)
        {
            _repository = repository;
            _context = context;
            _time = time;
            _diagnostics = diagnostics;
            State = state;
            OfflineRewards = offlineRewards;
            ReturnSummary = returnSummary;
            LoadResult = loadResult;
            PersistenceSuspended = persistenceSuspended;
            _minimumResumeAbsence = minimumResumeAbsence;
        }

        public ProfileRuntimeState State { get; }
        public OfflineRewardService OfflineRewards { get; }
        public OfflineReturnSummary ReturnSummary { get; }
        public ProfileLoadResult LoadResult { get; }
        public bool StartupCheckpointSucceeded { get; private set; }
        public bool PersistenceSuspended { get; }

        public static ProfileSession Start(
            IProfileRepository repository,
            ProfileRestoreContext context,
            SaveOfflineConfiguration configuration,
            ITimeProvider time,
            ISaveDiagnostics diagnostics)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (time == null) throw new ArgumentNullException(nameof(time));
            if (diagnostics == null) throw new ArgumentNullException(nameof(diagnostics));
            var now = time.UtcNow;
            Func<SaveRootDto> freshFactory = () =>
                ProfileSaveMapper.ToDto(ProfileSaveMapper.CreateFresh(context, now), context);
            Action<SaveRootDto> validate = dto =>
            {
                _ = ProfileSaveMapper.Restore(dto, context);
            };

            ProfileLoadResult loadResult;
            var persistenceSuspended = false;
            try
            {
                loadResult = repository.LoadOrCreate(freshFactory, validate);
            }
            catch (Exception exception) when (IsStorageFailure(exception))
            {
                diagnostics.Error(
                    "Profile storage failed during load or recovery. Persistence is suspended for this session; continuing with a fresh in-memory profile without writing main or backup.",
                    exception);
                loadResult = new ProfileLoadResult(freshFactory(), true, false, false);
                persistenceSuspended = true;
            }

            var state = ProfileSaveMapper.Restore(loadResult.Save, context);
            var offlineRewards = new OfflineRewardService(configuration.OfflineReward, state.Offline);
            OfflineReturnSummary summary;
            if (loadResult.WasCreated || !OfflineRewardEligibility.IsUnlocked(state.Quests))
            {
                summary = new OfflineReturnSummary(TimeSpan.Zero, TimeSpan.Zero, 0, state.Offline.PendingReward, false, OfflineClockAnomaly.None);
            }
            else
            {
                summary = offlineRewards.Accrue(state.LastSeenUtc, now);
                if (summary.WasCapped) diagnostics.Warning("Offline elapsed time exceeded the configured cap and was clamped.");
                if (summary.ClockAnomaly != OfflineClockAnomaly.None) diagnostics.Warning("Offline clock delta was non-positive; no reward was granted.");
            }

            state.SetLastSeenUtc(now);
            var session = new ProfileSession(
                repository,
                context,
                time,
                diagnostics,
                state,
                offlineRewards,
                summary,
                loadResult,
                persistenceSuspended,
                configuration.MinimumResumeAbsence);
            if (!persistenceSuspended)
            {
                session.StartupCheckpointSucceeded = session.FlushNow();
            }
            return session;
        }

        public OfflineReturnSummary ProcessResume()
        {
            var now = _time.UtcNow;
            var elapsed = now - State.LastSeenUtc;
            OfflineReturnSummary summary;
            if (elapsed <= TimeSpan.Zero)
            {
                summary = new OfflineReturnSummary(
                    elapsed,
                    TimeSpan.Zero,
                    0,
                    State.Offline.PendingReward,
                    false,
                    OfflineClockAnomaly.NonPositiveElapsed);
                _diagnostics.Warning("Offline resume clock delta was non-positive; no reward was granted.");
            }
            else if (elapsed < _minimumResumeAbsence || !OfflineRewardEligibility.IsUnlocked(State.Quests))
            {
                summary = new OfflineReturnSummary(
                    elapsed,
                    TimeSpan.Zero,
                    0,
                    State.Offline.PendingReward,
                    false,
                    OfflineClockAnomaly.None);
            }
            else
            {
                summary = OfflineRewards.Accrue(State.LastSeenUtc, now);
                if (summary.WasCapped) _diagnostics.Warning("Offline resume elapsed time exceeded the configured cap and was clamped.");
            }

            State.SetLastSeenUtc(now);
            return summary;
        }

        public bool FlushNow()
        {
            if (PersistenceSuspended) return false;
            State.SetLastSeenUtc(_time.UtcNow);
            Func<SaveRootDto> freshFactory = () =>
                ProfileSaveMapper.ToDto(ProfileSaveMapper.CreateFresh(_context, State.LastSeenUtc), _context);
            try
            {
                _repository.Save(
                    ProfileSaveMapper.ToDto(State, _context),
                    freshFactory,
                    dto =>
                    {
                        _ = ProfileSaveMapper.Restore(dto, _context);
                    });
                return true;
            }
            catch (Exception exception)
            {
                _diagnostics.Error("Profile save failed; committed gameplay state remains active and will be retried.", exception);
                return false;
            }
        }

        private static bool IsStorageFailure(Exception exception)
        {
            return exception is System.IO.IOException ||
                   exception is UnauthorizedAccessException ||
                   exception is System.Security.SecurityException;
        }
    }
}
