# Chapter 01 gameplay UX

Accepted baseline: origin/main `0141b0a88c0e68b34c3a26c3b2a5ff3a5277807b` (PR #62).

Scope: ordinary first-kill 120-second wave eligibility and safe return; authoritative encounter state/timers; topology derived from traversal/blockers/gates; phone-readable tactical HUD; animated recovery presentation; bounded DEV cold-start evidence and regression coverage.

Preserve Chapter 01 layout, balance/rewards/progression, elite/boss cooldown authorities and global live cap. No Chapter 2 or concept fidelity rebuild.

Delivery gate: one verified ARM64 Android DEV APK, after compile, ProjectValidator, EditMode and PlayMode. Device installation/playthrough remains the human gate.

## Checkpoint

Isolated branch created from fetched accepted origin/main; checkpoint and ordinary respawn block pushed to Draft PR #63.

## Implementation

- Authored ordinary configurations now use a 120-second first-kill wave clock. Strong ordinary packs inherit it; Magnetar/Custodian retain their separate repeat authority.
- Later kills preserve the deadline. Full missing-group admission checks safe distance, live wave combat, global capacity and pool capacity. Relevance recycling preserves the clock. Strong-pack relevance includes a safe admission band.
- Both map read models consume authoritative time/state. Compact relevant markers show mm:ss/ГОТОВО; expanded markers and selection expose availability. Gate locks also govern inaccessible farming marker presentation.
- Two vector tactical surfaces derive wall/perimeter/gate footprints from actual HardBlocker boxes. Traversable floor, farming footprints, elite approach, containment arena and recovery pad use existing bounds/configuration. Open gates retain their opening outline.
- The existing recovery observer drives two articulated servo arms, a scanner and a repair pad identity. Objects, two shared material instances, one audio source and existing pooled repair VFX remain bounded. Healing/reward/save authorities are unchanged.
- DEV-only startup telemetry begins before Bootstrap scene loading, captures composition phases, early stalls, profiler counter availability, first enemy visual creation, and periodic hierarchy/material/VFX/audio inventories. Fixed buffers and two session files; stops after 90 seconds and disposes recorders. No speculative loading delay or shader warmup.

## Assumptions

The 120-second rule applies to all ordinary actors, including strong ordinary packs. Unloaded packs retain wave cooldowns in session; ordinary transient encounter cooldowns do not enter the progression save schema. Legacy adaptive calculation types remain for existing deterministic/nonproduction callers; canonical content always selects the wave policy.

Safe distance derives from the furthest anchor plus the enemy's existing aggro release radius (or minimum spawn distance, whichever is larger). Map topology depicts the open movement floor with actual solid blockers; zone shading describes authored encounter engagement footprints, not additional movement restrictions.

## Verification in progress

Focused EditMode: 5/5. Focused Chapter gameplay UX PlayMode: 6/6, including real 90-second Editor observation with first combat/farming and reload. Full compile, ProjectValidator, full EditMode, full PlayMode and ARM64 DEV APK verification follow.

Next milestone: CHAPTER 01 CONCEPT FIDELITY V2, after the APK device gate.
