# GRAVIVORE — Approved Enemy Visual Targets V1

Status: **APPROVED BY OWNER**  
Date: 2026-10-06  
Scope: Scout, Cutter, Magnetar visual direction for Visual Production V2.

## Authority

This document records the owner-approved enemy art direction for the next production pass.

The approved reference board was created from the current GRAVIVORE visual direction and is the artistic target for enemy production. Codex must treat this document as an art-direction gate, not as loose inspiration.

The reference board itself is currently held outside the repository in the active project conversation under the name:

`визуальные_цели_врагов_gravivore.png`

When the current G-0 V2.1 branch is finished, copy that board into the repository as:

`docs/visual-production-v2/reference/ENEMY_VISUAL_TARGETS_APPROVED_V1.png`

Until the binary is added, the text rules below are authoritative.

## Shared faction language

All three enemies belong to one coherent hostile machine faction.

Shared requirements:
- serious modern sci-fi combat machines;
- dark graphite exposed mechanics;
- pale / cool metallic armor shells;
- localized red-orange hostile energy;
- layered armor over visible joints and actuators;
- aggressive angular hard-surface architecture;
- readable silhouettes from the portrait gameplay camera;
- mechanical plausibility at joints and articulation points;
- no toy-like proportions;
- no low-detail box-mech final forms;
- no humanoid soldier-in-armor look;
- no neon-everywhere treatment;
- no generic stock-asset appearance after the production pass.

Silhouette differentiation is mandatory. Color alone must never be the main identifier.

## Scout — approved target

Role:
- fastest ordinary enemy;
- recon / pressure / close harassment;
- fragile individually, dangerous in groups.

Approved visual read:
- low radial / spider-like body;
- thin, fast articulated legs;
- compact central body mass;
- narrow pointed forward forms;
- small localized red hostile core;
- lightest armor coverage of the three;
- movement silhouette must imply speed and agility.

Production intent:
- Hasan Spider Robot remains a donor candidate, not final art;
- retain only useful rig/locomotion mechanics where justified;
- rebuild shell/core/weapon identity so the donor is not visually recognizable;
- final Scout must read immediately as the lightest hostile unit.

Reject if:
- too bulky;
- too symmetrical and static;
- looks like a toy spider;
- donor identity remains obvious;
- silhouette becomes confused with Cutter.

## Cutter — approved target

Role:
- medium ordinary close-combat enemy;
- frontal pressure / higher damage;
- forces player movement and positioning.

Approved visual read:
- larger and heavier than Scout;
- forward-biased predator silhouette;
- conspicuous paired cutting / blade structures;
- stronger frontal armor;
- lower, aggressive stance;
- red hostile energy concentrated near core and cutting assemblies;
- broader mechanical mass while remaining mobile.

Production intent:
- Mirandanimator Scorpion Robot may be used only as a chassis donor;
- stock tail/front identity must be removed;
- custom blades, core housing, upper shell and attack-facing silhouette are mandatory;
- final model must not read as a recolored scorpion.

Reject if:
- blades are small decorative details;
- silhouette reads as Scout with more armor;
- tail/scorpion donor identity dominates;
- attack direction is unclear from gameplay camera.

## Magnetar Guard — approved target

Role:
- elite enemy;
- heavy area-control threat;
- stronger armor and pressure than ordinary units.

Approved visual read:
- much heavier and wider than Scout/Cutter;
- broad low multi-support stance;
- major layered armor masses;
- large contained orange/amber central energy focal point;
- visible heavy joints, pistons and mechanical support systems;
- strong elite silhouette before emissive effects are considered;
- orange energy language differentiates elite threat from ordinary red accents.

Production intent:
- donor sources are mechanics pools only;
- torso, armor hierarchy, containment/core assembly and elite silhouette must be custom;
- useful crab/spider leg/joint mechanisms may be retained after provenance and topology review;
- final Magnetar must not be a scaled-up ordinary enemy.

Reject if:
- it reads as Scout/Cutter enlarged;
- only scale and glow distinguish it;
- central core overwhelms the body like a floating lamp;
- heavy armor makes the model visually static or toy-like.

## Relative hierarchy

Required visual order:

Scout < Cutter << Magnetar

This must be visible through:
1. silhouette mass;
2. limb reach;
3. armor layering;
4. mechanical complexity;
5. core / energy containment;
6. emissive intensity.

Do not solve hierarchy only by scaling the same design.

## Gameplay-camera rule

All production approval must include real or representative top-down / portrait gameplay-angle evidence.

At phone size:
- Scout must read as fast/light;
- Cutter must read as blade-focused frontal threat;
- Magnetar must read as elite immediately;
- the three must remain distinguishable even with emissive intensity reduced.

Beauty renders are supplementary only.

## Codex implementation rule

When Codex starts Enemy Production V2:
- use this document together with the approved visual target board;
- use existing donor/intake provenance from PR #54/#55/#56 only where useful;
- do not invent a new faction art direction;
- do not reinterpret these three enemies into generic marketplace sci-fi;
- do not integrate into Chapter01 until blockouts pass human art-direction review;
- stop after Scout/Cutter/Magnetar blockout evidence for review before final texturing/rig polish.

## Owner decision

The owner explicitly approved the generated Scout / Cutter / Magnetar concept sheet on 2026-10-06.

This direction is therefore the visual target for the next enemy modeling pass.
