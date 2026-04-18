# PlayFrame Systems Documentation

Reference documentation for all framework systems. Each system is designed to be plug-and-play: configure via ScriptableObject, assign references in Inspector, and extend with game-specific logic.

> **Creating a new game?** See [NEW_GAME_GUIDE.md](NEW_GAME_GUIDE.md) for the step-by-step walkthrough.

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
2. **Select the GridConfig** in the Project window — the Inspector shows the full Grid Designer.
3. **Add `GridManager`** to a GameObject with a RectTransform in your game scene.
4. **Add `GridInputHandler`** to the same GameObject (or let your game add it at runtime).
5. **Assign the GridConfig** and a **Piece Prefab** (a GameObject with `GridPiece` component + `Image`).
6. **Subscribe to events** in your game logic, or use the built-in Board Layout Designer.

### Configuration (GridConfig Inspector)

The GridConfig Inspector has collapsible sections:

| Section | Contents |
|---------|----------|
| **Grid Dimensions** | Columns, rows, shape, cell size, cell spacing, cell interaction |
| **Interaction** | Interaction mode flags, multi-select count, chain length, diagonals |
| **Spawn** | Spawn mode, animation duration, stagger delay, easing |
| **Visuals** | GridVisualConfig reference + inline editing |
| **Audio** | GridAudioConfig reference + context-sensitive editing |
| **Board Layout Designer** | Piece sprites, visual cell painter, deterministic placement |
| **Preview** | Visual grid preview with sprites |

### Interaction Modes (flags, can combine)

| Mode | Description |
|------|-------------|
| `Tap` | Tap a piece to select/activate it |
| `Swap` | Tap two adjacent pieces or swipe to swap (Match-3 style) |
| `DragAndDrop` | Drag a piece and drop onto another cell |
| `MultiSelect` | Tap multiple pieces, action triggers after N selections |
| `Draw` | Draw a path through connected pieces without lifting finger |
| `Chain` | Chain adjacent same-type pieces without lifting finger |

### GridInputHandler

`GridInputHandler` is the bridge between Unity's EventSystem and `GridManager`. It translates pointer events (tap, swipe, drag, chain/draw gestures) into GridManager API calls. It handles **all** interaction modes — not just swipe and drag.

| User Gesture | Interaction Mode | GridManager Method Called |
|-------------|------------------|-------------------------|
| Tap (short press) | Tap, Swap, MultiSelect | `HandlePieceTap()` |
| Swipe (drag > threshold) | Swap | `HandlePieceSwipe()` |
| Long drag | DragAndDrop | `HandleDragStart()` / `HandleDragUpdate()` / `HandleDragEnd()` |
| Touch + slide over cells | Chain, Draw | `HandleChainAdd()` (first piece on touch start) |
| Lift finger after chain | Chain, Draw | `HandleChainEnd()` |

**Setup:** Add `GridInputHandler` alongside `GridManager`. It auto-creates a transparent Image for raycast target and auto-finds the GridManager.

### GridManager Events

```csharp
GridManager grid = GetComponent<GridManager>();

grid.OnPieceTapped += (piece) => { /* handle tap */ };
grid.OnPiecesSwapped += (a, b) => { /* check for matches after swap */ };
grid.OnPieceDragDropped += (piece, fromCell, toCell) => { /* handle drop */ };
grid.OnMultiSelectCompleted += (pieces) => { /* check selected group */ };
grid.OnChainCompleted += (pieces) => { /* process chain */ };
grid.OnPiecesMatched += (pieces) => { /* animate match */ };
grid.OnPiecesDestroyed += (pieces) => { /* update score */ };
grid.OnPiecesSpawned += (pieces) => { /* new pieces entered */ };

// Factory callback - configure each new piece
// If you don't subscribe, GridManager auto-configures from GridConfig.pieceSprites + boardLayout
grid.OnConfigurePiece += (piece, col, row) => {
    int type = Random.Range(0, pieceSprites.Length);
    piece.PieceId = type;
    piece.PieceType = type.ToString();
    piece.SetSprite(pieceSprites[type]);
};
```

### GridManager API

```csharp
// Initialization
grid.InitializeGrid();                         // Build grid from config
await grid.FillGridAsync();                    // Spawn pieces with animation
grid.FillGridImmediate();                      // Spawn pieces instantly

// Deterministic Placement
grid.PlacePieceAt(col, row);                   // Place a single piece
await grid.FillGridWithLayoutAsync(layout);    // Fill from int[,] array

// Piece Management
await grid.RemovePiecesAsync(pieces);          // Destroy pieces with animation
await grid.CollapseGridAsync();                // Gravity — fill gaps
await grid.SwapPiecesAsync(a, b);              // Animated swap
await grid.SwapBackAsync(a, b);                // Reverse swap (no match)

// Queries
GridCell cell = grid.GetCell(col, row);
GridPiece piece = grid.GetPiece(col, row);
List<GridCell> neighbors = grid.GetNeighbors(col, row);
GridCell hitCell = grid.GetCellAtPosition(localPos);

// Cleanup
grid.ClearGrid();                              // Remove all pieces
```

