# Environment Pack 03 — Nature Expansion

Second nature/environment gameplay pack for the locked Keeper: First Covenant dark-fantasy 2.5D visual identity.

## Art payload

### Rocks
- Rock_SmallPile_A
- Rock_Cluster_Medium_A
- Rock_Shard_Tall_A
- Rock_Boulder_Mossy_A
- Rock_Rubble_Flat_A

### Wind-reactive plants
- Grass_Clump_B
- Grass_Dense_A
- Flower_Blue_B
- Flower_Pale_A

### Trees / woody props
- Tree_Leafy_C
- Sapling_A
- Stump_B

### Environmental fillers
- Puddle_MuddyCluster_A
- BrokenSignpost_A
- Debris_Small_A

## Gameplay behavior

Rocks use stable world-space collision where their size should block movement. Flat rubble is visual-only to avoid snagging navigation.

Grass and flowers reuse the same FoliageWind shader and WindReactiveProp from Environment Pack 02. EnvironmentVisualVariation adds deterministic scale/mirroring to reduce obvious repetition without changing colliders.

Tree_Leafy_C uses subtle canopy sway and a stable trunk CapsuleCollider. Sapling_A sways but intentionally does not block movement. Stump_B uses a compact BoxCollider.

Puddle_MuddyCluster_A is walkable and uses a shallow trigger plus Water/Wet EnvironmentSurfaceMarker metadata.

BrokenSignpost_A blocks movement with a compact BoxCollider. Debris_Small_A is visual-only.

## Build

After the PNG payload is copied into:

`Assets/KeeperFirstCovenant/Art/Runtime/World/EnvironmentPack03/`

Unity auto-builds all 15 prefabs and:

`Assets/KeeperFirstCovenant/Scenes/EnvironmentPack03_Test.unity`

Manual command:

`Keeper First Covenant -> Production Art -> Environment Pack 03 -> BUILD EVERYTHING`

Validation:

`Keeper First Covenant -> Production Art -> Environment Pack 03 -> Validate Pack`

The pack is also inserted into ProductionArt_Playground when that scene is rebuilt.

## Style lock

Pack 03 intentionally reuses the same production-art source family, color language, isometric perspective, sprite import rules, foliage material, wind shader, scene lighting and scale conventions as Packs 01–02. It is not a separate visual direction.
