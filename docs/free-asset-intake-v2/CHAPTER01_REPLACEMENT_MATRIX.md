# Chapter01 presentation replacement matrix — V2 proposal

All24 required presentation roles are covered below. No production file was changed. CURRENT identifies the merged builders' generated asset bindings and runtime presentation code; generated art is largely owner-local/ignored, so these are binding paths, not a claim that every prefab is tracked in Git.

Inspection anchors: `Assets/_Game/Editor/VisualIntegration/FirstVisualSliceBuilder.cs`, `ConceptFidelityBuilder.cs`, `Chapter01ProductionBuilder.cs`, `Chapter01V3Builder.cs`; local generated `Chapter01_VisualIntegration.asset`; the approved enemy-target document; merged historical `01_repair_active.png` and `g0_bipedal_with_m0.png`. Visual weaknesses are technical-art assessments from those captures and the concept board, not a new production playtest.

Path shorthand: **CF** = `Assets/_Game/Content/ConceptFidelityV2/Prefabs/`; **VS** = `Assets/_Game/Content/VisualSlice/Prefabs/`; **CP** = `Assets/_Game/Content/Chapter01Production/Prefabs/`; **V3** = `Assets/_Game/Content/Chapter01V3/Prefabs/`; **P6** = `Assets/_Game/Content/Presentation/Phase6B/VFX/Prefabs/`; **UI** = `Assets/_Game/Runtime/Presentation/UI/`; **MAP** = `Assets/_Game/Runtime/Presentation/Map/`.

## 01. Repair Hub

CURRENT: CF Repair_Platform_V2, Repair_Pedestal_V2, Repair_Upper_V2, Repair_Forearm_V2, Repair_Joint_V2, Repair_Tool_V2, Repair_Scanner_V2; service point binding.

PROBLEM: hero silhouette and service mechanisms blend with the deck; the concept needs a layered working machine with connected services.

RECOMMENDED SOURCE: existing hub + MegaKit Prop_Vent/Column_Pipes/TopCables + Molten Command/Centrifuge parts.

USE / DONOR / CUSTOM: CUSTOM hub retained; selected USE service modules become DONOR attachments where reshaped.

EXPECTED VISUAL GAIN: articulated tool mass, nested platform/rail detail and readable cyan service identity.

TECH RISK: preserve ServicePoint/interaction radius/arm pivots; reduce static material count; private source handling for optional Asset Store parts.

## 02. Floors/platforms

CURRENT: CF Deck_V2_0/1/2; CP Deck_Module/Service_Markings; V3 Broken_Edge_V3_0/1 and Service_Trench_V3.

PROBLEM: repetitive broad plate surfaces lack layered seams, grating and irregular edge construction.

RECOMMENDED SOURCE: MegaKit Platform_Metal/DarkPlates/Rails/Ramp/Stairs; CreepyCat Floor_Squared_01/02_6x6, Floor_Hole_01; Molten Floor Metal Square Grate.

USE / DONOR / CUSTOM: USE selected surfaces; CUSTOM existing broken-edge/trench geometry retained and extended.

EXPECTED VISUAL GAIN: mixed floor construction, functional rails and local wear while keeping a readable central lane.

TECH RISK: no collision/map footprint changes; atlas/static batching; repeated four-material platforms must consolidate; source colors require one metal palette.

## 03. Wall/gate transitions

CURRENT: CF Bulkhead_V2/Containment_Frame_V2; CP Service_Arch.

PROBLEM: thin/sparse transitions provide weak zone enclosure and industrial scale.

RECOMMENDED SOURCE: MegaKit ShortWall_MetalPlates_Straight/Corner, Door_Frame_Square/Door_DarkMetal; CreepyCat DoorWay_01_Large and Wall_Gear_01_Half.

USE / DONOR / CUSTOM: USE selected families with CUSTOM transition composition.

EXPECTED VISUAL GAIN: thicker layered gate frames, visible wall services and more distinct thresholds.

TECH RISK: preserve passage widths, occlusion/readability and existing spawn/zone boundaries; combine material slots.

## 04. Periphery

CURRENT: CF Pressure_Wreck_V2/Curved_Services_V2; V3 Collapsed_Hull_V3/Broken_Edge_V3_*; CP zone dressing.

PROBLEM: perimeter is too sparse and regular relative to the damaged layered concept.

RECOMMENDED SOURCE: existing broken hulls + MegaKit BottomMetal/ShortWall/cable corners; selected Molten grates/supports.

USE / DONOR / CUSTOM: CUSTOM broken silhouettes; USE supports and DONOR fragments.

EXPECTED VISUAL GAIN: connected edge services, collapsed layers and stronger dark industrial depth outside walkable space.

