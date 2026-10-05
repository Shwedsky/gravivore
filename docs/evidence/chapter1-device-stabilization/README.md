# Chapter 1 device stabilization evidence

Runtime source: `01c6b1ad510f946de3349a60e76219142e8e948f`.
Baseline: `60b4035a2a1528a0211ff877e00cf206c0e607c4`.

All eight PNGs are actual Unity 6000.3.0f1 PlayMode renders of
`Chapter01_ScrapExclusion`, at 540 × 960. No image generation, compositing,
or painted attack shapes were used. CanonicalSceneTestScope supplies a fresh
temporary profile and controlled time. Tests stop autonomous movement and
advance authoritative controllers explicitly for deterministic encounter poses.
The production perspective camera keeps its authored field of view; the capture
helper places it at player + (0, 14, -10), looking at the player, and renders the
production HUD canvas through the camera into an offscreen render target.
These are editor evidence, not captures from a physical Android device.

- `01_russian_map.png`: expanded production map, repair node selected.
- `02_strong_spot_pre_elite.png`: three living strong enemies at strong-elite-a;
  both progression gates remain locked. Arrival uses CharacterController.Move
  with physical collisions enabled.
- `03_magnetar_telegraph.png`: authoritative radial warning, radius 3.2 m.
- `04_boss_cone_readability.png`: authoritative 6 m / 35° half-angle cone.
- `05_boss_line_readability.png`: authoritative 7 m × 1.8 m line corridor.
- `06_boss_circle_readability.png`: authoritative radius 4 m circle.
- `07_enemy_impact_updated.png`: actual nonlethal EnemyDamaged event, sole
  Phase6B HostileImpact cue, particle simulation advanced 0.12 s within its
  unchanged 0.18 s lifetime. Player health remains unchanged.
- `08_repair_hub_russian.png`: production repair node after controlled player
  damage and the authoritative repair delay; Russian HUD and map compass.
  The existing red player-damage pulse belongs to S14 PlayerHitPool, a separate
  event from the enemy-hit effect replaced in this pass.

Generation is opt-in with `GRAVIVORE_STABILIZATION_EVIDENCE=1` and the full
PlayMode suite. Capture implementation lives in
`Chapter1ConsolidatedRuntimeSmokeTests.Capture`; dedicated language, access,
and hit tests live in `Chapter1DeviceStabilizationSmokeTests`.
`verification.json` records counts, relevant test outcomes, PNG hashes, and hashes
of the raw logs/XML retained locally under `Builds/StabilizationLogs`.
`SOURCE_FILES.txt` lists every source/test/asset changed against the baseline.

Human review on the versionCode 3 APK is still required to judge the final
contrast, impact feedback, and text size on the target phone.
