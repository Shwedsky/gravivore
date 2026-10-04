# GRAVIVORE — Phase 3B Actual Visual Asset Pack

Status: **FOUNDATION-COMPATIBLE, PENDING UNITY INTEGRATION**

This branch remains art/content only. It does not modify `S01SceneCompositionRoot`, gameplay composition, validators, encounter authority, save/progression, damage timing, gameplay colliders or the Phase 3A foundation implementation.

## Asset basis

The pack uses only sources already audited on `main`:

- Kenney Factory Kit 3.0 — CC0 1.0 Universal.
- Kenney Modular Space Kit 1.0 — CC0 1.0 Universal.
- Existing GRAVIVORE ArtSpike project-authored hard-surface meshes.
- Existing GRAVIVORE shared URP material family.

No new Sketchfab/login-gated, NC, SA, ripped, mirror-sourced or unclear-license asset was added.

## Presentation prefab sets

Environment:
- `RepairHub_Phase3B`
- `RelayYard_Phase3B`
- `CuttingFloor_Phase3B`
- `ShieldDump_Phase3B`
- `CapacitorField_Phase3B`
- `HaulerGraveyard_Phase3B`
- `EliteApproach_Phase3B`
- `EliteArena_Phase3B`
- `BossApproach_Phase3B`
- `BossArena_Phase3B`

Actors:
- `ScoutDrone_Phase3B`
- `ArcDrone_Phase3B`
- `Warden_Phase3B`
- `Carrier_Phase3B`
- `MagnetarGuard_Phase3B`
- `CustodianM0_Phase3B`

The accepted `Assets/_Game/ArtSpike/Prefabs/Enemies/Cutter_ArtSpike.prefab` remains the Cutter reference and is intentionally not duplicated or edited in this branch.

## Foundation compatibility

Foundation #38 discovers exact-name sockets through a direct child called `Presentation Sockets`. Every new Phase3B actor prefab now contains that container. Only meaningful sockets are authored; `Root` is omitted because the foundation deliberately falls back to the model root.

### Actor socket mapping

- **Scout Drone:** `Core`, `Sensor`, `AttackOrigin`, `HitCenter`, `GroundContact`, `VfxTop`. AttackOrigin is on the forward sensor/fork face; GroundContact is a hover reference, not a collider point.
- **Arc Drone:** `Core`, `AttackOrigin`, `HitCenter`, `GroundContact`, `VfxTop`. AttackOrigin sits between the electrical prongs.
- **Warden:** `Core`, `AttackOrigin`, `HitCenter`, `GroundContact`, `VfxTop`. AttackOrigin is forward of the recessed core/armor face.
- **Carrier:** `Core`, `AttackOrigin`, `HitCenter`, `GroundContact`, `VfxTop`. Its side manipulator remains visual geometry rather than gameplay authority.
- **Magnetar Guard:** `Core`, `AttackOrigin`, `HitCenter`, `GroundContact`, `VfxTop`, `VfxRear`, `TelegraphOrigin`. Existing custom `ShockwaveOrigin` is retained.
- **Custodian M-0:** `Core`, `AttackOrigin`, `HitCenter`, `GroundContact`, `VfxTop`, `VfxRear`, `TelegraphOrigin`. Existing `ConeAttackOrigin`, `LineAttackOrigin` and `CircleAttackOrigin` remain. Standard AttackOrigin is the best generic forward presentation origin; TelegraphOrigin is a low central presentation reference and must never redefine authoritative attack dimensions.

### Cutter compatibility

`Cutter_ArtSpike.prefab` currently has no `Presentation Sockets` container and no exact standardized socket direct children. Its direct root children are its accepted visual groups: `01_ArmoredBipedChassis`, `02_ArticulatedCuttingArms`, `03_HostileEnergy`.

For initial foundation integration, Codex may use the existing Phase 3A fallbacks for missing Cutter sockets. If a dedicated Cutter socket layer becomes useful, add it as an integration wrapper/adapter without changing Cutter gameplay authority or duplicating the accepted prefab in Phase3B.

### Repair Hub mapping

