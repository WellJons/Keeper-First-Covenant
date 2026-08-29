# Environment Foundation — gameplay-ready world base

This pack is the new foundation for walkable fantasy locations. It is not a
concept sheet: every visual source is consumed by an automatic Unity builder
that creates materials, colliders, gameplay metadata, prefabs, VFX and a
playable validation scene.

## Locked visual direction

- High-detail hand-painted 2.5D fantasy.
- Top-down three-quarter gameplay camera.
- The same silhouette, texture density and material language as the approved
  world-kit reference.
- The palette is biome-driven rather than permanently dark. This first set is a
  bright daylight meadow/road family; later ruins and ancient-magic packs may
  be colder or darker without changing the rendering style.

## Source art

Ground materials (opaque, at least 1024 px):

- `Ground_MeadowGrass_A.png`
- `Ground_WoodlandDirt_A.png`
- `Ground_NaturalStone_A.png`
- `Road_PackedDirt_A.png`
- `Road_OldCobble_A.png`

Interactive foliage (transparent, at least 1024 px):

- `Grass_LowMeadow_A.png`
- `Grass_TallMeadow_A.png`
- `Grass_Wildflowers_A.png`

All source files are independent runtime assets. No sprite is cut from the
reference sheet.

## Generated game assets

Unity automatically creates:

- 3 full walkable ground prefabs;
- packed-dirt and cobblestone road prefabs in straight, corner, T-junction and
  cross variants;
- grass-to-dirt and grass-to-stone transition tiles;
- 3 foliage prefabs with individual wind phase, scale/tint variation and
  player-contact bending;
- solid tile colliders and per-surface 3D physics materials;
- surface profiles for movement and footsteps;
- per-position surface resolution, so the grass at a road's sides and the road
  in its center return different gameplay profiles even though they share one
  tile;
- procedural footstep particles for grass, earth and stone;
- tall-grass concealment/movement/flammability metadata;
- pollen VFX on the wildflower patch;
- global wind with slow non-synchronized gusts;
- `EnvironmentFoundation_Test.unity`.

## Build and validation

The pack bootstraps on the first Unity domain reload when all eight textures are
present.

Manual build:

`Keeper First Covenant -> Environment Foundation -> BUILD COMPLETE PACK`

Validation:

`Keeper First Covenant -> Environment Foundation -> Validate Complete Pack`

The validator checks minimum source resolution, alpha padding/cropping,
texture-import settings, URP shader availability, materials, physical
colliders, triggers, gameplay components, VFX and the test scene.

The normal production-art build also invokes this pack and uses it as the
walkable ground of `ProductionArt_Playground.unity`.

## Test controls

Open:

`Assets/KeeperFirstCovenant/Scenes/EnvironmentFoundation_Test.unity`

Use WASD or the arrow keys. The test walker changes speed according to the
resolved surface profile, emits matching footstep particles and physically
pushes the interactive grass aside.
