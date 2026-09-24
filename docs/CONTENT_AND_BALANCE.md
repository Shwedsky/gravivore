# Vertical Slice Content and Balance Skeleton

These are starting values for playtesting, not sacred final balance.

## Starting player

- Max HP: 100
- Power: 10
- Armor: 0
- Attack interval: 1.15 s
- Move speed: 4.2 world units/s
- Acquisition radius: 4.5
- Release radius: 5.2

## Core stats

Each stat begins at Level 1.

Suggested effect per level:
- Power: +6–9% additive base contribution early.
- Hull: +7–10 max HP early.
- Armor: gradual flat armor value.
- Flux: reduce interval with a hard minimum.
- Mobility: modest speed increase with cap.

Exact curves belong in ScriptableObjects and are tuned from device playtests.

## Enemy spot order

The player can walk to all spots, but difficulty creates a soft order.

1. Relay Yard / Scout — easiest.
2. Cutting Floor / Cutter.
3. Shield Dump / Warden.
4. Capacitor Field / Arc Drone.
5. Hauler Graveyard / Carrier.

Do not hard-lock all spots behind sequential doors. Let curiosity create controlled risk.

## Population

Per ordinary spot:
- desired live: 4;
- supported authoring range: 3–5;
- spawn anchors: at least 6 so respawn position can vary;
- respawn delay: around 8–14 s with jitter.

Global ordinary cap: 25.

## Reward identity

- Scout -> Mobility Core.
- Cutter -> Power Core.
- Warden -> Armor Core.
- Arc Drone -> Flux Core.
- Carrier -> Hull Core.

Every first kill gives a larger tutorial-visible bump.
Subsequent kills follow the configured curve.

## Suggested first-session pacing

0–3 min:
- movement;
- first prey;
- first stat increase.

3–10 min:
- two or three spots understood;
- first visible evolution.

10–25 min:
- route optimization;
- harder spots become farmable;
- elite unlock.

25–40 min:
- elite;
- second evolution;
- boss preparation.

30–45 min:
- boss kill and completion.

If a competent tester cannot reach the boss inside 45 minutes on the first balance pass, reduce grind before adding shortcuts.

## Boss principle

Boss should be mathematically very difficult when encountered far too early.
At intended progression:
- basic hits survivable;
- standing in every telegraph leads to death;
- dodging most telegraphs produces a comfortable win.

This achieves the 80% stat / 20% movement target.
