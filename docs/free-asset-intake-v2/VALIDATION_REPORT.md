# V2 task validation — 2026-10-09

Task-scope validation passes with the explicit render/resource and runtime limits below. Machine-readable checks, input totals, sheet hashes and final Unity-log hash are in [VALIDATION_SUMMARY.json](VALIDATION_SUMMARY.json). This is a selection/evidence PR, not a production integration completion report.

## Executed checks

- Python unittest discovery: **30 tests PASS**, including traversal/Windows path rejection, case collisions, tar links, size limits, CRC/original preservation, conflicting ZIP variant preservation, binary material byte preservation, YAML texture-GUID preservation and spaced-name representative selection. The synthetic duplicate-ZIP fixture intentionally emits a duplicate-name warning. No owner input is removed by the test fixtures.
- Python compilation: all9 intake-tool `.py` modules compile. PowerShell AST parse: `run_zoo.ps1` passes. Unity6000.3.0f1 compiled the owned scratch editor script before the successful Inspect execution; no `error CS` diagnostic.
- Scratch Unity Inspect: **exit0**,1171 models measured,120 representative captures, categorized scene saved. This includes final declared glTF material binding and actual original model-name matching. URP17.3.0, no third-party executable intake code imported or run.
- Original inputs: **27 files / 5,000,627,772 bytes**, exact file set and SHA256/size match against the initial snapshot. Local raw downloads, extracted copies and ignored staging remain present.
- Git scope versus merged baseline: only `.gitignore`, owned `Tools/free-asset-intake/` tooling/config and `docs/free-asset-intake-v2/` reports/evidence change. Production Assets/, gameplay, ProjectSettings, scenes/prefabs, G-0, balance, saves, maps, camera and build settings unchanged. No raw mesh/texture/UI archive/package or Asset Store source file enters the commit.
- Import allowlist: source art in scratch is only FBX/PNG/material/meta data. Source scripts/shaders/graphs/prefabs/scenes/plugins/custom packages/Blender files excluded. Eight payload `.cs` files are catalogued in CODE_SAFETY_AUDIT.json, never executed. Unity/URP baseline package code and owned Editor code are expected framework code.
- Material integrity: all65 binary source material records retain their extracted bytes, and the preparer's binary copy path is byte-preserving. Unity upgraded two WarZone **scratch copies** to YAML on save; they are recorded explicitly rather than falsely claiming scratch outputs remain immutable.
- Resolution: all27 discovered payloads have a state;27 expected FOUND, one MISSING (`pipes-armored`), all11 Meshy SKIPPED_SUBSCRIPTION. Embedded source-format/pipeline archives are DUPLICATE variants and are not additional candidates.
- Selection: all26 actual art sources receive one decision; USE4/DONOR14/REJECT6/UNRESOLVED2. All USE candidates have confirmed production-compatible licenses. Weighted0–5 scores follow35/20/15/15/10/5 percent; item restrictions prevent pack-wide blanket acceptance.
- Completeness: all24 Chapter01 roles have CURRENT/PROBLEM/RECOMMENDED SOURCE/USE-DONOR-CUSTOM/EXPECTED GAIN/TECH RISK. One concrete stack covers all9 art roles and eight Meshy-related enemy/weapon gaps have named A/B/C/D routes.
- Evidence: all120 selected frames pass magenta screening. Every committed sheet/tile matches the actual local source/render hash. All8 final sheet pages visually inspected; aggregate size is approximately2MB. Raw source images/individual Unity renders stay ignored.

## Material limitations and assumptions

Source identity FOUND does not imply a resolved license: TechLab exact CC attribution version and Tiago/Icons8 mixed rights remain UNRESOLVED for production. Medium Striker is the actual MSGDI Asset Store package, not the expected V1 Sketchfab CC BY source; RTS assets are local free v1, not paid v3. Asset Store compiled-game permission assumes the owner's documented local acquisition and applicable EULA; raw/modified source redistribution remains prohibited. Any V3 use must retain private/owner-local payload handling.

Concept board is present and inspected, with the explicit approved bipedal G-0 override. Current B++ is the owner's assessment. Production comparison uses merged historical captures and builder/binding inspection, not a new device playtest. Selected previews normalize longest dimension to4m and use an elevated neutral camera; V3 must verify actual existing camera/scale and collision extents. Neutral source-map conversion is not native URP acceptance. Packed ORM channel conversion, material consolidation, animation retarget/playback, binary prefab hierarchies, exact runtime texture assignment and device performance remain separate V3 work.

Unity logs non-fatal package resource/type errors on AutodeskInteractive.shadergraph and TraceVirtualOffset.urtshader. The selected Lit captures render correctly and inspection exits0; unused graph/probe features are not validated. VFX is a **static serialized audit only**; source shaders/scripts/prefabs were not run. Particle caps are serialized maxima, not live-particle/performance measurements. These limits are present in the inventory and zoo report, not hidden as zero values.

## Checks intentionally outside V2

Production ProjectValidator, gameplay edit/play-mode suites and production compile were not launched: this task explicitly forbids production integration, and the art-only scratch project does not contain those systems. Intake tests and the V2 release gate cover the changed tooling/report behavior. No Android APK or device gate was required or run under the explicit current task; there is no APK path.

Next spec: **FREE ASSET INTAKE V3 — production integration plan**, selected after ChatGPT reviews Draft PR #67. The plan should prioritize shared surface vocabulary + one hub/floor/service prototype, then custom enemy/boss/weapon silhouettes, causal VFX and the EXE-based portrait UI.
