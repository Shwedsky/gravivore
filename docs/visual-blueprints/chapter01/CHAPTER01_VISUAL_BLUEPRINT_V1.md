# GRAVIVORE Chapter 01 Visual Blueprint V1

Status: **CURRENT / OWNER-APPROVED BLUEPRINT**

This document is the current scene-composition authority for the next full Chapter 01 visual rebuild.

It supplements, and does not replace:
- `docs/CURRENT_VISUAL_TARGET.md`;
- `docs/ART_DIRECTION.md`;
- `docs/ASSET_SOURCE_OF_TRUTH.md`;
- current gameplay specifications.

Expected visual board path:

`docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.jpg`

If the board image is unavailable in a working copy, use this document as the composition contract and record the missing board as a blocking evidence limitation before a full visual rebuild.

## 1. Why this blueprint exists

Previous visual passes improved materials, props, density and actor meshes while preserving too much of the old physical scene. The result became a better version of the same map rather than a new environment that matched the approved concept.

The next Chapter 01 rebuild must preserve **gameplay semantics**, not the previous presentation layout.

Do not treat the old V44–V47 scene composition, old collision fingerprint, exact old spot coordinates, old floor grid, old facility footprints or previous presentation-layer hierarchy as visual authority.

## 2. Global scene target

Chapter 01 should read as one damaged, functioning industrial exclusion complex.

Target frame language:
- large architectural masses create the scene;
- combat lanes stay physically clear;
- vertical machinery, walls, gates, service frames and tanks define the edges;
- floor is part of the architecture, not a repeated empty rectangle;
- meaningful machinery clusters are connected by pipes, cable routes, rails, platforms or service logic;
- visual density comes from authored large/medium forms, not random prop spam;
- cyan/blue identifies player/service/neutral technology;
- orange/red identifies danger, hostile energy and heavy industry;
- each sector has its own functional identity;
- hero facilities must be readable at the real portrait gameplay camera.

The concept board remains the mood/quality target. Current G-0 remains bipedal.

## 3. Chapter macro layout

The Chapter keeps these gameplay semantics:
- one safe Repair Hub/start area;
- five ordinary encounter spots, each tied to its existing progression role;
- all five ordinary spot objectives must remain completable;
- elite progression leads to Magnetar;
- boss progression leads to Custodian;
- save/progression/respawn/quest semantics remain unchanged unless a separate gameplay task explicitly changes them.

The physical arrangement may be rebuilt.

Recommended Chapter structure:

1. **Repair Hub** — safe service/repair base.
2. **Relay Yard** — Scout/light deployment sector.
3. **Capacitor Field** — Arc Drone/energy distribution sector.
4. **Cutting Floor** — Cutter/heavy fabrication sector.
5. **Shield Dump** — Warden/armor processing sector.
6. **Hauler Graveyard** — Carrier/logistics/wreck sector.
7. **Magnetar Complex** — elite induction/magnetic machinery zone.
8. **Custodian Core** — boss containment/reactor arena.

The five ordinary sectors may use loops/branches instead of a single corridor as long as onboarding, minimap clarity and progression remain readable.

## 4. Sector composition contracts

### 4.1 Repair Hub

Function: safe central service base and visual rest point.

Must read as a real maintenance installation, not a circle on a floor.

Use:
- circular/semicircular service platform;
- repair terminal and diagnostic consoles;
- cyan service energy;
- surrounding machinery wall or service bays;
- clear open player movement pocket;
- visible route exits into the industrial complex.

Avoid:
- excessive hostile orange/red;
- clutter in the player spawn footprint;
- isolated props without a service relationship.

### 4.2 Relay Yard — Scout

Function: light drone deployment / communications / relay assembly.

Use:
- compact deployment building or bay;
- visible open service mouth facing the patrol area;
- wall-mounted electronics / relay equipment;
- lighter machinery than later sectors;
- small gantries, crates or charging fixtures;
- limited red/orange hostile indicators.

The Scout spawn should visually originate from a believable dock/fabrication/deployment facility.

### 4.3 Capacitor Field — Arc Drone

Function: energy distribution / capacitor / induction zone.

Use:
- tall energy cylinders;
- generators and capacitor banks;
- blue/cyan contained energy contrasted with hostile red/orange control hardware;
- catwalks / platform changes / cable routes;
- strong vertical silhouettes;
- open attack lanes for ranged enemies.

This sector should be one of the strongest color-contrast areas in the Chapter.

### 4.4 Cutting Floor — Cutter

Function: heavy cutting/fabrication line.

Use:
- industrial presses / cutting machinery / mechanical frames;
- orange/red furnace or work-light accents;
- rails, service pits, heavy doors and machinery housings;
- visible work-cell logic around the enemy origin;
- fewer delicate props, more heavy masses.

The sector should feel dangerous before combat begins.

### 4.5 Shield Dump — Warden

Function: armor processing / shielding / scrap sorting.

Use:
- broad armored walls;
- heavy storage cells or shield housings;
- chunky processing machinery;
- protected pockets and narrow service edges while keeping the central fight readable;
- cooler steel with selected red/orange warning accents.

Warden must visually belong to the heavy defensive character of this sector.

### 4.6 Hauler Graveyard — Carrier

Function: logistics / transport / damaged storage yard.

Use:
- large cargo containers or transport-machine remnants;
- damaged carriers / loading frames / cranes / rails;
- asymmetrical wreckage and partial cover;
- broken service platforms;
- strong sense of previous industrial movement through the space.

