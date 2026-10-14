# Energy Chamber — final runtime presentation pass V1

**ENERGY CHAMBER FINAL RUNTIME PRESENTATION — READY FOR HUMAN DEVICE REVIEW**

Same Draft [PR78](https://github.com/Shwedsky/gravivore/pull/78), branch `integration/chapter01-architecture-production-v1`; keep unmerged. APK55's architecture/modeling/composition remains human-accepted. Final LookDev and device performance acceptance awaits the owner's review of APK56.

## Android delivery

- Path: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\chapter01-architecture-production-v1\Builds\Android\gravivore-dev-0.1.0+56.apk`
- Version **0.1.0**, versionCode **56**, Dev, portrait, ARM64 IL2CPP, `com.gravivore.mobile.dev`.
- Size **143,808,070 bytes** (137.15 MiB).
- SHA-256: `fbe8df66e96fb169477fe97d957570a31a5d72cd9de5e8b51962f9ff7d7b9ef8`.
- Actual runtime build source: `339a8bcf1852b7c19e02b4f0cb291362ce0ed125` (checkpoint C). Later changes are test evidence/reporting only; runtime art/shader/prefab code has not changed after that build.
- APK ZIP integrity, ARM64 `libil2cpp.so`/`libunity.so`, packed new shader, both production-only trims, chamber/core/node and collision serialization verified from the actual binary. `apk-badging.txt` confirms version56, API26 minimum / API36 target and portrait feature. Original SDK remains untouched; its previous selection was restored after the installed-SDK-mirror build.
- APK55 remains at its original path, **146,287,814 bytes**, unchanged SHA-256 `0b9ee3dc31185f6084bc8c961637e7624375404e013f0dd16c5f6cfa81593ffa`. It was not overwritten or cleaned.

## Visibility issue and exact correction

Root cause: the production LODGroup removed complete assemblies too early after Android's **.7 LOD bias**. Former editor evidence used bias **2**. South/west gameplay selected LOD1 (16 omitted assemblies), and the wider approach selected LOD2 (30 omitted assemblies). The same view's diagnostic-only LOD0 control restored them while their bounds stayed inside the camera frustum. All Renderers remained active/enabled, materials opaque/queue2000, with zero nested LODGroups and no runtime toggles. No bounds defect or repair was found.

Affected at LOD1: `EC_FoundationPolish`, `EC_ChamberPolish`, four `EC_NodePolish`, four `EC_ConnectorPolish`, two `EC_NodeVariant`, four `Service quadrant safety rail` Renderers. LOD2 additionally removed four generators, four centrifuges, four bridge pipe-holder supports and two stair Renderers. [Checkpoint A](CHECKPOINT_A_DIAGNOSIS.md) and its inventory give every exact hierarchy path, original membership, distance cause and proposed correction.

Thresholds **.38/.30/.08 → .26/.13/.035**. LOD1 retains the guard, top/caps, node frames/detail, major machinery/support and asymmetric side details. LOD2 retains guard silhouette, stairs, chamber top/caps and node outer-frame detail. Fine foundation/conduit dressing and auxiliary side-machine/support detail may drop at range. Bounds, size/reference, meshes, UVs, transforms and colliders are unchanged. Automatic LOD selection remains; no production ForceLOD or global quality override. [Checkpoint B](CHECKPOINT_B_VISIBILITY.md) records the fixed captures and quality sweeps.

## LookDev and living machine

- Eight authored PBR families distinguish graphite load-bearing mass, colder casing, brighter machined hardware, reflective pipes, matte dark gaskets, restrained copper, worn deck and pale cold panels. Thirty-two 512px maps carry metallic/smoothness, normal relief, occlusion and maintenance wear. Twenty chamber material assets change; two scoped trim copies protect the other facilities.
- Nominal metallic/smoothness pairs: graphite **.68/.26**, machined **.82/.64**, casing **.66/.43**, pipe **.85/.70**, recess **.12/.16**, copper **.75/.55**, deck **.45/.30**, cold panel **.68/.47**. Roughness is 1 minus smoothness; texture variation is retained. Upper casing plates add a machined zone **.78/.57**. New role normal strength is .8.
- Core emission `(2.6,.48,.045) → (4.3,.88,.07)`; inner energy `(2.8,.62,.045)`; nodes `(.045,.8,1.45) → (.035,1,1.9)`.
- Short service/plinth/collar sources, selected indicators and conduit segments are masks on existing opaque geometry. Warm/cool surface spill falls off locally and keeps underlying PBR response/occlusion visible. It affects the chamber's scoped materials, rather than changing surrounding facility materials or flooding the Chapter lighting.
- Slow irregular core breathing (20.3/45.9s waves, bounded ±12%), asynchronous cyan phases (roughly22–25s plus52.8s), very small indicator cycles and a .11m/s soft conduit crest provide operation. Sources never flash off or synchronize into a show, and have no gameplay meaning.
- Added **0 realtime lights, 0 runtime presenters, 0 particles/VFX systems, 0 transparent layers, 0 full-screen effects, 0 geometry**. Shader time handles motion; no new per-frame C# work or allocation.

[Checkpoint C](CHECKPOINT_C_LOOKDEV.md) contains the role/source parameters, emission treatment and explicit comparison with the supplied concept. Material richness, separation, warm/cool response, practical punctuation and operating feel improve. The concept still has stronger cinematic environmental bounce/aging; these captures do not establish a literal match or final acceptance.

## Performance

- LOD0: **45 Renderers / 279,694 triangles / 156 material slots / 22 unique materials**.
- LOD1: **40 / 253,846 / 135 / 22**.
- LOD2: **26 / 215,286 / 115 / 21**.
- Relative to APK55: near Renderer/triangle deltas **0**. More distant identity geometry is intentionally retained; this increases far cost versus the former29/15 Renderer levels. Cost still falls 9.2%/23.0% from complete near geometry.
- New maps use ASTC6x6/mips; estimated payload **5,069,824 bytes / 4.835MiB**, before driver overhead and excluding replacement/release of old maps.
- One opaque PBR forward pass plus standard shadow/depth passes. Analytic spill/practical masks and slow sine terms increase fragment arithmetic. No extra overlay/overdraw pass or C# presenter is added. Tiny sources can lose readability/alias on lower-resolution screens. Android GPU/FPS/thermals and driver output remain a device gate; no60FPS claim is made from Editor evidence.

## Executed validation

- Final Unity compile: **exit0**.
- Existing project validation: **exit0**, without weakening its checks.
- Full EditMode: **533 passed, 0 failed**.
- Final integration PlayMode: **9 passed, 0 failed**; supplementary phone-viewport PlayMode: **1 passed, 0 failed** (**10 total**).
- Actual G-0 perimeter and locomotion envelope, full Chapter route/shared tactical-map collision, all ordinary actor routes/arena clearance and attack/death/respawn regression passed. Fresh route receipts are copied into this pass.
- Camera/quality sweep: automatic LODs at six biases and22/30/50/80m; identity survives where in-frustum and Android default genuinely reaches both lower levels at distance.
- Real camera energy snapshots at0/9/21s confirm visible bounded GPU output change with other actor animation paused in the fixture.
- Architecture freeze: **27 source files and248 prefab records** verified; exact scene/layout, accepted FBX/UV/source, transforms, bounds and25 colliders preserved.
- Android56: **succeeded / exit0**, real artifact verified. Detailed XMLs/JSONs under `verification/`; logs remain local ignored evidence with hashes in `delivery-checks.json`.

## Screenshots and assumptions

The capture fixture loads the actual Chapter and its initialized PortraitFollowCamera/G-0, with unchanged offset/look-at/FOV48 and Android default quality. Only fixture combat and other actor animation are paused for clean presentation evidence. Runtime behavior is unchanged.

- [A — equivalent problematic west angle](screenshots/lookdev/A_problematic_west_view.png)
- [B — equivalent problematic south angle](screenshots/lookdev/B_problematic_south_view.png)
- [C — normal gameplay approach](screenshots/lookdev/C_normal_approach.png)
- [D — close gameplay view](screenshots/lookdev/D_close_north_view.png)
- [E — wider Chapter context](screenshots/lookdev/E_wider_chapter.png)
- [F — 5% emission/material diagnostic](screenshots/lookdev/F_low_emission.png)
- [G — normal final LookDev](screenshots/lookdev/G_normal_final.png)
- [H — top/node material detail at the close north gameplay view](screenshots/lookdev/H_material_detail.png)
- [Phone-aspect A](screenshots/phone-aspect/A_problematic_west_view.png) / [Phone-aspect B](screenshots/phone-aspect/B_problematic_south_view.png)

A–H are1080x1920 (9:16). The supplied JPEGs are591x1280 including about50px of OS status bar; supplementary591x1230 captures use the **inferred** game viewport aspect. Their two actual-camera tests retain all in-frustum secondary geometry; clipping outside the narrower frame is normal. JPEGs have no world-coordinate metadata, so positions are equivalent rather than exact recovered player coordinates. H naturally clips part of the installation at the fixed camera frame edge; it shows the near cap/node material response.

The material `_ChamberOrigin` is authored for the explicitly frozen `(11,.08,50.65)` placement. A later authorized move/duplicate must update that material configuration. The presentation pass does not add chamber mechanics, progression meaning or gameplay ownership.

## Source control and scope

- Diagnosis checkpoint: **eb3fe0a** — pushed before the visibility fix.
- Visibility checkpoint: **f146061** — pushed before LookDev.
- LookDev/runtime checkpoint and actual APK source: **339a8bc**.
- Delivery receipts, narrower-phone evidence and final report follow on the same branch/PR. The original production promotion/audit remains part of PR78. [FILES_CHANGED.txt](FILES_CHANGED.txt) lists this pass's delta from accepted APK55 integration HEAD `f8ce24a83ca5a50307396c3422a2660787c4cd24`.
- Changes are production chamber materials/shader/maps/prefab LOD/material references, scoped Editor tooling, tests, source custody/current authority clarification and delivery evidence. No gameplay-domain, player, actor, elite/boss, combat, progression, persistence, loot, packages, Chapter scene/layout, collision or another facility was changed in this continuation.
- Unrelated Unity-generated tracked changes restored only in the isolated checkout; recovery patches retained locally. Untracked files and the original user's checkout are preserved.

Next step is **human on-device review of APK56** for retention, dark/machined surfaces, warm/cool spill, small practical sources and restrained operating feel, plus FPS/heat. Another architecture object is not started on this branch.
