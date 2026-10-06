# Scout — final donor decision

**ONE preferred donor: Spider Robot — Muhammad Hasan Alasady. Grade B donor.**

Use `spider-robot.zip`, SHA256 in the source register, `source/spider.blend`, mesh `Body`, armature `Spider` (33 bones). The four-support chain and source Idle/Walk are the useful foundation. Real source action samples are saved as `Scout_animation_*.png` and their bone matrices in `data/scout_animation_samples.json`. This is a source-motion sample, not validation after redesign.

1,962 base triangles become 7,848 evaluated with subdivision. Preserve the economy of the base cage; do not accidentally export uncontrolled subdivisions. UVMap exists. Four non-manifold edges and 14 degenerate faces need repair. The material references missing `ImphenziaPalette01.png`; supplied `textures/Colors_Map.png` is present but not assumed equivalent. Replace the palette with the shared hostile material family.

Custom work remains: replace top shell, forward weapon and core housing; remove broad blade/gun identity; preserve fast radial stance and make compact red core visible. Idle and Walk are B useful; Fire is a timing reference, BigBoom is a destruction reference, `_Default` is not a useful clip. Rebind and validate all clips after redesign. Final target follows #53: 6–14k triangles, two to three materials, <=4 renderers where articulation permits.

PAndras's Robot spider: B parts donor but no rig/clips; six-legged gun silhouette and 18,754 triangles lose to the animated economy donor. Keralt: C donor / Reject body; upright box body, no clips, 150 non-manifold edges and six material slots. Neither is an additional preferred Scout donor. No Scout production model is completed in this task.
