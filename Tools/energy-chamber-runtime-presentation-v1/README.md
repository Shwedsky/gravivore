# Energy Chamber runtime presentation pass

Continue the existing PR78 checkout/branch. Do not run the original architecture `Author`/`Lod` helpers: those reproduce APK55's former LOD policy. This pass owns materials, shader and local LOD membership only; the accepted geometry, sources, scene, layout and collision are frozen.

1. `run_unity.ps1 -Action Inspect -Stage diagnosis` and the camera sweep reproduce the pre-fix inventory on the diagnosis checkpoint.
2. `-Action FixVisibility -Stage visibility` applies automatic silhouette-safe LODs without touching bounds/placement/collision.
3. Run `generate_surfaces.py`, then `-Action LookDev -Stage lookdev` to bind the eight PBR families, production-only trims and opaque shader. `-Action Survey` emits read-only surface samples into ignored Builds/Logs for light-mask placement.
4. `-Action Compile`, `-Action Validate`, full `-Action EditMode`, and filtered `-Action PlayMode` provide actual exit/XML receipts. The camera fixture selects the real Android quality, uses the initialized production camera and restores all diagnostic material overrides after the test. Only capture-fixture combat/actor animation is paused; production behavior is unchanged.
5. `verify_freeze.py` verifies exact FBX/UV/source, scene/layout and prefab record preservation. `record_delivery.py` copies fresh traversal receipts and records executed gates. Restore only unrelated Unity-generated tracked changes with `restore_unowned.py`; it preserves an ignored recovery patch and never deletes untracked files.
6. Commit/push the runtime checkpoint before the APK, then `-Action Android -VersionCode 56 -Label Android56`. This refuses an existing APK filename, never uses `-Clean`, and uses the previously verified private installed SDK mirror. The existing build entry restores the prior SDK preference. APK55 is preserved and re-hashed.
7. `record_delivery.py --apk` verifies the actual APK ZIP, ARM64 native code, compiled production shader/material/scene serialization, source commit and successful build log before recording READY FOR HUMAN DEVICE REVIEW.

All maps are project-owned deterministic source output. No package, downloaded art, realtime Light, particle system, transparent layer, additional geometry or runtime C# presenter is introduced. The shader keeps a consistent UnityPerMaterial buffer in all passes, uses normal/metallic/smoothness/occlusion maps and existing scene lighting/reflection. Static local spill and tiny practical masks affect only the chamber's production materials. Slow bounded energy variation uses shader time; `_PreviewTime` and `_EmissionScale` are authoring/test diagnostics and ship at -1 / 1.

The material `_ChamberOrigin` is authored for the frozen `(11,.08,50.65)` placement. If a later authorized pass moves or duplicates the installation, reauthor that explicit material configuration. It is not a gameplay dependency or scene-name lookup.
