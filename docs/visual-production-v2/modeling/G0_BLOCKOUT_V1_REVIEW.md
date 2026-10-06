# G-0 Blockout V1 — art-direction review

**Grade B for the blockout gate. Ready for human art-direction review, not production replication or Unity handoff.**

Editable source: `art/visual-production-v2/g0/G0_Blockout_V1.blend`. Blender 5.2.2 LTS, build `d13f752e3b9c`; executable `C:/Program Files/Blender Foundation/Blender 5.2/blender.exe`.

## Delivered model

- 15,080 actual triangulated faces, with bevel modifiers applied before saving.
- 87 authored mesh objects; 107 model objects including root, fourteen articulation pivots and five sockets. Five additional review-stage objects (ground, three lights, camera) are excluded from model totals.
- Four shared blockout material swatches: pale armor, graphite frame, exposed steel, cyan energy. The review ground has a fifth, stage-only material.
- Zero third-party donor parts, textures, rigs or animation clips.
- Blender Z-up / -Y forward; root at origin, rotation zero, scale one. No negative object scales. Eventual Unity axis conversion is deferred.
- Model bounds: approximately 3.229 × 3.423 × 1.704 m (width × length including blades × height). Height/width approximately 0.53. Visible lens diameter 0.804 m, approximately 25% of support-span width. These are authored proportion anchors, not locked runtime scale.
- Four distinct support chains; upper link, knee bearing, lower segment, piston and two-point mechanical toe contact per support. Fore/aft supports remain separate from paired forward attack mandibles.
- Custom tapered keel, sloped layered flank/mantle plates, deep angled core housing, recessed convex lens, closed dark rear containment, forward blade forms and compact rear heat-exchanger/accumulator mass.

Geometry was authored inside Blender with explicitly designed vertex contours, closed plate thickness, tapered cross-sections, mesh bevels and separate low-identity joint/actuator cylinders. The retained Python authoring script makes this `.blend` reproducible. It does not depend on Unity primitives, runtime geometry code, imported donor bodies or generated images.

## Strict visual review

- Silhouette: **B**. Low stance, four grounded supports, two forward mandibles, no human torso/head/pelvis. Primary masses and attack direction survive the black and phone-size checks. The rim is still too dominant relative to the prototype's integrated shell; retain this as an explicit review question.
- Proportions: **B**. Width/height and core/span ratios match the brief. Stance is deliberately symmetrical for the first neutral pose. The human review should confirm whether the front/back support spread feels agile enough.
- Core anatomy: **B**. Real housing depth and layered rim; full front aperture is clear. Rear cap prevents the lens reading as a second rear lamp. Plain swatch emission lacks the prototype's concentrated cyan center; final energy treatment is deferred.
- Armor/mechanics: **B** for blockout. Three readable depth layers; exposed knees/pistons and shell gaps. Mantle surfaces are intentionally coarse and angular. Their curvature and the transition into the core rim need refinement after direction is accepted.
- Rear shape: **B-/C boundary**. Compact authored mechanism mass exists and is lower than evolution fins. It is less characterful than the approved board implies; review the rear silhouette before production details.
- Top gameplay-angle appearance: **B** as a Blender approximation. Uses the current camera-offset ratio 14.8/11.2 (~53°), with G-0 facing staged toward the camera. Portrait orthographic frame and reduced phone-size inset are provided. This is not a Unity/device screenshot or a movement validation.
- Mobile handoff: **not approved**. Triangle cost is within the eventual player soft maximum, but 87 separate meshes must be consolidated around actual articulation. Do not import this object organization as 87 runtime renderers.

The bounded next revisions are core-rim integration, shell curvature, stance confirmation and rear shape. No final texture/wear, greeble, rig or enemy work is performed before this review.

## Evidence

All images are actual Blender renders of the saved authored geometry; comparison layout uses the supplied approved prototype crop.

1. `g0-evidence/01_G0_front.png`
2. `g0-evidence/02_G0_side.png`
3. `g0-evidence/03_G0_back.png`
4. `g0-evidence/04_G0_threequarter.png`
5. `g0-evidence/05_G0_top_gameplay_angle.png`
6. `g0-evidence/06_G0_black_silhouette.png`
7. `g0-evidence/07_G0_prototype_comparison.png`

Source board: actual local `Концепт GRAVIVORE_ Мехи и окружение.png`; original SHA256, exact crop coordinates and all final image hashes are recorded in `data/g0_evidence_manifest.json`. Prototype rear forms are not fabrication drawings; those ambiguous forms were interpreted using PR #53's custom-first visual brief.

## Verification and limits

`Tools/art/verify_g0_blend.py` reopens the actual saved source and verifies all 87 model meshes are closed/manifold, with no loose vertices or zero-area faces, positive scales and applied modifiers. It verifies four toe-contact heights, mechanically located articulation pivots, core/attack socket positions, material count and measured triangle total. Full result: `data/g0_blend_verification.json`.

`Tools/art/validate_production_artifacts.py` validates actual map hashes/channel sizes and positive-Z normal data, thirteen source records/eight character audits/189 environment audits, the sixteen-piece subset, non-identical sampled source animation poses, seven decodable PNGs and Git scope. Five tests pass; `data/artifact_validation.json` records the result. All authoring/inspection Python files compile. Archive integrity, source audits and Blender rendering were executed.

Static mesh checks do not validate inter-object motion clearance, locomotion, weighting or runtime shading/performance. No unified UV unwrap, texture bake, final materials, armature, animation, LODs, collider package or FBX export is delivered. Cylindrical mechanics retain incidental default UVs; these are not a finished texture layout.

Unity compile/validation/build checks and their exact outcome are recorded separately in `VERIFICATION.md`. The blockout stays outside `Assets/`. No Chapter01 prefab or scene is replaced, no enemy is finished and no PR is merged.

**STOP: next gate is G-0 Blockout V1 human art-direction review. No next implementation spec is started.**
