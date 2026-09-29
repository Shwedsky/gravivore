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

        private ProfileSession(
            IProfileRepository repository,
            ProfileRestoreContext context,
            ITimeProvider time,
            ISaveDiagnostics diagnostics,
            ProfileRuntimeState state,
            OfflineRewardService offlineRewards,
            OfflineReturnSummary returnSummary,
            ProfileLoadResult loadResult)
        {
            _repository = repository;
            _context = context;
            _time = time;
            _diagnostics = diagnostics;
            State = state;
            OfflineRewards = offlineRewards;
            ReturnSummary = returnSummary;
            LoadResult = loadResult;
        }

        public ProfileRuntimeState State { get; }
        public OfflineRewardService OfflineRewards { get; }
        public OfflineReturnSummary ReturnSummary { get; }
        public ProfileLoadResult LoadResult { get; }
        public bool StartupCheckpointSucceeded { get; private set; }

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
            try
            {
                loadResult = repository.LoadOrCreate(freshFactory, validate);
            }
            catch (Exception exception)
            {
                diagnostics.Error("Profile load failed; continuing with a fresh in-memory profile.", exception);
                loadResult = new ProfileLoadResult(freshFactory(), true, false, false);
            }

            var state = ProfileSaveMapper.Restore(loadResult.Save, context);
            var offlineRewards = new OfflineRewardService(configuration.OfflineReward, state.Offline);
            OfflineReturnSummary summary;
            if (loadResult.WasCreated)
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
                loadResult);
            session.StartupCheckpointSucceeded = session.FlushNow();
            return session;
        }

        public bool FlushNow()
        {
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
    }
}
