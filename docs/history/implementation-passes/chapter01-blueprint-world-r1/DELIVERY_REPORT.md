# Chapter 01 Blueprint World-only R1 — APK 48 delivery

Status: implementation, internal portrait review, automated validation and development APK build completed. PR #71 remains Draft for the owner's real-device world review.

## Branch and build identity

- Branch: `art/chapter01-blueprint-rebuild-v1`.
- Existing Draft PR: https://github.com/Shwedsky/gravivore/pull/71.
- Synchronized `origin/main`: `38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc`.
- Merged pre-implementation accepted V47 baseline: `db8828fdd33f157fc0e2591eaad387b906520175`.
- Built implementation commit: `d7786a73e03e8721e0f44737d4d06c5240744c27`. Subsequent delivery commits contain reporting/proof tooling only; the APK metadata records the implementation commit.
- Unity: `6000.3.0f1`; canonical rebuilt production scene: `Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity`.

## World replacement and preserved scope

The canonical scene now contains `Chapter 01 Blueprint World R1`, eight authored sector prefabs and a cross-sector service network. It replaces the previous physical world rather than overlaying it. Scene audit reports active V45/V46/V47 environment layers **0/0/0**, active repeated deck **0**, and old collision fingerprint required **false**. The post-build guard inspected length-prefixed object names in the actual APK scene data, found all eight new sectors, world root and service network, and rejected historical environment/deck names. See `verification/world-audit.json`, `verification/apk-world-serialized-sectors.txt` and `verification/build-evidence.json`.

The eight sectors are Repair Hub, Relay Yard, Capacitor Field, Cutting Floor, Shield Dump, Hauler Graveyard, Magnetar Complex and Custodian Core. Functional identities include maintenance/deployment bays, energy distribution, cutting press, armor processing, freight wrecks/crane, an amber induction pressure chamber and a larger red containment installation. All eight final 540 × 960 portrait views, two facility-walk views and the remapped map were reviewed before the Android build; see `INTERNAL_WORLD_REVIEW.md` and `internal/`.

Protected-scope comparison against the merged accepted V47 baseline passed for **645 files**, with no failures. G-0 anatomy/model/animation/equipment, Scout/Cutter/Warden/Arc Drone/Carrier models, Magnetar and Custodian models, M-0 weapon presentation, combat/VFX/audio, UI visual style, damage/HP/progression/rewards/saves and idle patrol source were not redesigned. Seven ordinary/elite/boss encounter configurations differ only in their authorized coordinate fields. Strong encounter multipliers, population/cooldown semantics and pre-/post-gate accessibility remain covered by tests. See `verification/locked-scope.json`.

## Assets, collision and map

Actual promoted CC0 donor families, with exact archive-entry hashes and license evidence recorded in `promoted-assets.json`, `asset-intake.json` and the updated third-party notices:

- MegaKit: Column_Astra, Column_Pipes, Door_Frame_A, Door_Frame_SquareTall, Platform_Round1, Prop_Vent_Wide, TopCables_Straight, WallAstra_Straight and WallAstra_Straight_Broken.
- Molten Maps: Catwalk, Centrifuge, Command Console, Cryo Tube ON, Generator, Generator Pile Large, Generator Pile Small and Wall Command.

The authored derivative world is supplied as ten runtime sector/service/gate FBXs, ten pure presentation prefabs, four controlled URP PBR materials, eighteen 1024 maps, a reflection cubemap and a portable Blender source at `art/blueprint-world-r1/Chapter01_BlueprintWorld_R1.blend`. Intake selections that were not used are excluded from promoted-family claims. Both owner-local canonical ZIP originals were preserved byte for byte.

World layout is data-driven: **12 movement surfaces, 76 authored architectural/perimeter blockers and 25 route points**, with existing explicit outer boundaries and progression-gate colliders. Minor decorative meshes have no gameplay collision. Actual player CharacterController traversal passed across the intended route; radius 0.42, height 1.4 and skin width 0.08 were retained. Elite placement also preserves the minimum five-second route to the boss. The old collision fingerprint is superseded by the new layout.

The minimap projects the new 54 × 134 world and uses the same ground union and actual blocker/gate authority as gameplay. Marker coordinates, route and sectors were remapped; frame, skin, heading, marker semantics and interaction were retained. Russian labels include «Комплекс Магнетара» and «Ядро Кустодиана».

## Executed checks

