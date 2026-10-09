# Phase 3D final static self-review

This review is the final static gate before Unity 6000.3.0f1 and Android device verification for PR #48. It supersedes the provisional static A/B/C grade claims in `PHASE3D_VISUAL_QUALITY_PASS.md`; those earlier grades were intentionally optimistic implementation grades, not a final-art verdict.

## Blockers

All 11 Phase 3D environment blockers resolve to authored `ChapterVisualEnvironment` region ids and remain gameplay-side `HardBlocker` BoxCollider proxies with no Renderer or MeshFilter. No collider was added to a pure Phase3D visual prefab.

The canonical world/spawn review uses Chapter 1 zone centres, four ordinary spawn offsets per zone, elite centre `(0,0,69)`, boss centre `(0,0,94)`, elite gate `(0,0,60)` and boss gate `(0,0,80)`.

The first static pass found three proxies too close to authored outer-loop traversal, including a direct centre-line intersection at the Capacitor Field right bank. The existing 11 blockers were retained, but the affected proxies were made conservative interior blockers rather than broad outline blockers:

- Shield Dump left/right emitter carcasses were reduced to interior solids.
- Capacitor Field left bank was narrowed; the right-bank proxy now covers the forward solid capacitor bank instead of bridging multiple modules across the Shield Dump -> Capacitor Field traversal line.
- Hauler Graveyard chassis proxies were reduced to interior chassis volumes.
- Elite Arena containment and Boss Arena crane were already well clear of the canonical central route and were not expanded.
- Repair Hub left/right service-frame proxies remain outside the authored south exit.

A PlayMode regression now checks all 11 blockers for HardBlocker ownership, renderer/mesh stripping, canonical ordinary spawn clearance, elite/boss centre clearance, gate-centre clearance, radial/outer-loop centre-line clearance, elite/boss traversal clearance, and Repair Hub south-exit clearance.

## Transform assumptions

`AddEnvironmentBlocker` currently places each authority proxy with `region.Root.TransformPoint(localCenter)`, copies `region.Root.rotation`, parents it under `GameplayRoot`, then applies `localScale = size`.

For the current Chapter 1 hierarchy this is correct because the relevant authored region roots and gameplay root are unit-scale and the blocker regions are identity-rotated. Therefore no transform-code change was made. The hidden assumption is now guarded by PlayMode assertions: relevant region roots must remain unit-scale/identity-rotated and collider world bounds must match the authored proxy scale. If a later art pass scales or rotates those roots, that test intentionally fails and blocker conversion must be revisited instead of silently distorting collision.

## Motion lifecycle

`ChapterVisualEnvironment` remains the owner of Phase 3D motion initialization; it was not moved into `S01SceneCompositionRoot`.

The lifecycle was tightened:

- repeated successful `Initialize()` returns without creating another presenter;
- the created presenter is stored by the environment;
- `OnDestroy()` explicitly destroys the sibling presenter so environment replacement cannot leave an updater behind;
- the presenter performs exactly one `GetComponentsInChildren<Transform>(true)` scan during initialization;
- `Update()` only iterates cached channels and performs no Find/GetComponents hierarchy query;
- missing optional named/prefix motion targets are skipped safely;
- actor authority roots are not scanned or animated.

A PlayMode regression calls `Initialize()` again and requires exactly one `Phase3DMechanicalMotionPresenter`.

## Gate presentation

The existing `Physical Gate Barrier` collider remains the authoritative centre blocker. Phase 3D visual pieces are presentation-only primitives; their generated colliders are disabled and destroyed. Unlocking disables the gameplay centre collider and hides the full `Containment Gate Assembly`; left/right flank walls and their visuals remain as the existing permanent side geometry. Locking restores the centre collider and assembly.

Each locked gate uses one frame material and one energy material for the assembly, plus the bounded authority material already tracked by `Chapter01WorldPresenter`; materials are shared within the gate and destroyed by the world presenter. No realtime Light is added. The assembly is explicitly placed at the gate world pose; current visual hierarchy is unit-scale. Unity/device verification still owns the final camera-readability verdict.

## Prefab serialized safety

The 16 Phase3D prefabs are component-preserving derivatives of the accepted Phase3B pure-presentation sources. The Phase3D authoring transform changed names, transforms and renderer material references; it did not add gameplay components or replace mesh references.

Static review confirms:

