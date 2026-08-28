# Combat Systems v3

This milestone extends the tactical prototype beyond basic turn-taking.

## Optional party composition

No companion is required by the combat loop.

Valid test configurations include:

- Edward alone;
- Edward + one companion;
- Edward + several companions.

Aelis and Lucian are ordinary optional Ally combatants from the perspective of the tactical systems.

The F1 sandbox has a **Solo test: disable allies** command.

## Downed and revive

Player/Ally combatants do not immediately become corpses at 0 HP.

They enter **DOWNED**:

- cannot act;
- remain targetable;
- occupy their tactical cell;
- have a limited bleedout round counter;
- can be revived by healing.

If all active party members are downed/dead, combat ends in defeat.

Further damage to a downed combatant causes final death.

### Aelis prototype

Aelis is generated as optional party test content with:

- Healing Light — healing that can revive a downed ally;
- Silver Barrier — grants a temporary damage barrier.

She is not forced into the party or prototype scene.

## Equipment

EquipmentComponent supports:

- Main Hand;
- Off Hand;
- Head;
- Chest;
- Hands;
- Legs;
- Feet;
- Cloak;
- Amulet;
- two ring slots.

WeaponDefinition can grant combat actions while equipped.

ArmorDefinition can modify:

- Armor;
- Magic Guard;
- movement;
- granted actions.

The runtime action list combines character abilities and equipped-item abilities.

## Damage affinities

CharacterDefinition supports per-damage-type multipliers.

Examples:

- 0.0 = immunity;
- 0.5 = resistance;
- 1.0 = normal;
- 1.5 = vulnerability.

Damage previews include affinity and mitigation.

## Smarter positioning

Enemy AI evaluates reachable cells using:

- attack range;
- line of sight;
- distance;
- high ground;
- dangerous elemental surfaces;
- occupied cells.

This prevents the default behavior of blindly taking the shortest path through fire or other hazards.

## Forced movement and falling

CombatActionDefinition can specify push distance.

ForcedMovementSystem:

- moves targets through tactical cells;
- stops at blocked cells;
- prevents pushing uphill through impossible steps;
- detects vertical drops;
- applies physical fall damage;
- resolves extreme off-grid drops as severe/fatal falls.

Edward's prototype action list includes **Shove** for testing.

## Developer support

F1 developer sandbox now supports:

- side-by-side comparison of characters/enemies/items/weapons/abilities;
- spawning test combatants during live combat;
- dynamic initiative insertion;
- giving/equipping test gear;
- solo-party testing;
- resource restoration;
- elemental surface creation;
- combat restart;
- live combat log.

All generated DEV assets remain mechanical test content, not final lore or balance.
