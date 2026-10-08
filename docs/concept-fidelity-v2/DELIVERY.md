# Chapter 01 concept fidelity V2 — v41 delivery

Final artifact: `Builds/Android/gravivore-dev-0.1.0+41.apk` (76.52 MiB).
Draft PR: https://github.com/Shwedsky/gravivore/pull/64
Branch: `art/chapter01-concept-fidelity-v2`.
Accepted main baseline: `5999bffd0eb4aa5b291f2347aa4c2704ae98af45`.
Built source: `e788170b9bd80e12737eef83eeb8c322ee6f2afa`.
Subsequent delivery commits contain verification tooling and report files only;
Android runtime source and production assets remain those of the built commit.

## What changed

- One continuous authored industrial route connects repair/spawn, the five
  ordinary sectors, industrial transitions, elite approach, Magnetar,
  containment threshold and Custodian. Side connections use the same worn deck.
  Existing chapter art remains outside the selected presentation coverage.
- Eighteen original Blender meshes replace visible temporary hero art: irregular
  fitted deck, curved services, bowed bulkheads, pressure machinery, wrecks and
  containment frames. Open pressure cradles expose luminous energy behind cages.
  Sparse patches, service openings and scorch interrupt the surface pattern.
- The repair station uses an authored platform, servo joints, articulated arms,
  tool heads, hoses and a moving scanner. Healing and repair-state authority remain
  with the existing health/repair systems.
- Custodian V2 is a new 12-bone containment machine with 89 authored mechanical
  parts, asymmetric lance/clamp, split curved shells, articulated supports and
  a recessed power chamber. Its three LODs contain 16,092 / 8,366 / 3,862 triangles.
  Authored idle, movement, windup, release, special, hit and shutdown presentations
  observe the existing boss controller; they do not own root motion or damage.
- One new shared opaque atlas separates painted metal, dark mechanics, steel,
  worn surfaces, rubber, energy and scorch through color, metallic and smoothness
  maps. Recessed HDR energy, restrained bloom, two reused unshadowed local lights,
  rotating machinery and one prewarmed intermittent spark instance add local life.

## Files changed

The exact list is in [changed_files.json](changed_files.json).
Principal production files and groups:

- `art/concept-fidelity-v2/`: eighteen editable `.blend` sources.
- `Assets/_Game/Content/ConceptFidelityV2/`: eighteen FBX imports and safe visual
  prefabs, four animation controllers, three 512-pixel atlas maps, shared material,
  bloom profile and presentation configuration.
- `Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity` and
  `Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset`: canonical
  placement, lighting and new boss/hub binding.
- The existing `Chapter01_IndustrialAtlas.mat` and `Slice_IndustrialAtlas.mat`:
  HDR emission response for the accepted actors.
- `ConceptFidelityBuilder.cs`, `ConceptFidelityDependencies.cs` and
  `Chapter01ProductionDependencies.cs`: deterministic import/integration,
  source dependency validation and independent APK archive checks.
- `ConceptFidelityDefinition.cs`, `FidelityAtmospherePresenter.cs`,
  `RepairManipulatorPresenter.cs`, `RepairHubProductionPresenter.cs`,
  `S01SceneCompositionRoot.cs`, `CustodianPresentationSelector.cs` and
  `VisualSliceAnimationBridge.cs`: presentation configuration, bounded pools,
  injected bindings and authority-observing animation.
- Two assembly definitions reference the already-installed URP/Core assemblies;
  no package or paid dependency was introduced.
- `ConceptFidelityPresentationTests.cs` and `ConceptFidelitySmokeTests.cs`, plus
  four existing integration test files updated for the canonical V2 bindings.
- `Tools/concept-fidelity-v2/`: Blender production/validation scripts, Unity
  runner and APK verifier. `docs/concept-fidelity-v2/` contains review captures,
  source budgets, provenance, test results and archive evidence.

## Tests and validation actually executed

- Blender 5.2.2 LTS authoring/export succeeded. All eighteen saved sources were
  reopened and validated for geometry, UVs, material count, pivots, rigs,
  skin weights, LOD budgets and required boss clips.
