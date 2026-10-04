# Persistence / Release Hardening

## Scope

This change set resolves the two external-audit Major findings without touching Chapter 01
composition, Phase 3B/3C visual content, map/minimap work, or repeatable encounter runtime.

- MJ-1: content evolution must not turn an otherwise structurally valid profile into a fresh profile.
- MJ-2: Dev and Candidate Android builds must not share the same Android application identity.
- Minor durability review: profile temp writes are durably flushed where the platform supports it.

Save schema remains at v1. Android Candidate remains the canonical committed project identity.

## MJ-1 — tolerant restore

### Structural corruption versus content evolution

Restore now separates data that defines the integrity/identity of a profile from optional
catalog references that can disappear as content evolves.

Structural/critical state remains strict and rejects the save when invalid:

- save schema and migration chain;
- profile GUID and UTC timestamps;
- required profile sections;
- non-negative finite progression values and non-negative assimilation score;
- duplicate/non-empty first-kill structure;
- stable quest identity (`questId`);
- boss identity (`bossId`);
- world/boss authoritative consistency;
- malformed quest life GUIDs, duplicate objectives, invalid progress for objectives that still exist;
- malformed inventory arrays, duplicate ownership, duplicate slots, invalid slots, wrong-slot known equipment,
  or any equipped equipment reference that is not present in the saved ownership set.

Optional content references are tolerant:

- removed first-kill enemy reward ids are dropped;
- removed quest objective ids are dropped;
- removed owned equipment ids are dropped;
- removed equipped equipment ids are dropped.

A development/editor load reports each tolerant drop/normalization through the existing save
diagnostics warning boundary. Validation and release restore do not require logging in order to
succeed.

A stale optional reference therefore cannot by itself make both main and backup fail validation
and cause a fresh profile to be created.

## Restore normalization rules

Stat level is treated as durable achieved progression. Saved stat XP is residual XP inside the
saved level, matching `AssimilationProgressionService`.

For every stat, restore applies this deterministic policy:

1. Saved level must still be at least 1. Saved XP must be finite and non-negative.
2. If the saved level is above the current stat maximum, level is clamped to the current maximum.
3. If the resulting level is the current maximum, residual XP is set to zero because current
   runtime progression cannot retain or consume XP above max level.
4. Otherwise, saved residual XP is run forward through the **current**
   `ProgressionThresholdCurve`, using exactly the same repeated threshold subtraction used by
   runtime progression.
5. If this reaches the current maximum, remaining XP is set to zero.
6. A more expensive new threshold never demotes a level that the player already achieved.
7. Total assimilation score is preserved unchanged.
8. Normalized state is persisted by the normal startup checkpoint, so restoring the normalized
   save again is idempotent and does not duplicate progression.

This intentionally does **not** reconstruct historical total XP from today's thresholds. Doing
that would invent progression because the old threshold history is not stored.

## Content compatibility version decision

No `contentVersion` is added in this PR.

`schemaVersion` continues to mean serialization/shape compatibility only. Current tuning
changes are deterministic from the saved durable level/residual-XP state plus current catalogs,
so a second version number would not add information needed for this recovery.

Introduce a separate content compatibility version later only when a content migration cannot be
derived safely from current data — for example, a semantic id rename that must map one critical
identity to another, a split/merge of durable progression currencies, or a one-time compensation
rule. Such a version must remain separate from `SaveSchema`.

## Backup and corruption behavior

The existing repository recovery contract is retained:

- main is attempted first;
- invalid main falls back to the validated backup;
- corrupt main is preserved when backup recovery succeeds;
- if both copies are structurally invalid they are preserved as corrupt files before a fresh
  profile is created;
- unsupported future schema remains invalid for the current migration pipeline;
- transient storage I/O failures remain storage failures rather than being reclassified as
  semantic corruption;
- temp save read-back validation and portable replace semantics remain in place.

### Durability minor

`SystemProfileFileSystem.WriteAllText` now writes through `FileStream`, flushes the writer,
then requests `FileStream.Flush(true)`. On platforms where durable flush is explicitly not
supported it falls back to the normal stream flush. I/O failures are not swallowed.

The single backup remains the previous validated main generation. Adding a second-generation
backup is deferred: it would change file lifecycle/recovery ordering and needs a separate
failure-injection review. The current one-generation lag is therefore an explicit
previous-known-good rollback policy, not silently changed in this hardening PR.

## MJ-2 — Dev / Candidate isolation

