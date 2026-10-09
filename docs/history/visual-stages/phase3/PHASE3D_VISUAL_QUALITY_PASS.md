# Phase 3D — Visual Quality, Shape Language & World Cohesion

## Scope and source of truth

Phase 3D starts from main `4f72a775221e02250d887f77304c3f9dd6fd20d5` and addresses the human device verdict from Phase 3C: gameplay, camera, thermal behaviour and frame-rate were acceptable, but the presentation still read as toy-like, blocky and kitbashed. The pass is deliberately camera-first for the production portrait camera: offset `(0, 14.8, -11.2)`, FOV `46`.

This pass does not change combat authority, encounter coordinates, spawn anchors, progression, persistence, map/minimap ownership, Android release tooling, or audio/VFX candidate/runtime files.

## Visual diagnosis

The committed Phase 3C captures show five recurring causes of the toy-like impression:

1. large forms are assembled from repeated orthogonal modules with little hierarchy;
2. bright cyan/orange rings dominate silhouettes instead of communicating a local function;
3. identical shiny metal and saturated accents make unrelated machinery read as one construction toy set;
4. symmetric layouts and hard square deck edges make destinations read as placed props rather than industrial spaces;
5. the Repair Hub silhouette reads as two lamp-like uprights because the service tooling is visually secondary.

The remedy is not more micro-detail. Phase 3D keeps the same underlying lightweight mesh inventory and changes proportion, pose, asymmetry, material hierarchy and the role of emissive elements.

## Shared shape language

Phase 3D uses these rules across the new pack:

- broad primary mechanical masses first;
- layered or offset secondary modules rather than identical stacks;
- angled supports, plates and machinery to break right-angle-only silhouettes;
- visibly supported cargo, reactors and tool heads;
- intentional left/right asymmetry on damaged or heavy machinery;
- small emissive elements only at cores, diagnostics, field nodes and energy indicators;
- cables/rubber as subordinate routing language rather than saturated decoration.

All Phase 3D pure-art prefabs remain script-free and collider-free.

## C asset replacements

### Carrier — C -> provisional B

New prefab: `Carrier_Phase3D`.

The body is re-proportioned into a lower coherent chassis, the two cargo masses are protected/offset rather than stacked boxes, the core is reduced to a functional front indicator, the loader boom is angled into the chassis, and the four supports are spread and canted. The silhouette now has a readable forward direction and logistics-machine construction.

### Capacitor Field — C -> provisional B

New prefab: `CapacitorField_Phase3D`.

The large glowing-ring hierarchy is removed. The field is now composed as two side capacitor/transformer banks around a clear combat centre, joined by a dark power trunk. Former coils are reduced to small energy/heat indicators. One damaged transformer breaks symmetry without obstructing the route.

### Shield Dump — C -> provisional B

New prefab: `ShieldDump_Phase3D`.

The two generator forms become damaged emitter carcasses. Former dominant rings are reduced, tilted and treated as broken emitter loops/residual hardware. A broad damaged shield plate anchors the scrapyard read.

### Hauler Graveyard — C -> provisional B

New prefab: `HaulerGraveyard_Phase3D`.

The two large masses are posed as tilted broken hauler chassis, the cargo module is overturned, frame pieces are snapped outward and former axle rings are reduced/rotated into drive components. The centre route stays open.

### Elite Arena — C -> provisional B

New prefab: `EliteArena_Phase3D`.

The arena keeps an open combat centre but its perimeter is now damaged industrial containment: an offset north section, asymmetrical side carcasses and small functional emitters. It reads as a destination without adding tall centre clutter.

No C-grade world asset remains in the static Phase 3D re-grade. This is a provisional implementation grade only; the human device review remains the final quality gate.

## Hero asset improvements

