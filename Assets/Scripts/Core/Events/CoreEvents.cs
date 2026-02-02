namespace PlayFrame.Core.Events
{
    /// <summary>
    /// Core game event definitions.
    /// These are framework-level events used across all systems.
    /// For analytics-specific events, see PlayFrame.Systems.Analytics.AnalyticsGameEvents
    /// </summary>
    public static class CoreEvents
    {
        #region Scene Events

        /// <summary>Triggered when scene loading starts</summary>
        public static readonly GameEvent SceneLoadStarted = new("OnSceneLoadStarted");

        /// <summary>Triggered during scene loading with progress (0-1)</summary>
        public static readonly GameEvent<float> SceneLoadProgress = new("OnSceneLoadProgress");

        /// <summary>Triggered when scene loading completes</summary>
        public static readonly GameEvent SceneLoadCompleted = new("OnSceneLoadCompleted");

        #endregion

        #region Game State Events

        /// <summary>Triggered when game starts</summary>
        public static readonly GameEvent GameStarted = new("OnGameStarted");

        /// <summary>Triggered when game is paused</summary>
        public static readonly GameEvent GamePaused = new("OnGamePaused");

        /// <summary>Triggered when game resumes from pause</summary>
        public static readonly GameEvent GameResumed = new("OnGameResumed");

        /// <summary>Triggered when game ends</summary>
        public static readonly GameEvent GameEnded = new("OnGameEnded");

        #endregion

        #region Save System Events

        /// <summary>Triggered after game is saved</summary>
        public static readonly GameEvent GameSaved = new("OnGameSaved");

        /// <summary>Triggered after game is loaded</summary>
        public static readonly GameEvent GameLoaded = new("OnGameLoaded");

        #endregion

        #region UI Events

        /// <summary>Triggered when a UI panel opens (with panel name)</summary>
        public static readonly GameEvent<string> UIPanelOpened = new("OnUIPanelOpened");

        /// <summary>Triggered when a UI panel closes (with panel name)</summary>
        public static readonly GameEvent<string> UIPanelClosed = new("OnUIPanelClosed");

        #endregion

        #region Score Events

        /// <summary>Triggered when score is updated (with new score)</summary>
        public static readonly GameEvent<int> ScoreUpdated = new("OnScoreUpdated");

        /// <summary>Triggered when high score is updated (with new high score)</summary>
        public static readonly GameEvent<int> HighScoreUpdated = new("OnHighScoreUpdated");

        #endregion

        #region Audio Events

        /// <summary>Triggered when music volume changes (0-1)</summary>
        public static readonly GameEvent<float> MusicVolumeChanged = new("OnMusicVolumeChanged");

        /// <summary>Triggered when SFX volume changes (0-1)</summary>
        public static readonly GameEvent<float> SfxVolumeChanged = new("OnSfxVolumeChanged");

        /// <summary>Triggered when music mute state changes</summary>
        public static readonly GameEvent<bool> MusicMuteChanged = new("OnMusicMuteChanged");

        /// <summary>Triggered when SFX mute state changes</summary>
        public static readonly GameEvent<bool> SfxMuteChanged = new("OnSfxMuteChanged");

        #endregion

        #region Localization Events

        /// <summary>Triggered when language changes (with language code)</summary>
        public static readonly GameEvent<string> LanguageChanged = new("OnLanguageChanged");

        #endregion

        #region Input Events

        /// <summary>Triggered when input mode changes (touch/mouse/gamepad)</summary>
        public static readonly GameEvent<string> InputModeChanged = new("OnInputModeChanged");

        #endregion
    }
}
