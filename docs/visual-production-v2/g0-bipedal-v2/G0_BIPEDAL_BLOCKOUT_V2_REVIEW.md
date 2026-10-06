# G-0 bipedal blockout V2 review

2026-10-06. Blender 5.2.2 LTS. Human art-direction approval is pending. This branch stops at the requested G-0 blockout gate.

## Visual verdict: B

The new G-0 has a clear two-legged machine silhouette, narrow exposed waist, long articulated legs, swept split chest, a small blade sensor and unequal gravity tools. It reads as a combat robot rather than a human inside armor. The body/weapon proportions and purpose-built chest are substantially different from the original Catfish. Only 1499 triangles of low-identity pelvis/knee/ankle mechanisms remain; no stock military torso, armor, guns, feet or camouflage is retained in G-0.

The cyan core is a small vertical diamond/slit, recessed between the carapace ridges. It is neither a giant round eye nor the character's face. The original gravity identity is carried by the chest cleft, open emitter rails and compact sensor aperture. Side/back views show mechanical load paths, a narrow spine and swept containment vanes. The ankle-to-foot bridges visibly connect both supporting feet.

The black silhouette keeps the separated legs, shoulder-to-arm spaces, open waist structure and unequal tools without relying on energy color. It does not resemble a spider/crab/crawler. The equal-height comparison uses the actual original Catfish rest silhouette on the LEFT and G-0 on the RIGHT, with matching grounding and the same camera/light stage. Labels and feet are fully in frame. Original donor textures are downsampled only for portability; its silhouette is not redesigned.

G-0 reads as a potential hero rather than a finished premium character. Familiar sci-fi vocabulary remains in the repeated cylindrical joint caps, symmetric shoulder rhythm and simplified shin shells. These are the principal generic cues, and the blockout does not yet earn an A. Some shell/actuator intersections and the abrupt rear vane profiles still need refinement. They are reported openly, not hidden by the verdict. Human approval is required before refining the approved direction further.

## Gameplay-angle result and assumption

Actual perspective Blender render: **900 × 1600**, portrait 9:16. S20 Unity camera mapped into Blender: offset (0,14.8,-11.2), target height 0.9, vertical FOV 46 degrees. Authoring geometry is 3.605 m tall; **0.5 presentation scale** produces a 1.8025 m hero for this evidence. The hero faces the camera. This scale is an explicit review assumption, not a changed Unity root/prefab.

Measured projected bounding box: **108 × 153 pixels**, entirely within the frustum. Two feet/legs, torso shoulders, cyan core and forearm tools remain readable in the actual full portrait. The core is a small marker, not a giant beacon. Fine actuator detail disappears at this size as expected. Readability is sufficient for the isolated blockout gate; strength against real enemies, VFX and the production environment remains unproven because Chapter01 integration is explicitly deferred.

## Editable deliverable and measured geometry

- `art/visual-production-v2/g0/G0_Bipedal_Blockout_V2.blend`.
- Hero: **134 mesh objects**, **151 total hero objects** (includes 16 editable pivots and one armature).
- **7051 base triangles / 24705 evaluated triangles** with editable bevel modifiers applied for measurement.
- Adapted donor: **17 objects / 1499 triangles**, exact source identities and transforms in [donor_adaptation.json](data/donor_adaptation.json).
- Custom: **117 mesh objects / 5552 base triangles / 23206 evaluated triangles**. All armor, torso, core, sensor, shoulders, arms, forearm tools, rear mass, leg rails and feet are custom.
- Six shared hero materials; no donor textures on G-0.
- Original **16-bone rigid mechanical skeleton**, with one full-weight bone per vertex. No donor animation, facial skeleton or shape keys in the saved file. This is an unanimated blockout rig, not polished locomotion.
- Zero near-zero-area hero faces. 285 boundary edges remain in retained open mechanical parts. Final welding/deformation certification is deferred.
- Portable hidden comparison collection: 53 static original donor meshes; 12 packed reference images, maximum 1024 square. Whole review scene: 215 objects including the donor, studio and cameras. These extra objects are not part of the hero count or runtime budget.
- Internal attribution text, named authoring collections, explicit bone/pivot hierarchy and review cameras are included. The old spider file and prior PR remain untouched.

