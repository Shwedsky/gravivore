# Phase 2 — Player feel & combat presentation

Status: **PLAYER FEEL & COMBAT PRESENTATION DEVICE REVIEW: PENDING**

Base: `origin/main` at `7ab1b129e4f12c88acbbe1b176954b112c26f8bc`.
Branch: `codex/player-feel-combat-presentation`. This is a new isolated main-based milestone, not a continuation of a stacked ART/S20 branch. No merge is authorized.

## G-0 refinement

The same three replaceable, whole-form prefab bindings remain. Original project-authored tapered carapace/pelvis meshes replace the boxy chassis; cylindrical forearm housings and a compact recessed sensor reinforce a low industrial combat machine. Existing CC0 PBR maps/materials are retained. No imported character, paid dependency, rig or gameplay component was added.

- Tier 0: exposed hip/knee/elbow mechanisms, narrow waist, cyan core and lighter arms.
- Tier 1: moving shoulder pauldrons and extra core containment.
- Tier 2: broader articulated shoulder armor, rear vector fins and additional containment.
- Shoulder upgrades are parented to their weapon pivots, so they follow firing/gait motion.
- Progression remains at the baseline 20/42 score thresholds.

## Locomotion and idle

One `MechMotionPresenter` on the authoritative player owns all cached visual joints. It reads horizontal root displacement in LateUpdate, not joystick intent; blocked movement therefore does not animate walking. Distance advances a deterministic alternating cycle. Hip swing follows movement expressed in the active form's coordinates, including strafing/backward motion while aiming. Knees fold during swing; feet counter-rotate and receive a ground-relative plant/lift adjustment. Arms counter-swing and the torso stabilizes with restrained bob.

Stopping resets to the plant stance. Idle adds a small deterministic actuator adjustment and cyan core pulse. Respawn/DEV position jumps above two metres per sample reset observed movement instead of producing a huge step. The model remains simple transform articulation: no Animator, root motion, full-body IK or retargeting. All balance/movement authority stays in the existing CharacterController/PlayerLocomotion.

Settings are authored in `S14_Presentation.asset`: 1.15 m cycle distance, 27 degree hip swing, 42 degree knee fold, 0.09 m maximum lift, 0.6 degree idle correction. Actual speed and device gait readability still need human review.

## Attack sequence and authority

`ITrackedGravityLashVfx` and its optional `IPrechargedGravityLashVfx` capability extend the existing presentation contract. GravityAttackController reads its existing remaining cooldown to start cosmetic preparation; cadence, range, target selection, damage request, combat activity and pull calculations are unchanged. Presentation has no damage API or animation-event authority.

1. Cosmetic pre-charge starts when the existing cooldown enters its final 150 ms. Frame sampling can shorten it; it never lengthens the gameplay interval.
2. At the existing authoritative attack tick, the controller invokes release. Recoil and the narrow beam are activated synchronously, sampled from the active core-face socket, before the existing damage request executes. HP, rewards and pull remain on that same tick.
3. Authoritative nonlethal hit/death events immediately produce their distinct tilt/burst and audio. The old 230 ms cosmetic hit/death queue is removed.
4. The beam lasts 80 ms; its decorative 120 ms ring/sliver tail follows at the impact snapshot/live point. This tail does not gate damage or hit/death feedback.

**First attack:** acquisition keeps the original immediate attack and uses direct release, with no pre-charge or additional gameplay delay. Reacquisition after an empty-target interval has the same behavior. A newly selected target during an existing cooldown receives only the remaining cosmetic charge time; a large frame that crosses the entire window also releases directly. Attack interval, damage and DPS configuration are unchanged.

**Cancellation:** loss of range, line of sight, target validity, target switch, transient reset or controller disable cancels the pending charge and resets the firing pose. Charge expiry can only hold the cosmetic charge: it cannot autonomously release a beam, impact or damage. The release always comes from the gameplay controller. A replaced ordinary `EnemyLifeId` cancels the old charge; released effects stop tracking that object and keep their prior position snapshot, never following its next pooled life.

The whole active form aims visually toward the selected target without rotating/moving the gameplay root. The socket follows the exposed core face after body motion. Live-target effects follow pull displacement. Lethal feedback uses the synchronous death-position snapshot; hit-tilt decay also checks life identity before touching a reused visual root.

Cue observers use a cached array rebuilt during subscription changes. An observer exception is logged and later observers still run. Gameplay guards begin-charge, cancel and release calls independently. A failed/missing presentation cannot prevent damage or require VFX completion. No locomotion, model, audio, pooling architecture or camera/map redesign is part of this timing correction.

## VFX

Required prototype events are present:
- player charge: pooled cyan ring/slivers at the core;
- release: pooled cyan line and recoil;
- enemy impact: pooled amber ring/slivers plus visual-only tilt;
- enemy death: stronger orange shutdown ring/fragments at the death snapshot;
- player hit: red/orange local strike cue and independent haptic;
- player death: separate larger red/orange shutdown pool at the pre-respawn position.

The ring/sliver mesh is original project geometry, 112 triangles, shared within each pool. No ParticleSystem package, bloom stack, per-enemy decorative Update or gameplay collider was added. Repeated effects reuse their prewarmed slots. Saturation reuses the oldest slot rather than allocating. Existing assimilation/evolution routing remains.

## Combat SFX provenance and processing

Eight runtime mono PCM cues derive from the selected Kenney files under `Assets/ThirdParty/Kenney/CombatAudio`:

