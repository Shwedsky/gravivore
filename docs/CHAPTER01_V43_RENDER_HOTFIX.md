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
