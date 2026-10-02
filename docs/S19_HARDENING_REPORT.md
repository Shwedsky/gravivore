# S19 Hardening Report

## Scope and baseline

S19 audits the integrated Chapter 01 slice at base commit
`81c2be14331e1bf50e1bf460be2ddbd1a8c0b5d8`. It adds no gameplay,
balance, content, or release feature.

Baseline executed before S19 changes:

- Unity 6000.3.0f1 compile: PASS.
- EditMode: 233/233 PASS.
- PlayMode: 54/54 PASS.
- ProjectValidator: PASS.
- Dev Android IL2CPP: PASS, `Builds/Android/gravivore-dev-0.1.0+1.apk`.
- Candidate Android IL2CPP: PASS, `Builds/Android/gravivore-candidate-0.1.0+1.apk`.

## Audit findings

No Critical or Major defect was reproduced. Existing persistence, reward,
pooling, and build-flavor boundaries already implement the required behavior.
S19 closes verification gaps with deterministic tests and stable validation.

### Save interruption and recovery

The repository validates `profile.temp.json` before promotion, validates the
old main before promoting it to backup, and never considers a stale temp file
as a load candidate. Recovery distinguishes invalid profile data from storage
exceptions. New tests prove:

- a successful save preserves the exact previous main as backup;
- a stale temp cannot supersede the main profile;
- a temp write failure leaves both validated main and backup unchanged;
- repeated lifecycle flush attempts after a DEV profile reset cannot recreate
  any deleted profile file.

Existing tests continue to cover valid main load, backup recovery, corrupt
preservation, fresh fallback, migration, invalid ranges and ids, persistence
suspension after storage failure, and exactly-once offline accrual/claim.

### Duplicate reward and lifecycle review

The existing suite proves `EnemyLifeId` deduplication, one reward per pooled
life, no first-kill replay after save/load, idempotent elite/boss completion,
DEV boss reset without a new defeat event, exactly-once offline claim, repeated
resume deduplication, and non-stacking equipment restore. S19 adds a 64-life
pool stress test that requires a unique life id and exactly one death event per
cycle.

`OnApplicationPause(true)` and `OnApplicationQuit` flush through the same
coordinator. Resume advances `LastSeenUtc` before the checkpoint, so repeated
resume at the same timestamp earns zero. DEV profile reset sets a session guard
before teardown can flush. Physical Android background/lock/force-stop behavior
remains part of the manual checklist.

### Pool and presentation reset review

Ordinary enemy return clears health/activity, target references, cooldown,
life id, delegates, visual state, rotation, and collider activity. The new
64-cycle test verifies fixed hierarchy size, no multiplied death events, unique
life ids, zero leaked leases, and non-targetability after return.

Pulse and gravity-lash saturation tests execute 64 plays against capacity two,
verify active counts never exceed capacity, verify the hierarchy does not grow,
and verify all items return inactive. Existing tests cover S15 visual reset,
activation-failure accounting, target motion reset, and encounter telegraph
cleanup.

### Scene, asset, and build isolation review

ProjectValidator now opens both canonical scenes additively and rejects missing
MonoBehaviour scripts on every active or inactive GameObject in their complete
hierarchies without validating transient runtime-generated objects.
Required ScriptableObjects, S14 references, S15 catalog/models/materials, build
scene paths, URP assets, audio definitions, and Android settings remain covered
by existing checks.

Production runtime source is rejected if it introduces `Shader.Find` or
`Resources.Load`. Dev sources remain guarded by
`UNITY_EDITOR || DEVELOPMENT_BUILD`. Deterministic Android tests prove Dev uses
`Development | AllowDebugging`, Candidate uses neither, and both use identical
canonical scenes. Both real IL2CPP flavors are required in final verification.

## Allocation and performance review

Player/enemy ticks, targeting, spawn capacity, HUD change handlers, and VFX
pools showed no obvious project-owned per-frame collection or hierarchy growth.
S16 already exposes session elapsed time, approximate FPS/frame time, and live
ordinary-enemy count, so no new profiling UI was added.

`SafeEventDispatch` and the gravity-lash cue publisher use
`GetInvocationList()`, which allocates per event publication. These events occur
on damage/death/lash cues rather than every frame. Without profiler evidence of
a recurring GC problem, replacing event dispatch would be speculative and is
deferred as non-blocking debt to the physical max-population profile.

## Automated results

After adding S19 coverage:

- Unity compile: PASS.
- EditMode: 238/238 PASS.
- PlayMode: 56/56 PASS.
- ProjectValidator: PASS.
- Dev Android IL2CPP: PASS, 56,284,458 bytes,
  `Builds/Android/gravivore-dev-0.1.0+1.apk`, log
  `Builds/Logs/android-dev-20261002-060846.log`.
- Candidate Android IL2CPP: PASS, 24,014,892 bytes,
  `Builds/Android/gravivore-candidate-0.1.0+1.apk`, log
  `Builds/Logs/android-candidate-20261002-061623.log`.

## Package warning status

The previously observed Package Manager signature warnings for packages such
as Mathematics and Mono Cecil were not reproduced in the final logs and did not
prevent baseline or final compile, tests, validation, or either Android IL2CPP
flavor. Both final build logs did contain Unity Licensing Client signature
diagnostic code 10 at startup; licensing recovered and each build exited zero
with its APK present. This remains environment/maintenance debt. S19 does not
modify package versions, `packages-lock.json`, or global caches.

## Deferred manual verification

Physical-device lifecycle survival, boss/player bar rendering, 15-minute
max-population stability, thermal behavior, real haptics/audio, and portrait
safe-area/readability require the procedure in
`docs/S19_MANUAL_DEVICE_CHECKLIST.md`.

S19 PHYSICAL DEVICE / 15-MINUTE HARDENING CHECK: PENDING
