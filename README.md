# Mini Game Framework

A modular Unity framework for building mini-games with clean architecture and event-driven design.

## Features

- **Modular Architecture**: Clear separation between Core, Systems, MiniGames, and UI layers
- **Singleton Patterns**: Three types for different lifecycle needs (Singleton, MonoSingleton, PersistentSingleton)
- **Event System**: Loosely coupled communication via EventManager with support for parameterless and parameterized events
- **Scene Management**: Async scene loading with progress tracking and event notifications
- **Save System**: PlayerPrefs-based persistence with JSON serialization for game data and high scores
- **UI Framework**: Theme-based UI system with prefab factory and automatic styling
- **Game Registry**: Auto-discovery system for mini-games using ScriptableObject configs
- **Unit Tests**: Covering EventManager and SaveManager
- **SOLID Principles**: Clean code following Single Responsibility and Dependency Inversion

## Architecture

```
Assets/Scripts/
├── Core/                      # Framework foundation (no dependencies)
│   ├── Base/                 # Singleton patterns (Singleton<T>, MonoSingleton<T>, PersistentSingleton<T>)
│   ├── Interfaces/           # Common interfaces (IInitializable, IUpdatable, ICleanable)
│   ├── Extensions/           # Utility extensions
│   └── Utilities/            # Helper classes (Timer, CoroutineRunner, ObjectPooler)
│
├── Systems/                   # Framework systems (depends on Core)
│   ├── EventSystem/          # EventManager for loose coupling
│   ├── SceneManagement/      # SceneLoader with async support
│   ├── SaveSystem/           # SaveManager with JSON serialization
│   └── UISystem/             # UIPanel base class
│
├── MiniGames/                 # Game-specific logic
│   ├── GameConfig.cs         # ScriptableObject for game configuration
│   ├── GameRegistry.cs       # Auto-discovery of available games
│   └── Match3/               # Example game implementation
│
├── UI/                        # UI components and panels
│   ├── Panels/               # MainMenuPanel, GameSelectionPanel, LoadingPanel
│   ├── UITheme.cs            # ScriptableObject for theme configuration
│   ├── ThemedUIElement.cs    # Auto-apply themes to UI elements
│   └── UIPrefabFactory.cs    # Factory for creating themed UI prefabs
│
└── Tests/                     # Unit tests
    ├── EditMode/             # Editor tests
    └── PlayMode/             # Runtime tests
```

**Layer Dependencies:**  
UI → MiniGames → Systems → Core  
Each layer depends on Core. UI and MiniGames both use Systems (EventManager, SceneLoader, SaveManager).

## Testing

The framework includes unit tests covering EventManager and SaveManager.

**From Terminal:**
```bash
make test          # Run EditMode tests
make test-all      # Run all tests
make clean         # Clean artifacts
make logs          # View logs
```


**Test Coverage:**
- SaveManager: Save/load, score tracking, high scores, settings persistence
- EventManager: Subscribe/unsubscribe, event triggering, multiple listeners, edge cases

Results are saved in `TestResults/` as `EditMode-results.xml`

## Quick Start

**1. Clone the repository**
```bash
git clone https://github.com/cemasil/mini-game-framework.git
cd mini-game-framework
```

**2. Configure Unity path (if needed)**

The Makefile auto-detects Unity on macOS. For custom paths:
```bash
export UNITY_PATH=/path/to/Unity
```

**3. Open in Unity**
1. Open Unity Hub
2. Add project from disk
3. Select this folder

## Adding New Mini-Games

The framework uses a config-based system for adding games without modifying existing code.

**1. Create a GameConfig asset**
```
Assets/Resources/Games/ → Right-click → Create → MiniGames → Game Config
```

Configure in Inspector:
- **Game Name**: Internal identifier (e.g., "Puzzle")
- **Display Name**: User-facing name (e.g., "Puzzle Game")
- **Description**: Short description
- **Scene Name**: Unity scene name (e.g., "PuzzleGame")
- **Theme Color**: Primary color for UI

**2. Create the game scene**
- File → New Scene → Save as `PuzzleGame.unity`
- Add to Build Settings (File → Build Settings → Add Open Scenes)

**3. Implement game logic**
```csharp
using MiniGameFramework.MiniGames;

public class PuzzleGame : MonoBehaviour
{
    protected override void OnInitialize()
    {
        // Game initialization
    }
    
    private void OnGameWin(int score)
    {
        SaveManager.Instance.SaveGameScore("Puzzle", score);
        // Handle win logic
    }
}
```

**4. GameRegistry automatically discovers config and GameSelectionPanel displays it.**

## Creating Custom UI Panels

The framework provides a theme-based UI system with automatic styling.

**1. Create a UITheme asset** (if not exists)
```
Assets/Resources/UI/Themes/ → Create → UI → UI Theme
```

**2. Create a panel script**
```csharp
using MiniGameFramework.Systems.UI;
using TMPro;
using UnityEngine.UI;

public class SettingsPanel : UIPanel
{
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    
    protected override void OnInitialize()
    {
        backButton.onClick.AddListener(OnBackClicked);
        titleText.text = "Settings";
    }
    
    protected override void OnCleanup()
    {
        backButton.onClick.RemoveListener(OnBackClicked);
    }
    
    private void OnBackClicked()
    {
        Hide();
    }
}
```

**3. Create the panel in Unity**
- GameObject → UI → Panel
- Add your script component
- Add `ThemedUIElement` components to buttons/text for automatic theming
- Save as prefab in `Assets/Prefabs/UI/Panels/`

**4. Use in scenes**
```csharp
public SettingsPanel settingsPanel;

private void Start()
{
    settingsPanel.Show();
}
```

## Using the Event System

The EventManager enables loose coupling between systems.

**Subscribe to events:**
```csharp
protected override void OnInitialize()
{
    EventManager.Instance.Subscribe("GAME_START", OnGameStart);
    EventManager.Instance.Subscribe("SCORE_UPDATE", OnScoreUpdate);
}

private void OnGameStart() { /* Handle event */ }
private void OnScoreUpdate(object score) { /* Handle with parameter */ }
```

**Trigger events:**
```csharp
EventManager.Instance.TriggerEvent("GAME_START");
EventManager.Instance.TriggerEvent("SCORE_UPDATE", 100);
```

**Unsubscribe:**
```csharp
protected override void OnCleanup()
{
    EventManager.Instance.Unsubscribe("GAME_START", OnGameStart);
    EventManager.Instance.Unsubscribe("SCORE_UPDATE", OnScoreUpdate);
}
```

## Project Structure

```
Assets/
├── Resources/
│   ├── UI/
│   │   ├── Themes/          # UITheme ScriptableObjects
│   │   └── Prefabs/         # Themed UI prefabs
│   └── Games/               # GameConfig ScriptableObjects
├── Scripts/                 # All C# scripts
│   ├── Core/
│   ├── MiniGames/
│   ├── Systems/
│   ├── UI/
├── Scenes/                  # Unity scenes
```

## License

MIT