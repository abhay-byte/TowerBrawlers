# 🍎 Snack Generation System

## Overview

Buildings (like Houses) can generate snacks passively over time, providing income for both **Player** and **AI** teams. When a House is placed on a BuildSlot, it automatically generates snacks for the owning team's wallet.

---

## How It Works

```
House Placed on BuildSlot
        ↓
PlacedBuilding.Initialize() called
        ↓
SnackGenerator component activated
        ↓
Every tickInterval seconds:
    → Add snacksPerTick to wallet
    → Works for Player (Blue) or AI (Red)
```

---

## Components

### 1. SnackGenerator

**File:** `/Assets/Scripts/Buildings/SnackGenerator.cs`

The core snack generation component. Attach to any building prefab to enable passive income.

**Required Components:** `PlacedBuilding`

#### Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `snacksPerTick` | int | 1 | Snacks added per generation tick |
| `tickInterval` | float | 2.0 | Seconds between each tick |
| `wallet` | SnackWallet | auto-find | Reference to wallet (auto-finds if null) |

#### How It Works

```csharp
// Every tickInterval seconds:
GenerateSnacks() {
    wallet.AddSnacks(snacksPerTick);
}
```

**Team Ownership:**
- Automatically determines owner from the `BuildSlot.ownerTeam`
- If slot is Blue → snacks go to player's wallet
- If slot is Red → snacks go to AI's wallet

#### Methods

| Method | Description |
|--------|-------------|
| `Initialize(slot)` | Called when building is placed |
| `SetGenerationRate(snacks, interval)` | Override generation rate |
| `GetOwnerTeam()` | Returns CombatTeam (Blue/Red) |
| `IsPlayerOwned()` | Returns true if Blue team |
| `IsAIOwned()` | Returns true if Red team |
| `GetSnacksPerSecond()` | Returns generation rate for UI |
| `GetGenerationProgress01()` | Returns normalized progress through the current snack cycle |

---

### 2. House

**File:** `/Assets/Scripts/Buildings/House.cs`

A specialized building that generates snacks. Wraps `SnackGenerator` with house-specific settings.

**Required Components:** `PlacedBuilding`, `SnackGenerator`, `CombatTarget`

### 3. Runtime UI Bars

**BuildingHealthBar**
- Added to placed buildings if missing
- Shows HP above the building on the active HUD canvas

**HouseSnackGenerationBar**
- Added to houses / snack-generating buildings if missing
- Shows the fill progress until the next snack tick
- Uses Blue/Red fill colors based on owner team

#### Settings

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `houseName` | string | "House" | Display name |
| `customGenerationRate` | bool | false | Use custom generation settings? |
| `customSnacksPerTick` | int | 2 | Custom snacks per tick |
| `customTickInterval` | float | 3.0 | Custom interval in seconds |

#### Example Generation Rates

| Configuration | Snacks/Sec | Description |
|---------------|------------|-------------|
| Default (SnackGenerator) | 0.5/sec | 1 snack every 2 seconds |
| House Custom | 0.67/sec | 2 snacks every 3 seconds |
| Fast House | 1.0/sec | 1 snack every 1 second |
| Mansion | 2.0/sec | 4 snacks every 2 seconds |

---

## Setup Guide: Creating a House Prefab

### Step 1: Create House Prefabs

You need at least **two prefabs** (one for each main team). This project currently also includes color variants such as `House1`, `House2`, and `House3` under multiple team folders.

```
Assets/Prefabs/Buildings/
├── Blue_Buildings/
│   └── House.prefab          ← Player house
└── Red_Buildings/
    └── House.prefab          ← AI house
```

### Step 2: Configure Prefab Components

Create the prefab with these components:

#### Required Components:

1. **SpriteRenderer**
   - Sprite: House sprite (team-colored if needed)
   - Sort Order: Appropriate for Y-sorting

2. **BoxCollider2D**
   - Size: Match tower size (e.g., {1.4575, 1.088125})
   - Is Trigger: ✅ Yes

3. **CombatTarget**
   - Team: Set based on prefab (Blue=1, Red=2)
   - Target Type: Building
   - Max Health: 50-100 (houses are fragile)
   - Aim Point Offset: (0, 1.0)
   - Destroy On Death: 1

4. **YSortSpriteRenderer**
   - Configure for Y-axis sorting

5. **PlacedBuilding**
   - Required on current house prefabs
   - Runtime placement also adds it automatically if a variant is missing it

6. **SnackGenerator** ⭐
   - Snacks Per Tick: 1
   - Tick Interval: 2.0
   - Wallet: (leave empty, auto-finds)

7. **House** ⭐
   - House Name: "House"
   - Custom Generation Rate: ☐ (unchecked for default)
   - OR check and set custom values

