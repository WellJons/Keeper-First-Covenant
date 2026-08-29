# Save Payload V1

The runtime save file now captures more than slot metadata.

## Persisted world data

SaveGameData.worldStateJson contains:
- world boolean flags;
- world integer values;
- current world day;
- current world hour.

WorldState can now capture, restore and reset deterministic snapshots.

## New game isolation

Starting a new game resets the persistent WorldState before loading the first playable scene.
This prevents flags from a previous playthrough leaking into a fresh run.

## Restore behavior

When an active save scene finishes loading, GameFlowController restores the saved world payload into:
- WorldState;
- WorldTimeSystem.

Gameplay systems listening to WorldState.FlagChanged, ValueChanged or StateRestored can refresh themselves after restore.

## Loading bar

The loading progress bar uses RectTransform anchors rather than Image fill sprites, so it remains reliable with the procedural UI factory and does not depend on a sprite asset.
