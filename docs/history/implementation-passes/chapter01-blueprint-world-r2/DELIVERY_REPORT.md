# Chapter 01 world R2 — presentation convergence

Status: R1 failed the owner's device visual acceptance. R2 retains its technical topology and reworks the world presentation toward the owner-approved Chapter 01 Visual Blueprint. Internal eight-sector portrait review, automated checks and APK 49 build/delivery are complete. Owner device visual acceptance remains pending. PR #71 stays Draft; do not merge.

## Branch and scope

- Same branch: `art/chapter01-blueprint-rebuild-v1`; same Draft PR: https://github.com/Shwedsky/gravivore/pull/71.
- Frozen R1 technical reference: `67236348a2be8732f5fbd88eb2093da18f5f7251`.
- Main already synchronized for R1: `38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc`. R2 does not rebuild or resynchronize gameplay topology.
- Built R2 source commit: `0d20154c0cf87d516078eb8c03ec2e508858bace`. Later delivery commits contain proof/reporting only.
- Unity: `6000.3.0f1`; production scene: `Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity`.

The presentation is replaced in the existing `BlueprintWorldR1` runtime asset paths, with the scene root renamed `Chapter 01 Blueprint World R2`; there is no second world overlay. `BuildR2` skips layout creation and encounter remapping. R1's 12 movement surfaces, 76 architectural blockers, 25 route points, sector locations, progression gates, encounter semantics and functional 54 × 134 minimap remain authoritative. Actual player CharacterController traversal passed all 24 route segments.

Protected-scope comparison against the R1 delivery reference passed for **1,568 tracked files**. Runtime/gameplay/persistence, content outside the allowed world presentation, G-0 source and the R1 layout are frozen. Layout/definition bytes are also compared directly after line-ending normalization. G-0, Scout/Cutter/Warden/Arc Drone/Carrier, Magnetar/Custodian occupants, M-0, UI skin, damage, HP, rewards, progression, saves, idle patrol and combat/VFX/audio were not redesigned. See `verification/preservation.json`.

## What changed

The material hierarchy now separates dark graphite structure, exposed machinery steel, recessed service metal, worn/painted deck cassettes, work-zone markings and contained cyan/amber/red technology. Sixteen purpose-specific PBR cells carry seams, inspection panels, machining, grates, paint and wear tied to hardware. Native trim colors are reworked while donor UV/PBR relationships remain intact.

Large deck assemblies use replaceable panels, flush service bands, grates and connected cable/pipe routes. The continuous R1 top strip that hid sector floors is removed from presentation; the movement-surface union remains R1. Supported machinery is concentrated along edges and overhead layers, while central combat lanes stay open. The visual roof/frame changes expose function at the fixed portrait camera rather than adding featureless gray masses.

- Repair Hub: cyan diagnostics, cleaner service deck, repair machinery and overhead service framing.
- Relay Yard: open deployment bay, antenna/comms hardware, carrier rails and lighter assembly machinery.
- Capacitor Field: tallest contained cyan energy vessels, grounding/distribution hardware and electrical routing.
- Cutting Floor: stepped heavy press crown, hydraulic supports, hot work jaw, production rails and heat-stained work cells.
- Shield Dump: layered armor masses, clamped shield cassettes and restricted service pockets.
- Hauler Graveyard: asymmetrical damaged cargo shells, open frames, displaced lids, severed hydraulics and loading hardware.
- Magnetar Complex: amber curved induction machinery, magnetic yokes and pressure heads around the open elite fight pocket.
- Custodian Core: red containment architecture, interlocked stepped cheeks, pressure vessels and hot/cold service routes behind the preserved boss occupant.

Lighting uses a darker ambient/reflection base and sixteen static local light pools around machinery, plus one directional key. All pools are unshadowed, ranges are bounded to 9/6.5 m, and URP retains four additional lights per object. Emission is localized. This adds real pixel-lighting cost and requires device profiling.

## Actual-camera review and provenance

All eight final 540 × 960 sector views, two facility-walk views and the map were captured before the APK build and inspected. Ten matching no-UI copies temporarily disable Canvas visibility only; production follow-camera parameters and occupants are unchanged. There are **21 final PNGs**. See `NO_UI_GALLERY.md`, `INTERNAL_WORLD_REVIEW.md` and `internal/`.

The first two R2 iterations were internally rejected for clean dominant deck fields, insufficient light contribution, repeated chamfered panels and closed cargo shells. The final candidate includes smaller panel corner cuts, connected overhead services, open cargo remnants and stronger local contrast. Internal review selects an APK candidate; it does not claim owner approval.

The same selectively promoted CC0 donors are reused: nine Quaternius MegaKit and eight Molten Maps model families. Exact canonical archive-entry/license evidence is retained from R1; no new raw source is introduced. `promoted-assets.json` records current hashes for ten runtime FBXs, eighteen surface maps and the packed editable `art/blueprint-world-r2/Chapter01_BlueprintWorld_R2.blend`. R1's editable source and prior APKs remain available. Donors are reworked into authored facilities rather than presented as untouched kit modules.

