# Chapter 1 Reward Ladder

Status: **Phase 4 design contract**  
This document defines relative reward hierarchy. It deliberately avoids pretending final economy numbers are known before repeat-loop playtesting.

## 1. Existing reward baseline

Current S20 ordinary enemies use the existing core reward system:

- one stat XP tied to the enemy's primary stat;
- one assimilation score;
- first stat level cost remains low enough that early focused farming produces visible growth quickly.

Define this current ordinary kill package as **1R**.

`R` is not a new resource. It is a comparison unit for the value of existing progression rewards.

## 2. Required hierarchy

The authored average value per successful kill must satisfy:

`starting ordinary < strong ordinary near elite < strong ordinary near boss < elite < boss`

The hierarchy must remain visible after considering expected kill time and danger, not only raw reward quantity.

## 3. Reward categories

Phase 4 should prefer these existing reward categories:

1. **Stat XP**
   - permanent;
   - maps naturally to the five existing stat identities.

2. **Assimilation score**
   - permanent;
   - already drives evolution/progression thresholds.

3. **Existing authored equipment**
   - optional milestone/bonus reward;
   - non-stackable ownership semantics already exist;
   - should not be required on every repeat encounter.

Do not add a new generic elite token, boss currency, daily shard, key, or chest currency for the MVP.

## 4. Starting ordinary

### Guaranteed

- current archetype stat XP;
- current assimilation score.

### Optional bonus

- none required.

### Progression impact

Primary baseline for normal permanent growth.

### Repeat reward

Same as guaranteed reward; ordinary enemies are inherently repeatable.

### Relative target

**1R**

## 5. Strong ordinary near elite

### Guaranteed

- same reward categories as ordinary enemies;
- approximately **1.5-2R** total value.

This can be authored as larger stat XP/assimilation packages or as a stronger enemy-specific core reward definition. Avoid introducing reward type branching in combat code.

### Optional bonus

- low-frequency authored existing equipment grant is acceptable later;
- no new currency.

### Progression impact

A meaningful alternative once the player reaches the elite side of the Chapter; should accelerate growth without making starting regions obsolete immediately.

### Repeat reward

Full package on every kill, subject to that spot's ordinary adaptive respawn pressure.

### Relative target

**1.5-2R**

## 6. Strong ordinary near boss

### Guaranteed

- same progression categories;
- approximately **2-3R** total value.

### Optional bonus

- authored equipment can be used sparingly if it supports late-Chapter build choice.

### Progression impact

Late Chapter farming target and best continuously repeatable ordinary content.

### Repeat reward

Full package on every kill, governed by its spot's adaptive respawn profile.

### Relative target

**2-3R**

## 7. Magnetar Guard — first clear

The first defeat is a progression milestone, not merely repeat farming.

### Guaranteed

- one-time elite progression completion;
- boss path unlock;
- a substantial stat XP + assimilation package.

### Optional bonus

- one authored equipment item or one guaranteed item from an existing eligible set, if Phase 4 implementation intentionally adds this content;
- presentation milestone feedback.

### Progression impact

Major Chapter gate.

### Repeat reward

Not applicable to the first-clear transaction.

### Relative target

Target roughly **6-8R** in progression value, subject to real combat duration.

The exact package must feel materially above strong ordinary content but should not trivialize the remaining boss preparation.

## 8. Magnetar Guard — premium repeat

### Guaranteed

- stat XP + assimilation package;
- target roughly **4-6R**.

### Optional bonus

For the first premium repeat reward in a new 24-hour window:

- approximately +20-30% reward value using the same stat XP / assimilation categories.

Avoid claiming an exact percentage as final balance; this is the initial test band.

### Progression impact

Useful repeat growth, but not required for Chapter completion.

### Repeat reward

Premium package while `rewardedKillsInWindow < 3`.

### Relative target

**4-6R**, before first-window bonus.

## 9. Magnetar Guard — capped fallback

### Guaranteed

A small existing-progression package.

Recommended target:

- approximately **1-2R** raw value;
- specifically tune it so elite fallback farming is not more efficient per minute than strong ordinary farming.

### Optional bonus

None.

### Progression impact

The encounter remains worth playing for fun/testing, but its premium economy is exhausted.

### Relative target

**1-2R**, with time-efficiency deliberately below the strong-ordinary route.

## 10. Custodian M-0 — first clear

### Guaranteed

- Chapter 1 completion exactly once;
- substantial stat XP + assimilation package.

### Optional bonus

- one authored existing equipment reward if appropriate;
- Chapter-completion presentation.

### Progression impact

Highest one-time Chapter 1 milestone.

### Repeat reward

Not applicable to first-clear transaction.

### Relative target

Approximately **10-14R** progression value.

The value is intentionally above elite first clear, but the main value is completion/progression, not raw farming throughput.

## 11. Custodian M-0 — premium repeat

### Guaranteed

- premium stat XP + assimilation package;
- approximately **7-10R**.

### Optional bonus

First premium repeat in a new 24-hour boss reward window may receive approximately +20-30% using existing reward categories.

### Progression impact

Highest premium repeat encounter in Chapter 1.

### Repeat reward

Premium package while `rewardedKillsInWindow < 2`.

### Relative target

**7-10R**, before first-window bonus.

## 12. Custodian M-0 — capped fallback

### Guaranteed

A small stat XP + assimilation package.

Recommended:

- approximately **2-3R** raw value;
- tune time-efficiency below late strong ordinary farming.

### Optional bonus

None.

### Progression impact

Allows fight replay without turning the boss into the optimal grind after the premium entitlement is exhausted.

### Relative target

**2-3R**.

## 13. Why raw reward and time-efficiency are separate

A boss can grant 8R and still be a worse grind than a 2R ordinary enemy if:

- boss kill time is much longer;
- boss has a cooldown;
- ordinary enemies can be chained continuously.

Phase 4 balance tests must therefore compare:

- reward per kill;
- expected kill duration;
- traversal;
- cooldown;
- adaptive respawn;
- failure/death risk.

Do not judge the ladder using reward multipliers alone.

## 14. Equipment policy

Equipment is already a persistent existing system, so it is a valid optional reward category.

For Phase 4 MVP:

- do not create random rarity/loot-table infrastructure just to support repeat encounters;
- prefer deterministic authored grants;
- duplicate non-stackable equipment should not become a hidden extra currency;
- if no sensible duplicate behavior exists, equipment should remain first-clear or milestone-only.

## 15. Suggested first implementation shape

The cleanest initial authored shape is:

- normal ordinary: existing `CoreRewardDefinition`;
- strong ordinary: additional/variant `CoreRewardDefinition` values tied to strong enemy/profile content;
- elite/boss encounter reward package: deterministic data object containing stat XP, assimilation, and optional equipment id;
- first-clear/premium/fallback each select an authored package;
- first-window premium bonus composes with the selected premium package.

Exact type/file names are implementation decisions, but reward magnitude must stay data-driven.

## 16. Balance acceptance criteria

A repeat-loop playtest should fail review if any of these are true:

- starting ordinary remains the best late-game farming route;
- strong ordinary makes elite rewards feel negligible;
- elite repeat makes strong ordinary pointless;
- boss capped fallback becomes the best farm;
- waiting for boss cooldown is more attractive than playing ordinary content;
- premium caps make encounters feel disabled or pointless;
- first-clear rewards are so large that the remainder of Chapter 1 collapses.

## Status

**CHAPTER 1 REWARD LADDER: READY FOR IMPLEMENTATION REVIEW**
