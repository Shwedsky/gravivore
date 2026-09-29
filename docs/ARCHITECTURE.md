# Technical Architecture

## 1. Architecture goal

Production-shaped without overengineering the vertical slice.

Use clear module boundaries so future cloud saves, RuStore payments, ads and more chapters can be added around the gameplay rather than through it.

## 2. Recommended Unity project structure

```text
Assets/
  _Game/
    Runtime/
      Core/
      Combat/
      Progression/
      World/
      Enemies/
      Player/
      Quests/
      Save/
      Offline/
      UI/
      Platform/
    Content/
      Definitions/
      Prefabs/
      Scenes/
      Materials/
      VFX/
      Audio/
    Editor/
      Build/
      Validation/
      ContentTools/
    Tests/
      EditMode/
      PlayMode/
  ThirdParty/
    <vendor-pack-name>/
```

Do not place project-owned code in random root folders.

## 3. Assembly boundaries

Suggested asmdefs:

- `Gravivore.Core`
  - pure/simple domain types, math, stat models, interfaces where practical.
- `Gravivore.Gameplay`
  - player, combat, enemies, world, progression.
- `Gravivore.Persistence`
  - save DTOs, repositories, migrations.
- `Gravivore.Presentation`
  - MonoBehaviours, UI, VFX bridges.
- `Gravivore.Platform`
  - Android/platform abstractions and future payment/ad bridges.
- `Gravivore.Editor`
  - build/validation/content tools.
- test assemblies.

Do not create assembly boundaries that cause circular dependencies.

## 4. Composition

Use a small bootstrap/composition root in the gameplay scene.

Responsibilities:
- load global config;
- create plain C# services;
- wire scene presenters/controllers;
- start save/profile;
- start game session.

Avoid a universal service locator.

## 5. Data definitions

Use ScriptableObjects for static authoring data:

- `EnemyDefinition`
- `SpawnSpotDefinition`
- `PlayerProgressionDefinition`
- `StatCurveDefinition`
- `EvolutionDefinition`
- `BossDefinition`
- `QuestDefinition`
- `EquipmentDefinition` (minimal slice support)
- `OfflineRewardDefinition`
- `GameBalanceConfig`

Static asset references belong here.

Runtime mutable state does not.

## 6. Runtime state

Examples:
- `PlayerStatsState`
- `ProgressionState`
- `QuestState`
- `WorldUnlockState`
- `InventoryState`
- `SessionState`

Runtime state must be serializable to dedicated Save DTOs via mapping, not by serializing scene objects.

## 7. Combat contracts

Useful contracts:

```csharp
public interface IDamageable
{
    bool IsAlive { get; }
    DamageResult ApplyDamage(in DamageRequest request);
}

public interface ITargetable
{
    Transform TargetPoint { get; }
    bool CanBeTargeted { get; }
}

public interface IDisplaceable
{
    DisplacementResistance Resistance { get; }
    bool TryDisplace(Vector3 destination, DisplacementContext context);
}
```

Names may be adjusted, but preserve capability separation.

## 8. Events

Use explicit C# events/signals scoped to owned services.

Examples:
- enemy died;
- reward granted;
- stat level changed;
- evolution tier changed;
- objective progressed;
- boss defeated.

Do not implement a string-based global event bus.

## 9. Enemy state machine

Ordinary enemy states:
- Spawn
- Idle/Patrol
- Aggro/Approach
- Attack
- HitReaction (optional/short)
- Pulled/Displaced
- Dead
- Despawn/Pooled

Boss state machine is separate and phase-aware.

## 10. Pooling

Pool:
- ordinary enemies;
- core pickup visuals;
- gravity lash VFX/projectiles;
- hit VFX;
- floating combat text if used.

The pool must reset runtime state explicitly when an object is returned.

## 11. Save system

Path: `Application.persistentDataPath`.

Files:
- `profile.json`
- `profile.backup.json`
- temporary write file.

