# S13 Manual UI Checklist

Run this checklist in Unity 6000.3.0f1 Game view and on a portrait Android device. Use both 1080x1920 (9:16) and approximately 1080x2400 (20:9).

## Core HUD

- [ ] Player HP text and bar are readable, remain inside the safe area, and update after damage and respawn.
- [ ] Power, Hull, Armor, Flux, and Mobility levels are visible without opening a menu.
- [ ] A stat level increase shows readable feedback without hiding combat.
- [ ] The active objective and X/Y or assimilation gate progress are readable.
- [ ] The world objective marker still indicates the current target.
- [ ] The pause button is fully inside the safe area and has a comfortable touch target.
- [ ] The floating joystick remains usable across the lower gameplay region.
- [ ] Touching any visible button or modal panel does not move the player.

## Modal Flows

- [ ] Pause stops combat and movement; Resume restores both without stale joystick input.
- [ ] Compact/expanded stat details persist after a full app restart.
- [ ] A meaningful offline return opens one reward panel with duration and pending amount.
- [ ] Claim transfers the pending amount once, closes the panel, and a repeated tap grants nothing.
- [ ] Boss defeat opens the vertical-slice completion panel with the current five-stat summary.
- [ ] Continue Exploring closes completion and restores gameplay input.
- [ ] A restored completed save has coherent completion UI and does not reactivate the defeated boss.

## Boss And Readability

- [ ] Boss HP is hidden before engagement, visible during the encounter, and updates after damage.
- [ ] Boss HP hides after arena reset, player death, and boss defeat.
- [ ] Boss and elite telegraphs remain clearly visible beneath the HUD.
- [ ] No text is clipped at 9:16 or 20:9, including the longest objective and expanded stats.
- [ ] Critical state is communicated with text as well as color.
- [ ] Android status and navigation areas do not overlap HP, objectives, pause, joystick, or modal actions.
