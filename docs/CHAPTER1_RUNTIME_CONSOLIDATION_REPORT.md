# Chapter 1 runtime consolidation

## Source SHAs

- main: `43e0416f341e3b106ad976053746da540a1b85d4`
- PR #50: `aacc089c13b538ee5f25a4c6b73bdefb195868eb`
- PR #51: `1f199ec195b9291de63e2e7995e254be0acc1ae4`

The remote heads were fetched and verified before integration. They matched the
requested baseline. Both source histories were preserved with non-fast-forward
merges. PR #49 was excluded. main and both source branches remain unchanged.

## Integration

Branch: `integration/chapter1-runtime-consolidation`.
Workspace: `.codex-worktrees/chapter1-runtime-consolidation` under the original
checkout. The original checkout was not used for integration edits or tests.

Integration checkpoints:

- `c004930`: merge PR #50 playable presentation.
- `717956a`: merge PR #51 repeatable runtime.
- `10c3c8c`: production composition, authoritative map adapter, encounter lifecycle,
  Phase6B encounter/repair presentation, and crash recovery tests.
- `90c3a8e`: safe strong-pack admission, recovery checkpoints, production VFX
  handles, and compatibility test updates.
- `5770251`: repeat availability after reload and visible ground warnings/scanner.

No Git conflicts occurred. Semantic integration gaps required explicit production
wiring; mere presence of either module did not activate the combined runtime.
See [the complete changed-file manifest](evidence/chapter1-runtime-consolidation/FILES.txt).

## Production wiring

- **Map:** the real Chapter01 HUD explicitly initializes compact and expanded
  map surfaces. Bounds come from the authored gameplay world, including the
  Custodian area at z=94. The compact map is north-up and reads player heading.
- **Phase4 marker authority:** `Chapter1WorldMarkerMapAdapter` projects
  `Chapter1WorldMarkerAuthority` plus live population, actor, and permanent
  progression state. Its thirteen markers cover the player, five ordinary
  spots, four strong spots, both encounters, and Repair Hub. Encounter markers
  carry first-clear status, repeat availability, remaining cooldown, entitlement,
  premium repeats remaining, and window time. Map code grants no rewards.
- **Player:** the `GravityLashVfxPool` compatibility facade uses the injected
  Phase6B production asset and bounded audio/VFX pools. Windup updates mech pose
  before reading its socket. Gameplay still commits damage on release; expiry
  of a cosmetic charge never releases an attack. Beam/impact tails and cancellation
  clean up actual pooled instances.
- **Ordinary enemies:** one production observer handles nonlethal hits and
  shutdowns. Lethal hits produce one death presentation. Pooled life resets
  preserve existing population/respawn behavior and the configured cap of 25.
- **Strong ordinary:** four gameplay-defined side-route packs reuse existing
  enemy archetypes, pool, authored rewards, and visuals. Configurations scale HP,
  damage, and rewards and own independent adaptive pressure. Gate and player
  proximity govern admission. Five ordinary packs retain four enemies each;
  a nearby strong pack admits three, normally totaling 23.
- **Magnetar:** the encounter lifecycle reads permanent first-clear state and
  the shared repeat service. Phase6B consumes its exact telegraph, resolution,
  cancellation, damage, and defeat events, including the signature audio cue.
- **Custodian:** production uses injected repeat access and external durable
  defeat authority. Cone, line, and circle geometry receives exact gameplay
  direction, range, width/half-angle, and duration. Reset/death cancels warnings.
  Presentation never applies damage. Ground geometry has a configured cosmetic
  vertical lift above decorative decks; gameplay positions remain unchanged.
- **Repair Hub:** presentation observes actual `PlayerHealthController.Healed`
  events in the existing respawn/repair zone. One service voice and three bounded
  effects provide beam, sparks, and scanner. Leaving the zone, combat, death,
  full HP, or disable stops presentation. No HP rule was changed.
- **Save v2:** production starts `ProfileSession` with schema 2, automatic v1
  migration, effective UTC floor, and one `Chapter1EncounterRuntime` transaction
  coordinator. Pending recovery runs before new encounters become available.

Legacy S14 lash/enemy/encounter cues and the old encounter warning paths are
suppressed where Phase6B owns them. Compatibility APIs remain; unrelated player
feedback, haptics, assimilation, and evolution presentation continue. Production
initialization uses explicit references, without global scene-search bootstraps
or runtime `Resources.Load`.

## Phase4 adversarial review

- **Migration:** profile identity, stats, quests, equipment, offline state, and
  permanent historical clears are retained. Legacy kill times were absent, so
  historical clears migrate immediately repeatable. Existing corruption,
  backup, and optional-content restoration tests still pass. Fixed Unity JSON
  materializing null pending-reward DTOs as empty objects; entirely empty means
  absent, while partially malformed transactions remain rejected.
- **Time:** cooldown/window decisions share `max(wall UTC, persisted floor)`.
  Backward time cannot shorten cooldowns or roll a reward window backward.
