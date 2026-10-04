# GRAVIVORE — Audio / VFX Production Direction

Status: **READY FOR INTEGRATION REVIEW**  
Phase: **6A — research / production direction only**  
Branch: `audio-vfx/production-candidates`  
Research date: **2026-10-04**

## 1. Scope and boundary

This pack defines the final-feel direction that will replace the current toy-like S14 placeholder presentation after Phase 3C and the Chapter 1 content loop settle.

This branch intentionally contains **documentation only**.

Do not change in this lane:
- `S14AudioPresenter`;
- `S14CombatFeedbackPresenter`;
- `GravityLashVfxPool`;
- combat/gameplay authority;
- scenes;
- prefabs;
- composition root;
- build settings;
- current Phase 3C visual integration.

Reason no third-party binary assets are committed: the selected public sources expose suitable CC0 candidates, but the available autonomous browser path could not retrieve the binary files for local waveform/texture inspection. Committing uninspected binaries would violate the asset-quality gate. Source selection and exact license records are ready; binary intake remains a later explicit integration step.

## 2. Audio identity

Target language:

**heavy / mechanical / industrial / synthetic / restrained / cold / metallic / energetic / modern sci-fi**

The player should sound like a compact powered machine with a gravity-energy weapon, not a toy robot. The sound palette should be built from a small number of reusable layers rather than a unique sound for every event.

Avoid:
- 8-bit or arcade chirps;
- cartoon lasers;
- comedy impacts;
- generic UI beeps;
- fantasy magic;
- bright EDM laser sweeps;
- long cinematic tails on repeated combat;
- constant high-frequency machinery loops.

### Core layering model

Use four reusable layer families:

1. **Mechanical body** — clank, servo, actuator, panel movement.
2. **Energy** — charge, electrical texture, short discharge, ionized tail.
3. **Mass / impact** — low-mid transient, metal strike, brief sub-weight.
4. **System / UI** — restrained clicks, confirm/deny, diagnostic pulse.

A normal combat cue should usually use 1–3 layers, not all four.

## 3. Player SFX families

### Locomotion

**Step**
- dry mechanical foot plant;
- one short metal/body transient;
- optional very low actuator tuck;
- 80–220 ms target;
- no long ring-out;
- 2–4 variations preferred.

**Servo / actuator**
- very short movement accent tied to gait phase changes;
- quieter than foot plant;
- do not run a loud loop every frame.

**Idle servo**
- sparse, randomized, low-level;
- no continuous whining;
- one subtle event every few seconds is enough.

### Gravity Lash attack

Sequence must read as:

`CHARGE -> RELEASE -> TRAVEL -> IMPACT`

**Charge**
- rising electromagnetic/coil layer;
- restrained mechanical tension;
- duration follows authoritative charge timing;
- should not imply the hit has happened.

**Release**
- hardest player-owned transient in the sequence;
- short pressure impulse + synthetic snap;
- optionally one mechanical recoil layer.

**Travel**
- thin energy filament texture;
- mostly visual;
- if audible, use a quiet 60–150 ms hiss/zap rather than a full laser effect.

**Impact**
- short metal/energy hybrid;
- more weight than the release at target position, but not a huge explosion;
- one localized crack + spark/debris layer.

**Kill**
- stronger mechanical failure after impact;
- must be distinct from a non-lethal hit.

## 4. Damage / death / progression

**Enemy hit**
- small metal strike + electrical spit;
- fast decay;
- avoid identical pitch every hit.

**Critical / heavy hit**
- optional low transient layer;
- reserve for boss/elite or future critical system.

**Player hit**
- denser armor impact than ordinary enemy hit;
- short warning layer may be added, but no comedy beep.

**Player death**
- power-down / motor collapse / short energy discharge;
- 0.5–1.2 s target;
- avoid melodramatic cinematic tail.

**Enemy death**
- mechanical failure + final electrical vent;
- ordinary: compact;
- elite: larger coil collapse;
- boss: staged shutdown.

**Stat gain / evolution / unlock**
- clean ascending system tone plus low mechanical confirmation;
- evolution may be wider/stereo because it is non-positional and rare;
- do not reuse basic menu confirmation.

**Chapter completion**
- short 1.5–3 s stinger is enough for MVP;
- cold industrial success, not heroic orchestral fanfare.

## 5. Enemy audio identity

Do not build five isolated libraries. Reuse movement / attack / impact / death classes and add a signature only where silhouette/gameplay needs it.