### Board Layout Designer

Define which piece type goes in which cell at design time:

1. Add sprites to **Piece Sprites** array in your GridConfig.
2. Enable **Use Board Layout**.
3. Select a piece from the palette, then click/drag on cells to paint.
4. Use **X** button to switch to erase mode.

When `useBoardLayout` is true and no `OnConfigurePiece` subscriber exists, GridManager auto-configures pieces from the layout. Cells with value `-1` get random pieces.

### Visuals (GridVisualConfig)

GridManager **auto-creates** background, border, and cell background GameObjects at runtime if they are not manually assigned in the Inspector. Configure them in the GridVisualConfig asset:

- **Grid Background**: Sprite, color, padding
- **Grid Border**: Sprite, color, width, dynamic sizing
- **Cell Background**: Sprite, color (auto-created for each active cell)
- **Selection/Drag/Chain**: Visual feedback for interactions
- **Spawn**: Start scale and alpha for spawn animation

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

### Interaction Mode Configuration Guide

For each interaction mode, here's what you need to configure and which events to subscribe to.

#### Tap Mode

Select a piece by tapping it. Good for: tile removal, selection-based puzzles.

**GridConfig Settings:**
- Interaction Mode: `Tap`
- Default Cell Interaction: `Tappable` (or `All`)

```csharp
public class TapGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnPieceTapped += OnPieceTapped;
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row)
    {
        // Set up piece type, sprite, etc.
    }

    private async void OnPieceTapped(GridPiece piece)
    {
        // Remove tapped piece
        await gridManager.RemovePiecesAsync(new List<GridPiece> { piece });
        await gridManager.CollapseGridAsync();
        await gridManager.FillGridAsync();
    }
}
```

#### Swap Mode (Match-3)

Swipe or tap-tap to swap adjacent pieces. Good for: Match-3, sorting puzzles.

**GridConfig Settings:**
- Interaction Mode: `Swap` (optionally combine with `Tap`)
- Default Cell Interaction: `Swipeable, Tappable` (or `All`)

```csharp
public class Match3Game : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnPiecesSwapped += OnSwap;
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row) { /* ... */ }

    private async void OnSwap(GridPiece a, GridPiece b)
    {
        var matches = FindMatches();
        if (matches.Count > 0)
        {
            await gridManager.RemovePiecesAsync(matches);
            await gridManager.CollapseGridAsync();
            await gridManager.FillGridAsync();
        }
        else
        {
            await gridManager.SwapBackAsync(a, b);
        }
    }

    private List<GridPiece> FindMatches() { /* game-specific logic */ }
}
```

#### DragAndDrop Mode

Drag pieces freely between cells. Good for: sorting, arrangement puzzles.

