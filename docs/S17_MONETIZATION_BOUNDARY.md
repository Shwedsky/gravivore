# S17 Monetization Boundary

## Current v0.1 state

GRAVIVORE v0.1 has no monetization. Purchases, ads, subscriptions, store UI, payment recovery UI, network calls, and vendor SDK packages are absent. The production composition root supplies disabled project-owned providers and all monetization feature flags default to false.

## Project-owned boundary

`Gravivore.Platform.Monetization` owns the vendor-neutral contracts:

- `IPurchaseService` requests a purchase by stable `ProductId`, queries a stable `EntitlementId`, and provides a future restore entry point.
- `IRewardedAdService` reports availability and exposes an asynchronous show result.
- `PurchaseResult` and `RewardedAdResult` carry project-owned statuses only. They do not expose vendor transactions, receipts, SDK objects, or callbacks.
- `DisabledPurchaseService` and `DisabledRewardedAdService` are the v0.1 providers. They always report unavailable and never grant an entitlement or reward.

Gameplay does not consume these contracts in v0.1. Future gameplay or economy code may consume verified project-owned entitlement state, but must not reference a store SDK directly.

## Future RuStore adapter

A future RuStore Pay adapter belongs in the Platform layer and implements `IPurchaseService`. Vendor product identifiers, transaction objects, callbacks, and SDK-specific errors remain inside that adapter. Composition selects the adapter and injects the project-owned interface; Gameplay and Presentation do not import RuStore namespaces.

The adapter will map stable project `ProductId` values to vendor SKUs and verified outcomes to stable `EntitlementId` values. Adding an adapter must not change progression core or save domain contracts.

## Verified entitlement and recovery flow

A future successful flow requires authoritative verification appropriate to the product and store policy before an entitlement is exposed:

1. Request the vendor purchase through the Platform adapter.
2. Obtain and verify the transaction using the real provider/backend design selected for release.
3. Map the verified product to a project-owned entitlement.
4. Publish or persist only verified entitlement state through a dedicated future boundary.
5. Restore purchases through the same verification path when the account/device context supports recovery.

Local client booleans and `PlayerPrefs` are not trusted purchase proof. S17 does not fabricate server validation, receipts, accounts, or recovery guarantees. Account linking, cross-device ownership, conflict handling, revocation, and recovery UX remain future release work.

## Rewarded ads

A future ad SDK adapter implements `IRewardedAdService` inside the Platform layer. A reward is eligible only when the adapter returns `Completed`; unavailable, cancelled, and failed outcomes grant nothing. S17 does not add an ad placement, ad UI, simulated ad, or reward integration.
