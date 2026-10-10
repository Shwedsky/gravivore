# GRAVIVORE current art direction — industrial hard-surface sci-fi

Status: **CURRENT / AUTHORITATIVE**

Primary visual contract: `docs/CURRENT_VISUAL_TARGET.md`.

Chapter 01 actor anatomy follows the exact [owner-approved Actor V2 PNG](visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png), [V2 textual contract](visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md) and package README. Newest explicit owner correction > V2 PNG > V2 text > CURRENT visual target > this art direction > donor audit/matrix > historical evidence. Stop actor production if the exact PNG is missing; do not substitute an old board. World/layout remains under Chapter 01 Visual Blueprint V1.

## G-0

Current G-0 is a **bipedal robotic combat mech**.

Required read:
- separate torso/body mass;
- head/visor or equivalent readable sensor mass;
- two mechanical legs with believable joints and load-bearing feet;
- arms/tool/weapon mounting that reads at the portrait gameplay camera;
- central cyan gravity-core identity integrated into the body;
- non-human machine anatomy rather than a soldier in powered armor;
- visible equipment/weapon changes when equipped;
- evolution changes silhouette through added/replaced authored geometry, not only scale or brighter emission.

Historical low four-support/radial/tank-like G-0 directions are superseded and must not be resurrected from old phase docs or concept-board player anatomy.

## Hostile machines

Shared faction language:
- serious modern sci-fi combat machines;
- dark graphite exposed mechanics;
- cool/pale metal armor shells;
- layered armor over visible joints/actuators;
- localized red/orange hostile energy by default, preserving the approved V2 Arc Drone's visible blue core/weapon energy;
- mechanically plausible supports and attack sources;
- role-specific silhouettes readable from the real gameplay camera;
- no toy proportions;
- no low-detail box-mech final forms;
- no color-only role differentiation;
- no generic stock identity after production adaptation.

All Chapter 01 actor specifics now follow [Actor Visual Targets V2](visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md). Scout is a light blade-arm biped, Warden a heavy biped holding two separate shields, Carrier a low streamlined enclosed vehicle and Arc Drone an airborne energy machine. Cutter may use quadruped mechanics; elite Magnetar and boss Custodian require distinct custom heavy architecture. Shared faction materials do not imply one shared body family.

`docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md` is retained superseded evidence, not a current anatomy source. In particular its radial/spider Scout rule must not be used for new production. Donor mechanics must be adapted to V2, never the reverse.

## Environment

The world is a dense but readable industrial exclusion complex, not a quiet open floor with decoration around the edges.

Target composition:
- layered floor construction, seams, trenches, grates and damaged transitions;
- substantial wall/gate thickness and service framing;
- connected pipes/cables/services with visible endpoints and purpose;
- large machinery/facilities that explain the function of a zone;
- reactors, generators, containment systems, docks, gantries, cranes, storage and wreckage;
- broken/collapsed silhouettes and wear where the setting is damaged;
- strong edge/periphery massing and vertical machinery;
- open encounter centres and protected telegraph lanes.

A gameplay `SpawnSpot` is only spawn authority. Its presentation may be a substantial facility/dock/fabricator or other authored industrial structure if that best communicates the concept.

## Materials

Core palette:
- graphite/near-black structure;
- cool desaturated steel;
- worn painted machinery;
- dirty deck/concrete/industrial surfaces;
- restrained rust/heat/damage;
- cyan/blue service/player energy;
- red/orange hostile/danger energy by default; retain blue for Arc Drone where approved by the V2 actor image;
- amber/ochre warning paint where functional.

Use PBR surface depth and within-object variation. Do not let shared-material optimization make unrelated objects look like the same plastic construction toy.

## Lighting

Lighting should reveal mass, recesses and materials while keeping targets/telegraphs readable.

Prefer efficient techniques suitable for mobile: directional/baked/emissive/reflection/fake-light treatment first, bounded local realtime lights only where the gain is real.

## Camera-first acceptance

Judge production art in the actual playable portrait camera on device.

Beauty renders and close material views help diagnose quality but do not replace the gameplay-camera gate. Fine detail that cannot be perceived at gameplay scale must not carry the design; primary and secondary forms must do the work.

## Production quality

Primitive/procedural proxy modules, obvious blockout geometry and untouched stock models are acceptable for technical exploration only.

When the active task is a production/concept-fidelity pass, final acceptance requires authored/refined forms and surfaces that materially close the gap to the owner concept rather than simply making the previous proxy more complex.
