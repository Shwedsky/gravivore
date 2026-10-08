# v42 magenta device blocker — v43 rendering hotfix

Same branch and Draft PR #65; do not merge. Owner reports the entire frame magenta on the same Android device where v41 rendered correctly. v42 is invalid for acceptance. The next owner gate is rendering only, before gameplay review.

Investigation anchors: known-good v41 source `e788170b9bd80e12737eef83eeb8c322ee6f2afa`; v42 APK source `bc4b424907afffadfbc255aebea95903e9d4b01f`; resumed HEAD `4cbc5e11592d367154a3ed200d8b4d087d92ecef`, matching fetched branch.

Initial comparison: no tracked changes to ProjectSettings, Packages, URP settings or Android build code between those sources. Existing atlas and hostile-soft materials must not be blindly rewritten. Effective generated pipeline/renderer assets, packed shader resources, full-screen passes and all eight V3 prefab renderers require inspection.

Scope: rendering diagnosis, narrow correction, one-shot DEV diagnostics, focused regression and strengthened APK proof. Preserve gameplay, saves, equipment and V3 composition. Build a separate signed ARM64 IL2CPP DEV v43 APK; retain v42.

Required validation: compile, ProjectValidator, full EditMode, full PlayMode, Android v43 build and shader/material/dependency proof. Report whether actual Android execution is available; Editor rendering is not device proof.

## Evidence-based diagnosis

Generated `GraphicsSettings.asset` differs despite unchanged tracked render code: v41 retains the seven standard built-in always-included shaders; v42 has an empty list. Pipeline and renderer values match after normalizing generated GUIDs (HDR on, MSAA 1, render scale 1, forward, no renderer features, null post-process data).

A read-only SerializedFile format-22 inspector reassembles APK split chunks and checks actual class-48 Shader objects. Known-good v41 contains `UI/Default` (pathId 10770, 9,220 serialized bytes) and `UI/DefaultETC1` (10783, 9,136 bytes) in `Resources/unity_builtin_extra`. v42 contains neither shader object. Both versions have their names as runtime lookup strings in `globalgamemanagers`; those strings are not evidence of a packed shader.

`HudUiFactory` creates uGUI Images in code with implicit default materials, including transparent screen-sized input panels. uGUI's `Graphic.defaultGraphicMaterial` calls `Canvas.GetDefaultCanvasMaterial()`. Without the packed UI shader, the error shader can render those transparent panels as opaque magenta over the camera. This directly explains the entire-frame symptom while the intended Lit/hostile materials remain unchanged.

Narrow correction: deterministically retain only the two required runtime uGUI shaders on every project configuration/build. Do not disable URP stripping or rewrite scene materials. Add missing-UI-shader build validation and typed APK proof, plus the requested V3 renderer audit and DEV boot diagnostics.

Android execution availability: bundled adb reports no connected devices; no emulator executable, system images or AVD were found in the installed Android SDK/user configuration. Actual Android rendering remains the owner's first gate.

Unity's shader-loading documentation explains magenta fallback when the required compiled variant is unavailable: https://docs.unity3d.com/2023.2/Documentation/Manual/shader-loading.html. Inspector format was checked against the upstream SerializedFile implementation; no UnityPy package is installed or introduced.
