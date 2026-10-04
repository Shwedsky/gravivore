# GRAVIVORE — Post-S20 Product Roadmap

Status: **LOCKED WORKING PLAN — change only through explicit review.**

This roadmap captures the agreed direction after S20, ART V3 and the mecha/adaptive-respawn checkpoint. Product rule: first make GRAVIVORE a game the owner wants to play, then scale content and polish.

## Current baseline

Merged and verified prototype baseline on `main`:
- S20 balance, farther camera and enlarged Chapter 1 world;
- movement and combat;
- player regen and repair behavior;
- player/enemy respawn;
- save/restore and offline return;
- stat progression;
- HP bars;
- boss reset grace (<3s keeps state, >3s resets);
- background/resume and device lock/unlock;
- DEV tooling;
- ART runtime pipeline;
- Cutter V3 runtime presentation;
- biped mecha G-0 prototype;
- adaptive ordinary-spot respawn;
- world-marker read-model foundation for future map/minimap.

Current ART status:
- Phase 2 mechanics/synchronization are accepted on device: walking, charge → beam → hit timing, HP synchronization, hit/death readability and performance are all acceptable for the prototype;
- the current biped model is better than the previous tank-like proxy, but is **still not the target final character**;
- current character animation, combat VFX and combat SFX still read as too toy-like / dated / "2000s prototype" for external presentation;
- the current map remains visually empty, which amplifies the prototype feel;
- target player direction remains a deliberate **modern mecha robot / combat robot / techno-mechanical organism**;
- current models, gait, combat VFX and SFX remain replaceable presentation prototypes, not showcase-quality assets.

## Locked product decisions

### Player locomotion / movement presentation

The player must no longer visually glide across the floor.

Target:
- G-0 visibly walks/steps while moving;
- feet/legs articulate in a readable mechanical gait;
- idle stance remains mechanically alive;
- locomotion presentation follows actual movement velocity/direction but does not become gameplay authority;
- no root-motion dependency for gameplay movement;
- animation must tolerate future replacement of the temporary character model.

MVP may use transform/procedural articulation if that is safer than building a full rig/IK stack now. Final production gait can be replaced later.

### Player attack presentation

Combat needs a complete readable action sequence, not just a functional damage event.

Required presentation:
- attack anticipation/charge;
- visible attack release;
- coherent attack origin/socket;
- projectile/beam/energy effect as appropriate;
- impact effect on target;
- hit reaction/readability on the enemy;
- clear difference between attack, hit, kill and miss/no-target states;
- presentation timing must not change gameplay damage authority.

Attack VFX should remain presentation-only and be replaceable without changing combat logic.

### Combat sound effects

Current placeholder beeps are not target audio.

Attack/hit audio must include:
- player attack charge/release;
- player attack impact;
- enemy hit;
- enemy death;
- player hit;
- player death;
- elite/boss signature cues later.

The audio language remains sci-fi mechanical / energy / industrial, not generic UI beeps.

### Player spawn / repair hub

The player spawn becomes a **semi-hangar / repair bay**:
- platform or bay around the spawn point;
- 2–4 articulated repair manipulators;
- deterministic looping manipulator paths;
- repair lasers / welding beams / service VFX;
- subtle industrial audio bed;
- presentation explains accelerated healing / recovery.

The hub is presentation-first and must not become gameplay authority.

### Ordinary farming and anti-freefarm

Standing indefinitely on one spot must not be the optimal strategy.

Current implemented prototype:
- authored baseline respawn remains 8–12 seconds;
- every fourth kill on one spot adds +8 seconds;
- maximum four penalty steps (+32 seconds);
- idle recovery removes pressure;
- spots track pressure independently;
- pressure is session-local for now.

Tuning remains provisional until device/fresh-profile playtests.

### Elite loop

Elite is not a one-time key only.

Target:
- elite remains part of Chapter progression;
- after first progression kill, it becomes a repeatable valuable target;
- nominal respawn: **1 hour**;
- nominal daily reward cap: **3 rewarded kills/day**;
- reward value is materially above ordinary farming;
- first rewarded kill of the day may be the strongest reward.

Reward direction:
- large stat/progression package;
- special Chapter resource / core fragment;
- possible guaranteed equipment/mod reward later.

Exact economy numbers remain a balance task.

### Boss loop

Boss is not disposable after the first Chapter clear.

