# AI 3D Tool Evaluation — GRAVIVORE First Slice

Research date: 2026-10-06. Goal: evaluate AI as a base-mesh accelerator, not as an excuse to skip production cleanup.

## Meshy
- Current plans surfaced: Free, Pro $20/mo, Premium $40/mo, Ultra $100/mo; paid tiers include commercial rights and unlimited model downloads. Free outputs are CC BY 4.0.
- Inputs/outputs: text-to-3D, image-to-3D; FBX/OBJ/USDZ/GLB/STL/BLEND exports documented.
- Strengths: fast blockout generation, accessible Blender/Unity workflow, smart-topology/remesh options.
- Weaknesses: articulation boundaries, exact mechanical symmetry, blade thickness, nested armor and hinge logic can still require rebuilding.
- G-0: **B for silhouette/base exploration, C for final mesh**.
- Scout: **B+ base candidate** because compact radial forms tolerate more AI cleanup.
- Cutter: **B** if generated from clean front/side concept references; custom blades/joints still required.
- Magnetar: **B-** because layered elite armor and rig-safe multi-leg mechanics amplify cleanup burden.
- Commercial: use paid plan for private commercial ownership; only use source images the project owns or is licensed to transform.

## Tripo AI
- Current pricing surfaced: Pro $20/mo, Max $90/mo, Team pricing also available; private models/commercial use surfaced on paid tiers.
- Features surfaced: image-to-3D, text-to-3D, multiview-to-3D, smart mesh/topology options.
- Strengths: multiview route is useful when the concept can be expanded into consistent orthographic-ish references.
- Weaknesses: hard-surface topology and moving-joint segmentation still need artist validation.
- G-0: **B** as multiview base; not final.
- Scout: **B+**.
- Cutter: **B**.
- Magnetar: **B-**.

## Hyper3D Rodin
- Current Creator plan surfaced at $30/mo; Business $120/mo. Creator includes multi-image to 3D, Smart Low-Poly, HD/custom texture, baked normals and unlimited export/any use; Business adds high-poly quads/API and more production features.
- Strengths: best surfaced production-oriented control set of the three, especially multi-image, low-poly and baked-normal workflows.
- Weaknesses: “nearly production-ready” still does not guarantee hinge topology, correct shell intersections or animation-safe segmentation.
- G-0: **B+ base**, especially for a custom concept-derived shell/blockout.
- Scout: **A- base candidate**.
- Cutter: **B+ base**.
- Magnetar: **B**.

## Recommended AI use

1. Generate 4–8 silhouette/base variations from approved project-owned concept imagery.
2. Select only a macro-form that helps, not the most detailed texture result.
3. Bring into Blender and rebuild articulation-critical geometry manually.
4. Retopo/decimate deliberately to project triangle targets.
5. Replace or rebake textures into GRAVIVORE material families.
6. Never feed NoAI-tagged or unlicensed third-party donor imagery/models into generative tools.

## Where AI is useful
- rapid proportion exploration;
- rough shell volume generation;
- secondary prop ideation;
- Scout/Cutter concept-to-base acceleration;
- non-hero internal mechanical filler that will be heavily edited.

## Where AI is a bad idea
- final G-0 identity mesh without manual rebuild;
- Magnetar final articulation/armor topology;
- exact hero reactor engineering;
- animation-ready hinge chains where intersections/clearance matter;
- any workflow using copyrighted third-party donor input without appropriate rights.

## Preferred order

1. **Rodin Creator** for a one-month production experiment if budget allows.
2. **Meshy Pro** as lower-cost alternative with convenient DCC/game workflow.
3. **Tripo Pro** if multiview generation proves materially better on the approved concept set.

Run a single bake-off using the same owned G-0/Scout reference before committing to a subscription beyond one month.
