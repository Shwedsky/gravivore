# First-slice asset manifest — measured local intake

2026-10-06. Supersedes PR #55's unavailable-source gate for these thirteen actual archives. Reference art/design: PR #53; provenance: PR #54/#55 plus official rechecks recorded in modeling results. No whole donor body is approved unchanged.

- G0-T0: **Blockout V1 READY FOR ART-DIRECTION REVIEW, grade B**. `art/visual-production-v2/g0/G0_Blockout_V1.blend`: 15,080 triangles, 87 meshes / 107 model objects, four material swatches, zero donor parts. Source gate committed before modeling. Outer identity, four supports, core cavity, weapons and rear mass are custom. Seven PNGs and strict limitations: `modeling/G0_BLOCKOUT_V1_REVIEW.md`. No production/Unity approval.
- EN-SCOUT: Hasan Spider Robot, B preferred donor; actual `.blend`, rig and clips inspected. Custom shell/core/weapon, topology repair and retargeted motion required.
- EN-CUTTER: Mirandanimator Scorpion Robot, B preferred chassis donor; eight static rigid objects inspected. Tail/front replacement, optimization, rig and animation required.
- EN-MAG: custom torso/armor/core; restricted Vaportrash `crab_legs` and Preview_Tempest `Cylinder001` generic mechanism pool only. Stalenhag reuse rejected.
- ENV-FLOOR/WALL/CORNER/BARRIER/COLUMN: explicit B donor subset in `modeling/ENVIRONMENT_FIRST_SLICE_SUBSET.md`; sixteen unique pieces, no final kit approval.
- ENV-PIPE: selected PipeHolder donor; extra routing/flanges custom.
- ENV-TANK: Barrel_Large fragment only; substantial tank custom required.
- ENV-MACHINERY/REACTOR: selected fan/vent/access-point components; hero machinery custom required.
- ENV-GATE: hollow-column/truss donors only; authored leaf, frame treatment and mechanisms custom required.
- MAT-ARMOR/FLOOR/GRATE/WALL: four Poly Haven CC0 source sets inspected; fourteen 2K preparation maps stored outside Unity. Not applied as final G-0 textures.

Source archive hashes, imported measurements, candidate grades and limitations: `modeling/SOURCE_INSPECTION_RESULTS.md`. No runtime prefab, scene or gameplay asset is changed. Next gate: G-0 Blockout V1 art-direction review; stop there before production texturing/rigging/enemy completion or Unity integration.