### Scout Drone
- movement: light electric rotor/servo ticks;
- attack: thin high-mid discharge;
- impact: small metallic tick;
- signature: brief sensor chirp is acceptable only if subtle.

### Cutter
- movement: scraping actuator / low crawler mechanism;
- attack: mechanical spin-up or cutter bite;
- impact: sharper metal cut transient;
- signature: short blade/drive whirr.

### Arc Drone
- movement: light suspended motor;
- attack: electrical charge + arc snap;
- impact: crackle, not a red laser sound;
- signature: restrained pre-arc buzz.

### Warden
- movement: heavier joint/armor movement;
- attack: blocked pressure pulse / defensive discharge;
- impact: plated metal hit;
- signature: shield/system energize layer.

### Carrier
- movement: slow heavy chassis / load-bearing actuator;
- attack: low mechanical launch/discharge;
- impact: heavy metal;
- signature: cargo/power-pack hum used sparingly.

The weight hierarchy must remain obvious:

`Scout < Cutter/Arc < Warden < Carrier < Magnetar < Custodian`

## 6. Magnetar Guard

Identity: **electromagnetic industrial guardian**.

Required cue families:
- ambient coil: intermittent, low-level, not constant;
- movement: heavy servo with electrical undertone;
- charge: rising coil saturation;
- attack: dense magnetic discharge;
- shockwave: low pressure pulse + short crack;
- hit: reinforced metal + electrical sputter;
- death: coil collapse + mechanical falloff;
- availability/spawn: optional one-shot system wake cue.

Important: no permanent loud hum. Use a quiet bed only within short range or as intermittent one-shots.

## 7. Custodian M-0

Identity: **large hostile industrial maintenance machinery**.

Required:
- locomotion: crane/servo/large actuator;
- reactor: low restrained machinery bed;
- cone: broad pressure/vent discharge;
- line: focused high-energy rail/beam charge and snap;
- circle: radial capacitor/pressure build and pulse;
- heavy hit: dense plated metal transient;
- phase/telegraph: each attack family must be distinguishable before impact;
- death: multi-stage shutdown, not one explosion.

### Boss telegraph audio distinction

**Cone**
- wide noisy pressure rise;
- low-mid air/steam/energy character.

**Line**
- narrow tonal charge;
- fastest, most focused cue.

**Circle**
- cyclic pulse / capacitor rhythm;
- round low-frequency swell.

The three cues may share a common Custodian mechanical bed but must have different envelopes and frequency emphasis.

## 8. Repair hub audio

Desired:
- quiet industrial room tone;
- occasional service-arm servo;
- sparse welding/spark one-shots;
- soft diagnostic pulse;
- repair beam texture;
- stronger heal-complete cue only when useful.

Rules:
- no constant piercing welder loop;
- no high-frequency whine;
- ambient bed at least 8–15 dB below combat transients;
- service-arm events should be sparse enough that repeated visits do not fatigue the player.

## 9. Music direction comparison

### A — Dark sci-fi ambient

Best for:
- exploration;
- long repeated sessions;
- leaving space for combat SFX.

Risk:
- can become too empty or horror-like.

Masking:
- lowest risk if pads remain soft and midrange is sparse.

Implementation:
- one seamless loop is enough.

### B — Restrained industrial pulse

Best for:
- adding forward motion and machinery identity.

Risk:
- repeated pulse can fatigue quickly;
- percussion may mask footsteps/impacts.

Masking:
- medium risk.

Implementation:
- still simple, but mix discipline matters more.

### C — Hybrid ambient + mechanical rhythm

Best for:
- strongest GRAVIVORE identity;
- ambient space with occasional machine rhythm rather than a constant beat.

Risk:
- requires the best source/editing;
- rhythm must not become cyberpunk-club music.

Masking:
- low-to-medium if percussion remains sparse.

Implementation:
- one loop or one ambient loop plus a very light baked-in pulse.

### Baseline recommendation

**C — hybrid ambient + restrained mechanical rhythm**, but implemented as a **single exploration loop for first external test**.

If no candidate survives device audition, fall back to A rather than overbuilding dynamic music.

## 10. MVP music structure

Evaluated models:

- A. one Chapter loop;
- B. exploration + boss track;
- C. exploration + elite/boss intensity layer;
- D. dynamic layered system.

Recommended MVP: **A — one Chapter loop**.

Why:
- lowest integration complexity;
- lowest transition bug risk;
- easiest to judge fatigue;
- current game needs coherent baseline more than adaptive scoring.

Optional first upgrade after external test: **B — exploration + boss track**.

