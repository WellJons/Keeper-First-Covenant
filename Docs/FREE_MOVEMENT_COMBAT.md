# Free Movement Combat

Keeper: First Covenant does not use visible combat cells.

The target model is a seamless tactical RPG flow similar to BG3:

- exploration and combat happen in the same world space;
- entering combat never moves a character to a separate arena;
- positions are preserved exactly at the moment combat begins;
- movement in combat is continuous;
- distance travelled consumes the actor's movement budget in meters;
- the player clicks an exact point on the ground, not a cell center;
- the cursor previews the navigation path and its cost;
- a path that exceeds remaining movement is shown as unavailable;
- AoE targeting remains geometric and continuous.

## Hidden navigation

TacticalGrid3D currently remains as an internal navigation lattice only.

It is not gameplay-facing.

Its purpose is:

- obstacle avoidance;
- route finding;
- AI candidate generation;
- forced-movement/fall checks.

The player never sees or selects its cells.

The hidden lattice has been refined to 0.75 m resolution so AI movement does not visibly resemble grid stepping.

A future NavMesh replacement can be introduced without changing the player-facing combat rules.

## Exploration -> combat

ExplorationMovementController uses the same continuous navigation path as combat, but without consuming a turn movement budget.

SeamlessCombatEncounter:

1. detects a party member entering an encounter;
2. gathers nearby active participants;
3. stops any current movement at the exact current positions;
4. starts initiative in-place.

No teleport, arena swap or snapping occurs.

## Combat movement preview

When no ability is selected:

- hovering terrain builds the exact movement route;
- the route line is drawn in world space;
- HUD shows path cost versus remaining movement;
- valid/invalid destination state is displayed.

## Prototype combat start

CombatStartOnPlay is forcibly disabled by the installer.

Combat is started by:

- an in-world encounter trigger;
- the F1 developer sandbox;
- future dialogue/hostility/perception systems.

This prevents the prototype from behaving like a separate combat scene.
