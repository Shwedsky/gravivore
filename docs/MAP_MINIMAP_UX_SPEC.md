# GRAVIVORE — Map / Minimap UX Specification

Status: **Phase 5 pre-design / documentation only**  
Branch: `design/map-minimap-ux-spec`  
Baseline: `main@fdd57366eefd5958a083e32e256d262733d98cda`  
Target: Chapter 1, portrait Android, top-down/isometric, one-thumb play.

This document defines the future presentation and read-model contract for the Chapter 1 map/minimap. It does **not** implement Unity UI, mutate gameplay state, modify save data, or modify the repeatable-loop rules now present on `main` from former PR #40.

## 0. Goals and non-goals

### Goals

- Add navigation value without turning the HUD into a second game screen.
- Make player, repair hub, farming spots, elite, boss, gates and chapter objectives immediately distinguishable.
- Preserve mobile readability on narrow portrait screens.
- Keep encounter availability, progression lock and reward/cap state visually separable.
- Stay compatible with the existing `IWorldMarkerSource` / `WorldMarkerReadModel` direction where practical.
- Define a schematic, event-driven implementation model that is cheap on Android.
- Leave exact #40 cooldown/cap semantics configurable.

### Non-goals

- No Unity UI implementation.
- No scene, prefab, runtime MonoBehaviour, gameplay, save, validator or test changes.
- No pathfinding or route planner.
- No fog-of-war system.
- No new progression or reward rules.
- No dependency on Phase 3C visual prefab positions as gameplay authority.
- No render-texture minimap camera.

---

## 1. Map vs minimap decision

Options evaluated:

- **A — always-visible minimap only:** low interaction cost, but cannot comfortably show cooldown/reward details.
- **B — expandable minimap:** viable, but ambiguous if the same widget must morph between two very different information densities.
- **C — separate full map only:** preserves HUD space but makes routine orientation too expensive.
- **D — minimap + tap-to-expand:** best balance for portrait play.
- **E — no minimap, only POI indicators:** insufficient for a relatively large Chapter 1 and future denser stronger spots.

### Recommended MVP: D — compact minimap + tap-to-expand full map

The compact minimap answers only three questions:

1. Where am I?
2. Which useful/dangerous POIs are nearby?
3. Which direction should I move?

The expanded map answers:

1. What is the state of a POI?
2. Is it locked, available, cooling down or completed?
3. Is a repeat reward available?
4. How long until a timed encounter becomes available?

The expanded map is still a **map panel**, not an open-world map product. No quest journal, route planning, fast travel or world-region browser is part of MVP.

---

## 2. Screen placement

### Preferred compact minimap position

**Upper-right**, inside the portrait-safe gameplay area.

Rationale:

- the lower half is reserved for the floating joystick and thumb movement;
- top-left is the natural home for player HP/status hierarchy;
- boss HP can remain top-center/full-width;
- an upper-right map remains glanceable without crossing the active movement thumb.

### Relative size

Recommended compact footprint:

- width: approximately **24–28% of safe-area width**;
- height: approximately equal to width;
- never so large that it becomes a second combat viewport;
- icon size is prioritized over map-detail density.

This is normalized guidance, not a pixel contract.

### Safe margins

- respect platform safe area before local map padding;
- use approximately **3–4% of safe-area width** from the right edge;
- use approximately **2–3% of safe-area height** from the top-most occupied HUD row;
- do not anchor directly to raw screen corners.

### Interaction with existing HUD

**Player HP:** map must not overlap the player HP block. HP remains higher priority.

**Boss HP:** when boss HP is active, it owns the primary top-center band. The minimap either remains below that band or shifts down by a single layout slot; it must not scale smaller every frame or jump based on HP animation.

**Joystick:** no overlap with lower gameplay region and no map drag gesture in compact mode.

**Menu/pause:** the menu/pause control remains a discrete tap target. If it occupies upper-right, the map sits below it rather than becoming the menu button.

**DEV UI:** development overlays must not dictate release placement. DEV-only controls may relocate around the map, but the map should be validated with DEV UI hidden and with a representative debug overlay enabled.

