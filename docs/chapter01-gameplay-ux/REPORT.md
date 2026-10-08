# Chapter 01 gameplay UX APK delivery

One Android ARM64 DEV APK is built and verified. Device installation/playthrough is the next human gate.

- Branch: `polish/chapter01-respawn-map-repair-v1`.
- Draft PR: https://github.com/Shwedsky/gravivore/pull/63.
- Accepted base: `0141b0a88c0e68b34c3a26c3b2a5ff3a5277807b` (PR #62).
- APK source SHA: `3eb97e761bc9432e5a7bfebe769f2a580e4fe2b4`. The final evidence commit changes documentation only; the review HEAD is available on the PR.
- APK path: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\chapter01-respawn-map-repair-v1\Builds\Android\gravivore-dev-0.1.0+40.apk`.
- Version: `0.1.0`; versionCode: `40`; application ID: `com.gravivore.mobile.dev`.
- Size: **76,767,909 bytes (73.21 MiB)**.
- SHA256: `5865fc7f5f694bcb9fabd8be59356b193ad1e44194dd7dcac52e8264e883c2e2`.
- Unity: `6000.3.0f1`; IL2CPP; ARM64 only; DEV/debugging enabled; minimum Android API 26, target API 36.

## What changed

Ordinary and strong ordinary spots use one minimum 120-second first-kill wave clock. Later kills preserve its deadline. After expiry, the whole missing group waits for safe player distance, no active wave combat, and sufficient global/pool capacity. Relevance recycling preserves the deadline. Stats, rewards, progression, composition and the live cap remain unchanged. Magnetar and Custodian retain their separate encounter cooldown authorities.

Map markers project authoritative availability and remaining time: Available, Active, Cooldown, Ready and Locked. Relevant compact markers show a timer or ГОТОВО; the expanded map and selection show full availability. Both map surfaces draw actual world bounds and traversal-blocker footprints, open/closed gates, ordinary/strong engagement areas, the repair pad, elite approach and boss containment area. The existing north-up projection and HUD size/interaction are retained with an octagonal frame.

The recovery observer now animates two articulated manipulator arms, a scanner and a cyan repair pad. Existing healing events alone drive the presentation; healing balance is unchanged. Shared material instances, objects, pooled VFX and the repair audio source are bounded.

DEV-only diagnostics record the first 90 seconds, composition phases, significant wall-clock stalls, profiler availability/GC context, first enemy visual creation and hierarchy/material/VFX/audio counts. Fixed buffers and two session files permit comparison of the failing first launch and successful reopen.

## Executed verification

- Full Unity compile: exit 0.
- ProjectValidator: exit 0; also executed inside the Android build.
- Full EditMode: **436 passed, zero failed, zero skipped**.
- Full PlayMode: **120 passed, zero failed, one skipped**. The skip is the optional structural screenshot export requiring `GRAVIVORE_VISUAL_INTEGRATION_QA`.
- Coverage includes first-kill timing, no reset by later kills, camping/combat blocking, coherent safe return, cap/reward/save preservation, authoritative map time, strong ordinary waves, separate elite/boss cooldowns, gate/blocker topology, label bounds, repair authority/bounds, duplicate initialization and a real 90-second Editor farming/reload observation.
- Android ARM64 DEV build: exit 0.
- APK signature: `apksigner verify --verbose --print-certs` exit 0, v2 verified.
- Manifest: package/versionCode/DEV flag/launcher/minimum SDK/ARM64 checked with `aapt dump badging`.
- Native archive ABI is only `arm64-v8a`. IL2CPP metadata contains WaveRespawnState, TacticalMapTopology, TacticalHudFrame, RepairManipulatorPresenter and ColdStartDiagnostics.
- Accepted Chapter 01 production dependencies were verified before build and inside the serialized APK. The independent APK checksum matches the post-build dependency evidence.

Proof: [test summary](verification/test_summary.json), [EditMode XML](verification/EditMode.xml), [PlayMode XML](verification/PlayMode.xml), [APK verification](verification/apk_verification.json), [build metadata](verification/apk_build.json), [signature](verification/apk_signature.txt), [manifest](verification/apk_badging.txt), [packed dependencies](verification/apk_packed_dependencies.json). Complete command logs (trailing whitespace normalized) and internal QA captures are retained under this milestone directory.

## Cold-start conclusion and limitations

The recurring device slowdown remains a real, **unconfirmed device cause**. No claim of a confirmed fix is made. The focused 90-second Editor run recorded two early stalls (532 ms and 228 ms), then no further frame above 50 ms. An earlier fresh-import graphics run recorded a 2,677 ms first-presentation stall. Inventories remained stable across farming/reload: 3,913 transforms, 932 renderers, 59 shared materials, 20 particle systems and 13 audio sources. Twenty first-use ordinary visual creations took 0.11–1.09 ms each. Batch Editor render-thread data was unavailable and main-thread samples were zero, so these results cannot identify an Android shader/GPU cause.

The APK writes bounded `cold-start-current.json` and `cold-start-previous.json` to its actual persistent data directory and logs that path on Android DEV. It preserves the first startup frames, flushes on checkpoints/pause/quit and disposes recorders at 90 seconds. No speculative preload, shader warmup or artificial loading delay was added. See [the investigation](COLD_START_INVESTIGATION.md) and focused/full-suite JSON evidence in `verification/`.

No physical device install/playthrough was performed in this task. Frame rate and the original cold-start recurrence must be evaluated through the APK gate.

## Assumptions and changed files

Ordinary encounter cooldowns remain transient session state, preserving the existing progression save schema. Safe return distance derives from the furthest spawn anchor plus the existing enemy aggro-release radius or minimum spawn distance, whichever is larger. Map zone shading depicts engagement footprints; physical traversal comes from actual bounds/blockers/gates.

The complete source/configuration/test file inventory is in [source_files.txt](verification/source_files.txt). Main changes are under `Runtime/Gameplay/Enemies`, `Runtime/Presentation/Map`, `Runtime/Presentation/Composition` and `Runtime/Presentation/Development`; five S04 spawn assets, ProjectValidator, one EditMode suite, five PlayMode suites and `Tools/chapter01-gameplay-ux/run_unity.ps1` were added/updated. Documentation and verification evidence live here. Unrelated Unity serialization and older milestone evidence were restored before the delivery commit.

NEXT HUMAN ACTION: **INSTALL APK ON DEVICE AND PLAY CHAPTER 01.**

Next milestone: **CHAPTER 01 CONCEPT FIDELITY V2**, after the APK device gate.
