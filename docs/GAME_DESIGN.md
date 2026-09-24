# Game Design Specification — Vertical Slice v0.1

## 1. Camera and movement

Portrait top-down/isometric perspective.

Recommended initial camera:
- yaw fixed;
- pitch around 55–65 degrees downward;
- modest perspective, not orthographic;
- camera follows with damping;
- camera does not rotate in v0.1.

Movement:
- floating virtual joystick;
- dead zone;
- analog speed from input magnitude;
- player rotates smoothly toward movement vector;
- no sprint button;
- no auto navigation.

## 2. Targeting

Targeting exists to make one-thumb combat readable, not to play the whole game.

Candidate requirements:
- alive;
- hostile;
- enabled;
- inside acquisition radius;
- not occluded by a hard blocker when line-of-sight applies.

Scoring:
1. nearest target in front-biased sector;
2. strong preference for current target while it remains valid;
3. break lock if target exceeds release radius or dies.

Do not constantly retarget each frame when two enemies are nearly equal; use hysteresis/stickiness.

## 3. Auto attack / Gravity Lash

Default player attack:

1. target acquired;
2. attack wind-up;
3. gravity lash/beam reaches target;
4. damage applies;
5. if target is pullable, target moves toward an impact point near the player;
6. attack enters cooldown;
7. repeat while target remains valid.

Standard mobs: full pull.  
Elite: partial/short pull.  
Boss: displacement immune.

Pull must never clip an enemy through walls. Use a safe destination/nav/collision check.

## 4. Combat math

Keep formulas simple for the first slice.

Core derived concepts:
- MaxHP
- Power
- Armor
- AttackInterval or AttackSpeed
- MoveSpeed

Suggested ordinary physical damage:

`effectiveDamage = max(1, rawDamage * 100 / (100 + armor))`

Do not expose the exact formula in UI yet.

Attack speed must have a lower interval cap to prevent animation/system breakage.

## 5. Death

Enemy:
- cancel attacks;
- play death reaction;
- spawn/award core reward;
- notify objective/progression systems;
- return to pool after presentation delay;
- schedule respawn through owning spawn spot.

Player:
- death presentation;
- no permanent-stat loss;
- return to central basin;
- restore HP;
- ordinary world state remains;
- no currency penalty in v0.1.

## 6. Assimilation

Core reward can be represented visually as a shard/orb, but progression authority is deterministic.

Flow:
`enemy death -> reward event -> pickup/assimilation feedback -> progression service -> stat changed -> save dirty -> UI/evolution observers update`

Do not make a dropped physics object the authoritative record of reward. If presentation fails, progression reward must not disappear.

## 7. Five permanent stats

v0.1:
- Power: base attack damage.
- Hull: max HP.
- Armor: damage mitigation.
- Flux: attack cadence.
- Mobility: movement speed.

Gravity range/strength can improve at fixed slice milestones rather than as a sixth grind stat.

Each ordinary enemy archetype has a primary stat reward.

## 8. Growth curve

Goal: obvious early acceleration without exponential numerical nonsense.

For each stat:
- levels are integer;
- reward cores fill a small level progress requirement;
- cost increases gradually;
- first several levels arrive quickly;
- later levels take more kills.

All curves are ScriptableObject/config-driven.

Do not lock final economy constants into code.

## 9. Visual evolution

At minimum:
- Evolution Tier 0 at start.
- Evolution Tier 1 around 25–35% slice completion.
- Evolution Tier 2 around 65–75%.

Tier can use overall assimilation score.
Dominant-stat accents may choose which attachment is enabled.

Implementation must support:
`EvolutionDefinition -> required score -> attachment set -> VFX/material overrides`

No mesh editing at runtime is required.

## 10. Spawn spots

Five spots.

Each spot:
- owns fixed spawn anchors;
- desired live count 3–5 (default 4);
- respawns ordinary enemies after 8–14 seconds, configurable with jitter;
- caps total alive;
- never spawns within a minimum distance of the player if the anchor is on-screen/too close;
- returns pooled enemies.

A cleared ordinary spot should begin repopulating quickly. No hours-long wait mechanics.

## 11. Elite unlock

Elite unlock should occur after the player has demonstrated engagement with all five spots, not only raw time.

Default condition:
- complete one introductory objective per spot;
- reach a configured total assimilation score.

## 12. Boss unlock

Boss gate opens after elite defeat.

Boss arena should communicate danger before entry.
Player may leave/reset before the fight begins.
Once engaged, boss resets if player dies or fully exits the arena.

## 13. Boss skill share

The boss is designed so adequate stats are the main requirement, but movement matters.

Telegraphs:
- at least 0.8–1.2 s readable warning for major attacks;
- ground decal/VFX distinct from damage VFX;
- no unavoidable off-screen damage.

A sufficiently upgraded player should not need frame-perfect input.

## 14. Offline reward

Unlocked after the onboarding section.

For v0.1:
- cap: 2 hours;
- reward rate: roughly 20–30% of estimated active baseline;
- reward type: soft resource / generic assimilation material, not direct boss completion;
- show summary on return;
- claim in one tap;
- no ad multiplier in v0.1.

Architecture uses an `ITimeProvider`.
Local time is acceptable for the private APK, but must not be treated as cheat-proof.

## 15. Onboarding

No modal wall of text.

Sequence:
1. movement hint;
2. guide player toward Scout spot;
3. first auto-attack demonstrates itself;
4. first assimilation shows stat growth;
5. objective points at second stat spot;
6. after two systems are understood, UI expands.

Tutorial should be skippable only after first completion in future versions; no need in v0.1.

## 16. First-play completion

Target 30–45 minutes for a fresh tester with no purchases.

The slice ends after Custodian M-0 defeat with:
- short evolution feedback;
- chapter-complete panel;
- current stats;
- explicit "vertical slice complete" marker for development build.
