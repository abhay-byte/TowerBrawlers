# 🏗️ Building Placement Guide

## Overview

Towers and buildings in this game are placed on **pre-defined BuildSlot GameObjects** in the scene. There is **no freeform grid placement**.

---

## How Building Placement Works

### The Flow

```
Player drags card from HUD
        ↓
UI preview follows cursor
        ↓
Player drops on valid BuildSlot
        ↓
BuildSlot validates placement
        ↓
Snacks deducted from wallet
        ↓
Building prefab instantiated at slot
```

---

## Core Components

### 1. BuildSlot

**Location:** Scene GameObjects (BlueBuildSlots / RedBuildSlots)

**Required Components:**
- `BuildSlot` script
- `Collider2D` (BoxCollider2D, set as trigger)

**Serialized Fields:**

| Field | Type | Default | Purpose |
|-------|------|---------|---------|
| `ownerTeam` | CombatTeam | - | Which team owns this slot |
| `placementAnchor` | Transform | self | Override spawn position for building |
| `allowPlayerBuilding` | bool | true | Can the player build here? |
| `allowAIBuilding` | bool | true | Can the AI build here? |
| `allowSelling` | bool | true | Can buildings be sold from this slot? |
| `allowedCategories` | BuildingSlotCategory | Any | What types can be placed? |

**BuildingSlotCategory Flags:**
```
None     = 0        // Nothing
Building = 1        // Generic buildings only
Tower    = 2        // Towers only
Any      = 3        // Both buildings and towers
```

### 2. BuildingCatalogEntry

**Location:** Defined on `BuildingPlacementHUD` prefab or in code

Defines what buildings are available for construction:

| Field | Type | Default | Purpose |
|-------|------|---------|---------|
| `buildingId` | string | - | Unique identifier |
| `displayName` | string | - | Name shown in UI |
| `icon` | Sprite | - | Card icon (auto-resolves from prefab) |
| `snackCost` | int | 50 | Cost to build |
| `sellRefundMultiplier` | float | 0.5 | Refund percentage (50%) |
| `isCastle` | bool | false | Is this a castle? (not placeable) |
| `availableForPlayer` | bool | true | Player can build this |
| `availableForAI` | bool | true | AI can build this |
| `slotCategory` | BuildingSlotCategory | Tower | What slots accept this? |
| `blueTeamPrefab` | GameObject | - | Blue team variant |
| `redTeamPrefab` | GameObject | - | Red team variant |

### 3. BuildingPlacementController

**Location:** `/Assets/Scripts/Buildings/BuildingPlacementController.cs`  
**Attached to:** `BuildingPlacementHUD` prefab

Manages the entire building UI and drag-drop flow:

- Creates UI cards from catalog entries
- Handles mouse input (click to select, drag to place)
- Finds BuildSlot under pointer via `Physics2D.OverlapPoint`
- Validates placement and calls `slot.TryPlace()`
- Manages sell button

### 4. PlacedBuilding

**Location:** Added to instantiated buildings at runtime

Tracks the placed building:
- Stores `buildingId`, `snackCost`, `sellRefundMultiplier`
- References owning `BuildSlot`
- Sets `CombatTarget.Team` and `TargetType` on spawn
- Notifies slot when destroyed (clears slot reference)

---

## Placement Validation Checklist

A building can be placed if **ALL** conditions pass:

```
✅ Catalog entry is not null
✅ Entry is NOT a castle (entry.isCastle == false)
✅ Slot is empty (slot.currentBuilding == null)
✅ Entry has a prefab for this slot's team
✅ Entry's slotCategory matches slot's allowedCategories (bitwise AND)
✅ Permission: player/AI is allowed to build here
✅ Wallet has enough snacks (wallet.TrySpend(snackCost))
```

**Code Flow:**
```csharp
BuildSlot.CanPlace(entry, isAIBuilder) {
    1. entry != null
    2. !entry.isCastle
    3. currentBuilding == null
    4. entry.ResolvePrefabForTeam(ownerTeam) != null
    5. (entry.slotCategory & allowedCategories) != 0
    6. isAIBuilder ? allowAIBuilding : allowPlayerBuilding
}

BuildSlot.TryPlace(entry, wallet, isAIBuilder) {
    if (!CanPlace(entry, isAIBuilder)) return false;
    if (!wallet.TrySpend(entry.snackCost)) return false;
    SpawnBuilding(entry);
    return true;
}
```