### Boss encounter behavior

- compact minimap stays visible;
- non-critical labels remain hidden as usual;
- boss marker remains present and visually dominant;
- if the boss arena is local, nearby ordinary-spot clutter may be de-emphasized;
- map does not flash continuously during combat;
- boss HP remains visually above the map in hierarchy.

### Menus and modals

- pause/full-screen modal: compact map is hidden or visually disabled with the rest of gameplay HUD;
- expanded map cannot remain interactive under another modal;
- lightweight non-modal toast/pickup feedback must not hide the map.

---

## 3. Orientation

Options:

- **A — north-up / fixed world orientation**
- **B — player-up / rotating**
- **C — fixed world orientation with player heading arrow**

### Recommended MVP: C

Use a fixed world orientation, effectively north-up for Chapter 1, with a player arrow/chevron showing movement/facing direction.

Reasons:

- current gameplay camera yaw is fixed;
- a rotating minimap adds motion discomfort without meaningful navigation gain;
- fixed orientation helps the player learn where repair hub, elite and boss lie in the chapter;
- future POIs remain spatially memorable;
- the only rotating element is the player arrow, not the entire map.

If Chapter 1 art later chooses a strong diagonal visual axis, "north" means the authored fixed map orientation, not geographic north.

---

## 4. Player marker

### Symbol

Use a **filled triangular/chevron pointer** inside a restrained circular or hexagonal backing. It must be readable by silhouette before color.

### Orientation

- arrow points along current movement/facing heading;
- while stationary, keep the last meaningful heading;
- exact animation-facing jitter is not important.

### Priority

The player marker is the highest local-priority symbol:

- visually above ordinary POIs;
- always rendered at a stable apparent size;
- never hidden behind an encounter icon.

### Edge behavior

Compact minimap is player-centered/local-radius for MVP, so the player normally remains near center. If chapter-bound clamping shifts the local crop near an edge, the player may move away from exact center but must remain fully visible with map padding.

Expanded map shows the true player position in chapter coordinates.

### Exact facing

Exact frame-to-frame facing is not required. Update heading only when a meaningful directional change occurs, preventing visual vibration.

---

## 5. POI taxonomy

All markers use one industrial tactical icon language: angular silhouettes, brackets, rings and machine-like glyphs. Color is secondary to shape.

### Repair hub

- **Family:** hexagonal service node / wrench-like mechanical notch.
- **Hierarchy:** major safe-system POI.
- **Size:** medium-large.
- **Emphasis:** cyan/neutral system color; stable, no pulse by default.
- **Label:** hidden on compact map; "Repair Hub" on expanded map or selection.
- **Availability:** normally available; disabled state may use broken-outline treatment if future logic requires it.

### Ordinary farming spot

- **Family:** compact circular/hex node with small internal dot cluster.
- **Hierarchy:** standard resource/combat POI.
- **Size:** small-medium.
- **Emphasis:** restrained neutral/low-danger treatment.
- **Label:** no compact label; short authored spot name on expanded map.
- **Availability:** state shown through fill/ring, not text alone.

### Stronger ordinary spot

- **Family:** same ordinary-spot base family plus a **distinct tier notch/double bracket**.
- **Hierarchy:** above ordinary spot, below elite.
- **Size:** same core size or at most 10–15% larger.
- **Emphasis:** stronger outline + danger notch; no new rarity rainbow.
- **Label:** expanded map only.
- **Availability:** same availability grammar as ordinary spot.

### Elite

- **Family:** angular diamond/target plate.
- **Hierarchy:** major hostile POI.
- **Size:** large.
- **Emphasis:** danger outline, thicker than ordinary nodes.
- **Label:** expanded map or selection; compact label omitted.
- **Availability:** lock / alive / cooldown / repeat-ready / reward-state overlays.

### Boss

- **Family:** unique heavy hex/arena-core symbol with crown-like upper notch or triple-segment ring.
- **Hierarchy:** highest hostile POI.
- **Size:** approximately 125–140% of elite marker.
- **Emphasis:** dominant outline and contrast, not permanent animation.
- **Label:** expanded map or selection.
- **Availability:** lock / encounter-ready / alive / defeated / cooldown / repeat-ready / reward-state overlays.

