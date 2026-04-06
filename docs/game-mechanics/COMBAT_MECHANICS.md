# ⚔️ Combat & Unit Mechanics

## Overview

Units and towers fight each other using a real-time combat system with target detection, movement, and delayed damage application.

---

## Unit System

### Unit Types

**Location:** `/Assets/Prefabs/Units/[Team]/`

| Unit | Blue Prefab | Red Prefab | Role |
|------|-------------|------------|------|
| Warrior | `Blue/Blue_Warrior.prefab` | `Red/Red_Warrior.prefab` | Frontline melee |
| Archer | `Blue/Blue_Archer.prefab` | `Red/Red_Archer.prefab` | Ranged damage |
| Lancer | `Blue/Blue_Lancer.prefab` | `Red/Red_Lancer.prefab` | Fast attacker |
| Monk | `Blue/Blue_Monk.prefab` | `Red/Red_Monk.prefab` | Support/healer |

### UnitCombatController

**File:** `/Assets/Scripts/Combat/UnitCombatController.cs`

**Required Components:** Animator, Rigidbody2D, SpriteRenderer, CombatTarget

#### Movement Settings

| Field | Default | Description |
|-------|---------|-------------|
| `moveSpeed` | 2.0 | Units per second |
| `stoppingDistance` | 1.1 | Distance to stop before attacking |
| `detectionRange` | 8.0 | Range to scan for enemies |
| `retargetInterval` | 0.25 | Seconds between target re-evaluation |

#### Attack Settings

| Field | Description |
|-------|-------------|
| `attackDamage` | Damage dealt per attack |
| `attackCooldown` | Time between attacks |
| `attackAnimationDuration` | How long the attack animation plays |
| `attackDamageDelay` | Delay before damage is applied (mid-animation) |

---

## Combat Flow

### State Machine

```
                    ┌─────────────────┐
                    │   IDLE / MOVE   │
                    └────────┬────────┘
                             │
                    Has target in range?
                             │
                    ┌────────▼────────┐
                    │    ATTACKING    │
                    └────────┬────────┘
                             │
                    Animation + Damage
                             │
                    ┌────────▼────────┐
                    │    COOLDOWN     │
                    └────────┬────────┘
                             │
                    Target still valid?
                    ┌──────┬─┴─┬──────┐
                    │ YES  │   │ NO   │
                    │      │   │      │
               ┌────▼──┐  │  │ ┌─────▼─────┐
               │ ATTACK│  │  │ │ RETARGET  │
               └───────┘  │  │ └───────────┘
                          │       │
                          │  ┌────▼─────┐
                          │  │  MOVE    │
                          │  └──────────┘
```

### Step-by-Step Combat

#### 1. Find Target

```csharp
FindBestTarget() {
    Scan all CombatTarget objects in scene
    Filter:
        ✅ IsEnemy(team) → teams differ && neither is Neutral
        ✅ targetType matches validTargetTypes flags
        ✅ Within detectionRange (8.0 units)
    Pick nearest target
}
```

#### 2. Move Toward Target

```csharp
direction = (target.position - transform.position).normalized
rb.linearVelocity = direction * moveSpeed
Flip sprite based on horizontal movement
```

#### 3. Check If In Range

```csharp
distance = Vector2.Distance(transform.position, target.position)
if (distance <= stoppingDistance) {
    Stop moving
    Start attack coroutine
}
```

#### 4. Attack Coroutine

```csharp
AttackCoroutine() {
    // Play attack animation
    animator.SetTrigger(AttackHash) or alternate animation
    yield WaitForSeconds(attackDamageDelay)

    // Apply damage mid-animation
    if (target still in range: distance <= stoppingDistance + 0.35f) {
        target.TakeDamage(attackDamage)
    }

    yield WaitForSeconds(attackCooldown)

    // Loop or retarget
}
```

**Key Detail:** Damage is applied with a delay (`attackDamageDelay`) to sync with the attack animation, not instantly.

---

## CombatTarget (Health System)

**File:** `/Assets/Scripts/Combat/CombatTarget.cs`

Universal health component on ALL combat entities (towers, units, castles).

### Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `team` | CombatTeam | - | Faction (Neutral/Blue/Red) |
| `targetType` | CombatTargetType | - | What is this? (Character/Building/Player) |
| `maxHealth` | float | 10 | Maximum HP |
| `currentHealth` | float (private) | maxHealth | Current HP |
| `aimPointOffset` | Vector2 | (0, 0) | Offset for projectile aiming |
| `destroyOnDeath` | int | 0 | Destroy GameObject when HP reaches 0? |

### Properties

```csharp
CurrentHealth  → currentHealth
IsAlive        → currentHealth > 0f
AimPoint       → transform.position + aimPointOffset
```

### Methods

```csharp
TakeDamage(amount) {
    currentHealth -= amount;
    if (currentHealth <= 0) {
        Fire Died event;
        if (destroyOnDeath != 0) {
            Destroy(gameObject);
        }
    }
}

IsEnemy(otherTeam) {
    return (team != otherTeam) && (team != Neutral) && (otherTeam != Neutral);
}
```

