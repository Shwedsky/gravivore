# G-0 Production V3 source

Open `G0_Production_V3.blend` in Blender 5.2.2 LTS. The production collection contains one 18-bone mechanical armature and three rigid skinned LODs; only LOD0 is visible in the studio. Select an action in the Action Editor to preview Idle, Run, Attack, Hit or Death. The scene uses 30 fps and source meters, Z up / -Y forward. No Unity presentation scale is baked into the source.

The saved accepted V2.1 file remains unchanged in `art/visual-production-v2/g0/`. The build script opens that file, revises its surfaces/attachments, retains its bone assignments and joint locations, consolidates the meshes, generates the unique UV atlas and authors the original game animation actions. The old source text/provenance and hidden Catfish comparison remain embedded. Export selection includes only the production armature and three LODs.

Reproduce with Blender `--background --python-exit-code 1 --python Tools/g0-production-v3/build_production.py`. Verify separately with `validate_asset.py`; capture reopened-source studio evidence with `render_evidence.py`. Use `Tools/g0-production-v3/run_unity.ps1 -Action Capture` for the isolated Unity intake/review. All paths are resolved from the scripts' repository root.

The FBX, URP material, atlas textures, Animator controller and script-free/collider-free art prefab are in `Assets/_Game/ArtReview/G0ProductionV3`. The Animator has five independent states and no gameplay transitions/events. The prefab keeps source scale; the isolated scene applies the fixed accepted `.4585482776` normalization (V2.1 base fit × 1.10). Future runtime integration must supply its own presentation event binding and stride-speed mapping without replacing gameplay authority.

Attribution: Catfish Mech low-poly (animated), Jungle Jim (`jungle_jim`), CC BY 4.0. Only the previously approved internal pelvis/knee/ankle donor mechanisms are retained; see `ThirdPartyNotices.md` and the embedded attribution text. All upper-body identity, outer armor, feet, tools, production skeleton, animation and atlas treatment are GRAVIVORE original work.
