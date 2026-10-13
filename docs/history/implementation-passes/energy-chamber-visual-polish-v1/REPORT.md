# Energy Chamber Visual Polish Pass V1

The accepted composition is preserved. This pass improves surface roles, fitted
mechanical interfaces and the chamber's focal termination. It is ready for a
focused production finishing pass; it is **not approved for production integration**.
No merge, gameplay integration or shared production asset edits were performed.

## Preserved

- Accepted baseline: `5d1c981a4e5796335e5d0e8a2059d28200d45a4e`.
- Four primary custom FBXs are byte-for-byte identical, verified against that commit.
- Central chamber at the same origin and height; circular stepped base unchanged.
- Four cardinal master nodes remain at 4.40 m radius, with the same four carrier routes.
- Overall rendered bounds remain **11.48 × 6.095 × 11.48 m**.
- Existing machinery, stair and railing source geometry and placement remain intact.
- Current G-0 Tier 0 source is unchanged at unit scale; measured height remains 1.771 m.
- Same gameplay settings: perspective FOV 48, offset (3.4, 19.8, -13.6), look-at height 3.2.
  Both the original inspection anchor and actual G-0 follow anchor were recaptured.
- Original proof screenshots and receipts remain available as before evidence.

## Improved

- Material hierarchy now separates warm neutral cast structure, light containment
  armor, brushed steel fluid pipes, stamped service deck, graphite machinery,
  rubber seals and selective copper compression fittings. Each finish has authored
  base color, normal, metallic/smoothness and occlusion maps; Android ASTC import
  settings are explicit. These are surface finishes, not flat palette swaps alone.
- The crown gains fitted segmented taper covers, locking interfaces, compression
  ferrule and a small visible status aperture. Recessed induction turns and narrow
  junction accents communicate a contained mechanism inside the existing vessel.
- Carrier splice shoes, docking transitions, generator isolation pads, bolted
  manifolds and protected cable runs make the node/base/core handoff more explicit.
- Lower compression shoes, gasket seams and deck bearing pads strengthen the
  impression of load transfer without changing the accepted base silhouette.
- Paired latches, access plates, node status lamps and service identification add
  detail where maintenance or connection work occurs. Opaque marking plates avoid
  transparent decal sorting and overdraw.
- Two opposite nodes have a modest auxiliary service pod/cable variation. All
  four still use the same master node and machinery arrangement.
- Compression clamps, paired feed/return channels and restrained double chevrons
  provide a recurring GRAVIVORE motif within the current industrial direction.
- The proof-only floor retains its geometry and is quieter in material so the
  installation's hierarchy is easier to assess. Lighting and bloom are unchanged.

## Visual review

All screenshots are direct Unity camera renders, with no retouching. Reviewed:
low technical, high 3/4, gameplay without G-0, gameplay with G-0, exact player-follow
anchor, emission-disabled gameplay and close detail, normal 3/4 and close 3/4.

The central chamber remains dominant. The crown accents now survive the gameplay
angle, and the major base steps, bright pipe routes and darker side machinery remain
legible with emission disabled. The no-emission view disables the shader emission
keyword/color; naturally cyan-colored physical status inserts retain their albedo.
There is no volumetric glow or transparent energy shell concealing the structure.

Required review images:

- `screenshots/03_gameplay_camera.png`
- `screenshots/07_close_three_quarter.png`
- `screenshots/04_gameplay_G0.png`
- `screenshots/05_emission_disabled.png`

Additional geometry diagnostic: `screenshots/05b_emission_disabled_detail.png`.
High view: `screenshots/02_high_three_quarter.png`.

## Still weak / next art work

**Another focused art pass is needed before production integration.** The layout
does not need another concept exploration or architectural reconstruction.

- Close views expose faceting on the accepted armor/rings and jagged shadow
  response at the upper vessel. Production shading/normal baking and deliberate
  surface masks should address that while keeping the silhouette.
- The broad containment panels and crown still feel comparatively clean. Their
  subtle tile detail does not replace a final, asset-specific UV and wear treatment.
- Generator fin stacks retain recognizable donor shapes. The fitted saddles and
  docking details improve coherence, but final mask/trim treatment and selective
  donor cleanup are still warranted for a Chapter landmark.
