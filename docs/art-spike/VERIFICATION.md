# Art spike verification — 2026-10-02

Baseline: `codex/s20-balance-vertical-slice` at `4394448a7f3950cb62df701e8969137ae4b4a01c`. Isolated branch: `codex/art-spike-kitbash`. Unity: **6000.3.0f1**.

## Executed checks

- Compile/import: successful. Initial donor probe completed; final authoring/capture compiled and Unity terminated with return code 0. No art-script compilation errors.
- Actual scene capture: successful on **Direct3D11 / AMD Radeon(TM) Graphics**. Seven PNGs rendered. `01`, `04`, `06`: 1920×1080; `02`, `03`, `05`: 1080×1920; `07`: 3240×1920. The final renders were visually inspected after correcting first-frame material initialization and isolating review areas.
- Full EditMode suite: **253 total, 253 passed, 0 failed, 0 skipped**. Existing assembly: 244; new isolated art-spike assembly: 9. NUnit duration 3.343702 seconds. Raw final results: [EditMode.xml](verification/EditMode.xml).
- Full PlayMode suite: **56 total, 56 passed, 0 failed, 0 skipped**. NUnit duration 7.5372221 seconds. Raw results: [PlayMode.xml](verification/PlayMode.xml).
- Standalone ProjectValidator: **passed**. Unity logged `GRAVIVORE project validation passed.` and terminated with return code 0. It was run separately after both suites. Evidence excerpt: [ProjectValidator.txt](verification/ProjectValidator.txt).
- Imported source integrity: **18/18 retained FBX files match the official archives byte for byte**. SHA-256 values and archive hashes: [ASSET_MANIFEST.json](ASSET_MANIFEST.json).
- Production diff isolation: canonical runtime, content, existing vendor assets, packages, and ProjectSettings were restored to the pinned baseline after Unity's automatic serialization. The staged path audit allows only `Assets/_Game/ArtSpike`, its folder metadata, `ThirdPartyNotices.md`, the five requested `docs/ART_*.md` files, and `docs/art-spike/`. No canonical runtime content or production scene/catalog binding is included in the PR.
- Android build: **not run**, explicitly excluded by the art-spike task. No APK produced.

## Test correction and warnings

The first EditMode run passed 252/253. Its only failure was the new review-scene test's teardown attempting to restore a fresh batch session with no loaded scene. The new test cleanup was corrected; the full suite then passed. Existing tests were not changed to accommodate the spike.

Fresh Unity import emitted five existing CS0618 warnings in AndroidBuild, ProjectValidator, and AndroidBuildTests for older BuildTargetGroup APIs. Initial headless package/shader import logged URP fallback-shader notices and automatic global-settings creation. Final art authoring/capture introduced no donor/model import or art-code warnings.

Unity's validator log also contains license-client token-refresh and TLS service messages, followed by successful entitlement resolution and the explicit validation success/zero process return. PlayMode includes deliberate exception-path logs from existing tests; its final XML has zero failures. These messages are not silently counted as failed or passed tests: the suite/validator outcomes above come from their actual results.

Unity serialization produces empty-field whitespace in generated `.meta`, `.mat`, `.prefab`, and scene files; the original bundled license whitespace is preserved. This is not a C# compile or model-import warning. No vendor source geometry was edited to remove it.

Local full command logs are retained under the ignored `Builds/ArtSpike/` directory: `01-import.log`, `03-final-capture.log`, `05-editmode-final.log`, `06-playmode.log`, and `07-validator.log`.

## Reproduction

Use Unity `6000.3.0f1` with the isolated worktree as `-projectPath`.

1. Author/render: `-batchmode -quit -executeMethod Gravivore.ArtSpike.Editor.ArtSpikeCapture.BuildAndCapture -logFile <capture-log>`; a graphics device is required, so omit `-nographics`.
2. EditMode: `-batchmode -nographics -runTests -testPlatform EditMode -testResults <EditMode.xml> -logFile <edit-log>`.
3. PlayMode: `-batchmode -nographics -runTests -testPlatform PlayMode -testResults <PlayMode.xml> -logFile <play-log>`.
4. Validator: `-batchmode -nographics -quit -executeMethod Gravivore.Editor.ProjectValidator.ValidateProjectMenu -logFile <validation-log>`.

Do not run multiple Unity instances against the same worktree. Fresh checkout bootstrap tooling creates local URP/player settings; exclude its unrelated serialization changes from an art-only commit. If using the comparison scene interactively, choose a portrait Game view to match gameplay-scale screenshots.

**ART SPIKE VISUAL REVIEW: PENDING**