Rules:
- schema version integer;
- atomic-ish write via temp -> replace;
- maintain last known good backup;
- validate ranges/required ids on load;
- migrate older versions;
- if main corrupt and backup valid, recover and log;
- if both invalid, create a new profile after preserving bad files for diagnostics.

Progression must never use `PlayerPrefs`.

## 12. Save DTO example

```text
SaveRoot
  schemaVersion
  profileId (locally generated GUID)
  createdUtc
  lastSeenUtc
  progression
    statLevels
    totalAssimilation
    evolutionTier
  world
    defeatedEliteIds
    defeatedBossIds
    unlockedGates
  quests
  inventory
  offline
  settingsVersionRef (optional)
```

## 13. Time

`ITimeProvider` supplies UTC time.

Private APK:
- system UTC time.

Future online:
- trusted/server-adjusted time provider can replace it.

Do not bake `DateTime.UtcNow` throughout gameplay.

## 14. Future account boundary

Create conceptual interfaces only when needed by code; do not build a fake cloud system.

Future flow:
`AnonymousLocalProfile -> AuthenticatedAccount -> merge/link profile -> cloud snapshot/versioning`

Gameplay should consume a profile service, not an auth SDK.

## 15. Future monetization boundary

No RuStore SDK dependency in v0.1.

Future interfaces may look like:
- `IProductCatalog`
- `IPurchaseService`
- `IEntitlementService`
- `IRewardedAdService`

Gameplay/economy reacts to verified entitlements, never vendor SDK objects.

## 16. Scene strategy

Recommended:
- `Bootstrap` scene: minimal initial loading/composition.
- `Chapter01_ScrapExclusion` scene: vertical slice world.
- optional test scenes under Tests/Dev only.

Do not split the one small chapter into many additive scenes unless profiling/content size later justifies it.

## 17. Performance budget

Initial guardrails:
- <= 25 ordinary live enemies;
- <= 1 elite + 1 boss;
- mobile-friendly mesh complexity;
- shared materials/atlas where possible;
- avoid real-time shadows on every small enemy;
- minimal dynamic lights;
- pooled effects;
- physics layers configured narrowly;
- fixed update only where needed.

## 18. Logging

Use structured categories:
- Boot
- Save
- Combat
- Progression
- World
- Offline
- Platform

No noisy per-frame logging in release/dev APK.

## 19. Feature flags

Simple local dev flags are allowed for:
- god mode;
- unlock boss;
- grant progression;
- reset save;
- show debug overlay.

They must be excluded/disabled in non-development release configuration.

## 20. Combat health and death

- `DamageResolver` is the single deterministic implementation of physical armor mitigation.
- `HealthState` owns reusable hit-point, clamp, reset, and once-per-life death semantics.
- Player and enemy components compose `HealthState`; controllers do not duplicate damage math.
- Enemy death is published as a typed, scoped event before the enemy returns to its pool. Reward systems may observe that event later, but health does not grant rewards.
- `PlayerHealthController` owns the configured central respawn position and post-respawn invulnerability window. Respawn restores health and publishes a typed hook for transient combat reset without changing `PlayerStatsState.BaseLevels`.

## 21. Assimilation progression

- Every pooled enemy activation receives an immutable `EnemyLifeId`; reward deduplication uses this life token rather than a Unity object or instance id.
- `EnemyDeathEvent` is an immutable death snapshot containing the life id, enemy archetype id, and death position. It remains valid after the enemy returns to its pool.
- `AssimilationProgressionService` subscribes to the scoped `EnemyPopulationController.EnemyDied` event and routes rewards through `PlayerProgressionDefinition` data.
- `ProgressionState` owns stat XP, total assimilation, first-kill state, and processed life ids. `PlayerStatsState` remains the authority for permanent integer stat levels and derived values.
- Reward state is committed before presentation events. `ProgressionDirtyEvent` is only a persistence boundary notification; S06 does not implement save storage.

## 22. Visual evolution

