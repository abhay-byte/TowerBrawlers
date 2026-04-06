# 06. Game Mechanics

## 1. Core Gameplay Loop

1. Enter a mission with selected faction, items, and loadout.
2. Start generating Snacks and building Drama.
3. Spend Snacks on troops and structures.
4. Hold the lane while reading enemy composition.
5. Time pushes around the current battlefield phase.
6. Use abilities to swing fights or protect your tower.
7. Break enemy defenses and destroy their main tower.
8. Earn Coins, items, and unlocks.

## 2. Core Win and Loss Conditions

### Win

- destroy enemy main tower
- complete special mission objective if one exists

### Loss

- lose player main tower
- fail required mission objective

## 3. Battlefield Flow

The MVP uses a single lane.

Units:

- spawn from base
- move automatically
- attack enemies in range
- prioritize according to role rules

The player does not control units directly after deployment.

## 4. Core Systems

### 4.1 Deployment

- units are deployed by spending Snacks
- deployment can be instant or queued depending on unit type
- each card has a cost and optional cooldown

### 4.2 Structures

- placed on fixed slots
- separated into economy, defense, and utility roles
- can be damaged and destroyed

### 4.3 Abilities

- cost Drama
- serve as battle swing tools
- should not replace good composition

### 4.4 Progression

- unlocks after missions
- new troops, structures, and item options
- gradual complexity increase

## 5. Battlefield Phase Mechanic

The original serious concept used day and night. For the parody version, the same mechanical role can remain while the presentation becomes sillier.

Recommended phase system:

## Tea Time vs Tantrum Time

### Tea Time

- favors the Sunnybottom Empire
- stronger tower efficiency
- better support and defense effects
- slightly calmer battlefield pacing

### Tantrum Time

- favors the Mudsnout Horde
- stronger rush and aggression effects
- higher pressure from swarm units
- noisier visual and audio presentation

### Cycle Rules

- cycle changes every 90 seconds in standard missions
- 15-second warning before shift
- bonuses visible in HUD
- some missions alter the cycle duration

## 6. Combat Rules

### Basic Combat

- units attack automatically
- attack speed, damage, range, and health define baseline role
- special traits add faction identity

### Damage Types

Recommended simple damage groups:

- normal
- structure
- splash
- morale

### Status Effects

Recommended early effects:

- slow
- stun
- burn
- buff
- debuff

## 7. Tower Pressure

The game only works if towers feel threatened.

Rules:

- siege units deal meaningful tower damage
- tower defenses can stall but not permanently solve pressure
- late-game pushes should end battles instead of dragging forever

## 8. Economy Rules

- players gain passive Snacks over time
- economy structures increase generation
- overcommitting to economy creates vulnerability
- Drama meter grows from battlefield events

## 9. AI Rules

- AI uses faction-themed behavior
- AI should telegraph large pushes
- AI should not hard-counter the player with hidden information
- difficulty should improve timing and composition rather than just inflate stats

## 10. Mission Structure

Recommended mission types:

- basic assault
- holdout
- siege break
- escort
- boss fortress

## 11. Progression Structure

After missions, players can:

- earn Coins
- unlock units
- unlock structures
- equip items
- improve faction-specific upgrades

## 12. Difficulty Philosophy

- easy mode: forgiving economy and slower enemy pressure
- normal mode: baseline intended experience
- hard mode: stronger enemy timing, better mixed waves, tighter mission rules

Avoid:

- giant health inflation
- unreadable chaos
- unfair surprise mechanics without telegraphing

## 13. Balance Principles

- every troop must have a purpose
- every push must have an answer
- economy greed should be punishable
- timing should matter but not dominate everything
- comedy should not reduce clarity

## 14. MVP Mechanics Checklist

- one lane
- troop deployment
- resource generation
- one phase cycle
- one active ability per faction set
- tower destruction
- simple structure building
- mission results and rewards
