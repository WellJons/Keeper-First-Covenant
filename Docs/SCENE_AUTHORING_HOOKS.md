# Scene Authoring Hooks

The shell now includes reusable scene-level hooks.

## SceneSpawnPoint
Defines a stable spawn ID inside a scene. Scene travel places the player and allies into a small formation around the requested spawn.

## SceneTravelPortal
Implements IInteractable and sends the party to another build scene.

Flow:
1. save the current scene state;
2. load the target scene;
3. restore global world and quest state;
4. place the party at the target spawn ID;
5. autosave the new scene and party position.

Travel is blocked during dialogue and active combat.

## AutosaveCheckpoint
A trigger collider that autosaves when the player enters it.
A WorldState flag can make the checkpoint one-shot per save.

## QuestEventTrigger
Reusable quest hook activated by a player trigger or any UnityEvent.

Actions:
- start quest;
- add objective progress;
- complete objective;
- complete quest;
- fail quest.

One-shot state is stored through WorldState, so it survives save/load.

These components let scene content advance progression without adding custom C# for every door, discovery, ruin, encounter or scripted beat.
