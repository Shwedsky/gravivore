# Scout / Warden / Carrier production proof brief

Status: **CURRENT / NEXT ACTOR PRODUCTION TASK CONTRACT**
Contract identifier: **SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF**. Authored 2026-10-10; no model production was performed by the authority task.

## 1. Outcome, authority and boundaries

The next production task must prove three different manufacturing paths against the owner's approved [Actor V2 PNG](../visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png) and [V2 anatomy contract](../visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md): Scout's agile blade-arm biped, Warden's heavy biped carrying shields, and Carrier's enclosed streamlined vehicle. Success means all three look like their approved columns in the actual portrait camera, rather than three donor models with a shared recolor.

Inspect the exact PNG before production and verify SHA-256 `67960c1254133c48b1de97d2dd635a5969cea49b949a7e0e8981afd34738099f`. Missing or unexpectedly changed visual authority is a stop condition. Do not generate a substitute or use old enemy boards. Newest explicit owner corrections take precedence over the PNG; V2 text follows it, then CURRENT visual/art direction, then donor evidence. Historical text is evidence only.

The authority task only writes this contract. A later explicit implementation task applies it. G-0 is a retained reference, not a replacement-model scope. Cutter/Arc Drone/Magnetar/Custodian production belongs to later tasks. Keep world ownership separate: [Visual Blueprint V1](../visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md) owns world/layout; PR #71 owns its rebuild; PR #72 owns donor intake.

Proof work should use isolated presentation/test artifacts authorized by that later task. This brief does not authorize editing Chapter01 scene/world topology, runtime controllers, progression, map/minimap, collision authority or existing production actor bindings. Verify compatibility before any separately authorized promotion. New features such as reinforcement spawning, shield gameplay, vehicle speed changes or extra weapons are outside the proof contract.

## 2. Common production and evidence rules

Use the PR #72 donor audit and matrix at pinned commit `628f51d220a379fdaa14a60536b68275924976a9`, linked from the V2 contract. Start from exact measured archive members/hashes under the owner-local `ExternalAssetIntake/Current/` root. Do not copy or reinterpret old warehouse/worktree locations as canonical sources. Audit utility is not approval of final art or tested Unity clip semantics.

Record exactly what survives from each donor and what was authored/replaced. Keep restricted donor-derived editable work in the owner-private workflow; the audit does not clear Unity source redistribution or its conflicting Quaternius notices for a public repository. Preserve exact provenance/license evidence and derivative handling before promotion. No new paid dependency, package or acquisition is required by this contract.

Author layered PBR hard-surface geometry with clear metal/graphite/recess separation, controlled wear, visible mechanisms and board-matched armor coverage. Consolidate textures/materials while preserving normal relief, occlusion, metallic/smoothness variation and localized emission. Donor base-color maps or supplier shader defaults are not finished GRAVIVORE materials.

Use existing production camera settings, lighting/exposure and G-0 presentation as references. Do not choose a favorable beauty-render camera, shrink G-0 or enlarge the candidate in a comparison to hide a scale mismatch. Provide idle, locomotion, attack anticipation/contact/recovery, hit, displaced and death/despawn views only to the extent supported by current presentation events. Presentation must follow gameplay timing rather than take ownership of damage, root travel, targeting, shield mitigation or rewards.

### Scale interpretation

The board labels G-0 approximately 1.8 m, Scout 1.2 m, Warden 2.0 m and Carrier 2.2 m. Scalar label comparisons to G-0 are approximately 0.67, 1.11 and 1.22 respectively; these ratios do **not** establish a universal height ratio. The concept does not identify the measurement axis. Use G-0's actual approved presentation height as a stable scene reference and preserve the drawn proportions/massing. Report the chosen model bounds in height, width and length plus the axis assumption. In particular, do not make the low Carrier 1.22 times G-0's standing height.

This is a normalization of approximate concept scale, not a balance or collider resize instruction. Hidden dimensions are construction choices constrained by the PNG's silhouette, real-camera read and existing gameplay envelope. Do not claim exact owner-approved metric dimensions that the image does not supply.

### Runtime compatibility and performance

