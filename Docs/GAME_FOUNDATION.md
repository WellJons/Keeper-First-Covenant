# Gameplay foundation

## Exploration

Exploration is real-time. Combat switches into individual initiative turn order when hostilities begin.

The world supports:
- interactable doors, shrines, bodies, chests and environmental objects;
- hidden items discovered by Perception;
- locks and keys;
- companion-specific observations;
- quest/world flags;
- optional side regions that can be completed without touching the main mystery.

## Tactical combat

Combat is party-based with individual initiative.

Each combatant has:
- health;
- mana;
- action points;
- movement;
- reaction capacity (reserved for the next layer);
- physical and magical defenses;
- primary attributes.

Abilities are data assets and can represent:
- sword attacks;
- ranged attacks;
- fire/frost/lightning magic;
- healing;
- barriers;
- control;
- movement;
- ancient/unique powers.

Damage uses dice formulas. A spell may be `2d6 + Intellect`; a heavy sword may be `1d10 + Strength`.

## Environmental magic target

The combat API reserves surface hooks from the start:

- Fire
- Water
- Ice
- Poison
- Electrified
- Arcane

Later interactions can include:
- fire igniting oil/poison;
- frost freezing water;
- lightning propagating through wet areas;
- wind spreading or clearing effects;
- barriers blocking cells/lines;
- destroyed props changing routes.

## Loot and searching

Enemies do not simply explode into glowing loot.

A body/container can contain:
- visible common loot;
- hidden pockets;
- keys;
- letters;
- quest objects;
- rare items gated behind Perception;
- nothing valuable.

The same search system will be used for corpses, furniture, wall caches and hidden compartments.

## First playable milestone

The first vertical slice should eventually contain:

1. a road/forest scene;
2. Edward and Lucian;
3. White (unnamed at first);
4. Borg and the purchase encounter;
5. one optional roadside encounter;
6. one tactical battle;
7. searchable enemies and a hidden cache;
8. a small dialogue consequence;
9. arrival direction toward Reinholm.

The prototype scene created by the editor builder is only the systems/art-direction sandbox, not the final story scene.
