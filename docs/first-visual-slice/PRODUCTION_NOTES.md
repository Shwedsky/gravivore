# First playable visual slice production notes

## Source and authorship

Scout V1, Cutter V1, Magnetar V1, the eleven industrial kit models and their
256px shared palette atlas are original project geometry created by
`Tools/first-visual-slice/build_assets.py`. No marketplace model, external texture,
paid asset, extracted game content or donor enemy model was used. Editable
Blender sources are retained under `art/first-visual-slice/`.

G-0 reuses the merged Production V3 FBX, rig, clips and material maps. Its original
source attribution in `ThirdPartyNotices.md` remains applicable. The live child
uses the frozen `.4585482776` source normalization corresponding to the accepted
1.10 visual fit. No mesh or silhouette redesign was performed.

## Playable area

The rebuild follows the existing Cutting Floor (0,40), western strong pre-elite
spot (-18,55), existing elite gate (0,60), and Magnetar arena (0,69). A 16x24m
containment area, narrow southern approach, and western service apron form one
connected runtime section. This is authored directly into Chapter01, with no
new scene, debug entry requirement or change to spot/encounter coordinates.

The kit replaces local dressing with chamfered service decks and recessed grates,
braced bulkheads, armored barriers, hydraulic gate leaves, an amber containment
reactor, capacitor power banks, coolant pumps, freight stacks, conduit racks,
maintenance stations and structural gate supports. The existing directional key
and ambient probe provide cool separation; the rebuilt kit adds no realtime
lights or transparent materials.

## Integration and budgets

- Each enemy has one rigid skinned renderer/material per active LOD and three LODs.
- LOD0 triangle counts: Scout 4,412; Cutter 4,296; Magnetar 7,088.
- All new enemies and environment meshes share one opaque URP industrial atlas.
- In-place Idle/Run/Attack/Hit/Death clips have no gameplay events or root motion.
- The animation bridge observes live state; it does not move authority roots,
  deliver damage, grant rewards, delay recycling or change encounter timers.
- Eight preallocated Scout/Cutter shutdown visuals allow visible death motion
  after immediate gameplay recycling. Saturation skips extra corpse visuals.
- Solid machinery and walls have authored proxies in the existing gameplay world
  collision layer. Art prefabs contain no colliders. Open passages have no
  invisible proxy spans between separate conduit racks.
- Existing gate collision/lock authority controls the replacement gate leaf.
- G-0 remains the same accepted geometry through all three existing progression
  tiers, with restrained tier energy variation and distinct presentation bindings.

## Automated evidence

`production_dependencies.json` verifies all five visual deliverables are reachable
from Chapter01. Android preprocessing enforces that check. Detailed build packing
must also contain the G-0/Scout/Cutter/Magnetar FBX sources and the new reactor/deck
mesh sources; the resulting APK has a companion `.visual-slice.json` report.

Internal camera captures and automated smoke checks are engineering QA. They are
not a human visual gate. The next owner gate is the installed DEV APK on device.

## Remaining device checks

Phone performance, touch feel and visual acceptance require actual owner device
testing. The rest of Chapter01 retains its existing presentation. Broad chapter
art production and additional art phases are deferred until that review.