- Standalone compile: Unity exit **0**.
- ProjectValidator: Unity exit **0**; also executed during the Android build.
- Final EditMode: **502/502 passed**, no failures or skips.
- Full final PlayMode: **140/142 passed**, two test-fixture failures, no skips. After fixture-only corrections, the matching focused rerun passed **2/2**. All **142 unique cases** are resolved; original full-run and focused XMLs are retained. Runtime, assets and layout were unchanged after the full run.
- Coverage includes actual route clearance, all five ordinary encounter semantics, strong packs, Magnetar/Custodian progression, save/restore, respawns/population/idle patrol, minimap, equipment and preserved combat presentation.
- World combat/traversal soak: **300.0005491 seconds**, 68 reached waypoints, maximum 20 live enemies and 96 particles. A separate real-locomotion/combat/save soak completed **300.0010299 seconds** and all 14 stops. Cold-start and reload Editor evidence is retained.
- Renderer/material/shader audit and actual APK resource/scene packing guards passed. Required Lit/Particles/UI variants were recorded for GLES3 and Vulkan, including world PBR/normal/occlusion/emission variants. Missing world meshes/materials: **0/0**.
- Android build: **Success**, Unity exit **0**, ARM64 IL2CPP development/debuggable APK, versionCode **48**. Manifest and native-library checks independently confirm the requested package.

Test XMLs, executed-log hashes, native libraries, build metadata, capture hashes and APK hash are consolidated in `verification/delivery.json`; full local logs remain at the paths recorded in `verification/build-evidence.json`.

## Performance observations and limits

Static authored world inventory: **124 renderers, 803,699 triangles, four materials and one key light**. Existing bounded actor/VFX/atmosphere presentation is retained. The Android-import scene dependency texture inventory is 90,347,313 bytes; it includes preserved/transitive dependencies and is not a measurement of live world GPU memory.

The first soak recorded an Editor batch-loop mean of 1.5251 ms, a maximum interval of 6547.2266 ms and 29 Gen0 collections. The second real-locomotion run recorded a maximum interval of 407.0378 ms, constant material count 39, unchanged facility transform count 220 and bounded enemy population. Actor transform count grew 695→1169 during accepted actor-variant caching. These observations include Editor/test-runner overhead and do not establish zero runtime allocations or Android FPS. Capture frustum inventories can include occluded geometry, inactive LOD candidates and combined static mesh inventory; they are not measured GPU triangle counts or draw calls.

No Android device was attached during final verification. **60 FPS, touch traversal and the owner's concept-fidelity gate remain pending on device.** Internal screenshots and automated tests do not establish human visual acceptance.

## Delivered APK and preservation

- Owner delivery: `C:\Users\pamak\Documents\ChatGPT\gravivore\Builds\Android\gravivore-dev-0.1.0+48.apk`.
- Worktree build: `Builds/Android/gravivore-dev-0.1.0+48.apk`.
- Package: `com.gravivore.mobile.dev`, version `0.1.0`, versionCode `48`, ARM64 IL2CPP, DEV.
- Size: **95,416,825 bytes** (approximately **91.00 MiB**).
- SHA256: `3e9a76807adbba9e7a44e495eba4286a52c0dc0f2418698f7546736ac3fc65ad`.
- Build timestamp: `2026-10-09T23:15:13.8036619Z`.
- Matching `.build.json` and `.post-device.json` are delivered beside the APK.
- Version 47 APK and its metadata, plus both owner ZIP originals, match their captured pre-build SHA256 and sizes exactly; see `verification/owner-preservation.json`.

Files changed are listed in `verification/files_changed.txt`, relative to the merged pre-implementation baseline. Principal implementation groups are `BlueprintWorldR1` content, canonical scene/world definitions, explicit world layout/presenter, minimap topology, editor build/validation integration, regression fixtures, authoring tools, packed Blender source and provenance/report evidence.

Assumption: the approved blueprint controls macro-layout, sector function and visual hierarchy; exact new coordinates and collision footprints are authorized implementation choices. The unchanged production camera and accepted V47 occupants remain the reference presentation. The owner device/world review is the next task; no actor-concept pass is included here.

NEXT HUMAN ACTION:
INSTALL VERSIONCODE 48 AND REVIEW ONLY THE REBUILT WORLD:
ARCHITECTURE, SECTOR IDENTITY, CONCEPT FIDELITY, TRAVERSAL AND PERFORMANCE.
