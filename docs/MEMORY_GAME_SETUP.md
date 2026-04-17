# Memory Game - Unity UI Setup Guide

## Game Concept
Classic memory card matching game: Flip cards to find matching pairs. Match all pairs to win!

## Scene Setup

### 1. Create New Scene
1. Right-click in `Assets/Scenes` folder
2. `Create > Scene`
3. Name: `MemoryGame`
4. Open the scene (double-click)

### 2. Canvas Structure (Hierarchy)

```
Canvas (Screen Space - Overlay)
├── Background (Image)
├── TopPanel (Empty GameObject / Horizontal Layout)
│   ├── BackButton (ThemedButton prefab)
│   ├── MovesText (ThemedText prefab) - "Moves: 0"
│   ├── TimeText (ThemedText prefab) - "Time: 0:00"
│   └── ScoreText (ThemedText prefab) - "Score: 0"
├── GridContainer (Empty GameObject / Grid Layout Group)
│   └── (Cards will be spawned here at runtime)
└── GameOverPanel (Panel)
    ├── ResultText (ThemedText prefab) - "You Win!" / "Game Over!"
    ├── FinalTimeText (ThemedText prefab) - "Time: 1:23"
    ├── FinalMovesText (ThemedText prefab) - "Moves: 24"
    ├── HighScoreText (ThemedText prefab) - "Best Time: 0:45"
    ├── RestartButton (ThemedButton prefab) - "Play Again"
    └── MenuButton (ThemedButton prefab) - "Main Menu"
```

---

## Detailed Settings

### Canvas Settings
- Render Mode: `Screen Space - Overlay`
- Canvas Scaler: `Scale With Screen Size`
- Reference Resolution: `1920 x 1080`

### Background
- Component: `Image`
- Color: `#1A1A2E` (dark blue-gray)
- Anchor: Stretch-Stretch (full screen)

### TopPanel
- RectTransform:
  - Anchor: Top-Center
  - Pivot: (0.5, 1)
  - Width: 1800, Height: 120
  - PosY: -20
- Component: `Horizontal Layout Group`
  - Padding: Left=50, Right=50, Top=20, Bottom=20
  - Spacing: 30
  - Child Alignment: Middle Left
  - Child Force Expand: Width=true, Height=false

### BackButton
- ThemedButton prefab
- Text: "← Back"
- Size: 150 x 80

### MovesText
- ThemedText prefab
- Text: "Moves: 0"
- Font Size: 36
- Alignment: Middle Left
- Best Fit: Enable

### TimeText
- ThemedText prefab
- Text: "Time: 0:00"
- Font Size: 36
- Alignment: Middle Center

### ScoreText
- ThemedText prefab
- Text: "Score: 0"
- Font Size: 36
- Alignment: Middle Right

### GridContainer
- RectTransform:
  - Anchor: Middle-Center
  - Pivot: (0.5, 0.5)
  - Width: 1400, Height: 900
  - Pos: (0, -50, 0)
- Component: `Grid Layout Group`
  - Cell Size: (200, 240)
  - Spacing: (20, 20)
  - Start Corner: Upper Left
  - Start Axis: Horizontal
  - Child Alignment: Middle Center
  - Constraint: Fixed Column Count = 4

---

## Card Prefab Setup

### 3. Create Card Prefab
1. Right-click in Hierarchy: `Create Empty`
2. Name: `MemoryCard`
3. Add Component: `RectTransform`
4. RectTransform settings:
   - Width: 200, Height: 240

#### Card Structure:
```
MemoryCard (Empty GameObject)
├── CardFront (Image) - Card front face (image)
└── CardBack (Image) - Card back face (closed view)
```

### CardFront
- Component: `Image`
- Anchor: Stretch-Stretch (full card size)
- Margins: All=10
- Color: White (preserve image color)
- Raycast Target: ✓ Enable (clickable)

### CardBack
- Component: `Image`
- Anchor: Stretch-Stretch
- Margins: All=0
- Color: `#4ECCA3` (teal, matching framework theme)
- Sprite: Default Unity sprite (or custom pattern)
- Raycast Target: ✓ Enable

### Save Card Prefab
1. Drag `MemoryCard` object to `Assets/Prefabs` folder
2. Delete the instance in Hierarchy

