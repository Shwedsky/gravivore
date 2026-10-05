# Material, Color and Lighting Bible — Visual Replacement Production V2

Primary visual authority: approved GRAVIVORE prototype board `Концепт GRAVIVORE: Мехи и окружение.png`.

This document converts the prototype’s visible material/color hierarchy into production ranges. Values are guides for consistency, not absolute shader locks.

## 1. Global philosophy

GRAVIVORE uses dark industrial metals as the visual field, pale/cool armor to separate the player, and restrained energy colors to communicate role.

The target is:
- dark but readable;
- metallic but not mirror-polished;
- worn but not uniformly dirty;
- colorful through focal accents, not rainbow surfaces;
- strong warm/cool separation;
- mobile-readable at reduced screen brightness.

## 2. Graphite / dark structural metal

### Role
Internal frames, joints, underside, rails, wall frames, support structures.

### Base color
Approximate linear/sRGB visual range:
- RGB ~18–45 / 22–50 / 28–58
- neutral-to-cool near-black graphite, never pure black.

### Metallic
0.65–1.0 depending painted vs bare structure.

### Roughness
0.38–0.68.

### Wear
Polished exposed edges, oil/dirt in recesses, small localized scrape highlights.

### Rule
Dark structure should remain visible under level lighting; do not crush it into a black silhouette.

## 3. Mid-tone machinery metal

### Role
Environment shells, machine housings, pipes, tanks, neutral enemy structures.

### Base color
RGB ~65–110 / 70–115 / 75–125.

### Metallic
0.55–0.95.

### Roughness
0.35–0.65.

### Wear
Directional edge wear, panel grime, heat staining near vents, occasional paint abrasion.

## 4. Player pale armor

### Role
Primary G-0 shell separation from dark environment.

### Base color
Cool desaturated gray rather than white:
- RGB ~145–190 / 155–200 / 165–210.

### Metallic
0.45–0.8 depending coating.

### Roughness
0.30–0.52.

### Wear
Chipped leading edges, dark recess dirt, occasional polished contact scratches.

### Rule
Never become glossy white plastic.

## 5. Worn edges / exposed metal

### Role
Secondary material state, not a standalone dominant color.

### Base color
Slightly brighter or warmer than structural metal.

### Metallic
0.85–1.0.

### Roughness
0.22–0.42.

### Usage
- blade edges;
- foot contact;
- handles / service zones;
- battered frame corners;
- gate tracks;
- frequently struck armor.

Avoid uniform edge-generator appearance across every asset.

## 6. Hazard materials

### Role
Industrial safety and navigation accents.

### Colors
- burnt orange / amber: RGB ~180–235 / 75–135 / 20–55;
- muted hazard yellow where needed: RGB ~185–225 / 145–190 / 35–65.

### Metallic
0.0–0.35 for painted coating.

### Roughness
0.45–0.72.

### Rule
Use in strips, panels, service handles and danger zones. Hazard paint must not become the default environment color.

## 7. Warm emissive

### Role
Industrial power, reactors, practical lights, active machinery.

### Hue
Amber/orange tending toward hot orange in core centers.

### Visual behavior
- bright core;
- softer warm spill from actual light sources where budget allows;
- strongest on reactors and selected industrial landmarks;
- smaller machinery uses lower intensity.

### Emissive strength philosophy
Author relative hierarchy rather than a single numeric multiplier:
- indicator: 1x;
- local practical: 2–3x;
- machine energy aperture: 3–5x;
- hero reactor core: 5–8x visual intensity before bloom tuning.

Do not clip broad areas to flat white/orange.

## 8. Cyan / blue emissive

### Role
G-0 identity, repair/service systems, advanced energy technology.

### Hue
Electric cyan to cool blue.

### Priority
G-0 central core is the highest player-side cyan focal point.

### Usage
- central gravity core;
- narrow weapon channels;
- repair hub energy;
- selected advanced technological props;
- limited navigation cues.

### Rule
Do not put cyan on every neutral environmental light; otherwise player identity is diluted.

## 9. Enemy danger accent

### Ordinary hostile color
Red to hot red-orange.

### Base accent
RGB roughly 180–255 / 25–75 / 20–45 for bright zones, darker surrounding red-orange for housings.

### Role
Core, weapon charge, attack-origin indicators.

### Rule
Hostility must remain clear if emission is reduced. Color cannot replace silhouette.

## 10. Repair/service accent

### Color
Cool cyan / clean blue, slightly less saturated than G-0 core.

### Role
Repair hub, safe-service devices, diagnostics, charging structures.