- **Rewards:** production uses authored ordinary comparison units and the
  existing Phase4 reward ladder. First-clear permanence is committed with the
  reward. Stat-observer failures are logged after committed progression and
  cannot revert a transaction to Prepared.
- **Crash recovery:** durable Prepared precedes reward application; durable
  Applied precedes clearing. Failed final saves retain Applied in memory for
  retries. Fault tests cover all three writes, reload after Prepared/Applied,
  failed initial durability, and repeated recovery without duplicate rewards.
  These are simulated storage/process-restart tests, not physical-device kills.
- **Repeat caps:** Magnetar is 15 minutes, three premium repeats per anchored
  24-hour window; Custodian is 30 minutes, two. First clears use zero premium
  repeats. Fallback follows the cap, and repeats never complete Chapter again.
- **Strong spots:** inherited positions overlapped encounter acquisition areas.
  Gameplay catalog offsets now place elite-side and boss-side packs on distinct
  side routes. Tests check every anchor against both encounter zones and central
  traversal clearance. Distant admission returns enemies through the same pool
  while preserving killed-enemy timers and each spot's pressure; it grants no
  recycle reward. No positions came from visual prefabs.

## Unity

Unity `6000.3.0f1`:

- Final compile: passed.
- Final ProjectValidator: passed, exit code 0.
- Full EditMode: 382 passed, 0 failed, 0 skipped.
- Full PlayMode: 89 passed, 0 failed, 1 intentionally skipped out of 90.
  `VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested` needs
  its optional capture environment variable. The ten consolidation captures ran.

Local logs/results are under `Builds/Logs/compile-validator-final.log`,
`editmode-final.log/.xml`, and `playmode-final.log/.xml`.

## Runtime verification

The four consolidated production smoke tests load the actual Chapter01 scene.
They exercise visible compact/expanded maps, heading changes, all four strong
pack admissions, scaled HP/reward events, independent pressure and respawn,
Magnetar first clear/repeat/cooldown/map state, Custodian exact warning geometry
and timing, first clear/repeat without duplicate Chapter completion, durable
reload of reward windows/cooldowns, ordinary feedback/respawn, and repair
activation/termination from authoritative healing. Existing full PlayMode tests
also verify Gravity Lash charge/release damage timing and pooled reuse.

All ten [runtime PNGs](evidence/chapter1-runtime-consolidation/README.md) are
automated staged renders from this scene. Their provenance and limitations are
recorded with them. The boss warning images were visually inspected, including
the deck-occlusion correction.

## Android DEV build

DEV build succeeded, Unity exit code 0. `aapt` verified the actual manifest
application ID, versionCode, and ARM64-only native package. APK ZIP inspection
also confirmed `lib/arm64-v8a/libil2cpp.so`.

- Local path: `Builds/Android/gravivore-dev-0.1.0+2.apk` in the integration worktree.
- Size: 59,212,915 bytes (56.47 MiB).
- SHA256: `E604B37F7429D36A020204BCB7741E7A935E5AADBEEFC6565432B230A6F25434`.
- Application ID: `com.gravivore.mobile.dev`.
- Version: `0.1.0`; versionCode: `2`.
- Runtime/build git HEAD: `577025185b2320cda0d95da615243c63ced93943`.
- ARM64, IL2CPP, Development Build and AllowDebugging enabled.
- Build UTC: `2026-10-05T14:40:51.7805040Z`.
- Local build log: `Builds/Logs/android-dev-20261005-142658.log`.

The final reporting/evidence commit follows this build and changes documentation
only. Build provenance is preserved in the copied
[metadata](evidence/chapter1-runtime-consolidation/android-dev.build.json) and
[verification record](evidence/chapter1-runtime-consolidation/verification.json).
The APK remains local; no Candidate or release package was produced.

## Remaining issues and assumptions

Physical-device install/launch, touch usability, audio mix, warning readability,
sustained frame rate/thermal behavior, and Android OS process-kill recovery have
not been verified. No device performance claim follows from Editor smoke tests.
Existing Unity API-obsolescence warnings remain outside this integration scope.

Assumptions: strong packs use existing capacitor-field and hauler-graveyard
archetypes/reward units; only a nearby unlocked strong pack needs live admission,
so baseline ordinary populations remain intact. Repair Hub uses the existing
gameplay respawn coordinates and recovery radius. DEV version is 0.1.0 with
versionCode 2. No new art, paid dependency, backend, or Chapter 2 work was added.

Unity-generated import/settings normalization was excluded from the integration
commits; generated local settings and build outputs may remain in the isolated
worktree. The APK is a local DEV review artifact, not a release publication.
No draft PR was created: the GitHub connector returned transport errors and the
local GitHub CLI was unavailable. The integration branch is pushed for review.

Next milestone: device review of this integration candidate. No next spec ID is
defined after S20 in `specs/INDEX.md`.
