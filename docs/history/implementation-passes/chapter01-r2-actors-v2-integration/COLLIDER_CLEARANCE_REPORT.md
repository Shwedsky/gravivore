# Collision and animated clearance gate

No gameplay collider, sensor, attack range, spawn position, world blocker or gate dimension is changed.
Seven new production prefabs contain no collider. World/controller authority remains at the original roots.

The source actor envelopes exceed the original collision radii. The integration therefore uses a bounded
cosmetic model displacement away from nearby HardBlocker faces. An allocation-free overlap query and
three separation passes operate only on the presentation child. Existing pull cues are included in the
cosmetic envelope. The correction does not move authority or ground telegraphs.

Configured conservative planar radius / maximum displacement in metres:

- Scout: .76 / .43.
- Cutter: 1.46 / 1.08.
- Warden: 1.17 / .71.
- Arc Drone: .85 / .51.
- Carrier: 1.30 / .80.
- Magnetar: 2.11 / 1.48.
- Custodian: 2.98 / 2.00.

This keeps the source forms/scale and both Warden shields intact. The visual-to-authority offset near a
wall is a remaining feel/readability risk, especially for the two large encounters, and belongs in owner
device review. Conservative radii are presentation settings, not enlarged gameplay collision.
Near machinery, the visible core can be off-centre from the authoritative ground attack footprint;
the telegraph remains accurate for damage. This presentation association needs owner assessment.

The runtime tests load actual Chapter01 with R2 blockers and production bindings. They bake animated
LOD0 vertices in world space, test intersection against actual enabled HardBlocker BoxColliders, and
record minimum floor height. Gates are opened through the existing development encounter prerequisite;
near-face arena fixtures are rejected if the original controller capsule could not legally occupy them.

`verification/animated-clearance.json` covers ordinary spawn/combat contexts, four turn headings,
four times per role clip, actual wall pursuit, and elite/boss centre poses. `route-arena-clearance.json`
covers 24 original route segments for each ordinary family and four nearby arena faces × four headings
× six articulated poses for both Magnetar and Custodian. Traversal calls the real enemy controller;
the fixture explicitly asserts the cosmetic step never alters the authoritative root position.

These are finite Editor samples, not exhaustive collision proofs or human visual acceptance.
Decorative render-mesh intersections are not exhaustively tested by the blocker-volume gate.
Other live enemies are disabled as traversal obstructions in the controlled route fixture; the separate full-scene
combat/traversal soak retains normal population. Full no-UI sector captures and real encounter-event
captures use the portrait production camera. See final machine-readable results and DELIVERY_REPORT.
