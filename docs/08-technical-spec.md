# 08. Technical Spec

## 1. Technical Goal

Build a mobile-first lane-based 3D tower defense strategy game using Unity 6.5 (6000.5 Beta LTS) with a stylized low-poly/cartoon aesthetic, optimized for Android and desktop platforms.

## 2. Engine Recommendation

**Unity 6.5 (6000.5 Beta)**

Recommended stack:

- **Unity 6.5** (Editor + Runtime)
- **C# 11+** (Primary scripting language)
- **Universal Render Pipeline (URP) 3D** (Rendering)
- **Unity UI Toolkit** (Runtime UI) + **UIElements** (Editor tooling)
- **Unity Input System** (Cross-platform input handling)
- **Addressables** (Asset management & streaming)
- **Cinemachine** (Camera control, battle framing)
- **DOTween** (Code-driven animation & transitions)

This stack leverages Unity's mature 3D ecosystem, DOTS-ready architecture path, and modern mobile optimization tools.

## 3. Why Unity 6.5

- **DOTS integration** - ECS, Jobs, Burst compiler for performance-critical systems (large-scale battles)
- **Modern URP 3D** - Optimized forward rendering with GPU instancing, ideal for mobile 3D
- **URP 2D Features within 3D** - Sprite rendering for UI elements, 2D lighting for battle effects
- **UI Toolkit** - Modern data-driven UI with USS styling
- **Addressables** - Advanced asset loading & memory management
- **Enhanced mobile optimization** - Better battery life, thermal management, Adaptive Performance
- **Scriptable Render Pipeline** - Custom rendering control for stylized visuals
- **Long-term support** - LTS stability for production
- **Mature C# ecosystem** - Modern language features, async/await, source generators

## 4. Core Architecture Systems

### Primary Unity Systems

#### Universal Render Pipeline (URP) 3D

Used for:

- Stylized low-poly 3D battlefield rendering
- Forward+ rendering path for mobile efficiency
- GPU instancing for units, towers, projectiles
- Custom URP Renderer Features (outline effects, damage flash)
- Post-processing (vignette, color grading, bloom, depth of field)
- Real-time & baked lighting mix
- Shadow cascades (2 cascades for mobile, 4 for desktop)
- Screen-space ambient occlusion (SSAO) for depth

#### Unity Input System

Used for:

- Touch input (multi-touch, drag-to-deploy, hold for tooltips)
- Mouse/keyboard (desktop, click-to-build, hover tooltips)
- Controller support (gamepad-ready, D-pad navigation)
- Action-based input abstraction (Battle, Menu, Camera maps)
- Input rebinding support (desktop)

#### Addressable Asset System

Used for:

- Asset bundling & streaming (factions, levels, troops)
- Memory management (load/unload battle content)
- Remote content updates (balance patches, new units)
- Platform-specific asset variants (texture compression)
- Build size optimization (APK splitting)
- Async asset loading (no blocking during battle)

#### UI Toolkit (Runtime)

Used for:

- Battle HUD (tower HP, resources, cards, abilities)
- Menu screens (main menu, loadout, world map)
- Data binding support (resource counters, tower HP)
- Responsive layout system (safe areas, aspect ratios)
- Theme & style sheets (USS) for faction-specific styling
- UXML layout definitions for clean separation
- Transition animations (panel slides, fades)

#### Cinemachine

Used for:

- Battle camera framing (side-view lane perspective)
- Dynamic follow camera (tracks lane action)
- Camera shake (tower damage, explosions)
- Smooth transitions (battle intro, victory screen)
- Virtual cameras for cutscenes & mission intros
- Priority-based camera blending

### Secondary Unity Packages

#### Core Packages

| Package | Purpose |
|---------|---------|
| **URP 3D** | Main rendering pipeline |
| **TextMeshPro** | High-quality text rendering (HUD, menus) |
| **Audio Mixer** | Sound routing, effects, ducking, phase transitions |
| **Particle System (Shuriken)** | Combat VFX, projectile trails, explosions |
| **Animation** | Unit state machines, tower animations |
| **Timeline** | Mission intros, cutscenes, scripted sequences |
| **NavMesh** | Enemy pathfinding along lane |
| **Profiler** | Performance analysis (CPU, GPU, memory) |
| **Unity Analytics** | Telemetry, crash reporting, player behavior |

#### Performance & Architecture Packages

| Package | Purpose |
|---------|---------|
| **DOTS (Entities, Jobs, Burst)** | High-performance simulation (100+ units) |
| **Collections** | Native containers for DOTS |
| **Mathematics** | SIMD-optimized math (vector ops, matrix transforms) |
| **Memory Profiler** | Memory leak detection, snapshot comparison |
| **Adaptive Performance** | Automatic quality scaling for thermal management (Android) |

#### Visual Effects

| Package | Purpose |
|---------|---------|
| **Visual Effect Graph** | GPU-accelerated particle systems (large-scale battles) |
| **Shader Graph** | Custom stylized shaders (toon shading, damage flash, outlines) |
| **DOTween** | Code-driven animation (UI transitions, projectile motion) |

## 5. Target Platforms

### Primary

- **Android 10+** (API level 29+)
  - Phones (16:9, 18:9, 19.5:9, 20:9)
  - Tablets (4:3, 16:10)
  - Foldables (aspect ratio transitions)

### Secondary

- **Windows 10+** (64-bit, x86_64 & ARM64)
- **Linux** (Ubuntu 20.04+, 64-bit)
- **iOS** (future consideration, iOS 14+)

## 6. Minimum Gameplay Specs

### Android Minimum Target

- **OS:** Android 10+ (API 29)
- **RAM:** 4 GB minimum
- **GPU:** Adreno 506+ / Mali-G71+ / PowerVR Rogue GE8320+
- **CPU:** Snapdragon 660 / Exynos 9610 / Kirin 710 equivalent
- **Storage:** 500 MB install size
- **Screen:** 720p minimum, 16:9 baseline

### Desktop Minimum Target

- **OS:** Windows 10 / Ubuntu 20.04 LTS (64-bit)
- **RAM:** 4 GB
- **GPU:** Intel UHD 620+ / GTX 750 Ti+ / AMD Radeon R7 260X+
- **CPU:** Intel i3-8100 / AMD Ryzen 3 1200 equivalent
- **Storage:** 500 MB SSD preferred

## 7. Resolution and Display Targets

### Supported Aspect Ratios

- 16:9 (standard landscape battle)
- 18:9 (tall mobile - portrait for menus, landscape for battle)
- 19.5:9 (modern phones)
- 20:9 (ultra-wide mobile)
- 4:3 (tablets, iPad)
- 16:10 (Android tablets)

### Display Strategy

#### Orientation

- **Portrait orientation:** Menus, meta progression, world map, shop, loadout
- **Landscape orientation:** Battle screen (primary gameplay)
- **Auto-rotate:** Prompt player, save preference

#### Safe Areas

- Respect notch/cutout regions
- Use Unity SafeArea API for Android/iOS
- Pad critical UI elements (tower HP, resource counters, cards)

#### Camera Framing (Battle)

