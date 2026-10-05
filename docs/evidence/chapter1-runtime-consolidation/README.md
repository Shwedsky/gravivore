# Chapter 1 consolidated runtime evidence

These ten PNGs were rendered by Unity 6000.3.0f1 from the actual production
`Chapter01_ScrapExclusion` scene, using `Chapter1ConsolidatedRuntimeSmokeTests`
with `GRAVIVORE_CAPTURE_EVIDENCE=1`. They are automated, staged runtime captures,
not device screenshots or replacement art. Gameplay actors use their real
controllers, authored assets, production composition, map, and bounded Phase6B pools.

The capture helper positions the real camera for a portrait 540 x 960 frame and
temporarily renders the real HUD canvas through that camera. Deterministic test
time advances cooldowns; tests move the player and apply authoritative damage.
Boss warning captures wait for an engine frame after enabling the real geometry.

- [01: Compact minimap](01_chapter_minimap.png)
- [02: Expanded map and strong marker](02_expanded_map.png)
- [03: Player Gravity Lash attack](03_player_attack.png)
- [04: Ordinary enemy death](04_enemy_death.png)
- [05: Strong ordinary pack](05_strong_ordinary.png)
- [06: Magnetar encounter](06_magnetar.png)
- [07: Custodian cone](07_boss_cone.png)
- [08: Custodian line](08_boss_line.png)
- [09: Custodian circle](09_boss_circle.png)
- [10: Authoritative Repair Hub healing and presentation](10_repair_hub.png)

Physical-device audio balance, visual readability, touch interaction, sustained
60 FPS, thermal behavior, and Android process-kill recovery remain device review
work. The captures do not establish those properties.