---

## Team Restriction System

BuildSlots are **team-locked**:

| Slot Owner | Accepts Buildings From |
|------------|----------------------|
| Blue (1) | Blue team buildings only |
| Red (2) | Red team buildings only |
| Neutral (0) | Any team prefab available |

The building prefab is resolved via:
```csharp
entry.ResolvePrefabForTeam(CombatTeam.Blue) → returns blueTeamPrefab or fallback to prefab
entry.ResolvePrefabForTeam(CombatTeam.Red)  → returns redTeamPrefab or fallback to prefab
```

---

## Drag & Drop Mechanics

### Step 1: Begin Drag

```
Player clicks building card
    ↓
BuildingDragCardUI.OnBeginDrag()
    ↓
BuildingPlacementController.BeginDrag(entry)
    ↓
Creates UI preview (RectTransform + Image, 84x84px)
Sets sprite from entry.GetResolvedIcon()
```

### Step 2: Dragging

```
Player moves cursor
    ↓
BuildingDragCardUI.OnDrag()
    ↓
BuildingPlacementController.Drag(screenPosition)
    ↓
Preview follows cursor position
```

### Step 3: End Drag / Drop

```
Player releases cursor
    ↓
BuildingDragCardUI.OnEndDrag()
    ↓
BuildingPlacementController.EndDrag(screenPosition)
    ↓
Hide preview
Find slot: Physics2D.OverlapPoint(worldPosition)
    ↓
slot.TryPlace(entry, wallet, false)
    ↓
If success: Building instantiated
If fail: Nothing happens (preview disappears)
```

**Important:** The placement preview is a **2D UI element**, NOT a 3D ghost tower in the game world.

---

## Build Slots in Level1 Scene

### BlueBuildSlots (Player Side)

```
Parent: BlueBuildSlots
├── Blue_BuildSlot_1
├── Blue_BuildSlot_2  (-9.09, -14.74)  [Collider: 2.6 x 2.2]
├── Blue_BuildSlot_3
├── Blue_BuildSlot_4
├── Blue_BuildSlot_5
└── ... (extra slots)

All slots: ownerTeam = Blue (1)
           allowedCategories = Any (3)
```

### RedBuildSlots (Enemy Side)

```
Parent: RedBuildSlots
├── Red_BuildSlot_1
├── Red_BuildSlot_2
├── Red_BuildSlot_3
├── Red_BuildSlot_4
├── Red_BuildSlot_5  (27.05, -9.44)
└── ... (extra slots)

All slots: ownerTeam = Red (2)
           allowedCategories = Any (3)
```

---

## Tower Prefabs

**Location:** `/Assets/Prefabs/Buildings/[Team]_Buildings/`

| Team | Folder | CombatTarget.team |
|------|--------|-------------------|
| Blue | `Blue_Buildings/Tower.prefab` | 1 (Blue) |
| Red | `Red_Buildings/Tower.prefab` | 2 (Red) |
| Neutral (Black) | `Black_Buildings/Tower.prefab` | 0 (Neutral) |
| Neutral (Purple) | `Purple_Buildings/Tower.prefab` | 0 (Neutral) |
| Neutral (Yellow) | `Yellow_Buildings/Tower.prefab` | 0 (Neutral) |

### Tower Prefab Components

Every tower prefab has:

| Component | Configuration |
|-----------|--------------|
| `SpriteRenderer` | Team-colored sprite |
| `BoxCollider2D` | Size: {1.4575, 1.088125}, Offset: {0.015625, 0.5440625} |
| `CombatTarget` | team = owner, targetType = Building, maxHealth = 80, aimPointOffset = (0, 1.4), destroyOnDeath = 1 |
| `YSortSpriteRenderer` | Y-axis sorting for depth |

---

## Selling Buildings

```
Player selects placed building
    ↓
Sell button appears
    ↓
Player clicks Sell
    ↓
BuildingPlacementController.HandleSellPressed()
    ↓
slot.TrySell()
    ↓
Calculate refund: RoundToInt(snackCost * sellRefundMultiplier)
wallet.AddSnacks(refundAmount)
Destroy building GameObject
Clear slot.currentBuilding reference
```

**Default refund:** 50% of build cost (sellRefundMultiplier = 0.5)

Example: Tower costs 100 snacks → Sell for 50 snacks

