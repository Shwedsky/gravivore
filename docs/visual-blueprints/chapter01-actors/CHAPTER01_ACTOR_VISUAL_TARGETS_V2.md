# Chapter 01 actor visual targets V2

Status: **CURRENT / OWNER-APPROVED ACTOR VISUAL AUTHORITY**
Normalized: 2026-10-10. Scope: G-0 and all seven Chapter 01 hostile roles.

Primary visual authority: [the exact V2 PNG](CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png). This text translates that image and the owner's locked corrections into production constraints. It does not replace the image or authorize new designs.

## 1. Exact image, precedence and scope

The PNG is the existing owner-local file, copied without any image transformation. SHA-256: `67960c1254133c48b1de97d2dd635a5969cea49b949a7e0e8981afd34738099f`. See [README](README.md) for custody and the missing-image stop rule. If this exact authority is missing, stop; text, donor screenshots and older boards are not substitutes.

For actor visual anatomy:

1. Newest explicit owner correction.
2. `CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png`.
3. This `CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md`.
4. `../../CURRENT_VISUAL_TARGET.md`.
5. `../../ART_DIRECTION.md`.
6. Donor audit / donor matrix, as manufacturing evidence only.
7. Historical documents, as evidence only; never restored requirements.

For Chapter world/layout, [Visual Blueprint V1](../chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md) remains authoritative. Actor V2 does not redefine world topology. This task changes documentation/reference custody only: no scene, gameplay, progression, map/minimap, collision, controller, existing prefab, FBX or production material edits. PR #71 and PR #72 remain independent.

## 2. Reading the approved board without inventing design

The board has eight labeled actor columns, large primary views, smaller alternate views, a gameplay/top-view row, a relative-scale strip and illustrative combat scenes. Read the same actor across those views. Small variants demonstrate articulation and view dependence; they do not authorize new enemy identities, skins, weapons or evolution tiers. The illustrative scenes demonstrate readability and lighting; the world blueprint still owns layout.

**Visible evidence:** G-0 has dark armor and cyan/blue player accents; Scout has a narrow pale-armored biped silhouette and two long luminous red blade arms; Cutter has a low dark/red multi-support cutting body; Warden has broad pale armor with orange accents and two long outboard shields; Arc Drone has a conspicuous blue contained core and spread appendages; Carrier has a dark elongated enclosed hull and integrated orange-lit lateral propulsion forms; Magnetar has a broad articulated heavy frame around an amber/orange core; Custodian has a larger armored reactor architecture with red energy and multiple major structures.

**Explicit owner corrections control ambiguous imagery:** Arc Drone is airborne; its downward appendages are not permission to create ground-contact walking legs or a walking gait. Warden's shields are separate equipment controlled by its arms/hands, even where overlap obscures the grip. Carrier is a vehicle, never a legged mech. Current G-0 stays bipedal. Preserve the PNG unchanged when documenting these corrections.

**Color exception visible in the PNG:** Arc Drone uses blue energy. Preserve that approved blue core/weapon language; the general hostile red/orange rule does not authorize recoloring it. Distinguish it from G-0 by airborne anatomy, energy housing and attack source. Column number badges (including purple Carrier) and the header palette are board graphics, not body paint or emission instructions.

The scale strip prints approximately G-0 1.8 m, Scout 1.2 m, Cutter 1.6 m, Warden 2.0 m, Arc Drone 1.5 m, Carrier 2.2 m, Magnetar 3.0 m and Custodian 4.0 m. These are concept annotations, not surveyed dimensions or collision/balance values. The strip does not specify a measurement axis; especially the low vehicle's 2.2 m label must not be imposed as hull height. Preserve the visible relative mass/proportion hierarchy. The proof brief states how to compare against G-0 while reporting the axis uncertainty.

Rear/underside topology, exact support counts where occluded, joint limits, complete orthographic dimensions, exact shader values and hidden mechanisms are not fully established by this board. Later production may solve necessary hidden construction within the approved silhouette and mechanics; it may not present speculative features as owner-approved design. Do not add visible new anatomy, weapons, colors or locomotion to fill a gap.

Board captions about reinforcement calls, support, magnetic attacks and boss phases communicate role/weapon causality. They do not authorize new runtime abilities, progression, controllers or balance in this documentation task.

## 3. Shared hostile faction production language

