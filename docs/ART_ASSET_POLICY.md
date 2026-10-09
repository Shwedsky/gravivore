# GRAVIVORE art and asset policy

Status: **CURRENT / AUTHORITATIVE**

Read together with:
- `docs/CURRENT_VISUAL_TARGET.md`
- `docs/ASSET_SOURCE_OF_TRUTH.md`
- `docs/ART_DIRECTION.md`

## Visual target

The target is **mobile-optimized premium hard-surface industrial sci-fi**.

`Low-poly` is not an art direction. Polygon reduction, LODs, atlases, shared materials and draw-call reduction are optimization methods used after or during authoring while preserving the target appearance.

Reject final presentation that still reads as:
- primitive/proxy geometry;
- toy-like box mechs;
- flat single-color material replacement;
- generic untouched stock art;
- sparse test-floor dressing;
- neon used in place of form/material definition.

## Spend and sourcing

Initial asset spend remains **0 unless the owner explicitly changes it**.

Allowed sources:
- original project-created geometry/materials/textures/audio;
- Unity built-ins where they are not visible final art;
- truly free assets with verified commercial-use terms;
- CC0 preferred;
- CC BY acceptable with exact attribution and redistribution review;
- other free commercial licenses only after item-level review.

Never use:
- ripped/extracted commercial-game assets;
- recognizable franchise derivatives;
- NC assets;
- incompatible ShareAlike assets;
- unclear-license downloads;
- paid-only content while the zero-spend rule is active.

## Public repository rule

A license that permits using an asset in a shipped game does not automatically permit publishing its raw/editable source in a public Git repository.

Keep restricted Asset Store/custom-license payloads local/private. Commit only what the license permits. Record provenance in `ThirdPartyNotices.md` and the current asset register.

## Donor / kitbash rule

Donor assets may provide useful:
- joints;
- armor mechanisms;
- machinery pieces;
- panels;
- weapon parts;
- pipes/services;
- animation/rig structure where legally and technically suitable.

But a donor is not automatically final art. Hero actors, major facilities and landmarks must be adapted until they fit the GRAVIVORE faction/world language and do not look like an unchanged marketplace asset.

Kitbash is a production technique, not an excuse to preserve toy-like or obviously modular construction.

## Materials

Prefer a coherent shared material vocabulary, but preserve within-object variation through authored maps/masks:
- BaseColor;
- tangent-space Normal;
- Metallic/Roughness or Metallic/Smoothness;
- AO when useful;
- localized Emission;
- wear/recess/heat/damage information appropriate to the object.

Do not flatten every imported material slot to one generic Body/Accent/Dark material if that destroys meaningful authored surface response. Material consolidation must preserve or rebake the visual information that matters.

## Lighting and VFX

Keep realtime cost bounded, but use baked/emissive/fake-light techniques to preserve depth. The absence of many realtime lights is not permission to ship a flat scene.

VFX must remain pooled/bounded and support readable source -> travel -> impact causality.

## Mobile optimization

Target 60 FPS on a reasonable mid-range Android device.

Use:
- LODs;
- mesh simplification where invisible at gameplay scale;
- shared atlases/materials;
- ASTC/compressed textures;
- mipmaps;
- culling/batching;
- bounded particle counts;
- limited transparent overdraw;
- limited shadowed realtime lights.

Old per-asset triangle/material numbers are guidance/history unless reaffirmed by a current task. Real-device frame time is the final performance gate.

## Current external intake

New downloads go under `ExternalAssetIntake/Current/` only. Historical V1/V2 raw-cache locations are not current source paths.

See `docs/ASSET_SOURCE_OF_TRUTH.md` for the full path contract.