- `EvolutionStateSelector` deterministically derives Tier0/Tier1/Tier2 from total assimilation and a dominant stat from permanent base levels plus configured tie priority.
- `PlayerEvolutionPresenter` is a presentation read-model over `ProgressionState` and `PlayerStatsState`; it never grants stats or mutates progression.
- `PlayerEvolutionView` owns explicit Core/Left/Right/Rear sockets and idempotently activates the exact current tier module set plus one dominant-stat accent.
- Reinitializing the presenter performs a full state apply and does not replay milestone VFX. Tier-change events and VFX hooks only fire for runtime transitions.
- Canonical thresholds `10` and `30` are provisional absolute vertical-slice balance values. Final tuning is deferred to S20.

## 23. Minimal equipment

- `EquipmentDefinition` and `EquipmentCatalogDefinition` author immutable item data; runtime identity is the stable item id, never a mutable ScriptableObject reference.
- `InventoryState` owns non-stackable item ownership and one equipped item for each of the three v0.1 slots. `EquipmentService` validates mutations, applies the equipped set, and publishes scoped typed events after mutation.
- `PlayerStatsState` keeps deterministic modifier sources. Equipment replaces only the `Equipment` source, while progression continues to own permanent `BaseLevels` and other systems retain the `External` source.
- `InventorySaveMapper` maps runtime state to a dedicated DTO and validates restore data. S10 does not implement file storage, migrations, or the S12 save repository.

## 24. Quests and onboarding

- `QuestDefinition` authors stable quest/objective ids, generic objective types, target references, and data-driven counts. Runtime objective identity never uses Unity instance ids.
- `QuestState` owns authoritative objective progress, completed ids, processed enemy life ids, expanded-onboarding state, and primary sequence completion.
- `QuestService` observes committed progression, elite, boss, and movement signals; it mutates state before publishing quest events so presentation failures cannot roll back progress.
- S11 persistence stops at `QuestSaveDto` and `QuestSaveMapper`. It exports/restores runtime state and validates quest/objective ids, but it does not write files or implement S12 migration/repository logic.
- Chapter 01 elite unlock reads completed canonical spot objective ids plus `ProgressionState.TotalAssimilationScore`; it no longer uses first-kill enemy ids as the gate requirement.

## 25. Local profile and offline reward

- `JsonProfileRepository` owns JSON files under `Application.persistentDataPath`: `profile.json`, `profile.backup.json`, and `profile.temp.json`. A write is accepted only after the temporary file can be deserialized, migrated, and validated; the previous valid main becomes the recovery backup before the new main is committed.
- Loading prefers a valid main profile, falls back to a valid backup, and preserves invalid files with timestamped `.corrupt` names before creating a fresh profile. A stale temporary file is never a load candidate.
- `SaveMigrationPipeline` applies explicit one-version steps and rejects negative, future, or unsupported schema chains. Version `0 -> 1` fills the newly introduced offline section from current defaults without inventing progression.
- `ProfileSaveMapper` is the boundary between DTOs and authoritative runtime state. It validates stable ids, levels, XP ranges, quest/inventory DTOs, monotonic world unlocks, boss completion, timestamps, and offline balances before any gameplay service is constructed.
- `ProfileSession` restores the full state graph before scene services subscribe to events. Equipment modifiers, quest/world read models, evolution presentation, and defeated encounter states therefore initialize from restored state without replaying historical rewards or milestones.
- `SaveCoordinator` observes scoped authoritative mutation events, debounces routine autosaves, and performs synchronous checkpoints on pause, quit, and composition teardown. A failed write leaves runtime state active and dirty for retry.
- `ITimeProvider` is the only clock boundary. The production provider reads system UTC; tests inject deterministic UTC values. Gameplay and persistence code do not call `DateTime.UtcNow` directly.
- Offline reward is derived once at profile startup from `lastSeenUtc`, a configured active baseline, efficiency, and a strict two-hour cap. Earned material enters persisted `pendingReward`; claiming transfers it exactly once into `materialBalance`. Offline reward does not mutate assimilation, stats, quests, equipment, or world unlocks.
