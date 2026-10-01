using System.Threading;
using System.Threading.Tasks;

namespace Gravivore.Platform.Monetization
{
    public enum PurchaseStatus
    {
        Unavailable = 0,
        Succeeded = 1,
        Cancelled = 2,
        Failed = 3
    }

    public readonly struct PurchaseResult
    {
        private PurchaseResult(PurchaseStatus status, EntitlementId entitlement, bool hasEntitlement)
        {
            Status = status;
            Entitlement = entitlement;
            HasGrantedEntitlement = hasEntitlement;
        }

        public PurchaseStatus Status { get; }
        public EntitlementId Entitlement { get; }
        public bool HasGrantedEntitlement { get; }
        public bool Succeeded => Status == PurchaseStatus.Succeeded;

        public static PurchaseResult Success(EntitlementId entitlement)
        {
            if (!entitlement.IsValid)
            {
                throw new System.ArgumentException("A valid entitlement is required.", nameof(entitlement));
            }

            return new PurchaseResult(PurchaseStatus.Succeeded, entitlement, true);
        }

        public static PurchaseResult Cancelled() =>
            new PurchaseResult(PurchaseStatus.Cancelled, default, false);

        public static PurchaseResult Unavailable() =>
            new PurchaseResult(PurchaseStatus.Unavailable, default, false);

        public static PurchaseResult Failed() =>
            new PurchaseResult(PurchaseStatus.Failed, default, false);
    }

    public interface IPurchaseService
    {
        bool IsAvailable { get; }
        bool HasEntitlement(EntitlementId entitlementId);
        Task<PurchaseResult> PurchaseAsync(ProductId productId, CancellationToken cancellationToken = default);
        Task<PurchaseResult> RestorePurchasesAsync(CancellationToken cancellationToken = default);
    }
}