---

## Game Over Panel

### GameOverPanel
- Component: `Image`
- Anchor: Stretch-Stretch (full screen)
- Color: `#00000099` (semi-transparent black overlay)
- Raycast Target: ✓ Enable (block background)
- **Initially disabled** (✗ uncheck in Inspector)

#### Panel Content:
```
GameOverPanel (Image - overlay)
└── ContentPanel (Image - center panel)
    ├── ResultText (ThemedText) - "You Win!"
    ├── FinalTimeText (ThemedText) - "Time: 1:23"
    ├── FinalMovesText (ThemedText) - "Moves: 24"
    ├── HighScoreText (ThemedText) - "Best Time: 0:45"
    ├── ButtonContainer (Empty GameObject - Horizontal Layout)
    │   ├── RestartButton (ThemedButton) - "Play Again"
    │   └── MenuButton (ThemedButton) - "Main Menu"
```

### ContentPanel
- RectTransform:
  - Anchor: Middle-Center
  - Width: 600, Height: 500
- Component: `Image`
- Color: `#16213E` (dark panel background)
- Component: `Vertical Layout Group`
  - Padding: All=40
  - Spacing: 25
  - Child Alignment: Upper Center
  - Child Force Expand: Width=true, Height=false

### ResultText
- ThemedText prefab
- Text: "You Win!"
- Font Size: 56
- Alignment: Center
- Color: `#F39C12` (golden yellow)

### FinalTimeText
- ThemedText prefab
- Text: "Time: 1:23"
- Font Size: 42
- Alignment: Center

### FinalMovesText
- ThemedText prefab
- Text: "Moves: 24"
- Font Size: 42
- Alignment: Center

### HighScoreText
- ThemedText prefab
- Text: "Best Time: 0:45"
- Font Size: 36
- Alignment: Center
- Color: `#4ECCA3` (highlight color)

### ButtonContainer
- Component: `Horizontal Layout Group`
  - Spacing: 20
  - Child Alignment: Middle Center
  - Child Force Expand: Width=false, Height=false

### RestartButton
- ThemedButton prefab
- Text: "Play Again"
- Size: 180 x 70

### MenuButton
- ThemedButton prefab
- Text: "Main Menu"
- Size: 180 x 70

---

## Card Images Setup

### 4. Prepare Card Images
The memory game needs 8 different images (8 pairs = 16 cards)

**Temporary solution (for testing):**
1. Create `Assets/Resources/MemoryCards` folder
2. Add 8 different color/shape sprites
3. Naming: `card_0.png`, `card_1.png`, ... `card_7.png`

**Alternative (color via code):**
- Define 8 different Colors in script, distinguish cards by colors

---

## Preparation for Script Connections

### MemoryGame Script References to Connect in Inspector:

**Grid Settings:**
- Grid Container: Drag `GridContainer` object

**Prefabs:**
- Card Prefab: Drag `MemoryCard` prefab

**UI References:**
- Moves Text: Drag `MovesText` object
- Time Text: Drag `TimeText` object
- Score Text: Drag `ScoreText` object
- Back Button: Drag `BackButton` object

**Game Over Panel:**
- Game Over Panel: Drag `GameOverPanel` object
- Result Text: Drag `ResultText` object
- Final Time Text: Drag `FinalTimeText` object
- Final Moves Text: Drag `FinalMovesText` object
- High Score Text: Drag `HighScoreText` object
- Restart Button: Drag `RestartButton` object
- Menu Button: Drag `MenuButton` object

---

## Build Settings

Add scene to Build Settings:
1. `File > Build Settings`
2. Add `MemoryGame` scene to the list

---

## Game Mechanics (Scripts to be written)

- 4x4 grid (16 cards, 8 pairs)
- Shuffle and place cards randomly
- Flip two cards, keep them open if they match
- Close them again if they don't match
- Game ends when all pairs are matched
- Record time and move count
- Save best time as high score

---

## Next Steps

After completing the UI setup, we'll write these scripts:
1. `MemoryCard.cs` - Individual card behavior (flip, reveal, hide)
2. `MemoryGame.cs` - Main game controller (extends BaseGame)

When the UI is ready, let me know to "write the scripts"! 🎮
