# v43 rendering hotfix verification

Run from the existing Chapter 01 V3 worktree with Unity 6000.3.0f1 and its Android support. `run_unity.ps1` opens Unity hidden with a real graphics backend, and runs Compile, Validate, EditMode, PlayMode or Build. Build produces a separate DEV ARM64 IL2CPP APK with versionCode 43; it does not regenerate V3 content. Set `GRAVIVORE_VISUAL_INTEGRATION_QA` to an output directory when running full PlayMode to execute the existing optional structure capture test.

After building, run `verify_apk.ps1 -ExpectedSourceSha <committed-build-source>`. It checks manifest, signature against v41/v42, provenance, architecture, existing gameplay packing proofs, actual typed rendering objects, material shader references/keywords and retained Vulkan/GLES3x target variants. The source SHA must match the build metadata; do not commit during the Unity build.

`serialized_apk.py` reads Android SerializedFile format 22 without installing a dependency. It reassembles split APK entries and examines class-48 objects rather than accepting arbitrary shader-name strings. `inspect_rendering.py` rejects v42 because the two real UI shader objects are absent. It also checks packed URP asset objects and resolves the industrial atlas / hostile-soft material PPtrs to their actual shader objects.

Unity's editor build processors write `render_profile.json`, `serialized_scene_renderers.txt`, `compiled_shader_variants.txt` and `render_dependency_report.json`. The variant observer runs after stripping; it changes no variants. Shader inclusion is limited to the two programmatically used uGUI shaders, while real serialized URP material dependencies preserve their intended keywords.

The controlled full-screen error/recovery PNGs are Editor regression evidence only. `executedOnAndroid: false` remains explicit when no hardware/emulator is available. The owner must install v43 on the same device and confirm normal rendering before any gameplay review.
