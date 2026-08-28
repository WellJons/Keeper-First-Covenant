# Keeper: First Covenant

Russian title: **Хранитель: Первый Завет**

A standalone fantasy tactical RPG project set in the Edward Chronicles universe.

## Design pillars

- Story-first full RPG: exploration, companions, dialogue consequences, side quests, secrets and optional regions.
- Turn-based tactical combat with initiative, movement, melee weapons, magic, healing, barriers and environmental interactions.
- Dice-based damage formulas such as `2d6 + modifier`, without copying another game's ruleset.
- Searchable enemies, containers, hidden loot and perception-gated discoveries.
- Strong visual identity built around the contrast between warm living fantasy and cold ancient systems: fire, silver threads/circles, black restraint motifs, weathered stone and a white goat with an ancient collar.
- The main plot begins small and becomes progressively stranger: Edward leaves home, meets Lucian and buys the white goat that will later be named Eleanor.

## Project layout

```
Assets/KeeperFirstCovenant/
  Scripts/
    Combat/
    Characters/
    Core/
    Inventory/
    World/
    Editor/
  Art/
  Data/
  Prefabs/
  Scenes/
Docs/
Packages/
ProjectSettings/
```

## Current bootstrap

The first bootstrap establishes reusable systems before story scenes are built:

1. individual initiative and turn order;
2. action points, movement and mana;
3. dice formulas and typed damage;
4. spell/ability definitions;
5. status effects and elemental surface hooks;
6. a 3D tactical navigation grid;
7. character definitions and runtime combat state;
8. RPG item, weapon, inventory and loot/search foundations;
9. persistent world-state flags/values;
10. an Editor prototype builder for the first recognizable visual test scene.

Unity version is kept aligned with the existing Cats and Kills project for easier parallel development.
