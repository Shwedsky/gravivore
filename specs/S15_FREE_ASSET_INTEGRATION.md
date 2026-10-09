# S15 FREE ASSET INTEGRATION

## Goal

Integrate legally usable zero-cost art into a coherent production presentation that supports the current GRAVIVORE visual target.

## Current authority

Follow:
- `AGENTS.md`
- `docs/CURRENT_VISUAL_TARGET.md`
- `docs/ASSET_SOURCE_OF_TRUTH.md`
- `docs/ART_ASSET_POLICY.md`
- `docs/ART_INTAKE_CHECKLIST.md`

Historical Phase 3 / Art Spike / V1 intake documents are evidence only and must not define current style or paths.

## Scope

- item-level license/provenance verification;
- `ThirdPartyNotices.md` updates;
- coherent environment/hero-prop selection;
- player/enemy/equipment donors where they materially improve the current target;
- animation/rig import where intentionally used;
- authored adaptation/kitbash/custom work required to remove obvious stock/proxy identity;
- mobile optimization after visual fit is established.

## Out of scope

- ripped content;
- unclear or incompatible licenses;
- paid-only dependencies while the zero-spend rule is active;
- treating raw vendor/demo projects as production architecture;
- preserving low-poly/proxy appearance merely because an asset is mobile-friendly.

## Acceptance criteria

- [ ] Every promoted third-party asset has documented provenance/license and public-repository handling is legal.
- [ ] No paid asset is required for the build while the zero-spend decision remains active.
- [ ] Five ordinary enemy roles remain distinguishable by silhouette/function, not only color.
- [ ] Major actor/hero/environment presentation materially matches `CURRENT_VISUAL_TARGET.md` and does not read as obvious primitive/proxy/stock art.
- [ ] Materials retain meaningful authored surface variation after URP conversion/consolidation.
- [ ] Production art remains separate from gameplay authority and current behavior/save/progression is preserved.
- [ ] Android performance is measured; optimization decisions are based on actual device cost rather than historical prototype ceilings alone.

## Verification

- editor validation for missing meshes/materials/animations/references;
- license/provenance audit;
- real gameplay-camera review;
- relevant EditMode/PlayMode tests;
- Android device visual/performance gate for production acceptance.

## Definition of Done

- Implementation compiles.
- Relevant tests pass, or unavailable execution is truthfully reported.
- No new missing references/errors in the target scene.
- Documentation/config updated when contracts or authoring steps changed.
- Agent completion report follows `AGENTS.md`.
