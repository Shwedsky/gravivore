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