### Gate

- **Family:** barrier / split chevron / two vertical blocks.
- **Hierarchy:** structural.
- **Size:** small-medium.
- **Emphasis:** neutral when open, high-contrast lock/bar when closed.
- **Label:** only when selected or directly relevant to the objective.
- **Availability:** closed/open.

### Optional chapter objective

- **Family:** bracket/crosshair overlay around the underlying POI icon.
- **Hierarchy:** temporary attention layer, not its own map category.
- **Size:** overlay extends 10–20% around base icon.
- **Emphasis:** subtle pulsing bracket may be used, with reduced-motion fallback.
- **Label:** objective summary belongs in selected details, not permanent compact text.

### Future exit / next chapter hook

- **Family:** forward arrow through a gate/port.
- **Hierarchy:** major progression destination.
- **Size:** medium-large.
- **Emphasis:** neutral/cyan if unlocked, locked outline if unavailable.
- **Label:** expanded map.

---

## 6. Ordinary spot states

Ordinary farming uses adaptive respawn. The map should communicate usefulness, not expose the full balancing algorithm.

### Available / alive

- filled or solid-core ordinary icon;
- stable outline;
- no timer text.

### Combat active

- same icon plus a restrained combat ring or one-cycle emphasis on state entry;
- no continuous flashing;
- state should reflect the spot being actively contested, not every individual enemy attack animation.

### Pending respawn

- hollow core;
- partial progress ring may represent approximate recovery;
- no numeric countdown on compact map.

### Adaptive pressure active

Do **not** expose the exact pressure step on the compact map.

On expanded map, a selected ordinary spot may show a qualitative band such as:

- Respawn pressure: Elevated
- Respawn pressure: Maximum

Only if the underlying gameplay/read model intentionally exposes that concept.

### Max pressure

Same rule: qualitative selected detail only. The map should not turn pressure into a second rarity/economy system.

### Recovered

Return to normal available icon with no celebratory animation.

### Temporarily inactive

Use hollow icon plus a diagonal system slash or muted bracket. The distinction must be visible without color.

### Exact timer policy

- compact minimap: never;
- expanded map overview: progress ring only for ordinary spot;
- selected POI details: exact short countdown may be shown when timing data is available.

Because ordinary respawns are short, a permanent ticking label would create more noise than value.

---

## 7. Stronger ordinary spots

The map contract must support stronger ordinary spots without assuming their final #40 balance.

### Required distinction

Use the same family as ordinary spots to communicate "still a farming spot", then add exactly one restrained tier/danger discriminator:

- double outer bracket, **or**
- one extra angular notch, **or**
- a small "II" tier mark in expanded map.

Recommended: **double outer bracket + stronger outline**.

Do not use five rarity colors. Stronger spot status continues to use the same fill/ring availability grammar as ordinary spots.

Any exact spawn rules, reward multipliers, adaptive-pressure limits or unlock conditions are owned by gameplay/#40, not this UX spec.

---

## 8. Elite states

Elite presentation must treat three dimensions separately:

1. progression access;
2. encounter availability;
3. reward state.

Do not encode all three into one enum if the implementation can avoid it.

### Progression locked

- diamond silhouette remains visible because Chapter 1 map is always-known;
- lock/bar overlay;
- hostile color de-emphasized;
- selected details may show "Locked" and the known progression requirement if an objective service already exposes a player-facing description.

### Available first kill

- solid elite icon;
- strong outline;
- optional one-time discovery/availability emphasis when state changes;
- no repeating flash.

### Alive

- same available icon;
- optional combat ring when encounter is engaged.

### Cooldown

- hollow elite icon;
- outer cooldown progress ring;
- no numeric compact timer.

### Repeat available

- returns to solid encounter-ready icon;
- optional small repeat-loop sub-glyph in expanded map.

### Reward-cap reached / fallback reward

