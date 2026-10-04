# GRAVIVORE — Phase 3B Actual Visual Asset Pack

Status: **PENDING HUMAN REVIEW**

This branch is art/content only. It intentionally does not modify `S01SceneCompositionRoot`, gameplay composition, validators, encounter authority, save/progression, damage timing, colliders or runtime bindings.

## Asset basis

All environment geometry in this pack is assembled from the already-audited CC0 assets on `main`:

- Kenney Factory Kit 3.0 — CC0 1.0 Universal.
- Kenney Modular Space Kit 1.0 — CC0 1.0 Universal.
- Existing GRAVIVORE ArtSpike project-authored hard-surface meshes.
- Existing GRAVIVORE shared URP material family.

No Sketchfab/login-gated, NC, SA, ripped or unclear-license asset is introduced.

## Presentation prefab sets

Environment:
- RepairHub_Phase3B
- RelayYard_Phase3B
- CuttingFloor_Phase3B
- ShieldDump_Phase3B
- CapacitorField_Phase3B
- HaulerGraveyard_Phase3B
- EliteApproach_Phase3B
- EliteArena_Phase3B
- BossApproach_Phase3B
- BossArena_Phase3B

Actors:
- ScoutDrone_Phase3B
- ArcDrone_Phase3B
- Warden_Phase3B
- Carrier_Phase3B
- MagnetarGuard_Phase3B
- CustodianM0_Phase3B

Cutter remains the accepted `Assets/_Game/ArtSpike/Prefabs/Enemies/Cutter_ArtSpike.prefab` reference for this phase.

## Design notes

Repair Hub is a semi-hangar with a service deck, four manipulator assemblies, heavy framing, cable runs and explicit beam/service attachment transforms.

Ordinary enemies use visibly different topology:
- Scout: low triangular hovering sensor craft.
- Arc Drone: tripod electrical unit with twin coils/prongs.
- Warden: broad low quadruped with forward armor.
- Carrier: heavy logistics chassis with asymmetric cargo mass.
- Cutter: existing accepted asymmetric cutting unit.

Magnetar Guard uses paired shoulder generator/coil masses and reinforced multi-support stance.

Custodian M-0 is an asymmetric maintenance machine with crane/excavator arm DNA, exposed reactor, multiple supports and explicit cone/line/circle attack attachment transforms.

## Integration contract

The prefabs contain MeshFilter/MeshRenderer/Transform only. They intentionally contain:
- no gameplay MonoBehaviours;
- no colliders;
- no runtime search or loading logic;
- no dependency on a scene name.

Codex Visual World Integration Foundation should bind/instantiate these as presentation only.

## Review gate

Human/Unity review must still produce actual Unity renders for the 12 requested views. This repository session cannot execute Unity, so no render/build/test pass is claimed here.
