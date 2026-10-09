# Chapter 01 full production APK delivery

Accepted baseline: device-approved merged PR #61,
`0152ec421eed53cc64c0b07fb10c4496589ae047`.
Branch: `production/chapter01-full-visual-v1`.
Draft PR: https://github.com/Shwedsky/gravivore/pull/62.
Final APK source: `1c4f25b16f00cc5d4dcaabe4a4bf4c45265d3ffc`.
Subsequent delivery commits contain verification evidence/documentation only.

## Resulting chapter

All five existing sectors use original production machinery and the accepted
industrial atlas/deck language. Relay Yard has supported antenna masts and routing
cabinets; Shield Dump has layered armor stacks and damaged defensive shells;
Capacitor Field has vessel banks, ceramic isolators, buses and transformers;
Hauler Graveyard has wheel chassis, gutted cargo frames and broken loaders.
Cutting Floor retains the accepted central slice and gains connecting service
machinery. Cyan, green, amber and muted violet identify their respective sectors
inside the common graphite/steel/pale armor palette.

The chapter has continuous segmented ground, approved deck aprons, service paint,
conduit racks, supported raised trunks, maintenance units and perimeter bulkheads.
The elite slice and its strong-spot service doorway are preserved. The final
approach/arena uses reinforced buttresses, pressure vessels and containment
architecture outside the readable combat circle. Gate flanks and the north/side
perimeters have continuous visible architecture, including a regression against
invisible collision-backed gaps. No authority position was relocated.

Arc Drone, Carrier and Warden now have distinct original machine anatomy: ducted
lift turbines/electrodes, wheeled cargo bogies/forks, and a broad convex shield
with hydraulic legs. Custodian is a larger six-legged containment engine with
pressure drum, crown and articulated restraint clamps. Each uses three rigid
skinned LODs, five in-place animation states and one opaque 256px atlas material.
Original Blender sources and authoring scripts are retained. Accepted G-0 tiers,
Scout, Cutter, Magnetar and Environment_Slice sources/prefabs/materials are unchanged.

## UI and audio

Custodian uses one dedicated HUD with name, visible health fill and current/max HP.
The ordinary overhead actor list excludes Custodian. Actual encounter regressions
cover full, half and low health, reset, death and the repeat encounter after
permanent first-clear completion. Reset refreshes the bar to full; zero HP hides it.

The Russian Characteristics screen is reachable from the existing pause menu.
Preallocated scrolling stat rows show real level, XP/threshold/bar, current effect,
next-level result and descriptions. Tapping opens sources/details. XP and reward
routes come from ProgressionState/ProgressionConfiguration; effect and next-level
results use PlayerStatsCalculator with live equipment modifiers. No UI formula
duplicates the gameplay curves. Reward, level, equipment, objective, admission and
save restoration updates are observed. Assimilation shows the total score and
separate contextual admission progress, including required objectives and completed
admission. A restrained stat-name/level confirmation uses an existing audio cue.

Step gain is .168 versus v38's .24 (30% lower); sustained cadence measures about
20% lower at 4.5, 6 and 7.5 m/s. Gait distance is 2.25m versus 1.8m and the minimum
audio interval is .36s versus .28s. The accepted mechanical/servo clip remains.
Changing the user's volume setting preserves individual cue gains. Combat,
telegraph and music gain settings remain independently balanced.

The existing north-up map retains marker authority and shows all five sectors,
player, repair hub, gates, four strong spots, Magnetar and Custodian. Its rendering,
selection and live availability behavior is preserved; no map camera or pathfinder
was added.

## Executed validation

- Unity compilation/integration: passed in 6000.3.0f1; final standalone compilation also exited 0.
- ProjectValidator: passed, including final validation and the Android pre-build validation.
- Full EditMode: 431 passed, 0 failed, 0 skipped.
- Full PlayMode: 114 passed, 0 failed, 1 baseline opt-in structural capture skipped.
- All five normal centers and twenty spawn anchors, four strong spots, elite/gate approaches and boss arena: swept-capsule connectivity in locked/unlocked states; 33 unlocked targets traversed with the real CharacterController.
- Every new obstacle proxy: enabled, HardBlocker layer, matching world center/size, separate from collider-free art. Raised arches retain usable doorways.
- Boss HUD lifecycle, calculator/XP/reward authority, equipment/live/save UI, Russian text and text overflow: passed.
- Existing save/reward/repeat/respawn/gate/telegraph tests and 64-kill bounded UI/VFX/audio/hitch regressions: passed.
- Internal 540x960 renders: five sectors, traversal, final approach, full boss bar, full map and Characteristics inspected.
- Preservation audit: accepted art, balance/config/layout, combat/encounter mechanics and persistence sources match the accepted baseline.

