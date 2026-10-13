# Energy Chamber — vertical distribution node correction V1

The four external assemblies now have a related vertical cylindrical stabilization
module as their primary silhouette. The existing horizontal tanks remain underneath
as compressors/reservoirs. The central chamber's contained energy is warm orange;
the four peripheral cores are cool blue/cyan, following the explicit owner correction.

## What stayed

- Accepted main chamber, stepped base, four node roots at 4.40 m radius and existing
  core-to-node carrier layout. All nine prior custom FBXs remain byte-for-byte intact.
- Horizontal generator/tank, centrifuge, plinth, stair and rail source geometry and
  placements. The new modules fit above the retained reservoirs without resizing them.
- Overall installation bounds: 11.48 × 6.095 × 11.48 m. Central height remains 6.11 m;
  the smaller vertical modules reach 4.12 m from the same node platforms.
- Current G-0 prefab at unit scale, gameplay FOV 48 and its existing camera anchors.
- No gameplay authority, collision, Chapter scene, production prefab, global art
  direction, new package or shared rendering settings were authored.

## Focused correction

One reusable custom module is added to the master DistributionNode prefab, referenced
by all four existing cardinal assemblies. It contains an exposed cylindrical cyan
core, formed containment braces, narrow protective armor sectors, structural belts,
collector/crown flanges, keyed clamps and status apertures. A bolted two-leg support
frame clears the retained reservoir; twin fluid risers branch from the existing inlet
manifold into the collector, and a visible equalization return meets the reservoir.
Protected telemetry cables complete the interface. All four modules share this design.

The central orange energy insert stays inside the accepted vessel envelope. Only the
central energy slots are remapped to warm materials; the structural shell/frame/base
finish hierarchy remains. Existing small node/service indicators stay cool. Energy
surfaces use opaque URP Lit materials with authored containment-band emission masks,
normal and metallic/smoothness response. No transparent glow shell or point lights
hide the structure. URP's saved emission flags are explicit so material validation
does not silently remove the emission keyword after reload.

## Files and origin

Scene/prefab stay at:

- `Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity`
- `Assets/_Game/ArtProof/EnergyChamber/Prefabs/EnergyChamber_HybridProof.prefab`
- `Assets/_Game/ArtProof/EnergyChamber/Prefabs/DistributionNode.prefab`

New **CUSTOM GEOMETRY**:
`Models/EC_VerticalDistribution.fbx` and `Models/EC_CoreHeart.fbx` within that proof.
Editable project-owned source:
`art/energy-chamber-vertical-nodes-v1/EnergyChamber_VerticalNodesV1.blend`.
New energy materials: `EC_NodeCool`, `EC_CoreWarm`, `EC_CoreInnerWarm`, with maps
under proof `Textures/VerticalNodesV1/`. Tools live under the matching `Tools/` pass.
The scoped builder, exporter, runner and preservation regression are updated.
Full changed-file inventory: `FILES_CHANGED.txt`.

No new external assets were imported. Existing VR3 generators, centrifuges and
pipe holders, plus accepted CC0 stairs/rails, remain in use. Canonical current intake
archives were inspected again. The actual Cryo Tube source capsule has a closed,
low-detail dome and does not provide the required exposed stabilization structure;
the new primary silhouette is custom. Existing source/license records stay intact.
`source-review.json` records the intake decision. The current Chapter 01 visual
blueprint image was reviewed for the cylindrical containment/material language;
the owner's latest explicit warm-center/cool-periphery hierarchy controls this pass.

## Evidence and remaining limits

Direct Unity captures include gameplay, gameplay with unchanged G-0, true player-follow
anchor, low and high 3/4, normal and close 3/4, emission-disabled gameplay/detail,
and an isolated vertical-node detail. Previous pass captures remain unchanged.

The warm main chamber remains larger and dominant; the four blue cylindrical elements
now define the peripheral assemblies. Horizontal machinery is visibly secondary.
Diagnostic views keep the frame, cylinders, gantry and physical pipe paths readable
with emission disabled. Natural warm/cool albedo remains in that diagnostic.

Another production finishing/optimization pass is still needed: close-view normal and
shadow response, inherited donor fin treatment, crowded existing service crossings,
final asset-specific surface masks, LODs and material consolidation. Current assembly
is 279,694 triangles / 45 renderers before the stage/G-0. Device performance and the
human visual gate remain outstanding. This focused correction does not approve
production integration or a merge.

## Validation / source control

Full EditMode suite: **534 passed / 0 failed**, including five proof checks. The
new regression verifies four vertical modules, retained reservoir instances and
persisted warm/cool emission wiring. Dedicated scene PlayMode smoke:
**1 passed / 0 failed**. Direct normal/no-emission captures also demonstrate the
rendered energy contribution after scene reload; `emission-visibility.json` records
the comparison. Automated checks do not replace the human art gate.

Unity compile and project validation passed. The three contained-energy materials
retain `_EMISSION` with valid emission flags after both project reloads. The prefab
remains presentation-only with zero colliders and zero gameplay MonoBehaviours.

Executed test, compile, validation and APK receipts are recorded in `verification/`.
The Android build uses the exact saved proof scene in the existing isolated static
review project, keeping gameplay and production Chapter callbacks out of its package.

Android development APK: **Succeeded**, ARM64 IL2CPP / portrait, separate package
`com.gravivore.energychamberproof`. File:
`Builds/Android/gravivore-energy-chamber-vertical-nodes-v1-dev.apk`, **70,949,045 bytes**.
Actual APK hash, ARM64 archive/license checks and matching saved-scene hash are in
`preservation-and-delivery.json`; `apk-badging.txt` records package metadata.
Unity's aggregate build-summary byte count differs from the actual APK file size.

Three non-fatal logged errors remain from the installed URP Core
`ProbeVolumeBakingResources` loader's `TraceRenderingLayerMask.urtshader` asset type
mismatch, as in the preceding builds. The snapshot had no missing/unsupported
materials or C# compile errors. The SDK package-list network request waited over
600 seconds, then completed naturally; the original build succeeded without a
restart, SDK modification or retained timeout workaround. `sdk-wait.json` preserves
the diagnosis. Device performance and human device art acceptance were not run.

Baseline: `bd886ef6e96aab172b9b469877f382dfc83406fd`.
Branch: `art/energy-chamber-hybrid-proof-v1`.
Draft PR: <https://github.com/Shwedsky/gravivore/pull/77>. No merge.
Assumption: retain the useful horizontal machinery in its accepted position and mount
the new cylinder on a fitted support frame rather than replace the entire side assembly.
