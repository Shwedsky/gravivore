# Chapter 01 original production assets

All seventeen meshes, rigid rigs, five in-place animation states and the 256px
industrial atlas are original project-created assets. No third-party geometry,
texture, sample, paid tool dependency or reference-game content was imported.

Blender sources are retained here. `Tools/chapter01-production/build_assets.py`
uses the accepted first-slice chamfer, palette and export helpers without running
their actor/environment builds. Unity integration is reproducible with
`Tools/chapter01-production/run_unity.ps1 -Action Integrate`.

Arc Drone uses enclosed lift turbines and electrodes; Carrier uses wheel bogies,
forks and cargo vessels; Warden uses a convex shield and hydraulic legs;
Custodian uses a six-legged containment hull, pressure drum and articulated
restraint clamps. Each has three rigid-skinned LODs and one opaque atlas material.
The static kit uses the same material and has no colliders, gameplay components,
lights or object-specific Update methods. Separate gameplay proxies are authored
by the integration builder; the service arch keeps its doorway open.

Accepted Scout/Cutter/Magnetar, G-0 and Environment_Slice source assets are retained
unchanged. Their kit extends the new chapter's decks and service infrastructure.
