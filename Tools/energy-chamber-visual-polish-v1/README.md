# Energy Chamber polish V1

Continue the dedicated proof without changing its accepted primary geometry.
Run `materials.py` with Python/numpy/Pillow, and `author.py` with Blender 5.2 in
background mode. The latter borrows only the original mesh helper definitions;
it never runs the original structure authoring or exports the accepted four FBXs.
The editable overlay is `art/energy-chamber-visual-polish-v1/EnergyChamber_VisualPolishV1.blend`.

Use the existing `Tools/energy-chamber-hybrid-proof-v1/run_unity.ps1` actions
Author, Capture, Compile, Validate, ProjectValidate, EditMode, PlayMode and Android.
Evidence now goes to `docs/history/implementation-passes/energy-chamber-visual-polish-v1`.
The preceding proof's screenshots and receipts remain immutable before evidence.
Android exports the exact reviewed scene to the ignored `.utmp/ec77` visual-only
project; its package remains `com.gravivore.energychamberproof`. It is a static
presentation review, with no Chapter gameplay.

After Unity exits, run the scoped `restore_unity_reserialization.ps1` from the
isolated branch root to discard only unrelated automatic tracked serialization.
Then run `verify.py` to compare the primary FBXs, G-0, lens configuration and
original editable source against the accepted baseline, check scope/captures,
and record available executed test/build receipts. Do not infer device approval.