- **Side-view perspective** (slight angle, ~30-45 degrees)
- **Single lane** centered in frame
- **Player tower** (left side)
- **Enemy tower** (right side)
- **Cinemachine framing** keeps both towers visible at all times
- **Dynamic zoom** based on lane action (optional)

#### Responsive UI

- Canvas scaler with reference resolution:
  - Portrait: 1080x1920
  - Landscape: 1920x1080
- Adaptive layouts: Dynamic panel arrangement based on screen real estate
- Touch target size: Minimum 44x44px (mobile guideline)

## 8. Architecture Overview

### 8.1 Core Layers

#### Simulation Layer

**Responsibility:**
- Unit pathfinding & movement along lane (NavMesh or custom lane logic)
- Combat resolution & damage calculations (pure C#)
- Tower/structure logic & cooldowns
- Resource generation (Snacks, Drama) & consumption
- Wave spawning & timing (enemy AI)
- Phase cycle logic (Tea Time vs Tantrum Time)
- Victory/defeat conditions (tower HP)

**Rule:** 
- Keep simulation **deterministic** and **frame-rate independent**
- Use **fixed timestep** for game logic (`Time.fixedDeltaTime`)
- Decouple from rendering (no direct `Transform` manipulation in simulation)
- Pure C# classes where possible (testable without Unity runtime)

**Implementation:**
- Pure C# data classes for simulation state
- `ScriptableObject` for configuration data (unit stats, tower data, waves)
- Event-driven architecture (`C# events`, `UnityEvent`, or custom `EventBus`)
- Consider **Entities (DOTS)** for large-scale battles (50+ units on lane)

**Example Architecture:**

```csharp
// Pure C# simulation (no Unity dependencies)
public class BattleSimulation
{
    public float SnackGenerationRate { get; set; }
    public float DramaMeter { get; private set; }
    public PhaseCycle CurrentPhase { get; private set; }
    
    public void Update(float deltaTime) { ... }
    public void DeployUnit(UnitData data) { ... }
    public void ResolveCombat(Unit attacker, Unit defender) { ... }
}
```

#### Presentation Layer

**Responsibility:**
- 3D model rendering (units, towers, environment)
- Animation (state machines: idle, walk, attack, die)
- Particle effects (combat VFX, projectile trails, explosions)
- Camera control (Cinemachine: framing, shake, transitions)
- UI rendering & transitions (UI Toolkit)
- Audio playback & spatialization (Audio Mixer)
- Post-processing (phase-based color shifts, vignette)
- Screen shake, flash, slow-motion effects

**Rule:**
- Presentation **observes** simulation, never drives it
- Use `Animator` for state-driven animation (Mecanim)
- Use `Coroutine` or `DOTween` for sequenced effects
- Pool frequently spawned objects (projectiles, units, particles)
- Use Shader Graph for custom stylized effects (toon shading, outlines)

**Example Pattern:**

```csharp
// Presentation observes simulation events
public class UnitView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem hitVFX;
    
    public void Initialize(UnitSimulation sim)
    {
        sim.OnDamaged += PlayHitAnimation;
        sim.OnDied += PlayDeathAnimation;
    }
    
    private void PlayHitAnimation() 
    {
        animator.SetTrigger("Hit");
        hitVFX.Play();
    }
}
```

#### Content Layer

**Responsibility:**
- Tower definitions (stats, levels, visuals, faction identity)
- Enemy definitions (stats, behaviors, drops, wave composition)
- Unit definitions (roles, costs, traits, faction-specific abilities)
- Wave compositions & timing (enemy AI patterns)
- Level layouts (lane geometry, structure slots, environment)
- Item definitions & effects (consumables, equipment)
- Mission data (objectives, rewards, dialogue, intros)
- Phase cycle definitions (Tea Time vs Tantrum Time effects)

**Recommended format:**
- **`ScriptableObject`** for in-editor data authoring (designer-friendly)
- **JSON/Addressables** for remote/live updates (balance patches)
- **Custom editors** for data validation & workflow tools

**Rule:**
- All gameplay data should be **data-driven**, not hardcoded
- Designers should be able to balance content without code changes
- Every content type needs a custom inspector for validation

**Example ScriptableObject:**

```csharp
[CreateAssetMenu(menuName = "Tower Brawlers/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitID;
    public string displayName;
    public string flavorText; // Comedy!
    public UnitRole role; // Frontliner, Ranged, Breaker, etc.
    public Faction faction; // Sunnybottom, Mudsnout
    public GameObject prefab; // 3D model
    public int snackCost;
    public UnitStats baseStats; // HP, damage, attack speed, range
    public List<UpgradeData> upgrades;
}
```

#### Meta Layer

**Responsibility:**
- Player progression & unlocks (units, towers, levels)
- Save/load state management (Coins, rewards, settings)
- Settings (audio, display, controls, accessibility)
- Analytics tracking (battle outcomes, popular strategies)
- Achievement tracking (victory conditions, special objectives)
- Optional: Cloud sync, remote config (Unity Services)

**Implementation:**
- Local: `Newtonsoft.Json` serialization (robust, versioned)
- Cloud: Unity Services Cloud Save (optional, post-MVP)
- Versioned save formats for forward compatibility
- Auto-save after battle completion, major unlocks

### 8.2 Suggested Package Layout

```
Assets/
├── Scripts/
│   └── Tower Brawlers/
│       ├── Core/
│       │   ├── GameManager.cs              # Global state, lifecycle, scene management
│       │   ├── EventBus.cs                 # Decoupled communication (publish/subscribe)
│       │   └── ServiceLocator.cs           # Dependency injection (optional)
│       │
│       ├── Game/
│       │   ├── GameManager.cs              # Match lifecycle (start, pause, end)
│       │   ├── LevelManager.cs             # Level state, transitions, objectives
│       │   ├── SaveManager.cs              # Persistence logic, versioning
│       │   └── ProgressionManager.cs       # Unlocks, upgrades, meta-progression
│       │
│       ├── Battle/
│       │   ├── Simulation/
│       │   │   ├── BattleSimulation.cs     # Core simulation loop (pure C#)
│       │   │   ├── UnitSimulation.cs       # Unit logic (HP, damage, traits)
│       │   │   ├── TowerSimulation.cs      # Tower logic (HP, defenses)
│       │   │   ├── PhaseCycle.cs           # Tea Time vs Tantrum Time
│       │   │   ├── ResourceManager.cs      # Snacks, Drama, Coins tracking
│       │   │   └── WaveManager.cs          # Enemy spawn logic, timing, AI
│       │   │
│       │   ├── Presentation/
│       │   │   ├── UnitView.cs             # 3D model, animation, VFX
│       │   │   ├── TowerView.cs            # Tower model, damage states
│       │   │   ├── ProjectileView.cs       # Visual projectile (model, trail)
│       │   │   ├── EffectPlayer.cs         # VFX, SFX triggers
│       │   │   └── BattleCamera.cs         # Cinemachine controller
│       │   │
│       │   └── Structures/
│       │       ├── Structure.cs            # Base structure logic
│       │       ├── EconomyStructure.cs     # Snack Shack, Slop Bucket
│       │       ├── DefenseStructure.cs     # Teacup Turret, Bone Barricade
│       │       ├── UtilityStructure.cs     # Praise Podium, Yell Drum
│       │       └── BuildingManager.cs      # Placement, upgrades, validation
│       │
│       ├── Units/
│       │   ├── Data/
│       │   │   ├── UnitData.cs             # ScriptableObject (stats, traits)
│       │   │   ├── UnitStats.cs            # HP, damage, speed, range
│       │   │   └── UpgradeData.cs          # Upgrade paths, costs
│       │   │
│       │   ├── Player/
│       │   │   ├── UnitAgent.cs            # Movement, pathfinding along lane
│       │   │   ├── UnitAI.cs               # Auto-attack, target selection
│       │   │   └── UnitDeployment.cs       # Spawn logic, cost validation
│       │   │
│       │   └── Enemies/
│       │       ├── EnemyAgent.cs           # Enemy-specific movement, behavior
│       │       ├── EnemyAI.cs              # Wave-based spawning, targeting
│       │       └── BossAgent.cs            # Boss patterns, phase mechanics
│       │
│       ├── Economy/
│       │   ├── ResourceManager.cs          # Snack generation, Drama meter
│       │   ├── LootManager.cs              # Drops, rewards, pickups
│       │   └── ShopManager.cs              # Meta-progression shop (Coins)
│       │
│       ├── Items/
│       │   ├── ItemData.cs                 # ScriptableObject (consumables)
│       │   ├── ItemManager.cs              # Pre-battle equip, in-battle use
│       │   └── ConsumableEffect.cs         # Base class for item effects
│       │
│       ├── UI/
│       │   ├── HUD/
│       │   │   ├── BattleHUD.cs            # In-battle UI (tower HP, resources, cards)
│       │   │   ├── TowerInfoPanel.cs       # Tower stats, upgrade options
│       │   │   ├── PhaseIndicator.cs       # Tea Time vs Tantrum Time display
│       │   │   └── WaveIndicator.cs        # Wave progress, enemy preview
│       │   │
│       │   ├── Menus/
│       │   │   ├── MainMenu.cs
│       │   │   ├── WorldMap.cs             # Campaign progression, mission nodes
│       │   │   ├── LevelSelect.cs
│       │   │   ├── LoadoutScreen.cs        # Pre-battle troop/item selection
│       │   │   └── SettingsMenu.cs
│       │   │
│       │   └── Meta/
│       │       ├── ResultsScreen.cs        # Victory/defeat, rewards, retry
│       │       ├── ProgressionScreen.cs    # Unlocks, upgrades, spending Coins
│       │       └── RewardScreen.cs
│       │
│       ├── Content/
│       │   ├── ScriptableObjects/
│       │   │   ├── UnitData.cs
│       │   │   ├── TowerData.cs
│       │   │   ├── EnemyData.cs
│       │   │   ├── StructureData.cs
│       │   │   ├── ItemData.cs
│       │   │   ├── LevelData.cs
│       │   │   ├── WaveData.cs
│       │   │   └── MissionData.cs
│       │   │
│       │   └── DataFiles/                  # JSON exports, addressables
│       │
│       ├── Audio/
│       │   ├── AudioManager.cs             # Mixer control, pooling, ducking
│       │   ├── SFXPlayer.cs                # One-shot sounds (combat, UI)
│       │   └── MusicManager.cs             # Background music, phase transitions
│       │
│       ├── Input/
│       │   ├── BattleInput.cs              # Touch/mouse input (deploy, build)
│       │   └── UIInput.cs                  # Menu navigation, tooltips
│       │
│       ├── Camera/
│       │   ├── BattleCameraController.cs   # Cinemachine setup for lane view
│       │   └── CameraTransitions.cs        # Intro, victory, cutscenes
│       │
│       └── Utilities/
│           ├── ObjectPool.cs               # Generic pooling (units, projectiles, VFX)
│           ├── Timer.cs                    # Time-based triggers, cooldowns
│           ├── Extensions.cs               # C# extension methods
│           └── Helpers.cs                  # Math, vector, color utilities
│
├── Art/
│   ├── Models/                             # 3D models (FBX, Blender)
│   │   ├── Units/
│   │   │   ├── Sunnybottom/
│   │   │   └── Mudsnout/
│   │   ├── Towers/
│   │   ├── Structures/
│   │   ├── Environment/
│   │   └── Projectiles/
│   │
│   ├── Textures/                           # PBR textures, stylized materials
│   ├── Animations/                         # Animation clips, controllers
│   ├── Materials/                          # URP materials, Shader Graph
│   ├── Prefabs/                            # Runtime prefabs (units, towers, VFX)
│   └── VFX/                                # Particle systems, effects
│
├── Addressables/
│   ├── Units/                              # Addressable unit prefabs
│   ├── Towers/                             # Addressable tower prefabs
│   ├── Levels/                             # Addressable level scenes
│   └── UI/                                 # Addressable UI panels
│
├── Scenes/
│   ├── Boot.unity                          # Initialization, service setup
│   ├── MainMenu.unity                      # Title screen, main menu
│   ├── Battle.unity                        # Core battle scene (reusable)
│   └── Loading.unity                       # Async loading screen
│
├── UI/
│   ├── UIToolkit/
│   │   ├── BattleHUD.uxml
│   │   ├── MainMenu.uxml
│   │   └── ...
│   └── Styles/
│       ├── SunnybottomTheme.uss
│       ├── MudsnoutTheme.uss
│       └── ...
│
└── Audio/
    ├── SFX/
    │   ├── Units/
    │   ├── Towers/
    │   ├── Structures/
    │   ├── UI/
    │   └── Environment/
    └── Music/
        ├── Menu/
        ├── Battle/
        └── PhaseTransitions/
```

### 8.3 DOTS Architecture (Optional for Large Scale)

If battlefield complexity exceeds 100+ units or performance degrades:

```
Assets/
└── DOTS/
    ├── Authoring/                          # MonoBehaviour → IComponentData
    │   ├── UnitAuthoring.cs
    │   ├── TowerAuthoring.cs
    │   └── ProjectileAuthoring.cs
    │
    ├── Components/
    │   ├── UnitComponent.cs                # Position, HP, damage (pure data)
    │   ├── MovementComponent.cs
    │   ├── CombatComponent.cs
    │   └── FactionComponent.cs             # Sunnybottom, Mudsnout
    │
    ├── Systems/
    │   ├── MovementSystem.cs               # ISystem (lane movement)
    │   ├── CombatSystem.cs                 # Damage calculation, auto-attack
    │   ├── SpawnerSystem.cs                # Wave spawning
    │   └── CleanupSystem.cs                # Dead unit removal
    │
    └── Jobs/
        ├── MovementJob.cs                  # IJobChunk (parallel movement)
        └── CombatJob.cs                    # IJobChunk (parallel combat)
```

**Rule:** 
- Start with **MonoBehaviour + pure C#** architecture
- Migrate to **DOTS** only when profiler identifies CPU bottleneck
- Use **Burst compiler** for math-heavy operations (damage calculations)

## 9. Rendering Approach

### Stylized 3D Battlefield

**Visual Style:**
- Low-poly/cartoon aesthetic (exaggerated proportions, bright colors)
- Toon/cel shading via Shader Graph (Sunnybottom: polished, Mudsnout: rough)
- Outlines for unit readability (custom URP Renderer Feature)
- Faction-specific color palettes (Sunnybottom: gold/white, Mudsnout: brown/green)
- Exaggerated animations (theatrical, comedic timing)

**Rendering Pipeline:**

| Feature | Mobile Setting | Desktop Setting |
|---------|---------------|-----------------|
| **Render Path** | Forward | Forward+ |
| **Shadow Cascades** | 2 | 4 |
| **Shadow Resolution** | 512 | 2048 |
| **Anti-Aliasing** | FXAA | SMAA |
| **Post-Processing** | Lightweight (vignette, color grading) | Full (bloom, DOF, SSAO) |
| **Max Draw Calls** | 1500 | 3000+ |
| **Max Triangles** | 200k | 500k+ |

**Lighting Strategy:**
- **1 directional light** (sun, with soft shadows)
- **Baked GI** for environment (static towers, background)
- **Real-time lights** for dynamic effects (projectiles, explosions)
- **Reflection probes** for polished Sunnybottom materials (baked)
- **Emissive materials** for Drama meter, ability glow effects

**Post-Processing:**
- **Vignette** (focus attention on lane)
- **Color grading** (phase-based: warm for Tea Time, saturated for Tantrum Time)
- **Bloom** (ability activation, special effects)
- **Depth of field** (optional, desktop only)
- **Chromatic aberration** (tower damage, low HP warning)

### Animation Strategy

**Unit Animation:**
- **State-driven Animator** (Mecanim): idle, walk, attack, hit, die
- **Blend trees** for movement speed variations
- **Animation events** for attack timing, VFX triggers
- **Root motion** disabled (lane movement driven by simulation)

**Tower Animation:**
- **Damage states** (undamaged → damaged → destroyed)
- **Attack animations** (Teacup Turret firing, Bone Barricade blocking)
- **Idle animations** (subtle movement, faction-specific flavor)
- **Destruction sequence** (collapsed tower, particle debris)

**UI Animation:**
- **DOTween** for code-driven transitions (panel slides, fades, pops)
- **UI Toolkit transitions** for data binding updates (resource counters)
- **Particle UI effects** (sparkles on unlock, Drama meter glow)

**Camera Animation:**
- **Cinemachine** for battle intro (pan across lane, show both towers)
- **Camera shake** (tower damage, explosions, boss mechanics)
- **Smooth follow** (tracks lane action, keeps both towers in frame)
- **Timeline** for mission intros, victory sequences

### Performance Targets

| Metric | Mobile Target | Desktop Target |
|--------|--------------|----------------|
| **FPS** | 60 FPS (mid-range), 30 FPS (low-end fallback) | 60+ FPS (uncapped option) |
| **Draw calls** | < 1500 per frame | < 3000 per frame |
| **Triangles** | < 200k per frame | < 500k per frame |
| **Texture memory** | < 300 MB | < 600 MB |
| **Load time** | < 3 seconds (battle scene) | < 2 seconds |
| **APK size** | < 500 MB | N/A |

## 10. Asset Pipeline

### Asset Organization

```
Assets/
├── Art/
│   ├── Models/                             # Source files (Blender, FBX)
│   │   ├── Units/
│   │   │   ├── Sunnybottom/
│   │   │   │   ├── SpoonGuard.fbx
│   │   │   │   ├── PeaShooterArcher.fbx
│   │   │   │   └── ...
│   │   │   └── Mudsnout/
│   │   │       ├── MudGrunter.fbx
│   │   │       ├── ForkFlinger.fbx
│   │   │       └── ...
│   │   ├── Towers/
│   │   │   ├── SunnybottomTower.fbx
│   │   │   └── MudsnoutTower.fbx
│   │   ├── Structures/
│   │   ├── Environment/
│   │   └── Projectiles/
│   │
│   ├── Textures/
│   │   ├── Units/
│   │   ├── Towers/
│   │   └── Environment/
│   │
│   ├── Materials/
│   │   ├── ToonShading/                    # Shader Graph materials
│   │   ├── Sunnybottom/                    # Polished, gold, white
│   │   └── Mudsnout/                       # Rough, brown, green
│   │
│   ├── Animations/
│   │   ├── Units/
│   │   └── Towers/
│   │
│   ├── Prefabs/                            # Runtime-ready prefabs
│   │   ├── Units/
│   │   ├── Towers/
│   │   ├── Structures/
│   │   └── VFX/
│   │
│   └── VFX/                                # Particle systems
│       ├── Combat/
│       ├── Projectiles/
│       └── Environment/
│
├── Addressables/
│   ├── Units/                              # Addressable groups
│   ├── Towers/
│   ├── Levels/
│   └── UI/
│
└── Audio/
    ├── SFX/
    │   ├── Units/
    │   ├── Towers/
    │   ├── Structures/
    │   ├── UI/
    │   └── Environment/
    └── Music/
        ├── Menu/
        ├── Battle/
        └── PhaseTransitions/
```

### Pipeline Rules

1. **Texture compression:** ASTC (6x6 or 8x8) for mobile, DXT5 for desktop
2. **Model optimization:** 
   - Low-poly targets (500-2000 triangles per unit)
   - Shared materials across units (reduce draw calls)
   - GPU instancing enabled for repeated objects (projectiles, environment)
3. **Prefab validation:**
   - All prefabs must reference Addressable assets
   - Missing reference check before commit
   - Prefab variant workflow for upgrades (base → upgraded versions)
4. **Audio:** OGG/Vorbis for music, ADPCM for SFX
5. **Addressable labels:** Tag assets for runtime loading by faction, level, type
6. **Source art:** Keep Blender/PSD source in separate `SourceArt/` folder (not in Unity project)
7. **Version control:** Use Unity YAML merge tools, lock binary files

### Model Guidelines

| Asset Type | Triangle Budget | Texture Size | Material Count |
|------------|----------------|--------------|----------------|
| **Unit** | 500-2000 | 512x512 or 1024x1024 | 1-2 |
| **Tower** | 2000-5000 | 1024x1024 or 2048x2048 | 2-4 |
| **Structure** | 300-1000 | 512x512 | 1-2 |
| **Environment** | 1000-5000 | 1024x1024 | 2-4 |
| **Projectile** | 50-200 | 128x128 or 256x256 | 1 |

## 11. Data-Driven Design

The game should be **heavily data-driven**. All gameplay content should be configurable without code changes.

### Data-Driven Content

| Content Type | Storage Format | Editor Integration |
|--------------|----------------|-------------------|
| Units | `ScriptableObject` + JSON export | Custom inspector, role icons, validation |
| Towers | `ScriptableObject` + JSON export | HP display, faction preview |
| Structures | `ScriptableObject` + JSON export | Structure slot validation, upgrade paths |
| Waves | `ScriptableObject` / JSON | Wave preview editor, timing visualization |
| Levels | `ScriptableObject` + scene reference | Level builder tools, objective setup |
| Items | `ScriptableObject` | Item database inspector, effect preview |
| Missions | `ScriptableObject` + JSON | Mission flow graph, dialogue editor |
| Phase Cycles | `ScriptableObject` | Timing, effect preview, faction bonuses |

### Benefits

- **Easy balancing:** Designers tweak stats in inspector (Snacks cost, HP, damage)
- **Less code churn:** Content changes don't touch code
- **Workflow tools:** Custom editors for level/wave/unit creation
- **Remote updates:** JSON can be patched without app update (balance changes)
- **A/B testing:** Remote config integration ready (difficulty tuning)

### ScriptableObject Architecture

**Example: Unit Data**

```csharp
[CreateAssetMenu(menuName = "Tower Brawlers/Units/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    public string unitID;
    public string displayName;
    public string flavorText; // "Deeply proud of being issued a spoon-shaped shield"
    public UnitRole role; // Frontliner, Ranged, Breaker, Support, Siege, Skirmisher
    public Faction faction; // Sunnybottom, Mudsnout
    
    [Header("Visuals")]
    public GameObject prefab; // 3D model prefab
    public Sprite cardIcon; // UI card image
    public Color factionColor;
    
    [Header("Economy")]
    public int snackCost;
    public float deploymentCooldown;
    
    [Header("Stats")]
    public UnitStats baseStats;
    public List<UpgradeData> upgradeLevels;
    
    [Header("Abilities")]
    public UnitTrait trait; // Special behavior (e.g., "blocks frontal damage")
    public string traitDescription;
}

[System.Serializable]
public struct UnitStats
{
    public int maxHP;
    public int damage;
    public float attackSpeed; // Attacks per second
    public float range;
    public float movementSpeed; // Along lane
}
```

**Example: Wave Data**

```csharp
[CreateAssetMenu(menuName = "Tower Brawlers/Waves/WaveData")]
public class WaveData : ScriptableObject
{
    public string waveID;
    public List<WaveSpawn> spawns;
    public float waveDuration;
    public PhaseCycle requiredPhase; // Optional: wave only spawns in Tea Time or Tantrum Time
    
    [Header("Rewards")]
    public int coinReward;
    public List<ItemDrop> possibleDrops;
}

[System.Serializable]
public struct WaveSpawn
{
    public UnitData unit;
    public float spawnTime; // Seconds from wave start
    public SpawnPosition position; // Enemy side
    public int count;
    public float spawnInterval; // Seconds between units
}
```

**Rule:** 
- All content data lives in `ScriptableObject` for editor workflow
- Export to JSON for remote/Addressables loading if needed
- Custom inspectors validate data (missing prefabs, invalid costs, broken references)

### Custom Editor Tools

**Priority Tools for Designer Workflow:**

| Tool | Purpose | Priority |
|------|---------|----------|
| **Unit Database Inspector** | View/edit all units, validate stats, preview cards | High |
| **Wave Editor** | Timeline-like UI for spawn timing, unit composition | High |
| **Level Builder** | Scene setup, structure slot placement, lane geometry | High |
| **Mission Editor** | Objectives, rewards, dialogue, intro cutscenes | Medium |
| **Stat Balancer** | Spreadsheet import/export for unit/wave tuning | Medium |
| **Content Validator** | Scan for missing references, invalid data, cost imbalances | High |
| **Battle Sandbox** | Quick-play testing with debug controls (spawn units, skip waves) | High |
| **Performance Overlay** | Runtime FPS, memory, draw call counter | Medium |

## 12. Save System

### Save Data Structure

```csharp
[System.Serializable]
public class GameSaveData
{
    public int saveVersion; // For migration
    public string saveTimestamp;
    
    [Header("Progression")]
    public PlayerProgression progression; // Unlocked units, levels, upgrades
    public List<string> completedMissions;
    public Dictionary<string, int> missionStars; // 1-3 stars per mission
    
    [Header("Economy")]
    public int coins;
    public Dictionary<string, int> itemInventory;
    
    [Header("Equipment")]
    public EquippedLoadout currentLoadout; // Pre-battle setup
    
    [Header("Settings")]
    public AudioSettings audioSettings;
    public DisplaySettings displaySettings;
    public InputSettings inputSettings;
    
    [Header("Analytics")]
    public int totalBattlesWon;
    public int totalBattlesLost;
    public string favoriteUnit;
}

[System.Serializable]
public class PlayerProgression
{
    public List<string> unlockedUnits; // Unit IDs
    public List<string> unlockedStructures;
    public List<string> unlockedItems;
    public int currentChapter;
    public int currentMission;
}
```

### Implementation

| Aspect | Approach |
|--------|----------|
| **Format** | JSON (`Newtonsoft.Json` for robustness, versioning support) |
| **Storage** | `Application.persistentDataPath` (platform-specific safe location) |
| **Versioning** | Save version field + migration logic (handle old save formats) |
| **Validation** | Checksum/hash to prevent corruption (detect tampered/invalid saves) |
| **Cloud** | Unity Cloud Save (optional, post-MVP) |
| **Auto-save** | After battle completion, major unlocks, settings changes |
| **Manual save** | Player-triggered (settings menu, before risky actions) |
| **Backup** | Keep previous save version (recover from corruption) |

### Save Contents

- Unlocked missions & levels (campaign progress)
- Completion stars & rewards (1-3 stars per mission)
- Unit levels & upgrades (if unit progression system exists)
- Equipped items & inventory (pre-battle loadout)
- Currency & resource counts (Coins, consumables)
- Settings (audio, display, input, accessibility)
- Analytics consent & preferences
- Battle statistics (wins, losses, favorite units)

### Migration Example

```csharp
public class SaveManager
{
    private const int CURRENT_SAVE_VERSION = 1;
    
    public GameSaveData LoadSave()
    {
        string json = File.ReadAllText(savePath);
        GameSaveData save = JsonConvert.DeserializeObject<GameSaveData>(json);
        
        if (save.saveVersion < CURRENT_SAVE_VERSION)
        {
            save = MigrateSave(save);
        }
        
        return save;
    }
    
    private GameSaveData MigrateSave(GameSaveData oldSave)
    {
        // Handle version migration
        if (oldSave.saveVersion == 0)
        {
            // Add new fields, convert old data
        }
        oldSave.saveVersion = CURRENT_SAVE_VERSION;
        return oldSave;
    }
}
```

## 13. Input Requirements

### Mobile (Touch)

| Input | Action |
|-------|--------|
| **Tap** | Select unit card, confirm deployment, place structure |
| **Long press (hold)** | Tooltip (unit stats, structure info, ability description) |
| **Drag** | Drag unit card to lane deployment zone, drag structure to slot |
| **Tap outside** | Cancel selection, close tooltip |
| **Swipe** | Avoid for critical gameplay actions (gesture conflicts) |

### Desktop (Mouse/Keyboard)

| Input | Action |
|-------|--------|
| **Left click** | Select, confirm, deploy, build |
| **Right click** | Cancel, deselect, close panel |
| **Hover** | Tooltip (unit stats, structure info), highlight valid targets |
| **Scroll wheel** | Zoom in/out (optional), cycle card selection |
| **Keyboard** | Hotkeys (1-5 for cards), pause (Escape), speed toggle (Space) |

### Input System Architecture

**Unity Input System Setup:**

```
Input Actions (asset)
├── Battle (action map)
│   ├── DeployUnit (action: tap/click on card, drag to lane)
│   ├── BuildStructure (action: tap/click on structure slot)
│   ├── UseAbility (action: tap/click ability button)
│   ├── Pause (action: Escape key, UI button)
│   └── CameraPan (action: drag, edge scroll, WASD)
│
├── Menu (action map)
│   ├── Navigate (action: tap/click, D-pad, arrow keys)
│   ├── Confirm (action: tap/click, Enter, A button)
│   ├── Cancel (action: Escape, B button)
│   └── Tooltip (action: long press, hover)
│
└── Camera (action map)
    ├── Pan (action: drag, edge scroll, WASD)
    ├── Zoom (action: pinch, scroll wheel)
    └── Reset (action: double-tap, home key)
```

**Implementation Rules:**
- **Action Maps:** Contextual input modes (auto-switched: Battle → Menu → Camera)
- **Input Processors:** Dead zones (analog), normalization, sensitivity
- **Input Interactions:** Tap, hold, multi-tap, press/release
- **Rebinding:** Player-configurable keybinds (desktop, settings menu)

### Accessibility

- **Remappable controls** (desktop hotkeys, mobile gesture alternatives)
- **Touch target size** (minimum 44x44px per mobile guideline)
- **Colorblind-friendly palettes** (faction colors: distinct in all modes)
- **Haptic feedback** (mobile: deployment, tower damage, victory)
- **Screen reader support** (UI elements, tooltips via Unity Accessibility API)
- **Reduced motion option** (disable screen shake, particle effects)
- **High contrast mode** (toggle for visibility in bright/dark environments)

## 14. Performance Goals

### Frame Rate Targets

| Platform | Target | Fallback |
|----------|--------|----------|
| High-end Android (Snapdragon 860+) | 60 FPS | 30 FPS |
| Mid-range Android (Snapdragon 680) | 60 FPS | 30 FPS |
| Low-end Android (Snapdragon 460) | 30 FPS | Stable 30 |
| Desktop (GTX 1050+) | 60+ FPS | Uncapped |

### Memory Budget

| Category | Mobile Budget | Desktop Budget |
|----------|--------------|----------------|
| **Textures** | 300 MB | 600 MB |
| **Audio** | 150 MB | 200 MB |
| **Meshes/Models** | 100 MB | 200 MB |
| **Runtime memory** | 400 MB max | 800 MB max |
| **Total APK/IPA size** | 500 MB max | N/A |

### Loading Performance

| Scene Type | Target Load Time |
|------------|-----------------|
| Battle scene | < 3 seconds (mobile), < 2 seconds (desktop) |
| Menu scene | < 2 seconds |
| Transition (fade) | < 1 second |
| First launch | < 10 seconds |

### Optimization Strategy

1. **Profiler-first:** Identify bottlenecks before optimizing (Unity Profiler, Frame Debugger)
2. **Object pooling:** Reuse projectiles, units, effects (avoid Instantiate/Destroy during battle)
3. **Addressables:** Async loading, memory management (load battle content on demand)
4. **GPU instancing:** Shared materials for repeated objects (units, projectiles, environment)
5. **Level of detail (LOD):** Reduce mesh detail for distant objects (optional, desktop)
6. **Culling:** Disable off-screen updates (lane units outside camera frustum)
7. **Jobs/Burst:** Offload simulation to worker threads (DOTS path for 100+ units)
8. **Static batching:** Mark non-moving objects as static (environment, towers)
9. **Texture streaming:** Load textures at appropriate resolution (Addressables)
10. **Audio pooling:** Pool AudioSources (avoid playback delays)

### Mobile-Specific Optimization

- **Adaptive Performance** (Unity package): Auto-scale quality based on thermal state
- **ASTC texture compression**: Best quality/size ratio for Android
- **Forward rendering path**: More efficient on mobile GPUs
- **Reduced shadow cascades**: 2 cascades (vs 4 on desktop)
- **Baked lighting**: Pre-calculate environment lighting (reduce real-time lights)
- **Disable post-processing on low-end**: Bloom, DOF, SSAO are expensive

### Performance Monitoring

- **Runtime FPS counter** (debug build, toggle in settings)
- **Memory usage overlay** (debug build)
- **Draw call counter** (debug build, Frame Debugger)
- **Unity Cloud Diagnostics** (crash reporting, ANR tracking)
- **Player feedback** (in-game report button, analytics for battle duration, rage quits)

## 15. Tooling Recommendations

### Custom Editor Tools

| Tool | Purpose | Priority |
|------|---------|----------|
| **Level Builder** | Scene setup, lane geometry, structure slot placement, environment dressing | High |
| **Wave Editor** | Timeline-like UI for spawn timing, unit composition, phase gating | High |
| **Unit Preview** | Real-time preview of unit stats, animations, card display | High |
| **Battle Sandbox** | Quick-play testing with debug controls (spawn units, skip waves, force phase) | High |
| **Stat Balancer** | Spreadsheet import/export for unit/wave tuning (CSV → ScriptableObject) | Medium |
| **Content Validator** | Scan for missing references, invalid data, cost imbalances, broken prefabs | High |
| **Mission Editor** | Objectives, rewards, dialogue trees, intro/outro cutscenes | Medium |
| **Performance Overlay** | Runtime FPS, memory, draw call counter, GC alloc tracking | Medium |

### Unity Built-In Tools

| Tool | Usage |
|------|-------|
| **Profiler** | CPU, GPU, memory, audio, physics, rendering analysis |
| **Memory Profiler** | Memory leak detection, snapshot comparison, texture tracking |
| **Frame Debugger** | Draw call analysis, render pass inspection, shader variant tracking |
| **Addressables Analyzer** | Asset bundle size, dependency analysis, build report |
| **Test Runner** | Play Mode & Edit Mode unit tests (NUnit framework) |
| **Version Control** | Git/PlasticSCM integration, YAML merge for Unity scenes |
| **Shader Graph** | Visual shader creation (toon shading, outlines, damage flash) |
| **Animation Window** | Preview animation clips, test state transitions |

### External Tooling

| Tool | Purpose |
|------|---------|
| **Rider/VSCode** | IDE with Unity debugging support (attached to Unity Editor) |
| **Blender** | 3D modeling, animation, low-poly asset creation |
| **Aseprite/Photoshop** | Texture painting, UI icon creation, card art |
| **Audacity/FL Studio** | Audio editing, music production, SFX creation |
| **Git** | Version control, collaboration, branching strategy |
| **Sheets (Google/Excel)** | Content balancing spreadsheets (unit stats, wave timing) |
| **Trello/Notion** | Project management, task tracking, design documentation |

## 16. Audio Stack

### Unity Audio System

| Component | Purpose |
|-----------|---------|
| **Audio Mixer** | Master, Music, SFX, UI, Environment channel routing |
| **Audio Source** | Per-unit/tower/structure sound playback (pooled) |
| **Audio Listener** | Camera-based spatial audio (battle camera) |
| **Audio Mixer Groups** | Ducking, effects, EQ per channel (compress music during combat) |
| **Audio Mixer Snapshots** | Phase transitions (Tea Time: calm mix, Tantrum Time: aggressive mix) |

### Audio Requirements

| Type | Specification | Format |
|------|---------------|--------|
| **SFX** | Short (< 2s), triggered on events (attack, hit, die, build) | ADPCM |
| **Music** | Looping, phase-based transitions (Tea Time → Tantrum Time) | OGG/Vorbis |
| **UI Sounds** | Instant playback, minimal latency (button click, card deploy) | WAV/ADPCM |
| **Voice Lines** | Faction commander taunts, battle announcements, victory/defeat | OGG/Vorbis |
| **Environment** | Ambient battle sounds (clashing steel, shouting, soup bubbling) | OGG/Vorbis |

### Implementation

- **AudioManager singleton** or **Service Locator** pattern (centralized control)
- **Object pooling** for frequently played sounds (avoid AudioSource creation during battle)
- **Mixer snapshots** for phase transitions (Tea Time: softer music, clearer SFX; Tantrum Time: louder music, more bass)
- **Ducking** for music during voice/dialogue (commander taunts)
- **Platform-specific volume** normalization (mobile speakers vs desktop headphones)
- **Spatial audio** for projectile sounds (pan left/right based on lane position)

### Phase-Based Audio Design

**Tea Time:**
- Music: Calm, orchestrated, polite
- SFX: Clear, crisp, precise (polished sounds)
- Announcer: Formal, measured tone

**Tantrum Time:**
- Music: Aggressive, fast-paced, loud
- SFX: Heavy, bassy, chaotic (smashing, shouting)
- Announcer: Excited, theatrical, exaggerated

## 17. Testing Recommendations

### Automated Testing

| Test Type | Scope | Tool |
|-----------|-------|------|
| **Unit Tests** | Battle math (damage calculations, resource generation, phase effects), unit traits | Unity Test Framework + NUnit (Edit Mode) |
| **Integration Tests** | Wave outcomes (does wave X defeat defense Y?), tower placement, victory conditions | Play Mode Tests |
| **Data Validation** | Missing references, invalid ScriptableObjects, broken prefab links | Custom editor tools, pre-build validation |
| **Performance Tests** | Frame time budgets (battle stays under 16.6ms for 60 FPS), memory limits | Unity Test Framework + Profiler API |
| **Save/Load Tests** | Version migration, corruption recovery, large save files | Edit Mode tests with mock data |

### Manual Testing

| Test Type | Focus |
|-----------|-------|
| **Device Compatibility** | Multiple Android devices (phones, tablets, low/mid/high-end) |
| **Aspect Ratio** | UI layout on 16:9, 18:9, 19.5:9, 4:3 (safe areas, touch targets) |
| **Input Testing** | Touch accuracy, hover tooltips, drag-to-deploy, gesture conflicts |
| **Performance** | Large wave battles (50+ units), sustained FPS monitoring, thermal throttling |
| **Memory** | Long play sessions (30+ minutes), leak detection, scene transitions |
| **Save/Load** | Version migration, corruption recovery, cloud sync (if enabled) |
| **Accessibility** | Colorblind modes, text size, touch targets, reduced motion |
| **Readability** | Unit silhouettes clear at distance, tower HP visible, phase indicator obvious |

### Battle Readability Testing

**Key Question:** Can the player understand what's happening on the lane during a large wave?

- Unit roles distinguishable by silhouette (frontliner vs ranged vs siege)
- Tower HP clearly visible at all times (top HUD, large text)
- Phase indicator obvious (color, icon, text)
- Damage numbers readable (not obscured by VFX)
- Deployment zones clear (where can I place units/structures?)
- Enemy telegraphs visible (warning before big wave, boss mechanic)

### Continuous Integration (Optional)

- **Unity Cloud Build** or **GitHub Actions** with Unity runner
- **Automated test suite** on each commit (unit tests, data validation)
- **Build artifact generation** for QA testing (APK, executable)
- **Linting & code style checks** (Roslyn analyzers, `.editorconfig`)
- **Automated APK size check** (fail if > 500 MB)

## 18. Networking

### MVP Scope

**Offline only.** No networking required for initial release.

All progression, saves, and unlocks are local.

### Future Expansion

| Feature | Approach | Timing |
|---------|----------|--------|
| **Cloud Save** | Unity Cloud Save service (cross-device sync) | Post-MVP |
| **Remote Config** | Unity Remote Config for balancing (tweak unit stats without update) | Post-MVP |
| **Leaderboards** | Async submission (Google Play Games / Game Center) | Post-MVP |
| **Async Multiplayer** | Turn-based replay sharing (challenge friend's loadout) | Phase 2 |
| **Live PvP** | Deterministic lockstep, rollback netcode (requires early planning) | Future |

### PvP Technical Note

If PvP is planned for future:

- **Design simulation to be deterministic** from day one (same inputs → same outputs)
- **Use fixed timestep** for all game logic (`Time.fixedDeltaTime`, constant rate)
- **Avoid floating-point non-determinism** (use integer math or fixed-point library for damage calculations)
- **Log input events** for replay & rollback (deployment timing, ability usage)
- **Plan DOTS/ECS architecture** early for performance (deterministic simulation at scale)
- **Test across platforms** (Android vs desktop determinism)

**Rule:** 
- If PvP is not in MVP, do not over-engineer for it
- Keep simulation layer pure C# and testable (easier to add determinism later)

## 19. Technical Risk Notes

### Identified Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| **Content scale outgrows tooling** | High | High | Build custom editors early (Wave Editor, Level Builder), validate data before commit |
| **Effect-heavy visuals hurt mobile performance** | High | Medium | Profile early (Profiler), use object pooling for VFX, limit active particles, use GPU instancing |
| **Simulation & rendering coupling** | High | Medium | Strict separation (pure C# simulation, MonoBehaviour presentation), event-driven communication |
| **Memory leaks from asset loading** | Medium | Medium | Use Addressables with explicit unloading, Memory Profiler validation, test scene transitions |
| **Fragmented Android devices** | Medium | High | Test on multiple devices (low/mid/high-end), graceful degradation (quality presets, Adaptive Performance) |
| **Long load times** | Medium | Medium | Async loading (Addressables), scene optimization (reduce build size), loading screen with tips |
| **Unity 6.5 beta stability** | Medium | Medium | Monitor beta releases, test target devices before updating, have LTS fallback (Unity 2022 LTS) |
| **Touch input conflicts on mobile** | Medium | High | Test gesture handling (drag vs swipe), safe area padding, input logging, avoid edge controls |
| **Battle readability with many units** | High | Medium | Unit pooling (max 50 on lane), LOD for distant units, VFX culling, clear silhouettes |

### Architecture Principles

1. **Simulation-Presentation Split:** Simulation drives logic, presentation follows (event-driven)
2. **Data-Driven Content:** No hardcoded gameplay values (everything in ScriptableObjects)
3. **Event-Driven Communication:** Decoupled systems via EventBus (avoid tight coupling)
4. **Object Pooling:** Reuse, don't instantiate/destroy frequently (units, projectiles, VFX, AudioSources)
5. **Async Loading:** No blocking operations on main thread (Addressables, scene loading)
6. **Graceful Degradation:** Support low-end devices with scaled-back visuals (quality presets)
7. **Testability:** Pure logic functions testable without Unity runtime (Edit Mode tests)
8. **Comedy Supports Gameplay:** Jokes enhance, never obscure, mechanical clarity first

### Debugging & Logging

- **Debug.Log** for gameplay events (deployment, damage, phase changes) in development builds
- **Custom debug overlay** for battle state (resource rates, unit counts, wave progress)
- **Battle replay logging** (optional: record inputs for bug reproduction)
- **Crash reporting** (Unity Cloud Diagnostics, stack traces on exception)
- **Performance counters** (FPS, memory, draw calls) in debug builds

## 20. Unity 6.5-Specific Features

### Leveraging Unity 6.5 Capabilities

| Feature | Application | Maturity |
|---------|-------------|----------|
| **Adaptive Performance** | Automatic quality scaling for thermal management (Android) | Stable |
| **Enhanced DOTS** | ECS-ready for large-scale battles (100+ units) | Preview |
| **UI Toolkit Runtime** | Modern data-driven UI (Battle HUD, menus) | Stable |
| **GPU Resident Drawer** | Reduced CPU overhead for 3D rendering (URP) | Preview |
| **SpeedTree 8** | Background environment detail (if needed) | Stable |
| **Visual Effect Graph** | GPU-accelerated particle systems (large-scale battle VFX) | Stable |
| **Shader Graph** | Custom stylized shaders (toon shading, outlines, damage flash) | Stable |
| **Cinemachine 3.0** | Advanced camera control (battle framing, shake, transitions) | Stable |
| **Nested Prefab Mode** | Better prefab editing workflow | Stable |
| **Terrain Tools 3D** | Level environment sculpting | Stable |

### Beta Considerations

- **Test thoroughly** on target devices before committing to beta features
- **Monitor Unity 6.5 release notes** for breaking changes (API deprecations, URP changes)
- **Have LTS fallback** (Unity 2022 LTS) if beta proves unstable on target devices
- **Report bugs early** to Unity forum/issue tracker (helps final release)
- **Avoid cutting-edge preview features** for core gameplay systems (DOTS, GPU Resident)
- **Lock package versions** in `Packages/manifest.json` (prevent unexpected updates)

### Recommended Package Versions (at time of writing)

```json
{
  "com.unity.render-pipelines.universal": "17.5.0",
  "com.unity.cinemachine": "3.1.0",
  "com.unity.inputsystem": "1.19.0",
  "com.unity.addressables": "1.22.0",
  "com.unity.textmeshpro": "3.2.0",
  "com.unity.entities": "1.3.0",
  "com.unity.visualeffectgraph": "17.5.0"
}
```

## 21. Development Milestones

### Phase 1: Prototype (Weeks 1-4)

**Goal:** Single-lane battle, basic units, no polish

- ✅ Lane setup (camera, environment, two towers)
- ✅ Deploy 2-3 units per faction
- ✅ Basic combat (auto-attack, HP, damage)
- ✅ Resource generation (Snacks)
- ✅ Tower destruction (win/lose condition)
- ✅ No UI polish, placeholder art

### Phase 2: Core Loop (Weeks 5-8)

**Goal:** Full battle loop with economy, structures, abilities

- ✅ Economy (Snacks, Drama meter)
- ✅ Structures (Snack Shack, Teacup Turret)
- ✅ Abilities (Motivational Yelling)
- ✅ Wave system (enemy spawning)
- ✅ Battle HUD (tower HP, resources, cards)
- ✅ Phase cycle (Tea Time vs Tantrum Time)
- ✅ Basic audio (SFX, music)

### Phase 3: Content & Progression (Weeks 9-12)

**Goal:** Multiple units, meta-progression, save system

- ✅ Full unit roster (5 per faction)
- ✅ Structure variety (economy, defense, utility)
- ✅ Item system (consumables, equipment slots)
- ✅ Meta-progression (Coins, unlocks)
- ✅ Save/load system
- ✅ World map, mission selection
- ✅ 3-5 missions (Level 01 + expansions)

### Phase 4: Polish & Optimization (Weeks 13-16)

**Goal:** Visual polish, performance, accessibility

- ✅ Final art (low-poly models, textures, animations)
- ✅ Post-processing (vignette, color grading, bloom)
- ✅ VFX (combat, projectiles, explosions)
- ✅ Audio polish (phase transitions, commander taunts)
- ✅ Performance optimization (60 FPS target)
- ✅ Accessibility (colorblind mode, text scaling)
- ✅ Testing (device compatibility, balance)

### Phase 5: Launch Preparation (Weeks 17-20)

**Goal:** Content complete, testing, store preparation

- ✅ All missions complete (6+ total)
- ✅ Boss battle (Monument of Excessive Glory or Grand Slop Citadel)
- ✅ Localization (if applicable)
- ✅ Analytics integration
- ✅ Store assets (screenshots, description, trailer)
- ✅ Beta testing (external testers, feedback incorporation)
- ✅ Bug fixing, final optimization

## 22. Final Technical Recommendation

**Use Unity 6.5 with URP 3D, C# ScriptableObjects, and data-driven architecture.**

Build a **single-lane prototype** first proving:

1. ✅ Lane-based unit deployment (drag card to lane)
2. ✅ Auto-combat (units fight on their own, strategy not action)
3. ✅ Tower placement & targeting (structures defend lane)
4. ✅ Enemy pathfinding & wave spawning (AI pushes lane)
5. ✅ Combat resolution & damage (pure C# simulation)
6. ✅ Resource management (Snacks generation, spending)
7. ✅ Phase cycle (Tea Time vs Tantrum Time, faction bonuses)
8. ✅ Victory/defeat conditions (tower HP, destroy enemy)
9. ✅ UI feedback (HUD: tower HP, resources, cards, phase)
10. ✅ Comedy tone (flavor text, faction identity, silly names)

**Then expand** to:

- Full unit roster (5 per faction + advanced troops)
- Multiple structure types (economy, defense, utility)
- Meta-progression & unlocks (Coins, upgrades, items)
- Multiple level types (basic assault, holdout, boss fortress)
- Visual effects & polish (VFX, audio, post-processing)
- Mission variety (objectives, rewards, dialogue)

**Prove the battle loop before expanding content scope.**

Keep the simulation readable, the comedy sharp, and the lane chaotic but clear.

---

**Document Version:** 2.0 (Unity 6.5 URP 3D)  
**Last Updated:** 2026-04-03  
**Engine:** Unity 6.5 (6000.5 Beta)  
**Render Pipeline:** URP 3D (Forward rendering)  
**Platform:** Mobile-first (Android 10+), Desktop secondary  
**Game Type:** Lane-based 3D tower defense (single lane, auto-combat)  
**Factions:** Sunnybottom Empire vs Mudsnout Horde  
**Tone:** Comedic parody fantasy (silly surface, solid strategy)
