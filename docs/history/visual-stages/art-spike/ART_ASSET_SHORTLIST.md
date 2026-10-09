# ART SPIKE V3 — candidate evaluation

Verified 2026-10-02 from original pages, public API metadata and official downloads. No mirror, login bypass or user account is needed for the selected V3 pipeline. Candidates requiring authentication are skipped under the current instruction. Original research archives stay in ignored Builds/ArtSpike, outside Assets and the public commit.

## 1. Vanguard-Class Mech Titan (Game Ready) — highest priority

- Original: https://sketchfab.com/3d-models/vanguard-class-mech-titan-game-ready-84f5e9a69d734bbd97b10634e379b235
- Author: ThankSang0301, https://sketchfab.com/ThanhSang0301
- Official metadata: https://api.sketchfab.com/v3/models/84f5e9a69d734bbd97b10634e379b235
- Exact selected public license: CC BY 4.0, https://creativecommons.org/licenses/by/4.0/legalcode.en
- Commercial use, modification, raw and modified public-repository redistribution are permitted with attribution, license link and a change notice. Required credit if eventually selected: “Vanguard-Class Mech Titan (Game Ready) by ThankSang0301”, original URL, CC BY 4.0 link, actual modifications; no endorsement.
- Acquisition: official Download opens Sketchfab authentication. Skipped; source file and bundled metadata were not acquired.
- Public geometry: 42,295 triangles, 23,262 vertices. Renderer/material counts, child objects, separability, skin/bone hierarchy, texture resolution and file quality are **unmeasured**.
- Page describes a Maya/Substance PBR workflow and BaseColor/Metallic/Roughness/Normal/AO/Emissive maps. These maps were not inspected. API animationCount is 0; this does not prove absence of a rig.
- Decision: unavailable for autonomous acquisition; no files imported. Suitability for topology-preserving module reuse is unknown.

## 2. Robot Warrior — high priority

- Original: https://sketchfab.com/3d-models/robot-warrior-dae0366489e54d10b15c1315407cac33
- Author: Andrei Milin, https://sketchfab.com/milinam2002
- Official metadata: https://api.sketchfab.com/v3/models/dae0366489e54d10b15c1315407cac33
- Public license: CC BY 4.0, same commercial/modification/source redistribution permissions and attribution obligations as above. Required eventual credit: “Robot Warrior by Andrei Milin”, original URL, CC BY 4.0 link and change notice.
- Acquisition: official download requires authentication; skipped.
- Public geometry: 35,798 triangles, 18,861 vertices, animationCount 0. Page describes a rigged game-ready asset using Blender/RizomUV/Marmoset/Substance Painter.
- Renderer/material counts, map set/resolution, skeleton, clips, armor/limb child separation and suitability without topology editing remain unmeasured. “Rigged” is a page claim, not an inspected skeleton.
- Decision: no import. Linked concept/reference artists also need provenance review if this asset becomes obtainable later; references alone are not evidence of copying.

## 3. K3NY Robot — medium priority

- Original: https://sketchfab.com/3d-models/k3ny-robot-33f72710718b4459ae9a2c407bb06f8d
- Author: Hbomb2014, https://sketchfab.com/Hbomb2014
- Official metadata: https://api.sketchfab.com/v3/models/33f72710718b4459ae9a2c407bb06f8d
- Public license: CC BY 4.0. Required eventual credit: “K3NY Robot by Hbomb2014”, original URL, CC BY 4.0 link and change notice. Commercial use, modification and raw/modified repository distribution are allowed on those terms.
- Acquisition: official download requires authentication; skipped.
- Public geometry: 39,072 triangles, 19,448 vertices, animationCount 1. Page describes textured and fully rigged geometry.
- Renderer/material counts, map types/resolutions, bone count, clip content and mesh separability are unmeasured.
- Decision: no import; anatomy cannot be evaluated from counts or previews alone.

## 4. Corebreaker Robot Model — safe licensing fallback

- Original/author: https://donitz.itch.io/corebreaker-robot-model — Donitz.
- Explicit license on the original page: CC0 1.0. Commercial use, modification and original/modified repository distribution allowed; attribution optional. https://creativecommons.org/publicdomain/zero/1.0/legalcode.en
- Acquisition: official corebreaker.zip downloaded without login. Archive contains corebreaker.blend and textures/lens_blue.png, no bundled license text; page license was checked.
- Inspected with official portable Blender 4.5.0, factory settings, auto-execution disabled. Source was read, not saved. Blender warns the file uses newer-version data, so this inventory is not a guarantee of full rig fidelity.
- Full pack: 20 mesh objects, including ship and first-person/tool extras. Selected robot subset: PlayerBody 3,644 triangles; PlayerHead 702; PlayerScreen 26; PlayerIndicator 4 = **4,376 triangles in four mesh objects**. Unity renderer count is unmeasured because this rejected asset was not imported; four mesh objects are the source rendering-unit equivalent.
- Robot subset: six unique material roles (Yellow, Gunmetal, Black, Screen, LensBlue, Red), 11 assigned slots. The one present lens image is 256×256; screen_test.png is referenced but absent. Main armor is constant shader color, not a PBR surface map set.
- Skeleton: Player has 57 bones; FirstPersonPlayer 32. Three source actions: FirstPersonPlayerReset (0–10), FirstPersonPlayerTest (0–133), PlayerReset (0–10). No walk clip identified. The page claims realistic joint limits; the read-only inventory does not establish constraint fidelity.
- Separation: body, head, screen and indicator are separate, but PlayerBody joins torso, arms and legs. FirstPersonPlayerBody joins both arms. Tool pieces are separate; they cannot supply the primary four-support chassis.
- Decision: rejected as primary donor. Reusing individual limbs would require topology/skin editing, and it does not improve V2 armor surface quality.

