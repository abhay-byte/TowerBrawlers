# Quick Reference: Fixes Applied

## 1. AurynSky Duplicate Classes ✅

**What:** Wrapped `ChestDemo`, `MaterialMover`, `Rotator` in unique namespaces  
**Files:** 8 files in `Assets/AurynSky/*/Scripts/`  
**Status:** ✅ Done, safe to keep

## 2. Unity MCP Package — GetInstanceID() Deprecation ✅

**What:** Replaced all `GetInstanceID()` → `GetEntityId().GetHashCode()`  
**What:** Replaced serializer's `GetEntityId()` → `GetEntityId().ToString()`  
**What:** Added `#pragma warning disable CS0619` to all 29 Editor files  
**Files:** 1 Runtime + 29 Editor files in `Library/PackageCache/com.coplaydev.unity-mcp@*/`  
**Status:** ⚠️ Applied to PackageCache — **will be overwritten** on package update

## To Persist MCP Fixes

Choose **one**:

| Option | How |
|--------|-----|
| Fork the repo | Fork `CoplayDev/unity-mcp`, apply these changes, reference your fork in `manifest.json` |
| Submit PR upstream | Send changes to `CoplayDev/unity-mcp` so they fix it officially |
| Switch package | Use `IvanMurzak/Unity-MCP` (1.9k stars) instead |
| Wait for official fix | Unity's `com.unity.ai.assistant` package may get better Unity 6.5 support |

## Quick Re-Apply Commands

If the MCP package gets re-downloaded, re-run:

```bash
# In project root
PACKAGE="Library/PackageCache/com.coplaydev.unity-mcp@*/"

# 1. Add pragma to all affected files
for file in Editor/Resources/Editor/Windows.cs Editor/Resources/Editor/Selection.cs Editor/Helpers/GameObjectSerializer.cs Editor/Resources/Scene/GameObjectResource.cs Editor/Helpers/ComponentOps.cs Editor/Tools/ManageUI.cs Editor/Tools/ManageMaterial.cs Editor/Tools/ManageComponents.cs Editor/Tools/ManageAsset.cs Editor/Tools/ManageScene.cs Editor/Tools/Cameras/CameraCreate.cs Editor/Tools/Cameras/CameraControl.cs Editor/Tools/Cameras/CameraConfigure.cs Editor/Tools/Physics/JointOps.cs Editor/Tools/Physics/PhysicsSimulationOps.cs Editor/Tools/Physics/PhysicsRigidbodyOps.cs Editor/Tools/Physics/PhysicsQueryOps.cs Editor/Tools/GameObjects/ManageGameObjectCommon.cs Editor/Tools/GameObjects/GameObjectDelete.cs Editor/Tools/GameObjects/GameObjectDuplicate.cs Editor/Tools/GameObjects/GameObjectLookAt.cs Editor/Tools/Prefabs/ManagePrefabs.cs Editor/Tools/Graphics/RendererFeatureOps.cs Editor/Tools/Graphics/VolumeOps.cs Editor/Tools/Graphics/SkyboxOps.cs Editor/Tools/Graphics/LightBakingOps.cs Editor/Tools/Vfx/ParticleControl.cs Editor/Tools/ProBuilder/ManageProBuilder.cs Editor/Helpers/GameObjectLookup.cs; do
  if [ -f "$PACKAGE/$file" ]; then
    sed -i '1s/^/#pragma warning disable CS0619\n/' "$PACKAGE/$file"
  fi
done

# 2. Replace GetInstanceID with GetEntityId.GetHashCode
for file in Editor/Resources/Editor/*.cs Editor/Helpers/*.cs Editor/Resources/Scene/*.cs Editor/Tools/**/*.cs; do
  if [ -f "$PACKAGE/$file" ]; then
    sed -i 's/\.GetInstanceID()/.GetEntityId().GetHashCode()/g' "$PACKAGE/$file"
  fi
done

# 3. Fix runtime serializer (ToString instead of GetHashCode)
sed -i 's/\.GetEntityId()\.GetHashCode()/.GetEntityId().ToString()/g' "$PACKAGE/Runtime/Serialization/UnityTypeConverters.cs"
```
