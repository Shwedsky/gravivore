# v42 magenta device blocker — v43 rendering hotfix

Same branch and Draft PR #65; do not merge. Owner reports the entire frame magenta on the same Android device where v41 rendered correctly. v42 is invalid for acceptance. The next owner gate is rendering only, before gameplay review.

Investigation anchors: known-good v41 source `e788170b9bd80e12737eef83eeb8c322ee6f2afa`; v42 APK source `bc4b424907afffadfbc255aebea95903e9d4b01f`; resumed HEAD `4cbc5e11592d367154a3ed200d8b4d087d92ecef`, matching fetched branch.

Initial comparison: no tracked changes to ProjectSettings, Packages, URP settings or Android build code between those sources. Existing atlas and hostile-soft materials must not be blindly rewritten. Effective generated pipeline/renderer assets, packed shader resources, full-screen passes and all eight V3 prefab renderers require inspection.

Scope: rendering diagnosis, narrow correction, one-shot DEV diagnostics, focused regression and strengthened APK proof. Preserve gameplay, saves, equipment and V3 composition. Build a separate signed ARM64 IL2CPP DEV v43 APK; retain v42.

Required validation: compile, ProjectValidator, full EditMode, full PlayMode, Android v43 build and shader/material/dependency proof. Report whether actual Android execution is available; Editor rendering is not device proof.

## Evidence-based diagnosis

Generated `GraphicsSettings.asset` differs despite unchanged tracked render code: v41 retains the seven standard built-in always-included shaders; v42 has an empty list. Pipeline and renderer values match after normalizing generated GUIDs (HDR on, MSAA 1, render scale 1, forward, no renderer features, null post-process data).

A read-only SerializedFile format-22 inspector reassembles APK split chunks and checks actual class-48 Shader objects. Known-good v41 contains `UI/Default` (pathId 10770, 9,220 serialized bytes) and `UI/DefaultETC1` (10783, 9,136 bytes) in `Resources/unity_builtin_extra`. v42 contains neither shader object. Both versions have their names as runtime lookup strings in `globalgamemanagers`; those strings are not evidence of a packed shader.

`HudUiFactory` creates uGUI Images in code with implicit default materials, including screen-sized translucent modal backdrops. uGUI's `Graphic.defaultGraphicMaterial` calls `Canvas.GetDefaultCanvasMaterial()`. Without the packed UI shader, the error shader can render those backdrops as opaque magenta over the camera. A controlled Editor test reproduces this: a 1%-opacity full-screen Image with the error shader makes approximately 90% of the captured frame magenta (other HUD layers draw above it). A fully clear panel can be culled by Canvas and is not the reproduction used. Restoring the real UI shader is checked separately. This is a plausible explanation for the owner symptom, not a claim that an Android frame was captured here.

Narrow correction: deterministically retain only the two required runtime uGUI shaders on every project configuration/build. Do not disable URP stripping or rewrite scene materials. Add missing-UI-shader build validation and typed APK proof, plus the requested V3 renderer audit and DEV boot diagnostics.

Android execution availability: bundled adb reports no connected devices; no emulator executable, system images or AVD were found in the installed Android SDK/user configuration. Actual Android rendering remains the owner's first gate.

Unity's shader-loading documentation explains magenta fallback when the required compiled variant is unavailable: https://docs.unity3d.com/2023.2/Documentation/Manual/shader-loading.html. Inspector format was checked against the upstream SerializedFile implementation; no UnityPy package is installed or introduced.

## Render configuration and scope audit

The v41/v42 source diff for ProjectSettings, Packages, Editor configuration and Android build code is empty. The generated URP pipeline and renderer match after resolving their generated GUIDs. Both use Gamma, HDR with default buffer precision, MSAA 1, render scale 1, forward renderer, no renderer features, intermediate texture mode 1 and null renderer postProcessData. Android API selection remains automatic with Vulkan and OpenGLES3. Graphics lightmap/fog/instancing/BRG stripping values are 0 in both generated configurations; preloaded shaders and keyword overrides are empty; shader compile logging and pipeline variant log level are 0. No global stripping setting is changed by this hotfix.

The existing mobile-bloom content reference and camera setup remain unchanged: the runtime Chapter camera enables HDR and post-processing when its injected fidelity definition enables MobileBloom. No V3 camera-stack, blit, post-process material or renderer feature was added. HDR format support is included in the new DEV boot diagnostic; actual device format support remains observable only when the APK runs on Android. A null renderer postProcessData is shared with the known-good configuration and is not changed speculatively.