Use premium industrial hard-surface forms, with dark graphite exposed mechanics, cool/pale metallic armor, layered shells and visible joints, pistons, actuators and load paths. Follow each column's actual armor coverage and colors; Cutter, Carrier and Custodian must not become pale recolors of the biped body family. Localized red/orange energy is the shared default, with the approved blue Arc Drone exception. Magnetar's amber/orange core and Custodian's red reactor remain distinct focal points.

Faction unity comes from materials, mechanical vocabulary, energy containment, surface treatment and manufacturing detail. Role separation comes from silhouette, locomotion, mass, attack source, stance and proportions. Never flatten all roles into one chassis or differentiate them only by glow.

Primary masses and secondary mechanisms must carry the design at the actual portrait camera. Recesses, panel seams, bolts, vents, edges and authored PBR variation support that read. Use serious machine proportions, clear functional armor separation and restrained wear; no toys, cubes with attachments, generic marketplace identity, stock markings or excessive neon.

Local emission belongs to the visible core/emitter, blade edge, propulsion recess or selected warning detail. Do not make broad armor faces emissive, add luminous trims everywhere or use bloom to hide weak geometry. Match the image's luminous locations and apparent coverage before tuning exposure in the gameplay camera. No numeric emission percentage or new RGB palette is invented here.

Use LODs, shared materials/atlases, compressed textures, efficient URP shaders and bounded effects to reach Android performance without changing the aesthetic. Preserve actor identity with bloom/emission disabled and at lower quality. Old prototype triangle/material ceilings do not define the approved art.

## 4. Donor evidence and reuse boundaries

Manufacturing evidence was read from PR #72 / `origin/chore/actor-asset-intake-v1` at **`628f51d220a379fdaa14a60536b68275924976a9`**:

