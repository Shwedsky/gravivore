# G-0 Blockout V1

Open `G0_Blockout_V1.blend` in Blender 5.2.2 LTS. This is an editable original GRAVIVORE silhouette/proportion blockout, not a finished game asset.

Model collection: `G0_Blockout_V1_AUTHORED`. Review floor/lights/camera are in `REVIEW_STAGE_NOT_EXPORT`. Root: `GV_Player_G0_T0_ROOT`; Blender Z up / -Y forward. Root transforms are clean. Source contains 15,080 triangles, 87 mesh objects, 107 model objects, four material swatches and zero donor parts. The review stage adds five objects.

Named pivots organize four support chains and two mandibles; they are not a finished rig. Five named sockets include core, left/right attack, hit VFX and UI anchor. Geometry is intentionally split for editability; renderer consolidation, UVs, final texturing, rig, motion-clearance tests, LODs, export and Unity orientation/scale fitting remain later work.

Reproduce with Blender background mode and `Tools/art/build_g0_blockout.py -- all` from repository root. Reopen/validate using `Tools/art/verify_g0_blend.py`. The comparison script expects the user's local approved board; it does not fetch or invent a reference.

Review: `docs/visual-production-v2/modeling/G0_BLOCKOUT_V1_REVIEW.md`. Required seven PNGs: `docs/visual-production-v2/modeling/g0-evidence/`. This source is custom authored; no third-party geometry or texture is embedded.
