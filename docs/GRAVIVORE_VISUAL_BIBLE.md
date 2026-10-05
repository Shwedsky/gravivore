# GRAVIVORE Visual Bible — Visual Replacement Production V2

Status: **production source of truth for the next visual replacement milestone**  
Primary reference: **approved prototype board — `Концепт GRAVIVORE: Мехи и окружение.png`**  
Runtime baseline when authored: `40b682d3570d08c427999d42514501bb6c1c2dfd`

## 1. Authority and intent

This document translates the approved prototype board into rules that can be executed by a 3D artist, asset researcher, AI-assisted modeling workflow, and later Unity integration without making new art-direction decisions.

Where older GRAVIVORE art notes conflict with the approved prototype, **this package wins for Visual Replacement Production V2**. The prototype clearly depicts G-0 as a low, non-humanoid, multi-support techno-organic combat machine with a bright central cyan core, layered pale armor and dark exposed mechanics. It also establishes a dark industrial world with dense authored machinery, mixed warm/cool practical lighting, readable hostile silhouettes, and a very large spider-like Custodian M-0.

The target is not to copy pixels. The target is to preserve the same **shape grammar, hierarchy, density, material behavior, color logic, scale relationships and camera readability**.

## 2. Overall visual identity

GRAVIVORE is **compact techno-organic predation inside a heavy industrial machine world**.

The prototype combines three visual ideas:

1. **Predatory biomechanical silhouette** — machines read like purposeful organisms rather than vehicles or human suits.
2. **Engineered industrial mass** — floors, walls, gates, reactors and props feel manufactured, serviced, bolted and inhabited by infrastructure.
3. **Energy as anatomy** — emissive cores are not decoration. They reveal power, allegiance, danger and functional weak points.

The result must feel mechanically plausible at gameplay distance while still having exaggerated, readable forms for a portrait mobile camera.

### What makes GRAVIVORE distinct

- G-0 has a **bright cyan central gravity core** embedded in a low mechanical body.
- G-0 is not a conventional biped. Its mass spreads laterally through multiple articulated supports and weapon-like forward forms.
- Player armor is cool pale gray / desaturated steel over a darker internal structure; enemies use darker hostile shells with red/orange energy.
- Silhouettes are built from **overlapping wedges, curved plates, tapered limbs, claws, guards, housings and visible joints**, not rectangular boxes.
- Industrial scenery is dense enough to imply a functioning facility, but combat lanes stay visually legible.
- Warm orange infrastructure and cool cyan technology coexist, producing deliberate warm/cool contrast.
- Elite and boss escalation is expressed first through **silhouette, mass, limb reach and armor complexity**, only second through brighter emission.

## 3. Forbidden visual directions

These directions are rejected even when technically polished:

- cubic / blockout-like final models;
- toy robots or miniature-plastic proportions;
- generic humanoid robots;
- military exosuits or human armor silhouettes;
- boxy tanks with decorative legs;
- clean showroom sci-fi with no wear or service history;
- random greeble noise with no large/medium/small hierarchy;
- a world assembled from repeated rectangular wall slabs and crates;
- excessive neon on every edge;
- color coding that replaces silhouette differentiation;
- giant empty floors with props sprinkled around the perimeter;
- props that look like fake scenery rather than functioning equipment;
- close-up-only detail that disappears from the real gameplay camera;
- copied franchise language used as shorthand for art direction.

## 4. Shape language

### 4.1 Primary forms

Use a hierarchy of:

- **large masses:** low armored body shells, major limb groups, reactor bodies, gate frames, structural wall pylons;
- **medium forms:** armor plates, forearm blades, joint housings, vents, pipe bundles, maintenance panels, power trunks;
- **small forms:** fasteners, seams, narrow ribs, cable clamps, warning markings, localized surface damage.

A final asset must read correctly with small detail removed. If the silhouette and large/medium forms are weak, adding greebles is not a fix.

