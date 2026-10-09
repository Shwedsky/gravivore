# Chapter01 Visual Replacement V3 — production integration

Status: Production integration and validation complete; Android packaging pending. Draft PR; owner device acceptance is required before merge.

Baseline: `9516026da7ff88dd982c43527cab04d4ca8a222b` (merged Free Asset Intake V2).
Branch: `art/chapter01-visual-replacement-v3`.
Delivery target: fresh ARM64 IL2CPP development APK, versionCode 44.

## Constraints

Reuse the existing `free-asset-intake-v1` worktree. Its ignored source payloads and local concept board were verified present before switching branches. Do not clean or delete this worktree.

Retain bipedal G-0, gameplay, camera, collision, progression and save authority. Production art must adapt to the existing contracts. Public dependencies must be CC0, original work, or attributed redistribution-compatible CC BY art; Asset Store and unknown-rights source files are excluded.

## Checkpoints

1. Branch persistence and draft PR — this checkpoint.
2. Shared materials, Repair Hub and representative adjacent environment; production-camera review.
3. Full environment and recognizable sectors.
4. Role-specific enemy presentation, including low broad Magnetar and industrial Custodian.
5. Visible mounted M-0 ranks 1–5, matching equipment imagery and causal attack origins.
6. Pooled VFX and Russian portrait UI with EXE attribution.
7. Production captures, structural/rendering/license audits, full tests, validation and five-minute soak.
8. Android v44 build, package/signature/shader/dependency verification and SHA256.

Each major checkpoint is committed and pushed. No completed validation or visual grade is claimed until executed and inspected. The PR stays draft and is not merged.

## Evidence and limitations

Implementation evidence, executed validation results, mobile budgets, assumptions and final APK metadata will be recorded here as work proceeds.

### Repair checkpoint

Production portrait capture inspected at 540×960. MegaKit's original trim UV detail is retained in a shared cold-metal atlas; fractured original panels and recessed grates add construction depth. Command machinery, pipe columns and terminated service runs fit existing occupied art footprints. Repair ring remains visible. The real capsule passed the accepted right-hand exit (x=0 to x=6). One PlayMode repair test passed, including flat floor bounds, supported URP materials and absence of decorative colliders/scripts. Collision authority fingerprint is unchanged.

The Molten payload contains no Container mesh despite the V2 role listing; the verified CC0 Essentials crate is substituted. CC0 proofs and hash receipts accompany production assets. Historical V43 captures remain comparison evidence, not new validation.

### Integrated production presentation

- Environment: worn MegaKit trim detail, fractured deck panels, grates, supported service runs, adapted columns, command machinery, generators, cold-metal boundary cladding, and containment apparatus. Existing broken perimeter, trenches, hulls, reactor and turbine landmarks remain. Replacement art fits existing proxy bounds; cladding stretch is capped at 8 to keep narrow donors credible and bounded.
- Enemies: Scout retains its light radial supports with extracted CC0 upper plating; Cutter retains paired forward blades with larger cutting surfaces and guards; Warden gains overlapping armor plus selective Trilobite plating; Arc Drone has an original angular shell and visible electrodes; Carrier gains loaded gantries and radiators. Magnetar gains broad magnetic banks, laminations, exposed stabilizers and a dorsal amber core. Custodian gains reactor shoulders, armor layers, heat racks, pressure services and asymmetric exhaust. Existing bone names, animation names, socket families and gameplay bindings remain.
- Equipment: five distinct M-0 meshes share one worn atlas. The external housing is lifted and moved outward on the existing R_TOOL socket, with increased presentation scale. Inventory events toggle cached rank meshes and move the same direct-child Muzzle to the selected emitter tip. The existing equipment service, item bonuses, reward transaction, rank cap and save schema are unchanged.
- UI: injected EXE frames, button contours, status rails and selected state; Kenney health-track utility; restrained cyan and hostile orange/red. The equipment panel displays a deterministic render of the actual rank geometry. Russian labels, button hit rectangles, safe area, map markers/timers and boss health logic remain. The EXE credit appears in the pause modal and complete notices are shipped in StreamingAssets.
- VFX: original industrial spark/energy masks replace the relevant shared particle textures. Beam masks retain a broad alpha cross-section for thin mobile lines; a separate shared material gives hostile impacts narrow metal sparks. Existing shaders, timings, causal source/travel/impact, capacities and particle limits remain. No stock VFX code, shader or light is imported.

### Executed production validation