TECH RISK: avoid center-screen clutter, transparent overdraw and camera-blocking tall props; static merge/LOD.

## 05. Pipes/conduits

CURRENT: CF Curved_Services_V2; CP service presentation around zone machinery.

PROBLEM: isolated pipe shapes do not form a convincing connected service network.

RECOMMENDED SOURCE: MegaKit TopCables_Straight/Column_Pipes/Prop_PipeHolder; CreepyCat Floor_Pipes_01; Sickhead Pipes01/02 and DuctVent reserve donors.

USE / DONOR / CUSTOM: USE MegaKit/CreepyCat; DONOR Sickhead; CUSTOM junctions/brackets.

EXPECTED VISUAL GAIN: layered pipe bundles, strong vertical service columns and clear machinery connections.

TECH RISK: 6480-triangle Column_Pipes is a limited landmark; reduce four-material cable bundles. TechLab/license-unresolved and missing armored pipes excluded.

## 06. Reactors/generators

CURRENT: CF Reactor_V2/Turbine_V2/Containment_Vessel_V2; CP zone machinery.

PROBLEM: generic surface treatment and sparse supporting machinery weaken the functional energy hierarchy.

RECOMMENDED SOURCE: existing animated hero assets + Molten Generator/Generator Pile Large/Cryo Tube/Centrifuge.

USE / DONOR / CUSTOM: CUSTOM current animated heroes; USE supporting generator families, DONOR rings for reshaped attachments.

EXPECTED VISUAL GAIN: distinct energy-column masses and nested industrial mechanisms with restrained orange/cyan accents.

TECH RISK: retain animation/core pivots; no balance changes; gradient atlas needs metal/wear treatment and material consolidation.

## 07. Containers/cover

CURRENT: CP zone props and existing cover presentation around approved gameplay proxies.

PROBLEM: repeated generic blocks have little top-facing panel or latch detail.

RECOMMENDED SOURCE: CreepyCat Crate_01; Essentials Prop_Crate/Prop_Barrel; Molten Container family.

USE / DONOR / CUSTOM: USE CreepyCat/Molten selected shells; DONOR Essentials recolored/reshaped shells.

EXPECTED VISUAL GAIN: clearer crate/cover vocabulary, top-panel variation and industrial latches.

TECH RISK: match proxy extents and firing visibility; Essentials crate3984 tris should be simplified for repeated cover; atlas instances.

## 08. Hero machinery

CURRENT: CF articulated hub/reactor/turbine/containment; CP zone landmark structures; V3 collapsed hull.

PROBLEM: bespoke large silhouette and worn-metal surface complexity are below the concept's machinery focus.

RECOMMENDED SOURCE: existing custom machines with Molten rings/command props and MegaKit vents/pipe holders.

USE / DONOR / CUSTOM: CUSTOM hero modeling + selected DONOR details.

EXPECTED VISUAL GAIN: distinctive working-machine silhouettes and layered cold metal rather than many small generic props.

TECH RISK: custom Blender work remains; keep pivots/LOD and mobile material budget; source library does not provide finished concept hero machinery.

## 09. Scout

CURRENT: VS Scout_V1, current enemy catalog recipe/controller/anchors.

PROBLEM: light radial shell needs clearer thin supports, armor separation and a hostile energy source.

RECOMMENDED SOURCE: current Scout + Essentials QuadShell mechanisms/EyeDrone core donor.

USE / DONOR / CUSTOM: CUSTOM approved radial shell + DONOR articulation/core.

EXPECTED VISUAL GAIN: identifiable light radial enemy, independent from Cutter and player G-0.

TECH RISK: no stock bipedal substitution; reduce donor bone/tri count; preserve attack origin, footprint and timing.

## 10. Cutter

CURRENT: VS Cutter_V1 with current paired-blade role.

PROBLEM: front weapon mass and blade wind-up must remain unmistakable from above.

RECOMMENDED SOURCE: current Cutter; Quaternius mech joints/modular weapon support parts.

USE / DONOR / CUSTOM: CUSTOM blades/front armor + DONOR mounts.

EXPECTED VISUAL GAIN: heavier directional paired-blade silhouette and visible attack source.

TECH RISK: no available unchanged cutter; preserve blade sweep/telegraph footprint; geometry/rig authoring required.

## 11. Warden

CURRENT: CP Warden_V1 and current visual-integration recipe.

PROBLEM: defensive armor profile needs clearer width, weight and differentiation from ordinary chassis.

RECOMMENDED SOURCE: current Warden with Essentials Trilobite plates and selected QuadShell support mechanisms.

USE / DONOR / CUSTOM: CUSTOM armored shell + DONOR plates/joints.

EXPECTED VISUAL GAIN: broad heavy defense silhouette and role-specific core/weapon mass.

