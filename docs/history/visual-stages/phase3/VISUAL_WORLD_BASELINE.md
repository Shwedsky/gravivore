# GRAVIVORE — Phase 3 Visual World Baseline

Status: **READY FOR CODEX INTEGRATION**

Owner split follows `POST_S20_PRODUCT_ROADMAP.md`:
- ChatGPT lane: art direction, asset research, license/provenance rules, visual acceptance, independent review.
- Codex lane: Unity import/integration, prefab/runtime binding, placement, validation, Android build.
- Do not edit the same implementation branch concurrently.

This phase is intentionally a **world baseline**, not final art. The target is a coherent modern industrial sci-fi Chapter 1 that is pleasant to traverse and credible enough for internal/external prototype review. Do not spend this phase polishing G-0 gait/VFX/SFX in isolation.

## 1. Locked constraints

Preserve:
- Unity 6000.3.0f1 and the existing URP/runtime presentation pipeline.
- Current S20 camera: portrait 9:16; offset (0, 14.8, -11.2), look-at height 0.9, vertical FOV 46.
- Gameplay roots, colliders, damage timing, movement authority, respawn, save/progression, encounter logic and gates.
- Presentation-only art: no visual object becomes gameplay authority.
- Existing player combat synchronization accepted in Phase 2.
- Initial asset spend: 0.
- Public-repository license policy: CC0 preferred; CC BY acceptable only with precise attribution and redistribution terms; no NC/SA/unclear-license assets; no recognizable franchise derivatives; no asset ripping; no login/manual-acquisition dependency for the autonomous path.

## 2. Visual target

### Target words
**cold / industrial / worn / mechanical / heavy / utilitarian / hostile / readable**

### Explicitly avoid
- cute or toy-like robot proportions;
- glossy plastic primary surfaces;
- bright rainbow palettes;
- generic stock humanoid soldiers;
- oversized cartoon gears;
- clean empty test-floor presentation;
- noisy micro-detail that disappears from the current mobile camera;
- neon everywhere;
- VFX-heavy clutter that competes with combat telegraphs.

### Shared Chapter palette
Use a common material family across the world:
- graphite and near-black structure;
- cool desaturated steel;
- dirty concrete / worn industrial floor;
- restrained rust and heat damage;
- cyan/blue only for player/neutral power and diagnostics;
- hostile red/orange for enemy cores, danger and active machinery;
- local zone accent colors are secondary navigation cues, not full-surface paint.

The world should read first by **shape, height, density and landmark silhouette**, then by accent color.

## 3. Chapter 1 world layout to preserve

Authoritative world definition remains `S08_Chapter01World.asset`.

World:
- ground center: (0, 0, 30)
- ground size: 72 x 140
- player/repair basin: approximately (0, 0, -30)

Existing named zones:
- Relay Yard center (-26, 0, 20), landmark (-30, 0, 20)
- Cutting Floor center (0, 0, 40), landmark (0, 0, 45)
- Shield Dump center (26, 0, 20), landmark (30, 0, 20)
- Capacitor Field center (-20, 0, -12), landmark (-24, 0, -15)
- Hauler Graveyard center (20, 0, -12), landmark (24, 0, -15)
- Elite gate (0, 0, 60)
- Magnetar Guard spawn (0, 0, 69)
- Boss gate (0, 0, 80)
- Custodian M-0 arena center (0, 0, 94), current radius 5

Do not move these gameplay anchors in the art baseline.

## 4. Macro composition

The chapter should visually progress north from a comparatively controlled repair basin into increasingly damaged and dangerous industrial territory.

Use three dressing layers:

1. **Structural layer** — floor modules, perimeter wall fragments, gantries, pipes, power trunks, large industrial frames. This removes the empty-plane feel.
2. **Zone-signature layer** — 2–5 hero props per named zone with a distinctive silhouette visible from the current camera.
3. **Edge-clutter layer** — cables, crates, scrap, broken panels, small machinery and debris concentrated near borders, not in combat centers.

Keep the primary travel corridor and encounter footprints visually open. Decorative geometry must not create hidden collision traps or obscure telegraphs.

## 5. Zone baseline

### Repair Basin / spawn — semi-hangar
Purpose: immediately explain accelerated recovery and establish the game's industrial identity.

Required silhouette:
- recessed or raised repair platform;
- 2–4 articulated service/manipulator arms;
- overhead or side service frame;
- cable/power trunks leading into the platform;
- cyan diagnostic/repair light, restrained bloom;
- heavy bay framing around the perimeter while the player exit remains obvious.

The arms may animate deterministically for presentation, but healing remains authoritative elsewhere.

### Capacitor Field
Purpose: electrical-storage / power-processing space.