### 4.2 Curves versus angles

The prototype uses a mixed grammar:

- armor has faceted, pointed, tapered and shield-like planes;
- mechanical joints use cylinders, rings, pistons, bearings and hinge forms;
- energy cores are circular or lens-like and visually centered;
- hostile appendages often terminate in claws, blades or narrow contact points;
- infrastructure combines rectangular engineering volumes with cylindrical tanks, reactors, conduits and inset luminous channels.

Avoid pure orthogonal box construction. A rectangular industrial mass should receive a readable secondary shape: frame, beveled shell, pipe run, panel recess, vent, door split, lighting unit or structural brace.

## 5. Silhouette rules

### G-0

- Must read as a **low, fast, predatory machine** rather than a standing person.
- Tier 0 must already contain the central cyan core, four-point / multi-support stance, forward offensive direction and layered shell.
- Limbs must produce negative space between body and ground; do not merge into one solid block.
- Forward appendages / blades establish facing direction from the top-down camera.
- Core remains a visual anchor from all primary gameplay angles.

### Ordinary enemies

Each ordinary enemy must be recognizable in less than one second with materials mentally desaturated:

- Scout — small radial/spider silhouette, thin quick legs, exposed hostile core.
- Cutter — narrow predatory front, asymmetric blade emphasis, longer cutting reach.
- Warden — compact broad armored front, shield mass, slow heavy stance.
- Arc Drone — floating radial / tri-wing silhouette, no ground-contact legs, blue energy center.
- Carrier — tall/heavy hauler mass, thick supports, equipment-bearing body.

### Elite and boss

- Magnetar Guard must read as an escalation in body mass, armor layering, reach and stance before orange emission is considered.
- Custodian M-0 must dominate the frame through width, height, limb span and a large central red core. The boss cannot be a scaled-up ordinary enemy.

## 6. Hard-surface versus techno-organic balance

Target balance by visual impression:

- **~70% engineered hard-surface:** plates, housings, joints, frames, pistons, fasteners, vents, panel breaks.
- **~30% techno-organic flow:** predatory posture, shell overlap, limb taper, curved armor transitions, claw-like terminations, anatomical energy-core placement.

“Techno-organic” here does **not** mean flesh, slime or biological texture. It means the machine is organized like a creature: protected core, limbs, joints, directional armor, threat posture and functional anatomy.

## 7. Armor layering

Armor must appear assembled in layers, not extruded from one shell.

Use at least three readable depth levels on hero assets:

1. dark internal skeleton / mechanism;
2. mid-level structural housing;
3. outer armor plates with intentional overlap and gaps.

Gaps must expose believable mechanisms, shadow lines, cabling or actuators. Do not carve fake black lines into a monolithic mesh as a substitute.

Outer armor should protect core areas while leaving joints more mechanically exposed. The prototype consistently uses shell plates that wrap around a glowing core without hiding it.

## 8. Exposed mechanics

Visible mechanics are required where articulation or energy transfer needs explanation:

- hip and limb pivots;
- piston or linkage zones;
- weapon hinges;
- gate actuators;
- reactor connections;
- pipe junctions;
- cable terminations;
- maintenance access points.

Mechanical exposure should be clustered around functional areas, not spread uniformly as noise.

## 9. Emissive treatment

Emission is a hierarchy tool.

### Player

- cyan / electric blue is the G-0 identity color;
- brightest zone: central gravity core;
- secondary zones: narrow weapon channels, small joint or power indicators;
- never outline every armor edge.

### Enemies

- ordinary hostile units: red to warm orange, localized around eyes/cores/weapons;
- Arc Drone: blue energy is allowed because role is energy/ranged; hostile silhouette and non-player material balance must prevent confusion;
- elite: stronger amber/orange containment or weapon energy;
- boss: saturated red/orange core and attack energy, with the largest single hostile emissive focal point.

### Environment