`VALIDATION_COVERAGE.md` maps all 33 requested checks to executed tests. XML,
sanitized logs, controller routes, cadence counts, source-file list, preservation
audit and APK verification records are in `verification/`.

## Files changed

- `Assets/_Game/Content/Chapter01Production/`: seventeen original FBXs, atlas, four actor controllers, actor/static prefabs, five sector packages and full environment.
- `art/chapter01-production/` and `Tools/chapter01-production/`: Blender sources, reproducible authoring/integration/build/verification workflow.
- Canonical chapter scene, presentation catalog/integration definition, S14 step configuration and PostDevicePresentation gait configuration.
- Characteristics read model/presenter; boss, pause, stats and enemy readability presenters; animation bridge, mech gait/audio, world presentation and composition wiring.
- Three read-only domain access additions for calculator preview and reward/config inspection. Reward calculation, balance and persistence behavior are unchanged.
- New production EditMode/PlayMode cases and updated canonical-binding/isolation/collision/HUD expectations. Exact build-source paths are listed in `verification/source_files_changed.txt`.

## Assumptions and limits

The isolated worktree preserves the dirty primary source checkout. Inactive,
regenerable Unity Library caches from two previous worktrees were removed to make
disk space; their source files and previous APKs were retained. Unity-generated
importer/settings and prior-test evidence churn are excluded from the PR. Existing
ProjectConfigurator creates the required platform/URP configuration for builds.

Presentation offsets keep solid machinery clear of unchanged spawn/objective
anchors. Conservative box proxies cover each major solid prop; raised arches use
separate support/truss boxes. Service turns are allowed instead of requiring empty
straight prototype diagonals. The new regression actually walks the routes.
Machine display designations use Cyrillic «М»/«Г» for Russian-only text; runtime
IDs remain unchanged. No save schema, reward/threshold, population, quest, gate,
movement, boss mechanic or repeat-rule redesign was made.

Device installation, touch usability, sustained Android FPS and perceived audio
loudness are not measured here. Static batching flags, shared opaque atlases,
actor LODs and bounded pools are retained; no new realtime light, per-prop Update,
package, external art sample or paid dependency was added. This report makes no
claim of measured 60 FPS on a physical phone.

Next specification/gate: install the complete Chapter 01 APK and perform the
owner's device playthrough. No Chapter 2 or other subsequent feature was started.

## APK artifact

The final verified artifact fields are recorded in `verification/apk_verification.json`.

Version 0.1.0, versionCode 39; `com.gravivore.mobile.dev`; Unity 6000.3.0f1;
ARM64-only IL2CPP DEV, debugging enabled. Android build exit 0.

Exact APK path:
`C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\chapter01-full-visual-v1\Builds\Android\gravivore-dev-0.1.0+39.apk`

Size: 76,816,250 bytes (73.26 MiB).
SHA256: `a51895b3245866c8aa6203d1444be8dbf6d5fb1d92743bd9af6db0f0887113c5`.
APK source SHA: `1c4f25b16f00cc5d4dcaabe4a4bf4c45265d3ffc`.
Signer SHA256: `a5bb4e5fc349073f8f34233bcfb206b13c7cc2ba222f3ce608cce9d7728f6454`.
Signature verification passed and matches accepted v38 for update installation.

Build callbacks verify 23 new serialized model/environment/package entries,
fourteen accepted visual/music entries and ten accepted audio/settings entries.
Direct APK inspection confirms compiled Characteristics/boss/readability/hitch
types and methods, Russian panel strings, named serialized step gain .168,
minimum interval .36, cycle distance 2.25 and unchanged bounded UI capacities.
All dependency evidence matches the exact delivered SHA256. This is APK archive
verification, not a claim based only on Editor dependencies.

INSTALL APK ON DEVICE AND PLAY THE ENTIRE CHAPTER 01.
