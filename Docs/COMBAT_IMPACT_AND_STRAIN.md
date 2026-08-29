# Combat Impact and Arcane Strain

## Combat impact philosophy

Damage numbers are not the presentation of a powerful spell.

CombatActionDefinition can reference a CombatPresentationProfile. The profile controls the physical and audiovisual weight of an action independently from its damage formula.

Current hooks:

- camera shake amplitude / duration / frequency;
- hit-stop duration and slow-motion scale;
- temporary impact light;
- environment physics impulse;
- impact-tier world destruction;
- world noise radius and intensity;
- cast VFX prefab hook;
- impact VFX prefab hook;
- ground decal prefab hook;
- cast / impact audio hooks.

Final art/VFX can replace the placeholders without changing combat rules.

## Impact tiers

- Subtle
- Light
- Heavy
- Devastating
- Mythic

World props can define the minimum tier required to damage them.

This means a weak spell cannot destroy a reinforced object merely because its numeric damage happens to be high.

## Prototype impact tests

### Fire Burst

Heavy impact:

- noticeable camera impulse;
- short hit-stop;
- orange impact light;
- physics impulse;
- audible at meaningful range;
- creates fire surface;
- can destroy the DEV breakable crate.

### DEV Cataclysm Fireball

Development-only stress test, not a canonical Edward ability.

- Devastating impact tier;
- 4d8 + 4 fire;
- 4.2 m AoE;
- friendly fire enabled;
- strong camera impulse;
- stronger hit-stop;
- large impact light;
- large physics impulse;
- very loud world noise;
- can break the reinforced DEV door;
- leaves a large fire surface.

It exists only to test how the game behaves when late-game magic becomes physically huge.

## Edward: Rift / Разрыв

Rift is generated as late-game development-test content and is NOT placed in Edward's starting action list.

Current prototype cost:

- 1 AP;
- 12 mana;
- 80 / 100 Arcane Strain;
- grants 15 m of free movement;
- free movement does not trigger opportunity attacks.

It is intentionally rule-breaking.

### Arcane Strain

Strain is a separate anti-spam resource. It is not normal mana and cannot be solved by simply drinking a mana potion.

Current thresholds:

- 0–49: normal;
- 50–74: strained — movement reduced;
- 75–89: severe — stronger movement reduction, -1 AP, no reaction, attack accuracy penalty;
- 90–100: critical — major movement reduction and larger accuracy penalty.

Rift immediately applies its strain consequences during the current turn.

Strain recovers slowly on the owner's turns and otherwise persists. Future rest/camp systems can define stronger recovery.

This makes Rift a clutch technique rather than a replacement for normal movement.

## F1 testing

In **F1 -> Abilities**:

- select Rift or DEV Cataclysm Fireball;
- press **Grant ability**.

Rift automatically adds an ArcaneStrainComponent if needed.

In **F1 -> Cheats**:

- current strain is shown for the preferred party member;
- **Clear strain** resets it for repeated testing.