- warm orange practical light communicates machinery, heat and power;
- cyan/blue communicates higher-tech nodes, energy infrastructure and service/repair systems;
- emissives must support navigation and composition, not turn the level into a neon corridor.

## 10. Damage and wear

The world and machines are used, maintained and fought in.

Required wear language:

- edge polish on exposed metal;
- dirt collected in recesses;
- heat discoloration near vents/reactors where appropriate;
- localized scratches on travel-facing armor;
- chipped paint / coating on contact zones;
- soot or residue near machinery;
- small decals and warning labels where scale permits.

Avoid uniform procedural grunge. Damage must tell where the object is handled, struck, heated or serviced.

G-0 should be worn but cared-for. Ordinary enemies can be rougher. Elite and boss may be better maintained in protected areas while showing heavy operational wear on extremities.

## 11. Material palette

Detailed numeric ranges live in `MATERIAL_COLOR_LIGHTING_BIBLE.md`. This bible establishes the hierarchy:

1. **graphite / near-black structure** — internal frames, joints, underside, rails;
2. **cool gray / pale armor** — player shell and selected machinery panels;
3. **mid-tone industrial metal** — environment housings, pipe systems, props;
4. **hazard paint** — restrained orange/yellow markings and protected service edges;
5. **cyan/blue emissive** — G-0, repair/service and selected technology;
6. **orange/red hostile energy** — enemies, elite and boss escalation.

No asset should depend on pure black or pure white albedo. Preserve material response under changing light.

## 12. Color hierarchy

At gameplay scale the screen should read in this order:

1. player cyan core and active attack line;
2. immediate hostile threat cores / attack telegraphs;
3. elite or boss focal accent;
4. navigation / service energy;
5. warm industrial practical lights;
6. neutral environment structure.

The environment may be rich, but it must not compete with combat roles through equal saturation and emission.

## 13. Lighting philosophy

The prototype target is dark but not crushed.

- Keep broad surfaces readable in mid-dark values rather than black silhouettes.
- Use warm practical sources to reveal industrial structures and frame combat areas.
- Use cool fills / energy sources to separate G-0 from warm hostile machinery.
- Preserve a clear value separation between walkable floor, vertical obstacles and characters.
- Boss arenas may intensify red/orange danger lighting, but the player cyan core must stay readable.
- Bloom is supporting polish, not a substitute for emissive geometry or shape.

A phone viewed below maximum brightness must still reveal floor boundaries, enemy bodies and the player silhouette.

## 14. Environment density target

The approved gameplay mockup shows a **busy industrial facility**, not an empty arena.

Density rules:

- every camera-sized gameplay cell should show at least one meaningful structural or machinery cluster at its periphery;
- large empty traversal surfaces must be broken by floor seams, service trenches, inset plates, drains, grates, embedded lights or damage — not by collision-blocking clutter;
- vertical rhythm should alternate low barriers, waist-height machinery, tall columns/reactors and wall structures;
- prop clusters must have functional logic: maintenance bay, power routing, storage, cooling, repair, containment, access control;
- leave readable combat corridors and dodge space around the player.

The correct target is **dense edges + readable center**, with selected interruptions inside the combat space.

## 15. Portrait gameplay readability

All final approvals happen from the real portrait gameplay camera, not the modeling viewport.

At camera distance:

- the player facing direction must be obvious;
- the central core must remain visible;
- ordinary enemy classes must remain distinct when viewed at small screen size;
- thin decorative parts that collapse below a few pixels cannot be primary identifiers;
- feet/leg contact and hover height must be clear;
- attack origin points must align visually with weapon geometry;
- the environment must frame characters rather than hide them.

Close-up beauty renders are useful for quality control but never override gameplay-camera failure.

## 16. Scale language

Use G-0 Tier 0 as the primary relative-scale anchor.