- Existing continuous donor rails crowd pipe and stair crossings. A localized
  service-junction cleanup is needed, without moving the stairs or radial layout.
- Fine plaques and cable details become secondary/subpixel at gameplay distance.
  They support close inspection; readability still depends on the larger mass and
  finish separation, as it should.
- Current assembly is 210,202 triangles / 40 renderers, before the stage and G-0.
  This is an art-review assembly without production LODs or final material
  consolidation. Device profiling and the human visual gate remain outstanding;
  no 60 FPS claim is made.

## Assets and files

Scene: `Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity`.
Root prefab: `Assets/_Game/ArtProof/EnergyChamber/Prefabs/EnergyChamber_HybridProof.prefab`.
Master node: `Assets/_Game/ArtProof/EnergyChamber/Prefabs/DistributionNode.prefab`.

Five new **CUSTOM GEOMETRY** overlays under the same proof's `Models/`:
`EC_FoundationPolish`, `EC_ChamberPolish`, `EC_ConnectorPolish`, `EC_NodePolish`,
`EC_NodeVariant`. Editable source:
`art/energy-chamber-visual-polish-v1/EnergyChamber_VisualPolishV1.blend`.
Only the overlay source is reauthored; the original Blender source remains unchanged.

Materials and new project-owned maps: proof `Materials/` and `Textures/PolishV1/`.
The donor palette adapts the existing project-owned
`Assets/_Game/Content/SurfaceHeroV46/Textures/V46_Legacy_BaseColor.png` while preserving
its UV panel layout. Source generator geometry was inspected again directly;
measurements are in `node-donor-geometry.json`.

**No new external assets were imported in this pass.** Accepted CC0 stairs and
rails remain in proof `Donors/`; existing VR3 generator, centrifuge and pipe-holder
prefabs remain referenced. The canonical raw intake archives were inspected again,
recorded in `support-donor-review.json`; short fitted interfaces needed bespoke
dimensions. Previous license/provenance records remain in the original proof and
`Assets/_Game/ArtProof/EnergyChamber/Licenses/`.

Implementation tools are under `Tools/energy-chamber-visual-polish-v1/`, with scoped
updates to the existing builder, exporter, runner and serialization restore tool.
A new EditMode regression protects the accepted bounds and node transforms.
The full changed-file inventory is in `FILES_CHANGED.txt`.

## Validation and delivery

Unity compile: passed. Project validation: passed. Full EditMode suite:
**533 passed / 0 failed**, including four proof checks. Dedicated PlayMode scene
smoke: **1 passed / 0 failed**. Author/capture and presentation-only prefab
validation succeeded. The four primary mesh hashes, G-0 source, camera settings,
original Blender source and the unchanged footprint/cardinal layout were checked.

Android dev build: **Succeeded**, with **3 non-fatal logged errors** from the
installed URP Core `ProbeVolumeBakingResources` resource loader:
`TraceRenderingLayerMask.urtshader` host type does not match its registered asset
type. These are the same package-resource errors seen in the preceding proof build.
No C# compile error or missing/unsupported snapshot material was detected. They
remain recorded in `android-result.json`; build success does not waive them or
replace device validation. No shared engine/package workaround was introduced.

APK: `Builds/Android/gravivore-energy-chamber-polish-v1-dev.apk`, **70,497,529 bytes**.
SHA-256: `33f679aa7e0b0b553fd0eea54b29d2b74285303474b45d78530c97415a9c7be4`.
ARM64 IL2CPP / portrait / min API 26, separate package
`com.gravivore.energychamberproof`. The APK contains the proof scene and license
notices. The exact saved source scene was matched to the device project's copy.
The `android-result.json` bytes field is Unity's aggregate build-summary size;
the actual APK file size/hash are in `preservation-and-delivery.json`.

Executed results and APK receipt are recorded in `verification/` and summarized in
`verification/preservation-and-delivery.json`. Android uses the same exact saved
proof scene in an ignored, visual-only project, keeping Chapter gameplay and
production build callbacks outside this isolated review package.

Branch: `art/energy-chamber-hybrid-proof-v1`.
Draft PR: <https://github.com/Shwedsky/gravivore/pull/77>.
The accepted structure's branch/PR is continued for this authorized follow-up.
Assumption: production finishing and device approval follow this visual refinement;
there is no authorization to integrate or merge it here.
