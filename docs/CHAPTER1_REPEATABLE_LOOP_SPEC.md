# Chapter 1 Repeatable Loop Specification

Status: **PRE-DESIGN / IMPLEMENTATION CONTRACT**  
Branch: `design/chapter1-repeatable-loop-spec`  
Scope: documentation and state contracts only. No gameplay implementation is included in this PR.

## 1. Purpose

Phase 4 turns Chapter 1 from a one-shot vertical slice into a repeatable local loop without invalidating the existing first-clear progression contract.

The design must preserve these already-authoritative facts:

- ordinary farming uses 8-12 second base respawns and adaptive spot pressure;
- Magnetar Guard's first defeat is the progression event that opens the boss path;
- Custodian M-0's first defeat completes Chapter 1;
- first-clear progression and one-time rewards must never replay;
- the game remains single-player, local-only, without backend time authority;
- active play should remain preferable to waiting or clock management;
- repeatability is for prototype retention and testing, not live-service scarcity.

The core rule is:

> **Progression state is monotonic and permanent. Repeatable encounter state is renewable and time-bound. They are separate state dimensions.**

## 2. Existing baseline inspected

The current `main` baseline contains the accepted Visual World Integration Foundation and the S20 balance pass.

Relevant existing behavior:

- five ordinary spots each maintain four enemies;
- ordinary base respawn jitter is 8-12 seconds;
- adaptive respawn adds +8 seconds every four kills on the same spot;
- adaptive pressure caps at four steps (+32 seconds);
- adaptive pressure has a 60 second idle grace and then recovers one pressure step every 30 seconds;
- adaptive pressure is currently session-local and independent per spot;
- each ordinary kill currently grants one stat XP and one assimilation score through the existing progression system;
- elite progression is represented by persisted `WorldUnlockState.EliteDefeated`;
- boss completion is represented by persisted `BossCompletionState.Defeated`;
- save schema is currently version 1;
- UTC is already abstracted through `ITimeProvider`;
- `WorldMarkerReadModel` already exposes ordinary availability, respawn time, pending respawns, and pressure step.

S20 balance is the reference point for relative Phase 4 tuning, not a requirement to freeze exact combat values.

## 3. Design invariants

Phase 4 implementation must maintain all of the following invariants:

1. Elite first defeat can unlock boss progression **exactly once**.
2. Boss first defeat can complete Chapter 1 **exactly once**.
3. A repeat elite or boss kill cannot re-fire one-time quest/progression completion.
4. A repeat kill cannot re-grant a first-clear reward.
5. Encounter cooldown and premium reward entitlement are not the same state.
6. Reaching a repeat reward cap must not permanently disable the encounter.
7. All durable cooldown/window decisions use one UTC time source contract.
8. UI/map reads state; it does not own cooldown or reward logic.
9. Ordinary adaptive pressure remains independent by spot.
10. Elite/boss cooldowns are sufficient anti-farm controls; do not stack adaptive pressure on them.
11. A crash-safe encounter transaction must not produce a duplicated durable reward.
12. Existing v1 users must keep all progression when migrating to Phase 4.

---

# 4. First-kill versus repeatable state model

## 4.1 State dimensions

Do not build one large enum containing every combination of progression, cooldown, and reward cap.

Each elite/boss encounter has three independent dimensions:

### A. Progression state

Monotonic. Persisted. Never resets.

### B. Encounter availability state

Renewable.

- `Locked` — progression prerequisite not satisfied.
- `Available` — encounter may be started.
- `Cooldown` — encounter was defeated and is waiting for `nextAvailableUtc`.

### C. Premium reward entitlement

Renewable.

- `FirstClear` — progression kill has never happened.
- `PremiumAvailable` — repeat premium count is below the current 24-hour window cap.
- `PremiumCapped` — repeat premium cap is reached; the encounter may still be fought after cooldown, but only the fallback reward is available.

This split avoids combinations such as "BossCompletedButCooldownAndDailyCapReached" becoming gameplay states.

## 4.2 Magnetar Guard

### Progression state

`EliteProgressionLocked`

- elite gate not unlocked;
- Magnetar Guard is not startable.

`EliteFirstKillAvailable`

- elite gate unlocked;
- `EliteDefeated == false`;
- defeating Magnetar Guard records the one-time elite progression event.

`EliteProgressionComplete`

- `EliteDefeated == true`;
- boss gate is unlocked;
- this never becomes false again.

### First defeat transaction

The first elite defeat must:

1. mark elite progression complete;
2. unlock the boss gate if not already unlocked;
3. complete the elite quest objective exactly once;
4. grant the one-time first-clear reward exactly once;
5. set elite `nextAvailableUtc`;
6. **not** increment the repeat premium-window kill count.

After this transaction commits, future elite spawns are repeat content only.

## 4.3 Custodian M-0

### Progression state

`ChapterIncomplete`

- boss progression completion is false;
- boss may remain locked until existing boss-gate prerequisites are met.

`BossFirstKillAvailable`

- boss gate is open;
- Chapter 1 completion is still false.

`ChapterComplete`

- boss completion is true;
- Chapter 1 completion UI/quest/reward has already been committed;
- this never becomes false again.

### First defeat transaction

The first boss defeat must:

1. mark `BossCompletionState.Defeated`;
2. complete the boss quest objective exactly once;
3. grant the Chapter 1 first-clear reward exactly once;
4. trigger Chapter-complete presentation only after the authoritative transaction is durable enough to recover;
5. set boss `nextAvailableUtc`;
6. **not** increment the repeat premium-window kill count.

A later boss kill must never execute steps 1-4 again.

---

# 5. Cooldown model

Cooldowns begin on a successful defeat, including the first progression defeat.

The provisional 60-minute elite / 120-minute boss values are suitable for a later retention-oriented product, but are poor test values for the present prototype. Chapter 1 itself targets roughly 30-45 minutes, so those cooldowns make repeatability difficult to exercise during a normal test session.

## 5.1 Compared profiles

### Current proposal

- Elite: 60 minutes.
- Boss: 120 minutes.

Pros:
- strong scarcity;
- naturally pushes return sessions;
- low farming throughput.

Cons:
- repeatability is barely testable in one closed-test session;
- gives too little data on repeat loop quality;
- feels like live-service gating before the game has live-service content.

### Short-session-friendly

- Elite: roughly 10-20 minutes.
- Boss: roughly 20-40 minutes.

Pros:
- repeat loop is testable in the same session;
- cooldown remains long enough that ordinary farming matters between encounters;
- no need to add harsher anti-farm rules.

Cons:
- not suitable as a final retention cadence without later telemetry/playtest evidence.

### Longer retention-oriented

- Elite: roughly 90-120 minutes.
- Boss: roughly 180-240 minutes.

Pros:
- strong return-session cadence;
- protects premium encounter rewards.

Cons:
- inappropriate for the current prototype;
- encourages waiting/clock manipulation when there is not yet enough alternate content.

## 5.2 Recommended initial test values

Use:

- **Magnetar Guard: 15 minutes**
- **Custodian M-0: 30 minutes**

These are test values, not final economy constants.

Reason:

- a player finishing a 30-45 minute first Chapter can plausibly see elite repeatability quickly;
- boss repeatability can still be reached in an extended test session;
- the cooldown is large relative to ordinary 8-12 second farming, so elite/boss cannot replace the ordinary loop;
- the values are easy to lengthen later without migration complexity because `nextAvailableUtc` is absolute.

---

# 6. Repeat premium reward window and caps

## 6.1 Baseline caps

Keep the current nominal entitlement for the MVP:

- Elite: **3 premium repeat rewards per 24-hour reward window**.
- Boss: **2 premium repeat rewards per 24-hour reward window**.

The one-time first-clear reward is outside these counts.

## 6.2 Do not use local calendar-day reset

For the local-only prototype, do not define a "day" as local midnight.

Use a simple **24-hour anchored UTC reward window** per encounter:

- the first premium repeat kill when no active window exists sets `rewardWindowStartedUtc = nowUtc`;
- the window expires at `rewardWindowStartedUtc + 24h`;
- after expiration, the next premium repeat kill starts a new window and resets the count;
- timezone and DST never affect the window because all durable timestamps are UTC.

This is intentionally simpler than storing an exact rolling list of every reward timestamp.

## 6.3 Post-cap behavior

Choose **B: encounter remains killable with reduced reward**.

After the premium cap is reached:

- cooldown still applies;
- encounter becomes available normally when cooldown ends;
- the player may fight it;
- no premium elite/boss package is granted;
- no first-window bonus is granted;
- a small fallback progression reward is granted, deliberately no better in time-efficiency than ordinary/strong-ordinary farming.

Why:

- hard-locking the encounter punishes a player who simply enjoys the fight;
- zero reward makes replay feel broken;
- diminishing-return curves add complexity that the prototype does not need;
- a clearly weaker fallback preserves playability without creating an optimal boss-farm exploit.