TECH RISK: stock Trilobite8338-triangle/20-bone body is not a finished mobile Warden; reshape, simplify and preserve controller anchors.

## 12. Arc Drone

CURRENT: CP ArcDrone_V1.

PROBLEM: angular hostile housing and arc emitter need stronger high-angle readability.

RECOMMENDED SOURCE: mechs-combat-drone Drone shell, current ArcDrone anchors; EyeDrone core as reserve.

USE / DONOR / CUSTOM: DONOR shell + CUSTOM core/emitter.

EXPECTED VISUAL GAIN: angular fin silhouette with a visible orange/red arc charge origin.

TECH RISK: donor1362 triangles/one material/no rig; custom pose/animation/attack anchors remain; private Asset Store source.

## 13. Carrier

CURRENT: CP Carrier_V1.

PROBLEM: cargo/machine-carrying role needs a recognizable compartment and heavy support stance.

RECOMMENDED SOURCE: current Carrier + Essentials Prop_Crate/Trilobite plate donors and Molten container detail.

USE / DONOR / CUSTOM: CUSTOM load structure + DONOR attachment plates.

EXPECTED VISUAL GAIN: distinct heavy industrial transport/load silhouette rather than a recolored generic enemy.

TECH RISK: custom load pivots and rig clearance; preserve spawn cap/performance and current enemy footprint.

## 14. Magnetar

CURRENT: VS Magnetar_V1, elite visual recipe.

PROBLEM: approved low broad multi-support elite and magnetic machinery detail are not covered by downloaded stock mechs.

RECOMMENDED SOURCE: existing Magnetar + custom Blender shell; MegaKit cables/vents and Molten generator rings as attachments.

USE / DONOR / CUSTOM: CUSTOM elite + DONOR mechanisms.

EXPECTED VISUAL GAIN: unique broad core/magnet silhouette, layered armor and visible elite charge/source.

TECH RISK: highest custom modeling/rig cost among elite work; maintain combat anchors and readable telegraphs; no unchanged free replacement exists.

## 15. Custodian

CURRENT: V3 Custodian_V3; boss visual binding/phase/socket contract.

PROBLEM: boss needs bespoke massive machinery and energy structure, beyond stock bipedal mech proportions.

RECOMMENDED SOURCE: current boss + custom Blender housings; Molten rings/MegaKit services as small donors.

USE / DONOR / CUSTOM: CUSTOM boss + DONOR supporting details.

EXPECTED VISUAL GAIN: a distinct industrial boss landmark and causal phase weapon presentation.

TECH RISK: retain phases, AttackOrigin/HitCenter/HealthAnchor/DeathOrigin and arena footprint; LOD/skinning budget; no G-0/Striker substitution.

## 16. M-0 weapon

CURRENT: V3 Emitter_M0, Chapter01 visual-integration weapon binding.

PROBLEM: starting emitter needs a readable mechanical source and cyan energy identity.

RECOMMENDED SOURCE: existing emitter + Quaternius barrel/body modules; tower-defence turret head/support as reserve donor.

USE / DONOR / CUSTOM: CUSTOM emitter housing + DONOR mechanical parts.

EXPECTED VISUAL GAIN: recognizable mech-mounted emitter with clear source → travel connection.

TECH RISK: preserve firing socket and existing equipment behavior; remove humanoid grip, merge material slots and check animation clearance.

## 17. Player weapon presentation

CURRENT: `Assets/_Game/Runtime/Presentation/Player/WeaponEquipmentPresenter.cs`; current G-0 equipment/socket presentation.

PROBLEM: progression needs distinct visible module silhouettes, not color-only upgrades.

RECOMMENDED SOURCE: Quaternius AR_1/AR_4/Sniper_1/Pistol_1 and modular parts assembled around existing weapon mounts.

USE / DONOR / CUSTOM: DONOR parts + CUSTOM progression assemblies; approved G-0 retained.

EXPECTED VISUAL GAIN: coherent readable barrel/coil/head variants at the actual portrait camera.

TECH RISK: custom sockets/pivots/LOD; preserve equipment mechanics, saves and data definitions; four-material examples require merging.

## 18. Attack source/travel/impact VFX

CURRENT: V3 HostileChargeV3/HostileTravelV3; P6 PFX_HostileImpact; Phase6BCombatProductionBridge/Phase6BVfxPool.

PROBLEM: source, travel and impact must remain connected and distinguishable amid denser machinery.

RECOMMENDED SOURCE: current pooled causal chain + selected Wallcoeur muzzle/circle-impact masks.

USE / DONOR / CUSTOM: CUSTOM existing effects + DONOR texture masks.

EXPECTED VISUAL GAIN: localized industrial muzzle/charge and distinct impact without overwhelming the travel cue.

