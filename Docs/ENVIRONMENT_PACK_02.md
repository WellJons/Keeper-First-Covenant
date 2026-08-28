# Environment Pack 02 — Ground, Grass & Trees

First foundational nature pack for the approved Keeper: First Covenant dark-fantasy 2.5D visual direction.

## Included runtime art

### Ground
- Ground_Dirt_A
- Ground_Dirt_B
- Ground_Stone_A
- Ground_Stone_B
- Ground_Grass_A
- Path_Stone_A
- Transition_GrassToStone_A
- Puddle_A

### Nature
- Grass_Small_A
- Grass_Tall_A
- Flower_Blue_A

### Trees
- Tree_Living_A
- Tree_Living_B
- Tree_Twisted_A
- Tree_Dead_A
- Log_A

## Gameplay integration

Opening Unity after copying the source PNGs automatically builds all prefabs and the test scene.

Manual command:

`Keeper First Covenant -> Production Art -> Environment Pack 02 -> BUILD EVERYTHING`

Validation:

`Keeper First Covenant -> Production Art -> Environment Pack 02 -> Validate Pack`

Test scene:

`Assets/KeeperFirstCovenant/Scenes/EnvironmentPack02_Test.unity`

## Wind

Grass, flowers and trees use `FoliageWind.shader` plus `WindReactiveProp`.

The base remains anchored while the upper part of the sprite bends. Each instance receives a phase offset so a field of grass does not move in perfect synchronization.

Grass uses stronger/faster sway. Living trees use subtle slow movement. Twisted/dead trees are intentionally stiffer.

## Collision

- Ground tiles: visual only; scene/world provides walkable ground collision.
- Puddle: shallow trigger volume for future wet-footstep/status logic.
- Grass/flowers: no collision.
- Trees: stable capsule collider on the trunk root.
- Log: stable box collider.

Only the visual child billboards. Tree/log collision never rotates with the camera.

## Surface metadata

Ground prefabs include `EnvironmentSurfaceMarker` with Dirt / Stone / Grass / Water types so footsteps, particles and movement modifiers can use the same assets later.