Target:
- first kill still completes Chapter progression;
- boss later respawns as repeatable content;
- nominal respawn: **2 hours**;
- nominal daily reward cap: **2 rewarded kills/day**;
- reward is larger than elite reward;
- repeat kills do not repeatedly re-trigger Chapter completion authority.

Reward direction:
- major progression package;
- boss-specific rare Chapter resource;
- guaranteed higher-tier reward candidate later.

Exact economy numbers remain a balance task.

### Stronger zones around elite and boss

Chapter 1 should communicate danger/reward spatially.

Near elite:
- add **1–2 stronger ordinary farming spots**;
- clearly separated so they do not accidentally aggro with elite;
- tougher than starting spots;
- better stat/progression yield;
- slightly longer baseline respawn.

Near boss:
- add additional stronger spots by the same principle;
- separated from boss encounter and each other;
- greater danger / better reward than starting region.

### Map / minimap

Future map UX should show:
- player position;
- ordinary farming spots;
- elite;
- boss;
- repair hub;
- spot availability state;
- optional respawn countdown when useful.

Existing world-marker read-model should be reused. Do not build a large map framework prematurely.

### Audio direction

Target SFX language:
- sci-fi mechanical;
- servo / actuator movement;
- metallic impacts;
- restrained energy discharge;
- industrial repair sounds;
- distinct hit / death / progression / UI cues.

Required event families:
- locomotion/servo movement;
- player attack charge/release;
- enemy hit;
- player hit;
- enemy death;
- player death;
- stat/progression gain;
- elite;
- boss;
- repair hub;
- UI/menu;
- Chapter completion.

### Music direction

Initial direction:
**dark sci-fi ambient + restrained industrial texture**.

Goals:
- works over repeated sessions;
- does not fight combat readability;
- supports cold mechanical / techno-industrial identity.

Later compare several candidate loops before locking final music.

### Art direction

Player:
- more mecha-robotic;
- not tank-like;
- not a humanoid soldier;
- central energy/core identity remains useful;
- readable silhouette from current mobile camera;
- current biped proxy is a checkpoint, not the final target.

Content art sequence is now prioritized around **seeing the whole game world together**, not perfecting G-0 in isolation:

1. coherent Chapter 1 environment / terrain / industrial dressing;
2. ordinary enemy families;
3. elite;
4. boss;
5. repair hub;
6. spot-specific landmarks / stronger-zone identity;
7. G-0 refinement against the finished world context;
8. final locomotion/combat VFX/SFX polish.

Reason: isolated proxy iteration is no longer giving reliable quality judgments while the world is empty.

The boss should preserve the previously approved large industrial-mech visual direction.

## Roadmap

| Phase | Scope | Why now | Primary owner/tool | Gate to proceed |
|---|---|---|---|---|
| 0. Prototype checkpoint | S20 + ART V3 + mecha + adaptive respawn merged | Stable baseline now exists | Completed | main green and reproducible |
| 1. External design review | Independent main audit and product/system review | Second opinion before economy/retention decisions | Claude | Findings triaged; advisory, not blocking safe presentation work |
| 2. Player feel & combat presentation | Better mecha silhouette; visible walking/idle; attack motion; attack/impact VFX; initial real combat SFX | Player is on screen continuously; current sliding/placeholder combat presentation limits perceived quality | Codex + asset research | Implemented; device review PENDING — [delivery notes](PLAYER_FEEL_COMBAT_PRESENTATION.md), [checklist](PLAYER_FEEL_DEVICE_CHECKLIST.md) |
| 3. Visual world baseline | Populate Chapter 1 with coherent modern sci-fi environment dressing; replace ordinary mobs, elite and boss with credible prototype-quality models; add repair hub shell and spot landmarks | Current mechanics are good, but the empty/childlike presentation is now the biggest motivation and quality blocker | ChatGPT art/research lane + Codex integration lane | Android build feels like one coherent game world, even if assets are still prototype |
| 4. Chapter 1 content loop | Repeatable elite; repeatable boss; stronger spots near elite/boss; reward contracts; timer/cap state; complete repair-hub behavior | Build repeatable gameplay on top of a world worth traversing | Codex | Android loop works without progression/save regressions |
| 5. Map/minimap MVP | Player + POIs + elite/boss/repair hub + respawn availability/timers | Useful after POI layout and timers exist | Codex | Mobile readability/device review |
| 6. Audio/music + final feel pass | Replace toy-like prototype combat sounds; improve gait smoothness; modernize charge/beam/impact/death VFX; add repair/elite/boss cues; audition music loops | Polish only after all actors/environment are visible together | ChatGPT asset direction + Codex integration | Device audio/visual review |
| 7. Balance / fresh-profile run | 30–45 min clean profile timing, route quality, reward pacing, elite/boss recurrence | Validate actual player experience | Human playtest + tuning | Measured run accepted |
| 8. Closed external test | Small group gets APK with minimal instruction | Validate comprehension/fun outside project context | Human testers | Feedback triaged |
| 9. Next-content decision | Chapter 2 vs deeper meta-loop based on test results | Avoid speculative expansion | Product decision | Explicit go/no-go |