## 6.4 First reward in a window

Accept a modest "first premium reward in this 24-hour window" bonus for repeat kills.

The bonus must:

- apply only to a premium repeat reward;
- not apply to the first-ever progression clear, which already has its own one-time reward;
- be additive to the repeat premium package;
- use existing progression/reward categories;
- not introduce a new currency.

Treat its magnitude as a balance range in the reward ladder rather than a locked final number.

---

# 7. Local clock and reset behavior

The MVP is not cheat-proof. The goal is deterministic, understandable behavior and no accidental negative time.

## 7.1 Definition of time

- persistence stores round-trip UTC timestamps;
- `ITimeProvider.UtcNow` remains the external wall-clock source;
- no gameplay controller calls `DateTime.UtcNow` directly;
- cooldown/window ownership belongs to one repeatable-encounter domain/service boundary, not to elite and boss controllers separately.

## 7.2 Timezone changes

No effect on cooldown or reward-window duration.

UI may render local-friendly text later, but authority remains UTC.

## 7.3 DST

No effect. UTC timestamps make DST irrelevant to authoritative elapsed time.

## 7.4 Manual backward clock changes

The implementation should maintain an effective UTC floor:

`effectiveNowUtc = max(provider.UtcNow, persisted/session last-observed UTC floor)`

Behavior:

- a backward clock move never shortens a cooldown;
- it never reopens an expired reward window backwards;
- timers may appear stalled until wall clock catches up;
- log a development diagnostic for a meaningful backward anomaly.

Do not attempt complex anti-cheat compensation.

## 7.5 Manual forward clock changes

A large forward move can expire cooldowns/windows in a local-only build.

Accept this limitation for the prototype.

Do not add device fingerprinting, NTP, server validation, or punitive lockouts.

## 7.6 Offline time

Absolute UTC timestamps make cooldown and reward-window expiry continue while the app is closed.

This is independent of the existing offline-material reward system.

## 7.7 App restart and device restart

On load:

1. restore saved encounter timestamps/window counters;
2. obtain effective UTC from the shared time boundary;
3. derive current availability;
4. never replay historical completion/reward events merely because state was restored.

## 7.8 Reinstall / save reset

A reinstall or explicit development profile reset may erase local state.

The resulting profile is fresh:

- progression clears;
- encounter first-clear state clears;
- cooldowns/windows clear.

Cloud recovery is out of scope.

---

# 8. Reward contract

The reward system should reuse the existing permanent-growth vocabulary:

- stat XP;
- total assimilation score;
- existing equipment items where deliberately authored.

Do **not** add another generic currency for Phase 4.

Use relative reward weight `R`, where current S20 ordinary kills are the baseline `1R` (currently one stat XP + one assimilation for that archetype).

Required ordering:

`starting ordinary < strong ordinary near elite < strong ordinary near boss < elite < boss`

The detailed ladder is defined in `docs/CHAPTER1_REWARD_LADDER.md`.

General contract:

- one-time first clears are richer than corresponding repeat premium rewards;
- repeat premium rewards are materially above strong ordinary farming;
- post-cap fallback is intentionally not time-efficient;
- equipment is optional authored bonus content, never required for the repeat loop to function;
- first-window bonus uses the same reward categories.

---

# 9. Stronger ordinary spots

Add:

- 1-2 strong ordinary spots in the elite-side progression area;
- 1-2 strong ordinary spots in the late boss-side area.

They are still ordinary content:

- pooled;
- governed by ordinary target/aggro rules;
- counted under the existing global ordinary-enemy cap;
- independently pressure-tracked;
- no elite/boss cooldown semantics.

## 9.1 Near-elite profile

Relative to the chosen base ordinary archetype:

- HP: approximately **1.25-1.4x**;
- damage: approximately **1.15-1.25x**;
- aggro: keep near the S20 ordinary range; do not increase enough to pull the elite approach accidentally;
- reward: approximately **1.5-2R**;
- base respawn: approximately **12-16 seconds** before adaptive pressure.

## 9.2 Near-boss profile

Relative to the chosen base ordinary archetype:

- HP: approximately **1.5-1.75x**;
- damage: approximately **1.3-1.5x**;
- aggro: controlled at roughly ordinary S20 acquisition distances;
- reward: approximately **2-3R**;
- base respawn: approximately **14-20 seconds** before adaptive pressure.

These are authored ranges for testing. Final exact values require device playtesting after placement is known.

## 9.3 Spatial rules

