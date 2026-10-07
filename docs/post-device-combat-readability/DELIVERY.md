# Post-device combat readability APK delivery

Baseline: merged PR #60, `2369772a0dd897c3ad03e31adc56959a8b2406d6`.
Isolated branch: `polish/post-device-combat-readability-v1`.
Draft PR: https://github.com/Shwedsky/gravivore/pull/61.
APK source checkpoint: `875853594d7835cb2fe86cddaa0f21f05e4f1256`.
Subsequent delivery commits contain evidence/documentation only.

The primary checkout and previous APK were preserved. All implementation and
validation used the isolated `post-device-combat-readability-v1` worktree.

## Changes

- The strong spot at (18,64) keeps its original location. Two shortened wall
  modules, a raised conduit header and deck markings create a visible four-metre
  service portal. Matching solid blockers leave the doorway clear; a maintenance
  prop moves one metre clear of its approach. All four active strong spots pass
  capsule-clearance path checks with normal progression gates opened, and the
  actual CharacterController traverses the east portal to the unchanged spot.
- Movement keeps its established facing authority and temporary attack aim.
  At rest a live valid target controls facing through cooldown/recovery, with
  angular smoothing. Changing targets updates it; ending combat preserves it.
  Joystick semantics and target selection are unchanged.
- Scout/Cutter/strong ordinary and Magnetar enemies have compact current/max HP,
  immediate health bars and actual stat-XP/assimilation previews. Ordinary red
  and strong/elite amber identity preserve the industrial style; the boss keeps
  its existing numeric HUD and receives an encounter reward preview. Six plates
  limit clutter, suppress overlaps and avoid the minimap. Modal menus hide them.
- Ten damage and four reward floating slots are preallocated. Damage comes from
  `DamageResult.AppliedDamage`, including a mitigated-damage regression. Kill
  feedback observes committed ordinary progression or applied encounter reward
  transactions; retry/recovery does not replay it. Values and populations remain
  unchanged. “ОП” denotes stat experience, rather than granting whole stat levels.
- Actual planar velocity controls player run playback; short transitions,
  modest lean and bounded visual foot contact improve stride impression.
  Presentation never moves the player authority or enables root motion.
- Nine movement/servo/lash WAV masters are project-owned synthesis. Player
  attack/step mix is restrained. Background music, enemy/elite/boss telegraphs,
  visual models, tier growth, gates, saves and repeat encounters remain baseline.
- The device stall is not reproduced. The lifecycle audit and 64-kill soak
  found no runaway presentation-object/audio/VFX growth. Reinitializing S14
  audio now fails before allocating another voice pool. DEV-only diagnostics
  retain 64 frame intervals and four rotating snapshots at a 250 ms threshold
  and 20 second cooldown. See `PERFORMANCE_AND_AUDIO.md` for details and paths.

## Validation

- Compile: passed in Unity 6000.3.0f1.
- ProjectValidator: passed, exit 0.
- Full EditMode: 414 passed, 0 failed, 0 skipped.
- Full PlayMode: 109 passed, 0 failed, 1 skipped (110 total).
  The skipped case is the baseline opt-in structural capture test
  `VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested`.
- Focused presentation regressions: eight passed. Full suites include all of
  them plus existing save/repeat/gate/respawn/telegraph tests.
- Internal 540x960 render verified numeric HP and both reward lines fit and
  actually render glyphs. HUD stacking, modal hiding and non-blocking UI are
  covered. `internal/combat.png` is verification evidence, not a human gate.
- ARM64 DEV APK: passed, Unity exit 0, versionCode 38.
- APK verification: passed. ARM64-only IL2CPP native libraries; valid signature
  and the same certificate as accepted versionCode 37. All nine model sources,
  fourteen accepted visual/music entries and ten new settings/audio entries are
  verified against the delivered APK SHA256. The IL2CPP metadata contains the
  combat UI, applied-damage event and DEV hitch diagnostics runtime types.

Earlier attempts exhausted disk during native linking and Android packaging.
Reversible compression of generated native/symbol caches recovered space without
changing source or previous APKs. A subsequent APK was produced but the new
checker initially overlooked Unity's split archives and GUID-named streamed audio
metadata. Direct archive inspection found all ten required entries; the corrected
checker covers both formats and excludes IL2CPP class metadata as false positives.
The final build passed that checker. Attempt logs are retained.

Structured results, XML, sanitized Unity logs and source-file list are in
`verification/`. Existing unrelated Unity importer/serialization output is not
part of this change. No new package, third-party audio sample, progression schema
or balance change was introduced.

## Artifact

Version: 0.1.0, versionCode 38, application ID `com.gravivore.mobile.dev`.

Exact APK path:
`C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\post-device-combat-readability-v1\Builds\Android\gravivore-dev-0.1.0+38.apk`

Size: 66,987,925 bytes (63.88 MiB).
SHA256: `b23ad0e1572cea0a6841e0df33c8b2542f4c9bc19e52f87f712b2e1ded73303b`.
Source SHA: `875853594d7835cb2fe86cddaa0f21f05e4f1256`.
Unity: 6000.3.0f1; development build; ARM64; IL2CPP.
Signer SHA256: `a5bb4e5fc349073f8f34233bcfb206b13c7cc2ba222f3ce608cce9d7728f6454`.
Structured proof: `verification/apk_verification.json`, `apk_badging.txt`,
`apk_signature.txt`, `build_metadata.json` and packed dependency JSON records.

## Assumptions and limits

Reward labels display the existing configured stat experience and assimilation
score. Strong multiplier rounding and first-clear/repeat encounter ladders share
the authoritative grant paths. The existing boss HP HUD is sufficient and is not
duplicated. Relevant nearby/recent/selected enemies receive capped annotations.

Physical Android installation, speaker timbre, perceived stride/foot contact,
touch review and sustained device FPS have not been measured here. The reported
one-off severe stall remains unconfirmed; its next occurrence can produce DEV
snapshots in the app's persistent `hitch-diagnostics/` directory.

Next specification/gate: the requested post-device APK installation and gameplay
review. No Chapter 2 or other subsequent feature specification was started.

INSTALL APK ON DEVICE AND PLAY IT.