- **Scout**: lower/wider core, reduced sensor glow, thinner angled fins.
- **Arc Drone**: broader body, small energy nodes instead of shoulder rings, longer angled prongs and darker articulated supports.
- **Warden**: broader protected hull, thinner canted shield plates, smaller core and heavier feet.
- **Carrier**: lower coherent chassis, protected asymmetric cargo and supported loader boom.
- **Magnetar Guard**: wider carapace/pelvis hierarchy, asymmetrical generator masses, smaller functional field indicators and more load-bearing support stance.
- **Custodian M-0**: larger upper machinery/lower frame hierarchy, restrained reactor, separated reactor cage, strongly asymmetrical crane/cutter and cone-side machinery, reduced ground-discharge node.
- **Relay Yard**: cabinet damage/asymmetry, muted cable trench, less perfect mast/array alignment.
- **Cutting Floor**: offset presses, bridge/head variation, heat-stained cutting parts and dark rails.
- **Elite Approach**: containment nodes reduced to functional indicators; damaged frame and pylons are slightly irregular.
- **Boss Approach**: heavy structures, bridge and cable routing are offset/tilted to remove clean kit repetition.
- **Boss Arena**: crane, hook and damaged perimeter machinery are more asymmetrical; the combat centre remains unchanged.

The existing Cutter asset remains the Phase 3C A-grade reference and is not unnecessarily reworked.

## Repair Hub

New prefab: `RepairHub_Phase3D`.

The service-bay read is rebuilt around four manipulator silhouettes:

- left and right articulated service arms each have an upper segment, forearm and distinct tool head;
- two rear service arms reinforce the bay rather than resembling lamps;
- a crossbeam, side service frames and rear machinery define an actual docking/service volume;
- cable routing is dark/subordinate;
- tool surfaces use worn/heat-stained machinery materials;
- existing service/beam/socket anchors are preserved so regeneration and gameplay authority remain unchanged.

The hub still owns no regeneration logic.

## Physical blockers

Phase 3D adds **11 simple BoxCollider authority proxies** under `Chapter01WorldPresenter.GameplayRoot`; the visual environment remains collider-free.

| Proxy | Region | Purpose |
|---|---|---|
| Repair Hub Left Service Frame | repair-hub | stops walking through the left major service structure |
| Repair Hub Right Service Frame | repair-hub | stops walking through the right major service structure |
| Shield Dump Left Emitter Carcass | shield-dump | solid damaged machinery |
| Shield Dump Right Emitter Carcass | shield-dump | solid damaged machinery |
| Capacitor Field Left Bank | capacitor-field | large power-bank physical mass |
| Capacitor Field Right Bank | capacitor-field | large power-bank physical mass |
| Hauler Graveyard Left Chassis | hauler-graveyard | heavy wreck chassis |
| Hauler Graveyard Right Chassis | hauler-graveyard | heavy wreck chassis |
| Elite Arena Left Containment | elite-arena | large perimeter containment wreck |
| Elite Arena Right Containment | elite-arena | large perimeter containment wreck |
| Boss Arena Crane Tower | boss-arena | major ground-level crane tower |

The proxies have no Renderer or MeshFilter, use the existing `HardBlocker` layer, and are independent from the presentation meshes. No collider is added for cables, tiny props, overhead detail, particles or deck markings.

The Repair Hub centre and south exit are intentionally clear. Ordinary combat centres remain open; no spawn/gate/encounter coordinates are edited.

## Materials

Six shared URP material roles replace the repeated shiny/saturated treatment:

- `Phase3D_StructuralMetal` — dark load-bearing metal;
- `Phase3D_WornPaintedMachinery` — muted painted machinery;
- `Phase3D_HeatStainedMetal` — damaged/oxidised/heat-stained metal;
- `Phase3D_RubberCable` — dark non-metallic cable/rubber;
- `Phase3D_DirtyDeck` — low-sheen industrial deck;
- `Phase3D_SubtleEnergy` — restrained functional energy indicator.

The materials are shared across the pack. Bright saturated emissive is no longer a default decoration.

## Motion

