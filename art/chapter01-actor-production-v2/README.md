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

Original source: Actor Production V2 commit `a3b5c279f48a932127a1d3cebe63069bedc7f6f5`
in PR #74. Its reproduction script and original review evidence remain at that pinned commit.
This integration branch selectively promotes the seven FBX models, original controllers/PBR maps
and these editable sources; it does not import the review scene or playback scaffold.

Current runtime: `Assets/_Game/Content/ActorProductionV2/`.
Clean production wrappers and binding configuration are authored by
`Assets/_Game/Editor/VisualIntegration/Chapter01ActorIntegrationBuilder.cs`.
Integration evidence: `docs/history/implementation-passes/chapter01-r2-actors-v2-integration/`.
The unchanged owner PNG remains visual authority. All seven hostiles are now bound into actual
playable Chapter01. This is a full chapter convergence candidate; owner device visual acceptance
and Android performance verification remain required.
