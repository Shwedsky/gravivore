# One recommended Chapter01 stack

Use **Quaternius MegaKit + selected Molten Maps machinery**, with existing GRAVIVORE damage/hero assets and redesigned enemy shells. CreepyCat is a limited surface/cover supplement. Quaternius Essentials and modular guns provide donors; EXE provides the primary UI frame language. This is a concrete V3 selection proposal, not production integration or a claim that the current scene has reached A.

The local concept board is present and inspected. Its elevated gameplay frame establishes dark industrial density, layered broken floors, edge services, large vertical machinery and clear cyan versus orange/red energy. The approved **bipedal G-0 remains unchanged**. Approved enemy silhouettes and current map/collision/attack anchors remain authoritative. Current B++ evidence is the merged historical captures in `docs/concept-fidelity-v2/internal/01_repair_active.png` and `docs/chapter01-v3/internal/g0_bipedal_with_m0.png`; V2 did not recapture the production scene.

## Environment backbone

Primary: `env-quaternius-megakit` — Platform_Metal, Platform_DarkPlates, Platform_Rails_2, Platform_Ramp_2, Platform_Stairs_2, ShortWall_MetalPlates_Straight, Door_Frame_Square and Door_DarkMetal. Secondary: selected `env-creepycat-starter` floor/wall panels and door framing.

Why: the current large repeated deck plates and sparse transitions lack layered construction. These modules add raised seams, support rails and gate thickness while preserving the approved footprint. Adaptation: establish common module scale/pivots; author one cold-metal palette/trim vocabulary and reduce bright orange stock surfaces; batch static surfaces. The downloaded MegaKit is the **189-model free Standard subset**, not the complete paid/advertised collection.

## Floor / wall / damage detail

Primary: MegaKit platform variants over the existing `Deck_V2_*`, `Broken_Edge_V3_*`, `Service_Trench_V3` and `Collapsed_Hull_V3` damage language. Secondary: CreepyCat Tile/Floor/WallMetal families; Sickhead FloorTile01/02 and WallBayDoor are reserve **DONOR** surfaces only.

Why: repeated smooth/dark plates currently carry too little variation at the gameplay angle. Adaptation: retain custom fractures and playable collision, add selected grated/raised panel areas and broken perimeter silhouettes, bake/reuse worn-metal detail. None of the free packs supplies the whole approved damage composition. Sickhead's grey fallback renders establish geometry only; original PBR reconstruction needs a separate V3 material pass.

## Machinery / hero props

Primary: `env-molten-maps` Generator / Generator Pile Large / Cryo Tube / Centrifuge / Command families. Secondary: Quaternius Essentials Prop_Crate/Prop_Barrel for cover; existing GRAVIVORE Repair Hub, Reactor_V2, Turbine_V2 and Containment_Vessel_V2 remain custom hero structures.

Why: the current scene needs readable vertical industrial landmarks and a more functional hub silhouette, rather than additional generic boxes. Adaptation: recolor the gradient atlas to cold metal, reserve cyan for serviced tech and orange/red for active danger; consolidate up to three materials; add selected hoses/rails around existing hero geometry. Molten's atlas is **stylized color, not a PBR wear set**. Generator rings are useful supporting massing, not a ready replacement for the concept's large articulated machinery.

## Pipes / services

Primary: MegaKit TopCables_Straight / corners, Column_Pipes and Prop_PipeHolder, grouped at periphery and hero-machine connections. Secondary: CreepyCat Floor_Pipes_01; Sickhead Pipes01/02 and DuctVent as private geometry donors if needed.

Why: current isolated conduits lack connected service routing and depth at the map edge. Adaptation: reduce materials, simplify repeated bundles, establish endpoints/brackets and protect attack lanes. Column_Pipes is 6480 tris / three materials and is a **landmark**, not a repeated floor tile. TopCables_Straight is 942 tris / four materials and should be consolidated. TechLab is excluded: exact license unresolved and the assembly is 95364 tris. The missing armored-pipes source does not block this stack.

## Enemy donors

