# Energy Chamber — final polish and Chapter 1 validation

Owner request: one bounded final production-oriented surface polish, then commit and push. No merge.

Baseline: `535c99c1703bff88aaac3cfcd4e8b9833e3507d2`. Preserve all eleven accepted custom FBXs, their assembly transforms, the central proportions, stepped base, four vertical cyan modules, horizontal machinery and unchanged G-0 scale. Do not add another geometry pass.

Refine contained orange energy, role-specific PBR response, existing interface plates, maintenance markings and localized surface wear. Evaluate actual Chapter 1 meshes, lighting and grading with the production camera settings in an isolated static context scene under `Assets/_Game/ArtProof/EnergyChamber/Scenes/`. Keep the source Chapter scene and all gameplay content unchanged.

Assets remain under `Assets/_Game/ArtProof/EnergyChamber/`; reproducible surface tooling under `Tools/energy-chamber-final-polish-v1/`; evidence and report in this directory. No new donor intake, gameplay components, collision, production scene integration or shared settings changes. Continue the existing branch and Draft PR #77.

Validation: Unity compilation, relevant EditMode/PlayMode checks, project validator, gameplay/G-0/high/close/emission-disabled captures and standalone Android visual-review APK. A desktop static camera capture is not a real-device performance or human visual sign-off.
