# Chapter 01 architecture production pass V1

Branch: `integration/chapter01-architecture-production-v1`. [Draft PR #78](https://github.com/Shwedsky/gravivore/pull/78), unmerged. Base main is `e1669362fd3566f69939b547f263a3a600e3e1b7`, freshly fetched and unchanged from task preparation. Accepted chamber source is `7d0e375f7b0a12a9d46982af685cabe3ce57d731`, PR #77. The production/runtime checkpoint is `1077d8c0298442b645dc7fe9921b3b6164af33fb`; final delivery metadata/tooling are committed afterward. Final HEAD/counts are verified on the PR at delivery; [FILES_CHANGED.txt](FILES_CHANGED.txt) inventories the complete scope.

## Delivered content

- Production prefab: `Assets/_Game/Content/Chapter01Architecture/EnergyChamber/Prefabs/EnergyChamber_Production.prefab`.
- Production folder: `Assets/_Game/Content/Chapter01Architecture/EnergyChamber/`, containing the accepted runtime model/material/texture closure, reusable DistributionNode prefab, two cleared CC0 donor exports and license records.
- Actual Chapter integration: `Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity`, root `Energy Chamber / production`, position `(11,.08,50.65)`, identity rotation, unit scale.
- Editable project-owned sources: the three accepted `.blend` files under `art/energy-chamber-*`, with [source custody](../../../../art/ENERGY_CHAMBER_SOURCE_CUSTODY.md).
- [Architecture language](../../../CHAPTER01_ARCHITECTURE_LANGUAGE_V1.md), [complete architecture audit](../../../CHAPTER01_ARCHITECTURE_AUDIT_V1.md), [render/collision assessment](PERFORMANCE_AND_COLLISION.md), six live Chapter screenshots and fifteen architecture inspection screenshots.

One Energy Chamber is promoted; no other architecture is rebuilt in this branch. Its accepted local silhouette, proportions, plinth, four cyan towers, orange core, radial routes, support arrangement, meshes, materials and UVs are preserved. Original asset GUIDs survive the promotion; shared dependencies remain references rather than copies. Proof scenes/cameras/test helpers/editor logic/screenshots/review APKs are absent from runtime dependencies. Licenses and original donor mapping remain available in the production folder, root ThirdPartyNotices and promotion receipts.

Production placement is 0.65 m south of the accepted isolated Chapter-context reference. This is a clearance correction after the real actor-envelope tests, not an art redesign. The plinth and machinery stay on the existing deck. Scene lighting, reflection, grading and G-0 art remain unchanged; no permanent light is added.

## Collision, camera and performance

Twenty-five simple BoxColliders represent the plinth, central casing, four machinery bases/vertical supports, four radial service bridges and two compound stairs. They use HardBlocker and are explicitly bound to the world composition. Runtime adoption into Gameplay Geometry preserves world transforms, gives wall-envelope/obstruction systems the same collision, and includes all twenty-five boxes in the tactical map. The art owns no runtime behavior. Two small presentation/world files add this optional injected-root facility; no scene-wide runtime discovery or gameplay-domain dependency is introduced.

G-0 passes seven accessible blocked approaches, two service-pocket checks and the available west/north/east controller perimeter. The actual PlayerLocomotion algorithm also traverses the northern passage both ways with its unchanged 1.2 m wall envelope. All five ordinary actors pass the complete route/arena clearance regression, including Cutter's unchanged 1.46 m visual envelope. The south edge adjoins the existing closed platform rim and is not a through-route. Four added environment waypoints bypass the chamber through `(4,58)` / `(18,58)`, preserving every original destination, strong-spot position, surface and blocker.

The live views use the production 48-degree portrait lens, offset `(3.4,19.8,-13.6)` and look-at height `3.2`, with unchanged unit-scale Tier 0 G-0. Approach, beside, close, wider context, collision/navigation and normal emissive views are retained under [screenshots](screenshots/). Orange/cyan hierarchy remains readable; close frames may crop peripheral nodes as the accepted lens does. No new floating parts or z-fighting are evident in the preserved art and reviewed renders.

LOD0: 45 renderers, 279,694 triangles, 156 material slots, 22 unique materials. LOD1: 29 / 181,124 / 88 / 20. LOD2: 15 / 142,836 / 72 / 17. Screen-height thresholds `.38/.30/.08` preserve accepted nearby rendering and remove secondary polish/rails/variants at distance. No cross-fade or destructive source simplification. Gameplay-camera frustum records are conservative visible-cost bounds; actual GPU draws, shadow cost, Android FPS and heat remain device measurements. Do not repeat this landmark's full cost throughout the Chapter.

## Audit and next target

KEEP: Energy Chamber; supporting floor-access/grate/rail vocabulary.

REFINE: CapacitorField, CuttingFloor, IndustrialTransfer, ChapterServiceNetwork composition, supply routing/cabinet clusters and arena infrastructure. Their main functional silhouettes or route relationships work; material/depth/connections/secondary structure need refinement.

REBUILD: RepairHub, RelayYard, ShieldDump, HaulerGraveyard, major gate/lintel transitions and large wall/housing families. Their primary architecture remains floor-mark, generic box/frame or toy-like wreck language. The audit evaluates function, silhouette, depth, structural logic, G-0 scale, material hierarchy, actual lens and collision responsibility for each group.

MagnetarComplex and CustodianCore are REFINE targets whose implementation is intentionally deferred. Their identifiable induction/containment silhouettes work, but elite/boss approach, footprint, telegraph and gate coupling require coordination before replacement work.

Next recommended task: **`CHAPTER01_REPAIR_HUB_ARCHITECTURE_PROOF_V1`**. Repair Hub is frequently visible, central to Chapter identity, visibly weak in primary form and isolated from active combat/actor ownership. Next priorities are one non-progression gate/transition, Relay Yard, Hauler Graveyard and Shield Dump; the audit contains the complete ranking and reasons.

## Executed validation

- Unity compile/authoring: successful after correcting initial tooling errors.
- Final full EditMode: **531 passed, zero failed**.
- Final PlayMode integration: **6 passed, zero failed**, covering wiring/shared tactical-map boxes, controller collision, actual locomotion envelope, live capture, all ordinary actor routes/arena clearance and full Chapter traversal/map authority.
- Existing project validation: passed, including Blueprint World R2, Connected Concept Sectors and Actor Integration audits.
- Source/GUID/dependency/scene/layout preservation: passed. Existing scene fields are retained; two new serialized records plus one collision reference/root link are added. Only four route waypoints alter the old physical-layout data. No duplicate asset GUIDs or project proof/editor dependencies are introduced.
- Earlier broad PlayMode run: 11/13 passed and exposed the two obsolete straight-route failures. Both are repaired and passed in the final integration run. Its five-minute ordinary combat/traversal soak passed; that earlier observation is supporting Editor evidence, not final Android performance acceptance. Failed/corrected attempt XML is retained transparently alongside final receipts.
- Dev Android packaging: **succeeded**, portrait ARM64 IL2CPP, package `com.gravivore.mobile.dev`, version `0.1.0`, version code `55`, 146,287,814 bytes. Local APK: `Builds/Android/gravivore-dev-0.1.0+55.apk`. SHA-256: `0b9ee3dc31185f6084bc8c961637e7624375404e013f0dd16c5f6cfa81593ffa`. The actual build package/ZIP/version/hash receipts are in [delivery-checks.json](verification/delivery-checks.json) and [apk-badging.txt](verification/apk-badging.txt). The remote SDK stall and genuine installed-SDK retry are documented in [ANDROID_BUILD_NOTE.md](ANDROID_BUILD_NOTE.md); SDK preference before/after paths match exactly. No probe-volume shader errors were reported by this successful build.

## Ownership and limitations

Production files touched: the Chapter scene, new architecture content and the physical-layout route array. Runtime changes are restricted to optional collision-root reference/adoption in ChapterVisualEnvironment and Chapter01WorldPresenter. No enemy/elite/boss, combat, progression, persistence, loot, player gameplay, actor art, encounter definitions or package files change. All previous serialized scene fields are preserved rather than overwritten.

Remote divergence was inspected. The ahead-of-main legacy `chatgpt/phase3-visual-world-baseline-dev` scene change adds `_phase3EnemyVisualCatalog`; that field/branch is outside this pass and is not overwritten. Current actor proof and accepted chamber proof leave production scene/layout untouched. Future simultaneous scene/layout changes must reconcile the new root, collision reference and route additions. PR #77 remains an unmerged proof; merging both branches requires reconciling its GUID-preserving asset move rather than retaining duplicate ArtProof and Content GUIDs.

The original dirty user checkout is preserved; work occurs in an isolated checkout. Unity-generated unrelated importer/settings/material changes and regenerated old report files are excluded/restored before final delivery. Private SDK mirror, caches and APK binaries remain ignored/local. No new on-device human visual gate, Android FPS/heat result or A-level production-art claim is made. The human playable-APK gate remains required. Assumptions: one static landmark, no chamber mechanic; a closed south platform edge; approved nearby art is preserved while distance detail may reduce; other facility work remains audit-only.