Primary: current GRAVIVORE approved role shells, selectively kitbashed with `env-quaternius-essentials` QuadShell articulation, Trilobite plating and EyeDrone housing. Secondary: `mechs-combat-drone` angular shell for Arc Drone; Quaternius mech joints only for specific mechanical articulation.

Why: Scout/Cutter/Warden/Arc Drone/Carrier must remain distinguishable by silhouette and weapon placement. Stock bipedal robots do not cover radial Scout or paired front blades on Cutter. Adaptation: retain current controller/socket contracts; build role-specific red/orange cores and armor; reduce skinning/triangles for 25-enemy cap. Stock QuadShell7494 / Trilobite8338 triangles are donor starting points, not mobile acceptance thresholds.

## Elite / boss donors

Primary: existing `Magnetar_V1` and `Custodian_V3`, with custom Blender shell/detail work. Secondary: MegaKit PipeHolder/Vent and Molten generator rings as static attachment donors; Essentials shells only for a few armor mechanisms.

Why: Magnetar needs a low broad multi-support elite silhouette; Custodian needs unique boss machinery and attack causality. No downloaded stock mech matches either approved target. Adaptation: custom armor masses, exposed energy paths and weapon source silhouette; keep AttackOrigin/HitCenter/HealthAnchor/DeathOrigin and current phases. Do not promote Striker or a generic robot into the boss role.

## Weapon / equipment donors

Primary: `weapons-quaternius` AR_1/AR_4/Sniper_1/Pistol_1 plus barrel/body/scope/stock modules. Secondary: `weapons-tower-defence` turret head/support and selected RTS laser/turret mechanisms; existing `Emitter_M0` stays the presentation base.

Why: current M-0 and progression need distinct visible emitter/barrel/coil masses. Adaptation: remove humanoid handles, author mech socket/pivots and a cyan player emitter, preserve firing origin/timing. Merge material slots; built examples have up to four materials. A full weapon rank family remains a custom assembly task; donor quantity does not establish readable progression.

## VFX

Primary: existing pooled GRAVIVORE Phase6B + HostileChargeV3/HostileTravelV3/PFX_HostileImpact. Secondary: a few Wallcoeur impact/muzzle masks, Vefects fire/smoke flipbooks and localized Fog masks, all **DONOR**.

Why: the concept depends on visible source → travel → impact; cosmetic bursts alone do not fix causality. Adaptation: use existing URP particle materials/pools, reduce images/caps/layers, avoid point lights/colliders/audio scripts and preserve telegraph timing. No source animation was executed, and no static beauty image is offered as VFX validation. Reject Magic and Black Hole stock families; no VFX Graph package is added. Overdraw and simultaneous live particles require V3 runtime testing.

## UI

Primary: `ui-exe` normal buttons/dividers/inventory frames (Catherine Laserna, CC BY 4.0). Secondary: `ui-kenney` CC0 bars/small glyphs, adapted to the same frame language.

Why: current large plain rectangles and minimap border lack a consistent sci-fi frame hierarchy. Adaptation: author 9-slice borders and one atlas; recolor black frame art for contrast on dark surfaces; keep text, safe-area sizing, health/stat semantics and interaction unchanged. Use restrained cyan highlights; avoid solid luminous panels occupying large portrait-screen areas. Compose custom boss-health and minimap frames from this language. Include EXE attribution in shipped notices; excluded bundled fonts need independent OFL notices if used later. Tiago/Icons8 remains excluded pending mixed-rights resolution.

## Coherence and V3 order

Start with one representative Repair Hub + adjacent floor/wall/service section at the existing camera and scale. Compare it with the approved concept/current B++ capture before expanding. Author the shared material palette first, then layer broken edges and a few large machinery landmarks; preserve center-screen movement/telegraphs. Only then adapt enemies, weapon progression, causal VFX and UI in that order. Do not scatter all selected source families into every zone.

Asset Store donors/USE families stay owner-local or in private artifacts; raw or modified source meshes/textures cannot enter the public repository. A public V3 implementation must use reproducible local donor ingestion or independently authored replacement geometry. This is an integration constraint, not a request for another download or subscription.

Next spec: **FREE ASSET INTAKE V3 — production integration plan**, to be selected by ChatGPT's review of this Draft PR. No Android/device build in V2.
