# S19 Manual Device Checklist

Use the S19 Dev and Candidate APKs on a physical Android device. Record device
model, Android version, display resolution/aspect ratio, APK filename, start/end
time, observed FPS range, and every failure with reproduction steps. Do not mark
an item complete from Editor or emulator evidence.

## 1. DEV APK

- [ ] Install the Dev APK and launch in portrait.
- [ ] Confirm the DEV button is visible.
- [ ] Confirm Pause and DEV do not overlap and both remain inside the safe area.
- [ ] Open the overlay and confirm it is readable.
- [ ] Run `+1 stat` and verify exactly one selected-stat increase.
- [ ] Run `+1 all` and verify exactly one increase for every stat.
- [ ] Unlock elite and verify the canonical prerequisite state and encounter.
- [ ] Unlock boss and verify the boss encounter becomes available.
- [ ] Reset boss and verify it becomes testable without a defeat/reward replay.
- [ ] Toggle god mode on and off; damage must be blocked only while enabled.
- [ ] Reset profile, relaunch, and verify a genuinely fresh profile starts.

## 2. CANDIDATE APK

- [ ] Install and launch the Candidate APK.
- [ ] Confirm no DEV button, dev overlay, or visible cheat path exists.
- [ ] Confirm normal movement, combat, progression, pause, and persistence work.

## 3. UI

- [ ] Check a 9:16-ish portrait layout.
- [ ] Check a tall approximately 20:9 portrait layout.
- [ ] Confirm no clipped Cyrillic or square/missing glyphs.
- [ ] Confirm HUD and controls remain inside the safe area.
- [ ] Confirm Pause opens, resumes, and does not leave gameplay frozen.

## 4. GAMEPLAY

- [ ] Verify movement and auto-target selection.
- [ ] Verify gravity attack and ordinary mob damage/death.
- [ ] Verify ordinary respawns and pooled reuse.
- [ ] Verify equipment grant/equip effects.
- [ ] Verify progression and evolution presentation.
- [ ] Verify elite gate, encounter, defeat, and persistence.
- [ ] Verify boss gate, encounter, phases, defeat, and completion.

## 5. DEFERRED BOSS BAR CHECK

- [ ] Boss health numeric value decreases after damage.
- [ ] Boss red bar visibly decreases after damage.
- [ ] Player red bar visibly decreases after damage.

## 6. BOSS ARENA RESET

- [ ] Leave for less than 3 seconds and re-enter: boss does not reset.
- [ ] Leave for more than 3 seconds: boss fully heals and returns to start.
- [ ] Player death resets the boss as intended.

## 7. PLAYER REGEN

- [ ] Confirm no regeneration while in combat.
- [ ] Approximately 3 seconds after combat, confirm normal regeneration starts.
- [ ] Confirm a successful attack keeps the combat timer active.
- [ ] Near the respawn basin while out of combat, confirm approximately 10x regeneration.

## 8. SAVE / RESTORE

- [ ] Gain levels and equipment, fully close the app, and relaunch.
- [ ] Confirm stats and equipment persist without modifier stacking.
- [ ] Confirm normal enemies respawn.
- [ ] Confirm a defeated elite stays defeated.
- [ ] Confirm a defeated boss stays defeated.
- [ ] Confirm the chapter completion modal does not reopen merely from restore.

## 9. BOSS DEV RETEST

- [ ] Use DEV reset boss and confirm the boss becomes testable again.
- [ ] Kill the reset boss and confirm normal completion.
- [ ] Confirm no duplicate or inconsistent reward/state.
- [ ] Reset again and confirm the cycle remains repeatable.

## 10. AUDIO / HAPTICS

- [ ] Verify lash, hit, death, assimilation, evolution, telegraph, and boss-impact audio.
- [ ] Confirm the sound setting persists after relaunch.
- [ ] Confirm the haptic toggle persists after relaunch.
- [ ] Confirm no obvious duplicate haptic on elite/boss hit.

## 11. LIFECYCLE

- [ ] Background the app for 5-10 seconds during ordinary play, then return.
- [ ] Background during combat, then return.
- [ ] Open Pause, background, then return.
- [ ] Lock and unlock the phone while the app is active.
- [ ] Force-stop and relaunch.
- [ ] After every case, confirm controls, time scale, audio, save, and encounter state remain valid.

## 12. OFFLINE

- [ ] Leave long enough to exceed the configured minimum return absence.
- [ ] Confirm the reward summary appears appropriately.
- [ ] Claim once and confirm the balance changes once.
- [ ] Confirm the same reward cannot be claimed twice.

## 13. MAX POPULATION / PERFORMANCE

- [ ] Play continuously for at least 15 minutes.
- [ ] Visit all ordinary spawn spots and reach high enemy population.
- [ ] Record approximate FPS/frame time and live enemy count from the DEV overlay.
- [ ] Confirm no obvious escalating stutter or recurring GC-like hitch pattern.
- [ ] Confirm no runaway duplicate visuals or increasingly duplicated audio.
- [ ] Confirm no severe heat/performance collapse; record thermal observations.

## 14. VISUAL ASSETS

- [ ] Inspect the player model.
- [ ] Inspect Cutter and Carrier multi-mesh visuals.
- [ ] Inspect all five ordinary archetypes and their distinct silhouettes.
- [ ] Inspect landmarks.
- [ ] Confirm no magenta materials or missing meshes.
- [ ] Confirm no stray decorative collision geometry affects gameplay.
- [ ] Confirm no visual leftovers after pooled enemy respawn.

## Result

S19 PHYSICAL DEVICE / 15-MINUTE HARDENING CHECK: PENDING
