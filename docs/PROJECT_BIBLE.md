# Project Bible

## High concept

GRAVIVORE is a portrait one-thumb mobile action/idle RPG about **assimilation**.

The player is not a hero with a sword. It is a growing machine-organism. Its central gravity core identifies nearby prey, lashes onto a viable target, drags light units inward, dismantles them and absorbs a core fragment. Each enemy family grows a different aspect of the player.

The promise to the player is:

> Everything useful you destroy becomes part of you.

## Differentiators

### 1. Visible build evolution

Permanent growth changes the model, not just numbers.

At progression thresholds, modular parts appear:
- armor plates for Hull/Armor leaning;
- blade/mandible modules for Power leaning;
- thruster fins for Mobility leaning;
- brighter/faster rotating core rings for Flux leaning;
- larger gravitational emitter geometry for pull/range leaning.

v0.1 needs at least two clear body changes that a tester can notice without opening a stat menu.

### 2. No empty-world punishment

The game must not create long periods where the useful response is to close the app and wait for ordinary enemies.

Rare events may have timers later. Ordinary progression prey in the vertical slice respawns quickly enough that another useful target is always available.

### 3. Simple controls, meaningful target choice

The player only needs movement for the basic loop.

Depth comes from:
- deciding which stat spot to farm;
- judging whether a stronger enemy is currently beatable;
- positioning to choose which enemy auto-targeting acquires;
- dodging elite/boss telegraphs.

### 4. Enemy role readability

A player should infer what a mob improves by silhouette/color/VFX before memorizing menus.

Examples:
- Scout -> Mobility core.
- Cutter -> Power core.
- Warden -> Armor core.
- Drone -> Flux/attack cadence core.
- Carrier -> Hull core.

## Narrative wrapper

A research complex attempted to manufacture self-repairing autonomous organisms around compressed gravity cores. Prototype G-0 escaped into an industrial exclusion zone after a containment collapse.

The zone's machines are governed by a central Custodian AI. G-0 does not "loot" in a conventional sense: it **integrates compatible modules**.

This supports endless progression without narrative absurdity: the creature is designed to recursively rebuild itself.

## Chapter 1 — The Scrap Exclusion

Purpose: vertical slice.

Spatial structure:
- compact hub/repair basin in center;
- five radial or loop-connected farming spots;
- elite gate on the outer loop;
- boss arena beyond the elite gate.

Player should be able to see at least two future destinations from earlier paths. Avoid corridor-only design.

## Enemy spots

Default population: 4 live mobs per spot.
Configuration must support 3–5.

### Spot A — Relay Yard
Enemy: Scout Drone  
Reward bias: Mobility  
Behavior: circles/retreats slightly, low HP.

### Spot B — Cutting Floor
Enemy: Cutter Unit  
Reward bias: Power  
Behavior: short melee burst, medium HP.

### Spot C — Shield Dump
Enemy: Warden  
Reward bias: Armor  
Behavior: slow, durable, frontal mitigation.

### Spot D — Capacitor Field
Enemy: Arc Drone  
Reward bias: Flux  
Behavior: light ranged poke, fragile.

### Spot E — Hauler Graveyard
Enemy: Carrier  
Reward bias: Hull  
Behavior: high HP, slow heavy swing.

### Elite — Magnetar Guard
Checks that the player has not invested in only one stat.
Reduced gravity displacement.
Telegraphed shockwave.

### Boss — Custodian M-0
No gravity displacement.
Three simple attacks:
1. slow circular ground pulse;
2. frontal cone sweep;
3. charge/line attack.

At low HP, cadence increases but mechanics do not multiply dramatically.

## Progression fantasy

Early: fragile core on legs / small chassis.  
Mid-slice: first visible external modules.  
Late-slice: more aggressive silhouette and stronger gravity VFX.  
Boss kill: a unique "Custodian Core" unlocks a cosmetic/mechanical evolution marker and completion screen.

## Not in v0.1

- PvP.
- clans.
- chat.
- multiple chapters.
- prestige.
- live events.
- ads.
- purchases.
- subscriptions.
- cloud accounts.
- backend.
- complex crafting.
- gacha.
- battle pass.
- manual active-skill bar.
