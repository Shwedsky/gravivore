# Energy Chamber — final polish / validation V1

Accepted baseline: `535c99c1703bff88aaac3cfcd4e8b9833e3507d2`. Branch: `art/energy-chamber-hybrid-proof-v1`. Existing Draft PR: [#77](https://github.com/Shwedsky/gravivore/pull/77). No merge or production Chapter integration.

## Preserved

All eleven custom FBXs and all three Blender source files remain byte-for-byte unchanged. Central casing proportions, stepped circular base, four radial node placements, four vertical cyan stabilizers, horizontal reservoirs/generators/centrifuges, service stairs/rails and core-to-node routes are retained. Assembly: **11.48 × 6.095 × 11.48 m**, **45 renderers**, **279,694 triangles**, no collision or gameplay behaviours. Unchanged current bipedal G-0 Tier 0 prefab at unit scale, approximately 1.771 m high. No new geometry or external asset import.

## Improved

- Orange core: interleaved induction waves, narrow meandering discharge filaments and absorption troughs replace the flat grid-like emissive treatment. The inner sleeve uses a separate darker pattern. Opaque URP Lit surfaces, no transparent layer, runtime animation or added lights. Cyan cores share the containment language with lower emission intensity.
- Materials: warm neutral cast base iron, subdued base cassettes, pale warm chamber casing, cooler gunmetal load frame, brushed pipe steel, desaturated cool node casing and machined interface plates. Value, metallic response and roughness distinguish roles under the Chapter's cool lighting without adding broad new colors.
- Node integration: existing gantry feet, bearing heads, locking seats, risers, coupling blocks and base interfaces receive a distinct machined finish. Physical interfaces already present in the accepted geometry remain intact and read more clearly.
- Secondary detail: existing opaque maintenance plaques carry the GRV/C-07 containment label, paired feed/return channels and compression chevrons. Small handled-edge wear, witness seams and sparse machining scratches live in PBR maps. Existing cable junctions, brackets, fasteners and indicators are retained; no random additions.

## Actual Chapter 1 context

`Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_Chapter01Context.unity` is a static visual copy of the current `Chapter01_ScrapExclusion.unity`, with gameplay components stripped from the copy. It retains **476 source renderers**, their exact mesh/material references and world transforms, **17 source lights**, the source volume profile and reflection. The production scene, source environment assets and shared settings are unchanged in the delivered diff.

The installation sits in the open Magnetar sector lane at **(11, 0.08, 51.3)**. No raised source mesh AABB intersects the installation. This is a review placement, not a production layout decision or collision/navigation approval. G-0 stands at **(14, 0.08, 45.1)**. Camera: exact current player-follow offset **(3.4, 19.8, -13.6)**, perspective **48°**, look-at height **3.2**, portrait **9:16**. Gameplay evidence uses G-0's position as the settled player-follow anchor. High and close views are supplementary inspection cameras.

Source grading and lighting were not brightened for the machine. In the emission-disabled context diagnostic, only the installation's emission is disabled; surrounding Chapter emitters, lighting and grading are retained.

## Review evidence and findings

- [Current Chapter gameplay view](screenshots/09_chapter01_gameplay.png): orange center remains the local focal point; four cyan columns read as secondary system nodes. Machinery and metal casing remain visible around the energy surfaces.
- [Gameplay with G-0](screenshots/10_chapter01_gameplay_G0.png): unchanged character scale gives the installation a credible industrial size. The fixed portrait player-follow framing comes close to the left node's outer edge; the high view shows the full footprint.
- [High 3/4 context](screenshots/11_chapter01_high_three_quarter.png): radial layout, stepped base, service stairs and existing Chapter surroundings are legible.
- [Close 3/4 context](screenshots/12_chapter01_close_three_quarter.png): richer contained energy, contrasting casing/frame, node interfaces and pipe routes are visible. Emission does not bleach casing or braces.
- [Emission-disabled gameplay](screenshots/13_chapter01_emission_disabled.png), [detail](screenshots/14_chapter01_emission_disabled_detail.png): stepped mass, ring thickness, pipes, casing and connection gantries remain readable without the machine's glow. Inactive core surfaces are deliberately darker; there is no claim that their energy accents remain luminous with emission off.

Isolated low/high/gameplay/G-0/material/close/node and no-emission views are also retained as screenshots 01–08 for direct comparison to the accepted proof. Captures are Unity camera renders; no retouching or color correction outside the scene.

## Remaining weak areas / production decision

Close views still expose faceted ring shading and repetitive service-rail construction from the accepted meshes. Small plaque text and telemetry cables disappear at phone gameplay size; they provide detail at closer inspection rather than competing for attention. The Chapter's existing large red machinery can compete in wider views. These are bounded limitations of the accepted proof, not reasons for another concept or architecture pass.

**READY FOR PRODUCTION DEVELOPMENT.** This closes the final visual polish pass for the proof. Another concept/structural art pass is not needed before production integration. Integration should cover LOD/batching/texture consolidation, navigation/collision ownership and real-device human review; those remain outside this visual-only task. The assembly's 279,694 triangles and material variants need device profiling before a production performance claim.

## Validation / delivery

Unity compilation passed. EditMode: **535 passed, 0 failed**. PlayMode: **2 passed, 0 failed**, isolated proof and Chapter context. Project validation and Android receipt are recorded under `verification/`; the final delivery receipt provides hashes, exact changed-file inventory, camera/bounds preservation and build result.

Android review build targets the static Chapter-context scene in an isolated dependency-closure project, not the full gameplay application. Project-specific production Chapter build callbacks do not apply to that visual-only project. No phone human visual gate or device FPS measurement was performed by this pass. This review APK is not a production APK.

Proof prefab: `Assets/_Game/ArtProof/EnergyChamber/Prefabs/EnergyChamber_HybridProof.prefab`. Isolated technical scene: `Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity`. New surface tool: `Tools/energy-chamber-final-polish-v1/materials.py`. Exact file list: [FILES_CHANGED.txt](FILES_CHANGED.txt). All new textures/markings are project-owned. Previously selected CC0 machinery, stairs and rails retain their recorded origin and notices; no raw external source archives changed.

Assumptions: a static copy of current Chapter presentation and exact settled player camera is appropriate for this visual proof; no live combat, gameplay interactions, player prefab changes or production layout changes are authorized. There is no new gameplay spec/task ID in this request. Next phase: owner-reviewed production integration and device profiling.
