# GRAVIVORE — Map / Minimap Presentation Implementation

Status: Phase 5A presentation implementation.  
Design authority: merged `main` documentation from PR #43 (`MAP_MINIMAP_UX_SPEC`, marker state matrix, device review, wireframes).

This module is intentionally **not wired into Chapter01**. It can be instantiated under any UGUI `RectTransform`/Canvas and initialized with an `IMapMarkerSource`.

## Architecture

The implementation lives under `Assets/_Game/Runtime/Presentation/Map/` and keeps gameplay authority outside the map.

- `MapPresentationModel.cs` — presentation DTOs and input/selection boundaries.
- `CurrentWorldMarkerMapAdapter.cs` — thin adapter from the existing `IWorldMarkerSource` / `WorldMarkerSnapshot` contract.
- `MapProjection.cs` — deterministic world X/Z -> normalized map projection.
- `MapMarkerVisualResolver.cs` — kind/status -> shape/overlay mapping.
- `MapTimerFormatter.cs` — rounded overview and exact selected-POI duration formatting.
- `MapMinimapPresenter.cs` — the single refresh/controller loop, compact/expanded interaction, marker cache and selection details.
- `MapMarkerView.cs` / `MapUiFactory.cs` — allocation-light UGUI views built from simple geometry; no sprite atlas or RenderTexture camera is required.
- `Prefabs/MapMinimapPresentation.prefab` — reusable root component. Runtime children are built once on initialization so the prefab has no Chapter01 or scene-object references.

Dependency direction:

`gameplay/read model -> adapter -> IMapMarkerSource -> MapMinimapPresenter -> UGUI`

The presenter never reaches into enemy, boss, persistence, Phase 3C, scene composition or visual prefab transforms.

## Projection

`MapProjection` maps gameplay **X/Z** coordinates to UI **X/Y** coordinates. Positive world Z is always screen-up, therefore the map is north-up and player heading only rotates the player chevron.

Default Chapter 1 presentation bounds are centered at `(0, 0)` with the task-provided approximate size:

- width X: `72`
- depth Z: `140`

These are presentation configuration defaults, not renderer-derived bounds. Production wiring should pass authoritative Chapter bounds if the gameplay layout is offset from world origin.

Expanded map uses the whole configured Chapter bounds and clamps coordinates to `[0..1]`.

Compact map uses a local square window (default radius `28` world units). Near Chapter edges the local window is shifted/clamped inside Chapter bounds instead of exposing blank space. This intentionally allows the player chevron to move away from compact-map center near an edge.

No projection code accepts Renderer/Prefab bounds, Phase3C transforms or scenery anchors.

## Current marker support

The current `WorldMarkerReadModel` on `main` exposes:

- Player
- RegularSpot
- Elite
- Boss

`CurrentWorldMarkerMapAdapter` maps those existing values without changing their authority. Current `Respawning` becomes presentation `Cooldown`; current elite/boss `Locked` is represented with the progression-lock overlay because that is the only present read state available on `main`.

The current read model does **not** expose RepairHub, stronger ordinary, Phase 4 repeat cooldowns, repeat reward state or progression-lock detail as independent dimensions. The adapter does not fabricate them. Unknown current kind/status enum values are rejected explicitly rather than silently reclassified as an ordinary/inactive marker.

`MapMarkerSnapshot` already has presentation fields/kinds for:

- RepairHub
- StrongOrdinary
- cooldown state + optional progress fraction
- independent `progressionLocked`
- independent `rewardState`
- supplied `remainingSeconds`

These fields are extension points for the later Phase 4/read-model adapter. They do not add gameplay state.

## Visual language

The module uses a dark industrial diagnostic surface, thin grid, restrained system/steel/danger roles and angular geometry.

State is redundant with shape/overlay, not color-only:

- player — chevron, heading rotates inside fixed north-up map;
- ordinary — node;
- stronger ordinary — node plus outer brackets;
- elite — diamond;
- boss — larger core/diamond hierarchy;
- repair hub — cross;
- cooldown — hollow treatment + segmented recovery ring;
- progression lock — lock/bar overlay without the generic inactive slash;
- inactive — slash treatment;
- defeated boss — retained boss symbol with completion/check treatment;
- active — corner brackets;
- reward full/fallback — expanded-only pip, with barred fallback treatment.

Compact map has no permanent POI labels and uses a 27% safe-area-width square slot in the upper-right, below the current reserved top HUD bands. Expanded map owns marker labels. Rounded cooldown text is limited to elite/boss overview; ordinary/strong-ordinary recovery remains ring-only until selected, matching the merged timer policy.

## Timers

The map does not start, decrement or persist cooldown clocks.

Input is presentation state supplied by the adapter:

- `remainingSeconds`
- optional `cooldownProgress01`

Compact map shows only the segmented recovery ring.

Expanded elite/boss overview uses `MapTimerFormatter.FormatRounded`, for example `12 мин` or `1 ч 5 мин`. Ordinary/strong-ordinary overview remains ring-only.

Selected POI details use `MapTimerFormatter.FormatExact`, for example `12:34` or `1:02:34`.

Rounded marker timer strings are regenerated only when their coarse bucket changes. Selected exact text is regenerated only when the whole-second bucket or selected marker state changes.

## Performance model

