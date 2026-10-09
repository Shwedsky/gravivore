# Scratch URP configuration

Owned Unity-generated configuration copied from the local GRAVIVORE URP 17.3.0
baseline on 2026-10-09. Contains render settings/resource references only, no
third-party intake content or executable code. The intake preparer copies these
files into the ignored scratch project; production Assets/ and ProjectSettings/
are never written. Source camera/gameplay settings are not copied.

These files are required because GraphicsSettings/global URP resources are not
tracked in the merged production checkout. The zoo overrides exposure, MSAA,
render scale and lighting for neutral inspection, with no post-processing.
