# Asset Sourcing and Modeling Strategy — Visual Replacement Production V2

Primary visual authority: approved GRAVIVORE prototype board `Концепт GRAVIVORE: Мехи и окружение.png`.

## 1. Decision framework

Every asset category is assigned a preferred production path:

- **A — Authored from scratch in Blender**
- **B — Licensed donor asset + kitbash**
- **C — AI-generated base mesh + cleanup**
- **D — Existing project asset reuse**
- **E — Hybrid**

Decision order:
1. visual fit to prototype;
2. legal clarity and commercial use;
3. editability;
4. mobile-friendly topology;
5. rig/animation suitability;
6. production time;
7. cost.

Free is not a sufficient reason to use an asset.

## 2. G-0 Tier 0 / Tier 1 / Tier 2

### Recommended path
**E — Hybrid, biased strongly toward A.**

### Why
G-0 is the game identity. Existing generic robots are likely to push the silhouette toward humanoid, tank-like or toy-like forms. The prototype requires a very specific low multi-support form, central cyan core, layered pale armor and techno-organic weapon limbs.

Recommended workflow:
- block silhouette from scratch in Blender;
- optionally use permissive donor mechanical subparts for hidden/internal details, joints, pistons or fasteners;
- optionally use AI concept/base-mesh tools only for ideation or rough shape exploration;
- final visible outer shell, core housing, key limb forms and silhouette-defining parts must be artist-controlled;
- retopology, UVs, material grouping and rigging remain authored.

### Existing project reuse
Only reusable if it serves non-identity elements such as proven socket naming, scale conventions, material presets or hidden structural subparts. Do not preserve rejected proxy geometry merely because runtime wiring exists.

## 3. Scout

### Recommended path
**E — Hybrid.**

Start from authored silhouette. Donor spider/robot joints or mechanical leg components may accelerate production if license is permissive and shapes can be normalized.

Why:
- simple enough to author efficiently;
- silhouette is important;
- repeated enemy benefits from clean topology and simple rig.

AI-generated base mesh may be used as shape exploration, but likely needs full retopo and proportion correction.

## 4. Cutter

### Recommended path
**A or E.**

Blade silhouette and directional attack language are critical. Author body and main cutting structures deliberately. Donor mechanical internals can be kitbashed.

Avoid sourcing a complete “killer robot” and recoloring it.

## 5. Arc Drone

### Recommended path
**A / C hybrid.**

The compact radial form is suitable for AI-assisted base shape generation or quick authored modeling. Final fin geometry, central energy housing, pivot logic and topology should be cleaned manually.

Do not rely on a generic sphere with floating wings.

## 6. Warden

### Recommended path
**E — Hybrid.**

The shield/front mass and heavy body can leverage donor industrial/mech subparts. Main shield silhouette, body proportions and articulation should be authored.

## 7. Carrier

### Recommended path
**B/E — Licensed donor + kitbash with authored silhouette correction.**

Carrier benefits most from industrial/mech donor machinery because its identity is heavy support/hauler mass. A donor can supply mechanical density, but must be reshaped enough to avoid looking like an unrelated purchased asset.

## 8. Magnetar Guard

### Recommended path
**E — Hybrid, high authored share.**

Elite must look like a coherent GRAVIVORE escalation rather than a donor hero model. Use authored macro silhouette and armor. Donor joints/actuators/secondary machinery may accelerate detail.

## 9. Custodian M-0

### Recommended path
**A/E — Authored hero asset with selective donor internals.**

Boss silhouette, core and limb architecture must be bespoke. This is one of the highest-value art assets in Chapter 1.

AI may assist with concept variations and underside/mechanical detail ideation, but the final boss cannot depend on an uncontrolled generated mesh.

## 10. Floor kit

### Recommended path
**B/E — permissive modular donor base + authored normalization.**

Existing CC0/permissive industrial floor modules can save time if:
- scale is normalized;
- materials are rebuilt to GRAVIVORE palette;
- repeated modules are not visually obvious;
- additional authored panels/grates/edge trims are added.

Existing project floor assets may remain only if they meet the new acceptance bar.

## 11. Walls / barriers / gates

### Recommended path
**E — Hybrid.**

Donor structural modules are useful for base frames and detail vocabulary. Gates and hero wall modules should be authored or heavily kitbashed because current simple rectangles are specifically rejected.

