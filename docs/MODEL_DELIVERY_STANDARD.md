# Model Delivery Standard — Visual Replacement Production V2

Primary visual authority: approved GRAVIVORE prototype board `Концепт GRAVIVORE: Мехи и окружение.png`.

This document defines the handoff contract for 3D production to later Unity/Codex integration. Integration should receive finished assets and deterministic metadata, not unresolved art-direction choices.

## 1. Accepted source formats

Preferred delivery:
- `.fbx` for rigged/animated characters and mechanical enemies;
- `.glb` acceptable for static props/environment and some base-mesh workflows;
- source `.blend` retained outside runtime import where practical for editability/provenance.

Textures:
- PNG/TGA for production maps;
- source authoring files optional but desirable for hero assets.

## 2. Scale and orientation

- 1 unit = 1 meter.
- +Y up.
- +Z forward for characters/enemies unless existing Unity importer contract explicitly requires another orientation.
- Root transforms: position 0,0,0; rotation 0,0,0; scale 1,1,1 at delivery.
- Apply/freeze model transforms before export.
- No negative scales in delivered hierarchy.

## 3. Pivot standards

### Characters/enemies
Root pivot at logical ground-center of the gameplay body/footprint.

### Props
Pivot at practical placement point:
- floor props: bottom center;
- wall modules: grid-snapped lower corner or documented family origin;
- gates: frame origin aligned consistently across variants;
- rotating/animated machinery: actual mechanical hinge/axis.

### Repeated modules
All variants in one family must share a consistent pivot convention.

## 4. Hierarchy and naming

Naming pattern:

`GV_<Category>_<AssetName>_<Variant>_<Element>`

Examples:
- `GV_Player_G0_T0_ROOT`
- `GV_Player_G0_T0_Core`
- `GV_Enemy_Scout_ROOT`
- `GV_Enemy_Magnetar_Leg_FL_Upper`
- `GV_Env_Wall_A01`
- `GV_Prop_Reactor_Hero01`

No spaces. Use ASCII names for technical objects.

## 5. Required socket transforms

Characters/enemies should expose named transforms where relevant:
- `SCK_Core`
- `SCK_Attack_Primary`
- `SCK_Attack_Secondary`
- `SCK_VFX_Hit`
- `SCK_DeathVFX`
- `SCK_WeakPoint`
- `SCK_UI_Anchor`

G-0 additionally:
- left/right forward weapon sockets;
- rear/core containment sockets if evolution or VFX requires them.

Socket positions must be visually authored, not guessed during integration.

## 6. Material slots

Target minimal slot count.

Recommended:
- repeated environment module: 1–2 slots;
- hero prop: 1–3 slots;
- ordinary enemy: 2–3 slots;
- G-0: 2–4 slots;
- elite: 3–4 slots;
- boss: 3–5 slots.

Typical logical grouping:
- armor/painted metal;
- dark structure;
- emissive/energy;
- optional unique detail material.

Do not assign unique material slots to tiny bolts or panels that can share an atlas/material.

## 7. UV requirements

### UV0
Required for base color/normal/metal-rough/AO texture set.

- no unintended overlaps unless explicitly mirrored/stacked;
- consistent texel density within an asset family;
- adequate padding for target texture size/mips;
- hero assets receive clean authored unwrap.

### UV1 / lightmap UV
Provide when static-lighting workflow requires it; otherwise Unity-generated secondary UV may be acceptable for simple environment modules if validated.

## 8. Texture set

Preferred packed PBR approach should match project shader workflow.

Minimum logical data:
- base color/albedo;
- tangent-space normal;
- metallic;
- roughness or smoothness;
- AO where beneficial;
- emissive mask/color where needed.

Packing may combine metallic/roughness/AO into one mask texture if Unity material pipeline supports it.

No baked lighting in albedo.

## 9. Texture sizing guidance

Suggested maxima before device profiling:
- G-0: 2K master maps, Unity may import 1K/2K depending final need;
- ordinary enemy: 1K;
- elite: 1K–2K;
- boss: 2K, optionally one additional 1K detail/energy set if justified;
- hero environment prop: 1K–2K;
- repeated environment module: 512–1K, ideally atlas/trim based;
- small props: 512–1K.

Do not use 4K by default for a portrait mobile game.

## 10. Normal maps

