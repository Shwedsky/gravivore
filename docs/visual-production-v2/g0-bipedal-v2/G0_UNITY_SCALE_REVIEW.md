# G-0 V2.1 actual Unity scale review

2026-10-06, Europe/Moscow. Unity 6000.3.0f1, URP, real Camera.Render GPU output. These are Unity captures, not Blender camera approximations. Static isolated review, no production prefab replacement.

## Authoritative gameplay and review construction

Canonical `Chapter01_ScrapExclusion.unity` was opened read-only to extract its serialized production references. Its runtime composition was not invoked or saved. The review constructs the existing world from the actual `Chapter01WorldPresenter`, `S08_Chapter01World` configuration and `ChapterVisualEnvironment` reference in an unsaved temporary setup, then saves **renderer/light snapshots only** into `Assets/_Game/ArtReview/G0V21/G0_V21_ScaleReview.unity`. Production geometry is not resized or relocated. The temporary scene is deleted.

Scout Drone and Warden use the actual `S15_VisualCatalog` recipes/prefabs with their production transforms (Scout 0.7, Warden 0.7 and its 0.057 Y offset). Warden is the stronger ordinary reference. Magnetar uses the actual `Chapter01_VisualIntegration.Elite` binding: scale 0.8, Y offset 0.05. The current production Tier0 prefab is separately measured and shown beside G-0 at the real player spawn in capture 09.

- Player authority: capsule **radius 0.42, diameter 0.84, height 1.4, center (0,0.7,0)**; step offset remains 0.25. A presentation-only radius-0.42 ring marks its footprint.
- Actual production Tier0 visual bounds: **1.314694 wide / 1.484131 high / 0.741827 deep**. This visual already extends beyond the gameplay capsule; its width does not define a new gameplay collider.
- Actual production spawn: **(0,0,-30)**. Main gate-side review position: **(0,0,57)**. Supplemental capture 09 places G-0 at the actual spawn and the current Tier0 1.7 units to its left.
- Main staged references: Scout `(-2.5,0,54.8)`; Warden `(2.5,0,54.8)`; Magnetar `(2.1,0,57)`. These are static review actor placements; no production spawn/pathing data changes. Perspective differs for ordinary actors because they are closer to the camera; Magnetar and G-0 share depth. Each placement is identical across the three variants.
- Camera: **perspective, 46° FOV, offset (0,14.8,-11.2), look-at height 0.9**, 9:16 portrait, near 0.1/far 100. Production damping 0.18 is recorded; the review uses the exact settled pose. Main camera position `(0,14.8,45.8)`; camera configuration/tuning remains unchanged.
- Presentation-only review lighting uses the authored environment lights plus fixed ambient `(0.22,0.25,0.28)`, no postprocessing. Three warmup renders precede capture so all scale panels use stable shader/light conditions. This is not final lighting approval.
- Gameplay gate: position `(0,0,60)`, **5 wide / 2.5 high / 0.6 deep**. Generated top-frame underside is 2.48. Authored nearby wall/pipes/closed-gate dressing are preserved.

## Source units versus presentation multipliers

Blender source remains 2.062500 wide / 3.358425 high / 1.753808 deep after axis conversion. Import scale is 1, file unit scaling enabled, baked axis conversion, animation off. Imported forward is **-Z**, verified using the actual imported cyan chest-center Z=-0.29; hero yaw is zero to face the camera.

Nominal 1.00x is explicitly defined as **source-to-presentation fit 1.4 / 3.358425 = 0.416862071**, anchored to the existing capsule height. This is an isolated visual transform, not a source-mesh resize or gameplay scale change. Selection is judged from captured camera readability and hierarchy, not real-world meters.

- **0.90x:** visual transform **0.375175864**, bounds **0.773800 × 1.260000 × 0.657990** (width/height/depth). Projected conservative mesh-bounds extent **98.65 × 143.12 pixels** at 1080x1920; **32.88 × 47.71** at 360x640. Capsule-side visual overhang: zero.
- **1.00x:** visual transform **0.416862071**, bounds **0.859778 × 1.400000 × 0.731094**. Projection **110.17 × 159.71**; phone **36.72 × 53.24**. Visual width exceeds the capsule by **0.009889 per side**.
- **1.10x:** visual transform **0.458548278**, bounds **0.945756 × 1.540000 × 0.804214**. Projection **121.81 × 176.44**; phone **40.60 × 58.81**. Visual width exceeds capsule by **0.052878 per side**. This is geometry overhang, not a collider modification.

