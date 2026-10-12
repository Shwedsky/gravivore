# Connected concept sectors reproduction

Base: delivered APK 51, `3793b632790a791f8de861010eaedc236ecc77d1` (build source `2cc426ca003c3b4dd85a437eeb2db2d9a3256a49`). Current Blueprint sector 4, not old R2 shapes, governs the new cutting line. The existing isolated worktree/cache is reused on `codex/cutting-floor-connected-world`; the previous branch and owner APKs remain intact. No merge/push.

1. Bundled Python: run `prepare.py`. Original fabrication maps, shared CF maps remain byte-identical.
2. Blender 5.2 background: `--python Tools/connected-concept-sectors/author.py`. It uses `meshlib.py`'s project-owned physical-UV engineering helpers, makes a new heavy press/shear silhouette and a distinct transfer gallery, saves independently editable Blender objects, then consolidates/export two FBXs. No external source download or vendor code; no nonuniform hero fit.
3. `run_unity.ps1 -Action Author`. Replaces only Cutting Floor, inserts gallery, removes old neutral crossing surface triangles from a copied mesh while retaining all outside faces/native materials/UVs and original FBX. Explicit blockers drive gameplay and minimap. The camera stays the same; a small original static HDR reflection cubemap improves shared metal/actor readability without realtime probes.
4. Compile, EditMode, PlayMode `-VideoCapture`, Validate with the same wrapper. New camera fixture reviews 13 normal positions with paired UI visibility and both real enemy roles. Controller path and continuous combat recording use the real production systems and isolated saves. Recording god mode is a fixture; normal APK startup/balance unchanged.
5. `encode_video.py` in Blender encodes exactly the latest `video.json` frame count, ignoring old surplus frame files. `verify_video.py` decodes the delivered MP4 and exports first/middle/last QA frames.
6. Bundled Python `comparison.py` composes owner board crops and unretouched game frames; all game shots use the one actual camera. They are Editor scene evidence, not Android screenshots.
7. `evidence.py --strict` checks runtime, definitions, Actor V2/G-0, previous CF resources, other six sector resources, original APKs/intake. Capture owner manifest before new APK delivery and retain it.
8. Commit implementation first, then `build-android.ps1 -Flavor Dev -Version 0.1.0 -VersionCode <next-free-code>`; do not clean or overwrite previous APKs. Save current shader/actor/world packing audits under this pass before restoring generated historical output changes. Check SDK manifest/ABI/signature and metadata, deliver into owner's Builds/Android.

Device visual, touch, GPU/frame time/thermal acceptance remains separate. No automatic propagation to the other six sectors.
