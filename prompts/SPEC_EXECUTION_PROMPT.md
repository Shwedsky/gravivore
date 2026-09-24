# Reusable Codex prompt for one specification

Implement specification: `<SPEC_ID>`.

Read `AGENTS.md` and all prerequisite project documents first. Inspect existing implementation before editing.

Constraints:
- scope is only `<SPEC_ID>` plus the minimum prerequisites already defined by earlier specs;
- do not redesign established architecture without a demonstrated blocker;
- do not add paid packages/assets;
- do not add future monetization/backend/multiplayer functionality unless this spec explicitly asks for an interface;
- preserve save compatibility;
- add/update tests for deterministic behavior;
- if Unity CLI is available, run relevant tests/validation and Android build when this spec affects the candidate build;
- if execution tooling is unavailable, report that fact and still perform static consistency checks.

Do not ask for routine product decisions. Use the frozen defaults from `docs/DECISIONS.md`. If you must choose a reversible technical detail, choose the simplest option and record it.

Finish with the standard completion report from `AGENTS.md`.