8. **BuildingHealthBar**
   - Shows building HP above the prefab after placement

9. **HouseSnackGenerationBar**
   - Shows snack cycle progress above houses after placement

#### Optional Components:

10. **Animator** (if you want placement animations)

### Step 3: Add to Building Catalog

Open `BuildingPlacementHUD` prefab and add a new entry to `BuildingPlacementController.buildingCatalog`:

```csharp
BuildingCatalogEntry {
    buildingId: "house",
    displayName: "House",
    snackCost: 75,
    sellRefundMultiplier: 0.5,
    isCastle: false,
    availableForPlayer: true,
    availableForAI: true,
    slotCategory: BuildingSlotCategory.Building,  // or Any
    blueTeamPrefab: [Assign Blue House prefab]
    redTeamPrefab: [Assign Red House prefab]
    icon: [Assign house icon sprite]
}
```

### Step 4: Place BuildSlots

Make sure you have BuildSlots in the scene where houses can be placed:

```
BlueBuildSlots/
├── Blue_BuildSlot_1 (ownerTeam: Blue)
└── ...

RedBuildSlots/
├── Red_BuildSlot_1 (ownerTeam: Red)
└── ...
```

Each slot should have:
- `BuildSlot` component
- `BoxCollider2D` (Is Trigger: ✅)
- `allowedCategories`: Building or Any

---

## Generation Rates & Balance

### Recommended Values

#### Early Game House

```
snacksPerTick: 1
tickInterval: 2.0
Result: 0.5 snacks/sec (30 snacks/min)
```

#### Mid Game House

```
snacksPerTick: 2
tickInterval: 2.0
Result: 1.0 snacks/sec (60 snacks/min)
```

#### Late Game Mansion

```
snacksPerTick: 3
tickInterval: 1.5
Result: 2.0 snacks/sec (120 snacks/min)
```

### Economy Balance Example

```
Starting Snacks: 200
Tower Cost: 100
House Cost: 75

Tower placement: -100 snacks
House placement: -75 snacks

House generation: +0.5 snacks/sec
Break-even time: 75 / 0.5 = 150 seconds (2.5 minutes)

If you place 3 houses:
→ 1.5 snacks/sec
→ New tower paid off in: 100 / 1.5 = 67 seconds
```

---

## How Player vs AI Works

### Automatic Team Detection

The system automatically determines team ownership:

```csharp
1. BuildSlot has ownerTeam (Blue or Red)
2. When building is placed:
   → PlacedBuilding.Initialize(slot, ownerTeam, entry)
   → SnackGenerator.Initialize(slot)
   → SnackGenerator.GetOwnerTeam() returns slot's ownerTeam

3. Generation:
   → If Blue team: wallet.AddSnacks()
   → If Red team: wallet.AddSnacks()
```

### Current Wallet Behavior

The current scene setup uses a shared `SnackWallet` reference discovered in-scene, so both player and AI snack generation feed the same wallet unless you split that system later.

### Single Wallet vs Dual Wallet

**Current Implementation:**
- There is **ONE SnackWallet** in the scene
- Both player and AI add snacks to the **same wallet**

**If you want separate wallets:**

Option A: Create two wallets (PlayerWallet, AIWallet)
```csharp
// In SnackGenerator.GenerateSnacks():
if (IsPlayerOwned()) {
    playerWallet.AddSnacks(snacksPerTick);
} else if (IsAIOwned()) {
    aiWallet.AddSnacks(snacksPerTick);
}
```

Option B: Use wallet tags/IDs
```csharp
// Add to SnackWallet:
public CombatTeam ownerTeam;

// In SnackGenerator:
wallets = FindObjectsOfType<SnackWallet>();
foreach (var w in wallets) {
    if (w.ownerTeam == GetOwnerTeam()) {
        w.AddSnacks(snacksPerTick);
    }
}
```

---

## Visual Feedback

### Adding UI Notification

To show snacks being generated, add an event to `SnackGenerator`:

```csharp
// In SnackGenerator.cs:
public event Action<int> SnacksGenerated;

private void GenerateSnacks() {
    wallet.AddSnacks(snacksPerTick);
    SnacksGenerated?.Invoke(snacksPerTick); // Fire event
}
```

Then create a UI script to display generation:

```csharp
public class SnackGenerationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI generationText;

    private void OnEnable() {
        // Find all snack generators and subscribe
        SnackGenerator[] generators = FindObjectsOfType<SnackGenerator>();
        foreach (var gen in generators) {
            gen.SnacksGenerated += OnSnacksGenerated;
        }
    }

    private void OnSnacksGenerated(int amount) {
        generationText.text = $"+{amount} 🍎";
        // Animate text...
    }
}
```

### Adding Particle Effect