`AndroidBuild` temporarily changes applicationId/version/versionCode inside a try/finally, restores those three settings, and saves assets. ARM64/IL2CPP/portrait configuration is intentional. There are no temporary rendering mutations in this build path. The post-build settings and metadata are checked again for v43.

The eight V3 prefab audits check every shared material, shader/error/support, effective submesh coverage, finite world bounds and scale. Dedicated runtime checks reject camera-containing V3 bounds at player spawn. Hostile Charge/Travel/Impact pool construction is checked for zero active instances, particles and active line renderers. The scene build processor validates every serialized Renderer, including inactive objects, before writing build scenes. The atlas retains URP Lit with `_EMISSION` and `_METALLICSPECGLOSSMAP`; hostile soft retains URP Particles/Unlit, `_SURFACE_TYPE_TRANSPARENT`, queue 3000 and SrcAlpha/OneMinusSrcAlpha blending (5/10).

Static-batched renderers use their serialized firstSubMesh/subMeshCount subset of the combined mesh for material coverage. The audit validates that subset's range and effective material count; it does not incorrectly require each object to supply materials for the entire combined mesh. A regression checks valid subsets, invalid ranges and incomplete coverage of an ordinary mesh. The final build must include successful scene-audit entries for both canonical scenes.

Only the two runtime UI shaders are added to Always Included Shaders. Existing explicit inclusions are preserved; URP materials are left intact. A post-strip compiler observer records retained target variants for Vulkan and GLES3x. The typed APK inspector resolves material shader PPtrs to actual Shader objects and checks material keywords, alongside the build report's dependency and serialized-renderer audit. No fallback material substitution is used: the intended materials must remain valid.

Assumption: the demonstrated missing UI shader is the most plausible cause of the owner frame. The owner's same-device rendering gate is required to confirm the resulting APK on that GPU/API. Next spec remains the existing Chapter 01 V3 acceptance gate; first, rendering only.

## Final v43 verification and delivery

- APK source commit: `3aee32d13a8ce1a342371653d18859835a478415`. Following commits contain documentation/evidence only.
- Unity 6000.3.0f1 standalone compile and ProjectValidator: exit 0.
- Full EditMode: 469/469 passed, no skips. Full PlayMode: 134/134 passed, no skips, including the optional structure capture. PlayMode ran against the final runtime source at `7f083e9`; the later change affects only the Editor static-batch auditor and its passing EditMode regression.
- Final Android ARM64 IL2CPP DEV build: exit 0. Both scene audit entries are present: Bootstrap has 0 renderers; Chapter 01 has 898 renderers, including 93 V3, with no missing/error materials.
- Typed APK proof: both real UI Shader objects present; both real URP asset objects present; all eight V3 GameObject names present; compiled RenderingBootDiagnostics present. Atlas and hostile material PPtrs resolve to the intended real Shader objects, with their required keywords.
- Post-strip retained variants on each of Vulkan and GLES3x: UI/Default 4, UI/DefaultETC1 4, Lit with emission/metallic-map 17, transparent Particles/Unlit 2. Build-report dependencies, existing production/concept packing proofs and V3 runtime/equipment packing checks pass.
- Version 0.1.0, versionCode 43, package `com.gravivore.mobile.dev`. APK signature is valid and matches both actual v41 and v42 packages.
- Delivered APK: `C:/Users/pamak/Documents/ChatGPT/gravivore/Builds/Android/gravivore-dev-0.1.0+43.apk`.
- Size: **82,729,697 bytes**. SHA256: `ed1d778cf0108932d82151c66c30d608986d19efb472c25237e86c3262091ace`.
- Post-build PlayerSettings are restored to the configured candidate package `com.gravivore.mobile`, version 0.1.0, repository versionCode 1. The APK itself correctly retains DEV package/versionCode 43.
- The delivered v42 remains intact at its original SHA256 `efe69bc5c4e415dc05b0c3c4f23b30c1ff12d96107149699eb08f295d962f6c8`.

Verification artifacts: `docs/render-hotfix/verification/`. Changed source files: `source_files.txt`. The five-minute Editor route completed 16 stops with at most 20/25 live enemies, stable 54 shared materials and no non-actor transform growth. This is a regression smoke check, not Android performance evidence.

Actual Android execution: **not performed**; no connected hardware or installed emulator/AVD is available. No device capture or device FPS claim is made. PR #65 must remain Draft and unmerged. Install v43 on the same owner device; the first gate is **normal rendering with no magenta**. Gameplay acceptance follows only after that gate passes.
