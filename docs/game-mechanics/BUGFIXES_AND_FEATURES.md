# 🔧 Bug Fixes & Features

## Fixed Issues

### 1. ✅ Cannot Select Placed Buildings

**Problem:** Clicking on placed buildings didn't select them because building colliders blocked clicks from reaching BuildSlot colliders.

**Solution:** Updated `BuildingPlacementController.FindSlotUnderPointer()` to:
1. Use `Physics2D.RaycastAll()` to detect all colliders at click point
2. First search for BuildSlot directly
3. Then search for PlacedBuilding and find its owner slot
4. Fallback to original OverlapPoint method

**File:** `/Assets/Scripts/Buildings/BuildingPlacementController.cs`

---

### 2. ✅ Snack Generation Debugging

**Problem:** Snacks weren't increasing but no errors were shown.

**Solution:** Added comprehensive debug logging to `SnackGenerator`:
- Logs on initialization
- Logs every snack generation tick
- Warns if wallet is null or building is destroyed
- Shows team ownership (Player/AI) and current wallet total

**Expected Console Output:**
```
✅ SnackGenerator initialized on House1
🍎 House1 generated 1 snacks for Player. Total: 201
🍎 House2 generated 1 snacks for Player. Total: 202
```

**File:** `/Assets/Scripts/Buildings/SnackGenerator.cs`

---

### 3. ✅ Health Bars Above Buildings

**Problem:** No health bars visible above placed buildings.

**Solution:** Created `BuildingHealthBar` system that:
- Automatically adds to all placed buildings
- Shows health bar above building using Unity UI Canvas
- Updates in real-time based on CombatTarget health
- Color-coded: Green (>60%), Yellow (30-60%), Red (<30%)
- Shows health in selection UI: "Selected: house (HP: 80/80)"
- Optional fade-out when not damaged

**File:** `/Assets/Scripts/Buildings/BuildingHealthBar.cs`

---

## New Features Added

### Health Bar System

**Component:** `BuildingHealthBar`

**Auto-added to buildings when:**
- Building is placed via `PlacedBuilding.Initialize()`
- Building has a `CombatTarget` component

**Features:**
- ✅ World-to-screen position tracking
- ✅ Real-time health updates
- ✅ Color-coded health (Green/Yellow/Red)
- ✅ Configurable size and position
- ✅ Optional fade-out when at full health
- ✅ Works with any Canvas in scene

**Settings:**

| Property | Default | Description |
|----------|---------|-------------|
| Height Offset | 1.2 | Units above building |
| Bar Width | 1.5 | Width in world units |
| Bar Height | 0.15 | Height in world units |
| Always Show | true | Show even at full health |
| Fade When Full | false | Hide after 3s at full health |

### CombatTarget Damage Event

**Added:** `Damaged` event to `CombatTarget`

```csharp
public event Action<CombatTarget, float> Damaged; // (target, damageAmount)
```

**Fires when:**
- Building takes damage
- Health decreases but not zero
- Allows health bars to update immediately

---

## How to Test

### Test Building Selection

```
1. Enter Play Mode
2. Click on a placed house
3. Check "SelectedSlotText" in UI shows:
   "Selected: house (HP: 80/80)"
4. Sell button should become clickable
```

### Test Snack Generation

```
1. Place a House on a Blue BuildSlot
2. Check Console for: "✅ SnackGenerator initialized on House1"
3. Wait 2 seconds
4. Check Console for: "🍎 House1 generated 1 snacks for Player. Total: 201"
5. Check "SnacksText" in UI updates
6. Repeat to verify continuous generation
```

### Test Health Bars

```
1. Place any building
2. Health bar should appear above it
3. Check bar shows full (green)
4. Damage the building (via combat or debug)
5. Bar should:
   - Decrease in fill amount
   - Change color (green → yellow → red)
   - Show updated HP in selection UI
```

---

## File Changes Summary

### Modified Files

| File | Changes |
|------|---------|
| `BuildingPlacementController.cs` | Fixed `FindSlotUnderPointer()`, added health display in selection UI |
| `SnackGenerator.cs` | Added debug logging for initialization and generation |
| `PlacedBuilding.cs` | Auto-adds `BuildingHealthBar` component on placement |
| `CombatTarget.cs` | Added `Damaged` event |

### New Files

| File | Purpose |
|------|---------|
| `BuildingHealthBar.cs` | UI health bar system for buildings |

---

## Troubleshooting

### Health Bar Not Showing

| Issue | Fix |
|-------|-----|
| No Canvas in scene | Add a Canvas GameObject to scene (UI → Canvas) |
| Health bar not following building | Check `heightOffset` value, increase if needed |
| Health bar too small/large | Adjust `barWidth` and `barHeight` in Inspector |

### Snacks Still Not Generating

| Check | How to Verify |
|-------|---------------|
| SnackGenerator component exists | Select placed house in Hierarchy, check for component |
| Wallet exists in scene | Search for SnackWallet component |
| Generation enabled | Check `enabled` checkbox on SnackGenerator |
| Console messages | Look for ✅ and 🍎 messages in Console |

### Cannot Select Buildings

| Check | How to Verify |
|-------|---------------|
| Building has PlacedBuilding | Select building, check for component |
| BuildSlot has collider | Select BuildSlot, verify BoxCollider2D exists |
| Camera assigned | Check BuildingPlacementController.worldCamera |

---

*Last updated: April 4, 2026*
