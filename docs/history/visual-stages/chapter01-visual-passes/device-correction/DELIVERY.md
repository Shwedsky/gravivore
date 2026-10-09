# Visual slice device correction — APK v2

Continues the owner's accepted SOLID B APK-v1 on the same branch
`integration/first-visual-slice-apk-v1` and Draft PR
https://github.com/Shwedsky/gravivore/pull/60. The target is B+/A- quality; that
grade requires the owner's next phone review. No merge or intermediate art gate.
APK-v1's delivery and build evidence remain in `docs/first-visual-slice/`.

## Delivery identity

Status: **VISUAL SLICE DEVICE CORRECTION APK READY**.

- APK source commit: `5895497dca108a1e69b0e477b74bd470d21423f4`.
- Exact APK path: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\first-visual-slice-apk-v1\Builds\Android\gravivore-dev-0.1.0+37.apk`.
- Version: **0.1.0**; versionCode: **37**.
- Size: **66,765,975 bytes (63.67 MiB)**.
- SHA256: `3a6c4359e02181a48f660a99f2ad2d23e3c9c5d29843ee85a5f16e134e8622b2`.
- `com.gravivore.mobile.dev`, Unity **6000.3.0f1 / 6.3 LTS**, DEV,
  debugging enabled, IL2CPP, ARM64 only.
- Signature verified, including APK Signature Scheme v2. The signer matches
  accepted APK-v1; the same package with a higher versionCode supports updating
  that installation.

The final documentation checkpoint follows the APK source commit and does not
alter runtime source. `apk_build_metadata.json`,
`verification/apk_verification.json` and `apk_packed_dependencies.json` record
the build identity and bind the production packing evidence to this exact APK.

## Corrections included

- G-0's imported visual front is Unity -Z. The imported model child receives
  180 degrees of local yaw; gameplay authority, joystick semantics and sockets
  retain +Z. Eight sustained movement directions and attack facing are covered
  across all three tiers.
- Visual scale versus APK-v1 is 1.15 / 1.22 / 1.30 for Tier 0 / 1 / 2. Existing
  evolution thresholds and stats remain authoritative. CharacterController
  radius .42, height 1.4 and center (0,.7,0), attack range, camera and world layout
  are unchanged.
- Tier 1 adds articulated shoulder, side, waist and core containment armor.
  Tier 2 adds heavier shoulder caps, dorsal blades, tool and knee armor. These
  original chamfered meshes use rigid weights on the existing 18-bone G-0 rig,
  one shared industrial atlas and the base mech's three LOD levels. Armor LOD
  triangle counts are 1360/760/394 and 2368/1326/686. Duplicate imported LOD
  groups were removed so every renderer has one LOD owner. Approved source
  geometry and its atlas are preserved.
- Custodian access checks govern admission from Dormant. A transient access
  change cannot reset or disable targeting during an active encounter. Existing
  arena exit grace, player death and explicit reset paths remain. The regression
  damages the boss to about half HP, interrupts access, exits briefly, returns,
  then exceeds grace; the invalid cases preserve HP/position and the final case
  causes exactly one reset.
- Minimap receives a dark tactical frame, restrained grid, heading chevron,
  smaller marker hierarchy, Russian zone footer and two gate markers observing
  existing gate state. North-up, projection and existing marker authority remain;
  the inner map stays square inside the taller footer frame. No map camera,
  RenderTexture, pathfinding or fog of war was added.
- Two original 72-second stereo music loops provide exploration ambience and a
  synchronized combat layer. Streaming Vorbis, 2.8-second fades, combat hold,
  music ducking during warnings and softer SFX levels create a coherent mix.
  Telegraph audio reserves a voice so ordinary hits cannot steal it. Music is
  procedurally composed from project-owned oscillators/noise without samples.
- The existing industrial kit gets cool steel variation, dark floor separation,
  amber reactor energy, yellow hazard details, restrained rust panels and cyan
  service paint. Static opaque floor markings break broad flat areas. Encounter
  density, layout, collision proxies and normal enemy population are preserved.
- Pulled enemy visuals converge to the authoritative destination over .18 seconds
  with maximum offset 1.35 meters, short body lean and a thin gravity streak.
  Cached actors and pooled lines reset across deaths/respawns; gameplay roots,
  hit sensors and damage remain authoritative.
- Repeated player attacks cycle through snap lash, wider traveling pulse and
  curved arc presentations using the existing pools and attack timing. Scout
  straight strikes, Cutter sweeps and Magnetar's short heavy impact cue add
  identity. Elite/boss danger-zone geometry, shape, fill and timing are preserved.

## Validation

- Unity 6000.3.0f1 integration and compilation: exit 0.
- Independent ProjectValidator: exit 0.
- Full EditMode: 411 passed, 0 failed, 0 skipped.
- Full PlayMode: 102 passed, 0 failed, 1 optional capture skipped; 103 discovered.
  `CaptureStructuralFoundationWhenRequested` requires
  `GRAVIVORE_VISUAL_INTEGRATION_QA`. The device correction's camera test did run.
- AndroidBuild.BuildDev: **Success, exit 0**, after disk recovery. Project
  validation also ran inside the successful build.
- APK inspection: expected package/version, debuggable manifest, only
  arm64-v8a native libraries, IL2CPP player and verified signature.
- Nine required scene dependencies and nine model sources validated. Fourteen
  exact serialized entries were found in the actual APK, including both tier
  armor meshes, both music clips, device correction configuration/motion
  material, G-0/enemies and the environment/service markings. This evidence
  matches the delivered artifact's SHA256.
- Regression coverage includes facing, tier growth, unchanged collider, boss
  grace, pull boundedness/authority, music initialization/fades/mute, tactical map,
  three attack variants/pool reuse, telegraphs, respawns/deaths, gates, repeat
  encounters, Russian HUD and save/reload flows.
- Internal 540x960 captures of all three tiers use the gameplay camera and Russian
  HUD. They were inspected to check silhouettes, scale and local material
  hierarchy. These are internal QA, not an additional owner approval gate.

The first full PlayMode run exposed stale presentation expectations and duplicate
armor LOD ownership. Both were corrected; the final full run above passed.
Verification logs redact licensing identifiers and normalize whitespace.

## Files and original sources

`changed_files.txt` lists this correction pass's committed source/evidence paths.
Main changes are the Custodian controller; presentation audio/VFX/map/composition;
G-0 tier and environment assets; EditMode/PlayMode regression tests; and the
visual slice builder, dependency verifier and APK verification tools.

Editable originals are `art/first-visual-slice/G0_Tier1Armor.blend`,
`G0_Tier2Armor.blend` and `Deck_ServiceMarkings.blend`. Reproduction scripts are
`Tools/first-visual-slice/build_evolution.py`, `build_assets.py`,
`build_markings.py` and `compose_music.py`. All newly authored geometry, textures
and music are original project work. Prior source attribution remains intact.

## Limits and assumptions

- Phone FPS, speaker balance, touch feel and final B+/A- acceptance require device
  play. Automated editor captures and structural counts do not establish 60 FPS.
- The requested scale targets were retained exactly after internal camera QA.
  Cosmetic pull lag is intentionally capped at .18 seconds / 1.35 meters.
- The same playable slice is retained. No Chapter 2, progression redesign,
  monetization, backend or full-chapter art replacement was started.
- The dirty primary checkout was preserved. NTFS compression recovered space
  only in this isolated worktree's generated Library caches; no source or prior
  APK was deleted. Build attempts exhausted C: during native linking, debug
  symbol extraction and Gradle's native library copying. Only verified generated native copies and Gradle outputs
  inside this worktree's `Library/Bee` were cleared, and completed native cache
  binaries received stronger Windows compression. Failure logs are retained
  alongside the final build log.

Next gate: **INSTALL APK ON DEVICE AND PLAY IT.** No later spec is started before
that device review.
