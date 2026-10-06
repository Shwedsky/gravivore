# G-0 Tier 0 Production Path

Research/decision date: 2026-10-06.

## Visual contract

G-0 must remain a low, front-biased, four-support techno-organic predator organized around a visible cyan gravity core. It cannot read as a humanoid, tank, exosuit, boxy mech, toy robot, or generic spider drone.

## Path comparison

### 1. Full donor model
- Fidelity potential: low-medium.
- Cost: $0–$60 typical marketplace spend.
- Time: 0.5–2 days if an unusually close donor exists; otherwise false economy.
- Cleanup: medium-high because all currently found donors diverge from the G-0 core/armor anatomy.
- Rigging: low-medium if source rig exists.
- Mobile optimization: medium.
- Licensing risk: medium; must verify each exact source.
- Uniqueness: low.
- Verdict: **not preferred**.

### 2. Donor parts / kitbash
- Fidelity potential: high if the torso/core and silhouette are custom.
- Cost: $0–$100.
- Time: ~3–6 focused Blender days for Tier 0 including cleanup/rig.
- Cleanup: medium.
- Rigging: medium.
- Mobile optimization: controllable.
- Licensing risk: manageable with CC BY / paid royalty-free donors and attribution register.
- Uniqueness: high after substantial reconstruction.
- Verdict: **strong component of preferred path**.

### 3. AI-generated base mesh
- Fidelity potential: medium for ideation, inconsistent for exact articulated hard-surface anatomy.
- Cost: ~$20–$30 for one month of a suitable paid tool, plus artist time.
- Time: fast to generate, slower to make animation-safe.
- Cleanup: high. Expect topology replacement around joints, thin blades, shell overlaps and core cavity.
- Rigging: high burden unless retopology is intentionally built for hinges.
- Mobile optimization: high burden unless low-poly mode is usable.
- Licensing risk: low-medium depending on plan and source images; use only owned/internal concept inputs.
- Uniqueness: medium-high.
- Verdict: **use for volume/silhouette exploration, not final production mesh**.

### 4. Custom Blender model
- Fidelity potential: highest.
- Cost: no direct asset spend; highest manual labor.
- Time: ~5–10 focused production days for polished Tier 0 depending on artist skill/revision cycles.
- Cleanup: inherent/controlled.
- Rigging: medium but predictable.
- Mobile optimization: best controlled.
- Licensing risk: lowest.
- Uniqueness: highest.
- Verdict: **best quality route**.

### 5. Hybrid — custom torso/core + donor mechanical joints/limb fragments + selective AI blockout
- Fidelity potential: **very high**.
- Cost: $0–$100 + optional one-month AI subscription.
- Time: ~4–7 focused Blender days for first production-ready Tier 0.
- Cleanup: medium; limited to sourced components rather than whole-body salvage.
- Rigging: medium.
- Mobile optimization: controllable from the start.
- Licensing risk: low-medium with explicit provenance tracking.
- Uniqueness: very high.
- Verdict: **PREFERRED**.

## Preferred path

**Hybrid custom-first production.**

Author the central torso, cyan gravity-core cavity, upper armor layering, rear mechanical cluster and front weapon/mandible forms specifically from the prototype. Use donor geometry only for low-identity mechanics such as hinge housings, pistons, bearings, foot/claw fragments and internal greeble clusters. AI may be used to generate alternative blockout massing from the internally owned concept, but should not define final joint topology.

## Candidate donor pool

Potential source-inspected web candidates for mechanical parts only:

- PAndras — `Robot spider` — Sketchfab — CC BY — 18.8k tris.
  https://sketchfab.com/3d-models/robot-spider-c9c7188c7f9e4504b8499f1131693b72
- Muhammad Hasan Alasady — `Spider Robot` — Sketchfab — CC BY — 7.8k tris, animation set.
  https://sketchfab.com/3d-models/spider-robot-7ea58c2e0e7e48f281d7a840f02aa7b1
- Keralt — `Spider Robot (Rigged)` — Sketchfab — CC BY — 4.6k tris.
  https://sketchfab.com/3d-models/spider-robot-rigged-419632470d3b4328b8d8ea7ae0462ce7

None is approved as the G-0 body. They are candidate mechanical donors only.

## Blender production sequence

1. Set gameplay-camera reference and silhouette planes from approved board.
2. Block custom torso around core diameter ~22–30% of total width.
3. Establish four support root positions and negative spaces before detail.
4. Build core cavity as real depth: glass/core surface + dark housing + pale armor rim.
5. Build custom front weapon/mandible pair; preserve obvious forward axis.
6. Evaluate donor joint/foot fragments only after body proportions are locked.
7. Rebuild all moving intersections into explicit pivots/bearings; no intersecting-mesh fake joints.
8. Retopo/decimate toward ~22–35k LOD0 tris for Tier 0, prioritizing upper/core/front silhouette.
9. UV to 1–2 primary sets; bake donor/high-poly details into unified texture family.
10. Consolidate to target 3–4 material slots maximum: pale armor, dark mechanism/metal, energy glass/emissive, optional shared service decal.
11. Rig four supports with deterministic hinge axes; separate core containment and weapon sockets.
12. Create sockets/empties for core VFX, left/right attack origin, hit/impact anchors as required by runtime contract.
13. Create LOD1 ~50–60% and LOD2 ~20–30% of LOD0 where silhouette survives.
14. Create simple collider proxies separately from render mesh.
15. Validate FBX scale, transforms, pivots, normals/tangents and URP material conversion.

## Acceptance gate before animation polish

Reject the mesh if any of these fail from the real portrait gameplay camera with emission reduced:
- cyan core cavity is still visually legible;
- four-support stance reads in black silhouette;
- attack/front direction is obvious;
- pale shell and dark mechanics form layered macro-masses rather than noise;
- donor origin is not obvious;
- no human torso/head/pelvis anatomy appears;
- joints have believable clearance and pivot logic.

## Production estimate

Assumption: one competent Blender hard-surface generalist with the concept board already approved.
- blockout + silhouette approval: 0.5–1.5 days;
- production modeling/kitbash: 1.5–3 days;
- retopo/UV/bake/material consolidation: 1–2 days;
- rig/LOD/export validation: 1–1.5 days;
- revision reserve: 1 day.

Expected total: **4–7 focused days** for a strong Tier 0 first-slice model; allow **6–10** if multiple visual revisions are needed.
