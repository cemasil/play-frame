# Analytics/Telemetry System

This system provides a comprehensive analytics infrastructure designed to collect important metrics such as level completion, retry count, score, and other key data in puzzle games.

## Features

- **SOLID Principles**: Interface-based, extensible architecture
- **Multiple Provider Support**: Firebase, Unity Analytics, or custom backends
- **Performance Optimization**: Event batching and lazy initialization
- **Offline Support**: Offline data collection with local storage provider
- **Type-Safe Events**: Strongly-typed analytics events
- **Automatic Integration**: Automatic tracking through BaseGame

## Core Components

### 1. AnalyticsManager
Central analytics manager. Uses PersistentSingleton pattern.

```csharp
// Start analytics
AnalyticsManager.Instance.StartSession("MyGame", "1.0.0");

// When level is completed
AnalyticsManager.Instance.TrackLevelComplete(
    gameName: "Memory",
    levelNumber: 1,
    score: 1500,
    completionTimeSeconds: 45.5f,
    moveCount: 12,
    retryCount: 0,
    isNewHighScore: true
);

// When level fails
AnalyticsManager.Instance.TrackLevelFail(
    gameName: "Match3",
    levelNumber: 5,
    failReason: FailReasons.OUT_OF_MOVES,
    playTimeSeconds: 120f,
    score: 350,
    moveCount: 15
);
```

### 2. Analytics Events
Type-safe event classes:

- `SessionStartEvent` - Session start
- `SessionEndEvent` - Session end
- `LevelStartedEvent` - Level start
- `LevelCompletedEvent` - Level completion
- `LevelFailedEvent` - Level failure
- `LevelRetryEvent` - Level retry
- `MatchMadeEvent` - Match made (for Match-3)
- `GameMoveEvent` - Game move
- `HighScoreEvent` - New high score

### 3. Analytics Providers

#### ConsoleAnalyticsProvider
Logs to console for development/debug purposes.

#### LocalStorageAnalyticsProvider
Local JSON storage for offline analytics.

#### UnityAnalyticsProvider (Stub)
Ready-to-use template for Unity Analytics integration.

#### FirebaseAnalyticsProvider (Stub)
Ready-to-use template for Firebase Analytics integration.

## BaseGame Integration

BaseGame class includes automatic analytics hooks:

```csharp
public class MyGame : BaseGame
{
    // Override game name for analytics
    protected override string GameName => "MyPuzzleGame";
    
    // Override level number
    protected override int CurrentLevel => _currentLevel;
    
    // Override difficulty level
    protected override string Difficulty => "hard";
    
    private void OnGameWin()
    {
        // Call analytics when level is completed
        TrackLevelCompleted(
            score: _currentScore,
            moveCount: _movesMade,
            isNewHighScore: _isNewRecord,
            stars: CalculateStars(),
            matchCount: _totalMatches
        );
    }
    
    private void OnGameLose()
    {
        // When level fails
        TrackLevelFailed(
            failReason: FailReasons.OUT_OF_TIME,
            score: _currentScore,
            moveCount: _movesMade
        );
    }
    
    private void OnRestartClicked()
    {
        // Track retry on restart
        TrackLevelRetried();
        SceneLoader.Instance.LoadScene(SceneNames.MY_GAME);
    }
}
```

## Creating Custom Provider

```csharp
public class MyCustomProvider : BaseAnalyticsProvider
{
    public override string ProviderId => "my_custom_provider";
    
    public override void Initialize()
    {
        // Set up backend connection
    }
    
    public override void TrackLevelCompleted(LevelCompletedEvent levelEvent)
    {
        // Send to custom backend
        MyBackend.SendEvent(new {
            type = "level_complete",
            game = levelEvent.GameName,
            level = levelEvent.LevelNumber,
            score = levelEvent.Score,
            time = levelEvent.CompletionTimeSeconds
        });
    }
    
    public override void Flush()
    {
        // Send buffered events
        MyBackend.FlushQueue();
    }
}

// Register the provider
AnalyticsManager.Instance.RegisterProvider(new MyCustomProvider());
```

## Analytics Settings (ScriptableObject)

Create via `Assets > Create > PlayFrame > Analytics > Analytics Settings`:

- **Enable Analytics**: Global analytics toggle
- **Enable Debug Logs**: Console debug logging
- **Enable Batching**: Event batching for performance
- **Batch Flush Interval**: Batch send interval
- **Max Batch Size**: Maximum batch size
- **Privacy Settings**: Data collection settings

## Event Constants

```csharp
// Event names
AnalyticsEventNames.LEVEL_COMPLETED
AnalyticsEventNames.LEVEL_FAILED
AnalyticsEventNames.LEVEL_RETRY
AnalyticsEventNames.HIGH_SCORE

// Parameter names
AnalyticsParameterNames.GAME_NAME
AnalyticsParameterNames.SCORE
AnalyticsParameterNames.MOVE_COUNT

// Fail reasons
FailReasons.OUT_OF_MOVES
FailReasons.OUT_OF_TIME
FailReasons.TARGET_NOT_MET
```

## Best Practices

1. **Level Tracking**: Track at the start/end of each level
2. **Retry Tracking**: Increment retry count on restarts
3. **High Score Tracking**: Track new records separately
4. **Pause/Resume**: Track game pause/resume events
5. **Batching**: Enable batching in production
6. **Privacy**: Check user consent before collecting data

## File Structure

```
Assets/Scripts/Systems/Analytics/
├── IAnalyticsProvider.cs          # Provider interface
├── AnalyticsManager.cs            # Central manager
├── AnalyticsEvents.cs             # Event classes
├── AnalyticsConstants.cs          # Constants
├── AnalyticsSettings.cs           # ScriptableObject config
└── Providers/
    ├── BaseAnalyticsProvider.cs   # Base provider class
    ├── ConsoleAnalyticsProvider.cs
    ├── LocalStorageAnalyticsProvider.cs
    ├── UnityAnalyticsProvider.cs  # Stub
    └── FirebaseAnalyticsProvider.cs # Stub
```
