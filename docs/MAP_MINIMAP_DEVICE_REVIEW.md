# GRAVIVORE — Map / Minimap Device Review

Status: future human review checklist. No Unity implementation in this PR.

Use this after the map/minimap implementation exists and after the current visual-world integration baseline is available.

## Test matrix

Review at minimum:

- smallest supported portrait resolution/aspect available to the team;
- one representative mid-range Android phone;
- one tall/narrow portrait device;
- development build with DEV overlay enabled;
- release-like UI with DEV overlay disabled.

## 1. Safe area and placement

- [ ] Compact minimap remains inside platform safe area.
- [ ] Menu/pause target remains independently tappable.
- [ ] Player HP does not overlap the map.
- [ ] Boss HP does not overlap the map during encounter start, damage updates or phase changes.
- [ ] Upper-right layout does not force critical HUD elements below a comfortable glance zone.
- [ ] Lower movement/joystick region remains completely clear.

## 2. One-thumb usability

- [ ] Player can move continuously with one thumb while glancing at the minimap.
- [ ] Accidental map opening does not occur from normal movement.
- [ ] Compact map has a forgiving tap target.
- [ ] Opening expanded map is a single deliberate tap.
- [ ] Closing expanded map is obvious and does not require a precision target.

## 3. Player orientation

- [ ] Player chevron is identifiable in under one second.
- [ ] Heading remains readable while moving diagonally.
- [ ] Stationary heading does not jitter.
- [ ] Near chapter bounds, crop clamping never clips the player marker.
- [ ] The fixed map orientation feels consistent with the fixed gameplay camera.

## 4. POI readability

Without reading labels, verify the tester can distinguish:

- [ ] repair hub;
- [ ] ordinary farming spot;
- [ ] stronger ordinary spot;
- [ ] elite;
- [ ] boss;
- [ ] gate;
- [ ] player.

Then verify:

- [ ] elite and boss are not confused;
- [ ] stronger spot still reads as a farming spot, not a new rarity category;
- [ ] hostile danger does not rely only on red/orange;
- [ ] lock does not rely only on dim color;
- [ ] cooldown does not rely only on a color change.

## 5. State transitions

Trigger or simulate:

- [ ] ordinary available -> combat active;
- [ ] ordinary active -> pending respawn;
- [ ] ordinary respawn -> recovered;
- [ ] elite locked -> unlocked;
- [ ] elite available -> cooldown;
- [ ] elite cooldown -> repeat available;
- [ ] boss locked -> first encounter available;
- [ ] boss engaged;
- [ ] boss defeated / chapter completed;
- [ ] boss cooldown -> repeat available;
- [ ] full reward -> capped/fallback reward, if #40 implements it.

For each transition:

- [ ] icon does not disappear unexpectedly;
- [ ] no uncontrolled flashing;
- [ ] state is understandable without opening details where MVP requires it;
- [ ] reward state never makes a fightable encounter look locked.

## 6. Timers

- [ ] No exact timer text is present on compact minimap.
- [ ] Elite/boss rounded timer is readable on expanded map.
- [ ] Selected exact timer is readable at arm's length.
- [ ] Timer never shows negative values.
- [ ] At zero, state transitions cleanly to available.
- [ ] Timer text does not visibly update more often than needed.
- [ ] No timer layout jitter occurs as digit count changes.

## 7. Expanded map

- [ ] Full Chapter 1 fits at a useful scale.
- [ ] POIs remain tappable without zoom.
- [ ] Player position is obvious globally.
- [ ] Selection highlight is clear.
- [ ] Details panel does not cover the selected POI unnecessarily.
- [ ] Background tap clears selection.
- [ ] No route planner/path line is accidentally implied by selection.
- [ ] Labels do not overlap to the point of hiding icons.

If the full chapter cannot fit cleanly on the smallest supported device, record the evidence before adding panning/zooming.

## 8. Boss encounter coexistence

- [ ] Boss HP remains the primary combat status element.
- [ ] Boss marker is visually dominant but not distracting.
- [ ] Nearby ordinary POIs do not compete with boss telegraphs.
- [ ] Opening the expanded map during an active fight follows the intended pause/non-pause game rule.
- [ ] Map does not cover critical boss warning UI.

## 9. Modal behavior

- [ ] Pause menu hides or disables compact map consistently.
- [ ] Expanded map cannot stay active under another blocking modal.
- [ ] Chapter-complete panel owns focus when shown.
- [ ] Offline-reward/return summary does not create layered map interaction.
- [ ] DEV modal/debug windows do not leak into release UI behavior.

## 10. Contrast and visual-world integration

Review in:

- [ ] darkest Chapter 1 region;
- [ ] brightest/most cyan region;
- [ ] heavy red/orange hostile encounter;
- [ ] visually dense Phase 3C area.

Verify:

- [ ] controlled map background preserves icon contrast;
- [ ] no environment color makes player marker disappear;
- [ ] no Phase 3C prop/prefab movement changes map marker positions;
- [ ] repair hub map position follows gameplay authority, not art pivot;
- [ ] elite/boss map positions follow encounter authority, not renderer bounds.

## 11. Accessibility / reduced motion

- [ ] Fixed map causes no rotation discomfort.
- [ ] State-entry pulse can be disabled/reduced.
- [ ] No perpetual spinning radar treatment exists.
- [ ] Cooldown ring can be understood without animation.
- [ ] Color-blind review confirms shape/border redundancy.

## 12. Performance spot-check

During normal gameplay with compact map visible:

- [ ] no per-marker `Update` pattern appears in profiler;
- [ ] no continuous string allocations from timers;
- [ ] no layout rebuild storm while player moves;
- [ ] marker view count stays stable/reused;
- [ ] no second minimap camera/render texture is active;
- [ ] map cost remains a presentation/UI cost, not an extra world-render pass.

With expanded map open:

- [ ] opening/closing does not create a visible hitch;
- [ ] repeated open/close does not grow allocations or marker instances;
- [ ] timer updates do not trigger full hierarchy rebuilds.

## 13. Release acceptance

Device review passes when:

- all critical HUD overlap items pass;
- player and major POI kinds are identifiable without labels;
- lock/available/cooldown are distinguishable without color alone;
- compact map supports navigation without text clutter;
- expanded map exposes timed/reward detail without becoming a second game screen;
- no runtime marker derives authority from visual art hierarchy;
- no material performance regression is observed on the representative mid-range Android device.

Record device, resolution, build commit, #40 state-contract version and Phase 3C visual baseline commit with the review.
