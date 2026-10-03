# GRAVIVORE — Post-S20 Product Roadmap

Status: **LOCKED WORKING PLAN — do not expand scope without explicit review.**

This roadmap captures the agreed direction after S20 and the current ART prototype work. It is intentionally product-focused: first make GRAVIVORE a game the owner wants to play, then scale content and polish.

## Current baseline

Confirmed working on Android:
- movement and combat;
- player regen and repair behavior;
- player/enemy respawn;
- save/restore and offline return;
- stat progression;
- HP bars;
- boss reset grace (<3s keeps state, >3s resets);
- background/resume and device lock/unlock;
- DEV tooling;
- current farther camera direction is accepted;
- S20 enlarged map direction is accepted provisionally.

Current ART status:
- runtime art preview pipeline exists;
- current player proxy is too tank-like;
- target player direction is a more explicit **mecha robot / combat robot / techno-mechanical organism**;
- current proxy should be treated as temporary, not final art.

## Locked product decisions

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

Target system:
- each spawn/spot keeps a base respawn delay;
- repeated farming of the same spot increases its respawn delay stepwise;
- delay is capped;
- leaving the spot idle gradually returns the penalty toward baseline;
- tuning remains data-driven;
- movement between different spots should remain more efficient than camping one forever.

### Elite loop

Elite is not a one-time key only.

Target:
- elite remains part of Chapter progression;
- after first progression kill, it becomes a repeatable valuable target;
- nominal respawn: **1 hour**;
- nominal daily reward cap: **3 rewarded kills/day**;
- reward value is materially above ordinary farming;
- first rewarded kill of the day may be the strongest reward;
- no requirement for real-money monetization.

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

These are not new Chapters; they are Chapter 1 depth.

### Map / minimap

Future map UX should show:
- player position;
- ordinary farming spots;
- elite;
- boss;
- repair hub;
- spot availability state;
- optional respawn countdown when useful.

Map/minimap comes after point-of-interest layout is stable. Do not build a large map framework prematurely.

### Audio direction

Current placeholder beeps are not target audio.

Target SFX language:
- sci-fi mechanical;
- servo / actuator movement;
- metallic impacts;
- restrained energy discharge;
- industrial repair sounds;
- distinct hit / death / progression / UI cues.

Required event families:
- player attack;
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
- readable silhouette from current mobile camera.

Content art sequence:
1. player;
2. ordinary enemy families;
3. elite;
4. boss;
5. repair hub;
6. spot-specific landmarks;
7. Chapter environment dressing.

The boss should preserve the previously approved large industrial-mech visual direction.

## Roadmap

| Phase | Scope | Why now | Primary owner/tool | Gate to proceed |
|---|---|---|---|---|
| 0. Current Codex task | Player visual correction + adaptive ordinary respawn + minimal map-prep | Already in progress | Codex | Review code, tests and Android behavior |
| 1. Checkpoint stabilization | Review current Codex result; reconcile S20/ART branches; run automated + targeted device checks; merge only approved state | Create one trustworthy baseline before more content | ChatGPT review + Codex fixes | Main is green and reproducible |
| 2. External design review | Independent review of Chapter 1 loop, rewards, timers, anti-farm, map UX and priorities | Get a second design perspective before implementing economy/content depth | Claude | Review findings triaged into accept/reject/defer |
| 3. Chapter 1 content loop | Repair hub; repeatable elite; repeatable boss; stronger spots near elite/boss; reward contracts; timer/cap state | Turns technical slice into a repeatable game loop | Codex | Android loop works without progression/save regressions |
| 4. Map/minimap MVP | Player + POIs + elite/boss/repair hub + respawn availability/timers | Useful after POI layout and timers exist | Codex | Mobile readability/device review |
| 5. Audio pass | Replace placeholder SFX; add mix categories; repair/elite/boss cues; audition music loops | Major perceived-quality increase without changing core mechanics | Asset research + Codex integration | Device audio review |
| 6. Full visual content pass | Remaining mobs, elite, boss, spawn landmarks, Chapter environment | Art direction and core loop are now stable | Asset research + Codex | Human visual/device approval |
| 7. Balance / fresh-profile run | 30–45 min clean profile timing, route quality, reward pacing, elite/boss recurrence | Validate actual player experience rather than model estimates | Human playtest + tuning | Measured run accepted |
| 8. Closed external test | Small group gets APK with minimal instruction | Validate comprehension/fun outside project context | Human testers | Feedback triaged |
| 9. Next-content decision | Chapter 2 vs deeper meta-loop based on test results | Avoid speculative expansion | Product decision | Explicit go/no-go |

## Rules for upcoming work

- Do not introduce store/monetization work into the near-term roadmap.
- Do not build Chapter 2 before Chapter 1 is enjoyable and externally testable.
- Do not let visuals become gameplay authority.
- Do not add a large inventory/map/audio framework before its immediate MVP need exists.
- Keep new timers, reward caps and respawn behavior data-driven and testable.
- Repeatable elite/boss rewards must be idempotent and safe across save/reload/background transitions.
- Real device behavior remains the final gate for visual/audio/readability changes.

## Claude review checkpoint

After the current Codex task completes:

1. ChatGPT reviews the exact PR/commits first.
2. Only after that review, prepare a clean project snapshot for Claude.
3. Preferred review input:
   - repository ZIP or source snapshot at the reviewed checkpoint;
   - exclude Library/, Temp/, Logs/, Build/, caches and generated APKs;
   - include Assets/, Packages/, ProjectSettings/, docs/, specs/, AGENTS.md and relevant tests;
   - include this roadmap;
   - include current PR/commit identifiers and a short description of verified Android behavior.
4. Claude acts as an independent senior game/systems/product reviewer, not as the source of truth.
5. Claude recommendations are triaged into:
   - ACCEPT;
   - ACCEPT WITH MODIFICATION;
   - DEFER;
   - REJECT.

ChatGPT will provide the exact Claude prompt and exact ZIP/snapshot instructions after the current Codex result is reviewed.

## Current next action

**WAIT FOR CURRENT CODEX TASK TO FINISH.**

Do not merge active S20/ART/current-Codex work solely because this roadmap exists.
After Codex finishes, review first, then choose the checkpoint merge sequence.