Required:
- 3–5 vertical capacitor/coil structures of varied height;
- thick cables or bus bars;
- one broken/discharged unit;
- occasional restrained arc/glow;
- amber/yellow local warning accent, hostile red only when tied to combat.

Silhouette: vertical forest of industrial power hardware.

### Hauler Graveyard
Purpose: scrapyard and heavy logistics failure.

Required:
- 2–3 large broken hauler/chassis masses around the edges;
- collapsed cargo frame/crane element;
- scrap heaps and wheels/tracks/structural fragments;
- central combat lane kept clear.

Silhouette: low bulky wreckage rather than upright towers.

### Relay Yard
Purpose: communications / control distribution.

Required:
- relay mast or antenna stack;
- cabinet/rack cluster;
- cable trench or conduit line;
- one secondary dish/panel array;
- cool blue local indicator lights.

Silhouette: thin vertical mast + rectangular machinery cluster.

### Shield Dump
Purpose: discarded field-generation equipment.

Required:
- stacked/broken shield emitter rings or plates;
- a few heavy generator blocks;
- one half-active emitter or faint field remnant;
- green accent allowed only as a local systems cue.

Silhouette: circular/arched forms distinct from the rest of the chapter.

### Cutting Floor
Purpose: active salvage processing.

Required:
- large cutting/press frame;
- rail/conveyor geometry;
- suspended or side-mounted cutter machinery;
- scorched floor area / heat damage;
- red-orange hazard lighting and markings.

Silhouette: broad industrial machine over a flatter process floor.

### Elite approach / Magnetar Guard yard
Purpose: clear escalation before the elite.

Required:
- stronger perimeter frames and power pylons;
- compressed approach that opens into the fight space;
- magnetic/coil motifs reused from Capacitor Field but heavier;
- central arena kept clear for shockwave readability.

The elite must be visible as the dominant moving silhouette, not lost against props.

### Boss approach / Custodian M-0 arena
Purpose: visual destination and Chapter climax.

Required:
- visibly heavier architecture than previous zones;
- large broken crane/gantry or maintenance superstructure;
- arena edge ring / segmented floor treatment;
- damaged industrial wall silhouettes beyond the playable ring;
- strong but restrained red/orange hazard language;
- no tall prop in the central telegraph area.

The boss arena should feel purpose-built for a large industrial maintenance machine that became hostile.

## 6. Actor baseline

Do not use one stock robot family scaled up/down for every role. Preserve shared industrial construction language while changing topology/silhouette.

### Scout Drone
- smallest and fastest;
- hovering or very low body;
- triangular/three-fin or compact sensor-forward silhouette;
- one bright hostile sensor/core;
- minimal armor.

### Cutter Unit
- retain the accepted low asymmetric cutter identity;
- crawler/runner posture;
- unequal cutting arms/blades;
- red core, no humanoid head.

### Arc Drone
- compact floating or tripod electrical unit;
- twin prongs/coils visibly communicate its attack identity;
- round/central power body is acceptable if surrounded by mechanical forks so it does not read as a cute ball robot.

### Warden
- slow defensive silhouette;
- quadruped or broad low sentry stance;
- large forward armor/shield mass;
- smaller hostile core recessed behind armor.

### Carrier
- heavy logistics machine repurposed as an enemy;
- broad chassis, cargo/power pack, asymmetric rear mass;
- visually heavier than Warden without becoming a boss.

### Magnetar Guard — elite
- approximately 1.4–1.7x ordinary enemy visual height/footprint, without changing gameplay collision;
- tall/broad industrial guardian;
- paired magnetic generator shoulders or side coils;
- visibly reinforced legs/supports;
- central hostile core with restrained red/orange emission;
- strong negative-space silhouette at mobile camera distance.

### Custodian M-0 — boss
- preserve the approved **large industrial-mech** direction;
- approximately 2–3x the perceived mass of ordinary enemies;
- asymmetric industrial body, not a generic humanoid mech;
- crane/manipulator/excavator DNA;
- large exposed reactor/core;
- dedicated arm/attachment silhouettes that make cone/line/circle attacks visually plausible;
- broad upper mass + stable lower supports;
- damageable body must remain readable while boss HP UI is active.

For Phase 3, elite/boss need credible prototype models, not final bespoke rigs.

## 7. Asset strategy

Primary environment geometry:
- Quaternius **Modular Sci-Fi MegaKit — Standard/free subset**, CC0, as the broad structural kit.
- Existing/expanded Kenney **Factory Kit** and **Modular Space Kit**, CC0, only as secondary industrial dressing.
- Do not let Kenney geometry dominate the scene; its clean/simple language can read too toy-like at current quality target.

Surface/material layer:
- retain existing Poly Haven **Blue Metal Plate** CC0 material family;
- add a very small shared set of 1K CC0 PBR surfaces only when they materially improve the scene: worn metal plate, dirty concrete, debris/rust;
- import with mipmaps, Android ASTC 6x6, no CPU readback, matching the existing ART pipeline.

