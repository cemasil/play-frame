# Unity Scene Setup Checklist

## 1. Create Scenes

### Create with File → New Scene (or Ctrl+N):
- [ ] MainMenu
- [ ] GameSelection  
- [ ] LoadingScreen
- [ ] Match3Game

### Add to Build Settings
- [ ] File → Build Settings
- [ ] Drag each scene (in order):
  1. MainMenu
  2. GameSelection
  3. LoadingScreen
  4. Match3Game

---

## 2. MainMenu Scene

### Canvas Hierarchy:
```
Canvas
├── Background (Image)
├── TitleText (Text - "Mini Game Framework")
├── ButtonPanel (Empty GameObject)
│   ├── PlayButton (Button + Text "Play")
│   ├── SettingsButton (Button + Text "Settings")
│   └── QuitButton (Button + Text "Quit")
```

### Script Binding:
- [ ] Add `MainMenuPanel.cs` to Canvas
- [ ] Drag button references in Inspector:
  - Play Button
  - Settings Button
  - Quit Button

---

## 3. GameSelection Scene

### Canvas Hierarchy:
```
Canvas
├── Background (Image)
├── TitleText (Text - "Select Game")
├── GamesPanel (Empty GameObject)
│   ├── Match3Panel (Vertical Layout Group)
│   │   ├── Match3Button (Button + Text "Match 3")
│   │   └── Match3HighScore (Text - "High Score: 0")
│   └── MemoryPanel (Vertical Layout Group)
│       ├── MemoryButton (Button + Text "Memory")
│       └── MemoryHighScore (Text - "High Score: 0")
├── BackButton (Button + Text "Back")
```

### Script Binding:
- [ ] Add `GameSelectionPanel.cs` to Canvas
- [ ] Connect references in Inspector:
  - match3Button
  - memoryButton
  - backButton
  - match3HighScoreText
  - memoryHighScoreText

---

## 4. LoadingScreen Scene

### Canvas Hierarchy:
```
Canvas
├── Background (Image - dark background)
├── LoadingPanel (Vertical Layout Group)
│   ├── LoadingText (Text - "Loading...")
│   ├── ProgressBar (Slider)
│   └── PercentageText (Text - "0%")
```

### Script Binding:
- [ ] Add `LoadingPanel.cs` to Canvas
- [ ] Connect references in Inspector:
  - progressBar (Slider)
  - loadingText
  - percentageText

### Slider Settings:
- [ ] Min Value: 0
- [ ] Max Value: 1
- [ ] Set Fill Rect (for progress view)

---

## 5. Match3Game Scene

### Canvas Hierarchy:
```
Canvas
├── Background (Image + ThemedUIElement: Panel)
├── TopPanel (Horizontal Layout Group)
│   ├── MovesText (ThemedText Header - "Moves: 15")
│   ├── Spacer (Empty GameObject)
│   ├── ScoreText (ThemedText Header - "Score: 0")
│   ├── Spacer (Empty GameObject)
│   └── TargetText (ThemedText Header - "Target: 500")
├── GridContainer (Empty GameObject - game area)
│   └── (Gems will be spawned here at runtime)
├── BackButton (ThemedButton - "← Back")
└── GameOverPanel (ThemedPanel - initially hidden)
    ├── ContentPanel (Vertical Layout Group)
    │   ├── ResultText (ThemedText Title - "You Win!")
    │   ├── FinalScoreText (ThemedText Body - "Final Score: 0")
    │   ├── HighScoreText (ThemedText Body - "Best: 0")
    │   ├── RestartButton (ThemedButton - "Restart")
    │   └── MenuButton (ThemedButton - "Main Menu")
```

### Creating UI Elements:

**Note:** If ThemedText and ThemedButton prefabs don't exist, create them first (see MODULAR_UI_SETUP.md).

#### TopPanel Texts:
- [ ] Instantiate Resources/UI/Prefabs/ThemedTextHeader.prefab 3 times
- [ ] Names: MovesText, ScoreText, TargetText
- [ ] Contents: "Moves: 15", "Score: 0", "Target: 500"

