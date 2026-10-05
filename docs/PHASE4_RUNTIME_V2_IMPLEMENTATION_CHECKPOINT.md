# Phase 4 Runtime V2 — Implementation Checkpoint

Branch: `feature/chapter1-repeatable-loop-runtime-v2`
Baseline main: `43e0416f341e3b106ad976053746da540a1b85d4`

This checkpoint exists to prove remote persistence before substantial implementation.

## Source of truth

Implementation follows the merged contracts:

- `docs/CHAPTER1_REPEATABLE_LOOP_SPEC.md`
- `docs/CHAPTER1_REWARD_LADDER.md`
- `docs/CHAPTER1_REPEATABLE_STATE_MATRIX.md`
- `docs/CHAPTER1_SAVE_MIGRATION_PLAN.md`

## Owned runtime scope

- save schema v2 and sequential v1 -> v2 migration
- repeatable Magnetar and Custodian domain state
- 15 minute Magnetar / 30 minute Custodian cooldowns
- anchored 24h premium reward windows and fallback rewards
- crash-safe pending reward transaction
- persisted effective UTC floor
- stronger authored ordinary spots with independent adaptive pressure
- authoritative WorldMarker read-state extensions
- deterministic EditMode/runtime-domain tests

## Conflict boundary

This branch must not own presentation wiring, Map/Minimap presenter UI, Audio/VFX production integration, Phase3D visuals, VisualReplacementProofV1, or production-art replacement. `S01SceneCompositionRoot` is avoided unless a gameplay integration hook cannot be exposed elsewhere.

## Checkpoint plan

1. Save v2 + shared effective UTC authority.
2. Repeatable Magnetar + Custodian state/reward entitlement.
3. Durable pending reward transaction and recovery.
4. Stronger ordinary authored spots and independent pressure.
5. Marker authority extensions + deterministic tests + final ownership audit.

No implementation redesign is intended unless the merged contracts contradict the current runtime architecture.