**Team Logic:**
- Blue (1) vs Red (2) → Enemies
- Blue vs Blue → Allies
- Blue vs Neutral → Not enemies
- Neutral vs anyone → Not enemies

---

## Tower Combat

### Tower as Target

Towers have `CombatTarget` with:
- `targetType = Building`
- `maxHealth = 80` (default for regular towers)
- `aimPointOffset = (0, 1.4)` — projectiles aim above tower center
- `destroyOnDeath = 1` — tower GameObject is destroyed on death

### Tower as Attacker (Battle Simulation System)

**File:** `/Assets/Others/Scripts/TowerDefense/Towers/TowerView.cs`

Towers in the simulation system auto-attack:

```csharp
Update() {
    if (alive && canAttack) {
        attackTimer -= deltaTime;
        if (attackTimer <= 0) {
            TryFire();
            attackTimer = 1f / attackSpeed;
        }
    }
}

TryFire() {
    Find nearest enemy UnitView in range:
        Physics.OverlapSphere(position, attackRange)
        Filter: enemy faction
        Pick nearest

    If target found:
        Instantiate projectile at firePoint
        projectile.SetTarget(target)
        projectile.SetDamage(damage)
        animator.SetTrigger("Fire")
}
```

### Tower Stats (Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Data/TowerData.cs`

| Field | Description |
|-------|-------------|
| `towerID` | Unique identifier |
| `displayName` | UI name |
| `faction` | Sunnybottom or Mudsnout |
| `maxHealth` | Tower HP |
| `damage` | Damage per attack |
| `attackSpeed` | Attacks per second |
| `attackRange` | Detection range |
| `projectilePrefab` | Projectile to fire |

**Enemy Tower Configuration:**
```
maxHealth = 800
damage = 10
attackSpeed = 0.4
attackRange = 7
```

---

## Projectile System

### Projectile (Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Towers/Projectile.cs`

| Field | Default | Description |
|-------|---------|-------------|
| `speed` | 10 | Movement speed |
| `damage` | 10 | Damage on hit |
| `trailEffect` | ParticleSystem | Visual trail |

```csharp
Update() {
    if (has valid target) {
        direction = (target.position - transform.position).normalized
        transform.position += direction * speed * deltaTime
        transform.LookAt(target)

        if (distance < 0.5) {
            HitTarget();
        }
    } else {
        // No target - fly straight
        if (distance from origin > 50) {
            Destroy self;
        }
    }
}

HitTarget() {
    target.PlayHitEffect();
    Spawn trail effect;
    Destroy self;
}
```

---

## Unit Movement (Battle Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/UnitSimulation.cs`

### Lane-Based Movement

```
Player units (Sunnybottom) → Move +X (right)
Enemy units (Mudsnout)     → Move -X (left)

Lane model:
    PlayerSpawnX = -14
    EnemySpawnX  =  14
    LaneLength   =  30
```

### Unit Update Loop

```csharp
Update(deltaTime) {
    if (IsAttacking) {
        // Check if still in range
        if (target in range) {
            Attack target;
        } else {
            IsAttacking = false;
        }
    } else {
        // Look for enemies in range
        enemy = FindNearestEnemy(units + towers);
        if (enemy in range) {
            Start attacking;
        } else {
            // Move toward enemy spawn position
            Move toward enemy side;
        }
    }
}
```

---

## Combat Resolution (Battle Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/BattleSimulation.cs`

Each frame, `ResolveCombat()` runs:

### Phase 1: Player Units Attack

```
For each playerUnit:
    1. Check for enemy units in range → Deal damage
    2. If no units, check enemy towers in range → Deal damage (with towerDamageMultiplier)
    3. If no towers, check enemy structures in range → Deal damage
```

### Phase 2: Enemy Units Attack

```
For each enemyUnit:
    1. Check for player units in range → Deal damage
    2. If no units, check player towers in range → Deal damage (with towerDamageMultiplier)
    3. If no towers, check player structures in range → Deal damage
```

### Phase 3: Player Towers Attack

```
For each playerTower:
    Find nearest enemy unit in range (1D distance on X axis)
    If found → Deal damage
```

---

## Castle Mechanics

### Castle Game Over Controller

**File:** `/Assets/Scripts/Buildings/CastleGameOverController.cs`

Monitors both castles:

```csharp
Fields:
    playerCastle: CombatTarget
    enemyCastle: CombatTarget

Update() {
    if (playerCastle.IsAlive && enemyCastle.IsAlive) return;

    if (!playerCastle.IsAlive) {
        Show "Game Over - Your castle was destroyed";
        Time.timeScale = 0f;
    }
    if (!enemyCastle.IsAlive) {
        Show "Victory - Enemy castle destroyed";
        Time.timeScale = 0f;
    }
}
```

**Castles are NOT placeable** (`isCastle = true` in catalog entry).

---

## Damage Types & Multipliers

### Standard Damage

```
unit.attackDamage → target.TakeDamage(amount)
```

### Tower Damage (from Units)

```
unit.attackDamage * unit.towerDamageMultiplier → tower.TakeDamage(amount)
```

