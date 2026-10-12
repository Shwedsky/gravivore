# Energy Chamber — Hybrid Proof Build V1

**Verdict: READY FOR PRODUCTION DEVELOPMENT.** The proof demonstrates a viable hybrid construction at the current portrait camera. It is a visual feasibility assembly, not final production art or a Chapter feature. Device visual/performance acceptance has not been performed.

Dedicated scene: `Assets/_Game/ArtProof/EnergyChamber/Scenes/EnergyChamber_HybridProof.unity`.

Root prefab: `Assets/_Game/ArtProof/EnergyChamber/Prefabs/EnergyChamber_HybridProof.prefab`. Repeatable node: `Assets/_Game/ArtProof/EnergyChamber/Prefabs/DistributionNode.prefab`.

## Asset origin

The exact component record is [asset-origin.json](asset-origin.json). It includes original archive paths/entries, source hashes, runtime prefab/model paths, usage, origin category and whether an import is new. [promoted-donors.json](promoted-donors.json) records the two exact newly imported source files. [intake-candidates.json](intake-candidates.json), [donor-geometry.json](donor-geometry.json) and individual [source geometry views](donor-inspection/) document direct examination of the actual CURRENT archives and FBXs.

- **CUSTOM GEOMETRY — foundation:** `Models/EC_Foundation.fbx` under the proof area. Chamfered octagonal cast foundation, an open supported service annulus, radial load ribs, removable deck cassettes, recessed cooling louvres and a raised pressure seat. Height zones end at approximately 0.60, 1.37 and 2.11 metres.
- **CUSTOM GEOMETRY — chamber:** `Models/EC_CentralChamber.fbx`. Segmented opaque armor, contained induction sleeve, bowed tie rods, pressure belts, bolted steel rings, nested crown, terminal and narrow inspection emission. Major structural thickness is geometry rather than a painted glow.
- **CUSTOM GEOMETRY — tailored connections and node integration:** `Models/EC_Connector.fbx` and `Models/EC_SideNode.fbx`. Paired continuous feed/return pipes, in-line unions, cable channel, carrier bridge, common node foundation and docking manifold. One connector and one node master repeat by quarter turns.
- **DONOR-DERIVED — existing machinery:** `Assets/_Game/Content/VisualReplacementV3/Prefabs/VR3_generator.prefab` and `VR3_centrifuge.prefab`; models in its `Models/` directory. Original CURRENT package: `molten-maps-scifi/Molten Maps SciFi Asset Pack.zip`, entries `Molten Maps SciFi Asset Pack/Assets/fbx/Generator.fbx` and `Centrifuge.fbx`. The existing runtime derivatives are uniformly scaled, rotated, grouped and assigned a proof-local clone of their existing atlas material. No duplicate Molten import.
- **DONOR-DERIVED — existing support:** `Assets/_Game/Content/VisualReplacementV3/Prefabs/VR3_proppipeholder.prefab` / `Models/VR3_proppipeholder.fbx`; original `Modular SciFi MegaKit[Standard]/FBX/Props/Prop_PipeHolder.fbx`. One per carrier route. It is reused rather than newly promoted.
- **EXTERNAL DONOR — new stairs:** original `C:/Users/pamak/Documents/ChatGPT/gravivore/ExternalAssetIntake/Current/quaternius-modular-scifi-megakit-standard/Modular_SciFi_MegaKit_Standard.zip`, entry `Modular SciFi MegaKit[Standard]/FBX/Platforms/Platform_Stairs_4.fbx`; imported `Assets/_Game/ArtProof/EnergyChamber/Donors/Platform_Stairs_4.fbx`. Two uniformly scaled diagonal access flights. Top tread faces the service deck and rises to 1.325 m.
- **EXTERNAL DONOR — new rails:** the same original archive, entry `Modular SciFi MegaKit[Standard]/FBX/Props/Prop_Rail_Round_Big.fbx`; imported `Assets/_Game/ArtProof/EnergyChamber/Donors/Prop_Rail_Round_Big.fbx`. Four curved safety sections establish service scale. Native UVs retained; current R1 trim materials referenced.
- **EXISTING ASSET — G-0:** `Assets/_Game/Content/VisualSlice/Prefabs/G0_V3_Live_Tier0.prefab`, confirmed by `S07_Evolution.asset` Tier 0 override. Bipedal, unit transform scale. Only the proof scene instance disables presentation scripts; source prefab/model/material bytes are unchanged.
- **EXISTING ASSET — PBR maps:** current `Assets/_Game/Content/CapacitorFieldProof/Textures/CF_{Graphite,Shell,Steel,Deck,Ceramic,Warning,Coil,Cold}_{BaseColor,Normal,MetallicSmoothness,Occlusion}.png`. Eight separate EC proof materials retain manufactured grain, normal relief and metallic/smoothness separation. No copied texture library.
- **EXISTING ASSET — donor trim and surface maps:** `Assets/_Game/Content/BlueprintWorldR1/Materials/R1_Trim1.mat`, `R1_Trim2.mat`, and `Assets/_Game/Content/VisualReplacementV3/Materials/VR3_WornIndustrialAtlas.mat` (proof-local machinery clone with reduced emission).
- **DONOR-DERIVED — inspection backdrop only:** existing `VR3_platformmetal.prefab` / `VR3_platformmetal.fbx`. 49 small panels give reflections and a scale grid; they belong to the review scene, not the installation prefab or its geometry statistics. Original CURRENT entry `Modular SciFi MegaKit[Standard]/FBX/Platforms/Platform_Metal.fbx`.

