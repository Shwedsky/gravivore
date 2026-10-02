# GRAVIVORE ART SPIKE V3 — completion report

**ART SPIKE V3 VISUAL REVIEW: PENDING**

Visual-only update on codex/art-spike-kitbash and the same draft PR #30:
https://github.com/Shwedsky/gravivore/pull/30
Base remains codex/s20-balance-vertical-slice, 4394448a7f3950cb62df701e8969137ae4b4a01c. Prior V2 HEAD: 783876869e5506304c4521a9ea9b89d6e37e40ef. No merge, new PR, retarget, S20 gameplay/balance change or production Chapter binding.

## Candidate evaluation

All five exact candidates were inspected first; detailed licensing/acquisition/source measurements and unknowns are recorded in [ART_ASSET_SHORTLIST.md](ART_ASSET_SHORTLIST.md).

- **Vanguard-Class Mech Titan:** official page/API CC BY 4.0, 42,295 triangles, animationCount 0. PBR map set described by author, but file/hierarchy/material/renderer/texture-resolution/rig data unmeasured because official download requires authentication. Skipped under the autonomous acquisition instruction.
- **Robot Warrior:** official CC BY 4.0, 35,798 triangles, animationCount 0, author describes rigged. Official download requires authentication; child/renderer/material/map/rig inspection unavailable. Skipped.
- **K3NY:** official CC BY 4.0, 39,072 triangles, animationCount 1, described textured/fully rigged. Official download requires authentication; file/hierarchy/renderer/material/rig detail unavailable. Skipped.
- **Corebreaker:** official CC0 ZIP acquired without login. Four robot mesh objects / 4,376 triangles, six material roles / 11 slots; 256×256 lens image and missing screen reference, no armor PBR set. Player skeleton 57 bones; three reset/test actions across player and first-person rigs. Body joins arms/legs/torso, unsuitable for separate four-support reuse without topology/skin editing. Rejected.
- **RetroStyle mech:** official FBX archive acquired. Active LOD0: four skinned mesh objects / 25,996 triangles / one material; LOD1 18,606. Actual five 2K PBR PNGs, 43 bones and seven animation FBXs. Six legs joined in one lower mesh. Free/royalty-free page does not establish public raw-source modification/redistribution terms; archive has no license. Also an identified Ocean Keeper protagonist. Rejected, no import.
- **Additional piacenti Robot:** official CC BY 3.0 ZIP acquired. Three complete alternate meshes, 23,857 triangles and one material each, 2K/4K color plus 2K normal/gloss; no armature/clips. Inseparable humanoid bodies rejected.

Rejected .blend/.fbx metrics are Blender source mesh-object counts, not asserted Unity renderer measurements. No archive with unresolved terms is committed.

## Selected donor strategy

The current instruction expressly permits a temporary mock/proxy rather than requiring user authentication or compromising the direction. Selected: original project-authored hard-surface proxy modules plus **Blue Metal Plate by Rob Tuytel / Poly Haven (CC0)**, downloaded from the official CDN without login. Kenney's existing CC0 subset remains scenery.

Eight original shared modules: Chassis, UpperSupport, LowerSupport, Foot, FlankPlate, WeaponHousing, EmitterFork, ShearBlade. No external character mesh or skin is loaded. Final replacement/refinement must preserve core position, four hip/knee/foot socket paths, units, root scale and tier silhouette. The proxy is not misrepresented as the unavailable Vanguard donor.

## G-0 V3

### Tier 0

Low chassis, central cyan core at (0,0.68,0) / diameter 0.30, four three-part mechanical supports, paired forward mandibles and fork tips. 19 renderers, 5,928 triangles. Original chamfered armor geometry, independent pivots and real surface maps replace unfinished donor planes.

### Tier 1

Adds flank plates, narrow cyan strips and outer containment ring. Common identity/locomotion/core preserved. 24 renderers, 6,936 triangles; width 1.961 m versus T0 1.500 m.

### Tier 2

Adds outer plates, heavier paired housing/fork weapons, supported upper ring/posts. 33 renderers, 9,216 triangles; width 2.498 m, height 0.862 m. Extra geometry establishes evolution without root scaling.

## Cutter V3

Low two-runner chassis, long/short asymmetric shear arms and tapered blades, offset shield and red spine. Same original proxy/PBR ecosystem; no humanoid head or cyan core. 13 renderers, 3,352 triangles, about 1.535 m wide / 0.614 m high, smaller/lower than T2. Static feet touch the review floor; no gait contact claim.

## Materials / PBR

