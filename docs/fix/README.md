# Unity MCP Package & Code Fixes Documentation

## Overview

This document covers fixes applied to Tower Brawlers: A Soup-to-Nuts War, including:
1. Unity 6.5 MCP package compatibility (Coplay `com.coplaydev.unity-mcp`)
2. Code compilation errors (variable shadowing, access modifiers)

## Problem Summary

### Issue 1: Unity 6.5 Deprecated APIs

Unity 6.5 made two previously-deprecated APIs into **errors** (not just warnings):

1. **`Object.GetInstanceID()`** — deprecated, replaced by `Object.GetEntityId()`
2. **`EntityId` to `int` implicit cast** — deprecated, will be removed in a future version

The Coplay MCP package (latest commit `8442f29db6b3`) used `GetInstanceID()` across **30+ files** and relied on implicit `EntityId → int` casting in many places, causing **105+ compilation errors**.

### Issue 2: Code Errors in Tower Brawlers Scripts

| Error | File | Line | Description |
|-------|------|------|-------------|
| CS0136 | `BuildingPlacementController.cs` | 169, 177 | Variable `hit` declared in nested scopes |
| CS0122 | `BuildingHealthBar.cs` | 182 | `CombatTarget.maxHealth` inaccessible (private field) |

**Root cause (CS0136):** `hit` was declared in a `foreach` loop (line 169), then redeclared in a second `foreach` loop (line 177), and again in a fallback method (line 194). C# doesn't allow reusing the same variable name in nested/overlapping scopes.

**Fix:** Renamed loop variables to `rayHit` and fallback variable to `overlapHit` to eliminate shadowing.

**Root cause (CS0122):** `BuildingHealthBar.cs` line 182 used `combatTarget.maxHealth` (private field) instead of `combatTarget.MaxHealth` (public property).

**Fix:** Changed `maxHealth` → `MaxHealth` to use the public property accessor.

## API Changes in Unity 6.5

### Old API (Deprecated → Error)

```csharp
int id = gameObject.GetInstanceID();
```

### New API

```csharp
EntityId id = gameObject.GetEntityId();
int numericId = gameObject.GetEntityId().GetHashCode(); // Convert to int safely
```

Key points:
- `GetEntityId()` returns a new `EntityId` struct type, not `int`
- The implicit `EntityId → int` cast is also deprecated
- **`.GetHashCode()`** is Unity's recommended way to get an `int` identifier from `EntityId`
- For serialization purposes, `.ToString()` can also be used to write `EntityId` as a string

## Files Modified

### Package Location

```
Library/PackageCache/com.coplaydev.unity-mcp@8442f29db6b3/
```

### Runtime Files (1 file)

| File | Changes |
|------|---------|
| `Runtime/Serialization/UnityTypeConverters.cs` | `GetEntityId().ToString()` for JSON serialization (3 occurrences) |

### Editor Files (29 files)

| File | Change Applied |
|------|---------------|
| `Editor/Resources/Editor/Windows.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Resources/Editor/Selection.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Helpers/GameObjectSerializer.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Resources/Scene/GameObjectResource.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Helpers/ComponentOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Helpers/GameObjectLookup.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/ManageUI.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/ManageMaterial.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/ManageComponents.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/ManageAsset.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/ManageScene.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Cameras/CameraCreate.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Cameras/CameraControl.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Cameras/CameraConfigure.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Physics/JointOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Physics/PhysicsSimulationOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Physics/PhysicsRigidbodyOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Physics/PhysicsQueryOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/GameObjects/ManageGameObjectCommon.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/GameObjects/GameObjectDelete.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/GameObjects/GameObjectDuplicate.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/GameObjects/GameObjectLookAt.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Prefabs/ManagePrefabs.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Graphics/RendererFeatureOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Graphics/VolumeOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Graphics/SkyboxOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Graphics/LightBakingOps.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/Vfx/ParticleControl.cs` | `GetEntityId().GetHashCode()` + pragma |
| `Editor/Tools/ProBuilder/ManageProBuilder.cs` | `GetEntityId().GetHashCode()` + pragma |

## Fix Strategy

### Step 1: Runtime File — Use `.ToString()` for JSON

In `UnityTypeConverters.cs`, `EntityId` values are written to JSON for serialization. Instead of converting to `int`, we write them as strings:

```csharp
// Before (error):
writer.WriteValue(value.GetInstanceID());

// After (fixed):
writer.WriteValue(value.GetEntityId().ToString());
```

This avoids the `EntityId → int` cast entirely and is semantically correct for JSON (instance IDs are opaque identifiers, not numbers used in arithmetic).

