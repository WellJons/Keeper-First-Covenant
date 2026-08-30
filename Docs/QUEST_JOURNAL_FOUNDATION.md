# Quest Journal Foundation

A quest framework is now part of the game shell without imposing story canon.

## Authoring
QuestDefinition contains:
- stable questId;
- title/description;
- category: Main, Side, Companion, Exploration;
- objective list;
- required progress amount;
- optional objective flag.

## Runtime
QuestJournal is persistent across scenes and supports:
- start quest;
- add/set objective progress;
- complete objective;
- complete quest;
- fail quest;
- select tracked quest;
- runtime-created quest entries for scripted content.

Required objectives automatically complete the quest when all are done.

## Save/load
QuestJournal is serialized into SaveGameData.questStateJson.
A new game resets the journal. Loading a save restores all active/completed/failed states and objective progress.

## UI
Pause menu now contains Журнал.
It shows:
- active/completed/failed state;
- objectives and progress counters;
- tracked quest selection.

Gameplay scenes also receive a small quest tracker HUD in the upper-right.
It is hidden when there is no tracked quest and shows up to three unfinished objectives.

No prototype quest has been forced into the project yet; actual quest content should be added when its story beat becomes canon.
