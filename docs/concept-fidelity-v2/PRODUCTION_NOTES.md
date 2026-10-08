# Concept fidelity V2 production notes

The route follows the existing Chapter 01 quest spaces: repair/spawn, capacitor
and hauler sectors, relay and shield sectors, cutting floor, elite approach,
Magnetar, containment threshold and Custodian. The complete chapter remains
playable. Its progression requirements and collision topology remain authoritative.

## Authored assets and provenance

All 18 new meshes and three new texture atlases are original project-created
assets. `Tools/concept-fidelity-v2/build_assets.py` authors and exports them in
Blender 5.2.2 LTS. Compressed editable sources are in `art/concept-fidelity-v2`;
production FBX, prefabs, controllers and atlas maps are in
`Assets/_Game/Content/ConceptFidelityV2`. The script uses the existing project's
Blender export utilities, without extracting geometry from another game or
introducing an external asset, package or license. Existing baseline asset
attributions in `ThirdPartyNotices.md` continue to apply outside this new pack.

Custodian has 89 authored mechanical parts consolidated into one rigid skinned
mesh per LOD, a 12-bone rig and Idle, Run, Attack, Windup, Release, Special, Hit
and Death clips. LOD triangle counts are 16,092 / 8,366 / 3,862. Its split curved
shell surrounds an exposed upward-facing internal core and mechanical cage.
The pressure lance, restraint clamp, rear supply trunk and maintenance panels
are asymmetric. Four cast rocker supports spread its load. Root motion is
disabled, and the prefab contains no collider or gameplay script.

The hub uses a separate authored maintenance platform, six reusable authored
servo/tool component meshes and a scanner aperture. Two arms observe the same
repair-active authority as the accepted implementation. Tool brightness uses
one reused MaterialPropertyBlock. The platform, arm meshes and scanner never
heal, damage, move gameplay actors or create materials during repair.

Reactor, turbine and containment vessel each have a three-bone rig, three LODs
and an authored machinery loop. Deck tiles, curved service bundles, bowed
bulkheads, pressure wrecks and containment frames are opaque static meshes.
All new renderers share one atlas material. Sixteen atlas regions separate
painted metal, steel, dark mechanics, worn metal, rubber, energy and scorch.
The three 512-pixel maps use ASTC 6x6 on Android with mipmaps. The metallic and
smoothness map changes surface response as well as color.

## Camera review and corrections

Review uses the live portrait follow camera at 540 x 960, including the actual
HUD, minimap and boss health display. `internal/01_repair_active.png` through
`16_custodian_shutdown.png` cover the route and boss clip poses; sustained
captures show real movement and combat rather than isolated model renders.

The internal review rejected the first regular deck pattern and rebuilt it as
large irregular fitted plates with narrow seams, overlapping welded patches,
service openings, inset cable runs, drainage covers and controlled scorch.
Variants rotate along the path; sparse hazard marks replace a uniform dashed
overlay. Lateral sector connections receive the same deck language.

The review also corrected static FBX axis handling: a neutral placement parent
preserves the imported source correction. Deck bounds are now flat and are
asserted in PlayMode. Pressure machinery was revised from closed upper domes
to open cradles and split crowns so its recessed source remains visible at the
game camera. Existing prop art at replaced footprints is retired while its
independent collision proxies remain in place.

## Runtime budgets

The world retains one shadowed key light. Two unshadowed point lights are reused
at the nearest authored energy anchors; they are configured in ConceptFidelity.
One prewarmed spark instance supplies intermittent nearby service faults.
Animated machinery uses existing Animator functionality. No permanent particle
field or transparent shell layer is added. HDR energy is contained behind
opaque cages; the single restrained bloom pass has intensity 0.20, threshold
1.15 and high-quality filtering disabled.

Static environment parts opt into static batching. Art placement adds no
collision obstacle. Runtime presentation scans its injected environment once
and caches light colors; it does not search the scene per frame, clone
materials, create meshes, or allocate new effect instances during traversal.

## Verification boundaries and assumptions

Blender source validation reopens every saved file and checks real geometry,
finite vertices, UVs, material count, triangulation, pivots, LOD budgets, skin
weights and boss clips. Unity tests check authoritative HP/root/collision
preservation, repair activation, scene dependencies and fixed presentation
inventories. APK callbacks independently check length-prefixed serialized mesh,
config and material names inside the delivered archive.

The five-minute test uses real CharacterController locomotion and the actual
combat, save and cooldown systems. Development gate unlock and god mode make
all route spaces reachable during this regression run; the test does not alter
production balance or shorten the normal first-play progression. Its clocks,
pathfinding, assertions and capture work are test instrumentation. Editor frame
gaps are evidence of an instrumented editor run, not Android FPS measurements.

The referenced approved image board was not supplied with this task and was
not found in the repository. The owner's detailed written concept direction
and accepted G-0/enemy assets therefore define this pass. Visual acceptance and
mobile performance remain the final owner device gate. Existing bounded
90-second cold-start diagnostics remain intact for that gate. No cause of the
previous device-only first-launch slowdown is claimed from editor evidence.

Next gate: final v41 concept-fidelity APK on the owner's Android device. No new
spec ID or Chapter 02 work is authorized by this milestone.