### Step 2: Editor Files — Use `.GetHashCode()` for int conversion

In all Editor files, `GetInstanceID()` was replaced with `GetEntityId().GetHashCode()`:

```csharp
// Before (error):
int id = gameObject.GetInstanceID();

// After (fixed):
int id = gameObject.GetEntityId().GetHashCode();
```

`.GetHashCode()` is the Unity-recommended way to get an `int` from `EntityId`.

### Step 3: Add Pragma for Safety

Each Editor file has `#pragma warning disable CS0619` at the top to suppress any remaining deprecation warnings:

```csharp
#pragma warning disable CS0619
using System;
// ... rest of file
```

## Why Pragma Alone Didn't Work

Initially, `#pragma warning disable CS0619` was added to all files. However, Unity treats `[Obsolete("...", true)]` attributes as **errors**, not warnings. The `true` parameter in the attribute means "treat as error." The pragma only suppresses **warnings**, not errors.

Therefore, the API calls themselves needed to be changed to the new API.

## Commands Used

### Clear stale package cache

```bash
rm -rf Library/PackageCache/com.coplaydev.unity-mcp@d6e58c68cf4b
```

### Add pragma to all affected files

```bash
for file in <29 files>; do
  sed -i '1s/^/#pragma warning disable CS0619\n/' "$file"
done
```

### Replace GetInstanceID() with GetEntityId()

```bash
sed -i 's/\.GetInstanceID()/.GetEntityId()/g' <all affected files>
```

### Append .GetHashCode() to convert EntityId to int

```bash
sed -i 's/\.GetEntityId()/.GetEntityId().GetHashCode()/g' <all affected files>
```

### Fix runtime serializer specifically

```bash
sed -i 's/\.GetEntityId()\.GetHashCode()/.GetEntityId().ToString()/g' \
  Runtime/Serialization/UnityTypeConverters.cs
```

## Error Count Timeline

| Phase | Errors | Description |
|-------|--------|-------------|
| Initial | 3 | `GetInstanceID()` deprecated in `UnityTypeConverters.cs` |
| After `GetEntityId()` | 3 | `EntityId → int` implicit cast obsolete in `UnityTypeConverters.cs` |
| After `.GetHashCode()` in one file | 3 | Fixed runtime file |
| After `GetEntityId()` on all files | 105 | `EntityId → int` implicit cast obsolete everywhere |
| After `.GetHashCode()` on all files | 14 | Remaining edge cases with different error patterns |
| Final | 0 | All resolved |

## Important Notes

### ⚠️ These changes are in `Library/PackageCache`

Files in `Library/PackageCache` are managed by Unity's Package Manager and will be **overwritten** when:
- The package version changes
- The package is re-resolved
- Unity regenerates package files

### 🔧 To persist these fixes

Options to make these fixes permanent:

1. **Fork the repository** and maintain your own branch:
   ```json
   // Packages/manifest.json
   "com.coplaydev.unity-mcp": "https://github.com/YOUR_FORK/unity-mcp.git?path=/MCPForUnity#main"
   ```

2. **Report upstream** — Submit these changes as a PR to CoplayDev/unity-mcp so they fix it officially.

3. **Use IvanMurzak/Unity-MCP** — An alternative package with 1.9k stars that may have better Unity 6.5 compatibility:
   ```json
   "com.ivanmurzak.unity-mcp": "https://github.com/IvanMurzak/Unity-MCP.git"
   ```

4. **Use Unity's official package** (pre-release):
   ```json
   "com.unity.ai.assistant": "2.0.0-pre.1"
   ```

## Verification

After applying all fixes, verify compilation with:
1. Re-focus the Unity Editor window
2. Check the Console for zero errors
3. Open `Window → MCP for Unity` to confirm the server starts
4. Test with an AI assistant (Claude Code, Cursor, etc.)

### Verification Result (2026-04-05)

| Error | Status |
|-------|--------|
| CS0136 `hit` shadowing | ✅ Fixed — renamed to `rayHit`/`overlapHit` |
| CS0122 `maxHealth` access | ✅ Fixed — changed to `MaxHealth` property |
| MCP package API errors | ✅ Fixed — `GetEntityId().GetHashCode()` |

**Remaining:** Only serialization warnings (UAC1001/UAC1002) from Unity Technologies template assets — non-blocking.

## Related

- Unity 6.5 release notes: `GetInstanceID()` deprecation
- Unity Forum discussion: [Planned breaking changes in Unity 6.5](https://discussions.unity.com/t/planned-breaking-changes-in-unity-6-5)
- MCP server: [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp)
