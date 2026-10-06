# Material Source Register — First Visual Slice

Research date: 2026-10-06. Preferred source policy: CC0/permissive PBR, download/use at 1K–2K for mobile even when higher-resolution masters exist.

## Approved source family

**Poly Haven — CC0**. Commercial use allowed, no attribution required. Use source maps only; do not ship unnecessary 8K/16K masters in Unity.

### MAT-FLOOR-PLATE — Metal Plate
- Author: Rob Tuytel
- URL: https://polyhaven.com/a/metal_plate
- License: CC0
- Maps: AO, ARM, bump, diffuse, displacement, metal, normal GL/DX, roughness, spec.
- Character: worn diamond-plate steel, chipped green paint, rust/pitting/oily wear.
- Use: dirty floor plates / maintenance walkway donor.
- Production: download 2K source; generate Unity URP basecolor/normal + packed mask; tune rust intensity down if it becomes visually noisy.
- Fit: A-.

### MAT-GRATE-RUST — Metal Grate Rusty
- Authors: Dimitrios Savva / Rob Tuytel
- URL: https://polyhaven.com/a/metal_grate_rusty
- License: CC0
- Maps: AO/ARM/diffuse/displacement/metal/normal/roughness.
- Use: grates, service walkways, drain/inset panels.
- Production: 1K–2K; avoid displacement in runtime; use normal + AO/rough/metal.
- Fit: A-.

### MAT-PAINT-WORN — Blue Metal Plate
- URL: https://polyhaven.com/a/blue_metal_plate
- License: CC0
- Character: painted steel with scuffs, scratches and abrasion.
- Use: cool worn armor / painted industrial panels after hue shift to GRAVIVORE palette.
- Production: recolor non-destructively; keep roughness variation, avoid literal bright-blue identity.
- Fit: B+.

### MAT-GRID-CORRODED — Rusty Metal Grid
- Author: Amal Kumar
- URL: https://polyhaven.com/a/rusty_metal_grid
- License: CC0
- Maps: AO/ARM/diffuse/displacement/normal/roughness.
- Use: wall service panels, corroded maintenance inserts, select floor/wall breakup.
- Production: 1K–2K; reduce heavy rust for hero kit consistency.
- Fit: B+.

## Required GRAVIVORE material families

1. **Graphite structural metal** — derive from neutral dark metal base, high roughness variation, restrained edge wear.
2. **Cool worn armor** — pale/cool gray coated metal, subtle scratches, darker exposed edges.
3. **Mid-tone industrial metal** — neutral steel for hinges/joints/pistons.
4. **Painted hazard surfaces** — service yellow/orange only on localized strips/panels; do not turn arena into hazard-stripe wallpaper.
5. **Scratched/worn coated metal** — reused across enemies/environment with tint masks.
6. **Grates** — use Metal Grate Rusty family.
7. **Dirty floor plates** — use Metal Plate family.

## Unity URP conversion standard

- Base Color: sRGB.
- Normal: Unity normal import; prefer OpenGL source when workflow expects it, verify green-channel convention visually.
- Mask map target: pack Metallic, AO, Smoothness/roughness-converted channels according to project shader convention.
- Runtime texture size: default 1K for ordinary props, 2K for hero shared atlas, lower where screen coverage is small.
- Displacement/height: baking/authoring aid only unless specifically justified; do not add tessellation workflow for mobile.
- Reuse shared tilers + decal/trim variation instead of unique 2K maps on every prop.

## Rejected workflows

- 8K/16K runtime textures for this mobile slice.
- Random per-asset PBR libraries with inconsistent roughness/metal response.
- Neon edge outlines as a substitute for silhouette.
- Unclear-license texture sites.