The encounter icon must still answer "Can I fight it?" independently from "Is premium/special reward available?"

Recommended small reward sub-badge on expanded map / details:

- full reward available: filled reward pip;
- capped/fallback: outlined or barred reward pip;
- no special reward: reward pip absent or struck-through.

Do not permanently place this sub-badge on the compact minimap unless device review proves it remains legible.

### Timer

- compact: radial/outline state only;
- expanded map: rounded time such as "12m";
- selected details: exact `mm:ss` when useful.

The current #40 draft may choose specific cooldown values and cap semantics. Those values are not part of this document.

---

## 9. Boss states

Boss follows the same three-dimensional contract as elite, with stronger hierarchy.

### Progression locked

- known boss marker remains visible;
- heavy outline is muted;
- clear lock/bar overlay.

### First encounter available

- full boss silhouette and danger outline;
- one-time state-entry emphasis allowed;
- no permanent flashing.

### Alive / engaged

- full marker;
- optional combat ring;
- boss HP owns top combat hierarchy.

### Defeated / Chapter completed

First-clear completion is distinct from repeat cooldown:

- show completion notch/check in expanded map;
- if repeat play is not currently available, icon may also show cooldown ring;
- do not replace boss marker with a generic checkmark.

### Cooldown

- hollow/heavy outline;
- cooldown ring;
- numeric timer only expanded/selected.

### Repeat available

- full marker restored;
- repeat availability can use small loop sub-glyph on expanded map.

### Reward-window/cap state

Same independent sub-badge rules as elite. If #40 chooses "available but no special reward", the boss remains visually fightable.

---

## 10. Timers

### Ordinary respawn

- compact minimap: no text, optional ring;
- expanded overview: no exact text by default;
- selected details: exact short countdown;
- update visible numeric text at a coarse cadence (for example once per second), not every frame.

### Elite cooldown

- compact: cooldown ring only;
- expanded overview: rounded minute value;
- selected details: exact `mm:ss`.

### Boss cooldown

Same as elite, with larger marker.

### Color transitions

Do not communicate timer progress solely by hue. Ring fill/arc length carries the primary meaning. A near-ready state may increase contrast, but shape remains sufficient.

### Hidden-until-near availability

Not recommended for MVP. Hiding long cooldown state causes uncertainty. Show the cooldown ring whenever the encounter is cooling down; only numeric detail is gated by expanded/selected context.

---

## 11. Map scale and cropping

Chapter 1 is approximately **72 x 140 world units**.

Options:

- full Chapter always visible;
- local-radius minimap;
- hybrid crop;
- dynamic zoom.

### Recommended MVP: hybrid

**Compact minimap:** local-radius crop centered on the player and clamped to chapter bounds.

**Expanded map:** full Chapter 1 bounds visible at a stable scale.

Why:

- the gameplay camera already shows a meaningful local area;
- a full-chapter thumbnail in a small square would compress nearby POIs too much;
- a local map adds navigation value;
- expanded full map preserves global route memory;
- dynamic zoom adds instability and test surface without solving a current problem.

### Suggested local coverage

Treat radius as an authored/configurable presentation value, initially enough to show roughly one nearby farming region plus a meaningful directional relationship to adjacent POIs. Do not hard-code this spec to an exact world-unit radius before device review.

### Boundary behavior

When the player approaches map edges, clamp the crop to chapter bounds rather than exposing blank space. This means the player can move away from the exact minimap center near world edges.

---

## 12. Fog of war / discovery

### Recommended MVP: no fog of war, no POI discovery system

Chapter 1 map is **always-known** at the structural level.

Reasons:

- exploration is not a current progression pillar;
- adding discovery creates save and objective semantics not otherwise needed;
- locked elite/boss visibility helps communicate long-term direction;
- the map should support farming/navigation, not hide content.

Hidden/secret POIs may be added later as a separate feature, but must not be inferred from this MVP.

---

## 13. Interaction

### Compact minimap

- tap anywhere on compact minimap -> open expanded map;
- do not require precise POI tapping on the compact widget;
- no drag, pinch or rotation in compact mode.