## Evidence

All files are actual Cycles renders of the saved Blender model, 32 samples with denoising. No image-generation substitute or painted-over concept was used.

1. [Front](evidence/01_G0_Bipedal_front.png)
2. [Side](evidence/02_G0_Bipedal_side.png)
3. [Back](evidence/03_G0_Bipedal_back.png)
4. [Three-quarter](evidence/04_G0_Bipedal_threequarter.png)
5. [Full gameplay angle](evidence/05_G0_Bipedal_gameplay_angle.png)
6. [Unlit black silhouette](evidence/06_G0_Bipedal_black_silhouette.png)
7. [Original donor LEFT / G-0 RIGHT](evidence/07_G0_Bipedal_vs_Catfish_donor.png)

Camera settings: [render_manifest.json](data/render_manifest.json). Mesh/reopen/rig checks: [blockout_validation.json](data/blockout_validation.json). PNG dimensions and hashes: [evidence_validation.json](data/evidence_validation.json).

## Verification

- Blender FBX import, saved blend reopening, finite meshes, exact donor subset, rigid weights, embedded attribution, packed reference dependencies and hidden reference default: **PASS**.
- Knee pose smoke: 10-degree rotation moved rigid shin vertices by up to **0.15087 m**, then reset. This proves one binding path, not a complete animated gait.
- All seven PNGs rendered and visually inspected. Image integrity, dimensions and hashes are recorded separately.
- Unity 6000.3.0f1 / URP compile: completed with no C# compiler errors.
- `Gravivore.Editor.ProjectValidator.ValidateOrThrow`: **PASS**, exit 0. Initial sandbox licensing IPC timeout and an incorrect namespace invocation were resolved; successful final log is recorded with verification evidence.
- EditMode: **386 passed, 0 failed, 0 skipped**.
- PlayMode: **92 passed, 0 failed, 1 skipped**. Optional `CaptureStructuralFoundationWhenRequested` requires `GRAVIVORE_VISUAL_INTEGRATION_QA` to export unrelated structural captures; it was not enabled.
- Dev Android build: **PASS**, Unity exit 0 and APK manifest/package verification passed. ARM64, version 0.1.0+2, applicationId `com.gravivore.mobile.dev`. APK: `C:/Users/pamak/Documents/ChatGPT/gravivore/.codex-worktrees/g0-bipedal-blockout-v2/Builds/Android/gravivore-dev-0.1.0+2.apk` (59221713 bytes). This is a regression build of the existing runtime; G-0 V2 remains outside `Assets/`. Build metadata records the then-current modeling checkpoint `ef90e1e`; final evidence/document changes do not enter the APK.

Saved Unity evidence: [EditMode XML](verification/EditMode.xml), [PlayMode XML](verification/PlayMode.xml), [project validation log](verification/project-validation-final.log), [Android build log](verification/AndroidBuild.log), [Android metadata](verification/AndroidBuildMetadata.json). SHA256 and scope outcome: [unity_verification.json](data/unity_verification.json).

## Production limits and next gate

134 separate hero mesh objects are an authoring structure, not a shipping renderer count. Batching, atlas, final UVs/textures, LODs, proper IK/locomotion and device performance need later authorized production. A 24705-triangle studio model alone does not prove Android 60 FPS.

No G-0 Chapter01 integration or production prefab replacement; no Scout/Cutter/Magnetar/environment work; no PR merge. Unity-generated serialization changes from checks were restored. `Assets/`, `Packages/` and `ProjectSettings/` match the verified main baseline after cleanup.

Next spec id: **none authorized at this gate**. The next action is human art-direction review of G-0 Bipedal Blockout V2. Do not start a later specification automatically.

G-0 BIPEDAL BLOCKOUT V2: READY FOR HUMAN ART-DIRECTION REVIEW
