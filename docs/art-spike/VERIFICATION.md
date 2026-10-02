# ART SPIKE V3 — executed verification

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