- Scout: clearly smaller and lighter.
- Cutter: similar footprint class but narrower and more attack-forward.
- Arc Drone: smaller body, elevated hover silhouette.
- Warden: roughly similar overall height/length but substantially broader frontal mass.
- Carrier: visibly larger, heavier and taller than ordinary units.
- Magnetar Guard: approximately 1.5–1.9× the perceived mass of G-0, with broader stance and heavier armor.
- Custodian M-0: approximately 3–4× the perceived mass of G-0 and several times its limb span; must feel arena-defining.

Exact meters may change during production fitting. **Perceived hierarchy must not.**

## 17. Boss / elite / ordinary hierarchy

Escalation order:

1. silhouette size and reach;
2. armor count and overlap;
3. visible mechanical complexity;
4. core size / energy containment;
5. emissive intensity;
6. unique material or hazard accents.

Do not create an elite by only increasing scale and orange glow. Do not create a boss by only increasing scale again.

## 18. Mobile-performance-aware art constraints

Visual complexity must be authored efficiently rather than removed.

- Put silhouette-changing geometry in mesh; put micro-surface information in normal/roughness textures.
- Share trim sheets / atlases within environment families where practical.
- Avoid one material per small part.
- Repeated modules should support LODs and renderer consolidation.
- Alpha-blended materials are exceptional, not default.
- Thin double-sided cards should not replace major mechanical geometry.
- Keep emissive surfaces integrated into existing material sets where possible.
- Geometry that is permanently hidden from the gameplay camera should be simplified.
- Boss detail budget should prioritize upper-facing / camera-visible surfaces.

Numeric budgets are defined in `MODEL_DELIVERY_STANDARD.md`.

## 19. MUST / SHOULD / MUST NOT

### MUST

- derive every hero asset from the approved prototype’s silhouette and hierarchy;
- preserve the low multi-support G-0 identity and central cyan core;
- use layered armor over darker exposed mechanisms;
- make five ordinary enemy silhouettes distinguishable without relying on color;
- make Magnetar clearly elite and Custodian clearly boss-sized;
- build dense, functional industrial scenery with real modular variation;
- keep combat readable from the real portrait gameplay camera;
- use emission as focal hierarchy, not decoration;
- show physically plausible joints, pivots and machine connections where visible;
- evaluate all hero assets in context before scaling production.

### SHOULD

- favor asymmetry in damage, cable routing and some weapon attachments while keeping the large silhouette readable;
- reuse coherent structural modules across a family;
- combine warm infrastructure light with cool player/service energy;
- exaggerate medium forms slightly so they survive mobile distance;
- support wear storytelling through contact, heat and maintenance zones;
- keep the outer combat lane cleaner than the environmental frame.

### MUST NOT

- ship primitives or procedural proxy geometry as final art;
- recolor current simple meshes and call it replacement;
- make G-0 humanoid, tank-like or toy-like;
- use rectangular slabs without secondary form language;
- fill empty rooms with random crates as a density solution;
- cover every edge with emissive strips;
- approve an asset because it imports correctly;
- approve assets only from close-up renders;
- accept C-grade or lower slice work for Chapter-wide replication.

## 20. Prototype ambiguities that remain open

The prototype is authoritative in style but not a fabrication drawing. These items are intentionally left as production questions rather than invented facts:

- exact limb count and rig segmentation for each G-0 tier;
- whether forward blade forms are dedicated weapons, manipulators or hybrid structures;
- exact physical scale in meters of G-0 and enemies;
- precise rear machinery layout not visible in the board;
- underside detail and hidden joint construction;
- exact surface text / decal language;
- whether all blue environment technology belongs to repair infrastructure or multiple systems;
- exact boss rear silhouette and complete leg articulation;
- exact wall/gate mechanical opening method.

When resolving these, preserve the prototype’s visible silhouette, material and role hierarchy rather than introducing a new style.

## 21. Approval statement

An asset belongs to GRAVIVORE only when it looks like it could stand in the approved prototype **without requiring a style explanation**. Technical correctness, polygon count, rigging quality and license compliance are necessary but not sufficient.