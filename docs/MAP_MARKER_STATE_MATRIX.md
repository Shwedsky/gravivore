# GRAVIVORE — Map Marker State Matrix

Status: documentation/read-model contract only.  
Companion: `docs/MAP_MINIMAP_UX_SPEC.md`.

The matrix separates **progression access**, **encounter availability**, and **reward state** so future #40 semantics can change without redesigning map icons.

## Legend

- **Base shape** communicates marker kind.
- **Fill / hollow state** communicates immediate availability.
- **Outer ring** communicates timed recovery/cooldown.
- **Lock/bar overlay** communicates progression access.
- **Small reward pip** communicates repeat reward state where applicable.
- Color reinforces state but is never the sole discriminator.
- Numeric timers are never required on the compact minimap.

## Player

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Moving | filled chevron, heading visible | true chapter position + heading | none |
| Stationary | filled chevron, last meaningful heading | same | none |
| Near chapter edge | fully visible after crop clamp | true position | none |

## Repair hub

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Available | stable service-node icon | labeled service node | "Repair Hub" / available |
| Future unavailable | broken-outline or slash treatment | labeled disabled state | player-facing reason if a source exposes one |

## Ordinary farming spot

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Alive / available | solid standard node | solid node + label | available |
| Combat active | solid node + restrained combat ring | same | active |
| Pending respawn | hollow node + recovery arc | hollow node + arc | exact short timer if available |
| Adaptive pressure active | no special compact clutter | optional small qualitative state | "Elevated" only if exposed by read model |
| Max pressure | no special compact clutter | optional stronger qualitative treatment | "Maximum" only if exposed |
| Recovered | normal solid node | normal state | available |
| Temporarily inactive | hollow node + system slash | inactive label/state | reason if exposed |

## Stronger ordinary spot

Use the ordinary-spot state grammar plus a **double outer bracket / tier notch**.

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Available | solid stronger-node silhouette | label + stronger tier cue | available |
| Combat active | stronger node + combat ring | same | active |
| Pending respawn | hollow stronger node + recovery arc | same | exact timer if available |
| Locked/inactive if future rules require it | slash/lock over stronger-node family | explicit state | reason if exposed |

No exact #40 balance or unlock values are assumed.

## Elite

Elite has three independent dimensions:

- **Progression:** locked / unlocked
- **Encounter:** available / active / cooldown / inactive
- **Reward:** full / capped-fallback / none / unknown

| Progression | Encounter | Reward | Compact minimap | Expanded map / details |
|---|---|---|---|---|
| Locked | any | any | muted diamond + lock/bar | "Locked"; requirement only if player-facing source exists |
| Unlocked | First-kill available | first-clear | solid diamond | "Available" / first encounter |
| Unlocked | Alive | first-clear or repeat | solid diamond + combat ring | "Active" |
| Unlocked | Cooldown | any | hollow diamond + cooldown ring | rounded timer; exact timer when selected |
| Unlocked | Repeat available | full | solid diamond | loop sub-glyph + full reward pip |
| Unlocked | Repeat available | capped/fallback | solid diamond | loop sub-glyph + outlined/barred reward pip |
| Unlocked | Repeat available | no special reward | solid diamond | fightable state remains; no special-reward emphasis |
| Unlocked | Inactive | any | hollow/slashed diamond | inactive state |

The compact map should not attempt to show every reward sub-state if the resulting glyph becomes unreadable. Encounter availability wins; reward state is expanded-map/detail information.

## Boss

Boss uses the same dimensional separation with a dominant base shape.

| Progression | Encounter | Reward / completion | Compact minimap | Expanded map / details |
|---|---|---|---|---|
| Locked | any | not cleared | muted boss symbol + lock/bar | "Locked" |
| Unlocked | First encounter available | first-clear | full boss symbol | "Available" |
| Unlocked | Alive/engaged | first-clear or repeat | full symbol + combat ring | "Active" |
| Unlocked | Defeated | chapter complete | completion notch/check retained on boss symbol | "Chapter completed" plus current repeat state |
| Unlocked | Cooldown | any | hollow boss symbol + cooldown ring | rounded timer; exact selected |
| Unlocked | Repeat available | full | full symbol | repeat sub-glyph + full reward pip |
| Unlocked | Repeat available | capped/fallback | full symbol | repeat sub-glyph + outlined/barred reward pip |
| Unlocked | Repeat available | no special reward | full symbol | encounter remains visibly available |
| Unlocked | Inactive | any | hollow/slashed boss symbol | inactive |

Boss is dominant by size/shape/line weight, not by perpetual flashing.

## Gate

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Closed | barrier icon + lock/bar | closed gate label/state | locked/closed |
| Open | separated/open barrier icon | open state | open |

## Optional objective overlay

Objective status is an **overlay on the underlying marker**, not a replacement kind.

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Current objective | bracket/crosshair around POI | same + label | short objective summary |
| Objective complete | remove overlay after normal completion feedback | underlying POI remains | normal POI state |

## Future exit / next chapter hook

| State | Compact minimap | Expanded map | Selected details |
|---|---|---|---|
| Locked | port/arrow icon + lock | label/state | locked |
| Available | solid port/arrow icon | label/state | available |

## Timer display matrix

| Marker | Compact minimap | Expanded overview | Selected details |
|---|---|---|---|
| Ordinary respawn | recovery arc only | arc, no text by default | exact short countdown |
| Elite cooldown | cooldown ring | rounded minutes | exact `mm:ss` |
| Boss cooldown | cooldown ring | rounded minutes | exact `mm:ss` |

Timer text must never be regenerated every frame.

## Read-model mapping

### Required to resolve marker state

- stable marker `id`
- `kind`
- `worldPosition`
- `visibility`
- presentation-facing availability state
- progression lock dimension where relevant
- next-availability absolute time where relevant
- repeat reward state where #40 requires it

### Derived by UI

- icon asset
- line weight / marker size
- map-space position
- time remaining
- timer string
- cooldown ring fraction
- compact vs expanded label visibility
- color role
- objective overlay presentation

## Priority rules

If multiple state cues compete on a compact marker, use this priority:

1. marker kind / player identity;
2. progression lock;
3. immediate encounter availability;
4. current combat state;
5. cooldown;
6. reward/cap state;
7. objective decoration.

The compact minimap must remain readable even if reward-state decoration is removed entirely.
