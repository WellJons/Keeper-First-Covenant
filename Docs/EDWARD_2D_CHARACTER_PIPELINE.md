# Edward — 2.5D production character pipeline

Edward is **not** a baked full-body sprite with a sword attached.

The playable character is a modular 2D paper-doll standing in the 3D tactical world.

## Why this approach

It lets the project support:
- turning in four directions;
- smooth movement animation without hundreds of full-body frames;
- separate weapons;
- swappable armor/clothing;
- persistent face/hair identity;
- spell FX independent from clothing;
- later equipment variants without rebuilding the character controller.

## Initial direction set

Use four gameplay facings:

- SouthEast
- SouthWest
- NorthEast
- NorthWest

For the first production pass only two painted directions are mandatory:
- SouthEast
- NorthEast

The west variants can be mirrored. Unique west sprites can be added later for asymmetric armor.

## Rig slots

The initial rig supports:

- Body
- Head
- Hair
- CloakBack
- Torso
- UpperArmLeft / ForearmLeft / HandLeft
- UpperArmRight / ForearmRight / HandRight
- Pelvis
- ThighLeft / ShinLeft / BootLeft
- ThighRight / ShinRight / BootRight
- CloakFront
- Weapon (separate renderer/socket)

## Equipment rule

Weapons never get baked into Edward's body art.

Armor/outfits are appearance overlays that replace only the affected body slots.

Examples:

- boots replace BootLeft + BootRight;
- gloves replace HandLeft + HandRight and optionally forearms;
- chest armor replaces Torso and optionally upper arms;
- a cloak replaces CloakBack + CloakFront;
- a helmet can hide Hair and replace Head/Hair layers;
- swords/staves/bows live only in the weapon renderer and socket.

## Animation strategy

Phase 1 uses transform animation:
- leg and arm swing for walking;
- subtle torso/head bob;
- cloak sway;
- weapon socket swing for attacks;
- free hand pose for spell casts.

This is enough for a polished prototype and keeps every equipment combination compatible.

Phase 2 can add directional replacement sprites for:
- extreme sword attack silhouettes;
- knockdown;
- dodge;
- heavy spell poses;
- climbing / interaction.

## Edward visual identity

The reference to preserve is the first key art:
- young dark-haired face;
- messy medium-length black hair;
- layered dark travel clothes;
- practical leather straps and gloves;
- long worn cloak/scarf silhouette;
- athletic but not bulky build;
- no royal armor at the start;
- fire is an FX layer, not permanently painted into the body.

The starting sword is a separate asset and may be replaced at any time.