Do not build a dynamic mixer/layer framework in Phase 6 integration unless a real device review proves the one-loop model insufficient.

## 11. Mobile audio guidance

Keep compatible with the current small pooled-source system.

### Source format
- keep master/source files lossless when available: WAV/FLAC;
- author at 44.1 or 48 kHz;
- 16-bit is sufficient for shipped SFX; 24-bit source is fine before import;
- do not upsample low-rate source material.

### Mono vs stereo
- positional combat, footsteps, servo, enemy movement: **mono**;
- UI: mono/non-spatial is fine;
- evolution / Chapter completion: stereo allowed;
- music: stereo.

### Unity import direction
- short frequent one-shots: PCM or ADPCM after profiling;
- longer one-shots: compressed-in-memory Vorbis;
- music: Vorbis streaming;
- avoid decompress-on-load for long music.

### Voice budget
Current S14 code uses four pooled AudioSources by default, with one effectively reserved for footsteps. P0 replacement must work within that envelope.

Future integration may raise the ceiling modestly, but target:
- 4 active S14 voices baseline;
- 6–8 total gameplay SFX voices only if device profiling proves safe;
- music/ambient handled separately only when introduced.

### Priority if future voice stealing is added

Highest to lowest:
1. boss telegraph / player death;
2. boss/elite impact;
3. player attack release / impact;
4. player hit / enemy death;
5. enemy hit;
6. footsteps;
7. minor UI;
8. ambient detail.

### Spatial philosophy
- player attack release: mostly centered/non-spatial;
- target impact / enemy events: light 3D spatialization;
- boss telegraph: readable even if off-center;
- UI/music: 2D;
- avoid exaggerated stereo pan on portrait phone speakers.

## 12. Replacement priority

### P0 — perceived-quality blockers
1. player attack release;
2. player attack impact;
3. ordinary enemy death;
4. boss cone/line/circle telegraphs;
5. player footsteps;
6. current pulse/beam VFX replacement.

### P1
1. enemy movement families;
2. elite identity;
3. repair hub;
4. player hit/death;
5. progression/evolution;
6. restrained UI sounds.

### P2
1. minor ambient machinery variation;
2. extra enemy micro-variations;
3. optional nonessential notifications.

## 13. Future integration map

Likely integration points only; do not edit them in this branch.

- `Assets/_Game/Runtime/Presentation/Feedback/S14AudioPresenter.cs`
  - replace/extend cue resolution;
  - preserve bounded pooling;
  - keep mute/volume settings behavior.

- `Assets/_Game/Runtime/Presentation/Feedback/S14PresentationDefinition.cs`
  - bind real clips or reference a small Phase 6 audio palette;
  - keep timings data-driven.

- `Assets/_Game/Editor/Configuration/S14PresentationAssetConfigurator.cs`
  - retain generated tones only as fallback/bootstrap;
  - do not overwrite curated clips during configuration.

- `Assets/_Game/Runtime/Presentation/Feedback/S14CombatFeedbackPresenter.cs`
  - currently receives enemy hit/death, progression, player damage/death, lash cues, elite/boss telegraphs/impacts;
  - extend routing without moving authority into presentation.

- `Assets/_Game/Runtime/Presentation/Combat/GravityLashVfxPool.cs`
  - replace simple charge/beam/impact presentation while keeping existing authoritative cue sequence.

- `Assets/_Game/Runtime/Presentation/Player/MechMotionPresenter.cs`
  - retain gait-driven step timing; replace sound assets, not movement authority.

- Phase 3C world presentation / repair hub visual layer
  - attach repair ambience and service VFX only after final anchors/prefabs settle.

- Elite/boss repeatable loop
  - availability/spawn cues must use its final repeatability state rather than inventing duplicate timers.

Dependencies:
- Phase 3C final actor/environment result;
- final elite/boss repeatability/events;
- final repair-hub presentation anchors.

## 14. Acceptance for future integration

On Android:
- combat no longer reads as toy-like or early-2000s;
- attack charge/release/impact are distinct without looking or sounding exaggerated;
- ordinary hit and death are immediately distinguishable;
- Scout and Carrier clearly differ in perceived mass;
- Magnetar and Custodian have unique identity without voice acting;
- boss attack families are distinguishable by both geometry and sound;
- repair hub is informative but not irritating;
- music survives repeated 20–30 minute sessions without masking combat;
- steady-state combat creates no audio/VFX allocations beyond existing pooled design;
- 60 FPS target remains intact.

Final status: **AUDIO / VFX PRODUCTION PACK: READY FOR INTEGRATION REVIEW**
