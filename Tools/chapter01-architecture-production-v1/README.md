# Chapter 01 architecture production V1

`promote.py` is the initial pinned-source promotion only. It copies the accepted runtime dependency closure with original GUIDs and retains editable sources/provenance. Do not rerun it over authored production collision/LODs.

`run_unity.ps1 -Action Author` authors the production collision/LODs and Chapter instance. `Lod` applies distance thresholds, layer, placement and explicit collision binding without rebuilding the art. After authoring, `finalize_scene.py` restores all existing scene fields and retains only the production instance/root and external collision binding. `verify_delivery.py` verifies immutable source assets, GUIDs, preserved Chapter fields and excluded gameplay/actor scopes.

Validation actions: `Validate`, `EditMode`, `PlayMode`, `Capture` and `Android`. Optional `TestFilter` and `Label` keep distinct test receipts. `Capture` creates static Chapter architecture inspection views, while `Chapter01ArchitectureProductionSmokeTests` produces actual playable Chapter captures, map collision assertions and controller/locomotion traversal receipts.

Run from this isolated checkout with Unity 6000.3.0f1. Runtime content references neither proof scenes nor proof tooling. Generated logs/APKs/caches remain local; result XML/JSON and final screenshots are delivery evidence.

If the installed SDK's remote metadata query stalls, `prepare_offline_sdk.py` wraps package listing in an already copied, ignored workspace SDK mirror. It uses the official installed-only query and leaves the original SDK unchanged. `AndroidOffline -SdkRoot <mirror>` selects that SDK for the unchanged Dev build and restores the previous SDK selection. See the Android build note; no packages or fake version records are introduced.
