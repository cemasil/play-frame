# PlayFrame Systems Documentation

Complete documentation for all framework systems. Each system is designed to be plug-and-play: configure via ScriptableObject, assign references in Inspector, and extend with game-specific logic.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Grid System](#grid-system)
3. [Panel Management](#panel-management)
4. [Common Panels](#common-panels)
5. [Layout System](#layout-system)
6. [Responsive Canvas](#responsive-canvas)
7. [Audio System](#audio-system)
8. [Save System](#save-system)
9. [Scene Loading](#scene-loading)
10. [Analytics](#analytics)
11. [Creating a New Game](#creating-a-new-game)

---

## Architecture Overview

### Layer Structure

```
PlayFrame.Core        → No dependencies. Events, Singletons, Pools, Logging, State Machine.
PlayFrame.Systems     → Depends on Core. Audio, Save, Scene, Grid, Layout, Canvas, Analytics, Localization.
PlayFrame.UI          → Depends on Core + Systems. PanelManager, all common panels, UIPanel base.
PlayFrame.MiniGames   → Depends on Core + Systems. Game-specific logic (Match3, Memory, etc.)
```

### Key Patterns

| Pattern | Class | Purpose |
|---------|-------|---------|
| PersistentSingleton | `PersistentSingleton<T>` | Managers that survive scene loads |
| MonoSingleton | `MonoSingleton<T>` | Scene-scoped singletons |
| EventManager | `EventManager` + `GameEvent<T>` | Type-safe decoupled events |
| ObjectPool | `ObjectPool<T>` | Reusable object pooling |
| StateMachine | `StateMachine` | Finite state machine |
| ScriptableObject Config | Various `*Config` | Inspector-configurable settings |

### Bootstrap Flow

`GameBootstrap` initializes all managers in order:
1. SaveManager
2. AudioManager
3. SceneLoaderManager
4. GameRegistry
5. (Custom managers added here)

---

## Grid System

**Location:** `Assets/Scripts/Systems/Grid/`

A comprehensive grid system supporting all common puzzle game interactions: tap, swap, drag-and-drop, multi-select, draw, and chain.

### Setup

1. **Create a GridConfig asset:** `Create → PlayFrame → Grid → Grid Config`
2. **Configure grid dimensions**, interaction mode, spawn mode, visual config, and audio config.
3. **Add `GridManager`** to your game scene.
4. **Assign the GridConfig** to the GridManager.
5. **Subscribe to events** in your game logic.

### Configuration (GridConfig)

```csharp
// ScriptableObject fields:
columns = 8;           // Grid width
rows = 8;              // Grid height
cellSize = 1f;         // Size of each cell
cellSpacing = 0.1f;    // Gap between cells
gridShape = GridShape.Rectangle;       // Rectangle, Diamond, Hexagonal, Custom
interactionMode = GridInteractionMode.Swap;  // Tap, Swap, DragAndDrop, MultiSelect, Draw, Chain
spawnMode = GridSpawnMode.FallFromTop;       // FallFromTop, RiseFromBottom, SlideFromLeft, SpawnInPlace, InitialOnly
```

### Interaction Modes (flags, can combine)

| Mode | Description |
|------|-------------|
| `Tap` | Tap a piece to select/activate it |
| `Swap` | Swipe to swap adjacent pieces (Match-3 style) |
| `DragAndDrop` | Drag a piece to a target cell |
| `MultiSelect` | Select multiple pieces then confirm (Memory game) |
| `Draw` | Draw a path through connected pieces |
| `Chain` | Build a chain through adjacent matching pieces |

### GridManager Events

```csharp
GridManager grid = GetComponent<GridManager>();

grid.OnPieceTapped += (piece) => { /* handle tap */ };
grid.OnPiecesSwapped += (a, b) => { /* check for matches after swap */ };
grid.OnPieceDragDropped += (piece, fromCell, toCell) => { /* handle drop */ };
grid.OnMultiSelectCompleted += (pieces) => { /* check selected group */ };
grid.OnChainCompleted += (pieces) => { /* process chain */ };
grid.OnPiecesMatched += (pieces) => { /* animate match */ };
grid.OnPiecesDestroyed += (count) => { /* update score */ };
grid.OnPiecesSpawned += (pieces) => { /* new pieces entered */ };

// Factory callback - configure each new piece
grid.OnConfigurePiece += (piece) => {
    // Assign random type, sprite, color, etc.
    int type = Random.Range(0, pieceSprites.Length);
    piece.PieceType = type;
    piece.SetSprite(pieceSprites[type]);
};
```

### GridManager API

```csharp
// Initialization
grid.InitializeGrid();                    // Build grid from config
await grid.FillGridAsync();              // Spawn pieces with animation
grid.FillGridImmediate();                // Spawn pieces instantly

// Piece Management
await grid.RemovePiecesAsync(pieces);    // Destroy pieces with animation
await grid.CollapseGridAsync();          // Gravity - fill gaps
await grid.SwapPiecesAsync(a, b);        // Animated swap
await grid.SwapBackAsync(a, b);          // Reverse swap (no match)

// Queries
GridCell cell = grid.GetCell(col, row);  // Get cell at position
GridPiece piece = grid.GetPiece(col, row); // Get piece at position
List<GridCell> neighbors = grid.GetNeighbors(cell); // Adjacent cells

// Grid Operations
grid.ShuffleGrid();                      // Randomize positions
grid.ClearGrid();                        // Remove all pieces
```

### Grid Editor Tool

Open via **Window → PlayFrame → Grid Editor**.

**Tabs:**
- **Grid:** Dimensions, shape, cell size, custom cell mask (click cells to enable/disable)
- **Interaction:** Select interaction modes, configure multi-select count, chain length, diagonals
- **Spawn:** Spawn mode, animation timing
- **Visuals:** Assign GridVisualConfig for colors, selection overlays, particles
- **Audio:** Assign GridAudioConfig for per-event sound effects
- **Preview:** Visual grid preview with cell rendering

### Custom Pieces

Extend `GridPiece` for game-specific behavior:

```csharp
public class CandyPiece : GridPiece
{
    public CandyType candyType;
    public bool isSpecial;

    public override bool Matches(GridPiece other)
    {
        return other is CandyPiece candy && candy.candyType == candyType;
    }
}
```

---

## Panel Management

**Location:** `Assets/Scripts/UI/PanelManager.cs`

Stack-based panel manager with transitions, queuing, and flow control.

### Setup

1. Add `PanelManager` to your persistent Canvas.
2. Register panels in the Inspector (`panelId` → `UIPanel` reference).
3. Or register at runtime: `PanelManager.Instance.RegisterPanel("MyPanel", panelInstance);`

### API

```csharp
// Show / Hide
PanelManager.Instance.ShowPanel(PanelIds.SETTINGS);
PanelManager.Instance.HideCurrentPanel();
PanelManager.Instance.HidePanel(PanelIds.SETTINGS);
PanelManager.Instance.HideAllPanels();

// Navigation
PanelManager.Instance.PopTo(PanelIds.MAIN_MENU);  // Pop stack to target

// Sequences
PanelManager.Instance.ShowSequence(PanelIds.LEVEL_COMPLETE, PanelIds.LEVEL_COMPLETE_REWARDS);

// Timed panels
PanelManager.Instance.ShowPanelTimed(PanelIds.INFO, 3f);  // Auto-hide after 3 seconds

// Typed access
var settings = PanelManager.Instance.GetPanel<SettingsPanel>(PanelIds.SETTINGS);

// State
UIPanel current = PanelManager.Instance.CurrentPanel;
bool busy = PanelManager.Instance.IsTransitioning;
```

### Panel IDs

All predefined constants in `PanelIds`:

```
MAIN_MENU, GAME_SELECTION, LOADING, SETTINGS
LEVEL_COMPLETE, LEVEL_COMPLETE_REWARDS, LEVEL_COMPLETE_BOOSTER
LEVEL_FAILED, LEVEL_FAILED_EXTRA_MOVES, LEVEL_FAILED_WATCH_AD, LEVEL_FAILED_GIVE_UP
IAP, IAP_SUCCESS, IAP_ERROR, IAP_RESTORE_SUCCESS, IAP_RESTORE_ERROR
TUTORIAL, INFO, SPECIAL_OFFER, PAUSE, CONFIRMATION
```

---

## Common Panels

All panels extend `UIPanel` and are in `Assets/Scripts/UI/Panels/`.

### SettingsPanel

Audio volume sliders, music/SFX mute toggles, language dropdown, haptic toggle, restore purchases button.

```csharp
var settings = PanelManager.Instance.GetPanel<SettingsPanel>(PanelIds.SETTINGS);
settings.SetLanguage("tr");
settings.OnRestorePurchasesRequested += () => { /* trigger IAP restore */ };
settings.OnDeleteSaveConfirmed += () => { SaveManager.Instance.DeleteSave(); };
```

### LevelCompletePanel

Star rating, score, rewards, optional booster earned. Three sub-states: Main, Rewards, Booster.

```csharp
var panel = PanelManager.Instance.GetPanel<LevelCompletePanel>(PanelIds.LEVEL_COMPLETE);
panel.Setup(level: 5, score: 12500, starCount: 3, coinsEarned: 100, bonus: 50);
panel.SetupBooster(boosterSprite, "Color Bomb", "Destroys all pieces of one color");
panel.OnNextLevel += () => { LoadNextLevel(); };
panel.OnReplay += () => { ReloadCurrentLevel(); };
panel.OnDoubleRewards += () => { ShowRewardedAd(); };
PanelManager.Instance.ShowPanel(PanelIds.LEVEL_COMPLETE);
```

### LevelFailedPanel

Retry, extra moves (coins/IAP), watch ad, give up. Four sub-states: Main, ExtraMoves, WatchAd, GiveUp.

```csharp
var panel = PanelManager.Instance.GetPanel<LevelFailedPanel>(PanelIds.LEVEL_FAILED);
panel.Setup(level: 5, message: "Out of moves!");
panel.SetupExtraMoves(movesCount: 5, coinsCost: 100);
panel.SetupAdReward("Watch ad for +5 moves");
panel.SetOptionsVisibility(showExtraMoves: true, showWatchAd: true, showGiveUp: true);
panel.OnRetry += () => { ReloadCurrentLevel(); };
panel.OnBuyExtraMovesCoins += () => { SpendCoins(100); AddMoves(5); };
panel.OnWatchAd += () => { ShowRewardedAd(); };
panel.OnGiveUp += () => { ReturnToMenu(); };
```

### IAPPanel

Store view with purchase/restore flows. Five sub-states: Store, PurchaseSuccess, PurchaseError, RestoreSuccess, RestoreError.

```csharp
var panel = PanelManager.Instance.GetPanel<IAPPanel>(PanelIds.IAP);
panel.OnPurchaseRequested += (productId) => { PurchaseProduct(productId); };
panel.OnRestoreRequested += () => { RestorePurchases(); };

// After purchase result:
panel.ShowPurchaseSuccess("Gem Pack", gemIcon);
panel.ShowPurchaseError("Payment failed. Please try again.");
panel.ShowRestoreSuccess(3);
panel.ShowRestoreError("No purchases found to restore.");
```

### TutorialPanel

Step-by-step tutorial with images, highlight overlays, and hand pointer.

```csharp
var steps = new List<TutorialStep>
{
    new TutorialStep { Title = "Welcome!", Description = "Tap pieces to select them." },
    new TutorialStep { Title = "Match 3", Description = "Swap to match 3 or more.", Image = swapSprite,
        HighlightTarget = gridRectTransform },
    new TutorialStep { Title = "Boosters", Description = "Use boosters for special effects.",
        HandPointerTarget = boosterButton.GetComponent<RectTransform>() }
};

var panel = PanelManager.Instance.GetPanel<TutorialPanel>(PanelIds.TUTORIAL);
panel.Setup(steps);
panel.OnTutorialComplete += () => { MarkTutorialDone(); };
panel.OnTutorialSkipped += () => { MarkTutorialDone(); };
PanelManager.Instance.ShowPanel(PanelIds.TUTORIAL);
```

### SpecialOfferPanel

Time-limited deals with countdown timer.

```csharp
var panel = PanelManager.Instance.GetPanel<SpecialOfferPanel>(PanelIds.SPECIAL_OFFER);
panel.Setup("Weekend Deal!", "50 Gems + 1000 Coins", "$2.99", "com.game.weekend_deal",
    image: dealSprite, originalPrice: "$4.99", discountPercent: 40);
panel.StartTimer(TimeSpan.FromHours(24));
panel.OnPurchaseRequested += (id) => { PurchaseProduct(id); };
panel.OnExpired += () => { /* offer no longer valid */ };
```

### InfoPanel

Generic information/notification with one or two buttons.

```csharp
// Simple notification
var panel = PanelManager.Instance.GetPanel<InfoPanel>(PanelIds.INFO);
panel.Setup("Update Available", "A new version is available. Please update.", icon: updateIcon);

// Two-button dialog
panel.Setup("Rate Us", "Enjoying the game?", heartIcon,
    "Rate Now", () => { OpenAppStore(); },
    "Later", () => { /* dismiss */ });
```

### PausePanel

Auto-pauses (`Time.timeScale = 0`) on show, resumes on hide. Quick audio toggles.

```csharp
var panel = PanelManager.Instance.GetPanel<PausePanel>(PanelIds.PAUSE);
panel.Setup(title: "Paused", level: "Level 5");
panel.OnRestart += () => { ReloadLevel(); };
panel.OnHome += () => { LoadMainMenu(); };
panel.OnSettings += () => { PanelManager.Instance.ShowPanel(PanelIds.SETTINGS); };
```

### ConfirmationPanel

Generic yes/no confirmation for destructive actions.

```csharp
var panel = PanelManager.Instance.GetPanel<ConfirmationPanel>(PanelIds.CONFIRMATION);
panel.Setup("Delete Save?", "This action cannot be undone.",
    onConfirm: () => { SaveManager.Instance.DeleteSave(); },
    onCancel: null,
    confirmLabel: "Delete", cancelLabel: "Cancel", icon: warningIcon);
PanelManager.Instance.ShowPanel(PanelIds.CONFIRMATION);
```

---

## Layout System

**Location:** `Assets/Scripts/Systems/Layout/`

Configurable game scene layout with zones for HUD, game area, controls, and side panels.

### Setup

1. **Create a GameLayoutConfig in `Assets → GameSettings → Layout`**:
    - `Create → PlayFrame → Layout → Game Layout Config`
2. **Add `GameLayoutManager`** to your game Canvas.
3. **Assign RectTransforms** for each zone (top, center, bottom, left, right).
4. **Assign the config** and it auto-applies on Awake.

### Configuration

```csharp
// GameLayoutConfig fields:
topPanelSize = 0.12f;     // 12% of screen for HUD
bottomPanelSize = 0.1f;   // 10% for controls
showLeftPanel = false;     // No side panels by default
showRightPanel = false;
respectSafeArea = true;    // Auto-adjust for notch/cutout
```

### Zone Rects

Each zone gets anchor-based positioning:
- **Top Panel:** Score, moves counter, target display
- **Center Panel:** Grid / game area (takes remaining space)
- **Bottom Panel:** Boosters, action buttons
- **Left/Right Panels:** Optional side panels (e.g., power-ups, info)

### Runtime Switching

```csharp
var layoutManager = FindObjectOfType<GameLayoutManager>();
layoutManager.SetConfig(newLayoutConfig);  // Switch layout at runtime
```

### Editor Preview

The `GameLayoutConfig` Inspector shows a visual preview of all zones with proportional sizing.

---

## Responsive Canvas

**Location:** `Assets/Scripts/Systems/Canvas/`

### ResponsiveCanvasSetup

Auto-configures `CanvasScaler` for responsive mobile UI.

```csharp
// Add to your Canvas GameObject
// Automatically sets:
// - Reference resolution: 1080x1920
// - UI Scale Mode: ScaleWithScreenSize
// - Match width/height auto-adjusts based on current aspect ratio
// - Wide devices (tablets): match more to height
// - Tall devices (phones): match more to width
```

### SafeAreaHandler

Handles notch/cutout for modern mobile devices.

```csharp
// Add to a child RectTransform under Canvas
// Automatically adjusts RectTransform anchors to respect Screen.safeArea
// Per-edge control: applyTop, applyBottom, applyLeft, applyRight
```

---

## Audio System

**Location:** `Assets/Scripts/Systems/Audio/AudioManager.cs`

PersistentSingleton with music crossfade, SFX one-shot, volume persistence via SaveManager.

```csharp
AudioManager.Instance.PlayMusic(bgmClip);
AudioManager.Instance.PlayMusicWithCrossFade(newBgm);
AudioManager.Instance.PlaySFX(clickSound);
AudioManager.Instance.SetMusicVolume(0.7f);
AudioManager.Instance.SetSfxVolume(0.8f);
AudioManager.Instance.SetMusicMuted(true);
AudioManager.Instance.MuteAll();
AudioManager.Instance.ResetToDefaults();
```

---

## Save System

**Location:** `Assets/Scripts/Systems/Save/`

PlayerPrefs-based save system with auto-save, backup slots, and validation.

```csharp
SaveManager.Instance.SaveGame();
SaveManager.Instance.LoadGame();
SaveManager.Instance.DeleteSave();
SaveManager.Instance.UpdateScore(100);

int highScore = SaveManager.Instance.GetGameHighScore("Match3");
SaveManager.Instance.CurrentSaveData.SetCustomData("key", "value");
```

### SaveSettings ScriptableObject

Configure: save key, auto-save interval, backup count, save on pause/quit, validation.

---

## Scene Loading

**Location:** `Assets/Scripts/Systems/Scene/SceneLoaderManager.cs`

Async scene loading with progress events.

```csharp
SceneLoaderManager.Instance.LoadScene("GameScene");
SceneLoaderManager.Instance.LoadScene(SceneNames.MAIN_MENU);
```

Scene name constants in `SceneNames`.

---

## Analytics

**Location:** `Assets/Scripts/Systems/Analytics/`

Event-based analytics system. See `docs/ANALYTICS_SYSTEM.md` for full documentation.

---

## Creating a New Game

### Step-by-step Guide

1. **Create game folder:** `Assets/Scripts/MiniGames/YourGame/`

2. **Create game class** extending `BaseGame`:
   ```csharp
   public class YourGame : BaseGame
   {
       [SerializeField] private GridManager gridManager;

       protected override void OnGameStart()
       {
           gridManager.OnConfigurePiece += ConfigurePiece;
           gridManager.InitializeGrid();
           gridManager.FillGridImmediate();
           StartGame(); // Explicit start
       }

       private void ConfigurePiece(GridPiece piece)
       {
           // Configure randomly
       }
   }
   ```

3. Create a GameConfig asset in `Assets → GameSettings → Games`:
   - `Create → PlayFrame → Game → Game Config`
   - Set gameName, displayName, sceneName, themeColor

4. **Register in GameRegistry:** Add your GameConfig to `GameRegistry.Instance`

5. **Create game scene** with:
   - Canvas with `ResponsiveCanvasSetup`
   - `GameLayoutManager` under Canvas with zone RectTransforms
   - `GridManager` in center zone
   - Your game MonoBehaviour

6. **Create GridConfig asset** with your game's grid settings

7. **Hook up panels** for level complete/failed flows

8. **Add scene to build settings**

### What You Provide (per game)

| Item | Type | Purpose |
|------|------|---------|
| Art assets | Sprites | Piece visuals, backgrounds, UI elements |
| Game logic | C# class | Match rules, scoring, win/lose conditions |
| GridConfig | ScriptableObject | Grid dimensions, interaction mode |
| GameConfig | ScriptableObject | Game metadata |
| GameLayoutConfig | ScriptableObject | Scene zone proportions |
| Audio clips | AudioClip | Music and SFX |
| Scene | Unity Scene | Game scene with prefabs |

### What the Framework Handles

- Grid creation, rendering, interaction handling
- Panel navigation (level complete, failed, pause, settings, IAP, tutorial)
- Audio playback and volume persistence
- Save/load with backup
- Scene transitions with loading screen
- Responsive canvas and safe area
- Analytics event tracking
- Object pooling for performance
- Event system for decoupled communication
