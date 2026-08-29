# Party Save Payload V1

Save slots now restore actual party state, not only scene/world metadata.

Persisted per player/ally characterId:
- world position;
- rotation;
- current health;
- current mana;
- barrier;
- downed/dead state;
- remaining downed rounds;
- inventory stacks by itemId;
- equipment by slot and itemId.

Item references are resolved from loaded ItemDefinition assets after the target scene loads.
The system uses stable itemId/characterId values, so moving or renaming GameObjects does not break saves.

## Combat safety

TurnCombatDirector initiative order and active status durations are not serialized yet.
Manual/quick saves are therefore blocked while CombatState.Active.

Returning to the main menu during an active battle is allowed, but it explicitly returns to the last save made before that battle rather than writing a partial combat snapshot.

## Extension point

The next persistence layer should add:
- quest journal state;
- searched/looted world objects;
- destroyed/interacted scene objects;
- full active-combat snapshot if mid-combat saves are desired.
