# Creating a New Game with PlayFrame

Step-by-step guide to creating a complete game scene from scratch.

---

## Table of Contents

1. [Scene Setup](#1-scene-setup)
2. [Canvas Hierarchy](#2-canvas-hierarchy)
3. [Layout Zones](#3-layout-zones)
4. [Grid Setup](#4-grid-setup)
5. [GridPiece Setup](#5-gridpiece-setup)
6. [Panel Integration](#6-panel-integration)
7. [Game Logic](#7-game-logic)
8. [Configuration Assets](#8-configuration-assets)
9. [Registration & Scenes](#9-registration--scenes)
10. [Complete Hierarchy Reference](#10-complete-hierarchy-reference)

---

## 1. Scene Setup

### Quick Setup (Recommended)

Use scene templates for instant setup:

1. **Assets → Create → PlayFrame → Scene → Bootstrap Scene** (once per project)
   - Creates all manager singletons, GameBootstrap controller, camera
   - Automatically added to Build Settings as index 0
2. **Assets → Create → PlayFrame → Scene → Game Scene**
   - Creates Canvas, SafeArea, GameLayoutManager, TopPanel/CenterPanel/BottomPanel
   - Enter your scene name (e.g., `MyPuzzleGame`)
3. **Assets → Create → PlayFrame → Scene → Loading Scene** (once per project)
   - Creates progress bar UI and LoadingSceneController
   - Used for scene transitions

### GridInputHandler

Add `GridInputHandler` component to your GridManager GameObject **in the Inspector**.
Do **not** rely on runtime `AddComponent` — the game will log an error if it's missing.

### Loading Scene Transitions

To load a game scene through a loading screen:

```csharp
// Uses default loading scene
SceneLoaderManager.Instance.LoadSceneWithLoading("MyPuzzleGame");

// Uses a specific loading scene
SceneLoaderManager.Instance.LoadSceneWithLoading("MyPuzzleGame", "CustomLoading");
```

Set `loadingSceneName` in your GameConfig asset to configure per-game loading screens.

---

## 2. Canvas Hierarchy

This is the most critical step. Follow this exact order:

### Step 1: Create the Canvas

1. **Right-click in Hierarchy → UI → Canvas**
2. A `Canvas` and `EventSystem` will be created automatically
3. Select the `Canvas`:
   - **Render Mode:** Screen Space - Overlay (default, leave as-is)
   - Add component: **`ResponsiveCanvasSetup`**
   - Default settings (1080×1920, auto-adjust match) work for most mobile games

> **Why nothing shows:** If you change Render Mode to "Screen Space - Camera" without assigning a camera, nothing renders. Always use **Screen Space - Overlay** unless you have a specific reason.

### Step 2: Create the Safe Area Container

1. **Right-click on Canvas → Create Empty**
2. Rename to **SafeArea**
3. Add component: **`SafeAreaHandler`**
4. Set `RectTransform`:
   - Anchor: Stretch-Stretch (hold Alt, click bottom-right preset)
   - Left/Right/Top/Bottom: all **0**
   - This makes it fill the entire Canvas, then SafeAreaHandler adjusts for notch/cutout at runtime

### Step 3: Create the Layout Manager

1. **Right-click on SafeArea → Create Empty**
2. Rename to **GameLayout**
3. Add component: **`GameLayoutManager`**
4. Set `RectTransform`:
   - Anchor: Stretch-Stretch
   - Left/Right/Top/Bottom: all **0**

### Step 4: Create Zone Containers

Under **GameLayout**, create these children:

| Child Name | Purpose | RectTransform |
|------------|---------|---------------|
| **TopPanel** | HUD: score, moves, target | Stretch-Stretch, all offsets 0 |
| **CenterPanel** | Grid / game area | Stretch-Stretch, all offsets 0 |
| **BottomPanel** | Boosters, controls | Stretch-Stretch, all offsets 0 |

> Don't set anchor values manually — `GameLayoutManager` will set them from the config.

5. On the **GameLayout** object:
   - Assign `Top Panel` → TopPanel
   - Assign `Center Panel` → CenterPanel
   - Assign `Bottom Panel` → BottomPanel
   - Assign `Layout Config` → your GameLayoutConfig asset (see [Section 8](#8-configuration-assets))

### Step 5: Panel Container

Panels live **directly under the Canvas**, not inside layout zones:

1. **Right-click on Canvas → Create Empty**
2. Rename to **Panels**
3. Set `RectTransform`: Stretch-Stretch, all offsets 0
4. Add component: **`PanelManager`**
5. All panel prefabs are children of this **Panels** object

### Step 6: Verify Hierarchy

Your final Canvas hierarchy should look exactly like this:

```
Canvas                          ← ResponsiveCanvasSetup
├── SafeArea                    ← SafeAreaHandler
│   └── GameLayout              ← GameLayoutManager
│       ├── TopPanel            ← HUD elements go here
│       ├── CenterPanel         ← GridManager goes here
│       └── BottomPanel         ← Buttons/boosters go here
├── Panels                      ← PanelManager
│   ├── [SettingsPanel]         ← Created via menu (see Section 6)
│   ├── [PausePanel]
│   └── ...
└── EventSystem                 ← (auto-created with Canvas)
```

### Common Canvas Mistakes

| Symptom | Cause | Fix |
|---------|-------|-----|
| Nothing visible | Render Mode = Camera with no camera | Set to **Screen Space - Overlay** |
| Nothing visible | EventSystem missing | Add GameObject with `EventSystem` + `StandaloneInputModule` |
| UI doesn't scale | No `ResponsiveCanvasSetup` | Add component to Canvas |
| Content behind notch | No `SafeAreaHandler` | Add to a child under Canvas |
| Panels don't appear | PanelManager not assigned | Assign panel refs in Inspector |
| Panels appear but no input | CanvasGroup blocksRaycasts=false | Panels handle this automatically via Show/Hide |
| Layout zones have 0 size | No GameLayoutConfig assigned | Create and assign via Create menu |

---

## 3. Layout Zones

### Create a GameLayoutConfig

1. **Right-click in Project → Create → PlayFrame → Layout → Game Layout Config**
2. Configure:
   - `Top Panel Size`: **0.12** (12% of screen for HUD)
   - `Bottom Panel Size`: **0.10** (10% for controls)
   - `Show Left/Right Panel`: **false** (unless needed)
   - `Respect Safe Area`: **true**
3. Assign to `GameLayoutManager → Layout Config`

### Adding HUD Elements to TopPanel

```
TopPanel
├── ScoreText (TMP_Text)
├── MovesText (TMP_Text)
└── TargetIcon (Image)
```

Use standard Unity UI layout groups (HorizontalLayoutGroup) for responsive positioning.

### Adding Controls to BottomPanel

```
BottomPanel
├── Booster1Button (Button)
├── Booster2Button (Button)
└── ShuffleButton (Button)
```

---

## 4. Grid Setup

### Create a GridConfig

1. **Right-click in Project → Create → PlayFrame → Grid → Grid Config**
2. Set: columns, rows, cellSize, cellSpacing, interactionMode, spawnMode
3. Optionally create:
   - **GridVisualConfig** (`Create → PlayFrame → Grid → Visual Config`)
   - **GridAudioConfig** (`Create → PlayFrame → Grid → Audio Config`)
4. Assign visual/audio configs in the GridConfig Inspector

### Configure the Grid

1. **Select your GridConfig** asset in the Project window
2. The Inspector shows the full Grid Designer with collapsible sections:
   - **Grid Dimensions:** Columns, rows, cell size, shape, custom cell mask
   - **Interaction:** Pick mode (Tap, Swap, MultiSelect, Chain, Draw, DragAndDrop)
   - **Spawn:** Animation timing and spawn direction
   - **Visuals:** Assign and edit visual config inline
   - **Audio:** Assign and edit audio config inline
   - **Board Layout Designer:** Paint piece types onto cells (optional)
   - **Preview:** Visual grid preview with sprites

### Add GridManager to the Scene

1. Select **CenterPanel** (or create a child of it)
2. Add components: **`GridManager`** and **`GridInputHandler`**
3. Assign:
   - `Config` → your GridConfig asset
   - `Piece Prefab` → your GridPiece prefab (see Section 5)
   - `Grid Container` → leave null (uses self) or assign a child RectTransform
4. Optionally add a background Image child and assign to `Grid Background Image`

---

## 5. GridPiece Setup

GridPiece is the visual representation of each cell's content. **You must create a prefab.**

### Create a Basic GridPiece Prefab

1. **Right-click in Hierarchy → UI → Image**
2. Rename to **GridPiece** (or your game-specific name like "CandyPiece")
3. Add component: **`GridPiece`** (or your custom subclass)
4. Set RectTransform size to something reasonable (e.g., 100×100) — GridManager overrides this at runtime
5. **Drag it into `Assets/Prefabs/`** to create the prefab
6. **Delete** the Hierarchy instance
7. Assign the prefab to `GridManager → Piece Prefab`

### How GridPiece Works at Runtime

1. `GridManager.InitializeGrid()` creates the cell grid in memory
2. `GridManager.FillGridAsync()` or `FillGridImmediate()` spawns pieces from an ObjectPool
3. For **each new piece**, GridManager fires `OnConfigurePiece(piece, col, row)`
4. **Your game must subscribe to `OnConfigurePiece`** to set type, sprite, color:

```csharp
gridManager.OnConfigurePiece += (piece, col, row) =>
{
    int typeIndex = Random.Range(0, pieceSprites.Length);
    piece.PieceId = typeIndex;
    piece.PieceType = typeIndex.ToString();
    piece.SetSprite(pieceSprites[typeIndex]);
    piece.SetColor(Color.white);
};
```

5. The piece will be sized to `config.cellSize` and positioned at the cell's local position

### Custom Piece Types

Extend `GridPiece` for game-specific behavior:

```csharp
public class MemoryCard : GridPiece
{
    private bool isFaceUp = false;
    
    public void FlipUp()
    {
        isFaceUp = true;
        SetSprite(faceSprite);
    }
    
    public void FlipDown()
    {
        isFaceUp = false;
        SetSprite(backSprite);
    }

    public override bool Matches(GridPiece other)
    {
        return other is MemoryCard card && card.PieceId == PieceId;
    }
}
```

### GridPiece Lifecycle

```
Pool.Get() → GridManager sets position/size → OnConfigurePiece callback
    → User interacts → OnSelected / OnDragStarted / OnMatched / etc.
    → RemovePiece → ResetPiece() → Pool.Release()
```

### GridPiece States

| State | When |
|-------|------|
| `Idle` | Default state, ready for interaction |
| `Selected` | Piece is tapped/selected |
| `Dragging` | Being dragged |
| `Chaining` | Part of an active chain |
| `Matched` | Matched, about to be destroyed |
| `Spawning` | Being spawned with animation |
| `Destroying` | Being destroyed with animation |
| `Disabled` | Cannot be interacted with (alpha 0.5) |

---

## 6. Panel Integration

### Create Panel Prefabs

Use the built-in prefab factory:

1. **Right-click in Hierarchy (or select your Panels container)**
2. **GameObject → PlayFrame → Panel → [Panel Type]**

Available panel types:
- Settings Panel
- Level Complete Panel
- Level Failed Panel
- IAP Panel
- Tutorial Panel
- Special Offer Panel
- Info Panel
- Pause Panel
- Confirmation Panel

Each panel is created with:
- Full UI layout (buttons, text, sliders, toggles)
- Correct components (UIPanel subclass, CanvasGroup, Image)
- Auto-wired serialized fields
- Ready to customize (change sprites, colors, text)

### Register Panels with PanelManager

1. Select the **PanelManager** object
2. In `Registered Panels`, add entries:
   - `Panel Id`: Use constants from `PanelIds` (e.g., `"settings"`)
   - `Panel`: Drag the panel GameObject

### Configure Panel Defaults

1. **Create → PlayFrame → UI → Panel Defaults**
2. Set global defaults:
   - Display Mode: Popup / Fullscreen / Custom
   - Popup size ratio (width/height)
   - Open/Close animation type
   - Animation duration and curve
   - Overlay color and behavior
   - Open/Close sounds
3. Assign to `PanelManager → Defaults`

### Per-Panel Overrides

Select any panel and check the override boxes in the Inspector:
- **Override Display Mode**: Fullscreen, Popup, or Custom
- **Override Animation**: Choose different open/close animation (Fade, Scale, Slide variants)
- **Override Overlay**: Custom overlay color, tap-to-close
- **Override Audio**: Custom open/close sounds

### Show Panels from Game Code

```csharp
// Show settings
PanelManager.Instance.ShowPanel(PanelIds.SETTINGS);

// Setup and show level complete
var lc = PanelManager.Instance.GetPanel<LevelCompletePanel>(PanelIds.LEVEL_COMPLETE);
lc.Setup(level: 5, score: 12500, starCount: 3, coinsEarned: 100, bonus: 50);
lc.OnNextLevel += () => LoadNextLevel();
lc.OnReplay += () => ReloadLevel();
PanelManager.Instance.ShowPanel(PanelIds.LEVEL_COMPLETE);

// Timed notification
PanelManager.Instance.ShowPanelTimed(PanelIds.INFO, 3f);
```

---

## 7. Game Logic

### Create Your Game Class

```csharp
// Assets/Scripts/MiniGames/YourGame/YourGame.cs
using System.Collections.Generic;
using UnityEngine;
using PlayFrame.Systems.Grid;

public class YourGame : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Sprite[] pieceSprites;

    private int score = 0;

    private void Start()
    {
        // Subscribe to events
        gridManager.OnConfigurePiece += ConfigurePiece;
        gridManager.OnPiecesSwapped += OnSwap;
        gridManager.OnPiecesMatched += OnMatch;

        // Initialize and fill
        gridManager.InitializeGrid();
        gridManager.FillGridImmediate();
    }

    private void ConfigurePiece(GridPiece piece, int col, int row)
    {
        int type = Random.Range(0, pieceSprites.Length);
        piece.PieceId = type;
        piece.PieceType = type.ToString();
        piece.SetSprite(pieceSprites[type]);
    }

    private async void OnSwap(GridPiece a, GridPiece b)
    {
        var matches = FindMatches(); // Your match-finding logic

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

    private void OnMatch(List<GridPiece> matched)
    {
        score += matched.Count * 100;
    }

    private List<GridPiece> FindMatches()
    {
        // Game-specific match logic
        return new List<GridPiece>();
    }
}
```

### Game Lifecycle

```
Bootstrap Scene → Managers init → Load Game Scene
    → GameLayoutManager applies zones
    → GridManager.InitializeGrid() creates cells
    → FillGridAsync() spawns pieces via OnConfigurePiece
    → Player interacts → Grid events fire → Game logic responds
    → Win/Lose → Show panel → Next level or menu
```

---

## 8. Configuration Assets

Create these ScriptableObjects in `Assets/GameSettings/`:

| Asset | Create Menu | Purpose |
|-------|-------------|---------|
| GameLayoutConfig | PlayFrame/Layout/Game Layout Config | Zone sizes and positioning |
| GridConfig | PlayFrame/Grid/Grid Config | Grid dimensions, interaction mode |
| GridVisualConfig | PlayFrame/Grid/Visual Config | Colors, selection overlays |
| GridAudioConfig | PlayFrame/Grid/Audio Config | Per-event sound effects |
| PanelDefaults | PlayFrame/UI/Panel Defaults | Global panel animation/sound defaults |
| GameConfig | PlayFrame/Game/Game Config | Game metadata (name, scene, theme) |

---

## 9. Registration & Scenes

### Register Your Game

```csharp
// In your MainMenu or GameSelection scene:
GameRegistry.Instance.RegisterGame(yourGameConfig);
```

### Build Settings Order

| Index | Scene |
|-------|-------|
| 0 | Bootstrap |
| 1 | MainMenu |
| 2 | YourGameScene |

---

## 10. Complete Hierarchy Reference

### Bootstrap Scene
```
Bootstrap                     ← GameBootstrap
EventSystem                   ← EventSystem + StandaloneInputModule
```

### Game Scene
```
Canvas                        ← Canvas + CanvasScaler + GraphicRaycaster + ResponsiveCanvasSetup
├── SafeArea                  ← RectTransform (stretch) + SafeAreaHandler
│   └── GameLayout            ← RectTransform (stretch) + GameLayoutManager
│       ├── TopPanel          ← HUD content
│       │   ├── ScoreText     ← TextMeshProUGUI
│       │   ├── MovesText     ← TextMeshProUGUI
│       │   └── TargetIcon    ← Image
│       ├── CenterPanel       ← Grid area
│       │   └── Grid          ← RectTransform + GridManager
│       │       └── [pieces are spawned here at runtime]
│       └── BottomPanel       ← Controls
│           ├── Booster1Btn   ← Button
│           └── Booster2Btn   ← Button
├── Panels                    ← PanelManager
│   ├── SettingsPanel         ← SettingsPanel (created via menu)
│   ├── PausePanel            ← PausePanel (created via menu)
│   ├── LevelCompletePanel    ← LevelCompletePanel
│   ├── LevelFailedPanel      ← LevelFailedPanel
│   └── InfoPanel             ← InfoPanel
├── YourGame                  ← Your game MonoBehaviour (can also be on Grid)
└── EventSystem               ← EventSystem + StandaloneInputModule
```

### What You Customize

| Item | What to change |
|------|---------------|
| Panel sprites | Replace placeholder backgrounds, icons, button images |
| Panel colors | Adjust colors on Image components |
| Piece sprites | Assign your art to pieceSprites array |
| Layout proportions | Tweak GameLayoutConfig percentages |
| Grid settings | Adjust GridConfig dimensions and interaction mode |
| Sounds | Assign AudioClips to GridAudioConfig and PanelDefaults |
| Fonts | Assign your TMP font asset to all text elements |