- [ACTOR_ASSET_INTAKE_AUDIT.md at the audited commit](https://github.com/Shwedsky/gravivore/blob/628f51d220a379fdaa14a60536b68275924976a9/docs/actor-production-v1/ACTOR_ASSET_INTAKE_AUDIT.md).
- [ACTOR_DONOR_MATRIX.md at the audited commit](https://github.com/Shwedsky/gravivore/blob/628f51d220a379fdaa14a60536b68275924976a9/docs/actor-production-v1/ACTOR_DONOR_MATRIX.md).
- [MODEL_INVENTORY.md at the audited commit](https://github.com/Shwedsky/gravivore/blob/628f51d220a379fdaa14a60536b68275924976a9/docs/actor-production-v1/MODEL_INVENTORY.md), with exact source members/hashes, bones, take names and measured modifier output.

These files are available on the independent intake branch and need not be merged or copied into this package. Re-read the measured evidence when selecting exact source bytes. A donor grade is utility, not visual acceptance; imported takes are not certified Unity gameplay clips. The measurements below are attributed audit findings, not measurements repeated by this task or mobile performance passes.

Raw acquisition stays under the owner-local `ExternalAssetIntake/Current/` root specified by [asset authority](../../ASSET_SOURCE_OF_TRUTH.md). Do not scan old worktrees/warehouse paths as current sources. The audit does not clear public raw redistribution for Unity Asset Store donors or for its conflicting Quaternius license notices. Keep restricted donor-derived editable work private and carry provenance through later promotion; check the exact item/license evidence before committing any geometry. No raw donor bytes are included here, and no new licensing conclusion or download is made by this task.

### 4.1 G-0 — retained player biped

**Approved anatomy:** separate dark torso/head-sensor mass, two articulated mechanical legs, arms and substantial visible equipment. Cyan/blue core/visor/weapon cues and player silhouette remain distinct from hostile machines. The board shows equipment views; do not treat them as permission to add unapproved weapons or replace the current player design.

**Likely donor / reusable components:** current project-owned biped and its existing rig/locomotion. PR #72 lists George/Stan/Striker as optional mechanical references only if a later task authorizes that scope. There is no donor replacement selection for G-0.

**Replacement / Blender requirements:** no G-0 modeling in this task or the proof trio. If later authorized, refine only geometry/material/equipment presentation needed to reproduce the approved biped; retain readable core and existing equipment integration.

**Rig / animation:** retain the current project rig and verified presentation contracts. Donor retargeting is not assumed or authorized here.

**Visual risks / rejection:** human armored-soldier read, indistinguishable hostile finish, tiny invisible equipment, stock replacement, radial/four-support/tank restoration or evolution expressed only through scale/glow. Reject any of these even if technically valid.

### 4.2 Scout — agile blade-arm biped

**Approved anatomy:** two mechanical legs, narrow waist/torso and light pale layered armor over graphite joints; small sensor/head mass; forward aggressive stance. Both arms end in long sword/blade/spike structures. Blade span and pointed attack direction remain obvious in the top/gameplay views. Red energy accents support the geometry. Scout is the fastest/lightest ground hostile, never a bulky soldier.

**Likely donor / reusable components:** George, from `ExternalAssetIntake/Current/quaternius-animated-mech-pack/`. Audit: 7,864 evaluated triangles, one material, 47 bones, 20 named takes; original Blender mesh has 61 disconnected islands. Reuse the leg/foot mechanics, suitable arm chains, root and selected internal body mechanics if they fit the PNG. Disconnected islands still require skin-weight inspection.

**Must replace / custom Blender:** stock hands/weapon interpretation with integrated long blade terminations; stock shell/proportions where they preserve George's rounded identity; donor texture/finish with the approved layered industrial surfaces. Author blade roots/actuation, narrow forward armor and contained core housing. Never shorten blades to decorative wrist fins to preserve donor animations.

**Rig / animation reuse:** preserve useful biped leg chains and verify arm reach after blade adaptation. George Run/Walk/Idle/Hit/Death are candidates only. Tune forward motion and foot contact; author blade sweep, anticipation, recoil/recovery and safe displaced/death poses. SwordSlash is a reference, not proof of compatible integrated-blade combat. Bake/export a suitable Generic rig and validate root motion ownership in later Unity work.

**Visual risks / rejection:** spider/quadruped/radial anatomy, hands carrying ordinary swords, tiny blades, heavy armored biped, toy-like donor face/body, or attack poses hiding one blade. Reject if silhouette cannot identify the light two-legged blade attacker before color.

### 4.3 Cutter — low paired cutting predator

**Approved anatomy:** non-humanoid low forward-biased body, heavier than Scout, with two conspicuous cutting assemblies and a stable articulated support arrangement. Dark/red housings, layered frontal armor and exposed drive/joint mechanics explain cutting force. The attack direction must be legible immediately.

**Likely donor / reusable components:** QuadShell from the audited Quaternius Sci-Fi Essentials source; 7,494 triangles, two meshes, 28 bones and eight takes. Use quadruped support chains, body pivots and Gun.L/R mount mechanics where they can support the board. Audited paired turret/gun mount parts are component donors only.

**Must replace / custom Blender:** guns with the actual paired blade/cutting forms, low aggressive shell and industrial drives. No new firearm, tail weapon or unshown tool is authorized. Rebuild armor/containment and PBR finish rather than recoloring QuadShell. Quadruped/scorpion/spider-derived mechanics are acceptable underneath the approved design; they do not select an alternative silhouette.

**Rig / animation reuse:** support gait/idle can be adapted after ground clearance and tool reach are checked. Author independent cutter actuation, anticipation, sweep/lunge presentation, recovery and destruction around existing gameplay events. A gun take is not a finished cutting attack.

**Visual risks / rejection:** humanoid Scout with armor, ordinary spider with small decorative cutters, forward tools hidden by shell, unchanged gun mounts as weapons or stock identity. Reject if the paired cutting threat is not visible at the portrait camera.

### 4.4 Warden — heavy biped carrying two shields

**Approved anatomy:** broad armored torso, heavy two-legged stance and two functional mechanical arms. Two separate elongated armored shields project outboard from hands/forearms; their lower reach, independent orientation and space from the shoulders make the carried-equipment read clear. Pale/cool armor, dark mechanisms and localized orange details match the board.

**Likely donor / reusable components:** Striker from `ExternalAssetIntake/Current/msgdi-medium-mech-striker/`. Audit: 6,472 triangles, three meshes, 17 bones. Reuse suitable load-bearing leg/torso mechanics and arm chains. Separate hand meshes are 999 triangles each but have no wrist/finger rig; those ASCII FBXs were parsed, not successfully imported by Blender, and require trusted private conversion.

**Must replace / custom Blender:** author both separate shields, physical grips and controlled attachment geometry; reconstruct hand/wrist or rigid mechanical grip as necessary. Rework stock torso/armor/markings/materials to the PNG. Stock shoulder plates are not shields. Leave discernible separation between shield, shoulder and torso through defensive poses.

**Rig / animation reuse:** donor bones stop at LowerArm_L/R. Create forearm-to-grip/shield transforms or needed wrists; the grip must remain mechanically clear without requiring human fingers. Inspect locomotion/hit/death takes; the 102 imported action records include duplicates, not 102 certified clips. Author shield raise, brace, independent arm control and recovery; any strike/break presentation must use an already supported event, not introduce a new gameplay system.

**Visual risks / rejection:** giant pauldrons, fixed torso wings, shields rigid to torso/shoulders, arms that cannot plausibly control shields, shields swallowed by armor overlap, or a narrow Scout enlarged. At the real camera the viewer must understand: **this robot is carrying two shields**.

### 4.5 Arc Drone — airborne contained energy machine

**Approved anatomy:** floating/flying central energy housing with spread side structures and visible downward mechanical appendages as depicted. Strong central blue contained energy and blue weapon causality are visible in the approved PNG. Its top view must remain distinct from a ground biped or Cutter. No humanoid walking legs, support-foot gait or ground-mech conversion.

**Likely donor / reusable components:** EyeDrone from the audited Sci-Fi Essentials source: 3,530 triangles, 14 bones, six takes. Use aerial articulation/pivots. Seed Hunter Keydrone is a secondary static shell (4,076 triangles, four 4K maps); VoodooPlay drone is another static shell (1,362 triangles). They have no ready flight rig/clips. Audited Quaternius parts are mechanics, not alternate actor concepts.

**Must replace / custom Blender:** author the board's central emitter/containment, shell, appendage and mechanical connections as necessary; rebuild toy/simple donor surfaces and excessive texture/material cost. Keep the airborne read and approved energy color. A bare glowing orb with attachments cannot substitute for containment hardware.

**Rig / animation reuse:** adapt EyeDrone aerial movement where its articulation fits; deliberate custom skinning is required for a different shell. Author floating idle, directional aim, discharge/recovery and destruction. Maintain flight clearance through movement/displacement; do not retarget biped walking clips.

**Visual risks / rejection:** tripod interpreted as walking ground legs, humanoid leg additions, ground contact as locomotion, blue recolored to red by a generic faction rule, invisible emitter, or ambiguous player silhouette. Reject if flight and energy source cannot be understood at gameplay scale.

### 4.6 Carrier — streamlined enclosed assault vehicle

**Approved anatomy:** long low closed hull with a pointed/wedge forward mass, integrated lateral/underside propulsion forms, enclosed upper/rear structure and dark layered armor. Orange energy is localized to propulsion recesses and selected functional details. It reads as an aggressive fast industrial/assault sci-fi vehicle or compact hovercraft/spacecraft, not friendly logistics equipment.

**Likely donor / reusable components:** UnityFan Vehicle 012 from `ExternalAssetIntake/Current/unityfan-scifi-vehicle-012/`, primarily its editable low wedge/hover shell and selected chassis forms. Its exact audited member is `source/sci-fi_vehicle_013_2.blend` despite the archive's Vehicle 012 name; SHA-256 `c3b6164b80876271a2569f41e2661c7fc030effc4b6d8558819dd40dee9393c6`. Audit: **9,248 evaluated triangles**, seven meshes, five materials; the 485-triangle control cage is not runtime cost. 012-01/011-02 are alternate component sources; 022 is a costlier 36,916-triangle secondary source. RTS Vehicle_v1 may provide selected chassis details only.

**Must replace / custom Blender:** refit nose, enclosed shell, upper/rear mass and integrated propulsion to the board; remove supplier identity/markings and author coherent UV/PBR surfaces. Author necessary concealed propulsion mechanics and compatible moving pivots. Any loading/attack opening mentioned in the audit must remain within approved hull forms and existing gameplay causality; donor doors or guns are not visual mandates. Do not add a turret, visible cargo box, wheels or walking legs merely because a donor has them. Follow the PNG's hover-like integrated propulsion read.

**Rig / animation reuse:** all four audited UnityFan vehicles have **zero rigs and zero clips**. Author vehicle root and functional pivots plus idle, turn/bank/drift, supported attack response, hit/death presentation. Separate meshes do not prove useful pivots. Locomotion remains vehicle motion; root travel/speed belongs to current gameplay configuration, not an invented fast-vehicle balance change.

**Visual risks / rejection:** high box roof, cargo crate on wheels, ice cooler/cart, friendly delivery robot, ordinary humanoid, hull on mech legs, uncontrolled material/mesh proliferation or unmodified donor spacecraft. Reject if its silhouette fails the approved low elongated aggressive vehicle read.

### 4.7 Magnetar — custom heavy magnetic elite

**Approved anatomy:** much larger/heavier than ordinary units; broad stable multi-support architecture around a contained amber/orange magnetic core. Major joints, elongated substantial supports, connected mechanisms and layered armor carry elite authority before emission. Match the board's containment cage, support distribution and broad stance rather than extrapolating an ordinary spider.

**Likely donor / reusable components:** project-authored architecture. QuadShell support mechanics, Trilobite joints and audited turret pivot modules may supply selected leg/joint/pivot mechanics only. No complete actor donor is selected.

**Must replace / custom Blender:** substantial newly authored central containment, structural/load-bearing members, reinforced support interfaces, armor and connected energy routing. A donor support is a mechanical starting point; strengthen/refit it to the approved architecture. No simple shell/core recolor or ordinary body scaled up.

**Rig / animation reuse:** new multi-support rig and articulated containment/weapon interfaces. Donor gait/pivot tracks are reference only until reworked for weight and stance. Author supported charge/vent/attack/stagger/destruction presentations with visible core-to-mechanism causality and readable telegraphs.

**Visual risks / rejection:** enlarged Cutter/QuadShell, simple spider with glowing ball, floating lamp core, toy robot or glow as sole elite cue. Reject without substantial custom heavy architecture and a clearly superior silhouette before color.

### 4.8 Custodian — custom governing reactor boss

**Approved anatomy:** largest hostile and arena-defining machine, with central contained reactor/core, heavy surrounding armor and multiple major articulated mechanical structures. Preserve the strong central red reactor architecture and boss-grade distribution of mass/weapon mechanisms in the PNG. It must read as the industrial governing machine of the complex.

**Likely donor / reusable components:** project-authored architecture. Audited Tower Defence / WarZone / RTS parts may inform selected pivots, joints or mechanism construction; existing containment forms are component references only. No full boss donor exists in the audited selection.

**Must replace / custom Blender:** substantial custom reactor enclosure, large articulated structures, armor, structural interfaces and visible weapon/energy routing. Do not attach generic turrets or world reactors as a complete boss. Do not create a simple radial ring/spoke proxy because it is cheap to rig.

**Rig / animation reuse:** new large articulated rig with explicit pivots and independently readable reactor/attack motions. Author presentation for the existing boss attack/phase contract, telegraph anticipation, recovery and destruction. Audited static pivot parts supply no complete phase animation; no new boss mechanics are introduced here.

**Visual risks / rejection:** ordinary humanoid at 200% scale, enlarged Magnetar, stock boss with recolor, simple radial machine, invisible weapon cause or large VFX hiding ordinary anatomy. Reject if major geometry, reactor containment and articulated mechanisms do not establish boss authority before glow.

## 5. Visual acceptance and later production evidence

Later production is judged against, in order of design/evidence use:

1. The exact owner-approved V2 PNG, subject to newest explicit owner corrections.
2. This V2 textual contract.
3. The actual production portrait gameplay camera.
4. The final playable Android device result.

The camera and device verify that the approved anatomy survives implementation; they do not authorize redesign. A technically excellent asset still fails if it does not resemble the approved concept. Donor screenshots, Blender beauty renders alone, old Phase 3 actors, old radial Scout text and marketplace screenshots are not acceptance targets.

Required later evidence includes matched concept/gameplay views with G-0 for scale, normal and emission-disabled silhouettes, motion at gameplay size, leg/foot or airborne clearance, shield grip/arm-control proof, actual attack source-to-impact presentation, LOD transitions and measured Android performance. Diagnose missing anatomy before tuning glow or reducing all geometry to meet an inherited prototype ceiling.

The first production task is [Scout / Warden / Carrier production proof](../../actor-production-v1/SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF_BRIEF.md). Its contract covers the three distinct manufacturing paths. This authority task produces no finished actor or device visual approval.