Three unchanged CC0 source files at 1024×1024: diffuse JPG, GL normal PNG, ARM PNG. MD5 matches official API; original URLs/SHA-256/byte sizes in [manifest](art-spike/ASSET_MANIFEST.json).

Four maps are actually referenced by mechanical materials: diffuse, normal, derived metallic/smoothness and AO. Linear ARM.B goes to metallic R; 1-ARM.G goes to smoothness A; ARM.R goes to AO RGB. Original source is retained; output masks are 8-bit. Normal/masks linear, diffuse sRGB, mipmapped, 1K max size, Android ASTC 6x6 override. Project-owned cooler armor and darker structure tints retain the authored surface maps; cyan/red are independent energy materials.

This is a generic real surface donor, not baked final character texturing. Planar UVs and nonuniform module scaling need final refinement.

## Animation viability

No acquired rig is selected. A project-authored three-second transform-only idle clip independently moves four hip pivots and two common weapon pivots. Build-time sample at 0.75 seconds confirms all hips move; automated test verifies no Animator/Animation is added to the main character prefabs. Optional menu preview is a temporary DontSave clone in the isolated scene.

This proves articulation only, not a locomotion rig, skinning, IK, gait, floor contact under animation or gameplay binding. No animation integration blocks this visual review.

## Performance

Measured inactive-inclusive prefab counts:

- T0: 19 renderers, 36 material slots, 5,928 triangles, four unique materials.
- T1: 24 renderers, 43 slots, 6,936 triangles, four materials.
- T2: 33 renderers, 58 slots, 9,216 triangles, four materials.
- Cutter: 13 renderers, 25 slots, 3,352 triangles, three materials.

All character budgets pass. Four 1K texture maps are shared across the characters; no SkinnedMeshRenderer/Animator/Animation/collider/light/camera/gameplay script in character prefabs. Two deliberate material submeshes per mechanical module mean renderer counts alone understate draw cost. One review directional light, a static small cubemap and modest bloom; device profiling still required. No Android FPS/frame-time result is claimed.

V2 versus V3 counts, bounds and visual tradeoffs: [review](ART_SPIKE_REVIEW.md), exact [snapshots](art-spike/PERFORMANCE.md).

## Licensing / cleanup

**SHARE-ALIKE CHARACTER DEPENDENCY: NO**

Julius raw OBJ/MTL/license directory and metadata removed. Character prefabs/comparison scene and ten screenshots regenerated from independent geometry, without donor vertices. Old Julius-specific importer behavior removed. Notices/license scope/manifests updated; historical V2 remains under its original terms in ordinary Git history.

Current external visual assets are CC0. No paid/NC/SA/unclear/franchise asset, donor script or engine package imported. Attribution remains voluntarily documented for CC0.

## Visual outputs

All ten requested actual Unity outputs are listed and linked in [ART_SPIKE_REVIEW.md](ART_SPIKE_REVIEW.md), including surface closeup and documented exploded composition. Capture uses Unity 6000.3.0f1 / URP 17.3 on Direct3D11, AMD Radeon(TM) Graphics. Image 07 joins three actual full portrait panels; no synthetic render substitutes.

## Verification

- Unity compile: succeeded; changed editor/test assemblies built without C# errors.
- Full EditMode: **257/257 passed**, zero failed/skipped; 13 ArtSpike isolation/PBR/pivot cases included.
- Full PlayMode: **56/56 passed**, zero failed/skipped.
- Standalone ProjectValidator: outcome recorded in [verification](art-spike/VERIFICATION.md).
- Performance snapshots and ten-image capture: executed.
- Android/APK: deliberately not built, as specified for V3.
- Production content/settings diff: checked against the S20 base; unrelated Unity-generated serialization is excluded from the commit.

Test XML/validator evidence: [verification artifacts](art-spike/verification/). Exact scope: [FILES_CHANGED.txt](art-spike/FILES_CHANGED.txt).

## Files / assumptions / limitations / next spec

Changes are confined to Assets/_Game/ArtSpike, the art documents/review evidence and ThirdPartyNotices.md. Rejected archives/tools are ignored, not committed. Root checkout and other worktrees are preserved.

Assumption: the explicit autonomous proxy fallback permits original temporary mesh authoring while preserving the V2 design language. It does not imply final visual approval or production readiness. Artist-quality geometry/UVs, a proper locomotion solution and device performance remain limitations.

No next production spec is started. The next stage is **human ART SPIKE V3 visual review**; Chapter integration needs subsequent explicit authorization.

**ART SPIKE V3 VISUAL REVIEW: PENDING**
