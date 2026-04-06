# 🏰 Tower Defense - Game Mechanics Documentation

## Table of Contents
1. [Game Overview](#game-overview)
2. [Core Architecture](#core-architecture)
3. [Building Placement System](#building-placement-system)
4. [Snack Generation System](#snack-generation-system) ⭐ NEW
5. [Tower Mechanics](#tower-mechanics)
6. [Unit & Combat System](#unit--combat-system)
7. [Enemy & Wave System](#enemy--wave-system)
8. [Economy System](#economy-system)
9. [Map & Level Design](#map--level-design)
10. [Game Flow & Win Conditions](#game-flow--win-conditions)
11. [Quick Reference Tables](#quick-reference-tables)

---

## Game Overview

**Type:** Lane-based Tower Defense with real-time combat  
**Perspective:** 2D Top-down / Isometric  
**Core Loop:** Place towers → Defend castle → Destroy enemy castle  

### Game Objective
- **WIN:** Destroy the enemy castle (Red team)
- **LOSE:** Your castle (Blue team) is destroyed

---

## Core Architecture

The project contains **three parallel systems**:

| System | Location | Purpose |
|--------|----------|---------|
| **Custom Building System** | `/Assets/Scripts/` | Main game: drag-and-drop tower placement, unit combat |
| **Battle Simulation** | `/Assets/Others/Scripts/TowerDefense/` | Data-driven simulation: waves, units, towers, economy |
| **Unity Template** | `/Assets/Others/UnityTechnologies/` | Reference template (not actively used) |

### Key Enums

```csharp
// CombatTeam.cs
CombatTeam.Neutral = 0
CombatTeam.Blue    = 1  // Player
CombatTeam.Red     = 2  // Enemy

// BuildingSlotCategory.cs (Flags)
BuildingSlotCategory.None     = 0
BuildingSlotCategory.Building = 1
BuildingSlotCategory.Tower    = 2
BuildingSlotCategory.Any      = 3  // Building | Tower

// CombatTargetType.cs (Flags)
CombatTargetType.None     = 0
CombatTargetType.Character = 1
CombatTargetType.Building  = 2
CombatTargetType.Player    = 4
CombatTargetType.Any       = 7  // All flags
```

---

## Building Placement System

### How It Works

**Towers can ONLY be placed on pre-defined BuildSlot GameObjects** in the scene. There is no freeform grid placement.

### Component Flow

```
BuildingPlacementController (HUD)
    ↓ manages
BuildingCatalogEntry[] (available buildings)
    ↓ creates
BuildingDragCardUI (drag cards)
    ↓ drag & drop onto
BuildSlot (scene placement points)
    ↓ validates & spawns
PlacedBuilding (runtime component)
```

### BuildSlot Component

**File:** `/Assets/Scripts/Buildings/BuildSlot.cs`

Each buildable position in the scene has a `BuildSlot` with:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `ownerTeam` | CombatTeam | - | Which team owns this slot (Blue/Red) |
| `placementAnchor` | Transform | self | Where the building spawns (override) |
| `allowPlayerBuilding` | bool | true | Can player build here? |
| `allowAIBuilding` | bool | true | Can AI build here? |
| `allowSelling` | bool | true | Can buildings be sold? |
| `allowedCategories` | BuildingSlotCategory | Any | What can be placed? |

**Requirements:** Must have a `Collider2D` (set as trigger)

### Placement Validation (`CanPlace()`)

A building can be placed if ALL conditions are met:

1. ✅ Catalog entry is not null
2. ✅ Entry is not a castle (`entry.isCastle == false`)
3. ✅ Slot is empty (`currentBuilding == null`)
4. ✅ Entry has a prefab for this slot's team
5. ✅ Entry's `slotCategory` matches slot's `allowedCategories`
6. ✅ Permission: player or AI is allowed to build here
7. ✅ Wallet has enough snacks (`wallet.TrySpend(snackCost)`)

### Drag & Drop Flow

```
1. Player clicks building card in HUD
2. OnBeginDrag → BuildingPlacementController.BeginDrag(entry)
   - Creates UI preview (84x84px Image)
3. OnDrag → Move preview with cursor
4. OnEndDrag → Find slot under pointer
   - Physics2D.OverlapPoint(worldPosition)
   - Calls slot.TryPlace(entry, wallet, isAIBuilder: false)
5. If valid → Deduct snacks → Instantiate building
6. If invalid → Preview disappears, no placement
```

**Important:** The placement preview is a **2D UI image**, NOT a ghost tower in the game world.

### Build Slots in Level1 Scene

**BlueBuildSlots** (ownerTeam: Blue):
- 5+ slots positioned on player's side of the map

**RedBuildSlots** (ownerTeam: Red):
- 5+ slots positioned on enemy's side of the map

**Team Restriction:** Blue slots only accept Blue buildings, Red slots only accept Red buildings.

---

## Snack Generation System

Buildings like Houses can generate snacks passively over time, providing income for both Player and AI.

### Components

| Script | Purpose |
|--------|---------|
| `SnackGenerator` | Core generation logic, works with any building |
| `House` | Specialized building wrapper for snack generation |
| `HouseSnackGenerationBar` | World-space UI bar showing the current snack cycle |
| `BuildingHealthBar` | World-space UI bar showing building HP |

### How It Works

```
House Placed → PlacedBuilding.Initialize() → SnackGenerator activated
Every tickInterval seconds:
    wallet.AddSnacks(snacksPerTick)
```

### Default Rates

| Setting | Value | Result |
|---------|-------|--------|
| snacksPerTick | 1 | 1 snack per tick |
| tickInterval | 2.0s | 1 tick every 2 seconds |
| **Rate** | **0.5 snacks/sec** | 30 snacks/minute |

### Team Support

- **Automatic**: Detects owner from BuildSlot.ownerTeam
- **Blue team** → Player income
- **Red team** → AI income
- **Current project state:** Blue and Red generators both write into the active `SnackWallet` found in-scene
- Can be extended later to separate wallets if needed

### Setup

1. Add `SnackGenerator` + `House` components to house prefabs
2. Buildings get `BuildingHealthBar` at runtime during placement, and houses also get `HouseSnackGenerationBar`
3. Configure generation rate
4. Add to building catalog
5. Place on valid BuildSlot

**Full guide:** [SNACK_GENERATION.md](./SNACK_GENERATION.md)

---

## Tower Mechanics

### Tower Prefabs

**Location:** `/Assets/Prefabs/Buildings/[Team]_Buildings/Tower.prefab`

| Team | Folder | CombatTarget.team |
|------|--------|-------------------|
| Blue | `Blue_Buildings/` | 1 (Blue) |
| Red | `Red_Buildings/` | 2 (Red) |
| Neutral | `Black_Buildings/`, `Purple_Buildings/`, `Yellow_Buildings/` | 0 (Neutral) |

### Tower Components (on prefabs)

| Component | Purpose |
|-----------|---------|
| `SpriteRenderer` | Visual appearance |
| `BoxCollider2D` | Size: {1.4575, 1.088125}, Offset: {0.015625, 0.5440625} |
| `CombatTarget` | Health & targeting (maxHealth: 80) |
| `YSortSpriteRenderer` | Y-axis sorting for rendering order |
| `BuildingHealthBar` | HP bar displayed above the building |

### BuildingCatalogEntry

**File:** `/Assets/Scripts/Buildings/BuildingCatalogEntry.cs`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `buildingId` | string | - | Unique identifier |
| `displayName` | string | "Tower" | UI name |
| `snackCost` | int | 50 (HUD: 100) | Cost to build |
| `sellRefundMultiplier` | float | 0.5 | Refund ratio (50%) |
| `isCastle` | bool | false | Is this a castle? |
| `availableForPlayer` | bool | true | Can player build? |
| `availableForAI` | bool | true | Can AI build? |
| `slotCategory` | BuildingSlotCategory | Tower | Where can it go? |
| `blueTeamPrefab` | GameObject | - | Blue variant |
| `redTeamPrefab` | GameObject | - | Red variant |

### PlacedBuilding Component

Attached to instantiated buildings at runtime:

- Stores `buildingId`, `snackCost`, `sellRefundMultiplier`
- Sets `CombatTarget.Team` and `CombatTarget.TargetType` on init
- Adds `BuildingHealthBar` automatically when missing
- Initializes `SnackGenerator` and adds `HouseSnackGenerationBar` when the building generates snacks
- On destruction: notifies owner `BuildSlot` to clear reference

### Selling Buildings

```csharp
TrySell() {
    refundAmount = RoundToInt(snackCost * sellRefundMultiplier); // 50%
    wallet.AddSnacks(refundAmount);
    Destroy building GameObject;
    Clear slot.currentBuilding reference;
}
```

---

## Unit & Combat System

### Unit Prefabs

**Location:** `/Assets/Prefabs/Units/[Team]/`

| Unit Type | Blue Prefab | Red Prefab |
|-----------|-------------|------------|
| Warrior | `Blue/Blue_Warrior.prefab` | `Red/Red_Warrior.prefab` |
| Archer | `Blue/Blue_Archer.prefab` | `Red/Red_Archer.prefab` |
| Lancer | `Blue/Blue_Lancer.prefab` | `Red/Red_Lancer.prefab` |
| Monk | `Blue/Blue_Monk.prefab` | `Red/Red_Monk.prefab` |

### UnitCombatController

**File:** `/Assets/Scripts/Combat/UnitCombatController.cs`

Controls unit AI movement and combat:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `moveSpeed` | float | 2.0 | Movement speed (units/sec) |
| `stoppingDistance` | float | 1.1 | Distance to stop before attacking |
| `detectionRange` | float | 8.0 | Range to scan for enemies |
| `retargetInterval` | float | 0.25 | How often to re-evaluate targets |
| `attackDamage` | float | - | Damage per attack |
| `attackCooldown` | float | - | Time between attacks |
| `attackAnimationDuration` | float | - | Attack animation length |
| `attackDamageDelay` | float | - | Delay before damage applies (mid-animation) |

**Required Components:** Animator, Rigidbody2D, SpriteRenderer, CombatTarget

### Combat Flow

```
1. FindBestTarget() → Scan all CombatTarget objects
   - Filter: IsEnemy(team) && targetType matches
   - Pick nearest within detectionRange

2. Movement → Move toward target
   - rb.linearVelocity = direction * moveSpeed
   - Flip sprite based on movement direction

3. In Range? (distance <= stoppingDistance)
   - YES → Stop, start attack coroutine
   - NO → Continue moving

4. Attack Coroutine
   - Play attack animation (attackAnimationDuration)
   - Wait attackDamageDelay
   - Check target still in range (tolerance: stoppingDistance + 0.35f)
   - Apply damage: target.TakeDamage(attackDamage)
   - Wait attackCooldown
   - Repeat or retarget
```

### CombatTarget (Health System)

**File:** `/Assets/Scripts/Combat/CombatTarget.cs`

Universal health component on towers, units, castles:

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `team` | CombatTeam | - | Faction alignment |
| `targetType` | CombatTargetType | - | What is this? |
| `maxHealth` | float | 10 | Maximum HP |
| `aimPointOffset` | Vector2 | (0, 0) | Visual offset for projectiles |
| `destroyOnDeath` | int | 0 | Destroy GameObject on death? |

```csharp
TakeDamage(amount) {
    currentHealth -= amount;
    if (currentHealth <= 0) {
        Fire Died event;
        if (destroyOnDeath != 0) Destroy(gameObject);
    }
}

IsEnemy(otherTeam) {
    return teams differ && neither is Neutral;
}
```

---

## Enemy & Wave System

### Two Enemy Systems

The project has two distinct enemy systems:

#### System A: Custom Unit Combat (Active)
- Uses `UnitCombatController` for target-seeking behavior
- Enemies dynamically move toward nearest hostile target
- **No predefined paths** - enemies react to the battlefield

#### System B: Battle Simulation (Data-driven)
- Uses `UnitSimulation` + `UnitView`
- Wave-driven spawning with `WaveManager`
- Lane-based movement (units move toward enemy spawn X)

### Wave System (Battle Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/WaveManager.cs`

```
Wave Structure:
├── WaveData
│   ├── waveID: string
│   └── entries[]
│       ├── unitData: UnitData
│       ├── count: int
│       ├── delayBetweenSpawns: float
│       └── delayBeforeWave: float
└── waveSpacing: float (default: 15s between waves)
```

**Wave Flow:**
```
For each wave:
    Wait delayBeforeWave
    For each entry:
        Spawn 'count' units of unitData
        Wait delayBetweenSpawns between each
    Wait waveSpacing before next wave
```

**Enemy Faction:** All waves spawn as `Faction.Mudsnout` (enemy)

### Enemy Movement (Battle Simulation)

- **Player units (Sunnybottom):** Move +X (right, toward enemy)
- **Enemy units (Mudsnout):** Move -X (left, toward player)
- Stop when enemy unit/tower/structure is in attack range
- Lane model: `PlayerSpawnX = -14`, `EnemySpawnX = 14`, `LaneLength = 30`

---

## Economy System

### SnackWallet

**File:** `/Assets/Scripts/Buildings/SnackWallet.cs`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `startingSnacks` | int | 200 | Initial currency |
| `CurrentSnacks` | property | - | Current amount |

```csharp
CanAfford(amount)  → return CurrentSnacks >= amount;
TrySpend(amount)   → if affordable: deduct, fire SnacksChanged, return true
AddSnacks(amount)  → add currency (used on sell/refund)
SetSnacks(amount)  → force set (debug/cheats)
```

### ResourceManager (Battle Simulation)

**File:** `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/ResourceManager.cs`

| Resource | Max | Generation | Description |
|----------|-----|------------|-------------|
| `Snacks` | 999 | 5/sec (default) | Primary currency |
| `DramaMeter` | 100 | - | Secondary resource (abilities) |

```csharp
Auto-generates snacks each second based on SnackGenerationRate
Economy structures can increase generation rate
TrySpendSnacks(amount) → deducts if affordable, fires OnSnacksChanged
```

### Phase Cycle

**File:** `/Assets/Others/Scripts/TowerDefense/Core/PhaseCycle.cs`

Alternates between two phases every `cycleDuration` (default: 90s):

| Phase | Effect |
|-------|--------|
| **Tea Time** | Buff for matching faction |
| **Tantrum Time** | Debuff for opposing faction |

```csharp
GetPhaseMultiplier(isMatchingFaction):
    Tea Time + matching → 1.15x (buff)
    Tantrum Time + opposing → 0.9x (debuff)
```

---

## Map & Level Design

### Scene Structure

**Main Scene:** `/Assets/Scenes/Level1.unity`

### Tilemap Terrain

**Grid Component:**
- CellSize: `(1, 1, 1)` - 1 unit per cell
- CellLayout: Rectangular
- CellGap: `(0, 0, 0)`

**Layers (11 total):**
| Layer | Purpose |
|-------|---------|
| Layer 2 - Collision | `TilemapCollider2D` + `CompositeCollider2D` (obstacles) |
| Layer 7 - Shadow | Shadow casting |
| Multiple visual layers | Ground, decorations, details |

### No Freeform Placement

Towers can **ONLY** be placed on pre-existing `BuildSlot` GameObjects. Each slot has:

- A `BoxCollider2D` trigger defining the placement area
- An `ownerTeam` restricting which faction can build
- An `allowedCategories` bitmask filtering building types

### Build Area Layout

```
[Blue Side] ←→ [No-Man's Land] ←→ [Red Side]
   ↓                                    ↓
BlueBuildSlots                     RedBuildSlots
(5+ slots)                         (5+ slots)
ownerTeam: Blue (1)                ownerTeam: Red (2)
```

### Castles

- **Player Castle:** Blue team castle (starting position)
- **Enemy Castle:** Red team castle (starting position)
- Monitored by `CastleGameOverController`
- **NOT placeable** via catalog (`isCastle = true`)

---

## Game Flow & Win Conditions

### Game Startup

```
1. GameLauncher.Start()
   ↓
2. Load LevelData from Resources/AssetDatabase
   ↓
3. Find BattleManager, inject LevelData via reflection
   ↓
4. Wait 1.5 seconds
   ↓
5. StartBattle()
   ↓
6. Create BattleSimulation
   ↓
7. Initialize towers, units, WaveManager, StructurePlacement
   ↓
8. Set state to Playing
   ↓
9. Start WaveManager coroutines
```

### Win/Lose Conditions

#### System A: Castle Game Over
**File:** `/Assets/Scripts/Buildings/CastleGameOverController.cs`

```csharp
Monitor playerCastle and enemyCastle CombatTarget components:

if (playerCastle.died) {
    Show "Game Over - Your castle was destroyed";
    Time.timeScale = 0f; // Freeze game
}
if (enemyCastle.died) {
    Show "Victory - Enemy castle destroyed";
    Time.timeScale = 0f;
}
```

#### System B: Battle Simulation Victory
**File:** `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/BattleSimulation.cs`

```csharp
CheckVictoryConditions():
    if (any TowerSimulation.IsDestroyed) {
        if (enemy tower destroyed) → Victory
        if (player tower destroyed) → Defeat
        Fire OnBattleEnded event
        Set Time.timeScale = 0
    }
```

---

## Quick Reference Tables

### Tower Stats (Default Catalog Entry)

| Property | Value |
|----------|-------|
| buildingId | "tower" |
| displayName | "Tower" |
| snackCost | 100 |
| sellRefundMultiplier | 0.5 (refund = 50 snacks) |
| slotCategory | 2 (Tower) |
| CombatTarget.maxHealth | 80 |
| CombatTarget.aimPointOffset | (0, 1.4) |

### Castle Stats (Typical)

| Property | Value |
|----------|-------|
| maxHealth | 800 (simulation) / varies |
| targetType | Building |
| destroyOnDeath | 0 (keep for game over screen) |

### Unit Roles (Battle Simulation)

| Role | Description |
|------|-------------|
| Frontliner | Melee, tanky |
| Ranged | Attacks from distance |
| Breaker | Breaks through defenses |
| Support | Aids other units |
| Siege | High tower damage |
| Skirmisher | Fast, hit-and-run |

### Key File Paths

| Purpose | Path |
|---------|------|
| **Building Placement Controller** | `/Assets/Scripts/Buildings/BuildingPlacementController.cs` |
| **Build Slot** | `/Assets/Scripts/Buildings/BuildSlot.cs` |
| **Catalog Entry** | `/Assets/Scripts/Buildings/BuildingCatalogEntry.cs` |
| **Placed Building** | `/Assets/Scripts/Buildings/PlacedBuilding.cs` |
| **Snack Wallet** | `/Assets/Scripts/Buildings/SnackWallet.cs` |
| **Combat Target** | `/Assets/Scripts/Combat/CombatTarget.cs` |
| **Unit Combat AI** | `/Assets/Scripts/Combat/UnitCombatController.cs` |
| **Castle Game Over** | `/Assets/Scripts/Buildings/CastleGameOverController.cs` |
| **AI Building Placer** | `/Assets/Scripts/Buildings/AIBuildingPlacer.cs` |
| **Battle Simulation** | `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/BattleSimulation.cs` |
| **Battle Manager** | `/Assets/Others/Scripts/TowerDefense/Battle/Presentation/BattleManager.cs` |
| **Wave Manager** | `/Assets/Others/Scripts/TowerDefense/Battle/Simulation/WaveManager.cs` |
| **Tower View** | `/Assets/Others/Scripts/TowerDefense/Towers/TowerView.cs` |
| **Tower Data** | `/Assets/Others/Scripts/TowerDefense/Data/TowerData.cs` |
| **Unit Data** | `/Assets/Others/Scripts/TowerDefense/Data/UnitData.cs` |
| **Wave Data** | `/Assets/Others/Scripts/TowerDefense/Data/WaveData.cs` |
| **Level Data** | `/Assets/Others/Scripts/TowerDefense/Data/LevelData.cs` |
| **Building HUD Prefab** | `/Assets/Prefabs/UI/BuildingPlacementHUD.prefab` |
| **Tower Prefabs** | `/Assets/Prefabs/Buildings/[Team]_Buildings/Tower.prefab` |
| **Unit Prefabs** | `/Assets/Prefabs/Units/[Team]/[Type].prefab` |
| **Main Scene** | `/Assets/Scenes/Level1.unity` |
| **Troop Purchase Controller** | `/Assets/Scripts/Combat/TroopPurchaseController.cs` |
| **AI Troop Buyer** | `/Assets/Scripts/Combat/AITroopBuyer.cs` |

---

## Troop Purchase System ⭐ NEW

Players (Blue) and AI (Red) can spend snacks to instantly spawn units. Troops may require a specific building to be placed first.

### Components

| Script | Purpose |
|--------|---------|
| `TroopPurchaseController` | Core shared controller — player UI, purchase logic, spawn |
| `AITroopBuyer` | Red team AI — periodically buys troops from its own wallet |

### TroopPurchaseController

**File:** `/Assets/Scripts/Combat/TroopPurchaseController.cs`

**Serialized Fields:**

| Field | Purpose |
|-------|---------|
| `playerWallet` | Blue team's `SnackWallet` |
| `troopOptions` | List of `TroopOption` (see below) |
| `blueSpawnPoint` / `redSpawnPoint` | Optional explicit spawn transforms |
| `spawnForwardOffset` | Offset from castle when no explicit spawn (default 1.75) |
| `troopPanel` | Root `RectTransform` of the troop section in HUD |
| `totalTroopsText` | `Text` showing "Bought: N" counter |
| `troopButtons` | List of `TroopButtonBinding` wired to prefab buttons |

**TroopOption Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `troopId` | string | Unique identifier (e.g. `warrior`) |
| `displayName` | string | UI label |
| `requiredBuildingId` | string | `BuildingId` that must exist for this team to unlock |
| `bluePrefab` | GameObject | Prefab spawned for Blue team |
| `redPrefab` | GameObject | Prefab spawned for Red team |
| `snackCost` | int | Cost to purchase |

**TroopButtonBinding Fields:**

| Field | Wired To |
|-------|----------|
| `troopId` | Match to `TroopOption.troopId` |
| `button` | `Button` component on the button GameObject |
| `icon` | `Image` under `Icon` child |
| `nameText` | `Text` under `Name` child |
| `costText` | `Text` under `Cost` child |
| `badgeText` | `Text` under `Badge/BadgeText` |
| `lockText` | `Text` under `LockText` |

### HUD Prefab Layout

The troop UI lives at `BuildingPlacementHUD/Panel/TroopPanel` and is **fully authored in the prefab** (no runtime generation):

```
Panel/TroopPanel         (anchors: 0.05→0.95 X, 0.68→0.80 Y)
├── TroopSectionTitle    (Text: "⚔ Troops")
├── TroopCounter         (Text: "Bought: N") ← wired to totalTroopsText
└── TroopButtonRow       (HorizontalLayoutGroup)
    ├── Warrior_TroopButton
    │   ├── Icon         (Image)
    │   ├── Name         (Text)
    │   ├── Cost         (Text)
    │   ├── LockText     (Text, hidden when unlocked)
    │   └── Badge
    │       └── BadgeText (Text: troop count)
    └── Archer_TroopButton
        └── (same children)
```

### Lock / Unlock Logic

```
IsTroopUnlocked(option, team):
  if requiredBuildingId is empty → always unlocked
  else scan PlacedBuilding[] in scene:
      match BuildingId == requiredBuildingId
      AND CombatTarget.Team == team
  → true if found, false otherwise
```

When locked:
- Button `interactable = false`, `disabled` color applied
- `LockText` activated showing `"Build barracks"` / `"Build archery"`

When unlocked but player can't afford:
- Button `interactable = false`

### AI Troop Buyer

**File:** `/Assets/Scripts/Combat/AITroopBuyer.cs`

Attached to `RedAIBuilder` in the scene.

| Field | Default | Purpose |
|-------|---------|---------|
| `troopPurchaseController` | auto-found | Shared controller |
| `wallet` | `GetComponent<SnackWallet>` | Red team wallet |
| `buyInterval` | 5s | How often AI tries to buy |
| `minimumSnacksToKeep` | 20 | Reserve snacks before buying |

AI iterates through troop options and calls `TryPurchaseForTeam(index, CombatTeam.Red, wallet)` for first affordable unlocked option.

### Purchase Flow

```
Player clicks button in HUD
  → TryPurchaseForTeam(index, CombatTeam.Blue)
  → IsTroopUnlocked? (check PlacedBuilding in scene)
  → wallet.TrySpend(cost)
  → Instantiate prefab at spawn point
  → CombatTarget.Team = Blue
  → IncrementPurchaseCount → RefreshPlayerUi()
  → Badge counter on button increments
  → totalTroopsText updates
```

### Adding a New Troop Type

1. Add a `TroopOption` to `troopOptions` list on `TroopPurchaseController`
2. Add a button child to `TroopButtonRow` in the prefab following the same structure
3. Add a `TroopButtonBinding` to `troopButtons` list, wire all refs
4. Set `troopId` to match the `TroopOption.troopId`
5. Set `requiredBuildingId` or leave empty for always-unlocked troops

---

## Adding New Building Types

To add a new tower/building:

1. **Create prefabs** for each team (Blue, Red) in `/Assets/Prefabs/Buildings/`
2. **Add BuildingCatalogEntry** to the HUD's catalog list
   - Set `buildingId`, `displayName`, `snackCost`
   - Assign team-specific prefabs
   - Set `slotCategory` to match target slots
3. **Place BuildSlots** in scene for where this building can go
4. **Configure allowed categories** on slots to accept the new building type

## Adding New Enemy Waves

To add a new wave:

1. **Create WaveData** asset with `waveID`
2. **Add WaveEntry[]** with unit data, count, delays
3. **Add to LevelData.enemyWaves** list
4. WaveManager will automatically spawn them in order

---

*Last updated: April 5, 2026*
