# Asset provenance and reuse boundaries

The final seven actor meshes, construction components, UVs, mechanical rigs,
weights, animation keys and trim/PBR maps are newly authored project assets
produced by `Tools/actor-production-v2/author_actors.py`. Actual donor data
reuse is **zero**. The authoring tool does not open any external model.
Source reopening validation records hashes of the delivered Blender files.

Exact donors were separately opened in Blender by `inspect_donors.py` from
the canonical owner-local `ExternalAssetIntake/Current/_actor-audit-v1`
extraction. Hashes, measured meshes, bone hierarchies and actions are in
`DONOR_REINSPECTION.json`. Inspection informed mechanical manufacturing,
not final anatomy; the unchanged approved PNG controls the final role and
silhouette. All raw donors remain ignored/private outside this worktree.

- Scout: Quaternius Animated Mech Pack / George. Examined biped joints,
  leg chains and action structure; no geometry, rig or take copied.
- Cutter: Sci-Fi Essentials / Enemy_QuadShell. Examined support/mount
  mechanics; newly authored low shell, four supports and paired blades.
- Warden: MSGDi Medium Mech Striker. Examined heavy biped chains; authored
  new arm/tool rig, physical grips, separate shields and all clips.
- Arc Drone: Sci-Fi Essentials / Enemy_EyeDrone. Examined aerial pivots;
  authored new containment, outriggers, airborne vanes and flight clips.
- Carrier: UnityFan Vehicle 012, exact member
  `source/sci-fi_vehicle_013_2.blend`. Examined the evaluated enclosed hull;
  authored new hull/nacelles, four-bone rig and all vehicle clips. The donor
  itself has no rig or animation.
- Magnetar: Sci-Fi Essentials / Enemy_Trilobite support/joint reference.
  No complete donor actor is used. Containment, buttresses, load paths,
  support architecture, magnetic terminals and rig are authored.
- Custodian: selected heavy joint/support construction references from
  the same audited inspection; no complete boss donor. Reactor vault,
  chimney, independent articulated mantles, discharge structures and
  phase articulation are authored separately from Magnetar.

PR #72's audit reports conflicting Quaternius license notices and restricted
Unity Asset Store raw/source redistribution for Striker. Those findings are
preserved; this task does not claim that inspection clears redistribution.
UnityFan's audited CC0 finding also did not cause any donor bytes to be
copied. No paid purchase or download was made, and no donor-derived editable
source, external texture or imported animation was promoted publicly.

Donor comparison images reuse the audit's neutral diagnostic renders as
documentary inspection evidence, labeled with exact source names and zero
reuse. They are not promoted gameplay textures. The owner concept PNG is
preserved byte-for-byte; cropped comparison boards are evidence only.

The isolated stage references existing main-branch project floor/gate/reactor
presentation and the unchanged G-0 live prefab. Their original provenance
remains with the project; they are not reauthored or relicensed here.
