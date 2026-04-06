# Unity Debugging Setup for Tower Defense

This project is configured for debugging Unity C# code in VSCode.

## Prerequisites

1. **Install VSCode Extensions:**
   - **Visual Studio Tools for Unity** (recommended) - Already in `.vscode/extensions.json`
   - **C# Dev Kit** (Microsoft's official C# extension)
   - **.NET Install Tool**

   Install recommended extensions:
   ```bash
   code --install-extension visualstudiotoolsforunity.vstuc
   ```

2. **Unity Editor Configuration:**
   - Open Unity Editor
   - Go to `Edit > Preferences > External Tools`
   - Set External Script Editor to **Visual Studio Code**
   - Enable:
     - ✓ Generate .csproj files for:
       - ✓ Local Unity packages
       - ✓ Registry Unity packages
   - Click "Regenerate project files"

## Debugging Configurations

### 1. **Unity Editor** (F5)
- Launches the Unity Editor with debugger attached
- Automatically builds before launching
- Best for: Full debugging workflow

### 2. **Attach to Unity**
- Attaches debugger to a running Unity Editor instance
- Best for: When Unity is already running
- **Usage:** 
  1. Start Unity Editor manually
  2. Select this configuration and press F5
  3. Or use `Run > Start Debugging`

### 3. **Unity Player (Debug)**
- Launches Unity Player in debug mode
- Best for: Testing built player builds

## How to Debug

### Quick Start:
1. Open Unity project in Unity Editor
2. Open VSCode in the project root
3. Set breakpoints in your C# code (click left of line numbers)
4. Press `F5` or go to `Run > Start Debugging`
5. Select "Unity Editor" configuration

### Debug Controls:
- **F5**: Continue/Pause
- **F10**: Step Over
- **F11**: Step Into
- **Shift+F11**: Step Out
- **Shift+F5**: Stop Debugging

### Debug Features:
- **Breakpoints**: Click to the left of line numbers
- **Conditional Breakpoints**: Right-click breakpoint > Edit Breakpoint
- **Watch Variables**: Add to Watch panel in Debug view
- **Call Stack**: View in Debug view
- **Immediate Window**: Use Debug Console to evaluate expressions

## Troubleshooting

### Breakpoints not hitting?
1. Ensure Unity is set to use VSCode as external editor
2. Regenerate project files in Unity: `Assets > Open C# Project`
3. Make sure you're building in **Debug** configuration
4. Check that the debugger is attached (green debug bar should appear)

### "Unity" debugger type not found?
- Install the **Visual Studio Tools for Unity** extension
- Restart VSCode after installation

### Build errors?
- Run the build task: `Ctrl+Shift+B` (or `Cmd+Shift+B` on Mac)
- Check the Problems panel for errors
- Try: `dotnet restore` then rebuild

### Can't attach to Unity?
- Make sure Unity Editor is running
- In Unity, enable: `Edit > Preferences > External Tools > Enable C# Debugging`
- Try restarting both Unity and VSCode

## Build Tasks

**Important:** Unity handles C# compilation automatically. You don't need to manually build the project.

Unity will:
- Automatically compile C# scripts when files change
- Regenerate project files when needed
- Manage the build process internally

The build tasks in VSCode are for reference only and may not work with `.slnx` files.

## Additional Tips

- Use `Debug.Log()` for console output in Unity
- View logs in Unity Console window
- Use conditional breakpoints for complex conditions
- Use exception breakpoints: Debug Console > `catch` or `throw`
- Watch Unity-specific variables in the Variables panel

## Project Structure

```
TowerDefense/
├── .vscode/
│   ├── launch.json          # Debug configurations
│   ├── settings.json        # VSCode settings
│   ├── tasks.json           # Build tasks
│   └── extensions.json      # Recommended extensions
├── Assets/
│   └── UnityTechnologies/   # Main game scripts
└── TowerDefense.slnx        # Solution file
```

Happy debugging! 🐛
