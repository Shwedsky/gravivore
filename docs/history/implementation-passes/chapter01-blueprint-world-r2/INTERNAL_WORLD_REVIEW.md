# Chapter 01 World R2 — internal portrait review

R1 failed the owner's device visual acceptance. Its topology/traversal remain the accepted technical baseline; the R1 delivery report is build evidence, not visual approval. The owner's review is retained verbatim in `OWNER_DEVICE_REVIEW_R1.md`.

The R2 review inspected all eight final actual-gameplay-camera views with and without UI, the two facility-walk views and the remapped map before the Android build. Portrait capture is 540 × 960. The production follow camera, FOV, framing parameters, actor meshes/animation, weapon and UI skin are unchanged. The no-UI copy temporarily disables Canvas visibility and restores it; it is not a separate render camera.

The first R2 candidate was rejected internally because giant clean panel fields and weak light contribution still carried the image. The second iteration exposed pools of light and subdivided the floor, but overly chamfered repeated panels and closed cargo shells remained too clean. The final iteration uses smaller corner cuts, flush channels/grates, connected overhead service shoulders, a stepped press crown and open damaged freight shells. Reactor presentation was compressed/adjusted within the existing footprints to show mechanical function at the fixed camera; collision footprints and progression coordinates were not moved.

- Repair Hub: the round maintenance pocket and accepted repair manipulators carry the image; cyan diagnostic lighting, clean service deck and overhead service machinery distinguish it from hostile work cells.
- Relay Yard: an open deployment mouth, antenna/comms roof hardware, carrier rails and marked assembly envelope communicate lighter manufacturing. The former full-width flat roof was opened.
- Capacitor Field: tall contained cyan energy vessels, opaque containment/grounding hardware, distribution generator and electrical service connections provide the strongest cool energy identity.
- Cutting Floor: stepped heavy press framing, hydraulic hardware, hot jaw/work lighting, production rails and darker heat-stained floor distinguish it from Relay's open light deployment bay.
- Shield Dump: broad layered armor storage/processing cells, clamped shield cassettes and cooler armored deck pockets communicate weight and storage. Warm local inspection light and cool service rim do not replace the heavy silhouette.
- Hauler Graveyard: open cargo shells, displaced lids, exposed frames, severed hydraulics, asymmetric patches and the loading structure communicate damaged transport. Warm inspection lighting is localized to the freight edge.
- Magnetar Complex: the amber curved-shell induction chamber, pressure heads, magnetic yokes and marked inspection perimeter remain distinct from ordinary energy vessels. The main fight pocket remains open; a facility-walk view checks the shunt/header side.
- Custodian Core: the red containment chamber, interlocked stepped cheeks, pressure vessels, hot/cold service routes and marked inspection perimeter establish boss machinery behind the locked Custodian occupant. A facility-walk view checks the containment flank.

The changed hierarchy uses dark graphite structure, medium exposed steel, deeper service recesses, worn/painted plate fields, hazard/work envelopes and restrained cyan/amber/red energy. Large deck assemblies now consist of replaceable inspection panels and flush grates rather than a single stretched hatch. No random decal scattering is used for sector identity. The continuous R1 top strip that hid sector floors was removed from presentation; the underlying movement-surface union remains exactly R1.

Sixteen bounded unshadowed local light pools supplement the directional key. Darker ambient and reflection levels retain peripheral depth; warm/cool light pools reveal machinery and central occupants. URP still limits additional lights per object to four. This adds real per-pixel lighting cost and requires device profiling; counts are not a 60 FPS guarantee.

The overhead edge equipment is supported from existing peripheral structures; its lowest components are above the 1.4 m player controller. Presentation meshes still have no collision or gameplay scripts. Traversal authority, minimap projection/skin, sector centres, encounters and progression remain frozen to R1 and are checked separately.

Large installations can still leave the frame when the player approaches closely; the review checks exposed function/energy/mechanical surfaces rather than requiring the entire installation in every follow-camera position. Device exposure, touch traversal, thermal performance and the owner's final concept-fidelity judgement remain pending. This internal review selects an R2 APK candidate; it does not claim owner approval or A-level visual acceptance.
