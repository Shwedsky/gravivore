# GRAVIVORE — Audio / VFX Event Matrix

Status: **integration planning matrix**

| Event | Sound candidate / layer | VFX direction | Priority | Playback | Intensity | Notes |
|---|---|---|---|---|---|---|
| Player step | Mechanical Sounds clank/lightclunk + low actuator | tiny foot contact dust/spark only if needed | P0 | one-shot | low | 2–4 variations; dedicated low-volume footstep voice |
| Player attack charge | Kenney Sci-fi / rubberduck energy layer + coil tension | core emissive ramp + inward particles + micro arcs | P0 | one-shot/envelope | medium | timing follows authoritative charge |
| Player release | short synthetic snap + low mechanical impulse | impulse flash + directional burst | P0 | one-shot | high | strongest player-owned transient |
| Lash travel | restrained energy hiss | thin core filament + faint halo | P0 | very short | medium | avoid cartoon laser tube |
| Player impact | metal transient + energy crack | localized flash + sparks + tiny debris | P0 | one-shot | high | target-position cue |
| Enemy hit | light metal + electrical spit | compact hit flash/sparks | P0 | one-shot | low/med | pitch/variation |
| Enemy death | mechanical failure + short discharge | stronger discharge/debris than hit | P0 | one-shot | medium | distinct from hit |
| Player hit | plated armor transient + short warning texture | brief red/orange localized damage flash | P1 | one-shot | medium | never UI beep only |
| Player death | power-down + mechanism collapse | shutdown burst / fade | P1 | one-shot | high | 0.5–1.2 s target |
| Scout movement | light motor/servo | minimal hover/ion flecks if needed | P1 | sparse/loop | low | lightest enemy |
| Cutter attack | blade/drive whirr + metallic bite | hot-edge / friction sparks | P1 | one-shot | medium | no beam |
| Arc attack | electrical pre-buzz + arc snap | forked short arc | P1 | one-shot | medium | unique from Cutter |
| Warden action | heavy armor/servo + shield pulse | plated shield pulse | P1 | one-shot | medium | defensive identity |
| Carrier movement | heavy chassis/actuator | minimal dust/debris | P1 | sparse/loop | medium | heavier than Warden |
| Elite charge | coil saturation rise | paired magnetic coil glow | P1 | one-shot | high | Magnetar signature |
| Elite attack | dense magnetic discharge | radial compressed pulse | P1 | one-shot | high | not boss-sized |
| Elite shockwave | low pressure pulse + crack | ground-hugging ring | P1 | one-shot | high | telegraph remains authoritative |
| Elite death | coil collapse + mechanical shutdown | unstable arcs then compact burst | P1 | one-shot | high | signature |
| Boss cone telegraph | broad pressure/vent rise | fan geometry / segmented hazard | P0 | one-shot | high | distinguish by envelope/shape |
| Boss cone impact | broad industrial discharge | forward pressure burst | P0 | one-shot | very high | no screen-fill |
| Boss line telegraph | focused tonal charge | narrow rail geometry | P0 | one-shot | high | clearest straight danger cue |
| Boss line impact | focused snap/rail pulse | thin high-energy directional strike | P0 | one-shot | very high | short tail |
| Boss circle telegraph | cyclic capacitor pulse | segmented closed ring | P0 | one-shot | high | distinct rhythm |
| Boss circle impact | low radial pulse | ground shockwave + sparse debris | P0 | one-shot | very high | center remains readable |
| Boss heavy hit | dense plated transient | localized heavy sparks | P1 | one-shot | high | feedback, not telegraph |
| Boss death | staged machinery shutdown | multi-stage core failure, restrained debris | P0 | one-shot sequence | very high | no single giant explosion |
| Repair hub ambience | electronic device / filtered machine bed | none/very subtle | P1 | loop | very low | no high-frequency fatigue |
| Repair arm | mechanical1/2-type actuator | arm motion + occasional warm spark | P1 | sparse one-shot | low | presentation only |
| Welding | short spark/crackle | warm welding sparks | P1 | sparse one-shot | low | avoid continuous piercing loop |
| Repair beam | soft energy texture | thin cyan filament | P1 | short/loop | low | explains heal visually |
| Diagnostic pulse | restrained UI/system click | scanner sweep | P1 | one-shot | low | low brightness |
| Evolution | synthetic rise + mechanical confirmation | cyan/white evolution burst | P1 | one-shot | high | may be stereo/2D |
| Stat gain | small system confirmation | tiny restrained pulse | P1 | one-shot | low | not same as evolution |
| Menu open/close | Kenney Interface dry click | none/minimal | P1 | one-shot | very low | reuse family |
| Confirm | short mechanical click | minimal | P1 | one-shot | low | non-combat |
| Deny/error | short lower/duller click | minimal | P1 | one-shot | low | no comic buzzer |
| Chapter completion | 1.5–3 s industrial success stinger | restrained success emissive sequence | P1 | one-shot | high | not heroic orchestral |
| Chapter music | Searching/Bilwe/Persistence audition | n/a | P1 | loop | low | first external test: one loop |

## P0 integration order

1. Player release.
2. Player impact.
3. Enemy death.
4. Boss cone / line / circle telegraph and impact distinction.
5. Player footsteps.
6. Gravity Lash visual sequence.
7. Boss death.

This order maximizes perceived quality change before expanding low-value ambience.
