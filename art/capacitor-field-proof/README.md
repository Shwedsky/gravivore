# Editable capacitor installation

`CapacitorField.blend` contains separate editable mechanical parts before runtime consolidation. Units are metres. Blender x/y/z maps to Unity x/z/y. The export remains at unit scale; there is no anisotropic squeeze to fit the camera.

Authored by `Tools/capacitor-field-proof/author.py`, surface preparation by `prepare.py`. Runtime output is `Assets/_Game/Content/CapacitorFieldProof/Models/CapacitorField.fbx`, wrapped by `CapacitorFieldProofBuilder` in its presentation-only prefab. Collision and minimap authority reside in the existing `Chapter01WorldLayout.asset`.

Pressure vessels, interlocked saddle, transformer enclosures/fins/bushings, bus carrier, conductor routes, support joints, platforms, grated service pits and PBR repeating surfaces are original project work. Embedded service donor geometry is CC0 from canonical Quaternius/Molten packages; exact sources/licenses are recorded in the pass provenance and ThirdPartyNotices. Donor native UVs survive export. Actor V2 and bipedal G-0 are external to this source and remain the existing live gameplay presentations.
