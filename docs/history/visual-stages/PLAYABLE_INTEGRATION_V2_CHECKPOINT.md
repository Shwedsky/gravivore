# Playable Integration V2 — persistence checkpoint

Branch: `integration/playable-presentation-pass-v2`
Base main: `43e0416f341e3b106ad976053746da540a1b85d4`

Purpose of this file: prove that the integration branch is persisted remotely before substantial implementation begins.

Planned production integration checkpoints:

1. Wire the merged Map / Minimap module into the real Chapter01 using the authoritative `WorldMarkerReadModel` only.
2. Wire the merged Phase 6B Audio / VFX pack into player Gravity Lash and enemy hit/death presentation without changing gameplay authority or timing.
3. Wire Magnetar, Custodian telegraph families, and Repair Hub presentation into existing gameplay state/events.
4. Remove superseded duplicate legacy presentation and add production-wiring tests/pool-reset coverage.

Out of scope: persistence/save schema, Phase4 runtime/economy/repeatability, cooldown redesign, gameplay balance, visual model replacement, and VisualReplacementProofV1.

Delivery rule for this branch: each major checkpoint is committed and pushed before moving on.
