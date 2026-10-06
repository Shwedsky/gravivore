# Mobile material preparation

All four actual Poly Haven material libraries were opened in Blender. Production maps live in `art/visual-production-v2/materials/` outside Unity. All are 2048×2048 PNG, 8-bit delivery copies; higher-precision EXR originals remain local. Source/output hashes and shader nodes are in `data/material_prep.json`.

Base color is sRGB. OpenGL tangent normals, roughness and metallic are linear data; EXR conversions use Raw view transform with zero exposure and gamma one. Normal green channel is preserved for later explicit importer/shader convention. No baked-lighting operation or normal inversion is performed. No displacement, height, transparency or tessellation is shipped. No AO was supplied in these downloaded sets, so none is invented.

## blue_metal_plate

- `art/visual-production-v2/materials/blue_metal_plate/blue_metal_plate_basecolor_2k.png` — 4,472,825 bytes, sRGB.
- `art/visual-production-v2/materials/blue_metal_plate/blue_metal_plate_normal_gl_2k.png` — 2,735,296 bytes, linear.
- `art/visual-production-v2/materials/blue_metal_plate/blue_metal_plate_roughness_2k.png` — 1,900,713 bytes, linear.
- Excluded: blue_metal_plate_disp_2k.png.

## metal_grate_rusty

- `art/visual-production-v2/materials/metal_grate_rusty/metal_grate_rusty_basecolor_2k.png` — 7,757,220 bytes, sRGB.
- `art/visual-production-v2/materials/metal_grate_rusty/metal_grate_rusty_normal_gl_2k.png` — 7,692,883 bytes, linear.
- `art/visual-production-v2/materials/metal_grate_rusty/metal_grate_rusty_roughness_2k.png` — 3,213,284 bytes, linear.
- `art/visual-production-v2/materials/metal_grate_rusty/metal_grate_rusty_metallic_2k.png` — 3,148,839 bytes, linear.
- Excluded: metal_grate_rusty_disp_2k.png.

## metal_plate

- `art/visual-production-v2/materials/metal_plate/metal_plate_basecolor_2k.png` — 6,363,354 bytes, sRGB.
- `art/visual-production-v2/materials/metal_plate/metal_plate_normal_gl_2k.png` — 6,471,965 bytes, linear.
- `art/visual-production-v2/materials/metal_plate/metal_plate_roughness_2k.png` — 2,998,366 bytes, linear.
- `art/visual-production-v2/materials/metal_plate/metal_plate_metallic_2k.png` — 3,185,730 bytes, linear.
- Excluded: metal_plate_disp_2k.png.

## rusty_metal_grid

- `art/visual-production-v2/materials/rusty_metal_grid/rusty_metal_grid_basecolor_2k.png` — 6,256,715 bytes, sRGB.
- `art/visual-production-v2/materials/rusty_metal_grid/rusty_metal_grid_normal_gl_2k.png` — 7,166,857 bytes, linear.
- `art/visual-production-v2/materials/rusty_metal_grid/rusty_metal_grid_roughness_2k.png` — 1,883,218 bytes, linear.
- Excluded: rusty_metal_grid_disp_2k.png.

## Intended use / strict review

- Blue metal plate: B surface breakup source for cool worn metal; the supplied blue coating is too dark for the pale G-0 shell without later calibrated authored color. No metallic map supplied; assign a chosen material value later rather than guessing from color.
- Metal plate: B industrial floor/panel source, includes actual metallic. Hazard stripes must be authored later, not claimed as supplied.
- Metal grate rusty: B grate/drain source with actual metallic; avoid repeating high-contrast rust in combat center.
- Rusty metal grid: B localized wall/service breakup; no metallic map supplied. Do not coat every surface in this pattern.

Fourteen maps are preparation assets, not four finalized runtime materials. Shared tiling scale, masks/smoothness packing, 1K import where appropriate, ASTC/compression and Android shader/device verification remain after art direction. G-0 Blockout V1 uses plain material swatches; these maps do not finish its textures.
