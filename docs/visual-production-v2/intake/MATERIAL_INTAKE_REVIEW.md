# Material Intake Review

Requested official Poly Haven sources:
- Metal Plate — https://polyhaven.com/a/metal_plate
- Metal Grate Rusty — https://polyhaven.com/a/metal_grate_rusty
- Blue Metal Plate — https://polyhaven.com/a/blue_metal_plate
- Rusty Metal Grid — https://polyhaven.com/a/rusty_metal_grid

All four official pages reconfirm CC0 licensing and PBR channel availability. Actual source map bytes were not acquired in this execution environment, therefore no file-level resolution/bit-depth/hash inspection was performed.

## Metal Plate
Official page exposes AO, AO/Rough/Metal, diffuse, displacement, metal, normal DX/GL, roughness and additional bump/spec options. Source maximum advertised: 8K. License: CC0. Author: Rob Tuytel.

## Metal Grate Rusty
Official page exposes AO, AO/Rough/Metal, diffuse, displacement, metal, normal DX/GL and roughness. Source maximum advertised: 8K. License: CC0. Authors: Dimitrios Savva / Rob Tuytel.

## Blue Metal Plate
Official page exposes AO, AO/Rough/Metal, diffuse, displacement, normal DX/GL and roughness. Source maximum advertised: 16K. License: CC0. Author: Rob Tuytel.

## Rusty Metal Grid
Official page exposes AO, AO/Rough/Metal, diffuse, displacement, normal DX/GL and roughness. Source maximum advertised: 16K. License: CC0. Author: Amal Kumar.

## Mobile retention plan

Once 1K/2K source files are actually acquired:
- retain base color/diffuse
- retain tangent-space normal; standardize on the engine-appropriate handedness during production
- retain roughness/metallic, preferably pack into the project's mask convention
- AO may be packed only if the runtime material path uses it
- do not ship displacement source maps in mobile runtime materials
- do not ship 8K/16K originals
- remove redundant DX/GL normal variants after target convention is chosen
- apply GRAVIVORE palette/value adjustments in authored derivative textures rather than altering provenance records

## Material verdict

**NOT YET APPROVED FOR PRODUCTION.**

Licenses are verified, but production approval remains blocked on actual source-file acquisition, hash capture and map inspection.
