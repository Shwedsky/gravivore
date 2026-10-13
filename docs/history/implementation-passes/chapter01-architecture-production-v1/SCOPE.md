# Chapter 01 architecture production pass V1

Initial checkpoint, 2026-10-13. Base: `e1669362fd3566f69939b547f263a3a600e3e1b7` (freshly fetched origin/main; unchanged from task preparation).

Accepted source: PR #77, `7d0e375f7b0a12a9d46982af685cabe3ce57d731`. The accepted Energy Chamber presentation is frozen. Promote its runtime dependency closure into Content with original Unity GUIDs; retain donor provenance and editable source custody. Exclude proof scenes, cameras, test helpers, evidence and proof Editor tooling from runtime content. Preserve near presentation, add explicit compound collision and measure render/triangle/material cost before choosing distance simplification.

Integrate a clean production landmark into `Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity` using accepted Chapter-context placement. Inspect active branch divergence before scene modification; edit only environment-owned objects. Validate real gameplay-camera scale, clearance, occlusion, contact, emissive hierarchy and G-0 traversal.

Audit all current large facilities, gates, walls, transitions, arena infrastructure and major machinery from the actual Chapter camera. Publish `docs/CHAPTER01_ARCHITECTURE_LANGUAGE_V1.md` and `docs/CHAPTER01_ARCHITECTURE_AUDIT_V1.md`, classify KEEP / REFINE / REBUILD and rank the next isolated replacement. No full Chapter rebuild; at most one additional isolated proof if justified.

Ownership: environment architecture, production landmark prefabs, architectural materials/collision, necessary dressing and visual scene integration. Enemy/elite/boss logic, combat, progression, persistence, loot and player gameplay code are outside scope. Actor- and boss-owned architecture will be deferred when coupled to active work.

Validation: compile, relevant EditMode/PlayMode tests, existing project validation, Dev Android build if Unity CLI is available, actual Chapter screenshots and reproducible collision evidence. Report limitations without substituting beauty shots or claiming device acceptance. Commit and push each major checkpoint. Keep the PR draft and unmerged.
