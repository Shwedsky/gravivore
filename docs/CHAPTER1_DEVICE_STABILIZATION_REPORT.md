# Chapter 1 device stabilization

Branch: `integration/chapter1-runtime-consolidation`.
Baseline: `60b4035a2a1528a0211ff877e00cf206c0e607c4`.
Verified runtime / APK source: `01c6b1ad510f946de3349a60e76219142e8e948f`.

## Russian audit

English leaked through the five ordinary map IDs, six equipment display names,
Latin compass `N`, the `DEV` button and metrics (`FPS`, `ms`, `HP`, stat initials,
boss enum names, session `s`), the menu's `II`, and Latin `M` in the boss name.
All production Chapter01 strings now use Russian text or neutral symbols.

Map zone names preserve established quest terminology: Релейный двор,
Разделочный цех, Свалка щитов, Поле конденсаторов, Кладбище тягачей.
Strong packs use «Усиленная зона», the repair marker «Ремонтный узел», the
compass «С», developer toggle «ТЕСТ», menu «МЕНЮ», boss «Хранитель М-0»
with Cyrillic М. Cooldown details use «Доступно через …»; repeat rewards use
«Повышенная награда» / «Базовая награда», retaining first-clear distinctions.
Selected-marker details allow larger 18–22 point text in a taller panel.

Equipment display strings replaced:

- Gravitic Fang → Гравитационный клык.
- Layered Carapace → Многослойный панцирь.
- Pulse Capacitor → Импульсный конденсатор.
- Impact Frame → Ударная рама.
- Flux Vanes → Лопасти потока.
- Vector Fins → Векторные стабилизаторы.

Item IDs, stats, ownership, and equipment save values are unchanged. Invalid
stat/slot fallbacks also return Russian labels instead of enum identifiers.

Changed language files: RussianUiText, Chapter1WorldMarkerMapAdapter,
CurrentWorldMarkerMapAdapter, MapMinimapPresenter, PauseMenuPresenter,
DevelopmentDebugOverlay, the six S10 equipment assets, and the S11 onboarding
quest asset. The runtime audit checks all Text components, including hidden
surfaces, all selectable map markers, every equipment item in the menu,
developer metrics, and real encounter cooldown/repair states. Existing HUD,
combat reward, quest guidance, offline duration/reward, and completion formats
were inspected; their established Russian terminology is retained.

## Strong spots

The reported empty pre-elite pack is `strong-elite-a`, at (-18, 0, 55).
The elite boundary is z=60; this pack is physically reachable with a fresh
profile. `strong-elite-b`, at (18, 0, 64), is behind that boundary.
`strong-boss-a`, at (-18, 0, 94), and `strong-boss-b`, at (18, 0, 92.25),
are behind the elite and boss (z=80) boundaries. WorldGateView builds permanent
left/right walls alongside each narrow gate opening, so those three packs
cannot be reached around a closed gate. No walls, gates, or pack positions moved.

Old rule: proximity AND EliteGateUnlocked/BossGateUnlocked, selected by region.
New rule for all four: proximity only, using the existing authored enemy
release radius plus minimum player distance. Physical gates govern traversal.
An accessible pack no longer waits for progression; nearby enemies can be
visible on the other side of a genuine boundary. Distant packs stay pooled.

Unchanged health/damage/reward multipliers and base respawn times:

- strong-elite-a: 1.3 / 1.2 / 1.75, 14 s.
- strong-elite-b: 1.4 / 1.25 / 2.0, 16 s.
- strong-boss-a: 1.55 / 1.35 / 2.25, 16 s.
- strong-boss-b: 1.7 / 1.45 / 2.75, 19 s.

The existing independent pressure policy, three-enemy pack size, respawn
timers, safe center route, encounter clearance, and global cap of 25 remain.
The dedicated collision test reaches strong-elite-a with both unlocks false,
finds three stronger enemies and an available map marker, then confirms the
elite side wall still blocks traversal. The consolidated regression test
checks admission of all four packs without unlock prerequisites, reward
scaling, independent pressure, respawn, repair, and the cap.