TECH RISK: static audit only; reduce source3K images/caps/layers, retain timing and pooling; no source scripts/shaders imported.

## 19. Damage/hit VFX

CURRENT: Phase6B hit/impact cues and existing pooled feedback (`Phase6BVfxInstance`, `PooledPulseVfx`).

PROBLEM: small hit cues need sharper industrial spark/contact feedback, not cartoon blood or magical bursts.

RECOMMENDED SOURCE: Wallcoeur selected impact masks, Vefects fire/smoke flipbooks in current particle materials.

USE / DONOR / CUSTOM: DONOR textures + CUSTOM small pooled effects.

EXPECTED VISUAL GAIN: readable metal contact with brief orange sparks and controlled smoke.

TECH RISK: animation/overdraw unmeasured; no point lights/audio/colliders/rigidbodies from stock prefabs; preserve damage semantics.

## 20. Fog/steam/ambient particles

CURRENT: current scene ambient presentation and Phase6B baseline; no third-party fog family is integrated.

PROBLEM: machinery atmosphere needs localized depth/steam while keeping lanes and enemy cores visible.

RECOMMENDED SOURCE: Fog masks and Vefects smoke flipbooks, limited to connected service/vent points.

USE / DONOR / CUSTOM: DONOR masks + CUSTOM sparse pooled/limited emitter setup.

EXPECTED VISUAL GAIN: functional vent steam and layered perimeter haze instead of full-screen fog.

TECH RISK: caps1000 are unsuitable defaults; overdraw/depth ordering require runtime test. Snow/lava/Magic/Black Hole excluded.

## 21. HUD

CURRENT: UI HudUiFactory, SafeAreaHudRoot, PlayerHealthHudPresenter and PlayerStatsHudPresenter.

PROBLEM: broad plain rectangles need a restrained sci-fi frame hierarchy and better separation from the game field.

RECOMMENDED SOURCE: EXE normal button/divider frames; Kenney bars/glyphs as coherent secondary donors.

USE / DONOR / CUSTOM: USE EXE skins; DONOR utility shapes; CUSTOM portrait layout.

EXPECTED VISUAL GAIN: consistent thin frames, readable cyan highlights and less solid bright panel area.

TECH RISK: authored9-slice/atlas, black-art recolor and text contrast; retain safe area/accessibility, interaction and number formatting; ship CC BY attribution.

## 22. Inventory/equipment presentation

CURRENT: UI WeaponEquipmentPanel and Player/WeaponEquipmentPresenter.

PROBLEM: selected slots/equipment hierarchy need visual structure matching the HUD and weapon family.

RECOMMENDED SOURCE: EXE selected/unselected inventory frames plus custom equipment icons based on approved weapon silhouettes.

USE / DONOR / CUSTOM: USE frame assets; CUSTOM item icons/layout.

EXPECTED VISUAL GAIN: coherent selected state and recognizable mechanical item previews.

TECH RISK: icons are not supplied as a finished equipment system; atlas/slicing, fit to portrait and localization; no equipment-mechanics changes.

## 23. Boss HUD

CURRENT: UI BossHealthHudPresenter and current HUD frame creation.

PROBLEM: boss health must have a distinct restrained hierarchy without hiding boss telegraphs.

RECOMMENDED SOURCE: EXE frame/divider language, Kenney bar donor, custom boss badge.

USE / DONOR / CUSTOM: USE frame skin + DONOR bar + CUSTOM boss composition.

EXPECTED VISUAL GAIN: clear orange/red boss identity and stable health/status hierarchy.

TECH RISK: preserve boss anchor/phase/health semantics, safe-area width and text contrast; no stock complete boss HUD is included.

## 24. Minimap frame

CURRENT: MAP TacticalHudFrame, MapMinimapPresenter, MapUiFactory and Prefabs/MapMinimapPresentation.prefab.

PROBLEM: plain minimap frame is visually disconnected from the industrial HUD and can dominate the top corner.

RECOMMENDED SOURCE: EXE button/inventory edge shapes with restrained Kenney utility markers.

USE / DONOR / CUSTOM: USE frame skin + CUSTOM minimap composition.

EXPECTED VISUAL GAIN: compact coherent frame, quieter background and clearer existing map markers.

TECH RISK: presentation only; preserve projection, map topology, marker meanings and touch behavior; pixel slicing/contrast requires actual-camera UI check.

## Integration boundary

This matrix proposes visible presentation changes only. All gameplay proxies, balance, saves, map layout, camera, G-0, enemy-controller/phase contracts and build/render settings remain current. In V3, each adaptation must be demonstrated at the existing camera/scale before expansion. Asset Store donor files remain local/private even after modification; public source commits must contain owned/reproducible integration tooling and permitted artifacts only.
