# Seven-actor production/review delivery V2

All seven hostile actors have authored editable sources, rigged FBXs, PBR maps, three LODs, isolated prefabs/controllers and complete-family review evidence. **All seven `READY_FOR_CHAPTER01_INTEGRATION` flags remain false.** This is a reviewable manufacturing package, not owner-certified visual/device acceptance.

Branch: `art/chapter01-actor-production-v2`. [Draft PR #74](https://github.com/Shwedsky/gravivore/pull/74). Baseline: `38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc`. No merge. World R2 / PR #71, production bindings, G-0, collision, balance, progression, combat, saves and UI remain unchanged.

## Actor manufacture and remaining visual gaps

Donors below were opened/hash-checked as mechanical references only. **100% of delivered geometry, rigs, weights, clips and textures is newly authored; 0 donor bytes reused.** Restricted raw sources remain private. See [PROVENANCE.md](PROVENANCE.md) and [DONOR_REINSPECTION.json](DONOR_REINSPECTION.json).

### Scout
Light biped, two long integrated blade terminations. Inspected donor: George. Custom geometry: 100%, 164 editable components; 15 authored bones.
LOD0/1/2: 47684/27178/13828 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Red. Source rest dimensions (width/depth/mesh height): 1.346/0.396/1.176 m; maximum ground-relative Z: 1.201 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Head/chest are more angular than the concept; aggressive stance and stride timing need device motion review. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/Scout_concept_Unity.jpg), [donor comparison](comparisons/Scout_donor_production.jpg), [gameplay screenshot](unity/Scout_gameplay_0.png), [G-0 scale](unity/Scout_scale_G0.png), [in-environment diagnostic](unity/Scout_in_environment.png).

### Cutter
Low four-support predator, paired articulated cutting drives. Inspected donor: QuadShell. Custom geometry: 100%, 210 editable components; 16 authored bones.
LOD0/1/2: 53595/30547/15537 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Red. Source rest dimensions (width/depth/mesh height): 1.611/1.921/0.850 m; maximum ground-relative Z: 0.879 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death, Cut. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Dorsal shell is simpler than the concept; cutting-drive prominence must be judged at phone scale. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/Cutter_concept_Unity.jpg), [donor comparison](comparisons/Cutter_donor_production.jpg), [gameplay screenshot](unity/Cutter_gameplay_0.png), [G-0 scale](unity/Cutter_scale_G0.png), [in-environment diagnostic](unity/Cutter_in_environment.png).

### Warden
Broad heavy biped, two separately held/controlled shields. Inspected donor: MSGDi Striker. Custom geometry: 100%, 188 editable components; 15 authored bones.
LOD0/1/2: 55840/31827/16190 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Amber. Source rest dimensions (width/depth/mesh height): 2.080/0.969/1.966 m; maximum ground-relative Z: 1.991 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death, Block. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Faceted shoulder/torso treatment and simpler grip mechanics; verify shield separation during motion. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/Warden_concept_Unity.jpg), [donor comparison](comparisons/Warden_donor_production.jpg), [gameplay screenshot](unity/Warden_gameplay_0.png), [G-0 scale](unity/Warden_scale_G0.png), [in-environment diagnostic](unity/Warden_in_environment.png).

### ArcDrone
Airborne emitter, blue core, no ground contacts or walking chains. Inspected donor: EyeDrone. Custom geometry: 100%, 61 editable components; 8 authored bones.
LOD0/1/2: 20934/11932/6070 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Blue. Source rest dimensions (width/depth/mesh height): 1.479/0.777/1.135 m; maximum ground-relative Z: 1.493 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death, Hover, Discharge. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Contained lens is flatter than the painted concept; verify airborne read in all device headings. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/ArcDrone_concept_Unity.jpg), [donor comparison](comparisons/ArcDrone_donor_production.jpg), [gameplay screenshot](unity/ArcDrone_gameplay_0.png), [G-0 scale](unity/ArcDrone_scale_G0.png), [in-environment diagnostic](unity/ArcDrone_in_environment.png).

