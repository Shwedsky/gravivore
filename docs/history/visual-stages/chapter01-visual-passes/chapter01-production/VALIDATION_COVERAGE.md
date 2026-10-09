# Chapter 01 validation coverage

Executed in Unity 6000.3.0f1 against the production source recorded in
`verification/build_metadata.json` and `verification/test_results.json`.
Full EditMode: 431 passed / 0 failed / 0 skipped.
Full PlayMode: 114 passed / 0 failed / 1 existing opt-in capture skipped.
The skipped test is `VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested`.
Compilation and ProjectValidator passed. APK proof is recorded separately after build.

The following maps the owner's 33 mandatory checks to executed coverage. Test
classes are under `Assets/_Game/Tests`, except the retained ArtSpike isolation suite.

1. All five production packages: `Chapter01ProductionTests.AllFivePackagesHaveAuthoredMachineryVerticalLandmarksAndOpaqueSharedMaterials` (five cases), scene dependencies, live package checks and internal rendered views.
2. Five-zone reachability: `CompleteWorldLockedAndUnlockedCapsuleRoutesAndActualControllerTraversal`, locked and unlocked capsule connectivity plus real controller travel.
3. Ordinary spots: all five centers and every authored anchor checked; twenty anchors traversed with the controller after unlock.
4. Strong spots: all four checked against locked state; all four traversed after unlock; the accepted east service doorway regression remains active.
5. Elite approach: locked approach reachable while encounter remains behind its gate; unlocked Magnetar approach/controller travel passed.
6. Boss approach/arena: inaccessible when gates are locked; controller reaches both after legitimate elite defeat/unlock.
7. Physical consistency: every new serialized obstacle has an enabled HardBlocker proxy with matching world bounds; art is collider-free. Raised arch uses separate posts/truss. Sampled wall coverage confirms continuous visible gate flanks, side boundaries and north perimeter. Existing authority-side collision suite passed.
8. Scout preservation: accepted binding checks plus Git preservation audit of original models, prefabs/materials and slice kit.
9. Cutter preservation: same audit and live binding assertions.
10. Magnetar preservation: same audit, live binding and accepted encounter/repeat tests.
11. Arc Drone production live: canonical binding/socket suite, three LODs, five animation states, valid meshes/materials and scene dependencies.
12. Carrier production live: same checks, plus pooled family replacement clears old socket references.
13. Warden production live: same canonical, LOD, animation and dependency checks.
14. Custodian production live: actual canonical model assertion, LOD/material/animation checks and all existing boss authority tests.
15. Full boss HP: actual encounter HUD visibly full, opaque fill and nonzero rendered width; internal portrait render.
16. Damage: actual HUD checked at half and five percent health after mitigated damage.
17. Reset: actual HP reset produces full fill and hidden HUD until next engagement.
18. Repeat: permanent first-clear completion does not suppress HUD; actual cooldown/repeat reinitializes full.
19. Duplicate plate: Custodian is absent from ordinary readability actors; dedicated HUD active; no ordinary overhead boss plate.
20. Current XP: every stat snapshot equals ProgressionState; live reward test and deterministic restored-state cases.
21. Required XP: every stat uses the authoritative threshold curve at the actual level.
22. Derived values: all five snapshots match the live PlayerStatsCalculator result, including equipment modifiers.
23. Next level: all five previews match the same calculator and modifier path without mutation/events.
24. Assimilation: authoritative requirement/objective count used; granted admission replaces until-unlock wording; total score remains explicit.
25. Live UI: reward/XP, level, equipment, quest and gate event subscriptions; reward/level/equipment revision assertions.
26. Save/reload: existing profile flushed, scene reloaded and per-stat XP/derived values matched before reopening the UI.
27. Rewards: preview equals actual committed grant; existing mitigated damage/reward-once and retry/recovery regressions passed.
28. Steps: measured 60-second sustained runs at 4.5, 6 and 7.5 m/s produce 80–80.24% of v38 counts; gain is 70%; minimum interval and clip duration checked; volume-setting changes preserve cue gains. Four voices remain bounded.
29. UI/VFX pools: existing 64-kill soak passed unchanged; six health plates, ten damage slots and four reward slots stay bounded; all five families use bounded shutdown meshes.
30. DEV hitches: existing 64-frame interval/four-snapshot/cooldown regression passed; telemetry remains compiled into DEV.
31. Telegraphs: existing elite/boss presentation and damage-geometry suites passed; encounter gameplay source/config audit unchanged.
32. Repeats: full encounter cooldown, capped rewards, retry/recovery and save restoration suites passed.
33. Russian UI: existing Russian smoke checks and all visible Characteristics/detail strings checked for Latin characters and text overflow.

## Interpretation and limits

Authored machinery introduces service turns. The old prototype test requiring
empty straight diagonals was replaced with swept-capsule connectivity; the new
regression additionally walks every target route with the real CharacterController.
Raised infrastructure is ignored by legacy planar checks only when its lower edge
clears the complete 1.4m controller. Gameplay target positions are not moved.

Internal captures are verification evidence, never a human approval gate. Android
device installation, sustained device FPS, speaker timbre and touch usability await
the owner's single APK review. No measured 60 FPS claim is made from Editor tests.
