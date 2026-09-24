# Test Strategy

## Test pyramid for v0.1

### Edit-mode deterministic tests
Required for:
- damage/armor formula;
- attack cadence clamping;
- stat progression curves;
- target scoring where separable;
- evolution threshold selection;
- save DTO validation/migration;
- offline reward caps;
- quest progress rules.

### Play-mode tests
Required smoke coverage for:
- player spawns and moves;
- enemy can spawn, be targeted, take damage and die;
- death grants progression once;
- pooled enemy resets correctly;
- evolution presenter reacts to threshold;
- save/reload preserves progression;
- boss gate unlock path;
- no duplicate reward after reload/respawn.

### Manual device checklist
For every candidate APK:
- install over clean state;
- launch in portrait;
- background/resume;
- lock/unlock phone;
- interruption by notification/app switch;
- movement on screen edges;
- combat with 20+ enemies present;
- 15-minute heat/performance check;
- kill app during/after save;
- relaunch;
- offline reward after clock delay;
- boss telegraph readability;
- no UI under Android navigation/status areas.

## Regression acceptance

A spec is not complete if:
- compile errors exist;
- critical tests fail;
- new warnings indicate broken serialization/references;
- main scene has missing scripts;
- build validation finds unassigned required definitions.

## Save tests

Test:
- current schema;
- one older schema migration fixture;
- corrupt main + valid backup;
- invalid ids/ranges;
- interrupted temp write simulation where practical.

## Performance checks

Use Unity Profiler/dev overlay.

Watch:
- GC allocations during steady combat;
- spikes at enemy respawn;
- VFX pooling;
- draw calls/material count;
- CPU time with max v0.1 population.

Do not optimize based only on intuition; record measured issues.
