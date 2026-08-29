# Branching Dialogue Foundation

Dialogue is now a data-driven gameplay system.

DialogueDefinition supports:
- stable node IDs;
- speaker ID/name;
- optional portraits;
- linear continuation;
- conditional branches;
- effects on entering nodes and selecting choices.

Conditions can test:
- world flags;
- world numeric values;
- quest active/completed/failed state.

Effects can:
- set/clear world flags;
- set/add world values;
- start quests;
- progress/complete objectives;
- complete/fail quests.

DialogueRunner pauses world time, releases the cursor, filters choices and restores the previous gameplay state when the conversation ends.

DialogueUiController provides the actual game UI with speaker, portrait slot, dialogue text, numbered choices, keyboard 1-8 selection and Enter/Space continuation.

DialogueInteractable plugs into the existing IInteractable world system.

No story dialogue content is hardcoded here.
