using System.Threading;
using System.Threading.Tasks;

namespace Gravivore.Platform.Monetization
{
    public enum RewardedAdStatus
    {
        Unavailable = 0,
        Completed = 1,
        Cancelled = 2,
        Failed = 3
    }

    public readonly struct RewardedAdResult
    {
        private RewardedAdResult(RewardedAdStatus status)
        {
            Status = status;
        }

        public RewardedAdStatus Status { get; }
        public bool RewardEarned => Status == RewardedAdStatus.Completed;

        public static RewardedAdResult Completed() => new RewardedAdResult(RewardedAdStatus.Completed);
        public static RewardedAdResult Cancelled() => new RewardedAdResult(RewardedAdStatus.Cancelled);
        public static RewardedAdResult Unavailable() => new RewardedAdResult(RewardedAdStatus.Unavailable);
        public static RewardedAdResult Failed() => new RewardedAdResult(RewardedAdStatus.Failed);
    }

    public interface IRewardedAdService
    {
        bool IsAvailable { get; }
        Task<RewardedAdResult> ShowRewardedAsync(CancellationToken cancellationToken = default);
    }
}
