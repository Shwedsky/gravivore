# Seven-actor isolated review

Open `ActorProductionV2_Review.unity` and enter Play, or use the separate
Android review APK. The first view uses the unchanged production camera
settings and unchanged live G-0 prefab. Controls cycle all seven actors,
in-place poses, LOD0/1/2/Auto and the complete family. The family overview
uses a wider diagnostic camera; it is not gameplay-camera evidence.

No gameplay domain, collider, damage, spawn, balance, UI or progression
authority belongs to this review. Floor/reactor/gate presentation is cloned
from current main; the review builder reads Chapter01 lighting and camera
without saving that scene. It checks canonical scene bytes and dependency
isolation. PR #71's world is not consulted or modified.

Each prefab has an in-place Generic Animator, one consolidated skin/material
per selected LOD, three LODs and named articulation/VFX/attack sockets. Run
is the import alias for the source Move action. Windup/Release/Special aliases
are review compatibility mappings, not changes to gameplay semantics.

`Gravivore/Art Review/Actor Production V2/Build and Capture` regenerates
prefabs/controllers/review and measured camera evidence. `ValidateProject`
executes the existing validator as well as the isolated asset contract.
`BuildReviewAndroid` builds ARM64 IL2CPP development output with separate
package `com.gravivore.actorreview` and review scene first. Existing global
build audits also require unchanged Bootstrap/Chapter01 scenes to be packed;
they are included after the review entry point, with existing actors intact.

Editor/play tests verify decreasing LOD geometry, preserved skin bindings,
separate Warden shield chains, Scout blade sockets, drone flight clearance,
vehicle anatomy, actual attack articulation and stationary roots across all
seven actors. These checks do not certify visual quality or device FPS.

Review models remain readable for inspection/testing. Later authorized
integration should profile GPU skinning, CPU mesh retention, draw calls,
LOD thresholds and complete encounter/wall/turn clearance on Android.
