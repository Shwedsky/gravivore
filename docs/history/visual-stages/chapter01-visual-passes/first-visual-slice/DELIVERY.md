# First playable visual slice — APK v1

Base: `a0aacf33d8739d167e3dd81e94ef97efe1f333cc` (verified origin/main).
Branch: `integration/first-visual-slice-apk-v1`.

## Delivery contract

Integrate the merged G-0 Production V3 at its accepted 1.10 presentation scale,
original Scout/Cutter/Magnetar production visuals, and a dense industrial rebuild
of the Chapter01 pre-Magnetar approach and encounter. Preserve gameplay authority,
normal progression, saves, Russian UI, combat, audio, VFX, and repeatability.

The next human visual gate is the installable ARM64 DEV APK. There are no
intermediate human art gates. Internal validation and visual checks continue
through the Android build. Commit and push at each major checkpoint.

## Required release checks

- Compile and project validation.
- Relevant EditMode and PlayMode tests.
- Build dependency verification for all five production visual dependencies.
- Android ARM64 DEV APK, `com.gravivore.mobile.dev`.
- APK metadata, SHA256, size, exact path, and build/source revision recorded here.

## Delivered APK

Status: **PLAYABLE VISUAL SLICE APK READY**.

- Draft PR: https://github.com/Shwedsky/gravivore/pull/60
- APK source commit: `0df48be51af221ca69875b8917039fd863a97cc0`.
- Exact APK path: `C:\Users\pamak\Documents\ChatGPT\gravivore\.codex-worktrees\first-visual-slice-apk-v1\Builds\Android\gravivore-dev-0.1.0+36.apk`.
- Size: **64,880,064 bytes (61.87 MiB)**.
- SHA256: `fb9fd7a17cca69ffc8a9f5ae116cd5c456204de2ff66ebbe5674b92344ece67f`.
- applicationId: `com.gravivore.mobile.dev`.
- Version: `0.1.0`; versionCode: **36**.
- Unity: **6000.3.0f1 (6.3 LTS)**; DEV, IL2CPP, ARM64 only.
- APK signature: verified with Android `apksigner`; v2 signature valid.

## Validation executed

- Unity integration/compilation: exit 0.
- ProjectValidator.ValidateOrThrow: passed independently and again inside the successful Android build.
- EditMode: **405 passed, 0 failed, 0 skipped**.
- PlayMode: **97 passed, 0 failed, 1 optional capture skipped** (98 discovered).
  The skipped test is `CaptureStructuralFoundationWhenRequested`, which requires
  `GRAVIVORE_VISUAL_INTEGRATION_QA`; the visual slice's own runtime camera test ran.
- AndroidBuild.BuildDev: **Success, exit 0**.
- APK inspection: expected package/version, only arm64-v8a native libraries,
  ARM64 IL2CPP player, valid signature, and matching artifact SHA256.
- All five production dependencies are reachable from Chapter01. The actual APK
  contains exact serialized G0/Scout/Cutter/Magnetar LOD0 names in shared asset
  data and the environment root/reactor/decks in Chapter01 scene data.
  `apk_packed_dependencies.json` and `verification/apk_verification.json` bind this
  evidence to the delivered APK hash.

The first build failed during symbol stripping because C: ran out of space.
NTFS compression of this worktree's generated compiler cache recovered space;
no source or root-checkout files were removed. A second attempt produced an APK
but was rejected by the initial source-path check because Unity's incremental
report omitted cached asset entries. The final verifier inspects serialized APK
content directly. Both failure logs and the successful final log are retained.

## What changed

- Approved G-0 Production V3 is the live player at the frozen 1.10 fit, with
  movement/attack/hit/death presentation observing authoritative gameplay.
- Original Scout V1, Cutter V1 and Magnetar V1 replace their live visual bindings.
  Each has a mechanical rig, five in-place clips, three LODs and one opaque atlas
  material per active LOD. Eight preallocated shutdown visuals preserve ordinary
  enemy death motion after immediate authoritative recycling/rewards.
- The existing Cutting Floor approach, western strong pre-elite spot, elite gate
  and Magnetar arena form the rebuilt section. Existing encounter coordinates,
  normal access requirements and progression remain in use.
- Replaced floors, bulkheads, barriers and gate leaves; added the hero containment
  reactor, power banks, coolant pumps, freight stacks, conduit racks, maintenance
  stations and structural supports. Solid machinery/walls use authored proxies
  in the existing collision layer. Art prefabs remain collider-free.
- Removed overlapping old corridor surfaces and trimmed the neighboring boss
  approach deck with a local prefab variant. Key light/ambient settings provide
  coherent industrial lighting without extra realtime lights.
- Existing Russian UI, combat, VFX/audio, saves, rewards, gate logic and repeatable
  encounters retain their gameplay authority; relevant regression suites passed.

## Changed files and sources

The complete committed file list is `changed_files.txt`. Main groups:

- `Assets/_Game/Content/VisualSlice/`: FBXs, atlas textures/materials, controllers,
  player/enemy/environment prefabs and the boss-approach boundary variant.
- Chapter01 scene and S07/S15/Chapter01 visual binding assets.
- `VisualSliceAnimationBridge.cs`, composition/world/presentation integration,
  editor builder, Android build report options and dependency/APK verification.
- Updated EditMode/PlayMode coverage and saved verification evidence.
- `Tools/first-visual-slice/` and editable `art/first-visual-slice/` Blender sources.

New enemy/kit geometry and atlas are original project work. G-0 retains its merged
source attribution and approved geometry. The dirty root checkout was preserved.

## Limitations and assumptions

- Device FPS, touch feel and final visual acceptance have not been measured on a
  phone. The rest of Chapter01 retains its existing art.
- G-0 uses the accepted geometry at all three progression tiers, with restrained
  energy variation; tier progression and legacy fallback definitions remain.
- Shutdown visuals are bounded to four Scout and four Cutter slots; saturated
  slots skip extra cosmetic deaths without delaying gameplay recycling.
- The smallest connected existing section was selected around Cutting Floor,
  the western strong spot and Magnetar. No encounter coordinates were moved.

The final PR checkpoint adds delivery documentation/evidence after the APK source
commit; it does not change the APK's runtime source.

Next owner gate: **INSTALL APK ON DEVICE AND PLAY IT.**
No further art phase is started before that device review.
