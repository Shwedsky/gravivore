# Chapter 01 architecture language V1

Status: current production language for architecture. The accepted Energy Chamber at `7d0e375f7b0a12a9d46982af685cabe3ce57d731` is the quality benchmark. Its silhouette, proportions, stepped foundation, four cyan vertical nodes, orange center and radial support machinery remain frozen.

## Function governs form

Each facility must explain a different job: repair, deployment, distribution, fabrication, armor processing, logistics, induction or containment. Start with one recognizable primary silhouette. Use layered levels, recessed working spaces and substantial structural thickness to make that silhouette credible at the 48-degree portrait gameplay lens. A maintenance bay may be open and asymmetrical; a relay may have a deployment mouth and antenna/service spine; a transfer gate may span a lane. Circular shapes, four nodes and an orange center are specific Energy Chamber choices, not a Chapter recipe.

## Connected machinery

Secondary machines must support the main function and remain visually subordinate. Show how a generator supplies the installation, how a pipe reaches a manifold and how a cable enters a cabinet. Foundations carry structural loads; columns meet beams; service platforms meet access stairs. Avoid disconnected props and decorative pipes with no source or destination. Detail should communicate fastening, heat exchange, flow, maintenance, protection or wear. More greebles do not compensate for a weak primary form.

## Surface and energy hierarchy

Separate heavy dark load-bearing structure, worn shell plates, exposed steel, copper/contact elements and recessed technical systems. Retain authored base color, normal relief, metal/smoothness variation and occlusion. Use wear selectively on working edges and access surfaces. Keep luminous areas contained and readable against these materials. Cyan denotes service/distribution systems; orange denotes hazardous energy or active heavy industry. Every landmark does not need both colors, and emission must not erase geometry, materials, enemy silhouettes or telegraphs.

## Scale and service access

Read scale against the unchanged unit-scale bipedal G-0, approximately 1.771 m tall in its Tier 0 presentation. Stairs, consoles, railings, platforms and maintenance clearances should agree with that scale. Keep player routes and combat centers open. Place meaningful industrial mass at edges and approaches where it frames movement. Sparse test-floor composition, toy cubes, flat rectangular placeholder buildings and random asset-pack scattering fail the production bar.

## Physical presence

Presentation remains independent of encounter authority. A static landmark needs no new gameplay mechanic. Its visually solid core, plinth, major supports and side machinery require simple compound collision. Fine bolts and pipes do not each require a collider. Service gaps are walkable only where the visible structure supports that interpretation. Validate collision with the actual G-0 CharacterController; collision counts or wireframe screenshots alone cannot prove traversal.

## Camera and distance

Judge the structure from actual player approach, beside G-0 and within surrounding Chapter architecture. Preserve the production camera offset `(3.4,19.8,-13.6)`, look-at height `3.2` and perspective field of view `48`. A distant silhouette should remain identifiable after secondary detail disappears. LOD0 preserves accepted nearby quality; lower LODs remove subordinate geometry while preserving the main masses, physical connections and energy hierarchy. Rendering budgets require measured device evidence; a low triangle count is not an art style.

## Review boundary

KEEP means current presentation already supports this language. REFINE means the main silhouette works but its connections, secondary machinery, surfaces or collision need work. REBUILD means the primary form itself remains primitive or lacks functional identity. The audit ranks replacements before implementation. Actor-owned Magnetar/Custodian machinery requires coordination; enemy behavior and combat are outside architecture ownership. Device visual/performance acceptance remains a separate final human gate.
