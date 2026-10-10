# Chapter 01 actor visual authority V2 delivery

Status: **DELIVERY EVIDENCE / NOT AN ADDITIONAL DESIGN AUTHORITY**

Delivered 2026-10-10. The current requirements live in the actor V2 package and current-authority entry points; this report records the work and its limits.

## Branch, PR and checkpoints

- Repository: `Shwedsky/gravivore`.
- Baseline: fetched `origin/main`, `38e0ed8fe5c95ff41ab5dbb246f45ed26bdcf9cc` (Chapter 01 Visual Blueprint V1 merge).
- Branch: `design/chapter01-actor-visual-targets-v2`.
- Isolated checkout: owner repository `.codex-worktrees/actor-visual-targets-v2/`.
- [Draft PR #73](https://github.com/Shwedsky/gravivore/pull/73), base `main`; no merge.
- `83ecadeba0784be28ca5be70ca6158482ef126cc`: preserve the exact PNG and locked anatomy/precedence; pushed before creating the draft.
- `4d4606bcf92aeeff9121fc78fd2232cd9c504929`: complete normalization, proof brief and current-authority conflict corrections; pushed.
- Final verification checkpoint: the commit containing this report; see the branch log for its hash.

The initial PNG checkpoint was persisted before the documentation continuation. The primary checkout's pre-existing Unity settings/assets were not staged or changed by this task. Neither `art/chapter01-blueprint-rebuild-v1` / PR #71 nor `chore/actor-asset-intake-v1` / PR #72 was checked out, committed to or merged here.

## Exact visual authority and custody

The original existed at owner-local `docs/visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png`. It was visually inspected and copied directly into this branch at the same repository-relative path. It was not regenerated, recreated, cropped, retouched, recolored, recompressed or replaced.

- SHA-256: `67960c1254133c48b1de97d2dd635a5969cea49b949a7e0e8981afd34738099f`.
- File size: 2,637,330 bytes.
- Dimensions: 1672 x 941 pixels.
- Original vs branch copy: exact byte equality.

The package records newest owner correction > PNG > V2 text > CURRENT visual target > ART_DIRECTION > donor evidence > historical comparison. Missing exact PNG stops actor production; an unexpected hash change requires establishing an owner-authorized image revision. World topology remains governed by Visual Blueprint V1.

## Changed files

New authority package:

- [CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png](../../../visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.png).
- [CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md](../../../visual-blueprints/chapter01-actors/CHAPTER01_ACTOR_VISUAL_TARGETS_V2.md).
- [chapter01-actors/README.md](../../../visual-blueprints/chapter01-actors/README.md).

New production task contract:

- [SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF_BRIEF.md](../../../actor-production-v1/SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF_BRIEF.md).

Current entry points and supersession markers:

- `docs/CURRENT_AUTHORITIES.md`.
- `docs/CURRENT_VISUAL_TARGET.md`.
- `docs/ART_DIRECTION.md`.
- `docs/ASSET_SOURCE_OF_TRUTH.md`.
- `docs/CURRENT_DOCUMENTATION_AUDIT.md`.
- `docs/current-visual/reference/README.md`.
- `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md`.

Delivery evidence:

- This `docs/history/implementation-passes/chapter01-actor-visual-targets-v2/DELIVERY_REPORT.md`.

No `Assets/`, `ProjectSettings/`, `Packages/`, gameplay, scene, progression, map/minimap, collision, controller, existing actor prefab, FBX or production material is changed by the branch. All world-blueprint files match baseline blobs exactly. No donor archives or editable third-party source bytes are committed.

## Conflicts found and resolution

The V1 enemy target called itself CURRENT and prescribed Scout as a low radial/spider body. CURRENT visual target routed actor work to it and described hostile radial/spider language; ART_DIRECTION did the same. All now route to V2's light two-legged blade-arm Scout. The V1 file is marked SUPERSEDED, and its original design sections are preserved unchanged for comparison.

Older current visual/asset text and `docs/current-visual/reference/README.md` allowed a text-only fallback for missing concept images. The actor-specific gate now explicitly requires the exact V2 PNG and stops without it. No older board or generated substitute is permitted.

The generic hostile red/orange color rule conflicted with the visible blue Arc Drone. The package and current entry points preserve this image-specific exception. Explicit owner anatomy wins over ambiguous drawn appendages: Arc Drone remains airborne, Warden carries separate shields through arms/hands, and Carrier remains a low vehicle.

The vetted `docs/free-asset-intake-v2/` evidence library still contains old radial Scout selection notes and V1 links. Its README already labels it reference/evidence rather than current instructions. Those historical measurements/recommendations were not rewritten; the current audit records the conflict and V2 supersession. No pre-existing `docs/history/` file was edited.

Current product/spec text uses legacy role/gameplay labels such as Scout Drone and Carrier's slow heavy swing. They do not define visual anatomy, and this documentation task does not retune movement, attacks, reinforcements or boss phases. No current rule explicitly making Warden shields into pauldrons or Carrier into a legged mech was found; the owner's rejection rules are now explicit in V2 and the proof brief.

## Donor findings incorporated

Read-only manufacturing evidence comes from PR #72 at `628f51d220a379fdaa14a60536b68275924976a9`, specifically its audit, donor matrix and model inventory. The V2 contract links the pinned files so this branch does not depend on merging the intake branch.

- G-0: retain current project-owned biped; no replacement modeling scope.
- Scout: George rig/leg/arm mechanics; new long blade terminations, forward shell, containment and adapted motion. Original measured Blender member/hash is specified.
- Cutter: QuadShell support and mount mechanics; replace guns with board-matched paired cutters and author low predator shell/motion.
- Warden: Striker biped mechanics; custom separate shields/grips and arm control. No donor wrist/finger rig or shields; ASCII hand conversion remains a production prerequisite.
- Arc Drone: EyeDrone aerial articulation; Seed Keydrone/Voodoo shells are secondary static donors requiring custom rig/material work.
- Carrier: UnityFan 012 editable shell, with 9,248 evaluated triangles rather than its 485 control-cage triangles; zero ready rigs/clips. Its actual Blender member is named `source/sci-fi_vehicle_013_2.blend`; that naming distinction and exact hash are preserved.
- Magnetar/Custodian: substantial custom heavy architecture; donor legs/joints/pivots only, never complete actors or enlarged ordinary units.

Source redistribution caveats are attributed to the audit, not newly adjudicated here. Raw assets remain owner-local and source promotion requires exact provenance/license evidence.

## Production-proof trio status

**Contract complete; models not produced.** The brief covers silhouette, approximate comparison with G-0, locomotion, major masses, attack source, moving parts, donor boundaries, custom geometry/rig work, materials/emission, gameplay-camera readability, collision-envelope compatibility, sockets, LODs and mobile concerns separately for Scout, Warden and Carrier. It defines evidence, rejection rules and later compile/test/validation/build/device gates.

Next task: **SCOUT_WARDEN_CARRIER_PRODUCTION_PROOF** — Scout, then Warden, then Carrier. The brief does not authorize this authority task to start modeling or alter existing production integration.

## Verification executed

Read-only documentation QA ran in the isolated checkout using bundled Python; its scratch script/output live in ignored `Builds/ActorAuthorityV2/`. Checks passed:

- Exact expected SHA-256 and byte equality against the owner-local original.
- PNG signature, chunk bounds/CRC, IDAT decompression, Pillow verification and full pixel decode; dimensions/size confirmed.
- Relative Markdown file links in all changed/new documents resolve.
- Changed paths are restricted to the twelve documentation/reference files above.
- The original V1 body from `## Shared faction language` onward is unchanged.
- Every existing Chapter 01 world-blueprint file has the same Git blob hash as baseline.
- `git diff --check` passes for the complete baseline diff; staged whitespace checks pass for checkpoints.

Initial contract QA checked 26 local links; final QA checked 30 including this report's links. This QA certifies reference/document consistency, not Unity runtime or visual production.

**Not run:** Unity compile, deterministic gameplay tests, play-mode smoke tests, Unity scene/project validation, Android APK build and real-device human visual acceptance. Unity 6000.3.0f1 is installed, but this delivery adds only documentation and a reference PNG outside `Assets/`; it changes no executable behavior or imported production assets. No actor implementation/spec integration is being certified, and no Unity/build session was launched against the parallel production workspace. No APK was produced; there is no APK path for this delivery. Those checks remain mandatory for the later implementation described by the proof contract.

## Assumptions and remaining limits

The existing file at the exact requested V2 path is the owner-approved authority identified by the task. No substitute was searched for or generated. Scale-strip labels are approximate and do not state their axis; the proof records scalar comparisons and prohibits imposing the vehicle label as height. Hidden construction details are not claimed to be approved orthographic design. Real-camera collider/socket alignment and Carrier's exact existing-event mechanism still require later integration evidence; no unshown weapon is invented to fill that gap.

The authority package has no missing-image blocker. Donor retargeting/import, final geometry/material quality, sustained Android performance and owner device approval remain unverified future production work. The PR remains draft and unmerged.
