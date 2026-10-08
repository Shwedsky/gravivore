# v42 magenta device blocker — v43 rendering hotfix

Same branch and Draft PR #65; do not merge. Owner reports the entire frame magenta on the same Android device where v41 rendered correctly. v42 is invalid for acceptance. The next owner gate is rendering only, before gameplay review.

Investigation anchors: known-good v41 source `e788170b9bd80e12737eef83eeb8c322ee6f2afa`; v42 APK source `bc4b424907afffadfbc255aebea95903e9d4b01f`; resumed HEAD `4cbc5e11592d367154a3ed200d8b4d087d92ecef`, matching fetched branch.

Initial comparison: no tracked changes to ProjectSettings, Packages, URP settings or Android build code between those sources. Existing atlas and hostile-soft materials must not be blindly rewritten. Effective generated pipeline/renderer assets, packed shader resources, full-screen passes and all eight V3 prefab renderers require inspection.

Scope: rendering diagnosis, narrow correction, one-shot DEV diagnostics, focused regression and strengthened APK proof. Preserve gameplay, saves, equipment and V3 composition. Build a separate signed ARM64 IL2CPP DEV v43 APK; retain v42.

Required validation: compile, ProjectValidator, full EditMode, full PlayMode, Android v43 build and shader/material/dependency proof. Report whether actual Android execution is available; Editor rendering is not device proof.