### Carrier
Low enclosed elongated hover vehicle, integrated propulsion. Inspected donor: UnityFan Vehicle 012 (member 013_2). Custom geometry: 100%, 70 editable components; 4 authored bones.
LOD0/1/2: 27863/15879/8077 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Amber. Source rest dimensions (width/depth/mesh height): 1.305/2.207/0.731 m; maximum ground-relative Z: 0.815 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death, Bank. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Roof/rear hull subdivision remains simpler than the concept; verify orange propulsion read without bloom. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/Carrier_concept_Unity.jpg), [donor comparison](comparisons/Carrier_donor_production.jpg), [gameplay screenshot](unity/Carrier_gameplay_0.png), [G-0 scale](unity/Carrier_scale_G0.png), [in-environment diagnostic](unity/Carrier_in_environment.png).

### Magnetar
Custom heavy containment, buttresses, plated supports and magnetic terminals. Inspected donor: Trilobite joints/support reference. Custom geometry: 100%, 247 editable components; 19 authored bones.
LOD0/1/2: 69074/39372/20030 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Amber. Source rest dimensions (width/depth/mesh height): 3.423/2.426/2.948 m; maximum ground-relative Z: 2.973 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death, Charge, Vent. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Containment and support armor remain cleaner/simpler than the concept; heavy charge/vent weight needs device review. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/Magnetar_concept_Unity.jpg), [donor comparison](comparisons/Magnetar_donor_production.jpg), [gameplay screenshot](unity/Magnetar_gameplay_0.png), [G-0 scale](unity/Magnetar_scale_G0.png), [in-environment diagnostic](unity/Magnetar_in_environment.png).

### Custodian
Custom reactor vault/chimney, independent boss mantles, discharge structures and phase poses. Inspected donor: Audited heavy joints/support reference; no boss donor. Custom geometry: 100%, 300 editable components; 19 authored bones.
LOD0/1/2: 96893/55228/28096 triangles; one consolidated skinned renderer and one shared family PBR material per visible LOD. Energy: Red. Source rest dimensions (width/depth/mesh height): 4.901/3.226/3.933 m; maximum ground-relative Z: 3.958 m. Imported animation bounds are conservative and separately recorded in UNITY_VALIDATION.json.
Source clips: Idle, Move, Attack, Hit, Death, Telegraph, AttackLine, AttackCircle, AttackCone. Move imports as Run; ROOT stays stationary. Socket hierarchy is in the per-actor metrics.
Remaining visual gap: Mantle armor and reactor routing remain cleaner than the concept; verify phase differentiation and arena silhouette on device. `READY_FOR_CHAPTER01_INTEGRATION=false`.
[Concept / source / gameplay camera](comparisons/Custodian_concept_Unity.jpg), [donor comparison](comparisons/Custodian_donor_production.jpg), [gameplay screenshot](unity/Custodian_gameplay_0.png), [G-0 scale](unity/Custodian_scale_G0.png), [in-environment diagnostic](unity/Custodian_in_environment.png).

## Rig, maps, LODs and evidence

Generic mechanical rigs use one rigid bone weight per vertex. Warden shields parent through TOOL -> ELBOW -> SHOULDER, with physical grips, independent pivots and Block. Scout has two long integrated blade roots/tips. Arc has aerial outriggers/vanes, Hover/Discharge and measured flight clearance; Carrier has propulsion pivots and Bank, without leg chains. Magnetar has independent containment/tool chains and Charge/Vent. Custodian has independently articulated reactor, mantles and discharge tools, with Telegraph/AttackLine/AttackCircle/AttackCone. All poses are available in the device reviewer.

Clips are authored in-place at 30 FPS, with baked presentation floor correction through BODY; ROOT is never translated. Sources were reopened and every authored frame sampled: finite geometry, no zero-area export triangles, one skin weight per vertex, UVs, decreasing LOD geometry, packed maps, stationary ROOT and >1 cm posed floor clearance. This does not prove stride synchronization with future gameplay movement or continuous mechanical/foot IK.

Shared 1024 trim maps: BaseColor, MetallicSmoothness (R metallic / A smoothness), Normal, Occlusion and localized Red/Amber/Blue emission. Painted armor, bare metal, graphite recesses and energy have distinct surface responses. URP/Lit is opaque; Android ASTC 6x6 with mipmaps. No added realtime actor lights or transparent effects. LOD transitions .14/.075/.025; quality bias applies. Readable CPU meshes are deliberately retained in the review and need profiling during integration.

Camera is unchanged main: offset (0, 14.8, -11.2), look-at 0.9, FOV 46, portrait 9:16. G-0 remains its actual live prefab/root scale, measured renderer height 1.771 m. Primary captures use automatic camera LOD; forced LOD0/1/2 and emission-disabled captures are separate. Each actor has Blender front/three-quarter/top/silhouette, donor and concept comparisons, four Unity headings, phone view, G-0 scale view and in-environment diagnostics. Elite/boss have telegraph/phase evidence. Family overview is wider diagnostic framing, explicitly not gameplay camera.