For every strong spot:

- no spawn anchor may sit inside elite/boss automatic acquisition/engagement space;
- the player's intended traversal corridor to the elite/boss must remain outside ordinary acquisition radius plus a safety margin;
- fighting a strong ordinary pack must not accidentally start elite or boss combat;
- entering elite/boss combat must not automatically pull a strong ordinary pack;
- maintain at least one visually obvious safe route past each strong spot;
- environmental staging must communicate escalation before the player crosses into the stronger pack;
- late spots may be close enough to be tempting, but not so close that combined aggro becomes the default.

Exact coordinates belong to implementation/content authoring after the visual-integration checkpoint.

---

# 10. Anti-freefarm interaction

## 10.1 Existing ordinary pressure

Keep the current baseline semantics:

- +8 seconds every four kills on the same spot;
- maximum four steps;
- pressure isolated per spot;
- 60 second idle grace;
- pressure recovery afterward;
- pressure session-local for Phase 4 MVP unless a later product reason justifies persistence.

## 10.2 Stronger spots

Choose **C: separate authored profile**, but initialize it conservatively.

Recommended first test:

- use the same adaptive step shape as normal spots;
- allow a different base respawn range;
- expose the adaptive profile independently so later tuning does not require code changes.

Do **not** start with steeper pressure. Stronger HP/damage, longer base respawn, spatial risk, and higher opportunity cost already provide friction.

## 10.3 Elite and boss

Do not add adaptive pressure to elite or boss.

Cooldown + premium-window cap + weak post-cap fallback is sufficient.

Stacking another anti-farm penalty would create opaque punishment without solving a current prototype problem.

---

# 11. Persistence contract

Phase 4 requires a **save schema v2**.

Reason: repeatability adds authoritative, durable state that changes encounter availability and reward entitlement. Silently adding fields while leaving the file at schema v1 would weaken the project's explicit migration contract.

## 11.1 Already persisted

Existing v1 state that remains authoritative:

- profile id;
- created/last-seen UTC;
- permanent stat levels and XP;
- total assimilation;
- ordinary first-kill enemy ids;
- inventory/equipment;
- quest state;
- elite gate unlocked;
- elite first progression defeat;
- boss gate unlocked;
- boss first completion;
- offline material state.

## 11.2 New persisted

Add a root repeatable-encounter section containing elite and boss entries.

Each encounter entry needs:

- stable encounter id;
- `nextAvailableUtc` or explicit empty/unset value;
- `rewardWindowStartedUtc` or explicit empty/unset value;
- `rewardedKillsInWindow`;
- pending encounter reward transaction data, if one exists.

The pending transaction must include enough immutable reward data to recover deterministically after a crash; see the migration plan and idempotency section.

## 11.3 Session-only

Keep these session-only:

- ordinary spot adaptive pressure;
- live/pending ordinary spawn state;
- current elite/boss combat phase;
- current HP;
- current telegraph/attack timers;
- arena reset timers;
- map/UI cached presentation values;
- session monotonic effective-time floor above the last persisted UTC floor.

## 11.4 Derived

Do not persist:

- cooldown remaining seconds;
- whether a cooldown has expired;
- whether premium reward is currently available;
- reward-window remaining seconds;
- map marker status;
- "first repeat reward in window" boolean;
- Chapter complete presentation visibility.

Derive these from progression state, persisted timestamps/counters, current effective UTC, and current runtime encounter state.

Exact v1 -> v2 migration rules are in `docs/CHAPTER1_SAVE_MIGRATION_PLAN.md`.

---

# 12. Time ownership contract

Future implementation should have one repeatable-encounter time owner.

It should:

- depend on `ITimeProvider`;
- read/write absolute UTC encounter timestamps;
- apply the effective-time floor;
- resolve cooldown/window state on load and resume;
- expose derived availability/read models to controllers and map/UI;
- be the only domain boundary that advances repeat encounter time.

Elite/boss controllers remain combat controllers.

They must not independently:

- calculate wall-clock cooldowns;
- reset 24-hour windows;
- inspect local timezone;
- mutate persisted window counters.

This keeps foreground, resume, process death, save/load, offline, and device restart behavior consistent.

---

# 13. Reward idempotency and crash transaction

A repeat/first-clear encounter outcome is a small local transaction.

The specification requires a prepare/commit pattern for elite and boss rewards.

## 13.1 Transaction sequence

On lethal encounter completion:

1. Determine whether this is first-clear, premium repeat, or capped fallback.
2. Compute the deterministic reward grant payload.
3. Generate a unique local transaction id.
4. Mutate authoritative progression/completion/repeat state to the new encounter state **and** record a persisted `pendingEncounterReward` containing the immutable grant payload.
5. Force a synchronous profile checkpoint before showing irreversible completion/reward presentation.
6. Apply the pending reward to authoritative progression/inventory state in memory.
7. Clear `pendingEncounterReward`.
8. Force a second profile checkpoint containing the reward-applied state and cleared transaction.
9. Only then treat the reward transaction as fully committed for presentation purposes.

## 13.2 Recovery

If load finds a pending transaction:

- do not replay the encounter;
- restore the already-persisted completion/cooldown/window state;
- apply the persisted immutable reward payload once to runtime state;
- clear the pending transaction;
- synchronously checkpoint the resolved state;
- then expose normal presentation/read models.

Because the whole profile is saved atomically with validated backup fallback, a crash during recovery resolves to either:

- old durable state with the pending grant still present; or
- new durable state with reward applied and pending grant cleared.

It must never produce two durable grants.

## 13.3 Ordinary rewards

Do not retrofit this transaction protocol to every ordinary kill in Phase 4.

Ordinary progression already has life-id deduplication and ordinary farming does not carry one-time Chapter completion semantics.

The stronger guarantee is reserved for elite/boss one-time and premium repeat transactions.

---

# 14. Map / UI read model

Do not build a minimap in Phase 4.

Keep `IWorldMarkerSource` as the pull-based presentation boundary where practical.

## 14.1 Ordinary spots

Existing fields remain useful:

- id;
- position;
- live count;
- pending respawns;
- next respawn seconds;
- pressure step;
- available/respawning status.

Likely additional future field:

- ordinary spot profile/tier (normal, strong-elite-side, strong-boss-side) for marker styling.

## 14.2 Elite and boss

Do not represent permanent progression completion as permanent `Defeated` marker status once encounters are repeatable.

Expose separate read dimensions:

- progression locked/unlocked;
- first-clear completed;
- encounter available/cooldown;
- cooldown remaining;
- premium rewards remaining in current window;
- premium cap reached;
- reward-window remaining;
- whether the next premium reward receives the first-window bonus.

A future marker can display both:

- "Chapter progression complete" badge;
- "Encounter available in 12m" state.

This avoids conflating completion with current spawn availability.

---

# 15. Future test matrix

Implementation must cover at least the following deterministic tests.

## 15.1 Elite

- gate locked -> elite unavailable;
- gate unlocked -> first elite available;
- first kill records progression once;
- first kill unlocks boss gate once;
- first kill reward is not counted against repeat premium cap;
- first kill starts cooldown;
- exact `now == nextAvailableUtc` boundary is available;
- one tick before boundary is cooldown;
- first repeat kill grants premium reward and starts reward window;
- three premium repeat kills fill cap;
- post-cap kill remains possible after cooldown and grants fallback only;
- reward-window expiry resets repeat count on the next premium grant;
- reload during cooldown preserves remaining time;
- background/resume across cooldown expiry makes elite available;
- repeat kill never emits elite progression unlock again.

## 15.2 Boss

- boss unavailable before boss gate;
- first boss kill sets Chapter completion exactly once;
- first boss kill reward is not counted against repeat premium cap;
- repeat boss kill never re-completes Chapter 1;
- repeat boss kill never replays one-time Chapter reward;
- 30-minute cooldown boundary behavior;
- two premium repeat rewards fill cap;
- post-cap boss remains replayable with fallback only;
- reload preserves completion/cooldown/window count;
- pending reward transaction recovers exactly once after simulated crash.

## 15.3 Persistence

- v1 migration preserves all existing progression;
- v1 elite-defeated state remains elite progression complete;
- v1 boss-defeated state remains Chapter complete;
- new repeat fields default safely;
- old save with no repeat section migrates rather than failing restore;
- corrupt main with valid v2 backup recovers;
- corrupt pending transaction payload is rejected/recovered through normal repository behavior;
- future/unsupported schema remains rejected.

## 15.4 Clock

- normal forward UTC;
- exact cooldown expiry;
- small backward clock move;
- large backward clock move;
- timezone change with identical UTC;
- DST transition has no authority effect;
- large forward move is accepted as prototype limitation;
- process restart with wall clock behind persisted floor does not shorten timers.

## 15.5 Stronger ordinary spots