Use a documented export axis/unit convention compatible with current Unity import. Verify frozen/evaluated modifier output, pivots, normals, skin weights, bounds, root-motion ownership and material channels. Bake DCC controls to a suitable Generic/rigid export; avoid importing unused controls/duplicate actions. Existing serialized dependency injection and gameplay/presentation separation remain mandatory.

Plan LOD0 for the actual near gameplay view, LOD1 for ordinary gameplay distance and LOD2/culling for smaller/offscreen views when profiling warrants it. Choose transitions from real screen coverage, not invented permanent distance/triangle limits. Bake tertiary detail into maps first; preserve the defining blades, shield outlines/gaps and vehicle length/propulsion silhouette at every visible LOD. Show normal, lower-quality and LOD-transition captures; reject identity loss or weapon/shield popping.

Measure evaluated triangles, skinning bones, renderer/submesh/material passes, texture memory, transparent overdraw, shadow and animation cost after adaptation. Donor counts are starting measurements, not budgets or performance passes. Target 60 FPS on a reasonable mid-range Android device at the configured live-enemy cap (default 25); the later proof should report tested hardware, population, quality, frame time and sustained behavior. Pool repeated VFX and avoid steady-state managed allocations, full-screen glow, uncontrolled realtime lights or one renderer per small bolt. Any performance-driven simplification must still meet concept fidelity.

No numeric emission-area percentage is invented. Emission must stay in the board's functional locations, preserve readable material/form at normal exposure and leave a recognizable role silhouette with emission/bloom disabled. Quality scaling must reduce effects/detail without removing actor identity.

## 3. Scout production contract

**Silhouette and approximate scale:** the smallest/lightest ground hostile in the proof, visibly lighter than G-0 and Warden. Use the board's approximately 1.2 m label versus G-0's 1.8 m as an initial concept comparison (about 0.67 scalar ratio, axis unconfirmed), then match the visible standing/attack proportions. Narrow torso/waist, small sensor mass, articulated two-legged stance and two long outward/downward blade arms remain obvious from above. Forward inclination communicates agility.

**Locomotion:** bipedal mechanical run/step with only two load-bearing legs; fastest/lightest ground-hostile visual language. Tune stride, foot placement and body lean to current movement configuration. No spider gait, four supports, radial chassis or new movement-speed balance.

**Major masses:** small head/sensor, narrow forward torso/core enclosure, light shoulder/hip armor, visible graphite leg/arm joints and two blade assemblies. Keep useful gaps around arms/legs; do not bulk up the body to accommodate George unchanged.

**Weapon/attack source:** both arms terminate in long integrated blades/spikes. Blade roots, actuation and contact path must make melee causality clear. Ordinary hands holding swords and tiny wrist blades fail. Do not introduce ranged guns or unapproved tool types.

**Required moving parts:** root/body lean, hips/knees/feet, shoulders/elbows and blade-root actuation as construction requires. Blades follow mechanical arm motion through anticipation, sweep/contact and recovery; they must not look like floating props or pass through the torso in normal poses. Motion must preserve two-legged support and readable forward threat.

**Donor boundaries:** George's measured original `Animated Mech Pack - March 2021/Textured/Blends/George.blend` is the primary editable donor; SHA-256 `55e24fbae28736f87684129d2f3e27e0e3ba246cc8e91668822d0b3faf155b0c`. The audit reports 7,864 evaluated triangles, 47 bones and 20 named takes. Reuse suitable root, leg/foot and arm mechanics; preserve skin-weight correctness when separating the 61 geometry islands. Original arms/hands, rounded stock shell and texture identity cannot dictate the result. Leela lacks arm bones and is not a drop-in substitute; heavier Mike/Stan bodies cannot override the approved light silhouette.

**Custom geometry and rig:** author blade terminations with load-bearing roots, forward shell, narrow armor layers and contained core. Retain/tune useful biped chains and add pivots only where needed for those approved mechanisms. Inspect Run/Walk/Idle/Hit/Death as adaptation candidates; SwordSlash is a motion reference, not certified integrated-blade combat. Author matched attack/recovery and displaced/death poses. Bake an economical rig and test feet/root behavior in Unity before promotion.

