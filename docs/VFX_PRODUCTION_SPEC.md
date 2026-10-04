# GRAVIVORE — VFX Production Specification

Status: **READY FOR INTEGRATION REVIEW**

## 1. Visual language

Target:
- restrained energy;
- sparks;
- ionization;
- electrical arcs;
- industrial heat;
- small shockwaves;
- brief emissive bloom;
- mechanical debris;
- clear gameplay telegraphs.

Avoid:
- giant cartoon explosions;
- colorful fireworks;
- opaque screen-filling particles;
- magic circles/runes;
- bloom as geometry;
- thick solid laser tubes;
- excessive trails.

Palette:
- player/neutral energy: cyan / cold white with restrained blue;
- hostile: red/orange with hot-white core only at impact;
- repair: cyan/white with occasional warm welding spark;
- environment: do not add rainbow accent families.

## 2. Player Gravity Lash

Existing authoritative sequence remains:
`CHARGE -> RELEASE -> TRAVEL/LASH -> IMPACT -> HIT/DEATH`

### Charge
Production direction:
- core emissive ramp at the attack socket;
- 6–16 small inward-moving particles maximum;
- intermittent micro-arcs;
- one subtle expanding/contracting ring only if it reads as machinery, not magic;
- no screen-space flash.

Timing:
- follows existing charge/windup duration;
- visual may anticipate but never decides damage.

### Release
- 1–2 frame-equivalent impulse flash;
- short directional particle burst;
- optional small mechanical recoil accent on presentation rig;
- no large radial explosion.

### Travel / Lash
Use a layered filament:
1. thin bright core;
2. softer faint halo;
3. sparse travelling sparks/ion particles;
4. optional very light UV/noise distortion only if profiled safe.

Do not use:
- opaque cylinder;
- huge ribbon trail;
- multi-color plasma.

### Impact
- localized white-hot/cyan flash;
- 6–18 sparks;
- 2–6 tiny debris fragments;
- optional small ring/distortion;
- total visible tail ~0.10–0.25 s for normal hit.

### Kill
Distinct from hit:
- stronger electrical discharge;
- 8–24 sparks/debris;
- short component-failure vent;
- no oversized explosion unless a future enemy archetype explicitly warrants it.

## 3. Ordinary hostile VFX

### Scout Drone
- compact needle-like attack cue;
- fast small red/orange pulse;
- tiny exhaust/hover particles if required;
- silhouette remains readable.

### Cutter
- hot metal edge / friction sparks;
- short directional slash/cut line;
- avoid beam language.

### Arc Drone
- forked electrical pre-arc;
- brief branching strike;
- cyan is reserved for player/neutral, so hostile arc leans hot white/red-orange.

### Warden
- plated/shield pulse;
- broader but dimmer energy sheet;
- hit can throw small armor sparks.

### Carrier
- heavy mechanical discharge;
- denser debris/smoke puff;
- slowest hostile effect rhythm.

Archetypes must not all collapse to the same red beam.

## 4. Magnetar Guard

Signature:
- restrained magnetic coil glow;
- short orbiting ion flecks near coils;
- charge: paired coil saturation;
- attack: compressed radial magnetic pulse;
- shockwave: ground-hugging ring with sparse debris lift;
- hit: reinforced metal sparks;
- death: coil instability then shutdown burst.

No constant particle halo.

## 5. Custodian M-0

Identity: large hostile maintenance machine.

### Cone
Geometry:
- fan/cone floor telegraph;
- segmented hazard bands or directional chevrons;
- growing intensity toward resolve.

Impact:
- forward industrial vent/pressure burst;
- sparks/debris near source;
- no full-screen cone opacity.

### Line
Geometry:
- narrow straight corridor;
- strong center line + faint edge rails;
- easiest of the three to read as "do not stand here".

Impact:
- focused high-energy pulse;
- brief directional filament and floor sparks.

### Circle
Geometry:
- closed ring around target/center;
- segmented perimeter;
- interior remains mostly transparent.

Impact:
- radial low shockwave;
- upward dust/debris only at low density.

Shared requirements:
- geometry remains gameplay-authoritative;
- VFX must exactly respect existing telegraph footprint;
- each family has different silhouette before color is considered;
- portrait readability wins over detail.

## 6. Repair hub VFX

- welding sparks: small, warm, intermittent;
- repair beam: thin cyan line/filament, not solid tube;
- scanner sweep: transparent low-opacity plane/ring, very brief;
- energy transfer: a few particles moving toward player/core;
- service effects tied to repair arms/presentation anchors only.

Purpose: explain accelerated healing without becoming gameplay authority.

## 7. Candidate public sources

### Kenney Particle Pack
- Official: https://kenney.nl/assets/particle-pack
- 80 files
- CC0
- Grade: **A/B source**
- Use: neutral particle sprites/textures for sparks, smoke, flashes and small effects.
- Do not drop sprites into game unmodified if they read cartoonish; recolor/crop/materialize consistently.

### OpenGameArt CC0 particle/beam references
Only promote individual files after exact page/license verification and local texture inspection. The integration branch may also author simple procedural gradients/noise textures in-project to avoid unnecessary external dependencies.

## 8. Mobile guardrails

Per frequent one-shot effect:
- preferred live particles: **8–32**;
- hard review threshold: **>64**;
- lifetime: usually **0.08–0.45 s**;
- transparent stacked layers: aim **<=3** at any one impact;
- trails: **0–1** per attack instance;
- realtime lights: **0 by default**;
- distortion: one low-resolution/lightweight layer at most, and optional by quality tier;
- smoke: small screen footprint and short lifetime;
- pooled objects/materials where effects repeat.

Boss telegraph:
- may exceed ordinary particle count, but geometry should be mesh/decal-like rather than hundreds of particles.

Overdraw:
- never fill most of the portrait screen with soft transparent quads;
- favor thin geometry, additive filaments and sparse sparks;
- inspect on real Android device.

Shaders:
- URP-compatible;
- avoid expensive multi-pass transparent shaders;
- avoid per-effect GrabPass/screen copy patterns;
- shader keyword count kept small.

Pooling:
- charge/beam/impact/hit/death repeated effects must be pooled;
- no instantiate/destroy loop during steady-state combat.

## 9. Quality scaling

Recommended tiers:
- Low: core telegraphs + flash + minimal sparks; no distortion.
- Medium: baseline sparks + halo + minimal debris.
- High: optional distortion/extra debris, still no extra gameplay light.

The telegraph shape and timing must never change between tiers.

## 10. Future integration points

Likely files:
- `GravityLashVfxPool.cs` — replace visuals, preserve cue/timing authority.
- `PooledPulseVfx.cs` / presentation pools — replace prototype pulse geometry where appropriate.
- `S14CombatFeedbackPresenter.cs` — continue consuming gameplay events; do not move authority.
- Phase 3C actor prefabs — sockets for muzzle/coil/death presentation only.
- repair hub presentation anchors — service VFX only.
- boss/elite presentation — attack-family-specific telegraph visuals while authoritative geometry stays in gameplay.

No runtime file changes belong to this Phase 6A research branch.