`RepairHub_Phase3B` has an identity root centered on the service platform, so the whole visual can be instantiated directly under foundation `MainPlatform` with identity position/rotation/scale.

| Foundation anchor | Phase3B visual reference | Purpose |
| --- | --- | --- |
| MainPlatform | RepairHub_Phase3B root | whole visual attachment |
| PlayerDockPoint | ServicePoint | visual service/dock alignment only |
| ManipulatorLeft | ManipulatorMount_L | left manipulator visual reference |
| ManipulatorRight | ManipulatorMount_R | right manipulator visual reference |
| RearManipulatorA | RearManipulatorMount_A | rear-left manipulator visual reference |
| RearManipulatorB | RearManipulatorMount_B | rear-right manipulator visual reference |
| RepairBeamOriginLeft | BeamEmitter_L | left repair beam presentation |
| RepairBeamOriginRight | BeamEmitter_R | right repair beam presentation |
| RepairBeamOriginRearA | BeamEmitter_RearA | rear-left repair beam presentation |
| RepairBeamOriginRearB | BeamEmitter_RearB | rear-right repair beam presentation |
| AmbientFxRoot | AmbientFxVisualRoot | ambient diagnostic/VFX attachment |

These are presentation references only. The prefab does not heal, dock/move the player, alter regen, or own an authoritative beam target.

### Expected integration binding

- Ordinary enemies: assign Phase3B whole prefabs to matching `S15_VisualCatalog` enemy presentation-prefab recipes while retaining existing fallback data.
- Cutter: keep the accepted Cutter fallback and foundation missing-socket behavior until an adapter is useful.
- Elite / boss: assign `MagnetarGuard_Phase3B` and `CustodianM0_Phase3B` through `Chapter01_VisualIntegration._elite` / `_boss`; do not infer controller dimensions from visual bounds.
- Repair Hub: assign `RepairHub_Phase3B` through `Chapter01_VisualIntegration._repairHub` under `MainPlatform`.
- Environment groups: attach as validated region/category dressing under matching Phase 3A visual region roots. Gameplay spawn/gate/encounter coordinates remain unchanged.
- Socket lookup: `PresentationSocketSet` resolves exact standardized transforms from `Presentation Sockets`; missing roles continue to use foundation fallbacks.

## Environment origin / pivot audit

All ten environment prefabs use an identity root: local position (0,0,0), identity rotation and positive (1,1,1) scale. Visible geometry uses local offsets only; there are no authored world-space coordinates inside these prefabs.

- **Repair Hub:** pivot = service platform/player service center. Exit/front remains open; machinery is peripheral/overhead.
- **Relay Yard:** pivot = region center. Mast/cabinets sit off the primary center lane.
- **Cutting Floor:** pivot = process-floor center. Press/cutters are overhead/side-biased.
- **Shield Dump:** pivot = region center. Rings/generators are edge-weighted.
- **Capacitor Field:** pivot = region center. The lane is usable, but one central capacitor is close enough to the encounter center to require gameplay-camera review.
- **Hauler Graveyard:** pivot = region center. Wreck masses remain edge-biased.
- **Elite Approach:** pivot = approach anchor. Pylons/coils frame the route rather than occupy its center.
- **Elite Arena:** pivot = elite spawn/arena center. Walls/coils are perimeter-only.
- **Boss Approach:** pivot = approach anchor. Heavy framing is side/overhead.
- **Boss Arena:** pivot = arena center. Crane/walls are peripheral and the central telegraph space remains open.

No gameplay anchor is moved.

## Visual self-review

This grading is deliberately strict and is a static prefab/material review, not a claim about final gameplay-camera appearance.