- no Phase3D visual prefab intentionally contains Collider, Rigidbody, Camera, Light or gameplay MonoBehaviour;
- renderer materials are constrained to the six Phase3D material GUIDs;
- material shaders are inherited from the existing valid URP source materials rather than introducing a new shader dependency;
- mesh references are inherited from the Phase3B source prefabs, so the pass introduces no new mesh GUID dependency;
- authored Phase3D scale edits are finite and strictly positive;
- existing integration/socket transforms are preserved unless a purely visual node was deliberately renamed; current integration binding names used by tests remain present.

`Phase3DVisualQualityTests` remains the Unity import-time proof for these serialized assumptions. This static review does not claim that Unity has run it yet.

## Strict visual grade

The final-art target is a modern, cohesive, smooth mobile-game presentation. Against that target, transform/material recomposition of the existing simple mesh inventory is not enough to justify B grades merely because Phase 3D is better than Phase 3C.

- Cutter: **B** as the strongest unchanged internal reference, but not an absolute final-art A under the stricter target.
- Scout: **C** — clearer silhouette, still simple-kit/primitive construction.
- Arc Drone: **C** — prongs/supports and body remain visibly modular and primitive.
- Warden: **C** — improved mass, but shields/feet/hull still read as hard assembled modules.
- Carrier: **C** — improved logistics silhouette, but cargo/chassis/supports still expose box-like kit construction.
- Magnetar Guard: **C** — hierarchy/asymmetry improved, but carapace/generators/supports remain primitive and mechanically under-resolved.
- Custodian M-0: **C** — stronger asymmetry and role readability, but large primitive masses, simple joints and unsupported transitions remain.
- Repair Hub: **C** — now reads more clearly as a service bay, but manipulator arms/joints and structural transitions remain simple-kit quality.
- Relay Yard: **C** — still dominated by basic cabinets/mast modules.
- Cutting Floor: **C** — layout is clearer, but presses/bridge/blades remain hard primitive assemblies.
- Capacitor Field: **C** — ring problem reduced, but repeated vertical banks remain conspicuously modular.
- Shield Dump: **C** — better damaged composition, but broken loops/carcasses still reveal primitive/ring construction.
- Hauler Graveyard: **C** — better staging/asymmetry, but wrecks are still scaled simple chassis/cargo forms.
- Elite Approach: **C** — containment read is improved, finish remains simple-kit.
- Elite Arena: **C** — destination composition improved, perimeter pieces/emitters still read as basic modules.
- Boss Approach: **C** — better damaged industrial staging, not final-quality geometry.
- Boss Arena: **C** — crane/perimeter story helps, but primitive construction remains obvious.
- Industrial containment gates: **C** — functionally clearer, but intentionally built from runtime cube/cylinder primitives and therefore visibly prototype-level.

This is not a request to redesign PR #48 before verification. It is an honest static verdict: the pass improves cohesion/readability and removes some worst Phase3C problems, but Unity/device review is still required to determine whether gameplay-camera distance hides enough of the remaining primitive finish to accept the pass as an interim visual baseline.

## Static performance delta

Relative to the corresponding Phase3B/Phase3C art prefabs, the 16 Phase3D prefabs preserve renderer count, mesh references and material-slot count by construction. The new art pack consolidates its renderer assignments to six shared Phase3D materials.

Runtime deltas introduced by #48 are bounded:

- +11 BoxCollider environment proxies, with zero Renderer/MeshFilter on those authority objects;
- +1 Phase3D mechanical-motion presenter per initialized Chapter visual environment;
- one initialization hierarchy scan, then cached-channel updates only;
- containment gate assembly increases locked-gate presentation from the old single visible centre barrier plus two flanks to eight assembly renderers plus two flank visuals; the hidden legacy centre visual remains non-rendering, so the visible delta is +7 renderers per locked gate;
- no realtime Light, Rigidbody, Camera or particle stack was added by Phase 3D.

No FPS claim is made from static review.

## Cross-PR ownership

Changed-file lists were compared against active PRs #45, #46 and #47 after the fixes above. PR #48 has zero file overlap with all three.

- #45 owns persistence/release hardening and Android build files.
- #46 owns the map/minimap presentation module and its tests/docs.
- #47 owns the Phase6B audio/VFX production pack and its tests/docs.

`S01SceneCompositionRoot.cs` remains unchanged in #48.

## Static gate result

Static integration risks found during this review were corrected without redesigning Phase 3D. The branch is ready for Unity compile/tests, real gameplay-camera capture, Android build, device performance review and human visual judgement. It is not approved for merge until those checks pass.

**PHASE 3D STATIC REVIEW COMPLETE — READY FOR UNITY / DEVICE VERIFICATION**