- exactly one `MapMinimapPresenter.Update`, never one Update per marker;
- no LINQ in refresh paths;
- marker views are cached by stable marker id and reused;
- no per-refresh GameObject creation for existing ids;
- compact/expanded marker positions use direct normalized anchors;
- no RenderTexture and no second camera;
- static grid/views are created once;
- stale marker views are deactivated and retained for reuse;
- visual state uses structs and direct loops;
- auto refresh defaults to 10 Hz and can be disabled for explicit dirty/event-driven integration;
- the presenter never derives or advances gameplay timers.

The later Chapter integration may call `RefreshNow()` from a read-model dirty signal and disable auto refresh entirely.

## Prefab / isolated review

Prefab:

`Assets/_Game/Runtime/Presentation/Map/Prefabs/MapMinimapPresentation.prefab`

It contains only a full-stretch `RectTransform` plus `MapMinimapPresenter`. The presenter creates its internal UGUI hierarchy when initialized. This keeps serialized dependencies out of Chapter01 and makes the same prefab usable under an isolated test Canvas.

The PlayMode smoke test initializes the module with a presentation-only mutable mock source and exercises:

- moving player marker;
- ordinary/elite/boss/repair marker families;
- compact -> expanded -> compact interaction;
- marker state transition while preserving the cached view;
- selected exact timer detail;
- multiple portrait host sizes/aspects.

Mock-only test states include RepairHub, independent reward state and cooldown progress because the current production read model does not expose those Phase 4-facing fields. They exist only in test input and are not labeled as mock data in runtime UI.

## Future Phase 4 adapter requirements

When Phase 4 runtime/read state exists, integration should supply resolved presentation snapshots rather than reimplementing Phase 4 rules in the map. The adapter should provide, where authoritative data exists:

- stable marker id;
- authoritative world/gameplay coordinate;
- marker kind including stronger ordinary/repair hub if exposed;
- encounter availability state;
- independent progression lock state;
- remaining cooldown/respawn duration;
- optional normalized cooldown/recovery progress;
- repeat reward state (`Full`, `CappedFallback`, `None`);
- localized/user-facing display name;
- player heading.

Do not add save fields solely for map presentation.

## Integration instructions

Production Chapter01 wiring is intentionally deferred. Later integration should be a small composition diff:

1. Instantiate `MapMinimapPresentation.prefab` under the existing safe-area HUD Canvas/root. Its compact panel is normalized to an upper-right safe-area slot below the current menu and boss-HP bands, without taking dependencies on those presenters. Any future HUD-layout change remains an integration-layout concern.
2. Create `CurrentWorldMarkerMapAdapter` around the existing `IWorldMarkerSource` (or a Phase 4-aware presentation adapter once available).
3. Supply player heading through the adapter input; do not read Phase 3C visual transforms for marker positions.
4. Call `MapMinimapPresenter.Initialize(adapter, chapterBounds, localRadius, optionalSelectionSink)`.
5. If the Chapter composition has a marker/read-model dirty signal, call `SetAutoRefresh(false)` and drive `RefreshNow()` from that signal. Otherwise the module's single coarse refresh loop is available.
6. Bind `ExpandedChanged` to the existing modal/input policy in the integration branch if expanded-map opening must block gameplay. This Phase 5A module deliberately does not own timescale or movement input.
7. Bind `PoiSelected` / `IMapPoiSelectionSink` only if another presentation surface needs selected-POI data.

Do **not** wire through `S01SceneCompositionRoot`, `Chapter01`, `Chapter01WorldPresenter` or `Phase3CIntegrationBuilder` in this branch.

## Tests

EditMode coverage added for:

- projection center;
- world edges;
- clamping;
- local-radius behavior;
- north-up stability;
- marker view reuse;
- exact and rounded timer formatting;
- icon/status mapping;
- current `WorldMarkerReadModel` adapter mapping;
- independence from unrelated visual transform positions.

PlayMode coverage added for the isolated module behavior listed above.

## Review captures

Required capture names for a Unity-capable review run:

- `01_compact_map.png`
- `02_expanded_map.png`
- `03_marker_states.png`
- `04_small_portrait.png`
- `05_tall_portrait.png`

This implementation session had repository access but no Unity Editor/CLI runtime, so those image files were **not fabricated or committed**. The PlayMode mock state is ready to serve as the capture fixture when the branch is checked out in Unity.

## Conflict boundary

This branch does not modify:

- `WorldMarkerReadModel` implementation;
- persistence/save;
- elite/boss controllers;
- repeatable-loop runtime;
- `S01SceneCompositionRoot`;
- Chapter01 scene;
- `Phase3CIntegrationBuilder`;
- Phase3C visual assets;
- `Chapter01WorldPresenter`.

**MAP / MINIMAP PRESENTATION: READY FOR INTEGRATION**


## Rebase self-review note

After `main` advanced to include Phase 3C and the merged #43 docs, this module was replayed on the new base and re-reviewed against the merged UX contract. The review corrected only local presentation defects: locked-vs-inactive overlay semantics, boss defeated completion treatment, ordinary overview timer policy, hidden/closed-map selection lifecycle, expanded-background selection clearing, strict current-adapter enum handling, and compact normalized safe-area sizing. No Chapter01, Phase3C, persistence or gameplay authority wiring was introduced.