- Step: Impact Sounds `impactMetal_light_000.ogg`, capped at 90 ms.
- Charge/servo: Sci-Fi Sounds `doorClose_001.ogg`, capped at 150 ms.
- Release: Sci-Fi Sounds `thrusterFire_000.ogg`, capped at 180 ms.
- Lash impact: Sci-Fi Sounds `impactMetal_000.ogg`, capped at 120 ms.
- Enemy hit: Impact Sounds `impactMetal_medium_000.ogg`, capped at 120 ms.
- Enemy death: Sci-Fi Sounds `explosionCrunch_000.ogg`, capped at 320 ms.
- Player hit: Impact Sounds `impactPlate_heavy_000.ogg`, capped at 180 ms.
- Player death: Impact Sounds `impactMetal_heavy_000.ogg`, capped at 400 ms.

Sources: [Kenney Sci-Fi Sounds 1.0](https://kenney.nl/assets/sci-fi-sounds), [Kenney Impact Sounds 1.0](https://kenney.nl/assets/impact-sounds).
Both official pages and both downloaded archive license files identify **CC0 1.0 Universal**, allowing commercial use and modification. Original license files are retained alongside the eight original OGG files; ThirdPartyNotices records the same sources.

Downloaded archive SHA256:
- Sci-Fi: `119340F351A5098AD814F78719438C0DA355A9CE8A4C8A3AF6A8D48AA3D49E04`
- Impact: `029D734AF1582474EDF3A694D1B0CEBC97C1C152F2F39FA34D4C2BAFC5DE77F8`

`PlayerFeelAssetConfigurator.Configure` reproducibly decodes the originals, trims initial silence, limits duration, converts to mono PCM, peak-normalizes to 0.55 and applies 3/18 ms attack/release fades. This is a first palette assembled from licensed sources, not a final bespoke sound library. Source OGGs are evidence/authoring inputs; only referenced derived WAVs ship. Source audio must not be replaced with ripped sounds.

The existing audio presenter/settings are reused. Four fixed sources provide three bounded combat voices plus one reserved quiet step voice; Play replaces that voice's clip rather than accumulating unlimited PlayOneShot voices. Steps use 0.22 gain and a 280 ms minimum interval; combat uses 0.65 gain multiplied by the saved volume. Mute stops all sources. Haptics keep their existing independent settings/throttle. Assimilation, evolution and encounter-specific cues are deferred and retain their previous placeholders. No music is implemented.

## Structural runtime performance snapshot

Captured in the canonical PlayMode scene; see [PERFORMANCE.json](player-feel/images/PERFORMANCE.json).

- Tier 0: 27 renderers, 44 material slots, 5376 triangles.
- Tier 1: 30 renderers, 49 material slots, 6360 triangles.
- Tier 2: 35 renderers, 58 material slots, 7760 triangles.
- Active particle systems: 0.
- Recurring VFX: four lash sequence slots with three objects each, plus 29 pulse objects = **41 prewarmed visual objects**. At most one phase object is active per lash slot.
- Snapshot has one active pulse object during the controlled Cutter impact.
- Audio sources: four fixed voices.
- Presentation ticking: one mech LateUpdate, one feedback Update (cached enemy visual roots), one lash Update, six pulse-pool Updates. No Update on individual limbs/fragments/audio sources.
- New steady-state presentation loops use cached arrays/transforms/property blocks and simple value math. No recurring Instantiate/Destroy path is added. This is code/structure verification, not an allocation profiler or device FPS measurement.

## Verification and evidence

Unity 6000.3.0f1 compile succeeded. Full EditMode **296/296** and full PlayMode **71/71** passed with zero skipped tests; ProjectValidator passed. The timing correction adds 15 deterministic controller cases for pre-charge before commit, unchanged commit frames/damage through stat changes and cosmetic durations, first/reacquired attacks, large frames, observer failures, target loss/switch and reset. Four added PlayMode tests check charge expiry without gameplay, disable/target-loss cancellation, pose reset, observer isolation and ordinary pooled-life reuse. Updated existing tests assert immediate beam/hit/death and regenerate attack captures through the real controller. Existing gait, save/progression/adaptive respawn/world/elite/boss regressions remain included.

Timing-correction logs/results are under `Builds/Logs/timing-*.log/xml`; APK identity and build outcome are recorded in the PR/completion report. The DEV build uses the existing Android pipeline, ARM64 IL2CPP and debug signing. Real Android install, movement/audio acceptance, frame timing and lifecycle recheck remain pending.

[Ten review captures](player-feel/images) use the settled S20 9:16 gameplay camera (offset 0,14.8,-11.2; FOV 46; look height 0.9), except the explicitly separate Tier 2 close view. Attack captures are regenerated through the authoritative controller: unchanged HP during pre-charge, then active beam and damage on the same tick, followed by the impact tail. They are actual Unity renders from controlled scene states, without HUD overlay, not concept art. Camera/map settings were not changed. WalkPose A/B are evidence of two poses, not proof that animation feels good over time.

Manual review: [PLAYER_FEEL_DEVICE_CHECKLIST.md](PLAYER_FEEL_DEVICE_CHECKLIST.md).

## Deferred / next phase

Final bespoke model and production rig; advanced IK/uneven-ground foot locking; final sound library and mix; music; remaining ordinary mobs; elite/boss art replacement; environment polish. Repair hub and repeatable elite/boss economy remain **Phase 3** prerequisites after Phase 2 device acceptance. No minimap/UI refactor or adaptive-respawn retuning is included here.

PLAYER FEEL & COMBAT PRESENTATION DEVICE REVIEW: PENDING
