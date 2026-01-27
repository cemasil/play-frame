namespace MiniGameFramework.Systems.Events
{
    /// <summary>
    /// Type-safe event definitions for the framework.
    /// Use these with EventManager for compile-time type checking.
    /// </summary>
    public static class GameEvents
    {
        // Scene Events
        public static readonly GameEvent SceneLoadStarted = new("OnSceneLoadStarted");
        public static readonly GameEvent<float> SceneLoadProgress = new("OnSceneLoadProgress");
        public static readonly GameEvent SceneLoadCompleted = new("OnSceneLoadCompleted");

        // Game State Events
        public static readonly GameEvent GameStarted = new("OnGameStarted");
        public static readonly GameEvent GamePaused = new("OnGamePaused");
        public static readonly GameEvent GameResumed = new("OnGameResumed");
        public static readonly GameEvent GameEnded = new("OnGameEnded");

        // Save System Events
        public static readonly GameEvent GameSaved = new("OnGameSaved");
        public static readonly GameEvent GameLoaded = new("OnGameLoaded");

        // UI Events
        public static readonly GameEvent<string> UIPanelOpened = new("OnUIPanelOpened");
        public static readonly GameEvent<string> UIPanelClosed = new("OnUIPanelClosed");

        // Score Events
        public static readonly GameEvent<int> ScoreUpdated = new("OnScoreUpdated");
        public static readonly GameEvent<int> HighScoreUpdated = new("OnHighScoreUpdated");

        // Audio Events
        public static readonly GameEvent<float> MusicVolumeChanged = new("OnMusicVolumeChanged");
        public static readonly GameEvent<float> SfxVolumeChanged = new("OnSfxVolumeChanged");
        public static readonly GameEvent<bool> MusicMuteChanged = new("OnMusicMuteChanged");
        public static readonly GameEvent<bool> SfxMuteChanged = new("OnSfxMuteChanged");
    }
}
