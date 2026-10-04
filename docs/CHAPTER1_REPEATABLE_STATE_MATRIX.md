# Chapter 1 Repeatable State Matrix

Status: **Phase 4 design contract**

This document is the compact state/read-model reference for implementation and tests.

## 1. General rules

Progression state, encounter availability, and reward entitlement are independent.

Never infer one-time progression from the current combat controller state.

Never use `Dead` as the durable meaning of "Chapter progression complete" once an encounter can respawn.

## 2. Magnetar Guard matrix

| Progression | Cooldown | Premium entitlement | Can spawn/fight? | Kill effect |
| --- | --- | --- | --- | --- |
| Elite gate locked | any | any | No | None |
| First kill not completed | none | FirstClear | Yes | Complete elite progression once; unlock boss path; grant first-clear package; start cooldown; repeat count unchanged |
| First kill completed | active | premium available | No | None |
| First kill completed | expired | premium available | Yes | Grant premium repeat package; increment 24h window count; start cooldown |
| First kill completed | active | premium capped | No | None |
| First kill completed | expired | premium capped | Yes | Grant capped fallback only; do not increment premium count; start cooldown |
| First kill completed | expired | previous 24h window expired | Yes | On premium kill, start a new 24h window at kill time, set count to 1, grant first-window bonus |

## 3. Custodian M-0 matrix

| Progression | Cooldown | Premium entitlement | Can spawn/fight? | Kill effect |
| --- | --- | --- | --- | --- |
| Boss gate locked | any | any | No | None |
| Chapter incomplete | none | FirstClear | Yes | Complete Chapter 1 once; grant first-clear package; start cooldown; repeat count unchanged |
| Chapter complete | active | premium available | No | None |
| Chapter complete | expired | premium available | Yes | Grant premium repeat package; increment 24h window count; start cooldown |
| Chapter complete | active | premium capped | No | None |
| Chapter complete | expired | premium capped | Yes | Grant capped fallback only; do not increment premium count; start cooldown |
| Chapter complete | expired | previous 24h window expired | Yes | On premium kill, start a new 24h window at kill time, set count to 1, grant first-window bonus |

## 4. Premium-window derivation

For an encounter with:

- `rewardWindowStartedUtc`
- `rewardedKillsInWindow`
- configured `premiumCap`

derive:

### No active window

If start timestamp is unset:

- count must be zero;
- next eligible repeat kill starts a window.

### Active window

If:

`effectiveNowUtc < rewardWindowStartedUtc + 24h`

then:

- premium remaining = `max(0, cap - rewardedKillsInWindow)`;
- capped = remaining == 0.

### Expired window

If:

`effectiveNowUtc >= rewardWindowStartedUtc + 24h`

then current read state is logically:

- premium remaining = cap;
- capped = false;
- first-window bonus eligible = true.

Do not require an eager mutation at the instant the window expires. Normalize persisted window state on the next authoritative mutation/save if useful.

## 5. Cooldown derivation

If `nextAvailableUtc` is unset or:

`effectiveNowUtc >= nextAvailableUtc`

encounter cooldown is expired.

Otherwise:

- cooldown is active;
- remaining = `nextAvailableUtc - effectiveNowUtc`.

Exact equality means available.

## 6. First-clear separation

### Elite

Existing durable progression authority:

- `WorldUnlockState.EliteDefeated`.

Repeatable state must never set this false.

### Boss

Existing durable progression authority:

- `BossCompletionState.Defeated`.

Repeatable state must never set this false.

The repeat system reads these values to decide whether a defeat is `FirstClear` or repeat content.

## 7. Reward classification

At lethal completion classify in this order:

1. If durable progression first-clear is incomplete -> `FirstClear`.
2. Else if current reward window is expired/unset -> `PremiumRepeatFirstInWindow`.
3. Else if premium count < cap -> `PremiumRepeat`.
4. Else -> `CappedFallback`.

Classification is deterministic using state captured before the transaction.

## 8. Ordinary spot read state

Existing ordinary map fields remain:

| Field | Source |
| --- | --- |
| available/respawning | live + pending runtime state |
| live count | `SpawnSpotRuntime` |
| pending count | `SpawnSpotRuntime` |
| next respawn | respawn schedule |
| pressure step | `AdaptiveRespawnState` |

Future addition:

- spot tier/profile: normal / strong-elite-side / strong-boss-side.

Adaptive pressure remains session-only.

## 9. Elite/boss map read state

Recommended read-only snapshot fields:

- encounter id;
- marker kind;
- world position;
- progressionLocked;
- firstClearCompleted;
- availability: locked / available / cooldown / engaged;
- cooldownRemaining;
- premiumCap;
- premiumRewardsRemaining;
- rewardWindowRemaining;
- premiumCapped;
- firstWindowBonusEligible.

Presentation may collapse these into a small marker enum, but the underlying read model should not discard the independent dimensions.

## 10. Combat-controller relationship

Combat controllers may still use internal transient states such as:

- Waiting;
- Approach;
- Telegraphing;
- Recovery;
- Dead;
- Dormant;
- Engaging;
- Resetting.

These are not persistence states.

The repeatable encounter service decides whether a combat controller should be made available/reset for a new encounter.

## 11. App lifecycle matrix

| Lifecycle event | Required behavior |
| --- | --- |
| Foreground normal play | Derive against shared effective UTC |
| Background/pause | Checkpoint authoritative dirty state |
| Resume | Update effective UTC; resolve derived expiry; no historical event replay |
| Process death | Load persisted progression/repeat state |
| Device restart | Same as process restart |
| Offline past cooldown | Encounter is available on load |
| Offline past reward-window expiry | Next premium kill starts a new 24h window |
| Backward device clock | Effective UTC floor prevents timers moving backward |
| Forward device clock | Expiry may advance; accepted local-MVP limitation |

## 12. Idempotency matrix

| Crash point | Durable state on reload | Required recovery |
| --- | --- | --- |
| Before transaction prepare save | old encounter/reward state | Encounter outcome was not committed; no reward |
| After prepared state + pending reward saved | completion/cooldown/window advanced; reward pending | Apply pending immutable reward exactly once, clear pending, save |
| During reward application before final save | durable save still contains pending reward | Reload old durable prepared state, apply once again to restored old profile snapshot, then commit |
| After final save | reward applied; pending empty | Normal load, no replay |

The system relies on whole-profile atomic/backup semantics; it must not attempt to merge partially written JSON.

## 13. Invalid combinations

Validation must reject or normalize unsafe combinations such as:

- negative rewarded kill count;
- count above configured cap;
- window count > 0 with no window timestamp;
- pending transaction with unknown encounter/reward identifiers;
- first-clear repeat transaction that contradicts durable progression state;
- boss first-clear completed while existing boss completion is false;
- timestamps not parseable as UTC;
- reward window start after an impossible future-floor policy if implementation chooses such validation.

Avoid validation that makes benign forward device-clock movement destroy a profile.

## Status

**CHAPTER 1 REPEATABLE STATE MATRIX: READY FOR IMPLEMENTATION REVIEW**
