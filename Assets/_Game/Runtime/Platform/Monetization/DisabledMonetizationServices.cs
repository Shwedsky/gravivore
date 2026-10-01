using System.Threading;
using System.Threading.Tasks;

namespace Gravivore.Platform.Monetization
{
    public sealed class DisabledPurchaseService : IPurchaseService
    {
        private static readonly Task<PurchaseResult> UnavailableResult =
            Task.FromResult(PurchaseResult.Unavailable());

        public bool IsAvailable => false;

        public bool HasEntitlement(EntitlementId entitlementId) => false;

        public Task<PurchaseResult> PurchaseAsync(
            ProductId productId,
            CancellationToken cancellationToken = default) => UnavailableResult;

        public Task<PurchaseResult> RestorePurchasesAsync(
            CancellationToken cancellationToken = default) => UnavailableResult;
    }

    public sealed class DisabledRewardedAdService : IRewardedAdService
    {
        private static readonly Task<RewardedAdResult> UnavailableResult =
            Task.FromResult(RewardedAdResult.Unavailable());

        public bool IsAvailable => false;

        public Task<RewardedAdResult> ShowRewardedAsync(
            CancellationToken cancellationToken = default) => UnavailableResult;
    }
}