| Asset | Grade A/B/C | Main strength | Main weakness | Integrate now? |
| --- | --- | --- | --- | --- |
| Repair Hub | B | Immediate service-bay read with four manipulator directions | Still visibly kitbashed; simple repeated surfaces can read prototype-clean | Yes — audition |
| Relay Yard | B | Strong mast + cabinet silhouette | Kenney machinery simplicity is obvious at close range | Yes — audition |
| Cutting Floor | B | Press/cutter silhouette communicates function | Repeated modules and limited bespoke wear | Yes — audition |
| Shield Dump | C | Circular emitter language is distinct | Rings + simple generators remain too abstract/placeholder-like | No — structural integration only |
| Capacitor Field | B | Vertical coil forest gives navigation identity | Repetition is high; central unit may crowd combat visually | Yes — audition |
| Hauler Graveyard | C | Low bulky composition differs from tower zones | Box-long wrecks read as stock blocks, not convincing hauler carcasses | No — replace/refine before visual acceptance |
| Elite Approach | B | Compressed frame + paired coils signal escalation | Sparse prop vocabulary | Yes — audition |
| Elite Arena | C | Clean combat center | Perimeter walls/rings are too sparse for a premium elite destination | No — structural integration only |
| Boss Approach | B | Broken heavy frame establishes escalation | Needs bespoke damaged superstructure/detail | Yes — audition |
| Boss Arena | B | Crane asymmetry and open center support telegraphs | Edge language is still too simple/not monumental enough | Yes — audition |
| Scout Drone | B | Small triangular sensor-forward silhouette is distinct | Static proxy surfaces can still feel toy-like | Yes — audition |
| Cutter (accepted ArtSpike) | A | Accepted low asymmetric hostile cutter identity | Temporary prototype art; no standardized socket layer yet | Yes |
| Arc Drone | B | Twin prongs/coils and tripod stance communicate electrical role | Ring/fork kitbash is obvious at close camera | Yes — audition |
| Warden | B | Broad quadruped and forward armor read defensive/non-humanoid | Large simple armor plates lack authored industrial detail | Yes — audition |
| Carrier | C | Cargo-heavy asymmetric mass differentiates it | Stock box cargo makes it read like a toy logistics prop with legs | No — replace/refine before visual acceptance |
| Magnetar Guard | B | Paired generators and reinforced stance read elite | Static proxy construction lacks hero-level density/material sophistication | Yes — audition |
| Custodian M-0 | B | Strong asymmetric crane/reactor/multi-support silhouette | Static kitbash, repeated modules and unproven gameplay-camera scale | Yes — priority audition |

A means strong enough for the current prototype target, not shipping art. B means useful in an integrated APK but visibly replaceable. C means useful only for structural/foundation testing and should not be mistaken for an accepted visual baseline.

## Known risks

- **Static actors / no rig animation:** all new actor candidates are static assemblies; gait, recoil and articulated attack motion remain future work.
- **Kenney/simple-geometry look:** several environment B/C assets still expose the clean modular language associated with the rejected toy/prototype feel.
- **Material repetition:** the shared palette is coherent but too few surface families can make the full chapter feel uniform.
- **Visual collider mismatch:** foundation correctly keeps physics on authority roots. Elite/boss and wide ordinary silhouettes may visually disagree with existing hit/sensing dimensions; never resize gameplay colliders from these bounds.
- **Boss scale/readability:** Custodian M-0 must be checked for perceived mass, target point, HP readability and cone/line/circle presentation at the real portrait camera.
- **Socket tuning:** socket positions are safe static art-space references but need Unity visual verification before VFX/telegraph polish.
- **Repair Hub mapping:** manipulators are static; internal mounts/beam references do not create recovery or animation authority.
- **Sparse hero destinations:** Elite Arena and Shield Dump remain below the intended quality bar despite being integration-safe.

## Static validation

The serialized Phase3B prefab set must contain presentation geometry/transforms only:
- GameObject / Transform / MeshFilter / MeshRenderer;
- no Collider, gameplay MonoBehaviour, Rigidbody, Camera or Light;
- finite positive local scale;
- no built-in Unity primitive mesh references;
- committed mesh/material GUID dependencies only;
- identity root for every environment group;
- direct `Presentation Sockets` child on all six new actor prefabs with only the intended standardized names.

Unity compile, ProjectValidator, EditMode/PlayMode tests, Android build and the requested final Unity review renders are not claimed from this session because no Unity runner is available here.

## Review gate

After #38 is merged/synchronized, integration should consume these prefabs through the foundation contracts and produce real Unity captures/device review. B/C grades remain active review constraints; contract compatibility is not visual acceptance.