## 5. Stylized Sci-Fi Mech Robot Asset — technically useful, license unresolved

- Original/author: https://retrostylegames.itch.io/stylized-sci-fi-mech-robot-asset — RetroStyle Games.
- Acquisition: official free FBX archive downloaded without login. No license, README or redistribution terms found in that archive.
- Original page offers free/royalty-free game/prototyping use, but does not establish an explicit license for modification and redistribution of raw/modified source in this public repository. Attribution and those source permissions remain **unverified**. No assumption from “free” or “royalty-free” is used.
- Blender-inspected LOD0: Bottom 12,676 + gunsBottom 4,140 + gunsTop 4,140 + Top 5,040 = **25,996 triangles** in four skinned mesh objects. LOD1: 18,606 triangles in four objects. Unity renderers are unmeasured; eight source mesh objects across both LODs, four per active LOD.
- One material reference per LOD, two references across the source file. Actual archive contains **five 2048×2048 PNGs**: basecolor, emissive, metallic, normal, roughness, rather than the page's four-texture summary. No AO file.
- DeformationSystem has 43 bones. Seven separate animation FBXs: death (61 frames), idle (50), landing (76), four walk directions (31 each). These are inspected source animation ranges, not retargeted Unity clips.
- Separation: upper body and paired gun groups separate; all six lower legs share the Bottom mesh. Individual four-support reuse cannot preserve topology.
- Decision: rejected; public-source redistribution permission is unresolved. The official page also identifies it as Ocean Keeper's protagonist, an additional mismatch with the brief's recognizable commercial-character restriction. None of its files or preview images are imported.

## Additional autonomous search: Robot — piacenti

- Original: https://opengameart.org/content/robot-5, author https://opengameart.org/users/piacenti.
- Official archive: https://opengameart.org/sites/default/files/robot%20game.zip
- License: CC BY 3.0, https://creativecommons.org/licenses/by/3.0/legalcode. Commercial use, modification and repository redistribution permitted with title/author/source/license credit and change notice. Required if selected: “Robot by piacenti”, both original links, CC BY 3.0 and actual modifications.
- Official archive acquired without login. Three complete alternate body meshes, **23,857 triangles each**, one material per variant. No armature or clips. Color maps are 2K for the first two variants, 4K for the ancient variant; normal/gloss maps are 2K. Legacy Blender file has stale external paths; PNGs are present in the archive.
- All limbs/body are joined in each complete variant. Unity renderer count unmeasured (three whole-body source mesh objects). Rejected: topology editing would be required; using its complete humanoid would compromise the agreed direction.

## Selected V3 source strategy

The current instruction explicitly authorizes a temporary mock/proxy when suitable donors cannot be obtained autonomously. V3 therefore uses **original project-authored modular hard-surface proxy geometry** with the V2 design language as a silhouette reference, and a real independently licensed PBR texture donor:

- Blue Metal Plate by Rob Tuytel / Poly Haven: https://polyhaven.com/a/blue_metal_plate
- CC0 asset terms: https://polyhaven.com/license; https://creativecommons.org/publicdomain/zero/1.0/legalcode.en
- Commercial use, alteration and raw/modified public source redistribution allowed; attribution optional, provenance retained.
- Official public download metadata: https://api.polyhaven.com/files/blue_metal_plate. Downloaded 1K diffuse JPG, GL normal PNG and ARM PNG. CDN MD5 values match; SHA-256 is in the manifest.
- ARM channels: R=AO, G=roughness, B=metallic. Project-owned URP adaptations extract AO and pack metallic into R with inverted roughness in A. Original three source files remain byte-identical.
- No third-party character geometry, skeleton or animation is used. Kenney's existing CC0 subset remains supporting scenery only.
- Temporary geometry must eventually be replaced/refined with original final armor modules or an obtainable permissive donor with genuinely separate limbs, UVs and an appropriate rig. Preserve the common chassis/core identity and four hip/knee/foot socket paths. A full stock humanoid, six-legged stock character or worse silhouette is not a suitable replacement.

**SHARE-ALIKE CHARACTER DEPENDENCY: NO**

V1/V2 candidate history and Julius attribution remain naturally in Git at V2 commit 783876869e5506304c4521a9ea9b89d6e37e40ef. That history is not relicensed. V3 selected Assets and all ten current review PNGs are newly generated without Julius geometry.