Gate mechanical logic must be intentional.

## 12. Reactors

### Recommended path
**E — Hybrid or A for hero reactor.**

Small background reactors can come from donor packs after normalization. The visual-slice hero reactor should be authored/kitbashed to match the board’s tall orange/cyan technology landmarks.

## 13. Pipes / cables / tanks

### Recommended path
**B/D/E.**

These are ideal donor/reuse categories because their forms are generic industrial components. Requirements:
- legal license;
- correct scale;
- consistent bevel/detail density;
- GRAVIVORE materials;
- modular connections;
- mobile budgets.

No need to reinvent every elbow or cylinder.

## 14. Industrial machinery

### Recommended path
**B/E.**

Use permissive donor machinery as raw material, then normalize materials, scale, silhouette and detail density. Combine parts into functional clusters.

## 15. Maintenance and repair-hub equipment

### Recommended path
**E.**

Generic service props can be sourced. Repair hub hero structures need authored cyan/service identity so they do not look like random machinery recolored blue.

## 16. Debris / scrap

### Recommended path
**D/B/E.**

Reuse broken variants of production modules wherever possible. Donor scrap can supplement. This keeps debris visually related to the world.

## 17. Decals

### Recommended path
**A.**

Create project-owned decal atlas for:
- hazard marks;
- numbering;
- maintenance symbols;
- wear;
- soot;
- impact;
- service arrows.

This gives coherence at very low cost.

## 18. Emissive infrastructure

### Recommended path
**A/E.**

Model simple housings in-house; donor industrial fixtures can be adapted. Emissive identity comes from material hierarchy, not expensive geometry.

## 19. Animation assets

### Recommended path
**A/E.**

Mechanical creatures require custom or heavily adapted animation because generic humanoid libraries are unsuitable. Donor animation is unlikely to fit G-0, Scout, Cutter, Magnetar or Custodian.

Use procedural/IK only as implementation support later, not as a substitute for correct rig design and authored poses.

## 20. AI-assisted production policy

AI tools are useful for:
- silhouette exploration;
- concept turnarounds;
- rough hard-surface volume generation;
- alternate armor arrangement ideas;
- texture/material ideation;
- non-hero background prop bases.

AI output is **not production-ready by default**.

Mandatory cleanup:
- retopology;
- removal of fused/invalid geometry;
- correct pivots and articulation;
- UVs;
- material consolidation;
- scale/orientation normalization;
- silhouette matching to prototype;
- mobile optimization;
- license/terms verification for the specific tool/output workflow.

## 21. Existing project reuse policy

Reuse is permitted only when all are true:
- visual fit passes the new bible;
- license/provenance is known;
- topology/material cost is acceptable;
- asset does not carry the rejected cubic/proxy look;
- it does not force the new production to inherit an obsolete silhouette.

Current proxy/simple-kit content is not protected by sunk cost.

## 22. Sourcing criteria

A donor candidate must be scored on:
- silhouette fit;
- detail-density fit;
- editability;
- license clarity;
- commercial use;
- redistribution constraints;
- source format availability;
- topology quality;
- rig quality if animated;
- texture quality;
- material count;
- mobile suitability.

Reject “free” assets with unclear rights or incompatible style.

## 23. Preferred legal posture

Priority:
1. project-authored original;
2. CC0 / public-domain-equivalent;
3. permissive commercial license allowing modification and distribution in game builds;
4. CC BY if attribution burden is acceptable and provenance is recorded.

Avoid:
- NC;
- ShareAlike when incompatible with project distribution goals;
- unclear terms;
- ripped assets;
- recognizable franchise derivatives;
- download sources that cannot prove license/provenance.

## 24. Recommended first-slice method

- G-0 Tier 0: authored from scratch + optional donor internals.
- Scout: authored/hybrid.
- Cutter: authored/hybrid.
- Magnetar: authored macro shell + donor mechanical subparts.
- Floor/wall kit: permissive donor base + authored variants.
- Gate: authored/hybrid.
- Hero reactor: authored/hybrid.
- 4–6 props: mostly sourced donor assets normalized to project style.
- Materials/decals: authored.
- AI: concept and base-shape assistance only, followed by cleanup.

This gives the slice enough original identity to prove the game while spending donor-asset time where uniqueness matters less.