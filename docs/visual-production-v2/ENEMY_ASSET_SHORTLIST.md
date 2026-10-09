# Enemy Asset Shortlist — First Visual Slice

Research date: 2026-10-06. Scope: Scout, Cutter, Magnetar Guard. Visual authority: approved GRAVIVORE prototype board and `CHAPTER1_ENEMY_FAMILY_BRIEF.md`.

## Production rule

A marketplace thumbnail is only a visual candidate. `source inspected`, `downloaded`, `topology inspected`, and `approved for production` remain false until the actual source package is obtained and opened.

## Scout — recommended path

Preferred: **donor mesh + substantial silhouette/material rework**. Scout is generic enough to benefit from a donor, but the final result must become a compact low radial hostile machine, not an identifiable stock spider.

### Candidates

1. **Robot spider — PAndras — Sketchfab**
   - URL: https://sketchfab.com/3d-models/robot-spider-c9c7188c7f9e4504b8499f1131693b72
   - License shown: CC BY; commercial use allowed with attribution; redistribution of unmodified source is not permitted by CC BY's normal terms.
   - 18.8k tris / 9.4k verts; Blender + Substance Painter stated.
   - Fit: **B+ donor**. Good radial mechanics and workable density. Needs smaller body, clearer hostile core, thinner faster legs, and full GRAVIVORE rematerial.
   - State: visual candidate + license page inspected; source package not downloaded/topology not inspected.

2. **Spider Robot — Muhammad Hasan Alasady — Sketchfab**
   - URL: https://sketchfab.com/3d-models/spider-robot-7ea58c2e0e7e48f281d7a840f02aa7b1
   - License: CC BY.
   - 7.8k tris / 2k verts. Existing Idle/Movement/Fire/BigBoom animations.
   - Fit: **B donor / strongest economy candidate**. Mobile-friendly and already animated; visual identity is generic and requires shell/core replacement.
   - State: visual candidate + license page inspected; source not downloaded.

3. **Spider Robot (Rigged) — Keralt — Sketchfab**
   - URL: https://sketchfab.com/3d-models/spider-robot-rigged-419632470d3b4328b8d8ea7ae0462ce7
   - License: CC BY.
   - 4.6k tris / 2.5k verts; rigged, game-ready claim.
   - Fit: **B- donor**. Attractive for mobile/rig efficiency, but likely requires the largest visible redesign to avoid cheap-stock reading.
   - State: visual candidate + license page inspected.

4. **Mechanical spider — techdefined123 — Sketchfab**
   - URL: https://sketchfab.com/3d-models/mechanical-spider-f949141ea4b64392866bdabb9f270f7e
   - License: CC BY.
   - 33.2k tris / 16.6k verts; AI-generated textures disclosed.
   - Fit: **B- donor**. Useful articulation/body fragments; texture set should not be trusted as final production art.
   - State: donor-only candidate.

5. **Mechanical Spider Robot — Serg_Stark — Sketchfab**
   - URL: https://sketchfab.com/3d-models/mechanical-spider-robot-1c56b7c7c851413b857900fb0adc72c5
   - License: CC BY.
   - 119.5k tris / 61.3k verts; fully rigged claim; created-with-AI tag.
   - Fit: **C+ donor**. Strong detail source but far too expensive for Scout as delivered; would require aggressive retopo and silhouette simplification.
   - State: donor-only; do not deploy raw.

6. **Crab Robot — Turkey — Sketchfab**
   - URL: https://sketchfab.com/3d-models/crab-robot-a0729527a07e4ba48a99b5e5fb5dfe67
   - License: CC BY.
   - 2.9k tris / 1.7k verts; rigged Blender source claimed.
   - Fit: **C**. Excellent technical economy but too stylized/simple for approved prototype. Use only for rig/anatomy reference or temporary donor parts.

### Scout decision

Acquire/inspect candidates 1–3 first. Target final Scout: ~8–18k tris LOD0, one primary atlas/material plus optional emissive, custom red core housing, custom top shell and limb-root language. Do not ship any donor unchanged.

## Cutter — recommended path

Preferred: **hybrid custom model using a donor locomotion chassis/rig plus custom blades and front shell**. Search did not reveal a ready-made asset that satisfies narrow front-heavy predator + conspicuous blade language without turning humanoid or generic drone.

### Candidates / donor paths

1. **Scorpion mech — gwim — Sketchfab**
   - URL: https://sketchfab.com/3d-models/scorpion-mech-db3225ec5dcf431d8b3b8dc328a42bbb
   - License: CC BY.
   - 23.8k tris / 11.5k verts.
   - Fit: **B donor** for directional chassis and leg anatomy. Remove/replace tail identity; author custom paired cutting structures.

2. **Scorpion Robot — Mirandanimator — Sketchfab**
   - URL: https://sketchfab.com/3d-models/scorpion-robot-e44be9a622af4e85b4b10cf59b25de1a
   - License: CC BY.
   - 29.3k tris / 16k verts.
   - Fit: **B- donor**. Mechanical arthropod language is useful; requires complete front-end redesign and material replacement.

