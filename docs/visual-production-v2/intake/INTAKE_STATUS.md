# Visual Production V2 — Intake Status

Intake date: 2026-10-06
Branch: `art/visual-production-v2-intake`
Base main: `40b682d3570d08c427999d42514501bb6c1c2dfd`
Production contracts: PR #53 and PR #54.

## Executive status

This pass preserved the production gate strictly: no donor was promoted to `APPROVED FOR PRODUCTION` from web-page evidence alone.

### Environment
- Quaternius Modular Sci-Fi MegaKit: license and official download offer reconfirmed. Official page states 277 models, FBX/OBJ/Blend/glTF, CC0; itch.io offers a free Standard ZIP plus paid Pro/Source ZIPs. Actual archive bytes were not acquired in this execution environment, therefore the pack is **not source-inspected** here.
- Result: no environment module is approved yet. Acquisition remains the next gate.

### Materials
- Poly Haven Metal Plate, Metal Grate Rusty, Blue Metal Plate and Rusty Metal Grid: official asset pages reconfirm CC0 and the expected PBR map families.
- Actual texture archive/map bytes were not acquired in this execution environment, so these remain license-verified source candidates rather than source-inspected runtime inputs.

### Scout / Cutter / Magnetar
- Official Sketchfab pages reconfirm all requested candidates are downloadable CC BY models and expose their public triangle/vertex metadata.
- Download acquisition is gated by Sketchfab account/login workflow. Intake rules prohibit bypassing login or requesting credentials.
- Therefore all requested Sketchfab donors are **BLOCKED BY LOGIN** for archive-level inspection in this pass.

### Blender
- Blender executable is not available in the current execution environment.
- No candidate is marked `BLENDER INSPECTED`.

### Paid sources
- Quaternius Pro/Source tiers are optional paid upgrades; Standard is free.
- OZEA HS-003 was not purchased; it remains optional and outside approval because user approval is required before spend.

## Decision gate

- Scout: **CUSTOM REQUIRED pending donor archive access**. No archive was inspected, so no donor can be approved.
- Cutter: **CUSTOM REQUIRED pending donor archive access**. Same gate.
- Magnetar mechanics: **CUSTOM ONLY for now**. Candidate mechanics remain blocked by archive access.
- Environment: **REJECT FOR APPROVAL AT THIS GATE** (not a content rejection; archive inspection is incomplete). Quaternius remains preferred acquisition target.
- Materials: **NOT YET APPROVED FOR PRODUCTION** because actual files were not acquired/inspected in this pass.
- G-0: **CUSTOM-FIRST**. No donor mechanics component is approved without archive/Blender inspection.

## Integrity note

`SOURCE INSPECTED`, `BLENDER INSPECTED` and `APPROVED FOR PRODUCTION` are intentionally withheld where the evidence threshold was not met. This is stricter than the prior sourcing documents and is the purpose of this intake branch.