Projection measurements use the eight corners of each imported mesh's local bounds; they are conservative screen envelopes, not segmented visible-pixel counts. Phone captures are separately rendered at native 360x640, not a resized full-resolution image.

## Per-variant visual judgment

### 0.90x

Readable core/shoulder marker and two-legged contour at full resolution, but lower-body separation and tool identity are marginal on phone. Scout's broad horizontal body can take comparable screen attention; Warden feels substantially heavier. Magnetar is clearly dominant. G-0 feels undersized beside the current Tier0 reference. Lateral gate margin **2.113100 each side**; top-frame clearance **1.220000**. Lowest presentation overhang risk, weakest hero presence.

### 1.00x

Compact, readable biped with separated feet and visible cyan core. Stronger hero cue than 0.90x, while Warden still reads as a broad, heavy enemy and Magnetar remains markedly larger. Phone silhouette is usable, though dark waist and small feet blend at a glance. Lateral gate margin **2.070111 each side**; top-frame clearance **1.080000**. A safe secondary choice but the slimmer new model carries less screen mass than the current player.

### 1.10x — recommended

Best of the three for the shoulder/core/leg arrangement at native phone size. The increased screen height helps the taller narrow contour carry hero identity while Scout remains wide and Warden remains mechanically heavy. Magnetar still dominates in width/mass and height; G-0 does not compete as an elite-sized tank. Lateral gate margin **2.027122 each side**; top-frame clearance **0.940000**. Small static visual overhang beyond the capsule is similar in kind to the existing production presentation and does not require gameplay changes.

## Clearance and occlusion findings

At the centered approach position all three variants fit comfortably within the authoritative 5-unit gate opening and under the generated frame. Feet are grounded; no static hero/reference intersections are visible in the final comparison placements. There is no reason to resize gates or change movement/attack/pathing.

**The actual closed-gate plane is a visual limit:** capture 10 places the 1.00x hero at Z=60 and retains authored closed-gate dressing. It can fully obscure the hero from the existing overhead camera. Capture 07 shows the near-side approach at Z=58, with only generated energy cells/emitters hidden for an open-clearance view; all authored dressing/frame dimensions remain intact. These are separate geometric-clearance and occlusion observations. A static snapshot does not certify unlocked-gate animation or live traversal visibility. No gate or camera fix is included in this pass.

Fine actuators, sole details and sensor slit do not remain individually legible at 360x640. The useful identity is broad shoulders, the central cyan chest accent, two legs/feet and unequal tools. Busy combat/VFX, moving gait, HUD occlusion, rear-facing movement and physical-phone performance remain untested by the static evidence.

## Captures

Evidence root: `C:/Users/pamak/Documents/ChatGPT/gravivore/.codex-worktrees/g0-bipedal-blockout-v2/docs/visual-production-v2/g0-bipedal-v2/evidence-v21/unity/`.

1. `01_G0_scale_090_gameplay.png` — 1080x1920, Scout/Warden/Magnetar/real gate and surroundings.
2. `02_G0_scale_100_gameplay.png` — same framing and references.
3. `03_G0_scale_110_gameplay.png` — same framing and references.
4. `04_G0_scale_comparison_board.png` — 3240x1920, left 0.90x / center 1.00x / right 1.10x, direct Unity pixel composition.
5. `05_G0_vs_ordinary_enemy.png` — 1.00x with Scout/Warden, Magnetar hidden.
6. `06_G0_vs_Magnetar.png` — 1.00x with Magnetar, ordinary references hidden.
7. `07_G0_gate_clearance.png` — 1.00x near-side approach, original gate/world dimensions.
8. `08_G0_phone_size_readability.png` — 1080x640 board of three native 360x640 renders, left/center/right in scale order.
9. `09_G0_current_player_spawn_reference.png` — actual production spawn, current Tier0 LEFT, new 1.00x CENTER, production references around them.
10. `10_G0_actual_gate_plane_occlusion.png` — disclosed closed-gate-plane occlusion.

Native phone sources: `phone_090.png`, `phone_100.png`, `phone_110.png`.

All annotations and boards are produced in Unity. No Blender/image-generator content is inserted into the Unity evidence. Full metrics and import-facing probe: `data-v21/unity_scale_metrics.json`. Capture log: `verification-v21/Capture.log`. Saved review scene contains no gameplay colliders or controllers and is excluded from build settings and canonical scene dependencies.

Blender measures 29,417 evaluated triangles; Unity's static FBX import reports **29,381 triangles / 142 renderers** after importer mesh processing. Neither count is a shipping budget certification. The separate authoring pieces are retained for review editability; no final renderer consolidation/LOD pass was performed.
