# Visual Acceptance Checklist — Visual Replacement Production V2

Primary visual authority: approved GRAVIVORE prototype board `Концепт GRAVIVORE: Мехи и окружение.png`.

This checklist is a hard gate for production approval. A technically correct import does not equal visual acceptance.

## 1. Silhouette

PASS only if:
- G-0 reads as a low, non-humanoid, multi-support techno-organic machine;
- G-0 facing direction is obvious from the real gameplay camera;
- player central core remains visible and dominant;
- Scout, Cutter, Arc Drone, Warden and Carrier are distinguishable in under one second;
- Magnetar reads as elite before color/emission is considered;
- Custodian reads as boss-scale before VFX is considered;
- major negative spaces between limbs/body survive phone-size viewing.

REJECT if:
- cubic;
- toy-like;
- flat silhouette;
- tank-with-legs read;
- generic humanoid robot;
- ordinary enemies differ only by color or weapon swap;
- boss looks like a scaled ordinary enemy.

## 2. Shape hierarchy and detail density

PASS only if:
- large forms establish identity first;
- medium armor/joint/mechanical forms support the silhouette;
- small detail is localized and functional;
- top-facing surfaces carry enough readable structure for the camera;
- hero assets have layered geometry rather than painted fake seams.

REJECT if:
- primitive-kit appearance;
- greeble noise with no hierarchy;
- smooth featureless shells;
- detail only looks convincing in close-up;
- repeated boxes make up most environment complexity.

## 3. Materials

PASS only if:
- graphite structure, mid-tone metal and armor are materially distinct;
- pale G-0 armor does not look plastic;
- metallic/roughness variation is visible but coherent;
- exposed wear appears in plausible contact/heat/service zones;
- emissive surfaces are localized and integrated into geometry.

REJECT if:
- overly clean;
- same roughness everywhere;
- random uniform grunge;
- glossy toy plastic;
- pure black structure with no readable form;
- emissive texture pasted onto otherwise weak geometry.

## 4. Color hierarchy

PASS only if:
- G-0 cyan remains a strong player identity cue;
- hostile red/orange cues read without flooding the whole screen;
- Arc Drone blue does not cause player confusion;
- repair/service cyan is subordinate to player core;
- boss red/orange is strongest hostile focal point;
- neutral environment remains mostly graphite/gray.

REJECT if:
- excessive neon;
- every wall/floor edge glows;
- color coding replaces silhouette design;
- saturated environment competes with combat.

## 5. Readability

PASS only if:
- player silhouette remains readable while moving/attacking;
- enemy attack origins are obvious;
- weak/focal areas can be found from gameplay camera;
- floor, obstacle and character values separate on phone screen;
- VFX does not hide the player or enemy body for most of the attack.

REJECT if:
- unreadable at game-camera scale;
- model only looks good in close-up;
- thin details collapse into noise;
- telegraphs merge with permanent environment emission;
- player disappears against floor/background.

## 6. World density

PASS only if:
- scene reads as a functioning industrial facility;
- large empty surfaces contain authored non-blocking construction detail;
- periphery includes coherent machinery clusters;
- combat center remains usable/readable;
- repeated modules are disguised through composition and variation;
- gates/walls have real secondary form language.

REJECT if:
- environment looks like a test scene;
- giant empty floor;
- random prop scattering;
- giant simple rectangles;
- fake scenery with no functional logic;
- one repeated low-detail kit dominates the world.

## 7. Lighting

PASS only if:
- dark structure remains readable;
- warm practicals create industrial depth;
- cool player/service accents remain distinct;
- boss/elite areas intensify threat without crushing readability;
- phone brightness reduction still preserves gameplay silhouettes;
- bloom supports, rather than defines, the look.

REJECT if:
- black crush hides geometry;
- all lighting is flat/ambient;
- bloom erases detail;
- one giant red/orange wash hides role colors;
- scene only looks good in a beauty-shot exposure.

## 8. Armor layering and mechanics

PASS only if:
- hero assets show dark internal structure, structural housing and outer armor layers;
- joints have plausible pivots/clearance;
- moving parts look mechanically connected;
- armor does not visibly intersect through required animation range.

REJECT if:
- monolithic shell with fake painted seams;
- limbs simply intersect the torso;
- impossible hinge axes;
- armor clipping is built into neutral pose.

## 9. Animation compatibility

PASS only if:
- riggable parts are separable;
- pivots match visible mechanical axes;
- weapon geometry supports attack poses;
- core/weak-point sockets remain stable;
- locomotion does not destroy the silhouette;
- boss telegraph poses can be made readable without deforming hard surface unnaturally.

REJECT if:
- model must be remodeled to articulate;
- joints collapse on first pose;
- attack origin has no physical relationship to weapon geometry;
- rig requires humanoid assumptions for non-humanoid body.

## 10. Mobile performance

PASS only if:
- triangle and renderer counts are within `MODEL_DELIVERY_STANDARD.md` targets or justified by profiling;
- material slots are consolidated;
- texture sizes are reasonable for portrait Android;
- LODs exist where value is meaningful;
- silhouette-changing geometry is prioritized over hidden detail;
- alpha/transparency is restrained.

REJECT if:
- 4K textures are used by default;
- many tiny material slots fragment one prefab;
- hidden underside receives hero-level geometry cost;
- boss/elite has no LOD strategy despite high complexity;
- effect/material cost is obviously unsuitable for repeated mobile combat.

## 11. Gameplay-camera appearance

This is the final authority.

PASS only if actual gameplay captures demonstrate:
- G-0 identity matches approved prototype language;
- enemies remain distinguishable during combat;
- environment looks production-authored;
- elite/boss hierarchy survives camera distance;
- attack/death presentation feels modern;
- no key visual depends on close-up inspection.

REJECT if:
- screenshot immediately recalls the old cubic/prototype baseline;
- scene reads as Unity test geometry;
- model quality collapses when camera pulls back;
- details become noise and silhouette remains generic.

## 12. Explicit reject reasons

Any one of these can block approval:

- cubic;
- toy-like;
- generic sci-fi;
- primitive-kit appearance;
- overly clean;
- excessive neon;
- flat silhouette;
- unreadable at game-camera scale;
- model only looks good in close-up;
- environment looks like a test scene;
- G-0 becomes humanoid/exosuit/tank-like;
- Magnetar does not read as elite;
- Custodian does not dominate the arena;
- walls/gates remain rectangular blockouts;
- attack/death language still feels placeholder/dated;
- donor asset looks stylistically foreign;
- license/provenance unclear;
- technical integration is correct but visual target is missed.

## 13. Review evidence required

For each hero review include:
- real portrait gameplay-camera screenshot;
- silhouette-only or low-emission check;
- phone-size check;
- one close-up surface/model check;
- triangle/material/renderer counts;
- comparison against prototype role/silhouette.

Environment review additionally includes:
- wide gameplay frame;
- gate/wall close comparison;
- density/readability check with combat entities present.

## 14. Grade mapping

### A
Production-ready visual direction. Safe to scale.

### B
Correct direction with a short, bounded fix list. Fix before broad replication.

### C
Internal baseline only. Not safe to scale.

### Reject
Fundamental mismatch, prototype look, technical unsuitability or severe readability failure.

**Anything C or below must not scale to the rest of Chapter 1.**