---

## AI Building Placement

**File:** `/Assets/Scripts/Buildings/AIBuildingPlacer.cs`

The AI automatically builds on its owned slots:

```csharp
Every buildInterval seconds (default: 4s):
    For each ownedSlot in ownedSlots:
        For each entry in buildingCatalog:
            if entry.availableForAI && slot.CanPlace(entry, true) && wallet.CanAfford(entry.snackCost):
                slot.TryPlace(entry, wallet, true);
                break; // Place one building per interval
```

**Setup:** AI placer has pre-assigned `ownedSlots` list in the Inspector.

---

## Adding a New Building Type

### Step 1: Create Prefabs

```
1. Create tower prefab for each team:
   /Assets/Prefabs/Buildings/Blue_Buildings/NewTower.prefab
   /Assets/Prefabs/Buildings/Red_Buildings/NewTower.prefab

2. Add components:
   - SpriteRenderer (team-colored sprite)
   - BoxCollider2D
   - CombatTarget (set team, targetType=Building, maxHealth)
   - YSortSpriteRenderer
```

### Step 2: Add Catalog Entry

```
1. Open BuildingPlacementHUD prefab
2. Find BuildingPlacementController component
3. Add new BuildingCatalogEntry to "buildingCatalog" list:
   - buildingId: "new_tower"
   - displayName: "New Tower"
   - snackCost: 150
   - sellRefundMultiplier: 0.5
   - slotCategory: Tower (2)
   - availableForPlayer: true
   - availableForAI: true
   - blueTeamPrefab: Assign Blue variant
   - redTeamPrefab: Assign Red variant
   - icon: Assign sprite (optional, auto-resolves)
```

### Step 3: Place BuildSlots (if needed)

```
1. Create empty GameObject in scene
2. Add BuildSlot component
3. Add BoxCollider2D (set Is Trigger = true)
4. Configure:
   - ownerTeam: Blue or Red
   - allowedCategories: Tower or Any
   - allowPlayerBuilding: true
   - allowAIBuilding: true (if AI should use it)
5. Position in scene where tower should be placeable
```

---

## Troubleshooting

### Building Won't Place

| Symptom | Cause | Fix |
|---------|-------|-----|
| Card is red in HUD | Can't afford (snacks < cost) | Add more snacks or reduce cost |
| Nothing happens on drop | No BuildSlot at drop position | Place BuildSlots in scene |
| "Cannot place here" message | Slot validation failed | Check slot's ownerTeam, allowedCategories, or if slot is occupied |
| Preview doesn't appear | Icon not resolved | Assign icon sprite in catalog entry |

### Building Spawns in Wrong Position

| Issue | Cause | Fix |
|-------|-------|-----|
| Offset from slot center | placementAnchor not set | Set placementAnchor to desired position, or leave empty to use slot's transform |
| Wrong team prefab | ownerTeam mismatch | Verify slot's ownerTeam matches intended team |

### Sell Button Doesn't Work

| Issue | Cause | Fix |
|-------|-------|-----|
| Button not visible | No building selected | Click on a placed building first |
| Refund is 0 | sellRefundMultiplier = 0 | Set to 0.5 in catalog entry |
| Building not destroyed | sell.allowSelling = false | Enable allowSelling on the slot |

---

## Key Scripts Reference

| Script | File Path |
|--------|-----------|
| **BuildingPlacementController** | `/Assets/Scripts/Buildings/BuildingPlacementController.cs` |
| **BuildSlot** | `/Assets/Scripts/Buildings/BuildSlot.cs` |
| **BuildingCatalogEntry** | `/Assets/Scripts/Buildings/BuildingCatalogEntry.cs` |
| **PlacedBuilding** | `/Assets/Scripts/Buildings/PlacedBuilding.cs` |
| **BuildingDragCardUI** | `/Assets/Scripts/Buildings/BuildingDragCardUI.cs` |
| **AIBuildingPlacer** | `/Assets/Scripts/Buildings/AIBuildingPlacer.cs` |
| **BuildingSlotCategory** | `/Assets/Scripts/Buildings/BuildingSlotCategory.cs` |
| **SnackWallet** | `/Assets/Scripts/Buildings/SnackWallet.cs` |
| **CastleGameOverController** | `/Assets/Scripts/Buildings/CastleGameOverController.cs` |

---

*Last updated: April 4, 2026*