Both source archives explicitly contain CC0 licenses. Their exact license bytes and a scoped `ThirdPartyNotices.md` are under `Assets/_Game/ArtProof/EnergyChamber/Licenses/`. The G-0's previously recorded Catfish CC BY internal-mechanism attribution remains in the existing repository notice. Archives are preserved and raw inspection extraction remains ignored. No vendor scripts, paid package or unrelated pack import.

## Visual findings

**Multilevel readability and structural thickness:** the lower octagonal mass, supported upper service deck and elevated pressure flange remain distinct in the portrait view. The under-deck gap, radial ribs and exposed edge bands keep the base from reading as a single thin circular floor. The central vessel begins above the pressure seat; its frame continues to the crown.

**Central dominance:** approximately 6.095 m installation height and the bright structural flange/armor hierarchy distinguish the core from the roughly 1.4 m generator bodies. The contained blue apertures are subordinate to opaque structural forms. The full footprint is about 11.48 × 11.48 m.

**Node/system integration:** four rotated copies of one master assembly share the same footing, generator/centrifuge pairing and material vocabulary. Continuous twin pipes and the carrier/cable routes visibly lead into the common chamber seat and node machinery. This reads as one distribution installation rather than scattered standalone props.

**Gameplay camera:** [03_gameplay_camera.png](screenshots/03_gameplay_camera.png) and [04_gameplay_G0.png](screenshots/04_gameplay_G0.png) use the current `S01_CameraFollowSettings.asset` without modifying it: perspective FOV 48°, offset (3.4, 19.8, -13.6), look-at height 3.2, portrait 9:16. The inspection anchor is (0, 0, -1.6). [04b_player_follow_anchor.png](screenshots/04b_player_follow_anchor.png) also uses the exact position/rotation formula from `PortraitFollowCamera.SnapToTarget`, anchored at G-0's actual position (2.7, 0, -3.65). The central height hierarchy survives both views. The portrait lens does not show all four nodes equally at once: the rear node is occluded by the chamber, and the lateral node approaches/crosses the frame edge at the player anchor. The high view establishes the complete system; production composition must preserve room to move around the installation.

**G-0 scale:** unit scale throughout. Captured renderer bounds are about 1.09 × 1.77 × 0.93 m; this includes accepted attachment/presentation bounds. It stands near the machine as a useful scale reference. The installation feels substantial without requiring an oversized city-scale monument. No character changes.

**Emission-disabled diagnostic:** [05_emission_disabled.png](screenshots/05_emission_disabled.png) clones every scene material temporarily, disables the emission keyword and sets emission color to black, then restores the originals. Support thickness, service deck, shell segmentation and pipe routing remain visible. The normal view uses confined blue energy slots and bloom intensity 0.075; neither broad glow nor VFX hides the structure.

**Material/lighting response:** shared industrial surface vocabulary unifies custom structural geometry with donor machinery. Bevels, seams, exposed metal bands and native trim maps are visible in [06_normal_material_lighting.png](screenshots/06_normal_material_lighting.png). The darker generator bodies stay secondary. This proof uses one shadowed directional key, a small unshadowed directional fill and the existing R1 reflection cubemap; no realtime point-light or transparency pileup.

## What failed or remains weak

