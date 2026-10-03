# ART V3 — executed runtime preview verification

Date: 2026-10-03. Same branch `codex/art-spike-kitbash`, draft PR [#30](https://github.com/Shwedsky/gravivore/pull/30), base `codex/s20-balance-vertical-slice`. Required S20 SHA `9f744b1fc08d9ed25d5a8f818513922a9d9c3ea4` was verified and merged normally.

## Current compile and capture

Unity **6000.3.0f1 / URP 17.3.0** compiled the changed runtime/editor/test assemblies with zero C# compile errors. `ArtRuntimePreview.Bind` completed with return code 0: `Builds/ArtSpike/RuntimePreview/compile-bind.log`.

`ArtSpikeCapture.BuildAndCapture` regenerated all ten PNGs **before runtime integration**, using latest S20 offset (0,14.8,-11.2), FOV 46, look-at 0.9, damping 0.18, portrait 9:16. Log: `Builds/ArtSpike/RuntimePreview/capture.log`, return code 0. D3D11 / AMD Radeon(TM) Graphics. Close cameras/geometry did not change, so six regenerated images retain identical content hashes.

## Current full EditMode

**265 total, 265 passed, 0 failed, 0 skipped.** [Raw XML](verification/RuntimeEditMode.xml).
Log: `Builds/ArtSpike/RuntimePreview/editmode.log`.

Includes existing deterministic domain/architecture tests, 13 ArtSpike geometry/PBR/pivot/isolation cases, and seven added binding/configuration cases. These cover exact player/Cutter references, unchanged thresholds 20/42, other S15 parts/landmarks and boss/elite definitions, latest camera/build scenes, and rejection of incomplete/duplicate/physics/gameplay-authority whole forms.

## Current full PlayMode

**59 total, 59 passed, 0 failed, 0 skipped.** [Raw XML](verification/RuntimePlayMode.xml).
Log: `Builds/ArtSpike/RuntimePreview/playmode.log`; graphics enabled.

All previous canonical gameplay smoke tests ran. Three added real Chapter tests prove:

- At every score from 0 through 42, existing progression/presenter select exactly one G-0 form, crossing at 20 and 42. Legacy S15 body is absent. CharacterController retains radius 0.42, height 1.4, center (0,0.7,0); visual children contain no scripts/physics; DEV overlay exists.
- All four actual Cutting Floor enemies use V3 Cutter, retain 42 HP and root collision radius 0.4, preserve original PBR materials, and hide their visual after lethal damage/recycle.
- Actual Tier2 plus four population Cutters are inside the settled portrait frustum. VFX beam starts at presentation socket (0,0.68,1.15) and ends at the unchanged gameplay target. Counts: [RUNTIME_PERFORMANCE.json](RUNTIME_PERFORMANCE.json).

Legacy evolution module/accent and existing S14 VFX/audio failure/lifecycle tests remain in the full run. Intentional injected exceptions are expected; XML confirms passed outcomes.

## Current ProjectValidator

Standalone `Gravivore.Editor.ProjectValidator.ValidateProjectMenu`: **GRAVIVORE project validation passed.**, return code 0. Log: `Builds/ArtSpike/RuntimePreview/validator.log`. [Saved outcome](verification/RuntimeProjectValidator.txt). Android build also executes the validator.

## Current source, image and scope checks

[SOURCE_AND_IMAGE_CHECKS.json](SOURCE_AND_IMAGE_CHECKS.json): 14 original source files with working-tree/Git-index/manifest SHA-256 agreement, official MD5 where supplied, ten correct-size nonblank PNGs, zero maximum byte error in metallic R, inverted roughness alpha and AO. Five historical Julius GUIDs are absent.

Against exact latest S20, only S07 presentation references/offset and S15 Cutter override differ in Content. Stripping those fields yields identical legacy configuration (dominance order, tier modules/accents, enemy recipes, landmarks, materials). Remaining Content, ThirdParty, ProjectSettings, gameplay and persistence code are unchanged. Incidental Unity serialization is excluded; root/other worktrees preserved.

## Current Android Dev build

Existing `build-android.ps1 -Flavor Dev`: **succeeded, Unity exit code 0**. ARM64 IL2CPP, portrait, development/debugging enabled. Bootstrap and real Chapter are the only build scenes; ArtSpike bay excluded. APK: `Builds/Android/gravivore-dev-0.1.0+1.apk`, **57,966,186 bytes**. Code SHA: `232eed29bd33b157235501494a2b7bf44b0306eb`. Log: `Builds/Logs/android-dev-20261003-105531.log`. [Metadata/ZIP/packed-asset evidence](verification/RuntimeAndroidBuild.json). All four new form names are present in packed Unity data; comparison bay name absent; ZIP integrity and ARM64 IL2CPP library verified. aapt2 reports package com.gravivore.mobile, version 0.1.0 / 1, min SDK 26, target SDK 36.

No device launch/FPS/thermal measurements. Four shared ASTC 6×6 maps including mips estimate **2,505,152 bytes**; this is not measured residency. Static proxy movement, wide side-plate intersections and touch-play attack alignment remain human checks.

**ART V3 RUNTIME DEVICE REVIEW: PENDING**

---

# Historical isolated V3 verification (superseded by the runtime results above)

Date: 2026-10-02. Worktree: .codex-worktrees/art-spike-kitbash. Branch: codex/art-spike-kitbash, stacked on codex/s20-balance-vertical-slice (4394448a7f3950cb62df701e8969137ae4b4a01c). Same draft PR #30.

## Unity compilation and graphics capture

Unity 6000.3.0f1 compiled the changed ArtSpike editor and test assemblies successfully, with zero C# compile errors. ArtSpikeCapture.BuildAndCapture completed and returned code 0. Local final log: Builds/ArtSpike/v3-capture.log.

Actual graphics device: Direct3D11 / AMD Radeon(TM) Graphics. All ten requested PNGs were regenerated from the isolated scene; image 07 joins actual portrait panels and image 10 is a temporary exploded view. Capture-only visibility/transforms are restored. Final palette/foot placement were inspected in the actual outputs.

Unity CLI capture command:
Unity.exe -batchmode -quit -projectPath <art-worktree> -executeMethod Gravivore.ArtSpike.Editor.ArtSpikeCapture.BuildAndCapture -logFile <local-log>
No -nographics on this run.

## Full EditMode

**257 total, 257 passed, 0 failed, 0 skipped.**
[Raw XML](verification/EditMode.xml). Local log: Builds/ArtSpike/v3-editmode.log.

Includes 13 ArtSpike test cases checking character-only geometry, owned materials, unchanged common core/four supports, geometric tier evolution, distinct sole Cutter, renderer/triangle budgets, no share-alike/scenery mesh dependencies, PBR import settings, ARM packing, independent idle pivots, production catalog/scene isolation and exact S20 comparison camera.

Unity.exe -batchmode -nographics -projectPath <art-worktree> -runTests -testPlatform EditMode -testResults <local-xml> -logFile <local-log>
The test runner controls exit; no -quit.

## Full PlayMode

**56 total, 56 passed, 0 failed, 0 skipped.**
[Raw XML](verification/PlayMode.xml). Local log: Builds/ArtSpike/v3-playmode.log.

All existing canonical gameplay smoke tests were run. Intentional failure-path tests log their expected injected exceptions (including failed presentation/pool activation); XML outcomes are passed. They are not hidden or interpreted as unexpected test failures.

Same command as EditMode with -testPlatform PlayMode.

## Standalone ProjectValidator

Explicit **GRAVIVORE project validation passed.**, followed by application return code 0.
[Saved outcome](verification/ProjectValidator.txt).
Local log: Builds/ArtSpike/v3-validator.log.

Unity.exe -batchmode -nographics -quit -projectPath <art-worktree> -executeMethod Gravivore.Editor.ProjectValidator.ValidateProjectMenu -logFile <local-log>

Some runs log Unity service TLS certificate errors; the license connection, asset import, executed checks and exit outcomes completed successfully. No claim of error-free service logs is made.

## Art-only checks

- ArtSpikeAudit.WriteSnapshot executed during the final BuildAndCapture. Four JSON reports and PERFORMANCE.md are generated from the saved prefabs, not estimated from donor page counts.
- T0/T1/T2/Cutter renderers: 19/24/33/13, all within 30/35/40/25.
- Triangles: 5,928/6,936/9,216/3,352, all below 50,000.
- Four actually referenced 1K PBR maps shared across characters, project materials, mipmaps, linear mask/normal/AO imports and Android ASTC 6x6.
- Mechanical idle build/sample proof succeeded; ARTICULATION.json records four independent hips sampled at 0.75 seconds. Main prefabs remain free of Animator/Animation/colliders/lights/gameplay scripts.
- Julius raw files and metadata removed after the replacement prefabs/scene/images were successfully generated. Tests verify no current character dependency on Julius or Kenney scenery meshes.
- Imported original source SHA-256/MD5 checks and derived map/image checks are recorded in SOURCE_AND_IMAGE_CHECKS.json. No rejected archives or editor executables enter the commit.
- Production Assets/_Game/Content, Assets/ThirdParty and ProjectSettings tracked serialization produced by Unity bootstrap was restored within this art worktree after checks; production diff against the S20 base is zero. Untracked generated local settings are preserved and excluded. No root checkout/other worktree edits.
- Git scope limited to Assets/_Game/ArtSpike, art documentation/review evidence and ThirdPartyNotices.md.

## Build and scope limits

Android build deliberately **not run**, as specified for V3; no APK was produced. Structural counts and ASTC import settings do not establish Android FPS, texture residency or measured runtime draw calls.

Final geometry/UVs, gait/IK and production binding remain outside this temporary proxy proof. Human visual approval is still required.

**SHARE-ALIKE CHARACTER DEPENDENCY: NO**

**ART SPIKE V3 VISUAL REVIEW: PENDING**