- tangent-space normals;
- no severe waviness from poor hard-surface bake;
- correct smoothing groups/custom normals;
- visible hard edges supported by UV splits where required;
- test under the actual Unity shader and camera.

## 11. Rig requirements

### G-0
Rig must support:
- full locomotion articulation;
- body stabilization;
- front attack appendage motion;
- core/containment secondary motion if needed;
- idle;
- hit/death reaction.

Mechanical joints should rotate around physically plausible axes.

### Ordinary enemies
Simple deterministic rigs. Avoid unnecessary deform bones where rigid parented components are sufficient.

### Elite/boss
Segment rigs by actual visible mechanical articulation. Boss legs should support readable planted and telegraph poses.

## 12. Animation clip delivery

Where animation is part of the asset handoff, name clips consistently:
- `Idle`
- `Move`
- `Attack_Primary`
- `Attack_Secondary`
- `Attack_Special`
- `Hit`
- `Death`
- `Spawn` where relevant.

Boss/elite may add explicitly named telegraph/phase clips.

No animation should embed unintended root scale changes.

## 13. LOD requirements

At least LOD0/LOD1 for hero repeated assets where savings are meaningful; LOD2 for boss/large environment hero pieces if needed.

Target screen-relative reductions, not arbitrary decimation.

Suggested triangle reduction:
- LOD1: ~45–65% of LOD0;
- LOD2: ~20–35% of LOD0.

Preserve silhouette and major joint geometry first.

## 14. Triangle budgets — LOD0 starting targets

These are production guardrails, not automatic acceptance criteria.

### G-0 Tier 0/1/2
- target: 18k–32k triangles per active form;
- soft maximum: ~40k if profiling justifies it.

### Ordinary enemy
- target: 6k–14k;
- soft maximum: ~18k.

### Elite
- target: 16k–28k;
- soft maximum: ~35k.

### Boss
- target: 35k–65k;
- soft maximum: ~80k only with profiling and LOD support.

### Hero environment prop
- target: 4k–15k;
- soft maximum: ~20k for landmark reactor/gate.

### Repeated environment module
- target: 500–4k;
- soft maximum: ~6k when silhouette requires it.

### Small repeated prop
- target: 300–3k.

## 15. Renderer budgets

Aim to minimize renderers/draw-call fragmentation.

Per prefab guidance:
- G-0: <= 6 renderers preferred;
- ordinary enemy: <= 4;
- elite: <= 6;
- boss: <= 10, lower if practical;
- hero prop: <= 4;
- repeated environment module: 1–2.

Separate renderers are justified for independently animated or materially distinct systems, not for arbitrary modeling organization.

## 16. Hard-surface modeling standard

Required:
- bevels large enough to catch light at gameplay scale;
- macro/medium shape hierarchy before greebles;
- no visibly intersecting unrelated shells;
- plausible panel thickness on silhouette edges;
- mechanical clearances around joints;
- hidden geometry simplified.

Avoid:
- micro-bevel spam;
- boolean scars visible in shading;
- razor-thin hero armor;
- dense topology on flat hidden surfaces.

## 17. Collision handoff

Visual meshes should not assume collision complexity.

Provide simple collision proxy suggestions where shape is nontrivial, but final gameplay collision ownership remains with Unity integration/gameplay. Environment art must visually agree with intended collision boundaries.

## 18. File package per asset

Each delivered hero asset folder should contain:
- final FBX/GLB;
- textures;
- preview image/turnaround;
- short README with scale, pivot and material notes;
- license/provenance if any third-party source contributed;
- source asset reference or hash for donor content when required;
- rig/clip list if animated.

## 19. Quality-control checklist before handoff

Artist must verify:
- transforms clean;
- no missing textures;
- correct orientation/scale;
- correct pivot;
- material slot count within target;
- UVs valid;
- normals/tangents valid;
- no hidden duplicate meshes;
- rig joints aligned;
- socket names/positions correct;
- triangle count reported;
- renderer count reported;
- LODs generated where required;
- gameplay-camera preview passes visual bible.

## 20. Integration boundary

Codex/Unity integration should be able to:
1. import the file;
2. bind existing gameplay/presentation references;
3. assign documented material/shader setup;
4. connect named sockets;
5. validate scale and performance;
6. replace old presentation asset.

It should **not** need to decide how G-0 should look, which armor plate is missing, where the weak point belongs, what color the elite should be, or how to redesign a bad silhouette.