- authored reward profile is above starting ordinary;
- elite-side profile remains below boss-side profile;
- adaptive pressure is independent per strong spot;
- pressure does not leak to normal spots;
- base respawn profile plus adaptive delay is respected;
- strong spot cannot accidentally engage elite in the canonical traversal path;
- late strong spot cannot accidentally engage boss;
- global ordinary live-enemy cap is still respected.

---

# 16. Future implementation boundary

This PR must not change runtime files.

After the visual-integration checkpoint is stable, Phase 4 implementation is likely to touch or add code around:

- `Assets/_Game/Runtime/Gameplay/Encounters/`
  - repeat availability/reward state/service;
  - Magnetar Guard reset/reactivation boundary;
  - Custodian M-0 repeat reset/reactivation boundary;
  - encounter completion transaction orchestration;
- `Assets/_Game/Runtime/Gameplay/Enemies/`
  - stronger-spot authored profiles;
  - possibly reward-context support if spot-specific rewards require it;
- `Assets/_Game/Runtime/Gameplay/Progression/`
  - encounter reward package application;
- `Assets/_Game/Runtime/Persistence/Profile/`
  - schema v2 DTO;
  - v1 -> v2 migration;
  - mapper/validation;
  - save coordinator transaction checkpoint hooks;
- `Assets/_Game/Runtime/Presentation/World/WorldMarkerReadModel.cs`
  - repeatable availability/read fields;
- `Assets/_Game/Runtime/Presentation/Composition/S01SceneCompositionRoot.cs`
  - future service wiring only;
- Chapter 1 definition assets / authored spawn definitions;
- deterministic EditMode and PlayMode tests.

## 16.1 Visual integration conflict boundary

PR #38 is already the accepted foundation on current `main`.

PR #39 remains a separate visual asset-pack PR and must not be modified here.

The later runtime integration of #39 is likely to touch presentation definitions, scene/composition wiring, visual bindings, and possibly `S01SceneCompositionRoot`. Phase 4 implementation is therefore intentionally blocked until that visual integration checkpoint is stable.

The documentation in this PR has no reason to edit:

- Phase 3B prefabs;
- presentation sockets;
- visual foundation classes;
- scenes;
- art assets.

Before Phase 4 coding begins:

1. merge/accept the intended visual integration checkpoint;
2. update/rebase the Phase 4 implementation branch from then-current `main`;
3. resolve composition/scene ownership once, rather than in parallel with visual integration.

---

# 17. Final decision table

| Topic | Current proposal | Recommended MVP | Reason | Revisit when |
| --- | --- | --- | --- | --- |
| Elite cooldown | 1 hour | 15 minutes | Repeatability must be testable in a normal closed-test session | Chapter 2 / retention testing / telemetry exists |
| Boss cooldown | 2 hours | 30 minutes | Keeps boss special while allowing same-session repeat testing | More repeatable content exists |
| Elite daily cap/window | 3 rewarded kills/day | 3 premium repeat kills per anchored UTC 24h window; first-ever clear excluded | Preserves nominal cap without local-midnight/DST complexity | Backend or live-ops calendar rewards exist |
| Boss daily cap/window | 2 rewarded kills/day | 2 premium repeat kills per anchored UTC 24h window; first-ever clear excluded | Same model, lower premium throughput | Backend or live-ops calendar rewards exist |
| Post-cap behavior | undecided | Encounter remains killable after cooldown; weak ordinary-equivalent fallback reward | Avoids punishing play while keeping premium economy capped | Reward economy is validated on device |
| First-of-day bonus | optional | First premium repeat reward in each 24h window gets a modest bonus | Adds a return-session hook without new currency | Telemetry shows it distorts routing |
| Stronger spots | 1-2 near elite and boss | 1-2 each; authored higher HP/damage/reward, controlled aggro, longer base respawn | Gives meaningful activity between encounter cooldowns | Chapter geometry/playtests prove density too high |
| Persistence | unspecified new fields | Persist repeat encounter timestamps, 24h-window counts, and pending encounter transaction | Required for restart/offline correctness and idempotency | Server authority/cloud save arrives |
| Save schema version | v1 today | **v2** | New authoritative durable semantics deserve explicit migration | Next persistent contract change |
| Time model | local device, no backend | Shared UTC `ITimeProvider` + monotonic effective-time floor; absolute timestamps | Consistent across foreground/resume/restart without fake anti-cheat | Trusted/server time becomes available |

## Status

**CHAPTER 1 REPEATABLE LOOP SPEC: READY FOR IMPLEMENTATION REVIEW**