**GridConfig Settings:**
- Interaction Mode: `DragAndDrop`
- Default Cell Interaction: `Draggable` (or `All`)
- Spawn Mode: `InitialOnly` (if pieces shouldn't respawn)

```csharp
public class DragGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnPieceDragDropped += OnDrop;
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row) { /* ... */ }

    private void OnDrop(GridPiece piece, GridCell from, GridCell to)
    {
        Debug.Log($"Dropped {piece.PieceType} from ({from.Col},{from.Row}) to ({to.Col},{to.Row})");
        // Validate placement, check win condition, etc.
    }
}
```

#### MultiSelect Mode

Tap multiple pieces to select them, action triggers when count is reached. Good for: memory games, group selection.

**GridConfig Settings:**
- Interaction Mode: `MultiSelect`
- Default Cell Interaction: `Selectable, Tappable` (or `All`)
- Multi Select Count: number of pieces to select before triggering

```csharp
public class MemoryGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnMultiSelectCompleted += OnGroupSelected;
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row) { /* ... */ }

    private async void OnGroupSelected(List<GridPiece> selected)
    {
        // Check if all selected pieces match
        bool allMatch = selected.TrueForAll(p => p.PieceId == selected[0].PieceId);
        if (allMatch)
        {
            await gridManager.RemovePiecesAsync(selected);
        }
        else
        {
            foreach (var p in selected) p.OnDeselected();
        }
    }
}
```

#### Chain Mode

Connect adjacent same-type pieces by dragging across them. Good for: connect-the-dots, chain puzzles.

**GridConfig Settings:**
- Interaction Mode: `Chain`
- Default Cell Interaction: `Chainable` (or `All`)
- Min Chain Length: minimum chain to trigger action (e.g., 3)
- Allow Diagonals: whether diagonal connections are valid

```csharp
public class ChainGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnChainCompleted += OnChainDone;
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row) { /* ... */ }

    private async void OnChainDone(List<GridPiece> chain)
    {
        Debug.Log($"Chain of {chain.Count} {chain[0].PieceType} pieces!");
        int score = chain.Count * chain.Count * 10; // Exponential scoring
        await gridManager.RemovePiecesAsync(chain);
        await gridManager.CollapseGridAsync();
        await gridManager.FillGridAsync();
    }
}
```

#### Draw Mode

Trace a path through any adjacent pieces (no type matching required). Good for: word games, path drawing.

**GridConfig Settings:**
- Interaction Mode: `Draw`
- Default Cell Interaction: `Chainable` (or `All`)
- Min Chain Length: minimum path length
- Allow Diagonals: true/false

```csharp
public class DrawGame : BaseGame
{
    [SerializeField] private GridManager gridManager;

    protected override async void OnGameStart()
    {
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnChainCompleted += OnPathDrawn;
        gridManager.InitializeGrid();
        await gridManager.FillGridAsync();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row) { /* ... */ }

    private void OnPathDrawn(List<GridPiece> path)
    {
        // Build word from path letters, validate pattern, etc.
        string word = string.Join("", path.ConvertAll(p => p.PieceType));
        Debug.Log($"Drew path: {word}");
    }
}
```

#### Combined Modes

Modes are flags — combine them for hybrid interactions:

```csharp
// Tap + Swap: player can tap-to-select then tap-to-swap, OR swipe to swap
interactionMode = GridInteractionMode.Tap | GridInteractionMode.Swap;

// Subscribe to both events
gridManager.OnPieceTapped += OnTap;
gridManager.OnPiecesSwapped += OnSwap;
```

### CellInteraction vs InteractionMode

| Setting | Purpose |
|---------|---------|
| `InteractionMode` (GridConfig) | Which interaction **types** are enabled for the whole grid |
| `DefaultCellInteraction` (GridConfig) | Which interactions each **cell** supports (per-cell filtering) |

`InteractionMode` controls what GridInputHandler sends to GridManager. `DefaultCellInteraction` controls what GridManager accepts per cell. Both must allow an interaction for it to work.

**Example:** `InteractionMode = Chain | Tap`, `DefaultCellInteraction = Chainable | Tappable`

---

## Panel Management

**Location:** `Assets/Scripts/UI/PanelManager.cs`

Stack-based panel manager with transitions, queuing, display modes, animations, overlay, and audio.

### Setup

1. Add `PanelManager` to your persistent Canvas (under a **Panels** container).
2. Create a **PanelDefaults** asset: `Create → PlayFrame → UI → Panel Defaults`
3. Assign it to `PanelManager → Defaults`
4. Register panels in the Inspector — drag a panel reference and the `panelId` auto-fills from the class name (e.g., `SettingsPanel` → `"Settings"`).
5. Or register at runtime: `PanelManager.Instance.RegisterPanel("MyPanel", panelInstance);`

### Creating Panel Prefabs

Use the built-in prefab factory:
- **GameObject → PlayFrame → Panel → [Panel Type]**
- Each panel is created with full UI layout, components, and auto-wired fields
- Only visual customization needed (sprites, colors, fonts)

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

### Panel Defaults (PanelDefaults ScriptableObject)

Global defaults that all panels inherit unless overridden:

| Setting | Default | Description |
|---------|---------|-------------|
| Display Mode | Popup | Fullscreen, Popup, or Custom |
| Popup Width/Height Ratio | 0.85 / 0.7 | Size relative to screen |
| Open Animation | FadeIn | Animation when panel opens |
| Close Animation | FadeOut | Animation when panel closes |
| Animation Duration | 0.3s | How long the animation takes |
| Show Overlay | true | Dark background behind popup |
| Overlay Color | rgba(0,0,0,0.6) | Semi-transparent black |
| Close On Overlay Tap | false | Tap overlay to dismiss |
| Open/Close Sound | null | AudioClip to play |

### Display Modes

| Mode | Behavior |
|------|----------|
| **Fullscreen** | Panel stretches to fill the entire screen. No overlay. |
| **Popup** | Panel is centered with configurable size ratio. Shows overlay. |
| **Custom** | Panel anchors are left as set in the prefab. |

### Animation Types

| Animation | Open | Close |
|-----------|------|-------|
| None | Instant | Instant |
| FadeIn / FadeOut | Alpha 0→1 | Alpha 1→0 |
| ScaleUp / ScaleDown | Scale 0→1 | Scale 1→0 |
| SlideFromTop | Slides in from top | Slides out to top |
| SlideFromBottom | Slides in from bottom | Slides out to bottom |
| SlideFromLeft | Slides in from left | Slides out to left |
| SlideFromRight | Slides in from right | Slides out to right |

### Per-Panel Overrides

Each panel (UIPanel subclass) can override any default in the Inspector:

```
[✓] Override Display Mode   → Fullscreen
[✓] Override Animation      → ScaleUp / ScaleDown, 0.25s
[✓] Override Overlay         → Custom color, tap-to-close
[✓] Override Audio           → Custom open/close sounds
```

Resolution order: **Panel override → PanelDefaults → Hardcoded fallback**

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

Auto-configures `CanvasScaler` for responsive mobile UI. Add to your Canvas GameObject.

```
Canvas (Screen Space - Overlay)          ← ResponsiveCanvasSetup
├── SafeArea                             ← SafeAreaHandler (stretch)
│   └── GameLayout                       ← GameLayoutManager (stretch)
│       ├── TopPanel
│       ├── CenterPanel
│       └── BottomPanel
└── Panels                               ← PanelManager
```

Settings:
- **Reference Resolution:** 1080×1920 (default, portrait mobile)
- **Match Width/Height:** 0.5 (balanced, auto-adjusts for device aspect ratio)
- **Auto Adjust Match:** true (automatically favors width on tall phones, height on wide tablets)

### SafeAreaHandler

Handles notch/cutout for modern mobile devices. Add to a full-screen child RectTransform under Canvas.

Settings:
- Per-edge control: `applyTop`, `applyBottom`, `applyLeft`, `applyRight`
- All true by default — adjust to keep content away from notch and home indicator

> **See [NEW_GAME_GUIDE.md](NEW_GAME_GUIDE.md) for complete Canvas hierarchy setup.**

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

## Scene System

**Location:** `Assets/Scripts/Systems/Scene/`

### Scene Templates

Create pre-configured scenes via: **Assets → Create → PlayFrame → Scene → [Type]**

| Template | Contents |
|----------|----------|
| **Bootstrap** | All manager singletons (EventManager, SaveManager, AudioManager, InputManager, SceneLoaderManager, GameRegistry, PanelManager, AnalyticsManager, LocalizationManager) + GameBootstrap controller |
| **MainMenu** | Canvas with ResponsiveCanvasSetup, SafeArea, title text, Play/Quit buttons |
| **Loading** | Canvas with progress bar, percentage text, LoadingSceneController |
| **Game** | Canvas with SafeArea, GameLayoutManager (TopPanel/CenterPanel/BottomPanel), background, score label |

Each template automatically:
- Creates the scene with all required GameObjects
- Configures Canvas (1080×1920, Screen Space Overlay)
- Sets up SafeAreaHandler for notch/cutout devices
- Adds the scene to Build Settings

### Scene Loading

**SceneLoaderManager** handles async scene loading with progress events:

```csharp
// Direct load (no loading screen)
SceneLoaderManager.Instance.LoadScene("GameScene");
SceneLoaderManager.Instance.LoadScene(SceneNames.MAIN_MENU);

// Load through a loading scene (shows progress UI)
SceneLoaderManager.Instance.LoadSceneWithLoading("GameScene");

// Load through a specific loading scene
SceneLoaderManager.Instance.LoadSceneWithLoading("GameScene", "CustomLoadingScreen");

// Reload current scene
SceneLoaderManager.Instance.ReloadCurrentScene();
```

### Loading Scene Transitions

**Flow:** Current Scene → Loading Scene → Target Scene

1. Call `LoadSceneWithLoading(targetScene, loadingScene)`
2. SceneLoaderManager stores the target and loads the loading scene synchronously
3. **LoadingSceneController** (on the loading scene) calls `LoadPendingScene()` on Start
4. SceneLoaderManager async-loads the target with progress events
5. **LoadingPanel** (if present) listens to `CoreEvents` and displays progress

Multiple loading scenes are supported. Specify per-transition:

```csharp
// From GameConfig
SceneLoaderManager.Instance.LoadSceneWithLoading(
    gameConfig.sceneName,
    gameConfig.loadingSceneName  // per-game loading scene
);
```

**GameConfig** has `loadingSceneName` field for per-game loading screen selection.

### Scene Constants

Scene name constants defined in `SceneNames`:

```csharp
SceneNames.BOOTSTRAP      // "Bootstrap"
SceneNames.MAIN_MENU      // "MainMenu"
SceneNames.GAME_SELECTION  // "GameSelection"
SceneNames.LOADING         // "LoadingScreen"
SceneNames.SORT            // "SortGame"
```

### LoadingSceneController

Place on a GameObject in any loading scene. Reads `SceneLoaderManager.PendingTargetScene` and triggers the async load automatically. Has a `fallbackScene` field for when opened directly without a pending target.

---

## Analytics

**Location:** `Assets/Scripts/Systems/Analytics/`

Event-based analytics system. See `docs/ANALYTICS_SYSTEM.md` for full documentation.
