# G-0 Production V3

Production starts from merged main `467b43be836e7104aa13baa60af648f43fb453d0` and the accepted `art/visual-production-v2/g0/G0_Bipedal_Blockout_V21.blend`.

Human-approved gate B: bipedal robotic mech, V2.1 proportions, broad shoulders, recessed central gravity core, asymmetric tools and mechanical legs. Presentation geometry uses the accepted 1.10x fit. No design or scale gate is reopened.

## Delivery boundaries

- Refine the saved source; retain source attribution and identifiable silhouette.
- Deliver consolidated production geometry, rigid mechanical articulation, Idle/Run/Attack/Hit/Death actions, clean UVs/materials and LOD0/1/2 in an FBX package.
- Import only below `Assets/_Game/ArtReview/G0ProductionV3`.
- Capture the unchanged gameplay camera with existing ordinary enemy and Magnetar references, plus studio/silhouette and animation evidence.
- Keep runtime player prefab, movement/combat timing, collider, footprint, camera, gates, layout and other characters unchanged.
- Commit and push every major production checkpoint. Stop at review readiness; live integration requires the next human gate.

## Verification and acceptance

Record source provenance, geometry/LOD counts, skeleton, animation ranges, UV/material checks, scale/orientation/hierarchy and actual Unity import results. Execute relevant EditMode/PlayMode checks, project validation and a dev Android build where available. Evidence must come from Blender/Unity output, never painted or generated substitutes.

The final recommendation is `PASS FOR PRODUCTION INTEGRATION` only if the delivered asset and camera evidence support it; otherwise `REWORK` with specific remaining defects. Human approval remains required before live production replacement.

## First checkpoint

This committed contract is the initial real change establishing production scope and measurable delivery requirements. Geometry production and validation are pending; no final-art claim is made by this checkpoint.
