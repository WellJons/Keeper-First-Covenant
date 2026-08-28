# High-resolution 2D production pipeline

Keeper: First Covenant uses **full painted frame animation** for important gameplay characters.

This replaces the rejected cutout/paper-doll placeholder approach.

## Visual target

The game must preserve the visual quality of the approved battle screenshot:

- adult human proportions;
- detailed dark-fantasy anime characters;
- top-down three-quarter camera;
- large readable silhouettes;
- layered cloth and cloak motion;
- strong hand-painted/cel-painted lighting;
- no chibi;
- no browser/MMO sprite look;
- no rotating paper-doll limbs.

## Character frame model

Every animation frame is a complete painted silhouette for that frame.

Runtime directions:

- N
- NE
- E
- SE
- S
- SW
- W
- NW

The initial art pass may author N / NE / E / SE / S and mirror west directions. Asymmetric equipment can later receive unique west strips.

## Required Edward states

- Idle — 6 frames
- Walk — 10 frames
- Run — 8 frames
- CombatIdle — 6 frames
- Guard — 6 frames
- AttackLight — 10 frames
- AttackHeavy — 12 frames
- Cast — 10 frames
- Interact — 8 frames
- Hit — 6 frames
- CriticalHit — 8 frames
- Knockdown — 10 frames
- Death — 14 frames

These numbers are production targets, not hard engine limits.

## Folder convention

Base frames:

```
Assets/KeeperFirstCovenant/Art/Characters/Edward/Base/
  Idle/
    N/
    NE/
    E/
    SE/
    S/
  Walk/
  Run/
  CombatIdle/
  Guard/
  AttackLight/
  AttackHeavy/
  Cast/
  Interact/
  Hit/
  CriticalHit/
  Knockdown/
  Death/
```

Frame names must sort in playback order:

```
frame_000.png
frame_001.png
frame_002.png
...
```

## Equipment

Equipment is **not** a rotating limb layer.

Armor, cloaks and weapons use full overlay frame sequences aligned with the base animation.

Example:

```
Edward/Base/Walk/SE/frame_004.png
Edward/Equipment/Cloak_Traveler/Walk/SE/frame_004.png
Edward/Equipment/Sword_Travel/Walk/SE/frame_004.png
```

All layers share the same canvas size, pivot and frame numbering.

This makes equipment exchange possible without sacrificing the painted animation style.

## Runtime

`HighResFrameCharacter2D`:

- plays the base frame strip;
- resolves 8-direction facing;
- synchronizes armor/cloak/weapon/headgear/accessory overlays;
- supports looping locomotion and one-shot attacks;
- emits animation impact events.

`HighResExplorationController`:

- free movement before combat;
- camera-relative WASD;
- Shift run;
- automatic 8-direction facing.

`HighResCharacterCombatBridge`:

- attack/cast state from tactical actions;
- hit and critical-hit reactions;
- death animation;
- returns to combat idle.

## Unity import

Character frame PNGs are imported automatically as:

- Sprite (2D and UI);
- transparent;
- no mipmaps;
- clamp;
- bilinear;
- uncompressed;
- 256 pixels per unit;
- max texture 2048.

## Building the library

After placing PNG frames:

`Keeper First Covenant -> High-Res 2D -> Rebuild Edward Base Library`

Then:

`Keeper First Covenant -> High-Res 2D -> Build Edward Prefab`

Result:

`Assets/KeeperFirstCovenant/Prefabs/Characters/Edward_HighRes2D.prefab`

## Production rule

Do not solve a missing animation by rotating a forearm, head or cape sprite.

If a pose materially changes the silhouette, draw the frame.
