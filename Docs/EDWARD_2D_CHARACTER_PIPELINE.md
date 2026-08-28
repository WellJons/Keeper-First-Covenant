# Edward — production 2D character pipeline

Edward is a **layered hand-drawn 2D gameplay character**, not a 3D model and not one baked full-body illustration.

The same character is used for free-roam exploration and tactical combat.

## Locked gameplay view

- top-down / three-quarter camera;
- character exists in the world as a billboarded layered 2D rig;
- movement is on the XZ plane;
- tactical combat reuses the same world and same Edward prefab;
- no separate "battle artwork" version of the character.

## Eight gameplay facings

Runtime facings:

- North
- NorthEast
- East
- SouthEast
- South
- SouthWest
- West
- NorthWest

The first production pass authors five real directions: North, NorthEast, East, SouthEast and South. West-facing variants mirror the corresponding east-facing art. The data format already supports unique west sprites later for asymmetric equipment.

## Rig parts

The prefab is assembled from independent SpriteRenderers and transforms: head, eyes, hair back/front, torso, left/right upper arms, forearms and hands, pelvis, thighs, shins, boots, belt/accessories, three back-cloak strips, two front-cloak strips and a separate weapon socket.

This is deliberately more granular than a single cutout body. Arms and legs have real joints, eyes can blink, and the cloak has independent secondary motion.

## Current animation coverage

PaperDollMotionAnimator supports idle, breathing, random blinking, tiny idle head movement, cloak sway, walk, run, guard, light sword attack, heavy sword attack, cast/fire pose, interaction, normal hit, heavy/critical hit, death and revive/reset.

Walk and run animate legs, shins, arms, body bob, head motion and cloak lag separately. Attacks expose an AttackImpact event so combat effects can later be synchronized to the strike frame.

## Damage and blood

PaperDollBloodVisual listens to CombatantRuntime and provides instant hit blood, stronger critical-hit bursts, damage flash, persistent low-health bleeding, blood drips, a growing ground pool and death blood.

## Equipment

Current generated test equipment:

- travel sword;
- weathered greatsword;
- no weapon;
- reinforced travel leather on/off;
- worn traveler cloak on/off.

Weapons are never baked into Edward's body. Armor replaces only the paper-doll slots it actually changes.

## Fire

Edward's orange-red fire is a separate runtime FX layer: sword embers, casting-hand flame, local warm light and state-dependent intensity.

## Free-roam movement

EdwardExplorationController uses camera-relative WASD movement through a CharacterController. Shift runs; facing updates in eight directions and locomotion animation switches automatically.

The sprite rig is under a separate BillboardRoot, so camera-facing orientation does not fight attack/death transforms inside RigRoot.

## One-click production test

In Unity use:

Keeper First Covenant -> 2D Production -> Build Edward + World Kit

This creates/rebuilds:

- Assets/KeeperFirstCovenant/Generated2D/Sprites/Edward/
- Assets/KeeperFirstCovenant/Generated2D/Data/
- Assets/KeeperFirstCovenant/Generated2D/Prefabs/Edward_2D_Production.prefab
- separate world sprites/prefabs;
- Assets/KeeperFirstCovenant/Scenes/Edward_2D_Production_Test.unity

The generated art is deterministic and separated by layer so animation/equipment can be tested immediately. Final painted replacements can be dropped into the same directional slots without rewriting gameplay code.

## Test scene controls

- WASD — walk
- Shift — run
- J — light attack
- K — heavy attack
- G — guard
- C — cast/fire pose
- E — interact
- H — receive normal hit
- Y — receive critical/heavy hit
- X — cycle sword / greatsword / none
- V — toggle leather armor
- B — toggle cloak
- Delete — death
- R — revive/reset

## Art replacement rule

Final painted source art must preserve the same anchors and slot boundaries. A prettier Edward should be a replacement of sprites inside the existing rig, not a rewrite of the technical character.
