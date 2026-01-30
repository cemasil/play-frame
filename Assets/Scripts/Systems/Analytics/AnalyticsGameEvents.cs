using PlayFrame.Core.Events;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Analytics-specific game event definitions.
    /// Use these events to subscribe to analytics data flow.
    /// For core framework events, see PlayFrame.Core.Events.CoreEvents
    /// </summary>
    public static class AnalyticsGameEvents
    {
        #region Session Events

        /// <summary>Triggered when an analytics session starts</summary>
        public static readonly GameEvent<SessionStartEvent> SessionStart = new("OnAnalyticsSessionStart");

        /// <summary>Triggered when an analytics session ends</summary>
        public static readonly GameEvent<SessionEndEvent> SessionEnd = new("OnAnalyticsSessionEnd");

        #endregion

        #region Level Events

        /// <summary>Triggered when a level starts (for analytics)</summary>
        public static readonly GameEvent<LevelStartedEvent> LevelStarted = new("OnAnalyticsLevelStarted");

        /// <summary>Triggered when a level is completed (for analytics)</summary>
        public static readonly GameEvent<LevelCompletedEvent> LevelCompleted = new("OnAnalyticsLevelCompleted");

        /// <summary>Triggered when a level is failed (for analytics)</summary>
        public static readonly GameEvent<LevelFailedEvent> LevelFailed = new("OnAnalyticsLevelFailed");

        /// <summary>Triggered when a level is retried (for analytics)</summary>
        public static readonly GameEvent<LevelRetryEvent> LevelRetry = new("OnAnalyticsLevelRetry");

        #endregion

        #region Gameplay Events

        /// <summary>Triggered when a game move is made (for analytics)</summary>
        public static readonly GameEvent<GameMoveEvent> GameMove = new("OnAnalyticsGameMove");

        /// <summary>Triggered when a match is made (for analytics)</summary>
        public static readonly GameEvent<MatchMadeEvent> MatchMade = new("OnAnalyticsMatchMade");

        /// <summary>Triggered when game is paused (for analytics)</summary>
        public static readonly GameEvent<GamePausedEvent> GamePaused = new("OnAnalyticsGamePaused");

        /// <summary>Triggered when game is resumed (for analytics)</summary>
        public static readonly GameEvent<GameResumedEvent> GameResumed = new("OnAnalyticsGameResumed");

        #endregion

        #region Progress Events

        /// <summary>Triggered when a new high score is achieved (for analytics)</summary>
        public static readonly GameEvent<HighScoreEvent> HighScoreAchieved = new("OnAnalyticsHighScore");

        /// <summary>Triggered when a milestone is reached (for analytics)</summary>
        public static readonly GameEvent<MilestoneReachedEvent> MilestoneReached = new("OnAnalyticsMilestone");

        #endregion

        #region UI Events

        /// <summary>Triggered when a UI element is interacted with (for analytics)</summary>
        public static readonly GameEvent<UIInteractionEvent> UIInteraction = new("OnAnalyticsUIInteraction");

        /// <summary>Triggered when a screen is viewed (for analytics)</summary>
        public static readonly GameEvent<ScreenViewEvent> ScreenView = new("OnAnalyticsScreenView");

        #endregion

        #region Generic Events

        /// <summary>Triggered for any custom analytics event</summary>
        public static readonly GameEvent<AnalyticsEvent> CustomEvent = new("OnAnalyticsCustomEvent");

        #endregion
    }
}
