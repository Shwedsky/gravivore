# Phase 4 Runtime V2 — Integration Hooks

This branch deliberately does not edit Chapter01 presentation composition, map/minimap UI, Audio/VFX integration, Repair Hub presentation, or production visual replacement files.

## Repeat encounter defeat hook

Production wiring should snapshot whether the encounter was already first-cleared **before** applying the current kill's permanent first-clear mutation:

- Magnetar: authoritative `ProfileRuntimeState.World.EliteDefeated`
- Custodian: authoritative `ProfileRuntimeState.Boss.IsDefeated`

Pass that snapshot to `RepeatableRewardTransactionCoordinator.PrepareAndCommit(...)` together with the selected authored `CoreReward` package.

The runtime domain then guarantees:

- first clear uses `FirstClear` and does not consume premium repeat count;
- later Magnetar kills use the 15-minute repeat cooldown and 3-per-window premium cap;
- later Custodian kills use the 30-minute repeat cooldown and 2-per-window premium cap;
- a repeat kill cannot be classified as a new first clear when the pre-kill authoritative progression snapshot is already complete;
- pending reward state is durably prepared before the authored reward is applied.

Permanent progression remains owned by the existing world/boss progression authorities. Presentation must not infer completion from visuals.

## Reward package hook

`RepeatableEncounterRewardLadder` provides data-driven Phase 4 test defaults using the midpoint of the merged relative reward bands:

- Magnetar first clear: 7R
- Magnetar premium repeat: 5R
- Magnetar fallback: 1.5R
- Custodian first clear: 12R
- Custodian premium repeat: 8.5R
- Custodian fallback: 2.5R
- first premium repeat in a new anchored window: 1.25x selected premium package

`R` is the existing ordinary `CoreReward` comparison unit, not a new currency. Production content can replace these scale sets without changing cooldown/window transaction logic.

## Effective time hook

All persisted cooldown/reward-window decisions should consume `ProfileSession.EffectiveTime`, not raw device UTC. The provider advances and persists a monotonic UTC floor; a backward device-clock change therefore cannot shorten cooldowns or reward windows.

## Strong ordinary hook

`Chapter1StrongOrdinarySpotCatalog.Create(...)` produces authored elite-side and boss-side stronger spots. Integration should pass the coordinates of existing ordinary spots so the catalog can reject accidental coordinate duplication.

Each `StrongOrdinarySpotRuntime` owns an independent `AdaptiveRespawnState`. Do not route Magnetar or Custodian repeat cooldowns through ordinary adaptive pressure.

## World marker read hook

`Chapter1WorldMarkerAuthority` is gameplay-owned read state intended for future map/presentation adapters. It exposes:

- `StrongOrdinary`
- Magnetar/Custodian first-clear state supplied from authoritative progression
- repeat availability
- cooldown remaining
- reward entitlement
- premium rewards remaining
- reward-window remaining

The current gameplay-owned `Chapter01WorldConfiguration` does not contain an authoritative Repair Hub coordinate. This branch therefore does not invent one and does not expose a fabricated Repair Hub marker. Presentation-owned Repair Hub placement can be connected later once an authoritative gameplay coordinate exists.

## Crash recovery hook

On profile startup, integration should call `RepeatableRewardTransactionCoordinator.RecoverPending()` before permitting another repeat reward transaction. Recovery semantics are:

1. `Prepared` on disk means progression on disk is still pre-reward; apply once, mark `Applied`, save.
2. `Applied` on disk means the reward is already committed; do not grant again.
3. Clear the pending transaction and save a final completion checkpoint.

If a save fails, the transaction remains in a phase that can be retried without silently losing a committed reward or granting it twice.
