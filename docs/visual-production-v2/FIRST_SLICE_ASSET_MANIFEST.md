# GRAVIVORE Visual Production V2 — First Slice Asset Manifest

Status: source-intake gate recorded on 2026-10-06.

Source of truth: Visual Replacement Production V2 preproduction package (PR #53) + sourcing package (PR #54) + approved GRAVIVORE prototype board.

Maturity rule: no web thumbnail/page is sufficient for `SOURCE INSPECTED`, `BLENDER INSPECTED`, or `APPROVED FOR PRODUCTION`. Actual source bytes and provenance are mandatory.

| Production ID | Role | Preferred source/path | License | Actual acquisition maturity | Production status | Blocking issue |
|---|---|---|---|---|---|---|
| G0-T0 | Player G-0 Tier 0 | Custom-first + future low-identity donor mechanics | internal + donor-specific | no donor archive inspected | CUSTOM-FIRST | donor mechanics not source-inspected; visible identity remains custom |
| EN-SCOUT | Scout | PAndras first; Hasan/Keralt alternatives | CC BY | donor pages/license verified; archives BLOCKED BY LOGIN | CUSTOM REQUIRED at current gate | authenticated archive + Blender inspection required before donor selection |
| EN-CUTTER | Cutter | gwim first; Mirandanimator alternative | CC BY | donor pages/license verified; archives BLOCKED BY LOGIN | CUSTOM REQUIRED at current gate | authenticated archive + topology/rig inspection required |
| EN-MAG | Magnetar Guard | custom torso/armor/core + future donor mechanics | CC BY candidates | donor pages/license verified; archives BLOCKED BY LOGIN | CUSTOM ONLY at current gate | mechanics cannot be approved without source inspection |
| ENV-FLOOR | Modular industrial floor kit | Quaternius Modular Sci-Fi MegaKit | CC0 | official source/license/download offer verified; archive not acquired | NOT APPROVED | actual archive inspection |
| ENV-WALL | Walls/barriers/columns | Quaternius backbone | CC0 | official source/license/download offer verified; archive not acquired | NOT APPROVED | actual archive inspection |
| ENV-GATE | Hero gate | Quaternius structural donor + custom hero pass | CC0 backbone | archive not acquired | CUSTOM/HYBRID REQUIRED | exact structural subset not inspected |
| ENV-REACTOR | Hero reactor | Custom/hybrid | internal + donor-specific | not modeled | CUSTOM MODEL REQUIRED | production modeling |
| ENV-PROP-01 | Tank / pressure vessel | Quaternius or optional OZEA | CC0 / paid license | Quaternius archive not acquired; OZEA PAYMENT REQUIRED | NOT APPROVED | archive/purchase gate |
| ENV-PROP-02 | Pipes/conduits | Quaternius | CC0 | archive not acquired | NOT APPROVED | archive inspection |
| MAT-ARMOR | Cool worn armor | Poly Haven Blue Metal Plate | CC0 | official page/license/maps verified; source bytes not acquired | NOT APPROVED | source-file/hash inspection |
| MAT-HAZARD | Painted hazard surface | Poly Haven Metal Plate + authored stripe layer | CC0 | official page/license/maps verified; source bytes not acquired | NOT APPROVED | source-file/hash inspection |
| MAT-GRATE | Grates | Poly Haven Metal Grate Rusty | CC0 | official page/license/maps verified; source bytes not acquired | NOT APPROVED | source-file/hash inspection |
| MAT-FLOOR | Dirty floor plates | Poly Haven Metal Plate | CC0 | official page/license/maps verified; source bytes not acquired | NOT APPROVED | source-file/hash inspection |
| MAT-WALLBREAKUP | Rusted wall/grid breakup | Poly Haven Rusty Metal Grid | CC0 | official page/license/maps verified; source bytes not acquired | NOT APPROVED | source-file/hash inspection |

## Intake interpretation

The previous sourcing package identified promising candidates. This manifest records the stricter actual acquisition maturity reached in PR #55. Where source bytes were not available, the asset remains below production approval even if the web page and license are clear.

## Next legal/technical gate

1. Acquire the free Quaternius Standard ZIP from the official distribution and immediately record filename + SHA256.
2. Acquire 1K/2K Poly Haven map sets from official distribution and hash the files/archive.
3. Use an authenticated Sketchfab session, without sharing credentials, to acquire the requested donor archives if the owner chooses to continue donor evaluation.
4. Inspect acquired geometry in Blender and update the per-asset review files before any `APPROVED FOR PRODUCTION` promotion.