Full EditMode: **471/471 passed** (`EditModeRelease.xml`). Full PlayMode: **136/136 passed**, no skips (`PlayModeRelease.xml`), including the optional full-chapter structural capture enabled for this run. The two production review tests also passed after aligning the attack capture target with the actual mounted emitter (`FinalCaptureReview.xml`). They verify rank 1–5 geometry switching, actual thumbnail selection, real muzzle charge/travel origin, impact destination, unchanged VFX instance count, rank-5 save/reload and equipped restore. **ProjectValidator passed**, including shader inclusion, all renderer materials and the new collision/license audit (`ProjectValidatorRelease.log`). **Production compilation passed** (`ProductionCompileRelease.log`). Android remains pending.

All twelve requested production-camera subjects (with three attack-phase frames), five structural views, and four comparison/review boards are retained under `docs/visual-replacement-v3/internal`. Internal review accepted the stronger layered construction, dense serviced perimeter, heavier elite/boss forms, external emitter and actual-model equipment display for device delivery. Before/after boards use archived V43 frames; differing encounter/UI state is visible and they are not pixel-identical performance comparisons. The local owner concept was inspected as visual authority and is not redistributed.

The real-time Editor soak ran **300.010 seconds**, completed **16** traversal stops and reached a maximum of **20** ordinary enemies. Shared materials stayed **57 → 57**; non-actor transforms stayed **5,082 → 5,082** and fidelity presentation transforms **397 → 397**. Existing lazy actor-art caches account for the actor transform increase. Combat granted assimilation and save flushes passed. The worst Editor frame gap was 482.086 ms during a run that included synchronous captures and concurrent source auditing; this is not a device FPS measurement. Android performance remains an owner-device gate.

The authority audit compares against the specified merged baseline: **360** gameplay/core/save/platform/camera/configuration files match, all **459** existing scene transforms retain their local pose and parent, and the three serialized gameplay components match. The sole permitted definition change adds the injected UI skin reference. Capsule reachability tests cover all ordinary/strong spots, locked and unlocked gates, the elite and boss arenas, and the Repair Hub's real right-hand exit.

License audit: 36 exact source hashes and 12 trim textures verified against retained V2 rights/evidence. Six allowed families are used; EXE attribution is shipped. Asset Store geometry and unknown-rights sources are excluded.

Authored static scene inventory: 1,377 renderers total, 884 enabled, 8 shared materials and one authored light. The sum across all enabled static instances in the entire chapter is 1,110,624 triangles; this is a whole-world inventory, not the visible-camera draw count. Runtime actor/VFX/UI inventory records 1,693 renderers, 57 shared materials and three lights. New static and weapon models total 57,833 source triangles, each with one material/submesh; five weapon meshes have 3,143–4,536 triangles. Enemy LOD0 ranges from 5,324 to 22,048 triangles, with 55% / 25% LODs and 7–17 bones. New shared surface atlases cap at 2,048; UI textures cap at 512; original VFX masks are 128. Existing VFX prefab counts/capacities, particle limits and transparent warning geometry are retained, with the bounded-pool test and soak covering reuse.

The raw FBX transform audit found scale-100 exports during iteration. Exports now use explicit metre units; the existing rendering guard was preserved and all affected EditMode checks pass. Intermediate failed logs are retained as iteration evidence and are superseded by the final successful XML.

Initial Android attempts assembled their APKs but the new postbuild guard assumed that static FBXs, expanded art prefabs and the scene itself appeared as ordinary packed-source entries. A complete source-list comparison identified 15 static FBXs and 15 renderer-only prefabs expanded into the scene; all required material, texture, UI and dynamic weapon paths were present. Typed inspection confirmed that Unity baked the 460 decorative renderers into combined scene meshes. The final guard keeps strict build-report checks for those runtime resources, rejects any supposedly static art prefab with scripts/colliders, records the expanded sources separately, and requires the production root in `level1` plus all 460 baked decorative MeshFilter references to resolve to actual class-43 meshes and production material objects in that scene. This preserves static batching and verifies the shipped representation. ProjectValidator and all 471 EditMode tests passed after the static-source correction; typed inspection also passed the direct scene-root checks. Packaging is being repeated from a pushed checkpoint.

### Assumptions and remaining acceptance

CC0 shells are donor material, not stock role replacements. Original independent machinery fills gaps where redistribution-compatible donors were unsuitable. Cladding may occupy less than its authority footprint when a narrow donor would otherwise require extreme stretch. APK and device performance are not inferred from Editor captures. The owner retains the A-/A visual grade and real-device rendering/readability acceptance gate. Next gate: V44 Chapter 01 device playtest, followed by draft-PR acceptance.
