# GRAVIVORE — Map / Minimap Wireframes

Documentation-only sketches. Not final art and not pixel-perfect coordinates.

## 1. Normal gameplay

```text
┌───────────────────────────────┐
│ HP ███████████      [≡]       │
│                    ┌────────┐ │
│                    │   ◇ E  │ │
│                    │ ·  ▲   │ │
│                    │ H    · │ │
│                    └────────┘ │
│                               │
│          GAME WORLD           │
│                               │
│                               │
│                               │
│       (floating joystick)     │
└───────────────────────────────┘

▲ player
H repair hub
· ordinary spot
◇ elite
```

No text labels inside the compact map.

## 2. Expanded Chapter map

```text
┌───────────────────────────────┐
│ MAP                      [X]  │
│                               │
│   [Repair Hub]                 │
│        H                       │
│        │       · Relay         │
│   · Cutting       · Shield     │
│        ▲ Player                │
│           ⟦·⟧ Strong spot      │
│                 ◇ Elite        │
│                 ║ Gate         │
│                     ⬢ Boss     │
│                               │
│ ┌───────────────────────────┐ │
│ │ Selected: Elite           │ │
│ │ State: Cooldown           │ │
│ │ Available in: 12m         │ │
│ └───────────────────────────┘ │
└───────────────────────────────┘
```

The chapter should fit without requiring route planning or a quest journal.

## 3. Elite cooldown

```text
Compact:
      ◇
    ◔   outer ring = cooldown progress
(no timer text)

Expanded:
   ◇  12m

Selected:
Elite
Cooldown
Available in 12:34
Reward: [state from #40]
```

Lock and reward state are separate overlays from cooldown.

## 4. Boss available

```text
Compact:
     ⬢
 heavy boss silhouette
 no permanent pulse

Expanded:
     ⬢  Custodian M-0
     AVAILABLE

Selected:
Custodian M-0
Encounter: Available
Progression: Unlocked
Reward: First clear / repeat state
```

## 5. Ordinary spot respawn

```text
Available:        Pending respawn:       Recovered:
    ●                    ○◔                  ●

Compact: ring only.
Expanded: ring only by default.
Selected: exact short timer may appear.
```

Adaptive pressure is not represented as another compact badge.

## 6. Locked boss

```text
Compact:
    ⬢̸     heavy boss family + lock/bar overlay

Expanded:
    ⬢  Custodian M-0
       LOCKED
```

The structural POI remains known; MVP has no fog of war.

## 7. Stronger ordinary spot

```text
Ordinary:      Stronger ordinary:
   ●                 ⟦●⟧
```

Same family, one tier/danger distinction. No rarity rainbow.

## 8. Modal behavior

```text
Gameplay HUD       Expanded map         Pause/modal
┌───────────┐      ┌─────────────┐      ┌─────────────┐
│ mini-map  │ tap→ │ full map    │      │   PAUSED    │
│ visible   │      │ interactive │      │ map hidden/ │
└───────────┘      └─────────────┘      │ disabled    │
                                        └─────────────┘
```

Only one blocking surface owns interaction at a time.