`Phase3DMechanicalMotionPresenter` is one bounded presentation-only updater attached alongside the visual environment. It scans the visual environment once at initialization and caches the small set of authored transforms; it does not scan per frame.

It supplies:

- subtle Repair Hub service-arm hinge idle;
- small capacitor/heat indicator pulse.

The presenter never touches Magnetar, Custodian, player or ordinary-enemy authority roots, has no root motion, and owns no AI, combat, regeneration, navigation or encounter timing. Keeping it initialized from `ChapterVisualEnvironment` also avoids touching the central `S01SceneCompositionRoot` owned by parallel presentation work.

## Zone transitions

The Phase 3C continuous floor/route foundation is preserved. Phase 3D reduces the hard “boxes on floor” transition by using the same dirty-deck/material hierarchy across destinations, dark cable/power trunks at the ground plane, damaged peripheral machinery and asymmetric deck-edge composition. Combat paths are not filled with decorative clutter.

## Gates

Gameplay gate authority is unchanged: the same `Physical Gate Barrier` collider still enables/disables from world unlock state.

The old large saturated visible block is hidden and replaced by an industrial containment presentation:

- dark structural left/right uprights and top frame;
- three separated energy cells;
- two small emitter nodes;
- dark structural flank presentation.

The presentation pieces contain no colliders and no realtime lights.

## G-0 context review

G-0 is intentionally not fully redesigned in this branch.

Observed in the improved-world context:

- **scale**: readable at gameplay distance, but body volumes do not yet sell the same mass as the upgraded machines;
- **silhouette**: still generic and vertically stacked compared with the stronger Phase 3D mechanical silhouettes;
- **blockiness**: several large rectangular/cylindrical forms remain immediately legible as primitive construction;
- **proportions**: the upper body/core relationship is comparatively toy-like and the feet/support relationship lacks mechanical weight;
- **material mismatch**: the player is cleaner and more uniformly treated than the worn industrial world;
- **gait mismatch**: locomotion presentation remains stiffer than the environmental/hero-machine motion language.

No movement authority is changed here. G-0 is a remaining presentation target for its own controlled pass.

## Death / explosion boundary

The human review called the current death/explosion treatment old-looking. Phase 3D does **not** edit audio/VFX candidate/runtime files because that work has separate branch ownership.

Required future integration: use the new Phase 3D actor sockets and restrained material hierarchy for the incoming death/impact VFX; avoid reinstating giant saturated rings or gameplay-owned timing in presentation code.

## Performance

Phase 3C reference:

- ordinary combat: ~17k visible mesh triangles;
- mixed comparison: ~30k;
- boss: ~14k;
- environment: ~150 active renderers.

Static Phase 3D delta:

- all 16 Phase 3D prefabs reuse the original mesh references and renderer counts; no micro-mesh expansion was introduced;
- six shared materials replace the wider repeated material treatment;
- 11 physical proxies add colliders but **zero renderers**;
- each locked gate uses eight small presentation renderers plus the existing two flank visuals; compared with the old visible barrier + flanks this is a net +7 visible renderers per locked gate;
- one environment-only motion presenter updates a bounded set of cached transforms;
- no realtime Light, Camera, ParticleSystem, Rigidbody or transparent-layer stack was added to the Phase 3D pack.

Actual frustum triangles/renderers and device FPS remain pending Unity/device execution; they are not inferred from static inspection.

## Tests

Added/updated coverage:

- all 16 Phase 3D prefabs pass presentation-prefab safety validation;
- pure art prefabs contain no Collider, Rigidbody, Camera, Light or MonoBehaviour;
- every renderer resolves to the shared Phase 3D material family;
- mandatory C-replacement semantic structures are present;
- production camera baseline remains `(0, 14.8, -11.2)` / FOV `46`;
- `VisualIntegrationValidator.ValidateOrThrow` remains the static guard for unchanged spawn/encounter/foundation layout;
- collision proxies remain on gameplay/environment authority and outside presentation meshes;
- Repair Hub south exit remains clear of its new blockers;
- industrial gate visuals retain the original blocking-collider authority;
- ambient motion changes only environment presentation children while elite/boss authority positions remain unchanged;
- Phase 3C binding/smoke expectations are advanced to the new Phase 3D canonical prefab names.

