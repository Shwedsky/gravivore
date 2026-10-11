# Chapter 01 world R2 presentation convergence

The owner's R1 device review rejects visual acceptance while retaining the technical topology. R2 keeps the same branch and Draft PR #71. Frozen reference: `67236348a2be8732f5fbd88eb2093da18f5f7251`.

`prepare_surfaces.py` derives darker native trim surfaces from the unchanged CC0 raw sources and authors sixteen purpose-specific machinery/floor surface cells. Panel joints, machining, grates, paint and wear follow hardware. Sector identity is not random texture noise.

`author_world.py` reuses the licensed R1 donor/import/export code without editing its layout or source. It replaces presentation in the existing BlueprintWorldR1 runtime asset paths; this is not another environment overlay. Changes include irregular replaceable deck cassettes, flush service grates, machinery-specific work zones, supported overhead edge machinery, open service-bay roofs, pressure-chamber framing and sector-specific contained energy. The former continuous top plates no longer cover sector floors. Overhead edge hardware stays above the player; collider authority is unchanged.

The editable R2 source is `art/blueprint-world-r2/Chapter01_BlueprintWorld_R2.blend`. The R1 source is retained for historical evidence. Both derive from the canonical owner-local ZIPs, which are never edited.

`run_unity.ps1 -Action Author` executes `Chapter01BlueprintWorldBuilder.BuildR2`, which updates materials/prefabs/environment/light only. It deliberately skips layout creation and encounter remapping. The same wrapper runs compilation, validation, audit and test suites. Sixteen static unshadowed local light pools supplement one directional key; each pool has a bounded range, and URP retains its four-additional-lights-per-object limit.

Before building, run the eight-sector actual-camera capture test and inspect both ordinary captures and `internal/no-ui/`. Those images use the real follow camera and gameplay occupants; only Canvas visibility changes for the no-UI copy. There are no beauty-render camera changes. Reject an iteration when floor fields, generic kits or cropped featureless masses still carry the image.

`verify_preservation.py` freezes runtime/gameplay/persistence/minimap/content and the R1 layout/definitions against the accepted technical baseline. Run it after restoring incidental Unity import serialization in protected files. `verify_owner_originals.py --capture` records the two ZIPs and APK/metadata 47 and 48 once; later calls verify preservation without replacing the baseline.

Next Android candidate is versionCode 49, ARM64 IL2CPP DEV. Keep APK 47/48 and PR Draft. Device concept fidelity and 60 FPS still require owner review; Editor timings are not device measurements.