## Telegraphs

Only the four Phase6B warning prefabs and their presentation renderer changed:
solid texture-free amber contour, width 0.14 m, RGBA (1, 0.48, 0.12, 0.95),
with a translucent interior at alpha 0.18. The contour has a restrained
80–100% emphasis pulse, holding contrast until the final 5% fade. A shared URP
unlit transparent material has depth writes off; the contour renders after the
fill. No lights or gameplay colliders were added.

Phase6BWarningFill prewarms one small mesh per pooled warning instance:
33 vertices for circles, 23 for the cone, 5 for the line. Cached arrays and a
MaterialPropertyBlock are reused; no mesh/material creation occurs per frame.
The mesh boundary copies the existing LineRenderer contour. Replay reuses the
same mesh, and immediate cancellation hides it with the contour.

Gameplay geometry/timing remains authoritative and unchanged:

- Magnetar: radius 3.2 m, 1 s warning.
- Custodian circle: radius 4 m, 1 s warning.
- Custodian cone: range 6 m, half-angle 35°, 1 s warning.
- Custodian line: length 7 m, width 1.8 m, 1.1 s warning.

The unchanged production bridge supplies event origin/direction/dimensions and
duration. Existing ground lift is 0.35 m, with the existing contour offset;
warnings still clear the authored deck. Domain calculations and damage
resolution were not edited. Tests verify contour/fill agreement, exact
dimensions/direction, duration, unchanged HP during warning, and reset cleanup.
All four warning shapes were inspected in portrait Chapter01 captures.

## Red effect

The enemy-hit path was traced and reproduced in the real Chapter01 runtime:

`OrdinaryEnemyController.ApplyDamage` → `EnemyPopulation.EnemyDamaged`
→ `Phase6BCombatProductionBridge.EnemyDamaged` (nonlethal only)
→ `GravityLashVfxPool.PlayEnemyHit` → `Phase6BVfxCue.HostileImpact`
→ `PFX_HostileImpact.prefab`.

Baseline HostileImpact used a 14-particle spherical billboard burst, warm
red/orange (1, 0.34, 0.12, 0.82), material `M_Phase6B_HostileSoft.mat`, and
`Phase6B_SoftDisc.tga`. This is the soft disc enemy-hit presentation replaced
here. The runtime test proves a nonlethal hit activates only HostileImpact,
with no player damage, death event, or legacy EnemyHit/EnemyDeath pulse.

Updated HostileImpact uses 10 directional cone sparks / energy fragments,
size 0.12, speed 4 m/s, amber (1, 0.68, 0.28, 0.8), stretched particle rendering,
and the existing `M_Phase6B_PlayerStreak.mat` / `Phase6B_Streak.tga`. Sparks
emit toward the incoming attack source for visible surface feedback. The
0.18 s lifetime and pooled limits remain; no realtime light was introduced.
Generic spark defaults preserve other cues. This shared HostileImpact prefab
also serves encounter resolution; no other impact/death prefabs changed.

Lethal enemy events instead go through `EnemyDied` → `PlayEnemyShutdown`
→ `MechanicalKillBurst` → `PFX_MechanicalKillBurst.prefab`, a cyan 22-particle
burst using `M_Phase6B_PlayerSoft.mat`; this was preserved. Gravity Lash
PlayerImpact and its travel smoke were also preserved. S14 PlayerHitPool is a
separate existing red pulse caused by damage to the player, visible in the
deliberately damaged repair capture; it is not emitted by the isolated enemy
hit. The second device review should confirm the reported hit complaint is
resolved with this distinction visible.

## Unity

Unity 6000.3.0f1 final verification:

