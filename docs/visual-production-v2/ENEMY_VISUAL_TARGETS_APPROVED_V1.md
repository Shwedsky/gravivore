# GRAVIVORE — Approved Enemy Visual Targets V1

Status: **APPROVED BY OWNER**  
Date: 2026-10-06  
Scope: Scout, Cutter, Magnetar visual direction for Visual Production V2.

## Authority

This document records the owner-approved enemy art direction for the next production pass.

The approved reference board was created from the current GRAVIVORE visual direction and is the artistic target for enemy production. Codex must treat this document as an art-direction gate, not as loose inspiration.

The reference board itself is currently held outside the repository in the active project conversation under the name:

`визуальные_цели_врагов_gravivore.png`

Intended repository path when available:

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

All production approval must be judged in the real playable runtime and portrait gameplay camera.

At phone size:
- Scout must read as fast/light;
- Cutter must read as blade-focused frontal threat;
- Magnetar must read as elite immediately;
- the three must remain distinguishable even with emissive intensity reduced.

Beauty renders and still boards are supplementary evidence only and are NOT a human acceptance gate.

## Codex implementation rule — superseded process

Owner process decision updated 2026-10-06:

- DO NOT stop after enemy blockouts for human review;
- DO NOT request approval of isolated renders, screenshots, model sheets, Blender views or evidence boards;
- continue through minimum production-quality Scout/Cutter/Magnetar assets, runtime integration and first visual-slice environment production;
- integrate the approved G-0 Production V3 into the live playable runtime;
- create one visually replaced industrial combat spot/area with new floor/wall/barrier/gate/reactor/prop treatment;
- preserve gameplay authority, encounter logic, collisions and Chapter01 progression unless a presentation-only adaptation is required;
- validate the complete slice in the actual gameplay camera;
- build an installable Android ARM64 DEV APK containing the integrated visual slice;
- the NEXT HUMAN VISUAL ACCEPTANCE GATE IS THE APK RUNNING ON DEVICE.

Use existing donor/intake provenance from PR #54/#55/#56 only where useful. Do not inherit their rejected or superseded G-0 art direction.

Do not invent a new enemy faction art direction and do not reinterpret these three enemies into generic marketplace sci-fi.

## Owner decision

The owner explicitly approved the generated Scout / Cutter / Magnetar concept sheet on 2026-10-06.

The owner subsequently changed the delivery process: visual corrections will be given only after reviewing a playable APK. Intermediate model/render approval is no longer required for this first visual slice.

This direction is therefore the visual target for the next enemy modeling + integration pass, with APK-on-device as the next human gate.