3. **Low Poly Robot Scorpion — oconop23 — Sketchfab**
   - URL: https://sketchfab.com/3d-models/low-poly-robot-scorpion-fa43c044b78445468fe2af0b78a2e899
   - License: CC BY.
   - 5.7k tris / 2.8k verts.
   - Fit: **C+ economy donor**. Good lightweight base/rigging study, insufficient authored detail for final slice without major rebuild.

4. **Mech Drone — Willy Decarpentrie — Sketchfab**
   - URL: https://sketchfab.com/3d-models/mech-drone-8d06874aac5246c59edb4adbe3606e0e
   - License: CC BY.
   - 9.1k tris / 5.2k verts.
   - Fit: **C+ parts donor**. Compact hard-surface pieces can seed a custom Cutter torso; not acceptable as final body.

5. **Mantis — Leyde — Sketchfab**
   - URL: https://sketchfab.com/3d-models/mantis-dd631c02928540f49bab339c6adbd4ff
   - License: CC BY, but asset is tagged NoAI; do not feed into generative tools.
   - 18.2k tris / 9.1k verts.
   - Fit: **anatomy/rig donor only**. Strong directional insect anatomy and forelimb language, but it is biological rather than mechanical.

6. **Mechanical Scorpion — SINNIK — Sketchfab**
   - URL: https://sketchfab.com/3d-models/mechanical-scorpion-098fe7f9592248a0be5b0d6d56cab056
   - License: CC BY.
   - 512.1k tris / 259.8k verts.
   - Fit: **D+ donor**. Useful high-detail design reference only; prohibitive cleanup/retopo for first slice.

### Cutter decision

Use candidate 1 or 2 only if source topology/rig quality is good after download. Build custom forward blade pair, core housing, and top shell. Target ~14–24k tris LOD0. Attack direction must remain obvious with emission disabled.

## Magnetar Guard — recommended path

Preferred: **custom/hybrid elite**, using high-quality multi-legged donor components only where they reduce mechanical labor. Do not buy a stock boss and recolor it.

### Alternatives

1. **Gunslinger Crab Mech v1.2 — Vaportrash — Sketchfab**
   - URL: https://sketchfab.com/3d-models/gunslinger-crab-mech-v12-5147619f337a45b0974f349eac27b34a
   - License: CC BY.
   - 91.5k tris / 47.5k verts.
   - Fit: **B donor only**. Broad stance and heavy mass are useful, but obvious military weapons must be completely discarded. Retopo required.

2. **Mechanical Spider — Preview_Tempest — Sketchfab**
   - URL: https://sketchfab.com/3d-models/mechanical-spider-d1f67d2e995d4c81a056242df1b97390
   - License: CC BY.
   - 101.3k tris / 57.4k verts.
   - Fit: **B- donor**. Plausible joint connections and large boss-like leg mechanics; silhouette requires substantial narrowing/reconstruction for Magnetar rather than Custodian.

3. **Stalenhag Environment Project: Spider Mech — Enrico Labarile — Sketchfab**
   - URL: https://sketchfab.com/3d-models/stalenhag-environment-project-spider-mech-ec5914b53b6a4cde8de4820050bc46c5
   - License shown: CC BY.
   - 103.4k tris / 54.8k verts.
   - Fit: **B- design/parts donor**. Strong authored industrial realism but too recognizable and not GRAVIVORE-shaped as-is; heavy retopo/kitbash required.

4. **Mechanical Spider Robot — Serg_Stark — Sketchfab**
   - URL above in Scout list.
   - 119.5k tris, rigged claim.
   - Fit: **C+ donor**; high cleanup burden but articulated hard-surface structure may save rigging work.

5. **Custom Blender model from prototype**
   - License risk: none for internally authored work.
   - Fit: **A**.
   - Cost/time: highest manual labor, lowest visual compromise.

### Magnetar decision

Build a custom elite around one dominant contained energy focal point and a broad multi-leg stance; donor limbs/joint housings may come from candidates 1–3 only after topology/license/source inspection. Target ~35–55k tris LOD0, 3–4 material slots max before consolidation, custom rig.

## Explicit rejects

- `spiderbot` by timgroote: CC BY-ND means derivative editing/kitbash is incompatible with the required production path; **REJECT**.
- AT-99 Scorpion: CC BY-NC and uncertain provenance disclosed by uploader; **REJECT**.
- Generic humanoid robots / military exosuits / bipeds: **REJECT by visual contract**, regardless of technical quality.

## Download/inspection priority

1. PAndras Robot spider — Scout.
2. Muhammad Hasan Alasady Spider Robot — Scout economy/animation baseline.
3. Keralt Spider Robot (Rigged) — Scout ultra-light baseline.
4. gwim Scorpion mech — Cutter chassis.
5. Mirandanimator Scorpion Robot — Cutter alternative.
6. Vaportrash Gunslinger Crab Mech — Magnetar parts study.
7. Preview_Tempest Mechanical Spider — Magnetar joint study.

All remain `PROMISING — REVIEW SOURCE FILES FIRST` until actual archives are opened in Blender.
