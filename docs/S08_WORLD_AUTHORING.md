# S08 World Authoring

Chapter 01 world layout is owned by `S08_Chapter01World.asset`. Its five zone IDs and
centers must match the S04 spawn-spot definitions. Decorative pads, paths, and
landmarks do not have active colliders; only locked gate barriers use the
`HardBlocker` layer.

The elite gate currently requires first kills for all five ordinary enemy IDs and
25 total assimilation score. This is a provisional vertical-slice balance value
for tuning in S20, not a gameplay constant in a MonoBehaviour.

`WorldUnlockSnapshot` is the persistence boundary. S08 only exports and restores
this immutable value; serialization and storage belong to S12.