## Phase 2 acceptance target — Player feel & combat presentation

The next safe milestone should produce a visibly better playable APK without changing core balance.

Required:
- G-0 no longer reads as sliding when moving;
- clear mechanical idle and walk/step cycle;
- no root motion; gameplay movement remains authoritative;
- attack has anticipation/release presentation;
- Gravity Lash/attack origin stays aligned from all directions;
- impact VFX is readable on mobile;
- enemy hit/death feedback is distinguishable;
- real non-beep prototype SFX for attack/hit/death;
- no regression to HP, regen, saves, progression, respawn, boss reset or camera;
- current player model may be refined further, but all work must remain replaceable presentation code/assets.

Deferred from this milestone:
- final bespoke character mesh;
- full IK/advanced procedural locomotion;
- complete final sound library;
- music lock;
- all enemy art;
- elite/boss repeatability;
- minimap UI.

## Rules for upcoming work

- Do not introduce store/monetization work into the near-term roadmap.
- Do not build Chapter 2 before Chapter 1 is enjoyable and externally testable.
- Do not let visuals, animation or audio become gameplay authority.
- Do not use root motion to drive authoritative movement.
- Do not let attack animation timing become damage authority.
- Do not add a large inventory/map/audio framework before its immediate MVP need exists.
- Keep new timers, reward caps and respawn behavior data-driven and testable.
- Repeatable elite/boss rewards must be idempotent and safe across save/reload/background transitions.
- Real device behavior remains the final gate for visual/audio/readability changes.

## Claude review checkpoint

Claude review is now **asynchronous advisory work**. It should not block low-risk presentation improvements.

When the full audit is available:
1. triage findings into ACCEPT / ACCEPT WITH MODIFICATION / DEFER / REJECT;
2. Critical/Major correctness or release-safety issues can interrupt the roadmap;
3. game-design recommendations feed into Phase 3+;
4. do not let speculative refactors invalidate a working main checkpoint.

## Current next action

Phase 2 has passed the human prototype check for mechanics/synchronization and is merged as a **prototype checkpoint**, not as final presentation quality.

Start **Phase 3 — Visual world baseline**.

Priority order:
1. give the entire Chapter 1 map a coherent modern industrial sci-fi visual language;
2. replace/upgrade the ordinary mob families;
3. produce recognizable elite and boss prototype models in the same art language;
4. establish repair hub and spot landmarks so spaces have meaning;
5. then re-evaluate G-0 proportions/silhouette in context;
6. only after that spend another dedicated pass on gait smoothness, modern combat VFX and final-quality SFX.

Quality note:
- the current gait, combat timing and gameplay synchronization are accepted;
- the current visual/audio treatment is explicitly **internal prototype quality only** and should not be treated as external-showcase ready.

### Parallel-work rule

Use two coordinated lanes where practical:

**Codex lane**
- Unity implementation;
- prefab/runtime integration;
- scene placement;
- animation/presentation code;
- tests/builds/validation;
- performance and regression safety.

**ChatGPT lane**
- art direction and consistency review;
- web/asset research with license verification;
- model/environment/FX/audio candidate shortlists;
- reference boards / visual concepts where useful;
- specifications, review checklists and independent PR review.

The lanes must not edit the same implementation branch concurrently. ChatGPT should preferably deliver research/spec/assets or isolated documentation/reference branches; Codex integrates into the active Unity feature branch.

Claude audit can arrive asynchronously. Critical/Major correctness or release-safety findings may interrupt Phase 3; design/polish suggestions are triaged into the roadmap rather than automatically accepted.
