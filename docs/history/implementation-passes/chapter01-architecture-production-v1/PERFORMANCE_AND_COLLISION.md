# Energy Chamber production assessment

One static production landmark in Chapter 01 at `(11,.08,50.65)`, zero rotation and unit scale. The accepted context reference was `(11,.08,51.3)`; production moves 0.65 m south solely to clear existing movement envelopes. Accepted local geometry, proportions, materials, UVs, radial arrangement and source GUIDs are preserved. Dimensions remain approximately 11.48 × 6.095 × 11.48 m.

## Measured render cost

- LOD0: 45 renderers, 279,694 triangles, 156 material slots, 22 unique materials.
- LOD1: 29 renderers, 181,124 triangles, 88 slots, 20 unique materials.
- LOD2: 15 renderers, 142,836 triangles, 72 slots, 17 unique materials.

Counts sum every renderer instance, including reused node meshes. Twenty materials are promoted with the chamber; two shared R1 trim materials remain read-only production references. Material slots measure potential submesh submissions, not actual profiled draw calls. Shadow passes, SRP batching, instancing, view occlusion and partial triangle visibility affect actual GPU work.

The six live Chapter camera receipts give frustum-visible upper bounds. Near approach/beside views can include the complete 279,694-triangle installation. These are conservative mesh costs, not Android GPU timings. No 60 FPS or heat acceptance is inferred from them.

## Distance strategy

LOD0 retains the accepted 45 renderers. LOD1 omits polish meshes, small variants and safety-rail detail. LOD2 retains the major foundation, central casing/heart, four horizontal machinery masses, four cyan vertical towers and four radial routes. Renderer lists reuse the accepted meshes; no destructive mesh simplification or extra duplicate render hierarchy is introduced. The three original editable Blender sources remain byte-identical.

LODGroup uses screen-height transitions `.38 / .30 / .08`, size approximately 11.48 m, and no cross-fade/double rendering. At the 48-degree lens these correspond approximately to center distances 34 / 43 / 161 m, before quality LOD bias; the production camera far clip is 100 m. The first two switches can occur while approaching this large landmark; close presentation remains LOD0. This is more useful for the fixed camera than thresholds that keep full detail until the object is almost off-screen. A future device profile may justify material/submesh consolidation or further source-derived simplified meshes, preserving close visual fidelity.

Do not copy this full render cost into every facility. This pass places one instance only. The architecture language extracts quality principles rather than replicating its shape or renderer budget.

## Collision and circulation

Twenty-five BoxColliders form a separate `Architectural collision` hierarchy: four crossed plinth boxes, one central casing, eight node/base/support boxes, four radial bridges and eight stair risers. Fine bolts, rails and pipe fittings do not own detailed mesh collision. No MeshColliders, rigidbodies, proof cameras or runtime behaviors are added to the art prefab.

An optional serialized external collision-root reference on ChapterVisualEnvironment injects this hierarchy into Chapter01WorldPresenter. World composition adopts it under Gameplay Geometry without changing its world transform. All boxes use the existing HardBlocker layer and enter the existing blocker list and tactical-map topology. Collision is shared by actual controllers, configured wall envelopes, obstruction masks and map geometry; no scene-wide runtime discovery is used.

The initial proof placement/collision blocked the old straight validation route to `(17,54)`. Four environmental route waypoints now pass through `(4,58)` and `(18,58)` on the outward and return paths; all original destinations, strong-spot positions, surfaces and blockers are preserved. A 0.65 m placement correction widens the northern machinery passage sufficiently for Cutter's unchanged 1.46 m visual wall envelope. G-0's unchanged 1.2 m locomotion envelope is exercised directly through the same route.

Seven accessible inward approaches are blocked by solid machinery. Two diagonal pockets and a 48-segment west/north/east controller perimeter are checked. The south side adjoins the existing closed platform rim and is not a service through-route; do not describe it as full 360-degree player circulation. A separate actual PlayerLocomotion test validates the usable northern passage in both directions. Existing ordinary-actor and full-Chapter route tests cover the widened passage and required objectives/arena approaches.

## Presentation review

The six unretouched live Chapter images retain the production lens and scene lighting. G-0 remains the unchanged bipedal Tier 0 at unit scale. The orange center and smaller cyan towers remain distinguishable; stairs/plinth contact the existing deck, source lighting/materials are retained, and no additional permanent lights are introduced. The close view can crop outer nodes as the accepted close lens does; approach/wider context provides the full installation relationship. Source preservation plus renders support the absence of new floating parts or z-fighting, but desktop evidence does not replace the final human APK gate.