Actors:
- preferred autonomous route is **project-authored hard-surface kitbash** using the existing ArtSpike proxy-mesh approach plus neutral CC0 mechanical parts where useful;
- whole Quaternius robots/mechs may be auditioned, but they are not the default because rounded/cute or humanoid stock silhouettes conflict with the locked direction;
- no third-party actor is accepted only because it is rigged.

See `VISUAL_WORLD_ASSET_RESEARCH.md`.

## 8. Materials and lighting

Keep material count deliberately small.

Recommended shared world roles:
- World_DarkStructure
- World_CoolMetal
- World_WornMetal
- World_Concrete
- World_Rust/Damage
- Neutral_CyanEnergy
- Hostile_RedEnergy
- Hazard_Amber

Prefer material tint/mask variation over many unique materials.

Lighting baseline:
- retain one dominant directional light unless profiling proves an additional local light is safe;
- baked/emissive-looking fixtures should usually use emissive material, not realtime point lights;
- restrained bloom;
- optional subtle fog/atmospheric depth only if it does not reduce enemy/telegraph contrast;
- shadows must be profiled on Android before expanding their use.

## 9. Mobile readability rules

At the current 1080x1920 reference:
- ordinary actor identity must read from silhouette and one large accent, not small decals;
- zone hero props should remain recognizable in normal gameplay camera, not only close-ups;
- floor texture contrast must stay below actor/telegraph contrast;
- no thin decorative geometry crossing the central player path at camera height;
- do not use tiny text labels as primary world communication.

## 10. Soft performance budget for Phase 3

These are integration targets, not new gameplay requirements.

Existing measured reference:
- G-0 Tier2 after Phase 2: 35 renderers, 58 material slots, 7,760 triangles.
- Art V3 Cutter reference: 13 renderers, 25 material slots, 3,352 triangles.
- Previous runtime sample with Tier2 + 4 Cutters: 23,012 triangles before environment/HUD/VFX.

Phase 3 target ranges:
- ordinary enemy presentation: preferably 3k–12k triangles and <= 20 renderers;
- elite: preferably <= 25k triangles and <= 30 renderers;
- boss: preferably <= 50k triangles and <= 40 renderers;
- repeated environment props: favor 1 material role and shared meshes;
- hero zone prop clusters: split for culling only when it helps; avoid dozens of tiny renderer objects;
- textures: 1K max by default; use shared atlases/PBR maps where practical.

These are deliberately conservative prototype targets. Real-device frame time is the final gate; do not chase a triangle number while creating excessive draw/material cost.

## 11. Integration architecture

Codex should add a presentation-only Chapter world layer rather than hand-editing gameplay definitions.

Preferred shape:
- dedicated `Chapter01WorldVisualCatalog` or equivalent presentation data;
- presentation prefabs for environment chunks / zone landmarks / repair hub;
- explicit spawn from the existing world definition positions;
- enemy/elite/boss presentation prefabs bound through the existing visual factory or a compatible presentation-only extension;
- no new colliders on imported art unless separately reviewed; decorative colliders disabled/removed by default;
- no third-party scripts;
- no Addressables/package/framework expansion for this baseline;
- reversible references: removing the Phase 3 presentation catalog should restore the prior prototype visuals.

## 12. Capture set

Before Android review, produce fixed screenshots from the real Chapter scene:
1. spawn / repair basin;
2. Capacitor Field;
3. Hauler Graveyard;
4. Relay Yard;
5. Shield Dump;
6. Cutting Floor;
7. elite approach;
8. Magnetar Guard encounter;
9. boss approach;
10. Custodian M-0 arena;
11. one ordinary-combat shot containing at least two enemy families;
12. one wide traversal shot showing multiple zones without looking like an empty plane.

Use the real game camera or a capture camera with the exact same FOV/height relationship. Do not use external paint-over as proof.

## 13. Phase 3 acceptance

Phase 3 is accepted when the Android build, without explanation:
- looks like one coherent industrial sci-fi game world rather than a mechanics test scene;
- has no large stretches that read as an empty default floor;
- makes all five ordinary zones visually distinguishable;
- makes repair hub, elite approach and boss destination immediately recognizable;
- gives every ordinary enemy family a distinct silhouette;
- gives elite and boss credible, non-cute, non-stock dominant silhouettes;
- keeps current combat telegraphs/HP feedback readable;
- introduces no gameplay/save/respawn/progression regression;
- avoids obvious frame-time regression on the target Android device;
- remains replaceable prototype art, with provenance documented for every third-party asset.

Only after this baseline is accepted should the project re-evaluate G-0 proportions in context and start the roadmap's later final feel/audio/VFX pass.
