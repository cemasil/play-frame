namespace MiniGameFramework.Systems.Events
{
    /// <summary>
    /// Common event names used across the framework
    /// Centralized to avoid string typos
    /// </summary>
    public static class GameEvents
    {
        // Scene Events
        public const string SCENE_LOAD_STARTED = "OnSceneLoadStarted";
        public const string SCENE_LOAD_PROGRESS = "OnSceneLoadProgress";
        public const string SCENE_LOAD_COMPLETED = "OnSceneLoadCompleted";

        // Game State Events
        public const string GAME_STARTED = "OnGameStarted";
        public const string GAME_PAUSED = "OnGamePaused";
        public const string GAME_RESUMED = "OnGameResumed";
        public const string GAME_ENDED = "OnGameEnded";

        // Save System Events
        public const string GAME_SAVED = "OnGameSaved";
        public const string GAME_LOADED = "OnGameLoaded";

        // UI Events
        public const string UI_PANEL_OPENED = "OnUIPanelOpened";
        public const string UI_PANEL_CLOSED = "OnUIPanelClosed";

        // Score Events
        public const string SCORE_UPDATED = "OnScoreUpdated";
        public const string HIGH_SCORE_UPDATED = "OnHighScoreUpdated";
    }
}
