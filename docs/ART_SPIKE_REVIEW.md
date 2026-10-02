# GRAVIVORE Art Spike V2 — visual review

**ART SPIKE V2 VISUAL REVIEW: PENDING**

V1 was not approved by human visual review. These eight images replace its image set and show the actual revised Unity scene. Start with the close hero surface view, then judge silhouettes at the S20 camera.

1. [Tier 2 close three-quarter hero](art-spike/images/08_G0_CloseHero.png), 1920×1080. Inspect armor, joints, forward assemblies and core cage. This uses a closer review camera.
2. [Tier 0 → Tier 1 → Tier 2](art-spike/images/01_G0_Evolution.png), 1920×1080. Same scale/core identity with a one-metre bar and 20 cm ticks.
3. [All tiers at S20 gameplay scale](art-spike/images/07_Evolution_S20Scale.png), 3240×1920. Each panel is a full 1080×1920 render from the unchanged S20 camera. Compare silhouettes on a phone-sized display.
4. [Tier 1 alone at gameplay scale](art-spike/images/02_G0_GameplayScale.png), 1080×1920.
5. [Tier 1 and Cutter](art-spike/images/03_Enemy_GameplayScale.png), 1080×1920. Cyan player is left; red Cutter is upper right. Compare four supports/core cage with two runners/asymmetric blades while ignoring color.
6. [Industrial bay overview](art-spike/images/04_Environment_Overview.png), 1920×1080. Low walls, panel insets, conduits and compact reactor frame the center.
7. [Gameplay composition](art-spike/images/05_Gameplay_Mock.png), 1080×1920. Tier 2 and Cutter in the bay at S20 scale. No HUD or combat simulation.
8. [Intended player/enemy scale](art-spike/images/06_ScaleReference.png), 1920×1080. A closer same-scale reference.

## Visible sources

Characters use existing child meshes from **Unfinished mech sketch by Julius**: a rotated torso shell, intact armored legs, weapon arms and shoulder plates. Cutter also reuses the separate pelvis armor mesh as tapered blades. Head geometry and a complete stock humanoid are not instantiated. Neither character contains Kenney meshes.

The bay uses **Kenney Factory Kit 3.0** and **Kenney Modular Space Kit 1.0** for scenery. Project-owned elements include hierarchies/transforms, shared materials, core/hip/containment primitives, a small ring mesh, static reflection environment, lighting, cameras and capture code.

## Limitations to judge

The selected donor is an unfinished untextured mech, not a finished modern PBR character. Existing planes and joints are reused without vertex editing. Nonuniform transforms compress limb proportions; joints are static; long arm shapes and module intersections may still need a better donor. Armor has no normal/detail maps. The environment remains a simple bay. Thin blades and interior mechanics can disappear at gameplay scale. Tests and fewer renderers do not resolve those visual limitations.

Tier 2 has **25 renderers, 8,292 triangles and four shared materials**, compared with V1's 60 renderers. Characters have no textures, rig or animation. Device frame time is unmeasured. See [performance](art-spike/PERFORMANCE.md), [source decisions](ART_ASSET_SHORTLIST.md), [verification](art-spike/VERIFICATION.md) and [report](ART_SPIKE_REPORT.md).

Attribution: [Unfinished mech sketch](https://opengameart.org/content/unfinished-mech-sketch) by [Julius](https://opengameart.org/users/julius), under [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/). Modified through child selection, duplication, transforms, material replacement and added core/connector geometry. V2 visual adaptations and these renders are distributed under CC BY-SA 3.0; see [license scope](art-spike/ART_LICENSE.md). No endorsement is implied.

The comparison scene remains under Assets/_Game/ArtSpike. Full Chapter 01 replacement, production binding, elite/boss art and other enemy prototypes have not started. Visual approval belongs to the human reviewer.
