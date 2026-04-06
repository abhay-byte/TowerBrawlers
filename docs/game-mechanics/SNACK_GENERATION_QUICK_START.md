# 🍎 Snack Generation - Quick Setup

## 1️⃣ Create House Prefab

**Components needed:**
```
✅ SpriteRenderer
✅ BoxCollider2D (Is Trigger)
✅ CombatTarget (maxHealth: 50-100)
✅ YSortSpriteRenderer
✅ PlacedBuilding
✅ SnackGenerator          ← NEW
✅ House                   ← NEW
✅ BuildingHealthBar       ← HP bar
✅ HouseSnackGenerationBar ← snack progress bar
```

**Make 2 versions:**
- `Blue_Buildings/House.prefab` (Player)
- `Red_Buildings/House.prefab` (AI)

---

## 2️⃣ Configure SnackGenerator

| Setting | Value | Description |
|---------|-------|-------------|
| Snacks Per Tick | 1 | Amount generated |
| Tick Interval | 2.0 | Seconds between ticks |
| Wallet | (empty) | Auto-finds SnackWallet |

**Result:** 0.5 snacks/sec (1 every 2 seconds)

---

## 3️⃣ Configure House (Optional)

| Setting | Value | Description |
|---------|-------|-------------|
| House Name | "House" | Display name |
| Custom Generation | ☐ Off | Use default rates |
| Custom Snacks/Tick | 2 | If custom enabled |
| Custom Interval | 3.0 | If custom enabled |

---

## 4️⃣ Add to Building Catalog

In `BuildingPlacementHUD` → `BuildingPlacementController` → `buildingCatalog`:

```
buildingId: "house"
displayName: "House"
snackCost: 75
sellRefundMultiplier: 0.5
slotCategory: Building (1) or Any (3)
availableForPlayer: true
availableForAI: true
blueTeamPrefab: [Blue House]
redTeamPrefab: [Red House]
```

---

## 5️⃣ Test

```
1. Place House on Blue slot → +0.5 snacks/sec to wallet
2. AI places House on Red slot → +0.5 snacks/sec to wallet
3. Check console: "🏠 House placed for Player/AI - Generating snacks"
4. Wait 2 seconds → Snacks increase by 1
5. Verify HP bar appears above the building
6. Verify the second bar fills up and pulses when snacks are generated
```

---

## 📊 Generation Examples

| Building | Cost | Rate | Break-Even |
|----------|------|------|------------|
| Basic House | 75 | 0.5/sec | 150 sec |
| Upgraded House | 100 | 1.0/sec | 100 sec |
| Farm | 40 | 0.25/sec | 160 sec |
| Market | 150 | 2.0/sec | 75 sec |

---

## 🔧 Quick Fixes

**Not generating?**
- Check SnackWallet exists in scene
- Verify PlacedBuilding component present
- Check BuildSlot has ownerTeam (not Neutral)

**Separate Player/AI wallets?**
- Create 2 SnackWallets (PlayerWallet, AIWallet)
- Update SnackGenerator to pick wallet by team

**Current behavior**
- Both Blue and Red houses use the active in-scene `SnackWallet`

---

*Full docs: [SNACK_GENERATION.md](./SNACK_GENERATION.md)*