### Expanded map

- tap POI -> select marker and open a small details sheet/panel;
- tap background -> clear selection;
- optional pan only if full-map fit proves impossible on small devices; target design is full Chapter fit without panning.

### Details panel

May show:

- POI name;
- state;
- availability;
- cooldown;
- reward-state summary;
- objective relevance.

Do not duplicate full quest journal text.

### Pin / waypoint

**Defer persistent custom waypoints from MVP.**

Selection may temporarily highlight a POI while the expanded map is open. A future "Track" action can add one lightweight bearing/edge indicator, but no route line is required for MVP.

### Route line

No route line and no NavMesh/pathfinding planner.

---

## 14. Visual language

Target: **industrial tactical system / machine diagnostic view**.

Use:

- dark translucent map surface;
- thin machine-grid / sector lines;
- restrained cyan/blue for player and neutral systems;
- neutral gray/steel for ordinary content;
- red/orange danger emphasis for hostile major encounters;
- angular icon silhouettes;
- limited glow only for current focus/state transition.

Avoid:

- parchment/fantasy-map styling;
- arcade radar sweep;
- rainbow rarity palette;
- constant bloom/glow;
- cartoon skulls and oversized emoji-like icons.

The visual layer should remain legible over every Phase 3C environment, so the map has its own controlled contrast background rather than relying on world color.

---

## 15. Accessibility and readability

### Must not depend on color alone

These distinctions require shape/border/state redundancy:

- player vs hostile;
- ordinary vs stronger spot;
- ordinary vs elite vs boss;
- locked vs available;
- available vs cooldown;
- full reward vs capped/fallback;
- open vs closed gate.

### Small portrait screens

- compact map has no permanent text labels;
- avoid sub-glyphs smaller than reliable glance size;
- expanded map owns explanatory text;
- tap target is larger than the visible icon.

### Text

Use the project's mobile UI scale rather than fixed pixel sizes. Selected details should remain readable at arm's length; compact map should work even if all text is removed.

### Motion

- no rotating map;
- no perpetual marker pulsing;
- state-entry pulse is optional and must have reduced-motion fallback;
- cooldown progress should be a static arc update, not spinning animation.

### Low contrast world

The map is drawn on a controlled dark/translucent panel with icon outlines, so environment lighting does not determine marker readability.

---

## 16. Performance architecture

Options:

- **A — schematic UI map using world coordinates**
- **B — second camera + render texture**

### Recommended MVP: A — schematic map

Reasons:

- avoids another camera render;
- avoids environment/lighting/VFX cost duplication;
- gives deterministic icon readability;
- naturally consumes world marker read-model data;
- is easier to test;
- allows map state to exist without the visual art hierarchy.

### Implementation guardrails

Future implementation should:

- use a single map presenter/controller, not one `Update` per marker;
- pool/reuse marker views;
- apply marker changes when the read model changes;
- precompute chapter world bounds -> normalized map transform;
- transform only changed marker positions unless the player/local crop moved;
- update player marker at the presentation cadence needed for movement;
- update numeric timers at coarse cadence, not per-frame string allocation;
- keep static labels cached;
- avoid rebuilding layout every frame;
- avoid LINQ/temporary allocations in steady-state map refresh;
- keep expanded-map open/close allocation-light.

A render-texture minimap camera is specifically out of scope unless later profiling/design proves schematic mapping insufficient.

---

## 17. Read-model contract

The map should avoid mirroring gameplay internals. Prefer a small UI-facing marker snapshot/adaptor over many direct references.

### REQUIRED MVP

Conceptual fields:

- `id` — stable marker identity for view reuse.
- `kind` — Player, RepairHub, OrdinarySpot, StrongOrdinarySpot, Elite, Boss, Gate, Exit/Hook.
- `worldPosition` — authoritative gameplay/world position used for map transform.
- `visibility` — whether the marker should be rendered by this presentation.
- `availabilityState` — compact presentation state such as Available, Active, Cooldown, Inactive, Defeated.
- `progressionLocked` — independent lock dimension when relevant.
- `nextAvailabilityUtc` (optional per marker) — absolute time boundary for timed state, if the underlying system exposes one.
- `rewardState` (optional per marker) — only for repeatable elite/boss when #40 finalizes the contract.

