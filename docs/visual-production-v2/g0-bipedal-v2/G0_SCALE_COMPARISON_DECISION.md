# G-0 V2.1 scale comparison decision

2026-10-06. **Recommendation: 1.10x. Verdict: B. Human decision pending.**

The revised mech is much narrower than the current production Tier0. At the actual Chapter01 camera, 0.90x looks too slight, and 1.00x is readable but has modest hero presence. 1.10x gives the clearest chest/shoulder/two-leg structure at native 360x640 without challenging Magnetar's much larger body mass. Warden retains the heavier ordinary-enemy cue and Scout remains a broad horizontal silhouette. The real production-spawn comparison confirms that nominal 1.00x has less visual mass than the current player.

This is a gameplay-camera/hierarchy choice. Authoring meters do not determine the recommendation. The nominal fit is 0.416862071; recommended geometry-only transform is **0.458548278**. No selection is bound into production until a human accepts it.

The gameplay footprint remains radius 0.42 / height 1.4. At 1.10x, the mesh is 0.945756 wide and 1.54 high, with 0.052878 static width overhang per side. Centered gate margins remain 2.027122 laterally and 0.94 below the generated top frame. No collider, movement, attack range, pathing, gate, layout or camera change is justified or made.

Verdict B means this pass supplies a usable proportion and scale decision, not final production approval. The lower body and combat feet improve the athletic machine silhouette, but simplified joints, shell intersections and tiny phone details remain blockout work. Existing closed-gate dressing can obscure the hero at the actual crossing plane (capture 10). Unlocked traversal, moving animation, combat/VFX and device performance need later authorized validation. Static gate clearances must not be presented as a guarantee of live traversal visibility.

Primary evidence: [three-scale Unity board](evidence-v21/unity/04_G0_scale_comparison_board.png), [native phone board](evidence-v21/unity/08_G0_phone_size_readability.png), [current production player at spawn](evidence-v21/unity/09_G0_current_player_spawn_reference.png), [Blender three-quarter](evidence-v21/blender/04_G0_V21_threequarter.png).

Exact proportion metrics, camera/footprint values, all captures and limitations are in `G0_BIPEDAL_V21_PROPORTION_REVIEW.md` and `G0_UNITY_SCALE_REVIEW.md`. Verification results are recorded after execution in `data-v21/verification_summary.json`.

No production prefab replacement, final texturing/UV/rig/animation/LOD, runtime composition change or merge. This pass stops at the human review gate. Next spec id: **none authorized**; next gate is the human V2.1 proportion/scale decision.

G-0 BIPEDAL V2.1: PROPORTION + UNITY SCALE REVIEW READY FOR HUMAN DECISION