**Materials and emissive limits:** pale/cool metallic armor, graphite exposed mechanics, localized red core/sensors and blade-edge energy where depicted. Preserve metallic blade structure with emission off. No all-red body paint inferred from the column badge, uniform plastic finish or broad glowing armor.

**Gameplay-camera readability:** inspect all four headings at production screen scale, moving and grouped. Both blade arms, the two legs and forward stance must read without close zoom. In silhouette, Scout must remain lighter and more pointed than Warden and unlike low Cutter/vehicle bodies.

**Collider-envelope considerations:** measure idle and attack render bounds against the existing gameplay collision/target envelope. The long blade sweep is presentation reach; it is not an instruction to extend collision, targeting or damage range. Keep core/body alignment, grounded feet and visual wall clearance coherent with current authority. Record mismatches for an authorized integration decision; do not add physics colliders per blade/piston or alter existing collision here.

**Attachments/sockets:** identify left/right blade-root transforms and left/right blade-contact or tip VFX origins, central target/core anchor and hit/death effect anchors if supported. Proposed names are presentation semantics; map them to existing contracts rather than assuming new runtime APIs. Blades are integrated arm equipment, not inventory swords attached to hands.

**LOD and mobile concerns:** preserve both long blades, pointed stance, leg separation and torso/core silhouette across visible LODs. Bake small fasteners/recesses before simplifying silhouette. Audit modified skinning/bone counts, group animation CPU, material passes and blade-trail overdraw with several Scouts active; avoid many child renderers and constant emissive lights.

**Reject:** any radial/spider/quadruped anatomy, held conventional swords, decorative small blades, bulky armored soldier, unchanged George identity or camera poses that merge both arms into one unreadable spike.

## 4. Warden production contract

**Silhouette and approximate scale:** broad heavy defensive biped, concept label approximately 2.0 m versus G-0's 1.8 m (about 1.11 scalar comparison, axis unconfirmed). Its greater width, armor coverage, two long shields and planted stance establish mass; simple scale is insufficient. The shield/body separation must survive the top-down view.

**Locomotion:** slow-looking deliberate mechanical biped motion with a tank-like defensive stance. It remains two-legged; shield equipment does not become fixed support legs. Match current movement timing and avoid human soldier choreography.

**Major masses:** broad armored torso, small integrated head/sensor, heavy hips/legs/feet, two functional arm chains and two independent elongated shield plates. Armor shoulders remain distinguishable from carried equipment. Preserve the board's shield lower reach and outboard silhouette.

**Weapon/attack source:** defense is carried through the arms/hands into separate shields, with visible physical grips or mechanical hand/forearm control. Any existing attack/hit response must visibly originate from an approved moving mechanism. This brief does not add shield-bash damage, break mechanics, mitigation logic or extra weapons.

**Required moving parts:** two leg chains, torso/head as appropriate, both shoulders/elbows, hand/grip-to-shield control and independent shield orientation. Demonstrate raise/brace/recovery and locomotion while grip continuity remains believable. A shield may move with a forearm assembly, but its equipment mass must stay separate from the shoulder/torso. Do not invent human finger animation solely to satisfy the grip.

**Donor boundaries:** MSGDI Striker's measured `Assets/MediumMechStriker/FBX/MediumMechStriker/MediumMechStriker.fbx` is the primary mechanical donor; SHA-256 `b0cea6976225126369264bb4ace54f52b4d3f46f2ebaec9b997703ccafc17e91`. Audit: 6,472 triangles, three meshes and 17 bones ending at LowerArm_L/R, with no wrist/finger bones and no shields. Separate hand meshes are 999 triangles each; Blender rejected those ASCII sources, so successful conversion must be demonstrated privately before reuse. Imported 102 actions contain duplicates; select actual locomotion/hit/death tracks after inspection rather than assuming 102 useful clips.

**Custom geometry and rig:** author both shields, grips/brackets and any needed wrists; refit torso/armor/surface styling to the PNG. Reuse suitable donor leg/arm mechanisms, adding explicit grip/shield transforms or joints. Stock pauldrons cannot become shields by scale or color. Author controlled defensive poses and verify hands/forearms remain visible enough to explain equipment control.