The exact enum names may differ. The important contract is **separation of lock, encounter availability and reward state**.

### OPTIONAL LATER

- qualitative respawn pressure band;
- danger tier beyond the distinction already implied by `kind`;
- discovery state;
- tracked/waypoint state;
- user-facing localized label key if not already resolvable from content IDs;
- objective priority when multiple objectives exist.

### DERIVED IN UI

Do not persist or duplicate these if they can be derived:

- marker screen position from `worldPosition` + map bounds;
- icon sprite from `kind`;
- icon size/hierarchy from `kind`;
- hostile/system color role from `kind` + state;
- `isAlive` if already represented by availability state;
- `isAvailable` if already represented by availability state;
- `timeRemaining` from `nextAvailabilityUtc` + injected time provider;
- cooldown ring fraction from timing window metadata;
- compact/expanded label visibility;
- player-edge/crop position.

### Fields intentionally not required for MVP

- raw `pressureStep`;
- both `respawnAt` and `timeRemaining` at the same time;
- scene object instance IDs;
- presentation prefab references;
- visual-anchor transforms from Phase 3C.

### Compatibility note

Where the current `IWorldMarkerSource` / `WorldMarkerReadModel` already exposes equivalent concepts, adapt rather than duplicate. If an extension is required after #40, prefer one presentation/read-model boundary over adding map-specific state to gameplay controllers.

---

## 18. Dependency on the Phase 4 repeatable-loop contract (#40)

### Can be decided independently

- compact + expanded interaction model;
- upper-right placement;
- fixed orientation;
- player marker language;
- POI icon hierarchy;
- ordinary/stronger/elite/boss visual families;
- no fog/discovery for MVP;
- schematic coordinate map;
- local compact crop + full expanded map;
- no pathfinding route;
- timer information hierarchy;
- accessibility rules;
- performance rules;
- map world-position authority independent from visual prefabs.

### Consumed from finalized #40 contract

- exact elite cooldown semantics and values;
- exact boss cooldown semantics and values;
- premium/special reward cap semantics;
- whether capped encounters remain available for fallback rewards;
- reward-window rollover rules;
- stronger ordinary spot unlock/availability semantics;
- whether adaptive pressure is exposed outside gameplay at all;
- exact persisted fields or save migration used to reconstruct repeat availability.

### Contract rule

The merged #40 contract currently defines the upstream repeat-loop semantics. Map implementation should consume that authoritative state rather than reimplement it. This UX document still requires only a resolved presentation snapshot: locked?, encounter available?, next availability time?, reward state?

---

## 19. Wireframes

Detailed documentation wireframes live in `docs/MAP_MINIMAP_WIREFRAMES.md`.

Required examples include:

- normal gameplay minimap;
- expanded map;
- elite cooldown;
- boss available;
- ordinary spot respawn.

The wireframes are layout contracts, not final art.

---

## 20. Future test plan

### Automated / implementation tests

Future implementation should test:

- world X/Z -> map normalized position;
- chapter-bound clamping;
- player marker heading mapping;
- compact local crop transform;
- expanded full-map transform;
- icon selection by marker kind;
- marker view reuse and removal;
- ordinary state transitions;
- progression lock transition;
- elite/boss cooldown state transition;
- reward-state presentation independent from encounter availability;
- timer formatting around 0, 59s, 60s, 59:59 and ready state;
- no negative countdown display;
- map hidden/disabled under blocking modal;
- boss HUD + minimap layout coexistence;
- safe-area/aspect handling;
- DEV-only overlays not present in release UI;
- no per-frame managed allocation regression in steady state;
- no dependency on presentation prefab transforms.

### Human device review

See `docs/MAP_MINIMAP_DEVICE_REVIEW.md`.

At minimum verify:

- one-thumb navigation while glancing at compact map;
- readability at the smallest supported portrait resolution;
- boss HP + minimap overlap;
- sunlight/dark-scene contrast;
- distinguishability without relying on red/green;
- cooldown state without reading text;
- compact map tap target;
- expanded map POI selection;
- modal behavior;
- reduced-motion comfort;
- Phase 3C art does not obscure or move gameplay map markers.

---

## 21. Output documents

This design lane owns:

- `docs/MAP_MINIMAP_UX_SPEC.md`
- `docs/MAP_MARKER_STATE_MATRIX.md`
- `docs/MAP_MINIMAP_DEVICE_REVIEW.md`
- `docs/MAP_MINIMAP_WIREFRAMES.md`

No Unity assets are created by this PR.

---

## 22. Final decision table

| Topic | Options considered | Recommended MVP | Reason | Depends on |
|---|---|---|---|---|
| Minimap vs map | minimap only; expandable; full map only; minimap + expand; indicators only | Compact minimap + tap-to-expand full map | navigation stays glanceable while detailed state stays off HUD | Independent |
| Orientation | north-up; rotating player-up; fixed + player arrow | Fixed world orientation + heading arrow | stable spatial memory, less motion | Independent |
| Map scale | full chapter compact; local radius; hybrid; dynamic zoom | Local compact crop + full Chapter expanded | preserves local readability and global context | Independent |
| POI labels | always; never; expanded only; selection only | No compact labels; expanded/selected labels | prevents portrait clutter | Independent |
| Timers | exact everywhere; rounded; ring; near-ready only | Ring compact, rounded expanded, exact selected | avoids ticking HUD text | Exact values from #40 |
| Pressure display | exact step; qualitative; hidden | Hidden compact; qualitative selected only if exposed | adaptive algorithm should not become HUD noise | #40 / existing respawn contract |
| Elite/boss state | single state enum; separate dimensions | Separate progression, availability, reward dimensions | supports first clear, cooldown and cap/fallback combinations | #40 for exact semantics |
| Interaction | compact POI tapping; expand; waypoint; route | Tap compact to expand; tap POI for details; no route | one-thumb simple MVP | Independent |
| Fog/discovery | fog; POI discovery; hidden encounters; always-known | Always-known structural map | exploration system adds no current value | Independent |
| Schematic vs camera minimap | coordinate UI; render texture camera | Schematic coordinate UI | cheaper, clearer, testable, read-model friendly | Independent |

---

## 23. Implementation boundary

### Likely future systems/files to change

Names are illustrative and should follow existing project placement/conventions:

- presentation/UI map panel prefab and UXML/UGUI-equivalent assets as appropriate to the current UI stack;
- map presenter/controller under `Assets/_Game/Runtime/UI/`;
- marker view pool / icon resolver;
- compact minimap layout + expanded map panel;
- a thin adapter around existing `IWorldMarkerSource` / `WorldMarkerReadModel`;
- localization/content mapping for POI display names;
- composition-root wiring only as needed to inject existing read models/time provider into presentation;
- EditMode tests for transform/state/timer logic;
- PlayMode/UI smoke tests for layout/modal behavior.

### Must not become map authority

- Phase 3C environment prefab pivots;
- landmark visual dressing anchors;
- character model sockets;
- renderer bounds;
- temporary visual review scene.

Map positions must originate from gameplay/world marker authority. Moving art must never move a gameplay marker.

### Conflict/dependency flags

**PR #40 repeatable-loop spec**

- likely adds/finalizes encounter availability/reward timing state;
- map implementation should consume the finalized #40 read-model/state contract from `main`;
- this documentation PR intentionally does not change #40 files.

**Phase 3C visual integration**

- may change what the player visually sees at a POI;
- must not redefine gameplay coordinates for the map;
- map implementation should wait for visual integration checkpoint only where final HUD occlusion/device review needs the finished visual baseline.

### Merge boundary

This PR is documentation-only and may be reviewed independently. Runtime implementation should consume the finalized Phase 4 state contract from `main` and avoid inventing duplicate map-owned state.

---

**MAP / MINIMAP UX SPEC: READY FOR IMPLEMENTATION REVIEW**
