# Editable hostile actor family

Seven project-owned Blender 5.2.2 LTS sources, meters / Z up / forward -Y.
Each source contains hidden, separately editable manufacturing components,
one Generic mechanical armature, rigidly weighted consolidated LOD0/1/2,
named bone-parented sockets, authored 30 FPS actions, packed PBR textures and
diagnostic cameras/lights. No donor object, topology, rig, weight or animation
data was copied into these files.

The `*_EDITABLE_AUTHORED_COMPONENTS` collection stores construction parts
with `binding` and `trim_region` properties. Their evaluated copies are
consolidated in `*_EXPORT_SKINS`; edit the construction through the authoring
tool and regenerate the skins rather than expecting hidden construction
parts to animate. LOD1/2 are hidden in source renders, not missing.

`Tools/actor-production-v2/author_actors.py` reproduces construction, trim
projection, skin consolidation, clips, LODs, source renders and FBXs. Export
uses FBX unit scaling, Generic bones, no leaf bones, in-place ROOT and no
camera/light objects. Unity recalculates Mikk tangents. Textures are packed
and also point relatively to the shared import maps for portability.

All actors share trim manufacturing finishes but have role-specific armor
coverage, mechanisms, proportions and energy localization. Arc Drone is
blue by the approved PNG; number-badge colors are not actor paint.

Final assets and review: `Assets/_Game/ArtReview/ActorProductionV2/`.
Evidence/report: `docs/history/implementation-passes/chapter01-actor-production-v2/`.
The evidence directory records this implementation; it is not new authority.
The unchanged owner PNG remains the visual authority.

These are isolated integration candidates. Device visual acceptance,
performance and production encounter clearance are still required. No
new actor is bound into the production Chapter01 scene in this branch.