**Materials and emissive limits:** layered pale/cool armor over dark graphite actuators, board-matched restrained orange functional accents and contained core details. Shield broad faces remain opaque armor with PBR surface depth. No luminous force-field shield substitute, excessive edge neon or new color scheme.

**Gameplay-camera readability:** show left/right side, front/back and top gameplay headings, including a brace frame and walking frame. At normal portrait scale a viewer must identify two shields carried by two arms. Include emission-disabled frames and an explanatory close view of the grip as supporting evidence; the close view cannot excuse an unreadable gameplay view.

**Collider-envelope considerations:** measure the wide carried shields and their motion separately from the authoritative body collider. Shields must not falsely promise new blocking volumes or let the body seem to pass through walls in routine movement. Keep targeting/core and feet aligned to existing authority. Record any envelope mismatch for later permitted integration; do not create authoritative shield colliders, change nav width or transfer damage/mitigation into art.

**Attachments/sockets:** left/right grip and separate shield attachment/pivot transforms, core/target anchor and supported impact origins. Attachment hierarchy must follow arms/hands rather than shoulders/torso. Retain grip continuity during raise/brace/hit. Any break/death detachment is presentation only and must use supported lifecycle events/pooling.

**LOD and mobile concerns:** preserve shield outline, equipment/body gap, forearm control read, broad torso and load-bearing feet at all visible LODs. Bake small shield fasteners/bevel detail; do not merge shields into pauldrons to reduce draw calls. Consolidate armor/shield materials where viable, inspect added hands/joints cost and avoid per-shield realtime lights, transparent broad shields or unnecessary physics animation.

**Reject:** shoulder armor mistaken for shields, giant pauldrons, fixed torso wings, grips with no load path, arms too hidden to establish control, shields fused to shoulders or a stock Striker recolor.

## 5. Carrier production contract

**Silhouette and approximate scale:** low elongated streamlined vehicle with concept label approximately 2.2 m versus G-0's 1.8 m (about 1.22 scalar comparison). The annotation's axis is unstated; preserve the board's low hull and long horizontal mass instead of imposing 2.2 m standing height. Record hull length/width/height and the provisional long-axis interpretation. Nose, lateral propulsion forms and enclosed upper/rear body create an aggressive compact assault-vehicle read.

**Locomotion:** vehicle/hovercraft motion with integrated propulsion, continuous root travel and controlled turn/bank/drift presentation. The PNG's underside/lateral glow supports a hover-like read; this proof follows it without adding walking legs or an exposed wheeled cart. Fast/aggressive visual language does not change configured gameplay speed or pathing.

**Major masses:** pointed armored nose, long low enclosed main hull, contained upper/rear assault/support housing and integrated lateral/underside propulsion structures. The upper mass must not become a tall box or exposed cargo rack. Do not bolt a vehicle body onto a biped rig.

**Weapon/attack source:** preserve the concept's assault/support identity and visible functional housings. Reinforcement/support captions are role cues, not authorization to implement new spawns or add a turret. Map existing attack presentation to board-visible mechanisms only; if the current event has no clearly documented visual mechanism, report that integration gap rather than inventing a gun. Loading/attack openings from the donor audit are construction candidates, not mandated new visible features.

**Required moving parts:** vehicle root and controlled body bank/recovery, integrated propulsion presentation and any board-compatible functional pivot needed by existing events. Do not animate a wheel/door/turret merely because a donor mesh contains it. Propulsion attachment continuity, idle height/clearance and hit/death settling must keep the vehicle silhouette coherent.

**Donor boundaries:** UnityFan Vehicle 012 under `ExternalAssetIntake/Current/unityfan-scifi-vehicle-012/` is the primary editable shell donor. The archive is `free-sci-fi-vehicle-012-public-domain-cc0.zip`, but its exact measured member is `source/sci-fi_vehicle_013_2.blend`; SHA-256 `c3b6164b80876271a2569f41e2661c7fc030effc4b6d8558819dd40dee9393c6`. Preserve this naming distinction and verify against pinned PR #72 MODEL_INVENTORY/MODEL_METRICS before editing. Audit: 9,248 evaluated triangles, seven meshes and five materials; 485 control-cage triangles are not exported runtime cost. All audited UnityFan vehicle candidates have zero rigs and zero clips. 012-01/011-02/022 or RTS Vehicle_v1 may provide selected chassis mechanisms only; their silhouettes, extra objects or material count do not determine the approved Carrier. Object separation does not establish usable hinges/pivots.

