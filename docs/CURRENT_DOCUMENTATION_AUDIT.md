# GRAVIVORE current-documentation conflict audit

Status: **CURRENT / PASSED FOR VISUAL AUTHORITY**

Purpose: verify that new Codex work is no longer pulled toward superseded prototype/low-poly assumptions by active documentation.

## Resolved conflicts

### Art style

**Resolved.** Active authority now defines the target as mobile-optimized premium hard-surface industrial sci-fi. Low-poly/LOD/material reduction is optimization, not aesthetics.

### G-0 anatomy

**Resolved.** Current authority says G-0 is bipedal. Historical radial/four-support/tank-like player directions cannot override it.

### Empty/quiet environment language

**Resolved.** Current art direction requires dense but readable functional industrial composition: layered floor, substantial gates/walls, connected services, large machinery/facilities, wreckage and edge massing while combat centres remain clear.

### Spawn spot vs real facility

**Resolved.** Spawn data owns spawn behavior only. Current docs explicitly allow substantial fabrication/deployment/maintenance facilities or other industrial structures around the same encounter identity without transferring gameplay authority to art.

### Old spawn coordinates / facility footprints as permanent authority

**Resolved.** `S04` now preserves encounter identity, population/respawn/progression semantics rather than historical anchor coordinates. During an explicitly approved world rebuild, anchors may move inside the rebuilt sector and visual facilities may use materially different footprints.

### Historical Chapter01 topology / collision fingerprint as permanent visual authority

**Resolved.** `S08` now preserves gameplay/progression semantics rather than the V44–V47 physical presentation layout. An approved full world rebuild may change sector coordinates, lanes, floor/wall geometry, facility footprints, gate framing, minimap geometry and explicit collision proxies. The old collision fingerprint is no longer an acceptance requirement for a new visual layout.

`docs/visual-blueprints/chapter01/CHAPTER01_VISUAL_BLUEPRINT_V1.md` is now the scene-composition authority for the next Chapter01 rebuild.

### Proxy/kitbash as final art

**Resolved.** Proxy geometry and quick kitbash are exploration tools, not automatic production acceptance. Production/concept-fidelity passes must materially remove obvious proxy/stock identity.

### Material flattening

**Resolved at documentation/authority level.** The legacy S15 `Body / Accent / Dark` material-replacement path may remain for backward compatibility, but active docs explicitly forbid routing new rich production art through it when it destroys PBR/surface information. Whole reviewed presentation prefabs or information-preserving consolidation are preferred.

### Performance ceilings

**Resolved.** Old Phase/Art-Spike per-asset triangle/material/light numbers are historical guardrails unless a current task reaffirms them. Real Android frame time/readability is the performance gate. Performance optimization must not redefine the style as low-poly.

### Dynamic-light limit vs visual depth

**Resolved.** Current docs explicitly permit baked/lightmapped, emissive, reflection/fake-light and bounded local-light solutions. A low realtime-light count is not permission for a flat scene.

### Historical docs masquerading as rules

**Resolved.** Superseded Art Spike, Phase 3, first-visual-slice, Chapter01 production/V3, device-correction and related evidence have been moved under `docs/history/` or replaced at their old root path by an explicit historical pointer.

`docs/history/` is declared non-authoritative in `AGENTS.md`, repository README and `docs/CURRENT_AUTHORITIES.md`.

### New phase documentation polluting current root

**Resolved by policy.** New versioned implementation reports/evidence belong under `docs/history/implementation-passes/<pass-id>/`. Stable decisions must be promoted separately into CURRENT docs.

### Asset source-path ambiguity

**Resolved.** New external raw intake has one canonical location: `ExternalAssetIntake/Current/`.

Historical locations (`ExternalAssetIntake/FreeAssetIntakeV1/`, root `98_unclassified/`, root `00_reference/`, `.local-g0-v2/`, old worktree scratch) are explicitly non-current. `docs/free-asset-intake-v2/` remains useful vetted reference/evidence, but not a live payload path.

For the Chapter01 Visual Blueprint V1, the current owner-local raw inputs are explicitly assigned to dedicated Quaternius MegaKit and Molten Maps subfolders under `ExternalAssetIntake/Current/`.

## Deliberately retained constraints

These are not visual conflicts and remain valid:

- portrait Android / URP / Unity 6.3;
- target 60 FPS on a reasonable mid-range Android device;
- gameplay authority separated from presentation;
- explicit collision/interaction/spawn authority;
- pooled/bounded repeated combat VFX;
- no asset ripping or unclear/incompatible licenses;
- current zero-spend asset decision until explicitly changed;
- public-repository redistribution safety;
- real APK/device as final visual gate.

## Remaining implementation debt, not documentation conflict

Some legacy runtime/editor systems still exist because old content depends on them. Examples include the S15 part/material fallback and presentation validators designed around strict gameplay/presentation separation.

Old V44–V47 presentation layers and builders also remain implementation/history debt until the new Chapter01 blueprint is implemented. They must not be treated as the geometric starting point merely because they still exist in the project.

They may be refactored or removed by the next explicitly approved world-rebuild task when they materially block the current target. Their mere existence must not be interpreted as visual authority.

## Final result

For new visual work, the active documentation no longer requires or prefers:

- a low-poly aesthetic;
- four-support G-0;
- quiet/open sparse industrial floors as the target;
- primitive proxy geometry as production art;
- tiny decorative spawn markers instead of functional facilities;
- flat three-role material replacement for rich authored assets;
- old Phase 3 triangle/light numbers as permanent quality ceilings;
- historical asset cache paths;
- old Chapter01 spot coordinates, facility footprints, repeated floor grid or collision fingerprint as permanent layout authority.

If future work reintroduces one of those assumptions, treat it as a regression against the current authority documents, not as a legitimate interpretation of project history.