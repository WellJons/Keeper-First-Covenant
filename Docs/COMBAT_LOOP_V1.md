# Combat Loop v1

This milestone turns the early systems foundation into a testable turn-based loop.

## Included

- Individual initiative.
- Player tactical input.
- Click-to-move through the tactical grid.
- Movement budget consumption.
- Occupied-cell protection.
- Ability selection on keys 1-8.
- Targeted melee/spell execution.
- AP and mana costs.
- Dice-based damage.
- Enemy turns with simple target selection.
- Enemy movement toward attack range.
- Searchable enemy corpses after victory.
- Exploration interaction with corpses/caches.
- Prototype debug HUD.

## Build the test scene

1. Open the Unity project.
2. Run **Keeper First Covenant -> Build Prototype Road Scene**.
3. Run **Keeper First Covenant -> Install Combat Loop In Open Scene**.
4. Enter Play Mode.

## Prototype controls

- **Left mouse on ground** — move active player character.
- **1-8** — select an ability.
- **Left mouse on target** — use selected ability.
- **Right mouse / Esc** — cancel ability.
- **Space** — end current turn.
- After victory, **left mouse on corpse/cache** — search it.

## Next combat layer

The next milestone should add:

- line of sight;
- height and cover;
- area-of-effect targeting previews;
- elemental surfaces and interactions;
- reactions / opportunity attacks;
- proper tactical HUD and target preview;
- smarter enemy archetypes;
- downed/revive rules;
- combat log and deterministic roll presentation.
