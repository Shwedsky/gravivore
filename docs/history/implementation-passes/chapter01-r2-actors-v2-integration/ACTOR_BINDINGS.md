# Production actor and animation bindings

Status: FULL CHAPTER01 VISUAL CONVERGENCE CANDIDATE. Owner visual/device acceptance pending.

The actual Chapter01 production catalogs bind five ordinary recipes plus elite/boss presentation
overrides to `Assets/_Game/Content/ActorProductionV2/Prefabs/*_V2.prefab`. Each wrapper contains only
transforms, the imported Generic rig/Animator, three skinned LODs and LODGroup. No colliders, gameplay
scripts, cameras, review playback or forced-LOD components are present. Source model bytes and all
controller state/clip content are retained from the pinned Actor V2 commit.

- `scout-drone` → Scout_V2: Idle / Run / Attack / Hit / Death; bipedal blade arms.
- `cutter-unit` → Cutter_V2: Idle / Run / Cut / Hit / Death; low four-support cutting platform.
- `warden` → Warden_V2: Idle / Run / Attack / Hit / Death, with Block during authoritative attack hold.
  The two source shields remain separate arm-controlled bones/geometry; no torso/shield fusion.
- `arc-drone` → ArcDrone_V2: Hover / Run / Discharge / Hit / Death; source hover geometry retained.
- `carrier` → Carrier_V2: Idle / Bank / Attack / Hit / Death. Bank is made looping through the ModelImporter;
  cosmetic turning is limited to 150 degrees/second. The FBX bytes and gameplay movement speed are unchanged.
- Magnetar guard → Magnetar_V2: Idle / Run / Attack / Hit / Death, Charge on shockwave telegraph,
  Vent on authoritative resolution. Telegraph duration controls visual playback speed only.
- Custodian M-0 → Custodian_V2: Idle / Run / Attack / Hit / Death; Telegraph on windup,
  AttackLine / AttackCircle / AttackCone on the corresponding authoritative phase event.

`HostileMotion.asset` stores the presentation-only mapping, movement-cycle distance, recovery,
turn rate and clearance envelope. Movement animation speed follows actual displacement with a bounded
playback multiplier. Root motion is disabled. No animator event commits damage, rewards or progression.
New pooled lives rebind/reset the Animator and refresh attack event observers. Twenty preallocated
ordinary corpse presentations allow up to four simultaneous shutdown poses per family while authority
recycles immediately. Corpse pose retains the last visual position/rotation, including wall separation.

The imported source faces -Z in Unity. A 180-degree child rotation presents its front along gameplay +Z.
Wrapper scale remains one for all seven actors; gameplay controller dimensions are unchanged.

## Socket and effect mapping

The `Presentation Sockets` marker includes each complete imported rig. The existing socket resolver
scans its descendants. Added aliases are zero-offset children of authored socket/bone transforms,
so weapon motion drives the visible effects. Exact aliases are recorded in
`verification/socket-aliases.txt` and validated for all enum roles.

- Scout/Cutter/Warden AttackOrigin follows authored AttackOrigin_L; WeaponRight follows AttackOrigin_R.
- Arc keeps its authored ProjectileSource; Carrier/elite/boss projectile aliases follow AttackOrigin.
- HitCenter drives gravity lash destinations and hit VFX. A lethal release preserves that life’s
  charged hit-socket snapshot through immediate pool recycling.
- DamageNumberAnchor drives hit text; a pre-recycle damage snapshot keeps lethal text at the visual actor.
- HealthBarAnchor drives ordinary/elite plates; boss HUD continues to observe authoritative boss health.
- GroundContact and TelegraphOrigin remain authored sockets. Ground hazard geometry/damage authority
  remains in the existing encounter presenter and is unchanged.
- DeathVfxOrigin aliases authored HitCenter. Source explosion/shutdown cues sample it before recycling.
- Custodian line source follows BossDischarge_L, circle source Core, cone source BossDischarge_R.
- VisualCenter, Core and Sensor are authored descendants; no old actor transform is used as a hidden driver.

## Materials and LODs

All authored albedo, normal, metallic/smoothness, occlusion and energy textures remain assigned.
Runtime skins use the three source family materials: red for Scout/Cutter/Custodian, amber for
Warden/Carrier/Magnetar, blue for Arc Drone. The emission shader keyword is explicitly enabled.
Base tint is 1.08; localized emission gain is 2.4 red / 2.6 amber / 3.2 blue. These are material adjustments,
not flat Body/Accent/Dark substitutions. No world material, light or postprocess setting is changed.

Three decreasing source LOD meshes are retained, with screen-height thresholds .14 / .075 / .025.
This is a mobile candidate configuration; a device GPU/thermal measurement is still required.
Source triangle counts (LOD0 / LOD1 / LOD2): Scout 47,684 / 27,178 / 13,828;
Cutter 53,595 / 30,547 / 15,537; Warden 55,840 / 31,827 / 16,190;
Arc 20,934 / 11,932 / 6,070; Carrier 27,863 / 15,879 / 8,077;
Magnetar 69,074 / 39,372 / 20,030; Custodian 96,893 / 55,228 / 28,096.