This sector may be visually messier than the others, but required traversal must remain clean.

### 4.7 Magnetar Complex

Function: elite magnetic-induction installation.

Keep the successful overall elite gameplay space, but rebuild the presentation around a coherent elite facility.

Use:
- major induction reactor / magnetic apparatus;
- large circular or articulated machinery;
- amber/orange contained power;
- broad structural frames;
- controlled asymmetry;
- a clear approach that escalates visually before the elite becomes active.

Magnetar must feel integrated with the machinery and clearly above ordinary enemies in visual authority.

### 4.8 Custodian Core

Function: final containment/reactor complex and boss arena.

Keep the successful boss-space readability while replacing generic edge dressing with boss-grade architecture.

Use:
- dominant central reactor / containment landmark;
- monolithic wall/support masses around the arena;
- service platforms / gantries / conduits;
- strong orange/red core light with restrained cyan service remnants;
- clean boss telegraph lanes;
- cinematic entry/approach framing.

Custodian should feel like the governing machine of this facility, not a large enemy placed in a generic room.

## 5. Traversal and collision rule

The rebuild is allowed to change:
- exact spot coordinates;
- old lane geometry;
- old wall/floor positions;
- presentation-layer hierarchy;
- visual collision proxies;
- transition shapes;
- facility footprints;
- gate framing;
- minimap geometry derived from the rebuilt world.

It must preserve:
- reachable required objectives;
- encounter order/semantics;
- elite/boss progression;
- readable combat space;
- telegraphs;
- player movement safety;
- save/progression identity of content;
- practical Chapter length close to the accepted compact V45/V46 feel.

Do not preserve an old collision fingerprint merely to prove visual safety. Instead, rebuild explicit gameplay collision proxies and test traversal after the new visual layout is authored.

Player route clearance is a first-class acceptance criterion. Decorative art must not make normal fast traversal feel sticky or trap the player between visually minor objects.

## 6. Asset language

Primary current environment source families for this blueprint:
- **Quaternius Modular Sci-Fi MegaKit [Standard]** — structural backbone;
- **Molten Maps SciFi Asset Pack** — machinery and functional industrial landmarks.

Use custom project-owned Blender geometry where donor modules cannot satisfy the approved concept or a zone needs a unique hero structure.

Do not approximate a missing hero structure with repeated cubes/rings merely because they are easy to author procedurally.

## 7. Floor language

Do not cover the whole world with one repeated 4 m tile grid.

Use a mixture of:
- large plate fields;
- inset service strips;
- grates;
- ramps;
- raised platforms;
- damaged sections;
- trenches / recessed service bands;
- local sector-specific floor treatment.

The floor should support sector identity and route readability.

## 8. Facility / spawn language

A gameplay SpawnSpot is not the visual facility.

For the full rebuild, ordinary enemy origins should use facilities that are large enough to communicate function at gameplay scale:
- deployment bay;
- fabrication bay;
- charging/induction station;
- maintenance berth;
- logistics dock;
- armored processing cell.

The facility may be much larger than the old spawn marker footprint. Spawn anchors may be repositioned inside the rebuilt sector while preserving the same encounter identity and gameplay semantics.

## 9. Asset-kit mapping summary

Quaternius MegaKit should primarily provide:
- `Platform_*` families;
- `Door_*` / `Door_Frame_*`;
- `ShortWall_*`, `Wall*`, `Bottom*`, `Top*` structural families;
- `Column_*`, especially `Column_Pipes` and support columns;
- rails, ramps and stairs;
- cable, vent, access point, pipe-holder and light props.

Molten Maps should primarily provide:
- `Generator`, `Generator Pile Large/Small`;
- `Cryo Tube ON/OFF`;
- `Centrifuge`;
- `Command Console`, `Wall Command`;
- `Corridor Large/Small`;
- `Catwalk`;
- `Floor Metal*`, `Floor Mid Path*`, `Hazard Floor*`;
- `Wall Pipe`, `Wall Light*`, `Wall with Door*`, `Wall * 2nd Floor`;
- batteries/energy modules and selected large monitors where useful.

Do not scatter every family into every sector. Each sector should use a small coherent subset.

## 10. Implementation method for the next rebuild

The next full rebuild must **not** be implemented as another V48 presentation layer placed on top of V45/V46/V47 geometry.

Preferred method:
1. capture current gameplay semantics/configuration;
2. create a new Chapter01 visual/world-layout implementation derived from this blueprint;
3. place rebuilt gameplay collision proxies explicitly;
4. remap existing encounter identities and progression references into the rebuilt coordinates;
5. regenerate minimap/world presentation from the rebuilt layout;
6. validate traversal/combat/save/progression;
7. only then remove/supersede old presentation layers from the production scene.

Old V44–V47 implementation remains history/evidence, not the geometric starting point.

## 11. Acceptance for the rebuilt Chapter

A successful rebuild should make a side-by-side comparison with the owner concept feel like the same visual family without requiring an explanation.

Reject the rebuild if it reads as:
- the old map with new props;
- a repeated tile grid;
- scattered machinery on a flat floor;
- nine upgraded spawn markers;
- one procedural geometry vocabulary reused everywhere;
- dense clutter that harms traversal;
- concept colors applied to fundamentally unrelated geometry.

Accept only when the Chapter reads as a coherent industrial place with distinct functional sectors and clean gameplay space.