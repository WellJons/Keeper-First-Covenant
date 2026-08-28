# F1 Developer Sandbox

The developer sandbox is installed automatically by:

**Keeper First Covenant -> Install Combat Loop In Open Scene**

Open or close it during Play Mode with **F1**.

The game pauses while the menu is open and normal tactical input is blocked.

## Tabs

### Characters

Shows all non-enemy CharacterDefinition assets.

Use A and B to compare two characters side-by-side.

Displayed data includes:

- HP / Mana
- Armor / Magic Guard
- AP / movement
- initiative
- primary attributes
- damage resistances / vulnerabilities
- starting abilities

Characters can be spawned as temporary DEV units.

### Enemies

Shows CharacterDefinition assets with the Enemy faction.

The menu can compare enemy archetypes and spawn them into the current scene.

If combat is already active, a newly spawned enemy is inserted into the live initiative order.

### Items

Shows non-weapon ItemDefinition assets.

Items can be given to the currently active party member, or to the first available player-controlled character.

### Weapons

Shows WeaponDefinition assets.

Comparison includes:

- weapon class
- damage dice
- damage type
- scaling attribute
- range
- two-handed flag
- finesse flag
- magical-focus flag
- weight and value

### Abilities

Shows CombatActionDefinition assets.

Comparison includes costs, targeting, damage, range, AoE, LOS, cover/height rules and surface creation.

### Cheats

Current test tools:

- restore party HP and Mana;
- restore AP, movement and reaction resources;
- kill all enemies;
- disable all optional Ally combatants for a solo-party test;
- restore disabled allies;
- start/restart combat;
- end current turn;
- clear DEV-spawned units;
- create elemental surfaces near the party.

## Automatic test catalog

The installer generates development-only comparison content under the ignored Generated folder.

Current DEV enemy archetypes:

- Bandit Skirmisher
- Ash Cultist
- Storm Guard

Current DEV weapons:

- Iron Sword
- Hunter Dagger
- Iron Greatsword
- Ritual Staff
- Hunter Spear

Current DEV items:

- Healing Draught
- Lockpick

These are mechanical test assets, not final lore or balance.

## Important party rule

No combat system requires Lucian, Aelis or any other companion to be present.

Player and Ally combatants are controlled by the player when they exist, but the combat loop works with Edward as the only party member.
