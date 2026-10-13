# Focused vertical-node correction

Run `materials.py` with Python/numpy/Pillow, then `author.py` with Blender 5.2.
The source creates only two new custom meshes: the reusable vertical stabilizer
and the central contained-energy insert. Its helper import stops before the
accepted base/chamber/node authoring code; it never exports those original FBXs.
Editable source: `art/energy-chamber-vertical-nodes-v1/EnergyChamber_VerticalNodesV1.blend`.

The existing isolated builder adds one stabilizer to the master node, used four
times without moving the reservoir, centrifuge or node roots. Orange replaces
only the central energy slots; the new node cores remain cyan. This is the
owner's explicit installation-specific hierarchy, not a global art-direction change.
All energy surfaces are opaque URP Lit materials; no local point lights, runtime
animation, new package, gameplay component or collision is introduced.

Use the existing `run_unity.ps1` Author/Capture/Compile/ProjectValidate/EditMode/
PlayMode/Android actions. Evidence now goes to the vertical-node correction pass;
the preceding screenshots/receipts remain untouched. The Android package is an
isolated static art review, not the production Chapter game.

After Unity exits, run the scoped serialization restore tool, then `verify.py`
to check the preserved reference assets, footprint, evidence, executed tests and
APK receipt. Keep local diagnostic logs out of Git; commit JSON/XML receipts.
