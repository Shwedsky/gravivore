# Chapter 1 Save Migration Plan — Schema v1 to v2

Status: **Phase 4 design contract**

## 1. Decision

Phase 4 repeatable elite/boss state requires **save schema version 2**.

Current `SaveSchema.CurrentVersion` is 1.

The implementation should add one explicit sequential migration:

`SaveMigrationV1ToV2`

Do not edit or remove the existing `0 -> 1` migration.

## 2. Why a version bump is required

Phase 4 adds authoritative persisted semantics:

- elite cooldown;
- boss cooldown;
- premium repeat reward windows;
- premium repeat counts;
- crash-recoverable pending encounter reward transaction.

These are not optional presentation fields. They determine whether content is available and whether a durable reward is owed.

Keeping `schemaVersion = 1` while silently adding those fields would violate the repository's explicit version/migration contract.

## 3. Proposed v2 DTO shape

Exact C# names are implementation details, but conceptually add:

```text
SaveRootDto
  schemaVersion = 2
  ...existing v1 fields unchanged...
  repeatableEncounters
    lastObservedUtc
    elite
      encounterId
      nextAvailableUtc
      rewardWindowStartedUtc
      rewardedKillsInWindow
    boss
      encounterId
      nextAvailableUtc
      rewardWindowStartedUtc
      rewardedKillsInWindow
    pendingEncounterReward
      transactionId
      encounterId
      rewardClass
      statType / statXp payload
      assimilation payload
      optional equipment item id
```

`pendingEncounterReward` must support an explicit null/empty state.

If implementation uses multiple reward components, persist the resolved immutable payload rather than merely a pointer to mutable balance data. Recovery must not change because a ScriptableObject was retuned between prepare and resume.

## 4. v1 -> v2 migration rules

Given a validated/migratable v1 save:

### Preserve unchanged

Copy all v1 values exactly:

- `profileId`;
- `createdUtc`;
- `lastSeenUtc`;
- player stat levels;
- stat XP;
- total assimilation;
- ordinary first-kill ids;
- world gate flags;
- elite defeated flag;
- boss id/defeated flag;
- quest state;
- inventory;
- offline balances.

No player progression may be reduced or reconstructed from current defaults.

### Initialize repeat state

Set:

- `repeatableEncounters.lastObservedUtc = v1.lastSeenUtc`;
- elite `nextAvailableUtc = unset`;
- elite `rewardWindowStartedUtc = unset`;
- elite `rewardedKillsInWindow = 0`;
- boss `nextAvailableUtc = unset`;
- boss `rewardWindowStartedUtc = unset`;
- boss `rewardedKillsInWindow = 0`;
- `pendingEncounterReward = empty`.

### Progression interpretation

If v1 says elite was defeated:

- keep `EliteDefeated = true`;
- boss gate remains unlocked as already validated;
- Magnetar first-clear is considered permanently completed;
- elite is **immediately repeat-available** after migration because no historical cooldown is invented.

If v1 says boss defeated:

- keep boss completion true;
- Chapter 1 first-clear remains permanently completed;
- boss is **immediately repeat-available** after migration.

Do not invent a cooldown from `lastSeenUtc` for historical v1 clears. The exact kill time is not stored, so any fabricated value would be false precision.

## 5. Fresh v2 profile

Fresh profile defaults:

- progression first-clear states false as today;
- repeat cooldown timestamps unset;
- window timestamps unset;
- repeat counts zero;
- pending reward empty;
- `lastObservedUtc = created/lastSeen UTC`.

## 6. Time-floor restore rule

On v2 load:

1. parse persisted `lastObservedUtc`;
2. read `ITimeProvider.UtcNow`;
3. set session effective UTC to the later of the two;
4. update the in-memory last-observed floor as effective UTC advances;
5. persist the floor on normal profile checkpoints.

This field is not an anti-cheat mechanism. It only prevents a backward local clock adjustment from making persisted cooldown time move backward.

If implementation determines that existing `lastSeenUtc` can safely serve this exact contract without coupling to offline-reward semantics, reusing it is allowed **only if** tests prove both systems remain correct. Separate `lastObservedUtc` is preferred because offline `lastSeenUtc` already has lifecycle meaning.

## 7. Validation rules

v2 mapper/validation should require:

- repeat root present;
- correct elite and boss stable ids;
- UTC round-trip timestamps when a timestamp is set;
- repeat counts in `0..cap`;
- zero count when reward-window timestamp is unset;
- pending transaction id non-empty when pending exists;
- pending encounter id is elite or boss configured id;
- pending reward payload is valid and non-negative/positive according to its component contract;
- optional equipment id exists in the configured catalog.

Do not reject a save solely because current device UTC is behind its persisted timestamps.

## 8. Backup and corruption behavior

Use the existing repository behavior unchanged:

- read/migrate/validate main;
- fallback to valid backup;
- preserve corrupt files;
- use temp file read-back validation before commit.

v2 does not need a second persistence repository.

## 9. Migration tests

Required deterministic tests:

1. v1 fresh-ish profile -> v2:
   - all progression preserved;
   - repeat state empty.

2. v1 elite defeated -> v2:
   - elite progression still complete;
   - boss gate still unlocked;
   - elite repeat immediately available;
   - repeat premium count zero.

3. v1 boss defeated -> v2:
   - boss completion still true;
   - Chapter completion not replayed;
   - boss repeat immediately available;
   - repeat premium count zero.

4. v1 inventory/quest/offline populated -> v2:
   - values byte/logically equivalent after mapping.

5. v0 -> v1 -> v2 chain:
   - pipeline executes both sequential migrations successfully.

6. malformed v2 repeat count:
   - validation rejects main and existing repository fallback behavior applies.

7. pending v2 reward:
   - restore produces recoverable pending transaction without replaying first-clear logic.

8. future schema >2:
   - remains rejected.

## 10. Compatibility boundary

PR #39 visual assets do not alter save data and must not be referenced from v2 DTOs.

Never persist:

- prefab paths;
- socket names;
- scene object ids;
- Unity instance ids;
- presentation binding ids that are not stable gameplay/content ids.

This keeps repeatable-loop saves independent of the visual integration checkpoint.

## Status

**CHAPTER 1 SAVE MIGRATION PLAN: READY FOR IMPLEMENTATION REVIEW**