- Existing round/curved platforms cannot supply the hero base silhouette: measured `Platform_Round1` is only 0.170 m thick and `Platform_Metal_Curve` is effectively a plane. They were rejected for the main mass. Cryo Tube ON/OFF and Generator Pile are sufficient small machinery donors, but are not sufficiently distinctive to replace the primary vessel/frame; they were not imported.
- The first assembly exposed FBX unit/axis transforms being overwritten by placement rotations, which collapsed raw service models and rotated conduit height into depth. Neutral assembly wrappers now preserve source transforms. A dedicated regression test checks stair rise, railing height and elevated route bounds.
- The original HDR capture target was read without the expected display conversion and produced misleadingly dark evidence. The final captures use an sRGB ARGB32 target. No image brightness edits were used to disguise geometry or materials.
- Donor generator silhouettes remain recognizable and mechanically repetitive. For production, retain their internals/fins but refine casing panels, dedicated UV packing, small scale wear and a coherent node/control housing. Four unique machines are unnecessary.
- Rails are feasibility placements: source section radii and reduced uniform-scale height need a production fit, and proper access breaks/end caps around stairs and pipe crossings remain to be authored. The current rail line is not a traversal/safety implementation.
- Core armor currently repeats a small number of panels; a production mesh needs purpose-specific removable plates, stronger inspection seams, cleaner curved shading and authored per-part AO rather than relying entirely on existing tiling surfaces.
- Rear-node occlusion and side-frame cropping are real camera findings. A full Chapter integration needs approach/circulation composition and player visibility checks; this proof deliberately does not edit Chapter runtime, map topology, collision or gameplay camera settings.
- No Android device performance profile or owner visual gate has been run. Triangle/render counts are measurements, not a claim of 60 FPS. No percentage-based acceptance target is used.
- The first full-project Android attempt packaged the APK but reported `Failed`: seven global production post-build checks expected unrelated Chapter scenes/actors/resources. Its receipt is retained as `verification/android-main-project-attempt.json`. The isolated alternative mirrors the exact proof scene and native visual dependency closure into ignored `Builds/EnergyChamberStage/`, with zero project C# or DLL dependencies. Its own builder validates presentation authority, missing scripts/materials, the reviewed lens and G-0 scale. Production callbacks remain unchanged; the standalone result is recorded in `verification/android-result.json`.

## Production follow-up

Keep the three-level massing, central pressure-seat/vessel/frame hierarchy, common four-node master design and visible paired distribution routing. Existing donor machinery and trim/detail assets are sufficient for secondary mechanical detail. Primary foundation/pressure seat, vessel shell/frame/crown and tailored structural connectors require project-owned production geometry; generic stairs, railings, support mechanisms and generator internals can remain donor-based.

Proceed to a separately authorized **Energy Chamber production geometry/material refinement** pass, followed by owner device visual review. That is a proposed follow-up; no new numbered project spec has been assigned. The proof alone does not authorize gameplay integration or merge.

## Evidence and verification

All six mandatory categories are present, plus the true player-position anchor:

1. [Low technical view](screenshots/01_low_technical.png).
2. [High three-quarter view](screenshots/02_high_three_quarter.png).
3. [Actual gameplay camera](screenshots/03_gameplay_camera.png).
4. [Gameplay camera with G-0](screenshots/04_gameplay_G0.png), [player-follow anchor](screenshots/04b_player_follow_anchor.png).
5. [Emission disabled](screenshots/05_emission_disabled.png).
6. [Normal materials/lighting](screenshots/06_normal_material_lighting.png).

Machine-readable camera/scale and component bounds: [camera-and-scale.json](camera-and-scale.json), [component-bounds.json](component-bounds.json). Installation metrics: [verification/assembly.json](verification/assembly.json), 28 renderers / 119,120 triangles, four nodes, four routes, zero gameplay MonoBehaviours and colliders in the installation prefab. G-0 and the inspection backdrop are excluded from those metrics.

Test/build results are recorded in [delivery.json](delivery.json); the complete exact changed-file manifest is [FILES_CHANGED.txt](FILES_CHANGED.txt). Scope is additive under the dedicated proof, source, tools and evidence paths; shared gameplay/presentation assets are dependencies only. Unity's automatic importer/configuration/validation reserialization outside those paths is restored before delivery and excluded from the PR.

Assumption: no separate Energy Chamber-specific concept bitmap was supplied in this request or located in current authoritative paths. The explicit structural brief and current industrial visual target define this feasibility proof. Pixel-level reproduction of a missing chamber concept is not claimed.