- Compile: PASS; no compiler errors.
- ProjectValidator.ValidateProjectMenu: PASS.
- Full EditMode: 386 passed / 0 failed / 0 skipped.
- Full PlayMode: 92 passed / 0 failed / 1 intentional skip, 93 total.
- Skip: VisualIntegrationSmokeTests.CaptureStructuralFoundationWhenRequested,
  an opt-in structural capture requiring GRAVIVORE_VISUAL_INTEGRATION_QA.
- Three dedicated stabilization PlayMode tests and four warning geometry
  EditMode cases passed. Actual Chapter01 produced all eight requested PNGs.

Final diff against 60b4035 covers 32 source/test/asset files. Gameplay,
persistence/save, encounter reward/repeat logic, audio assets, main, source
PR branches, and VisualReplacementProofV1 were not changed. No new package
or third-party asset was added. Unity import normalization and generated
performance data were excluded from the commit. The original dirty checkout
was not used for edits or verification.

See [source files](evidence/chapter1-device-stabilization/SOURCE_FILES.txt),
[verification metadata](evidence/chapter1-device-stabilization/verification.json),
and [capture provenance](evidence/chapter1-device-stabilization/README.md).

## Android DEV

Build: PASS, DEV ARM64 IL2CPP, development build and debugging enabled.

- APK: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\chapter1-runtime-consolidation\Builds\Android\gravivore-dev-0.1.0+3.apk`.
- Size: 59,228,545 bytes / 56.48 MiB.
- SHA256: `D3A566ACEC6FF1D03516A14B1905E1729B397BF467FC6ED4D7D6127D77B0CE3E`.
- applicationId: `com.gravivore.mobile.dev`.
- versionCode: 3; versionName: 0.1.0.
- Git HEAD at build: `01c6b1ad510f946de3349a60e76219142e8e948f`.
- Unity SDK aapt 36.0.0 verified package, versionCode, debuggable manifest,
  and ARM64-only native code. Independent ZIP inspection confirmed
  `lib/arm64-v8a/libil2cpp.so`, `libunity.so`, and no other architecture.
- Build log: `Builds/Logs/android-dev-20261005-172849.log`.

[Build metadata and hash](evidence/chapter1-device-stabilization/android-dev.json)
record the actual artifact and source. The subsequent report/evidence commit
changes documentation only; the APK runtime remains identical to final HEAD.
The APK is retained locally under Builds/Android, outside Git tracking.

## Device review checklist

- Install DEV versionCode 3 over versionCode 2; confirm existing progress loads.
- With a fresh profile, approach (-18, 0, 55) before elite unlock: three strong
  enemies should already spawn, and the map should show an available strong zone.
- Check HUD, minimap, expanded map, each POI detail, cooldown/reward text,
  equipment menu, boss name, and repair hub for Russian-only text and clipping.
- Judge Magnetar circle and Custodian cone/line/circle against the dark deck in
  portrait, including the warning edge near resolution and cancellation/reset.
- Hit and kill enemies: look for brief amber fragments on hits, preserved cyan
  shutdown feedback, and the absence of the previous soft red enemy-hit disc.
- Confirm safe traversal, cap, strong difficulty/respawn, repeat cooldowns,
  rewards, repair, restart, and resume remain correct on the phone.
- Reassess Gravity Lash release strength: this pass preserves the audio pack;
  the previously reported weak sound was outside the requested implementation
  checkpoints. The map's full visual redesign also remains deferred.

Known limitation: captures and functional checks are editor evidence. Phone
contrast, frame time, audio perception, and the final product verdict require
human review of this APK. No second device-review PASS is claimed.
Assumptions: proximity pooling remains appropriate for accessible packs; zone
translations preserve existing quest names; only the traced enemy-hit cue is
replaced. The initial assumption that side routes bypassed gates was corrected
after collision testing exposed their permanent flank walls.
Next checkpoint: second Chapter 1 device review; no new gameplay spec was opened.

CHAPTER 1 DEVICE STABILIZATION: READY FOR SECOND DEVICE REVIEW