A real-camera capture harness is added under `Phase3DReviewCaptureTests`, opt-in via `GRAVIVORE_PHASE3D_QA`.

Unity CLI is not available in the GitHub connector session used to author this branch, so compile, full EditMode, full PlayMode and ProjectValidator results must be obtained in Unity 6000.3.0f1 before merge. No pass result is fabricated here.

## Android

No Android/Unity runtime is available in this authoring session, so no DEV APK result is claimed. Device review is explicitly pending.

## Screenshots

`Phase3DReviewCaptureTests` is prepared to generate the required real Unity PNG review set using the production portrait camera:

1. `01_RepairHub_BeforeAfter.png`
2. `02_Carrier_BeforeAfter.png`
3. `03_CapacitorField_BeforeAfter.png`
4. `04_ShieldDump_BeforeAfter.png`
5. `05_HaulerGraveyard_BeforeAfter.png`
6. `06_EliteArena_BeforeAfter.png`
7. `07_Magnetar_NewWorld.png`
8. `08_Custodian_NewWorld.png`
9. `09_OrdinaryFamilies_NewWorld.png`
10. `10_Traversal_NewWorld.png`
11. `11_G0_In_NewWorld.png`
12. `12_IndustrialGate.png`

The first six combine committed Phase 3C evidence with the Phase 3D live render. The remaining six are Phase 3D live captures. They are not committed from this session because Unity was unavailable; generating fake/non-Unity review images would invalidate the quality gate.

## A/B/C grades

Static implementation re-grade, pending the human device verdict:

- Cutter — **A** (unchanged reference);
- Scout — **B**;
- Arc Drone — **B**;
- Warden — **B**;
- Carrier — **B** (from C);
- Magnetar Guard — **B**;
- Custodian M-0 — **B**;
- Repair Hub — **B**;
- Relay Yard — **B**;
- Cutting Floor — **B**;
- Capacitor Field — **B** (from C);
- Shield Dump — **B** (from C);
- Hauler Graveyard — **B** (from C);
- Elite Approach — **B**;
- Elite Arena — **B** (from C);
- Boss Approach — **B**;
- Boss Arena — **B**;
- industrial gates — **B** (from Phase 3C C-risk presentation).

These grades mean “no longer an obvious static C-grade hole”; they do not substitute for the portrait-camera device review.

## Remaining weak points

1. G-0 still needs a dedicated silhouette/material/gait pass.
2. Death/explosion quality remains owned by the separate audio/VFX work and needs integration review against the new forms.
3. Because the pass deliberately reuses the lightweight mesh inventory, chamfer/bevel quality is improved primarily through proportion, pose and textured material hierarchy rather than new high-density geometry.
4. Final A/B/C confirmation requires the generated real-camera captures on Unity/device.
5. Performance must be re-snapshotted after Unity import because gate presentation adds a small renderer delta.

## Conflict boundaries

Reviewed branch scope intentionally avoids:

- persistence/restore and release safety (#45 ownership);
- map/minimap module (#46 ownership);
- Android build system;
- audio candidate/runtime and future death-VFX ownership.

`S01SceneCompositionRoot.cs` is restored byte-for-byte to current main; Phase 3D ambient motion initializes from `ChapterVisualEnvironment` instead. Phase 3B source prefabs are retained untouched as the before/reference pack. Phase 3D is a separate pack wired through the existing visual catalog/scene integration.

## Git

Branch: `art/phase3d-visual-quality-pass`

Base: `4f72a775221e02250d887f77304c3f9dd6fd20d5`

The branch is intended for PR review only. Do not merge before Unity tests, real-camera captures and human device review.

**PHASE 3D VISUAL QUALITY PASS: DEVICE REVIEW PENDING**