Add a particle system to the House prefab that plays when snacks are generated:

```csharp
// In SnackGenerator.cs:
[SerializeField] private ParticleSystem generationParticles;
[SerializeField] private AudioSource generationSound;

private void GenerateSnacks() {
    wallet.AddSnacks(snacksPerTick);
    
    // Visual feedback
    if (generationParticles != null)
        generationParticles.Play();
    if (generationSound != null)
        generationSound.Play();
}
```

---

## Testing

### Test Player House

```
1. Place a House on a Blue BuildSlot
2. Check Console: "🏠 House placed for Player - Generating snacks"
3. Wait 2 seconds
4. Check wallet: Snacks should increase by 1
5. Repeat to verify continuous generation
```

### Test AI House

```
1. AI places a House on a Red BuildSlot (via AIBuildingPlacer)
2. Check Console: "🏠 House placed for AI - Generating snacks"
3. Wait 2 seconds
4. Check wallet: Snacks should increase (shared wallet)
5. If separate wallets: Check AI wallet specifically
```

### Debug Mode

In Unity Editor, select a House GameObject:
- **SnackGenerator component** shows current settings
- **Gizmos**: Yellow wire sphere + cyan label showing generation rate
- **Inspector**: Edit values in real-time while playing

---

## Troubleshooting

### House Not Generating Snacks

| Symptom | Cause | Fix |
|---------|-------|-----|
| No console message | SnackGenerator not initialized | Check if wallet exists in scene |
| Wallet not increasing | Wallet reference missing | Assign wallet in Inspector or ensure SnackWallet exists |
| Generator disabled | Missing PlacedBuilding | Add PlacedBuilding component to prefab |
| Generation too slow | tickInterval too high | Reduce tickInterval (e.g., 1.0) |

### Snacks Going to Wrong Team

| Issue | Cause | Fix |
|-------|-------|-----|
| AI snacks go to player | Single shared wallet | Implement dual wallet system (see above) |
| No snacks generated | ownerTeam is Neutral | Verify BuildSlot has correct ownerTeam |

### Performance Issues

| Issue | Cause | Fix |
|-------|-------|-----|
| Lag with many houses | Too many FindObjectOfType calls | Cache wallet reference in Awake() |
| High CPU usage | tickInterval too low (e.g., 0.01) | Increase tickInterval to 1.0 or higher |

---

## Extending the System

### Different Building Types

Create specialized buildings with different generation rates:

```csharp
// Farm.cs - Slow but cheap
public class Farm : MonoBehaviour {
    // 0.25 snacks/sec, cost 40
}

// Market.cs - Fast but expensive
public class Market : MonoBehaviour {
    // 2.0 snacks/sec, cost 150
}

// Tavern.cs - Generates snacks + buffs units
public class Tavern : MonoBehaviour {
    // 1.0 snacks/sec + unit speed buff
}
```

### Upgrade System

```csharp
// In SnackGenerator.cs:
public int generationLevel = 1;
public int[] upgradeCosts = { 75, 150, 300 };
public int[] snacksPerTickByLevel = { 1, 2, 4 };

public bool CanUpgrade(SnackWallet wallet) {
    if (generationLevel >= upgradeCosts.Length) return false;
    return wallet.CanAfford(upgradeCosts[generationLevel - 1]);
}

public void Upgrade(SnackWallet wallet) {
    if (!CanUpgrade(wallet)) return;
    wallet.TrySpend(upgradeCosts[generationLevel - 1]);
    generationLevel++;
    snacksPerTick = snacksPerTickByLevel[generationLevel - 1];
}
```

### Global Generation Multiplier

```csharp
// In SnackWallet or GameManager:
public float globalGenerationMultiplier = 1.0f;

// In SnackGenerator.GenerateSnacks():
int adjustedSnacks = Mathf.RoundToInt(snacksPerTick * globalGenerationMultiplier);
wallet.AddSnacks(adjustedSnacks);
```

---

## File Structure

```
Assets/Scripts/Buildings/
├── SnackGenerator.cs          ← Core generation logic
├── House.cs                   ← House building wrapper
├── PlacedBuilding.cs          ← Updated to initialize generators
├── BuildSlot.cs               ← Placement validation
├── BuildingCatalogEntry.cs    ← Building definitions
├── BuildingPlacementController.cs  ← UI & placement flow
├── SnackWallet.cs             ← Currency system
└── AIBuildingPlacer.cs        ← AI auto-building

Assets/Prefabs/Buildings/
├── Blue_Buildings/
│   ├── Tower.prefab
│   └── House.prefab           ← Create this
├── Red_Buildings/
│   ├── Tower.prefab
│   └── House.prefab           ← Create this
└── ...
```

---

*Last updated: April 4, 2026*