### Phase Multiplier (Battle Simulation)

```
During Tea Time (matching faction):  1.15x damage
During Tantrum Time (opposing):      0.9x damage
```

---

## Unit Roles (Battle Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Data/UnitData.cs`

| Role | Description |
|------|-------------|
| **Frontliner** | Melee, high health, tank damage |
| **Ranged** | Attacks from distance, lower health |
| **Breaker** | Specialized in breaking defenses |
| **Support** | Buffs/heals allies |
| **Siege** | High tower damage multiplier |
| **Skirmisher** | Fast, hit-and-run tactics |

### UnitStats

```csharp
struct UnitStats {
    maxHealth;              // HP
    damage;                 // Damage per attack
    attackSpeed;            // Attacks per second
    attackRange;            // Detection range
    moveSpeed;              // Movement speed
    towerDamageMultiplier;  // Multiplier vs towers
}
```

---

## Team Alignment

### CombatTeam Enum

```csharp
CombatTeam.Neutral = 0  // Neutral factions, never hostile
CombatTeam.Blue    = 1  // Player team
CombatTeam.Red     = 2  // Enemy team
```

### Enemy Determination

```csharp
CombatTarget.IsEnemy(otherTeam) {
    return (this.team != otherTeam)
        && (this.team != CombatTeam.Neutral)
        && (otherTeam != CombatTeam.Neutral);
}
```

**Examples:**
- Blue vs Red → ✅ Enemy
- Blue vs Blue → ❌ Ally
- Blue vs Neutral → ❌ Not enemy
- Neutral vs Red → ❌ Not enemy

---

## Slow / Debuff System

**File:** `/Assets/Others/UnityTechnologies/TowerDefenseTemplate/Scripts/TowerDefense/Affectors/SlowAffector.cs`

```
Tower with SlowAffector:
    When enemy enters range:
        Attach AgentSlower component to enemy
        Apply slowFactor (0-1, normalized slowdown)
        Play particle + audio effect

AgentSlower:
    Maintains list of active slow effects
    Applies strongest slow (minimum factor):
        newSpeed = originalSpeed * min(slowFactors)

    On RemoveSlow():
        Remove factor
        If no effects remain → restore originalSpeed

    On agent death:
        Cleanup and destroy
```

---

## Target Selection Priority

### Units (Custom System)

```
1. Find all CombatTargets in detectionRange
2. Filter: IsEnemy(team) && targetType matches
3. Sort by distance
4. Pick nearest
5. Re-evaluate every retargetInterval (0.25s)
```

### Towers (Simulation System)

```
1. Physics.OverlapSphere(position, attackRange)
2. Filter: enemy faction
3. Pick nearest UnitView
4. Fire projectile
```

---

## Attack Affector Types (Unity Template)

**Location:** `/Assets/Others/UnityTechnologies/TowerDefenseTemplate/Scripts/TowerDefense/Affectors/`

| Type | File | Behavior |
|------|------|----------|
| **AttackAffector** | `AttackAffector.cs` | Main tower attack controller, fires projectiles |
| **SlowAffector** | `SlowAffector.cs` | Slows enemies in range |

### Launcher Types

**Location:** `/Assets/Others/UnityTechnologies/TowerDefenseTemplate/Scripts/TowerDefense/TowerLaunchers/`

| Launcher | File | Behavior |
|----------|------|----------|
| **Launcher** (abstract) | `Launcher.cs` | Base class for single/multi-target |
| **BallisticLauncher** | `BallisticLauncher.cs` | Parabolic arc projectiles with gravity |
| **HomingLauncher** | `HomingLauncher.cs` | Homing missiles that track targets |
| **HitscanLauncher** | `HitscanLauncher.cs` | Instant-hit (no travel time) |
| **SuperTowerLauncher** | `SuperTowerLauncher.cs` | Homing with life timer + rotating fire vector |

### Projectile Types

| Projectile | File | Behavior |
|-----------|------|----------|
| **LinearProjectile** | `LinearProjectile.cs` | Straight-line movement |
| **HomingLinearProjectile** | `HomingLinearProjectile.cs` | In-flight target tracking |
| **BallisticProjectile** | `BallisticProjectile.cs` | Parabolic arc trajectory |
| **HitscanAttack** | `HitscanAttack.cs` | Instant damage with delay |
| **SplashDamager** | `SplashDamager.cs` | AOE damage on collision |

---

## Quick Reference

### Default Tower Stats

| Property | Value |
|----------|-------|
| maxHealth | 80 |
| snackCost | 100 |
| sellRefund | 50 (50%) |
| aimPointOffset | (0, 1.4) |
| targetType | Building |

### Default Castle Stats

| Property | Value |
|----------|-------|
| maxHealth | 800 (simulation) |
| targetType | Building |
| destroyOnDeath | 0 (keep for game over) |

### Default Unit Stats (Custom System)

| Property | Value |
|----------|-------|
| moveSpeed | 2.0 |
| stoppingDistance | 1.1 |
| detectionRange | 8.0 |
| retargetInterval | 0.25 |

---

*Last updated: April 4, 2026*
