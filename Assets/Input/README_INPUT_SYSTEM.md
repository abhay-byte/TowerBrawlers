# Input System Setup Guide

## Migration Complete! ✅

Your `PlayerController` has been migrated from the old `UnityEngine.Input` to the new **Unity Input System** (v1.19.0).

## What Changed

### Old System (Removed)
- `Input.GetAxisRaw("Horizontal")` and `Input.GetAxisRaw("Vertical")`
- `Input.GetKeyDown(KeyCode.Space)`

### New System (Added)
- `InputAction.ReadValue<Vector2>()` for movement
- `InputAction.performed` event for attacks
- Serialized `InputActionAsset` field in the Inspector

## Setup Instructions

### Step 1: Assign the Input Action Asset

1. **The Input Action asset has been created** at: `Assets/Input/PlayerInput.inputactions`

2. **Select your Player GameObject** in the Unity Editor

3. **In the Inspector**, find the `PlayerController` component

4. **Drag and drop** `Assets/Input/PlayerInput.inputactions` into the **"Input Actions"** field

### Step 2: Configure the Input Action Asset (If Needed)

If Unity doesn't automatically recognize the `.inputactions` file:

1. **Double-click** `Assets/Input/PlayerInput.inputactions` to open the Input Action editor

2. You should see:
   - **Player** action map with:
     - **Move** action (Vector2) - bound to WASD, Arrow Keys, and Gamepad Left Stick
     - **Attack** action (Button) - bound to Space bar and Gamepad Button South (A/Cross)

3. **Save** the asset (Ctrl+S / Cmd+S)

4. **Make sure "Generate C# Class" is DISABLED** (we're using `InputActionAsset` directly)

### Step 3: Test in Play Mode

1. Enter Play Mode
2. Test movement with **WASD** or **Arrow Keys**
3. Test attack with **Spacebar**
4. Check the Console for any warnings

## Controls

### Keyboard & Mouse
- **Movement**: W, A, S, D or Arrow Keys
- **Attack**: Spacebar

### Gamepad
- **Movement**: Left Stick
- **Attack**: Button South (A on Xbox, Cross on PlayStation)

## Troubleshooting

### Warning: "InputActionAsset not assigned"
- **Solution**: Make sure you've dragged `Assets/Input/PlayerInput.inputactions` into the "Input Actions" field on your PlayerController component

### Input Not Working
- **Solution 1**: Check that the InputActionAsset is properly assigned in the Inspector
- **Solution 2**: Make sure the Input System package is installed (Window > Package Manager > Input System)
- **Solution 3**: Check Project Settings > Player > Active Input Handling is set to "Input System Package (New)" or "Both"

### Old Input Errors Still Appearing
- **Solution**: Make sure no other scripts are using `UnityEngine.Input` - search your codebase for `Input.GetAxis` or `Input.GetKeyDown`

## Next Steps

For more advanced features, consider:
- **Player Input Component**: Use Unity's `PlayerInput` component for automatic input management
- **Generated C# Classes**: Enable C# class generation for type-safe input access
- **Input Action Maps**: Create separate maps for different game states (menu, gameplay, pause)
- **Action Callbacks**: Use `started`, `performed`, and `canceled` events for more granular control

## Reference

- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Input System Package (v1.19.0)](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/index.html)
