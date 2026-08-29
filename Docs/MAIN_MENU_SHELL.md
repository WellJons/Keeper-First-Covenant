# Main menu shell

The first application shell for **Keeper: First Covenant** now lives in code and is generated into Unity scenes automatically.

## Scenes

On the first editor domain load, MainMenuSceneBuilder ensures these scenes exist:

1. Boot — short title/sigil splash.
2. MainMenu — main menu and settings/save UI.
3. Prototype_Road — current systems sandbox used as the temporary first playable target.

All three are registered in Build Settings, with Boot first.

## Main menu actions

- **Продолжить** — loads the most recently modified save. Disabled when no saves exist.
- **Начать новую игру** — creates the first free save slot and loads Prototype_Road.
- **Выбрать сохранение** — shows six slots with timestamp, location, playtime, load and delete actions.
- **Настройки** — resolution, fullscreen mode, quality, FPS cap, VSync, master/music/SFX volume and camera shake preference.
- **Выход** — confirmation followed by application quit.

## Save foundation

Saves are JSON files under Application.persistentDataPath/Saves/save_XX.json.

The shell reserves uncoupled payload strings for world, party and quest state. This lets gameplay systems later serialize into the same save format without rewriting the slot browser.

## Presentation direction

The shell follows Docs/VISUAL_IDENTITY.md:

- restrained dark UI panels;
- cold silver geometry;
- warm living/fire accents;
- incomplete silver rings;
- broken restraint ring motif;
- no generic parchment theme.

MenuLiveBackground supports authored far/mid/foreground sprites later. Until final ComfyUI assets exist, it renders a procedural fallback with:

- slow autonomous camera drift;
- two moving fog layers;
- silver incomplete rings;
- warm embers;
- warm/cold color contrast.

Replacing the fallback with final layered artwork does not require changing menu logic.

## Audio

GameAudioService is ready for an authored menu track. Until one is assigned, it plays a deliberately quiet generated ambient drone plus synthesized hover/click tones. This prevents the shell from feeling completely dead while final music is missing.

## Rebuild

Manual editor action:

Keeper First Covenant -> Build Main Menu Shell

Normally no manual action is required because the builder runs once per editor session if the generated scenes are missing.
