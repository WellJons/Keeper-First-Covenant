# Approved production art is inside the Unity project

The approved visual direction is no longer stored only as chat concept art.

Source sheets committed to the project:

- `Assets/KeeperFirstCovenant/Art/ProductionSheets/Edward_ProductionSheet.jpg`
- `Assets/KeeperFirstCovenant/Art/ProductionSheets/Eleanor_ProductionSheet.jpg`
- `Assets/KeeperFirstCovenant/Art/ProductionSheets/Aelis_ProductionSheet.jpg`
- `Assets/KeeperFirstCovenant/Art/ProductionSheets/WorldKit_ProductionSheet.jpg`

## One command

In Unity run:

`Keeper First Covenant -> Production Art -> BUILD EVERYTHING INTO GAME`

The command performs all of the following:

1. Slices the approved character sheets into persistent Unity Sprite sub-assets.
2. Builds frame-animation libraries for Edward, Eleanor, Aelis and White.
3. Builds actual character prefabs with the high-resolution frame animator.
4. Builds independent world sprites and prefabs from the approved world sheet.
5. Raster-extracts transparent RGBA subtextures from the dark production-sheet panels, so characters/world props do not render as rectangular cards.
6. Keeps the diagnostic cutout shader only as a fallback/debug path.
7. Creates a playable exploration scene.
8. Adds the scene to Build Settings.

Playable scene:

`Assets/KeeperFirstCovenant/Scenes/ProductionArt_Playground.unity`

Generated character prefabs:

- `Assets/KeeperFirstCovenant/Prefabs/ProductionCharacters/Edward_FromSheet.prefab`
- `Assets/KeeperFirstCovenant/Prefabs/ProductionCharacters/Eleanor_FromSheet.prefab`
- `Assets/KeeperFirstCovenant/Prefabs/ProductionCharacters/Aelis_FromSheet.prefab`
- `Assets/KeeperFirstCovenant/Prefabs/ProductionCharacters/White_FromSheet.prefab`

Generated world prefabs:

`Assets/KeeperFirstCovenant/Prefabs/ProductionWorld/`

The world builder currently produces independent floor tiles, roads, walls, wall corners, arches, pillars, stairs, altar, rune stones, rune circles, puddles, rocks, grass, flowers, trees, braziers, lanterns, banners, campfire, crates, barrels, bench, wagon, tent, fences, statue, crystal and shrine props.

## Current frame coverage

The source sheets already provide usable frame rows for:

### Edward

- directional standing facings;
- idle;
- walk;
- run;
- combat idle;
- light attack;
- heavy attack;
- fire cast;
- hit;
- death.

### Eleanor

- directional standing facings;
- idle;
- walk;
- combat idle;
- ancient-magic cast;
- barrier;
- hit;
- death;
- White/goat directional facings;
- White idle/walk.

### Aelis

- directional standing facings;
- idle;
- walk;
- combat idle;
- heal;
- revive/support;
- barrier;
- hit;
- death.

## Important production limitation

The approved source sheets are enough to get the new visual style into the playable game immediately, but they are not a final animation export pack.

For locomotion, only the primary illustrated movement row is currently truly animated. Other directions use their correct directional standing silhouette until dedicated full cycles are painted for that direction.

The engine is already designed for unique N / NE / E / SE / S / SW / W / NW strips, so replacing these temporary directional fallbacks requires adding art, not rewriting movement/combat code.

No paper-doll limb rotation is used.
