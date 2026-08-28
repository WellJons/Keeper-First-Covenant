# Production World Pack 01

This is the first gameplay-ready environment pack for the approved dark-fantasy 2.5D visual direction.

## Runtime sprite paths

Place these four transparent 1024x1024 PNG files in:

`Assets/KeeperFirstCovenant/Art/Runtime/World/ProductionPack01/`

- `RuinedArch_Wide.png`
- `RuneStone_TallBlue.png`
- `CovenantCircle_BrightBlue.png`
- `Brazier_Blue.png`

The existing runtime-art texture postprocessor imports them as uncompressed sprites at 256 PPU.

## Generated gameplay prefabs

Unity automatically generates:

- `Prefabs/ProductionWorld/Pack01/RuinedArch_Wide.prefab`
- `Prefabs/ProductionWorld/Pack01/RuneStone_TallBlue.prefab`
- `Prefabs/ProductionWorld/Pack01/CovenantCircle_BrightBlue.prefab`
- `Prefabs/ProductionWorld/Pack01/Brazier_Blue.prefab`

### Collision

**RuinedArch_Wide**
uses three separate solid box colliders: left pier, right pier and lintel. The center opening remains physically passable.

**RuneStone_TallBlue**
uses a solid box collider matching the stone base/body.

**CovenantCircle_BrightBlue**
is walkable. It uses a shallow trigger volume rather than blocking collision.

**Brazier_Blue**
uses a solid base collider plus a separate trigger sphere around the flame region.

### VFX

Rune stone, covenant circle and brazier receive dynamic point lights, particle systems and a runtime pulsing-light controller.

The billboard component is applied only to the painted visual child. Colliders remain on the non-rotating prefab root so gameplay geometry does not move when the camera rotates.

## Build / validation

Automatic integration runs when the source PNGs are present.

Manual build:

`Keeper First Covenant -> Production Art -> World Pack 01 -> BUILD PACK`

Validation:

`Keeper First Covenant -> Production Art -> World Pack 01 -> Validate Pack`

Test scene:

`Assets/KeeperFirstCovenant/Scenes/ProductionWorldPack01_Test.unity`

The pack is also added to `ProductionArt_Playground` whenever the production-art playable scene is rebuilt.
