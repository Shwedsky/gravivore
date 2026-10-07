# Chapter 01 full production checkpoint

Accepted authority: origin/main 0152ec421eed53cc64c0b07fb10c4496589ae047,
merged and device-approved PR #61. No gameplay balance or save changes.

This isolated production branch covers all five zone environments and traversal,
remaining enemy and Custodian presentation, authoritative boss health HUD,
mechanical footsteps, Russian Characteristics and assimilation communication,
full-chapter reachability, full Unity regressions and a verified ARM64 DEV APK.

The only next human review gate is installation and play of the complete APK.
Intermediate art is verified internally. This checkpoint records scope only;
it does not claim that production work or validation is complete.

Draft PR: https://github.com/Shwedsky/gravivore/pull/62.

Production integration checkpoint: all five nested zone packages, continuous facility
deck and service infrastructure, complete containment approach/arena, three ordinary
machine families and Custodian are wired to the canonical scene. Accepted Scout,
Cutter, Magnetar, G-0 and containment-slice assets are unchanged.

Internal 540x960 rendering identified sparse camera framing around ordinary zones;
landmarks were moved within their presentation packages toward the camera-visible
edges and approved deck modules extend the combat aprons. Authoritative spawn,
objective, encounter and gate coordinates are unchanged. Every solid new prop has
a separately authored gameplay proxy; the raised arch uses posts and truss proxies
so its visible doorway stays open.

Focused PlayMode regressions passed boss full/half/low/reset/repeat/death, reward and
equipment/live/save Characteristics updates, locked/unlocked capsule routes and
actual CharacterController traversal. Measured 60-second runs at 4.5, 6 and 7.5 m/s
produce about 80% of v38's step count. Gain is 70% of v38; changing the volume setting
now preserves the individual cue gain, including an already playing step voice.

The first full EditMode run exposed obsolete Phase3D binding expectations and a
new fixture expecting an unclamped XP fraction. Those assertions now require the
new production bindings, retain art/gameplay isolation checks and match the UI's
bounded bar semantics. Full suites and APK validation remain pending at this point.

Full suites subsequently passed: 431 EditMode and 114 PlayMode, zero failures;
one baseline opt-in structural capture is skipped. The first ARM64 DEV build and
signature/content verification passed, including serialized gain .168, minimum
step interval .36 and cycle distance 2.25. Final boundary review then identified
short visible gaps backed by pre-existing gate/perimeter collision. Wall modules
now cover those flanks and the complete north boundary; a sampled wall-coverage
regression passes alongside all actual controller routes. The final APK is rebuilt
from the boundary-complete checkpoint rather than delivering the earlier artifact.
