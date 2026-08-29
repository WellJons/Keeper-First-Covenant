# Exploration and Interaction Layer

## Controls

- Left mouse button: interact with the hovered object.
- Right mouse button: inspect the hovered object when a WorldInspectable is present.
- Left/Right Alt: hold exploration focus mode.
- C: toggle party crouch outside combat.
- Shift + left mouse on a locked door: attempt force opening.

## Move-to-interact

Objects can be clicked from outside direct interaction range.
The player leader approaches the target through TacticalGrid3D and performs the captured action after reaching a valid distance.

The action remains bound to the originally clicked object even if the cursor moves while the character is walking.

## Exploration focus

ExplorationFocusHud marks nearby usable interactables and inspectable objects.

Important invariant:
HiddenDiscoverable content is never revealed by focus mode before its normal perception discovery succeeds.

This prevents the highlight system from becoming a secret-object cheat.

## Hover target presentation

WorldTargetFrameHud projects the real collider bounds of the current target into screen space and draws the Keeper-style corner frame.
No special mesh shader is required, so future generated/imported 3D assets can use the same system.

## Inspection

WorldInspectable supports:
- title;
- category;
- long description;
- optional discovery requirement;
- first-inspection WorldState flag;
- UnityEvent hook for quest/event progression.

Inspection opens a modal Keeper-styled panel and pauses world simulation while the player reads it.

## Loose pickups

WorldItemPickup represents individual items placed physically in the scene.
It:
- transfers only what the inventory can carry;
- leaves the remainder in the world;
- persists collected/remaining state through save/load;
- participates in normal interaction focus and hover UI.

## Stealth feedback

StealthAwarenessHud shows:
- crouched/standing state;
- visibility multiplier;
- movement noise radius;
- strongest enemy suspicion;
- per-enemy suspicion markers.

Crouching reduces visibility and movement noise and now also reduces exploration movement speed.

## Enemy investigation

EnemyInvestigationBrain already moves suspicious enemies toward LastStimulusPosition.
It now searches the arrival area by changing logical facing over time instead of freezing at the destination.

## Skill checks

Door lockpicking, forced entry, secret discovery, trap disarming and initiative do not use a D20 roll.

Skill checks are deterministic and compare character attributes, secondary attributes, tool bonuses and difficulty through SkillCheckResolver.
