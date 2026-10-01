# S16 Local Analytics and Development Tools

S16 is local-only QA instrumentation. It adds no remote analytics service, account identifier, advertising identifier, location, free-form user text, or hardware fingerprint. The only identifiers written are a local profile GUID, a per-run session GUID, and stable game-content ids.

## Local telemetry

Editor and Development Builds write one bounded JSON Lines file per runtime session:

`Application.persistentDataPath/dev-analytics/session-<UTC>-<session-guid>.jsonl`

Each line uses schema version `1` and contains `sessionId`, `profileId`, monotonic `sequence`, round-trip UTC timestamp, `eventName`, and a typed JSON `payload`. Events cover session lifecycle, stat levels, assimilation rewards, quest objectives, elite/boss lifecycle, player death, and offline reward generation/claim. Writes are synchronous and ordered because event volume is intentionally low. Writer failures are diagnosed and contained; they cannot roll back gameplay.

Normal non-development players do not compile or compose the writer, observer, command service, or overlay.

## Overlay

In the Editor, toggle the panel with `F1`, backquote, or the `DEV` button. Android Development Builds use the `DEV` button in the upper safe-area corner. The panel refreshes four times per second and shows FPS/frame time, ordinary population, player position and health, five stat levels, total assimilation, elite/boss state, and session counters.

## Commands

- `+1` stat buttons call `PlayerStatsState.SetLevel`, clamp to configured maxima, and mark the profile dirty. These are direct QA mutations, not earned assimilation.
- `+1 Все` applies the same authoritative mutation to all five stats.
- `Открыть элиту` unlocks the authoritative elite gate state.
- `Открыть босса` completes the active Magnetar Guard through its authoritative defeat flow, which updates quest, world, and save invariants together.
- `Сбросить босса` clears boss completion and its quest objective through explicit development boundaries, restores the runtime boss, and does not emit a defeat event or replay rewards.
- `Бессмертие` toggles an explicit damage override. It defaults off and does not heal every frame.
- `Сброс профиля` deletes only `profile.json`, `profile.backup.json`, and `profile.temp.json`, suppresses shutdown re-save, then reloads the active scene. Preserved corrupt diagnostics and unrelated application files are retained.

For a fresh S20 balance run, use `Сброс профиля`; the controlled scene reload creates a new profile through the normal repository path. For repeated Custodian testing, first use `Открыть босса`, then `Сбросить босса` after each completed attempt.

## Safety and limitations

All development implementation files and composition references are guarded by `UNITY_EDITOR || DEVELOPMENT_BUILD`. Save schema version remains unchanged. Telemetry is not profile progression and is never restored into gameplay state.

Manual checks remain required for portrait readability, touch ergonomics, scene reload behavior, repeated boss testing, and Android Development Build access.
