# S20 Chapter 01 Balance Report

## Goal and evidence level

S20 targets a fresh, competent 30-45 minute Chapter 01 completion. The deterministic model below catches obviously pathological pacing, but it is not evidence of fun or a completed human run. Final timing remains pending on a physical Android device.

## Camera

| Setting | Before | S20 |
| --- | ---: | ---: |
| Offset | `(0, 10, -7)` | `(0, 13.5, -10)` |
| Look-at height | `0.8` | `0.9` |
| Field of view | `42` | `46` |
| Position damping | `0.16` | `0.18` |

The camera preserves the existing follow implementation and portrait perspective while showing substantially more arena space. No gameplay value depends on the camera.

## Spatial scale second pass

Physical-device review found the first S20 camera and world layout functionally sound but still too compressed. The second pass keeps FOV at `46` and moves the camera from `(0, 13.5, -10)` to `(0, 14.8, -11.2)`. The roughly 10% increase in camera height yields approximately 20% more visible ground area without introducing wide-angle distortion. Look-at height `0.9` and damping `0.18` are unchanged.

The playable ground changes from `40 x 48`, centered at `(0, 0, 8)`, to `72 x 140`, centered at `(0, 0, 30)`. The starting basin and respawn move from `(0, 0, 0)` to `(0, 0, -30)`. The five ordinary region centers change as follows:

| Region | Before | Spatial second pass |
| --- | ---: | ---: |
| Relay Yard | `(-8, 0, 6)` | `(-26, 0, 20)` |
| Cutting Floor | `(0, 0, 10)` | `(0, 0, 40)` |
| Shield Dump | `(8, 0, 6)` | `(26, 0, 20)` |
| Capacitor Field | `(-7, 0, -7)` | `(-20, 0, -12)` |
| Hauler Graveyard | `(7, 0, -7)` | `(20, 0, -12)` |

The elite gate / Magnetar Guard / boss gate / Custodian arena move from `z=15 / 18 / 21 / 27` to `z=60 / 69 / 80 / 94`. This creates a fan-shaped ordinary route around the starting region followed by a separate final approach. Spawn populations, anchor offsets, collision authority, gates, quest ids, and encounter lifecycle are unchanged.

At the fresh-profile movement speed of `4.5` units/second, direct uninterrupted estimates are approximately `7-9` seconds between useful neighboring farming regions, `15.6` seconds from start to the farthest ordinary region, `6.4` seconds from the closest ordinary center to the elite spawn, and `5.6` seconds from the elite spawn to the boss center. Actual traversal will be longer when routing around combat and landmarks.

## Ordinary aggression

Base acquisition radii were reduced from `4.5-5.5` to `3.5-4.0`. Ordinary enemies compare existing player output/durability against their configured output/HP. At a player-to-enemy strength ratio of `15`, proactive acquisition becomes 55% of base; at `30`, it becomes 20% of base, never below attack range. The thresholds account for ordinary enemies fighting in local groups, so a fresh player still receives normal early pressure. Close contact remains interactive. Any damage explicitly engages the attacked ordinary enemy until normal release distance. Elite and boss controllers do not use this policy.

## HUD and menu

The gameplay HUD no longer creates a persistent five-stat panel. It retains the short level-up message, HP, objectives, and contextual boss HP. The existing pause modal is now the compact game menu and reads authoritative player stats, derived values, and equipped slots. It retains sound, volume, haptics, and resume controls; opening it still pauses scaled gameplay.

Pause and development button hit rectangles remain unchanged and non-overlapping. Their visible child is inset to 64% in each dimension, a 36% visual size reduction without shrinking the touch target.

## Balance changes

### Ordinary enemies

| Role | HP before / S20 | Damage before / S20 | Aggro before / S20 |
| --- | ---: | ---: | ---: |
| Scout | 28 / 26 | 3 / 3 | 5.5 / 4.0 |
| Cutter | 45 / 42 | 6 / 5.5 | 5.0 / 3.8 |
| Warden | 70 / 62 | 5 / 5 | 4.5 / 3.5 |
| Arc | 30 / 28 | 5 / 4.5 | 5.5 / 4.0 |
| Carrier | 85 / 72 | 8 / 7 | 4.5 / 3.5 |

All five spots retain four live enemies and their stat identities. Respawn jitter changes from 8-14 seconds to 8-12 seconds to reduce empty farming while remaining inside the design range and global cap.

### Progression and evolution

Rewards remain one stat XP and one assimilation per kill. The first stat level still costs three XP, so focused play produces a first increase after three relevant kills. The elite gate moves from 25 to 60 assimilation and still requires all five introductory objectives. This prevents a narrow rush while preserving the existing quest flow.

Evolution thresholds move from 10/30 to 20/42. Relative to the 60-assimilation elite gate, these are 33% and 70%, placing both visible moments before elite/boss eligibility.

### Elite and boss

Magnetar Guard changes from 250 HP, 10 armor, 14 damage to 220 HP, 8 armor, 11 damage; recovery changes from 1.4 to 1.5 seconds. It remains materially stronger than ordinary enemies but avoids demanding additional grind after the broader unlock.

Custodian M-0 changes from 1000 HP/20 armor to 820 HP/15 armor. Circle, cone, and line damage change from 22/24/28 to 18/20/24. Attack types, telegraphs, low-health cadence, displacement immunity, persistence, and three-second reset grace are unchanged. The intent is an approximately 80% progression check with readable movement decisions rather than frame-perfect reactions.

### Offline

The configured active baseline remains 120 units/hour, offline efficiency remains 25%, and the cap remains two hours. The 25% rate sits inside the 20-30% target and remains secondary to active play.

## Deterministic pacing model

The Editor model reads the real progression rewards, threshold curve, evolution thresholds, and elite requirement. The ordinary-kill assumption remains 24 seconds averaged across combat/rotation. The aggregate navigation, elite, and boss allowance increases from 10 to 11.5 minutes for the expanded route; this is a geometry-informed guardrail, not a substitute for the pending fresh-profile run.

- First focused stat increase: 3 kills, approximately 1-2 minutes.
- Tier 1: 20 assimilation, approximately 8 minutes of ordinary activity.
- Tier 2: 42 assimilation, approximately 17 minutes of ordinary activity.
- Elite eligibility: minimum 60 kills plus all five objectives.
- Estimated initial boss-ready completion path: approximately 35.5 minutes.
- Broad upper readiness estimate with 20 additional ordinary kills: approximately 43.5 minutes.

These estimates are guardrails. Player routing, deaths, stat distribution, equipment, and movement execution can shift real timing.

## Art status

The current S15 CC0/prototype visual set is explicitly a vertical-slice asset set, not final commercial-quality art. Commercial art direction and replacement are a post-S20 product milestone. S20 intentionally freezes gameplay and balance before spending time on another asset replacement pass.

## Remaining validation

- Fresh-profile milestone timing and final levels on a physical Android device.
- Portrait camera readability across ordinary combat and all boss telegraphs.
- Low-power and highly outscaled ordinary-enemy acquisition feel.
- Menu readability, safe-area placement, and touch comfort on tall devices.
- Subjective grind, difficulty, and empty-wait assessment.