Android identities are now:

- Dev: `com.gravivore.mobile.dev`
- Candidate: `com.gravivore.mobile`

`com.gravivore.mobile` remains the canonical committed PlayerSettings identity.

A build captures the scoped PlayerSettings values, applies the requested flavor application id,
version and versionCode, builds and verifies the artifact, then restores the captured values in a
`finally` block. A failed build therefore does not leave the next flavor with the wrong id.

Dev and Candidate consequently use different Android package sandboxes and different
`Application.persistentDataPath` locations on device.

## versionCode contract

The CLI remains explicit:

```powershell
# Development build; repository default versionCode is allowed
.\build-android.ps1 -Flavor Dev

# Development build with explicit version information
.\build-android.ps1 -Flavor Dev -Version 0.1.0 -VersionCode 27

# Candidate build; versionCode is mandatory
.\build-android.ps1 -Flavor Candidate -Version 0.1.0 -VersionCode 28
```

Candidate builds reject a missing/non-positive versionCode. The wrapper also scans existing local
Candidate `*.build.json` artifacts and rejects a new Candidate versionCode that is not greater
than the highest local Candidate code.

Global monotonicity across clean machines remains a release-process responsibility: use a
versionCode greater than every Candidate/production APK that has been distributed. This PR does
not invent a CI counter or remote release registry.

## Candidate post-build verification

Unity-side verification checks before/after `BuildPipeline.BuildPlayer`:

- expected flavor application id is active;
- requested version and versionCode are active;
- ARM64-only target is active;
- Candidate options contain neither `Development` nor `AllowDebugging`;
- the reported APK exists.

Build metadata now records:

- flavor;
- semantic version;
- versionCode;
- applicationId;
- development-build flag;
- allow-debugging flag;
- target architecture;
- timestamp, git SHA, Unity version and APK filename.

The PowerShell wrapper re-reads that metadata and checks it against the requested flavor.

When Unity's bundled Android SDK exposes `aapt.exe`, the wrapper additionally inspects the
produced APK through Android's manifest/package tooling and verifies:

- actual APK package id;
- actual APK versionCode;
- ARM64-only native code;
- Candidate manifest is not debuggable.

The wrapper also reports whether `android.permission.INTERNET` is present. It does not currently
fail a Candidate for INTERNET because the repository does not define a no-INTERNET release
contract; `forceInternetPermission = false` only means Unity is not forced to add it.

If `aapt.exe` is unavailable, the wrapper emits a warning and retains the Unity-side plus
metadata checks rather than performing fragile binary/text searches.

## Signing boundary

No key material is committed.

The current project has no custom Android keystore configured, so the present Candidate path is
appropriate for private/internal testing only. A distributable release must use a release
keystore and credentials supplied outside the repository (for example by the developer machine or
future secret-backed release tooling). This PR does not fabricate credentials and does not add a
keystore to source control.

## Test coverage

Persistence coverage added in `SaveOfflineTests`:

- tuning/threshold mismatch normalizes rather than wiping;
- reduced current max level clamps safely;
- unknown optional quest objective is dropped;
- unknown optional equipment ownership/equip is dropped;
- unknown optional first-kill content is dropped;
- unknown critical boss/quest identity still rejects;
- normalized progression round-trip is idempotent.

Existing persistence tests retained and continue to cover:

- current v1 round-trip;
- v0 -> v1 migration;
- malformed/semantically corrupt main recovery from valid backup;
- corrupt-file preservation;
- interrupted temp writes;
- future/negative/missing migration rejection;
- no duplicate offline/pending reward behavior.

Android coverage in `AndroidBuildTests` now includes:

- exact Dev/Candidate package ids;
- Dev development/debug options;
- Candidate exclusion and explicit rejection of injected development/debug options;
- scoped Dev -> Candidate -> Dev identity restoration;
- explicit Candidate versionCode contract;
- flavor-specific output naming;
- metadata application id/debug/architecture agreement;
- ARM64/IL2CPP/API 26 project settings.

## Deferred risks

- Second-generation backup: deferred for a dedicated recovery/failure-injection change.
- Critical content id rename/remap: still requires an explicit migration rule; tolerant restore
  intentionally does not guess a replacement for stable critical identities.
- Release signing: external release keystore provisioning remains outside repository.
- Global versionCode allocation across machines: release-process/CI responsibility; local wrapper
  enforces monotonicity against artifacts visible on that machine.