#### GridContainer Settings:
- [ ] Create Empty → "GridContainer"
- [ ] RectTransform → Width: 600, Height: 600
- [ ] Anchor: Center, Pivot: (0.5, 0.5)
- [ ] Position: (0, -50, 0)

#### BackButton:
- [ ] Instantiate Resources/UI/Prefabs/ThemedButton.prefab
- [ ] Position: Top-left corner (Anchor: Top-Left, Pos: 100, -50)
- [ ] Text: "← Back"

#### GameOverPanel:
- [ ] Instantiate Resources/UI/Prefabs/ThemedPanel.prefab
- [ ] Rename: "GameOverPanel"
- [ ] Initially DISABLED (uncheck Inspector checkbox)
- [ ] Add ContentPanel (Vertical Layout Group) inside
- [ ] Add themed elements inside ContentPanel

### Create Gem Prefab:
- [ ] UI → Image → "GemPrefab"
- [ ] RectTransform → Width: 90, Height: 90
- [ ] Add Component → Button
- [ ] Transition: None
- [ ] **NOTE:** DO NOT ADD ThemedUIElement (gems will get colors at runtime)
- [ ] Prefab: `Assets/Prefabs/Match3/GemPrefab.prefab`
- [ ] Delete from scene

### Gem Colors (to be set in Inspector):
- Red (#FF4444)
- Blue (#4444FF)
- Green (#44FF44)
- Yellow (#FFFF44)
- Purple (#FF44FF)

### Script Binding:
- [ ] Create Empty → "Match3GameManager"
- [ ] Add Component → `Match3Game.cs`
- [ ] Inspector references:
  - gemPrefab (GemPrefab.prefab)
  - gridContainer (Transform)
  - movesText (TextMeshProUGUI)
  - scoreText (TextMeshProUGUI)
  - targetText (TextMeshProUGUI)
  - backButton (Button)
  - gameOverPanel (GameObject)
  - resultText (TextMeshProUGUI)
  - finalScoreText (TextMeshProUGUI)
  - highScoreText (TextMeshProUGUI)
  - restartButton (Button)
  - menuButton (Button)
  - gemColors (Size: 5, Color array)

### TopPanel Layout:
- [ ] Horizontal Layout Group:
  - Child Alignment: Middle Center
  - Spacing: 20
  - Child Force Expand: Width ✓
- [ ] RectTransform: Height: 80, Anchor: Top-Stretch

---

## 6. Event System

In each scene:
- [ ] Check if EventSystem exists (if not, right-click → UI → Event System)

---

## 7. Test Et

### MainMenu:
- [ ] Play → transition to Game Selection
- [ ] Settings button (log in console)
- [ ] Quit button (editor stops)

### GameSelection:
- [ ] High scores are displayed
- [ ] Match3 → transition to Match3Game
- [ ] Memory button (scene doesn't exist yet but should work)
- [ ] Back → transition to MainMenu

### Match3Game (Dummy):
- [ ] SPACE → score increases
- [ ] High score updates
- [ ] ESC → Game Over panel opens
- [ ] Restart → Restarts
- [ ] Menu → Returns to MainMenu
- [ ] Back → Returns to GameSelection

---

## Tips:

### Canvas Settings (for each scene):
- UI Scale Mode: Scale with Screen Size
- Reference Resolution: 1920 x 1080
- Match: 0.5

### Text Settings:
- Font Size: Headers 48-60, buttons 32-40, others 24-32
- Alignment: Center (for most)
- You can use Best Fit

### Button Colors:
- Normal: White/Light gray
- Highlighted: Light yellow
- Pressed: Dark yellow
- Transition: Color Tint

### Use Layouts:
- Vertical/Horizontal Layout Group
- Content Size Fitter
- Set Anchor/Pivot correctly

---

## Next Step:
Once all scenes are ready, run the project and test the flow:
MainMenu → GameSelection → Match3Game → Game Over → Menu/Restart
