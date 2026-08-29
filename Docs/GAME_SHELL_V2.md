# Game Shell V2

This layer extends the main-menu work into the playable game shell.

## Pause menu

Every gameplay scene automatically receives an in-game pause menu.

Controls:
- Escape / gamepad Start — open or close pause.
- F5 — quick save into the active slot while unpaused.

Actions:
- Continue.
- Save game.
- Load game.
- Settings.
- Main menu.
- Exit game.

Returning to the main menu and exiting automatically save the active slot.

## Session play time

GameFlowController now tracks actual unpaused gameplay time for the current slot.
The timer stops while:
- the game is paused;
- a scene transition is active;
- Boot or MainMenu is active.

The value is persisted into SaveGameData.playTimeSeconds.

## Loading screen

All GameFlow scene transitions now use a persistent loading screen with:
- the approved fantasy menu artwork/live background;
- dark presentation veil;
- loading label;
- contextual hint;
- progress line;
- animated silver spinner.

The screen is shared by main-menu-to-game, load-game, and return-to-menu transitions.

## Save behavior

Manual save updates:
- current scene;
- current location metadata;
- accumulated play time;
- modified timestamp.

Application quit, application pause, and return-to-main-menu perform an automatic save when an active gameplay slot exists.

## Next layer

The save payload already reserves world/party/quest JSON. The next implementation step is serializing actual world flags, world time, quests, inventory and character state into those payloads.