[Complete Unity family at actual scale](unity/Complete_family.png), [complete phone family](comparisons/Complete_phone_family.jpg), [all authored sources](comparisons/Complete_authored_family.jpg). Studio tiles are independently framed and cannot be used as a scale comparison.

## Executed checks

- EditModeVerified: 480/480 passed; 0 failed, 0 skipped.
- PlayModeAll: 135/136 passed; 0 failed, 1 skipped.
- Optional skipped check: Gravivore.Tests.PlayMode.VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested. Set GRAVIVORE_VISUAL_INTEGRATION_QA to export structural review captures.
- Blender source validation: PASS for all seven; see SOURCE_VALIDATION.json.
- Unity 6000.3.0f1 compilation, asset validation, matched camera capture and production dependency isolation: PASS; see UNITY_VALIDATION.json.
- Existing ProjectValidator.ValidateOrThrow: PASS (executed).
- Read-only conservative rest-envelope audit: all current spawn anchors and boss-arena radius clear. See CLEARANCE_REVIEW.json; animated movement is not certified.
- Protected production paths and approved PNG custody: PASS; see SCOPE_VALIDATION.json.
- Android ARM64 IL2CPP development review APK: Succeeded.
- Owner device visuals / Android 60 FPS / steady-state allocations / animated encounter clearance: not executed; no Android device connected.

APK: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\chapter01-actor-production-v2\Builds\Android\gravivore-actor-review-v2.apk`. Package `com.gravivore.actorreview`, version 0.1.0/code 1, separate from the game. 108.0 MiB. SHA-256 `5585f72afb44f8f44d1f77935272f3612da7b4fad6ab394d53df12ac3c3e6bd6`.
Review scene starts first and exposes all seven actors, every role pose, LOD selection and family overview. Existing global build audits require unchanged Bootstrap/Chapter01 scenes to be packed after it; this does not bind any new actor into production.

## Acceptance limits and assumptions

The models identify all seven roles and reproduce the required locomotion/anatomy classes, but comparison images show simpler, more angular armor and cleaner surfaces than the painted reference. Concept fidelity and device motion remain open review gates, not inferred passes from mesh detail/import/tests. No device FPS or allocation claim is made.

Outboard blades, shields, wings, supports and boss structures exceed existing center collision radii. Those radii and the world were not altered. A read-only conservative rest-envelope check against serialized main collision boxes, closed gates, world bounds, all spawn anchors and boss-arena radius passed. Custodian has approximately 1.9694 m conservative arena margin at its start. This cannot guarantee clearance during animation, movement near walls or turns: center colliders do not contain the full visual reach. Production motion clearance, stride timing, animation blends and shadow/LOD behavior remain integration gates. The review does not demonstrate damage-source-to-impact behavior because combat integration is explicitly out of scope.

Scale-strip axes are not surveyed: biped/elite/boss height and vehicle length guide authored relative hierarchy; the airborne root stays on the gameplay ground plane with its mesh elevated. Hidden construction, support details, rigid grips and Generic bone axes are production assumptions, not newly approved anatomy.

Next defined gate: owner device review of the COMPLETE seven-actor family, followed by actor integration only after visual/motion/performance and clearance acceptance. No additional spec ID was supplied. Draft remains unmerged.

## Checkpoints and files

- 80ae82c: authority/scope checkpoint, pushed and Draft PR created before modeling.
- 6733e02: A — Scout/Warden/Carrier.
- 2f966e4: B — Cutter/Arc Drone.
- 16f8add: C — Magnetar.
- 74dc7c7: D — Custodian.
- E: final sources/material/motion refinements, Unity review, validation and delivery evidence in the final branch commit.

Files: [FILES_CHANGED.txt](FILES_CHANGED.txt). Source roots: `art/chapter01-actor-production-v2/`, `Assets/_Game/ArtReview/ActorProductionV2/`, `Tools/actor-production-v2/` and this evidence directory. Initial custody also adds the exact approved actor authority/donor documentation needed from the pinned commits. Unity-generated changes outside these paths are restored before delivery; private generated build settings/caches are not promoted.

Evidence is an implementation snapshot under docs/history, not new project authority.