- Unity 6000.3.0f1 compiled successfully and integration validation passed.
- Full EditMode suite: **445 passed, zero failures**.
- Full PlayMode suite: **124 passed, zero failures, one skipped**. The skip is
  the existing opt-in structural capture test requiring
  `GRAVIVORE_VISUAL_INTEGRATION_QA`; gameplay tests were not skipped.
- Focused post-review PlayMode rerun: **3 passed, zero failures**. It verifies
  actual imported shell motion and core shutdown, repair authority/pool bounds,
  flat deck orientation and route/pose camera captures. Manual pose capture
  accounts for batch-mode animator/skinning culling; production culling stays on.
- Five-minute real locomotion/combat regression: **300.0006 seconds**, **17 stops**,
  maximum **20 live ordinary enemies** against the configured cap of 25.
  New presentation transforms stayed **395 → 395** and all-slot shared material
  count stayed **53 → 53**. The total transform increase **4254 → 4634** is exactly
  the accepted actor-art cache increase **644 → 1024**; non-actor objects stayed
  fixed. Save flushes passed. Cold-start diagnostics finished their bounded
  90-second observation window and their report was preserved.
- `ProjectValidator.ValidateOrThrow` passed. Source comparison against the accepted
  baseline finds no changes to gameplay, persistence, map or development diagnostic
  code. No damage/stat/reward, gate, quest, wave-timer or save-schema rebalance.

Results and logs are under [verification](verification/). Portrait camera captures
are under [internal](internal/). See [PRODUCTION_NOTES.md](PRODUCTION_NOTES.md) for
asset provenance, camera-review details and runtime budgets.

## Android build and APK verification

Unity's Android Dev build succeeded, followed by independent SDK/archive checks:

- Package `com.gravivore.mobile.dev`, version `0.1.0`, version code **41**.
- **ARM64 only**, **IL2CPP**, development build and script debugging enabled.
- **80,237,135 bytes**; SHA-256
  `11815f91485980f97669b9187d38ef6395266ced6342ce868468bdcf07c3f0fa`.
- APK signature verified; certificate matches v40. Existing package identity and
  saved progression remain compatible at the source/signing level.
- All 18 new meshes, fidelity config, atlas and route root were found as
  length-prefixed serialized names in the final archive. Complete chapter,
  accepted visual/music and accepted audio packing evidence matches this APK hash.
- Compiled Russian UI, characteristics/boss HUD/readability/diagnostic types and
  the named serialized production movement/footstep settings were independently
  verified in the archive.

The first build attempt failed while copying IL2CPP debug symbols because the
host drive ran out of space. Lossless NTFS compression of Unity caches recovered
space; the retry succeeded. Source files and earlier APKs were preserved.
The failed attempt and successful build logs are retained separately.

Reproduce the final verifier with:

```powershell
.\Tools\concept-fidelity-v2\verify_apk.ps1 -ExpectedSourceSha e788170b9bd80e12737eef83eeb8c322ee6f2afa
```

## Known limitations and assumptions

- Android installation, device FPS, GPU draw calls/overdraw and final visual
  acceptance were not measured or claimed. This APK is the next owner device gate.
- The approved image board was not supplied and was not found in the repository.
  This pass follows the detailed written concept direction and preserves the
  accepted G-0 and enemy family.
- The five-minute test uses development gate unlock and god mode to cover every
  space with real locomotion/combat. Production progression requirements remain
  unchanged; a fresh-save boss unlock is not promised within five minutes.
- Instrumented editor frame gaps include test pathfinding/capture work and are
  not Android frame times. The previous device-only first-launch slowdown has
  no newly established cause here; its existing diagnostics remain intact.
- The entire chapter is coherent and playable, with higher authored density
  concentrated along the selected route. The baseline outside that coverage is
  retained, as requested.

Next gate: **final v41 concept-fidelity APK device review**. No next spec ID was
defined for this art milestone; Chapter 02 and broader progression work remain
outside its scope. Draft PR #64 remains open for that final acceptance gate.
