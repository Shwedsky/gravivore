# Art Spike V2 verification — 2026-10-02

Unity **6000.3.0f1**. Same branch `codex/art-spike-kitbash`, starting V1 HEAD `42dfbe83d1a28127cc1bf1eb192dca2092dfe0c3`. Stacked base `codex/s20-balance-vertical-slice` at `4394448a7f3950cb62df701e8969137ae4b4a01c`.

## Executed checks

- Compile/import and final authoring/capture: **passed**, Unity process return code 0. No art-code compilation errors.
- Capture: **eight actual Unity URP PNGs**, Direct3D11 / AMD Radeon(TM) Graphics. All were visually inspected after final material/reflection/normal and composition refinements. 01/04/06/08 are 1920×1080; 02/03/05 are 1080×1920; 07 is 3240×1920.
- Full EditMode: **254 total, 254 passed, 0 failed, 0 skipped**. Existing assembly 244; isolated art-spike assembly 10. NUnit duration **3.4907456 seconds**. Raw result: [EditMode.xml](verification/EditMode.xml). The additional V2 test verifies Julius character donors, no Kenney character geometry and renderer count under 40.
- Full PlayMode: **56 total, 56 passed, 0 failed, 0 skipped**. NUnit duration **5.7800653 seconds**. Raw result: [PlayMode.xml](verification/PlayMode.xml).
- Standalone ProjectValidator: **passed**, after both suites. Log states `GRAVIVORE project validation passed.` and process return code 0. Sanitized evidence: [ProjectValidator.txt](verification/ProjectValidator.txt).
- Performance snapshot: regenerated with final prefabs; Tier 0/1/2 renderers **13/18/25**, Cutter **9**. Detailed counts and method: [PERFORMANCE.md](PERFORMANCE.md), adjacent JSON.
- Retained-source integrity: **14/14 source files** (nine FBX, OBJ, MTL and three licenses) match the downloaded original archives byte for byte in both the working tree and staged Git blobs. Archive SHA-256 and per-file hashes: [ASSET_MANIFEST.json](ASSET_MANIFEST.json). The isolated import's .gitattributes preserves vendor source line endings; project code/metadata use normal repository text handling.
- Production isolation: Unity-generated serialization was restored only in this art worktree. The staged/base audit allows ArtSpike assets and folder metadata, ThirdPartyNotices, five ART docs and docs/art-spike. Canonical runtime/content, production vendor assets, packages and ProjectSettings have no PR diff. Root checkout and other worktrees untouched.
- Android build: **not run**, explicitly unnecessary for this task. No APK.

## Warnings and evidence limits

Existing Unity bootstrap/BuildTargetGroup deprecation warnings and license/TLS service messages are not art failures. PlayMode logs contain intentional exception-path tests; actual NUnit XML has zero failures. Validator success and exit code are explicit. V1's initial test cleanup issue was corrected before its final pass; V2's complete suites passed with the final compositions.

Generated Unity YAML empty-field whitespace and original vendor notice whitespace are retained. No source geometry was changed to satisfy whitespace checks. PNGs are static art review images; neither the renderer snapshot nor desktop captures demonstrate Android FPS or human visual approval.

Full local logs are ignored under `Builds/ArtSpike/`: `v2-final-render.log`, `v2-editmode.log`, `v2-playmode.log`, `v2-validator.log`. Committed XML/excerpt omit local service/token logs.

## Reproduction

Use Unity 6000.3.0f1 with this worktree as `-projectPath`; do not run simultaneous Unity instances against it.

1. Author/render: `-batchmode -quit -executeMethod Gravivore.ArtSpike.Editor.ArtSpikeCapture.BuildAndCapture -logFile <capture-log>`; graphics device required, omit `-nographics`.
2. EditMode: `-batchmode -nographics -runTests -testPlatform EditMode -testResults <EditMode.xml> -logFile <edit-log>`.
3. PlayMode: `-batchmode -nographics -runTests -testPlatform PlayMode -testResults <PlayMode.xml> -logFile <play-log>`.
4. Validator: `-batchmode -nographics -quit -executeMethod Gravivore.Editor.ProjectValidator.ValidateProjectMenu -logFile <validation-log>`.

Fresh checkout bootstrap can generate local settings; keep those unrelated serialization changes outside an art-only commit. Use portrait Game view for interactive gameplay-scale review.

**ART SPIKE V2 VISUAL REVIEW: PENDING**