### Rule
Safe/service lighting should feel orderly and cleaner than hostile machinery but remain within the same industrial world.

## 11. Boss accent

### Color
Deep red with hot red-orange core center.

### Role
Custodian M-0 central core and major attack systems.

### Hierarchy
Boss core may be the strongest hostile emissive in Chapter 1, but telegraphs and attack VFX must still have room to become temporarily brighter.

## 12. Dirt and wear philosophy

Wear must answer a functional question.

### High wear zones
- feet / ground-contact parts;
- blade edges;
- gate tracks;
- protruding corners;
- service handles;
- reactor vents;
- pipe joints;
- floor traffic lanes.

### Dirt collection zones
- panel recesses;
- lower wall corners;
- machinery bases;
- underside surfaces;
- cable trenches.

### Heat zones
- exhausts;
- reactor collars;
- weapon emitters;
- high-load joints if visually justified.

### Forbidden
- same grunge mask everywhere;
- random edge chipping with no contact logic;
- clean pristine hero models inside a worn facility;
- unreadable noise at mobile camera distance.

## 13. Roughness hierarchy

Use roughness to distinguish materials even when colors are close:
- coated player armor: medium-satin;
- raw structure: medium rough;
- polished wear edges: lower roughness;
- rubber/cable: high roughness, nonmetal;
- energy glass: lower roughness with controlled specular response;
- dirty floor: higher roughness except polished traffic zones.

## 14. Metallic behavior

Most structural and armor materials are metallic or painted metal, but not all visible surfaces should return identical metallic values.

Use non-metallic materials for:
- cable insulation;
- rubber seals;
- decals/paint coatings where shader workflow supports it;
- energy-glass housings;
- selected plastic/ceramic service components.

## 15. Color allocation by role

### G-0
Pale cool armor + dark structure + cyan energy.

### Scout/Cutter/Warden
Dark/gray hostile bodies + restrained red core/weapon accents.

### Arc Drone
Dark body + blue energy center, differentiated from player through compact floating radial silhouette and hostile housing.

### Carrier
Dark/gray body + stronger industrial orange armor/service accents.

### Magnetar
Dark heavy armor + amber/orange energy and increased warm accent density.

### Custodian
Dark/red structural armor + dominant red/orange core.

### Environment
Graphite and mid-gray dominate. Warm orange practical lighting is common but localized. Cyan marks higher-tech/service systems.

## 16. Lighting philosophy

The approved board’s gameplay example relies on warm industrial landmarks, cool technology, darker floor/structure, and high-value character cores.

### Ambient / fill
- enough cool-neutral fill to preserve metal form;
- no pitch-black corners in active combat space;
- preserve visible material variation without flattening contrast.

### Practical lights
Use visible fixtures or reactor apertures to justify warm light pools.

### Key composition
At least one warm focal source per major environment composition, with cool counterpoint where G-0/service technology appears.

### Character readability
G-0 should not require a dedicated spotlight. Material/value hierarchy must make it readable naturally.

## 17. Phone-screen readability

Lighting must survive:
- portrait 9:16;
- moderate brightness;
- smaller display sizes;
- compression and bloom loss;
- outdoor-ish viewing conditions.

Rules:
- preserve 10–20% luminance separation between adjacent large surfaces where possible;
- avoid large near-black floor regions without edge definition;
- keep player shell lighter than average floor/background around it;
- hostile cores should be visible without becoming giant blobs;
- telegraph colors must not merge with permanent floor emission;
- check screenshots at 25–35% native resolution before approval.

## 18. Bloom

Bloom should:
- soften energy core edges;
- create small bright halos around major practical lights;
- support VFX impact.

Bloom must not:
- erase model detail;
- merge separate emissive zones;
- turn floor guidance into fog;
- replace real geometry/material contrast.

## 19. Neon usage cap

Avoid “cyberpunk neon” as default language.

Good:
- one cyan core;
- one orange reactor chamber;
- narrow status strips;
- short route markers.

Bad:
- every wall edge glowing;
- every floor seam emissive;
- all pipes lit;
- player entirely outlined in cyan;
- red enemy armor covered in emissive trim.

## 20. Material acceptance checks

Reject if:
- armor looks like plastic;
- dark structure collapses to black;
- every material has same roughness;
- wear is uniform/procedural-looking;
- emission is needed to make silhouette readable;
- environment saturation competes with combat;
- phone-scale screenshot loses floor, player or enemy separation;
- boss and elite are differentiated mainly by brighter color instead of geometry.