**Custom geometry and rig:** refit low wedge, enclosed upper/rear structure, armor and integrated propulsion to the PNG; remove supplier logos/identity where present. Author UV/PBR atlas, functional roots/pivots and the required vehicle presentation clips. Freeze/review evaluated modifiers in a deterministic export copy. Concealed mechanical solutions are allowed only within the approved exterior; no tall cargo box, friendly cart fittings, new turret or mech legs.

**Materials and emissive limits:** board-matched dark graphite/metallic layered armor, selective cool metallic exposed surfaces and localized orange propulsion/functional details. Purple column badge is not a purple actor palette. No all-orange glowing underside plane, broad transparent shell or neon trim replacing armor form.

**Gameplay-camera readability:** show side/oblique diagnosis plus all production gameplay headings, moving, turning and beside G-0. From above, the long enclosed hull, front/rear distinction and integrated propulsion must identify a vehicle immediately. Its low profile remains clear with bloom disabled; it cannot read as a box on a glowing disc.

**Collider-envelope considerations:** evaluate low hull clearance, length/width and turn sweep against existing authoritative collision/target bounds. Visual propulsion extensions are not extra collision or damage volumes. Do not resize gameplay colliders or world lanes here. Capture wall/ground clearance and state any mismatch so an authorized integration task can resolve it without changing world topology in this proof.

**Attachments/sockets:** root/forward reference, hull target/core anchor, left/right integrated propulsion effect origins, supported hit/death anchors and only confirmed functional pivots. Socket naming is a handoff mapping to existing runtime contracts, not new API design. No humanoid hand/foot sockets or inventory sword points.

**LOD and mobile concerns:** preserve wedge nose, low enclosed roof, long hull, lateral propulsion masses and front/rear readability. Use evaluated geometry after modifiers; consolidate five donor materials and seven meshes where compatible with functional movement, without destroying material depth. Audit low-angle underside glow overdraw, shadow footprint, bounds/culling and vehicle animation cost. No continuous expensive particle jet or per-propulsion shadowed light is assumed.

**Reject:** walking mech, hull on legs, humanoid silhouette, ice cooler, friendly delivery robot, cargo box on wheels, tall roof/cargo stack, stock vehicle with recolor or unapproved weapon appendages.

## 6. Required proof handoff and acceptance

For each actor the later producer must deliver:

1. Provenance/source-member/hash record and a list of donor parts retained versus replaced, plus public/private source handling.
2. Editable production source in its legally permitted location, deterministic export recipe and measured final bounds/geometry/material/texture/rig costs. Do not publish restricted source to satisfy this handoff.
3. Matched concept and actual-camera comparisons beside the retained G-0; no replacement/generated concept board. Include normal and emission-disabled silhouettes, each heading and defining motion poses.
4. A short real-camera motion capture proving feet/blades, arm-held shields or low vehicle propulsion. Close diagnostic views may supplement it.
5. Existing controller/socket/collision-envelope compatibility mapping, LOD and lower-quality captures, and explicit unresolved integration gaps.
6. Unity compile, relevant deterministic/wiring/pooling tests and project validation for any later asset integration; report executed commands/results and anything not run. Build the dev Android APK when Unity CLI is available for that implementation, then provide APK path and device performance/readability evidence. No automated pass substitutes for the human device visual gate.

Review against the exact approved PNG, V2 textual contract, real production camera and final Android device result. Technical excellence, low cost, donor grades or a convincing beauty render cannot offset the wrong anatomy. Reject and correct geometry/poses before claiming production approval when the Scout lacks blade arms, Warden lacks carried shields or Carrier reads as a legged/boxy robot.

The authority package is complete when the reference and contracts are preserved and discoverable; it does not imply that these future assets, runtime integration, performance or owner device acceptance have been produced or passed. The next task is **SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF**, with Scout first, Warden second and Carrier third; the order tests rig adaptation, equipment control and newly authored vehicle motion without starting all seven actors at once.
