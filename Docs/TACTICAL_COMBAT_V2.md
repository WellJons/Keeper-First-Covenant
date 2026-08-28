# Tactical Combat v2

This layer builds on Combat Loop v1 and adds actual tactical rules.

## Added

- Party companions in both Player and Ally factions are player-controlled on their initiative.
- Three-sample line-of-sight checks.
- Half and full cover penalties.
- High-ground / low-ground hit modifiers.
- Target preview with hit chance, damage range, range, cover, height modifier, line of sight and AoE target count.
- Circular in-world AoE preview.
- Opportunity attacks when leaving melee threat range.
- One reaction per combatant, refreshed on their turn and available at combat start.
- Movement is consumed per grid step so hazards can interrupt or slow a route.
- Elemental tactical surfaces: Fire, Water, Ice, Poison, Electrified, Arcane and Steam.
- Surface reactions:
  - Water + Ice -> Ice
  - Water + Electrified -> Electrified
  - Fire + Water -> Steam
  - Fire + Ice -> Water
  - Fire + Poison -> detonation + Fire
- Fire, poison, electrified and arcane surfaces deal tactical hazard damage.
- Ice consumes additional movement on entry.
- Tactical enemy actions now respect target validation and line of sight.

## Prototype abilities

Running **Install Combat Loop In Open Scene** now upgrades generated character data automatically.

### Edward

1. Sword Slash
2. Fire Burst
   - ground-targeted AoE
   - 2d6 fire
   - creates a Fire surface

### Lucian

1. Seal Bolt
2. Lightning Arc
   - targeted lightning
   - creates an Electrified surface
3. Frost Field
   - ground AoE
   - creates Ice
4. Water Rune
   - creates Water for surface-combo testing

## Useful tests

- Cast Water Rune, then Lightning Arc into the same area.
- Cast Water Rune, then Frost Field.
- Cast Fire Burst onto Ice.
- Put Fire on Poison to trigger a detonation.
- Walk away from an enemy while adjacent to test an opportunity attack.
- Put a crate or ruin between attacker and target to verify cover and LOS.
- Attack from noticeably higher or lower ground and compare the hit preview.

## Next layer

- explicit directional cover objects;
- smarter AI that searches for firing positions and avoids hazards;
- cones, lines and shaped AoE;
- shove, knockback and falling;
- concentration and channeling;
- downed and revive system;
- proper combat log;
- equipment-driven actions;
- resistances and vulnerabilities;
- stealth and detection.
