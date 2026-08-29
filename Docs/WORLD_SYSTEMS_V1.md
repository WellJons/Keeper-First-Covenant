# World Systems v1

The world and combat use one continuous scene.

## Detection

Enemy PerceptionSensor supports:

- vision distance;
- field of view;
- line of sight;
- day/night visibility;
- movement noise;
- explicit world-noise events;
- suspicion accumulation;
- suspicion decay.

Awareness states:

1. Unaware
2. Suspicious
3. Alerted

A suspicious enemy moves toward the last seen/heard position before full combat begins.

## Loud combat and reinforcements

Combat does not create a silent bubble.

Enemies that are not yet in the current initiative can continue to see/hear the fight.

Loud magic can therefore pull additional enemies into a running battle through TurnCombatDirector.AddParticipant.

Impact profiles define their world noise.

## Stealth

Press **C** outside combat to toggle the current party's crouched state.

Only party members that actually exist are affected.

Crouching currently reduces:

- visibility signature;
- movement noise radius.

No companion is required.

## Optional companions in exploration

PartyFollowController follows the Player-faction leader with whichever Ally combatants are currently present.

At combat start all current movement is cancelled and everyone remains at their exact world position.

## Logical facing

WorldFacing stores gameplay-facing direction independently from visual billboard/sprite orientation.

Movement updates logical facing.

Enemy vision cones use logical facing.

This allows future 2.5D character visuals to face the camera correctly without breaking stealth mechanics.

## World time

WorldTimeSystem provides:

- game hour;
- day counter;
- adjustable time speed;
- day/night visibility multiplier;
- time freeze during turn-based combat by default.

WorldLightingController rotates the directional light and adjusts sun/ambient intensity.

Enemy vision range is reduced at night and transitional at dawn/dusk.

## Doors and locks

LockableDoor supports:

- unlocked open/close;
- required key item id;
- optional key consumption;
- lockpicking;
- force opening;
- different noise levels.

Prototype controls:

- normal click: open / use key / attempt lockpick;
- Shift + click: attempt noisy force opening.

Opening/closing rebuilds hidden navigation.

## Destructible environment

EnvironmentalDestructible supports:

- integrity;
- minimum ImpactTier;
- force-scaled impact damage;
- collider release;
- optional rigidbody release;
- UnityEvent destruction hook.

Destroying an obstacle rebuilds navigation.

## Hidden discoveries

HiddenDiscoverable performs a secret perception check for each nearby current party member.

If nobody succeeds, the object remains unknown.

On success:

- renderers/colliders are revealed;
- attached traps become interactable;
- navigation is refreshed if physical geometry appears.

## Traps

TrapMechanism supports:

- hidden/revealed state;
- disarm difficulty;
- optional tool requirement;
- dice-based damage;
- noise when triggered;
- one-shot or reusable state;
- optional elemental surface creation.

Failed disarm attempts can trigger the trap.

## Generated DEV test zone

The prototype installer creates:

- a locked door;
- DEV Road Ruin Key;
- DEV Lockpick;
- a Heavy-tier breakable crate;
- a loose physics object;
- a hidden loot cache;
- a hidden trap.

These objects are mechanical test content, not final level design.

## F1 World tab

The developer sandbox shows:

- world time and visibility multiplier;
- dawn/noon/dusk/night shortcuts;
- party stealth state;
- enemy awareness and suspicion;
- enemy distance;
- door state;
- destructible integrity.

Debug actions:

- emit 5 m quiet noise;
- emit 15 m loud noise;
- emit 30 m massive noise;
- reveal hidden objects;
- reset enemy awareness.