## Executed checks

- Standalone compilation: Unity exit **0**.
- ProjectValidator: Unity exit **0**; also required by the Android build.
- Full EditMode: **502/502 passed**, no failures or skips.
- Full PlayMode: **142/142 passed**, no failures or skips, including structural captures.
- The first full PlayMode launch exposed a missing test-file `System.Linq` import. It was corrected before the final full 142-case run; the initial failed local log is retained.
- Coverage includes actual route traversal, ordinary/strong encounter semantics, elite/boss progression, save/restore, respawn/population/idle patrol, minimap, equipment, scene wiring and preserved combat presentation.
- World combat/traversal soak: **300.0008046 seconds**, 62 reached waypoints, maximum 20 live enemies and 96 particles.
- Separate real-locomotion/combat/save soak: **300.0004766 seconds**, all 14 stops, maximum 20 live enemies; materials remained 39→39 and facility transforms 228→228.
- Cold start: **90.0044259 seconds** observed; reload regression evidence retained. These are Editor checks.
- Android build: **Success**, Unity exit **0**, ARM64 IL2CPP DEV/debuggable, versionCode **49**. Manifest and native-library checks independently passed.
- Actual APK resource/scene packing and render dependency guards passed. All eight sectors, the R2 root and service network are present in serialized APK scene data; historical world/deck roots are absent. Required world/actor assets and Lit/Particles/UI shader variants were packed for GLES3 and Vulkan, including normal/metallic/occlusion/emission variants.

Test XMLs, log hashes, captures, preservation and actual APK checks are consolidated in `verification/delivery.json`. Full local execution logs remain at the paths recorded there; Android log: `Builds/Logs/android-dev-20261010-175307.log`.

## Performance limits and device gate

Static authored world: **132 renderers, 934,657 triangles, four materials, 17 world lights**. Missing meshes/materials are **0/0**. Historical V45/V46/V47 environments and repeated deck layers are inactive. Android-import scene dependency textures total **90,347,313 bytes**; this includes preserved/transitive scene dependencies and is not live GPU memory.

The first soak recorded an Editor batch-loop mean of 1.02868 ms, a maximum interval of 495.16559 ms and 29 Gen0 collections. The second recorded a worst interval of 61.527 ms and actor transforms 695→1189 during preserved actor caching. Cold-start/reload worst intervals were 174.3575/151.0597 ms. These measurements include Editor/test-runner work and do not establish zero allocations or Android GPU/FPS. Capture frustum inventories are not measured GPU triangles or draw calls.

Large facilities can still leave the frame when approached closely; the review checks exposed silhouette/function/lighting/mechanical detail without requiring the entire installation in every follow-camera position. Android exposure, thermal behavior, touch traversal, 60 FPS and the owner's concept-fidelity judgement require device review. Automated green tests do not replace that gate.

## APK and original preservation

- Owner delivery: `C:\Users\pamak\Documents\ChatGPT\gravivore\Builds\Android\gravivore-dev-0.1.0+49.apk`.
- Worktree build: `Builds/Android/gravivore-dev-0.1.0+49.apk`.
- Package: `com.gravivore.mobile.dev`, version `0.1.0`, versionCode **49**, ARM64 IL2CPP, DEV/debuggable.
- Size: **97,793,249 bytes** (**93.26 MiB**).
- SHA256: `3d2fa87939418c267ed1e79acb327b8c12c145c66045ee9dd0c6cb72663f44a0`.
- Build timestamp: `2026-10-10T17:55:10.8178997Z`.
- Matching `.build.json` and `.post-device.json` are delivered beside the APK. Owner/worktree APK hashes match the actual post-build audit.
- No Android device was connected during final `adb devices` verification. Device visual/FPS/touch acceptance remains pending.

Six captured owner originals match their pre-build sizes/SHA256 exactly: both canonical ZIPs and APK/build metadata for versions 47 and 48. Only new version 49 and its metadata are delivered to the owner's `Builds/Android/`; `verification/owner-preservation.json` records the successful final comparison.

Files changed are listed in `verification/files_changed.txt` against the frozen R1 delivery reference. Main groups: world FBXs/PBR surfaces/prefabs, canonical scene lighting/environment, editor author/build audits, presentation smoke fixtures, R2 author/proof tools, packed Blender source, provenance and execution evidence. No gameplay/runtime implementation is added by R2.

Assumptions: versionCode 49 is the next development candidate; R1's accepted technical topology and fixed production camera are retained. Next task: **owner device review of APK 49 for world architecture, image-only sector identity, Blueprint convergence, traversal and performance**. PR #71 remains Draft and unmerged.
