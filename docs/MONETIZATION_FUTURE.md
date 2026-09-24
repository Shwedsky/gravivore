# Future Monetization and RuStore/SBP Plan

**Not part of Vertical Slice v0.1.**

## What the supplied Butcher Hero screenshots show

The payment surface is branded **RuStore**.

The flow visible in the screenshots is consistent with RuStore's official payment UI:

1. player chooses `Small Gems Pack` inside the game;
2. RuStore payment sheet opens with the product and amount;
3. player selects `СБП`;
4. RuStore opens a bank chooser;
5. selecting T-Bank / Alfa-Bank redirects into that bank's app;
6. the bank receives the already-created payment context/amount;
7. after confirmation, deeplink handling returns control to the game/payment flow.

This is materially better than asking a user to scan a QR code from the same phone.

## Current official integration direction (checked 2026-09-24)

Use **RuStore Pay SDK**, not the legacy BillingClient SDK.

RuStore documentation states:
- Pay SDK is the current payment SDK;
- Unity is supported;
- current Unity Pay SDK documentation exposes version 11.1.0;
- SBP is available as a payment method;
- deeplink handling is used to leave for the bank app and return;
- one-time purchases can work without requiring the RuStore app in supported Pay SDK scenarios;
- subscription flows require authorization in the documented Pay SDK subscription scenario;
- SBP is a one-stage payment mechanism;
- old BillingClient SDK is deprecated and should not be chosen for new work.

Official references:
- https://www.rustore.ru/help/sdk/pay
- https://www.rustore.ru/help/sdk/pay/unity
- https://www.rustore.ru/help/users/purchases-and-returns/payment-method/fast-payment
- https://www.rustore.ru/help/guides/deep-links
- https://www.rustore.ru/help/developers/monetization/without-rustore-app

## Architecture for later

Do not wire shop UI straight to RuStore classes.

Flow:
`Shop UI -> ProductCatalog/Application Purchase Use Case -> IPurchaseService -> RuStore adapter`

Verified result:
`RuStore adapter/backend verification -> EntitlementService -> inventory/currency grant`

For consumable currency, grants must be idempotent.

## Planned product types

Potential future catalog:
- consumable premium currency;
- starter pack;
- permanent `No Ads`;
- 7-day resource reactor/pass;
- monthly subscription only after account/recovery is solved;
- cosmetic evolution skins;
- rewarded ads for optional accelerators.

Avoid monetizing basic movement or making ordinary enemy respawn intentionally miserable.

## Private APK phase

Do not add RuStore SDK yet.

Reasons:
- no public/store rollout yet;
- payment setup adds external console/configuration/test requirements;
- it does not help validate the core loop.

Keep the boundary clean so the integration is a later isolated specification.

## Payment safety rule

Never grant paid items merely because a local client callback says "success".

At public monetization stage implement:
- transaction/order identity;
- idempotency;
- entitlement reconciliation;
- server notification or server-side verification where required/available;
- recovery flow;
- audit logging.
