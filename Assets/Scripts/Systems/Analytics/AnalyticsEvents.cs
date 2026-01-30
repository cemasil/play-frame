using System;
using System.Collections.Generic;
using PlayFrame.Core.Events;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Base class for all analytics events.
    /// Implements IEvent for integration with the core event system.
    /// </summary>
    [Serializable]
    public class AnalyticsEvent : IEvent
    {
        /// <summary>
        /// Event name (used for tracking)
        /// </summary>
        public string EventName { get; protected set; }

        /// <summary>
        /// Event ID (same as EventName for analytics events)
        /// </summary>
        public string EventId => EventName;

        /// <summary>
        /// UTC timestamp when the event occurred
        /// </summary>
        public DateTime Timestamp { get; protected set; }

        /// <summary>
        /// Additional custom parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; protected set; }

        public AnalyticsEvent(string eventName)
        {
            EventName = eventName;
            Timestamp = DateTime.UtcNow;
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Add a custom parameter to the event
        /// </summary>
        public AnalyticsEvent WithParameter(string key, object value)
        {
            Parameters[key] = value;
            return this;
        }

        public override string ToString()
        {
            return $"[{EventName}] at {Timestamp:HH:mm:ss.fff}";
        }
    }

    #region Game Session Events

    /// <summary>
    /// Event fired when a game session starts
    /// </summary>
    [Serializable]
    public class SessionStartEvent : AnalyticsEvent
    {
        public string SessionId { get; private set; }
        public string GameName { get; private set; }
        public string GameVersion { get; private set; }
        public string Platform { get; private set; }
        public string DeviceModel { get; private set; }
        public string OperatingSystem { get; private set; }

        public SessionStartEvent(string gameName, string gameVersion = "1.0.0")
            : base(AnalyticsEventNames.SESSION_START)
        {
            SessionId = Guid.NewGuid().ToString();
            GameName = gameName;
            GameVersion = gameVersion;
            Platform = UnityEngine.Application.platform.ToString();
            DeviceModel = UnityEngine.SystemInfo.deviceModel;
            OperatingSystem = UnityEngine.SystemInfo.operatingSystem;
        }
    }

    /// <summary>
    /// Event fired when a game session ends
    /// </summary>
    [Serializable]
    public class SessionEndEvent : AnalyticsEvent
    {
        public string SessionId { get; private set; }
        public float SessionDurationSeconds { get; private set; }
        public int LevelsPlayed { get; private set; }
        public int TotalScore { get; private set; }

        public SessionEndEvent(string sessionId, float durationSeconds, int levelsPlayed, int totalScore)
            : base(AnalyticsEventNames.SESSION_END)
        {
            SessionId = sessionId;
            SessionDurationSeconds = durationSeconds;
            LevelsPlayed = levelsPlayed;
            TotalScore = totalScore;
        }
    }

    #endregion

    #region Level Events

    /// <summary>
    /// Event fired when a level/game is started
    /// </summary>
    [Serializable]
    public class LevelStartedEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public int LevelNumber { get; private set; }
        public string LevelId { get; private set; }
        public string Difficulty { get; private set; }
        public int AttemptNumber { get; private set; }

        public LevelStartedEvent(string gameName, int levelNumber, string levelId = "", string difficulty = "normal", int attemptNumber = 1)
            : base(AnalyticsEventNames.LEVEL_STARTED)
        {
            GameName = gameName;
            LevelNumber = levelNumber;
            LevelId = string.IsNullOrEmpty(levelId) ? $"level_{levelNumber}" : levelId;
            Difficulty = difficulty;
            AttemptNumber = attemptNumber;
        }
    }

    /// <summary>
    /// Event fired when a level/game is completed successfully
    /// </summary>
    [Serializable]
    public class LevelCompletedEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public int LevelNumber { get; private set; }
        public string LevelId { get; private set; }
        public int Score { get; private set; }
        public float CompletionTimeSeconds { get; private set; }
        public int MoveCount { get; private set; }
        public int MatchCount { get; private set; }
        public int RetryCount { get; private set; }
        public bool IsNewHighScore { get; private set; }
        public int Stars { get; private set; }
        public string Difficulty { get; private set; }

        public LevelCompletedEvent(
            string gameName,
            int levelNumber,
            int score,
            float completionTimeSeconds,
            int moveCount,
            int retryCount = 0,
            bool isNewHighScore = false,
            int stars = 0,
            string difficulty = "normal",
            string levelId = "",
            int matchCount = 0)
            : base(AnalyticsEventNames.LEVEL_COMPLETED)
        {
            GameName = gameName;
            LevelNumber = levelNumber;
            LevelId = string.IsNullOrEmpty(levelId) ? $"level_{levelNumber}" : levelId;
            Score = score;
            CompletionTimeSeconds = completionTimeSeconds;
            MoveCount = moveCount;
            MatchCount = matchCount;
            RetryCount = retryCount;
            IsNewHighScore = isNewHighScore;
            Stars = stars;
            Difficulty = difficulty;
        }
    }

    /// <summary>
    /// Event fired when a level/game is failed
    /// </summary>
    [Serializable]
    public class LevelFailedEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public int LevelNumber { get; private set; }
        public string LevelId { get; private set; }
        public string FailReason { get; private set; }
        public float PlayTimeSeconds { get; private set; }
        public int Score { get; private set; }
        public int MoveCount { get; private set; }
        public int RetryCount { get; private set; }
        public string Difficulty { get; private set; }

        public LevelFailedEvent(
            string gameName,
            int levelNumber,
            string failReason,
            float playTimeSeconds,
            int score,
            int moveCount,
            int retryCount = 0,
            string difficulty = "normal",
            string levelId = "")
            : base(AnalyticsEventNames.LEVEL_FAILED)
        {
            GameName = gameName;
            LevelNumber = levelNumber;
            LevelId = string.IsNullOrEmpty(levelId) ? $"level_{levelNumber}" : levelId;
            FailReason = failReason;
            PlayTimeSeconds = playTimeSeconds;
            Score = score;
            MoveCount = moveCount;
            RetryCount = retryCount;
            Difficulty = difficulty;
        }
    }

    /// <summary>
    /// Event fired when a level is restarted/retried
    /// </summary>
    [Serializable]
    public class LevelRetryEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public int LevelNumber { get; private set; }
        public string LevelId { get; private set; }
        public int RetryNumber { get; private set; }
        public float TimeSinceLastAttemptSeconds { get; private set; }

        public LevelRetryEvent(
            string gameName,
            int levelNumber,
            int retryNumber,
            float timeSinceLastAttempt = 0f,
            string levelId = "")
            : base(AnalyticsEventNames.LEVEL_RETRY)
        {
            GameName = gameName;
            LevelNumber = levelNumber;
            LevelId = string.IsNullOrEmpty(levelId) ? $"level_{levelNumber}" : levelId;
            RetryNumber = retryNumber;
            TimeSinceLastAttemptSeconds = timeSinceLastAttempt;
        }
    }

    #endregion

    #region Gameplay Events

    /// <summary>
    /// Event fired when a move/action is made in the game
    /// </summary>
    [Serializable]
    public class GameMoveEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public string MoveType { get; private set; }
        public int MoveNumber { get; private set; }
        public bool WasSuccessful { get; private set; }
        public int PointsEarned { get; private set; }

        public GameMoveEvent(
            string gameName,
            string moveType,
            int moveNumber,
            bool wasSuccessful,
            int pointsEarned = 0)
            : base(AnalyticsEventNames.GAME_MOVE)
        {
            GameName = gameName;
            MoveType = moveType;
            MoveNumber = moveNumber;
            WasSuccessful = wasSuccessful;
            PointsEarned = pointsEarned;
        }
    }

    /// <summary>
    /// Event fired when a match is made (for match-3 type games)
    /// </summary>
    [Serializable]
    public class MatchMadeEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public int MatchSize { get; private set; }
        public string MatchType { get; private set; }
        public int ComboCount { get; private set; }
        public int PointsEarned { get; private set; }

        public MatchMadeEvent(
            string gameName,
            int matchSize,
            string matchType = "normal",
            int comboCount = 1,
            int pointsEarned = 0)
            : base(AnalyticsEventNames.MATCH_MADE)
        {
            GameName = gameName;
            MatchSize = matchSize;
            MatchType = matchType;
            ComboCount = comboCount;
            PointsEarned = pointsEarned;
        }
    }

    /// <summary>
    /// Event fired when the game is paused
    /// </summary>
    [Serializable]
    public class GamePausedEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public float PlayTimeSeconds { get; private set; }
        public string PauseReason { get; private set; }

        public GamePausedEvent(string gameName, float playTimeSeconds, string pauseReason = "user_initiated")
            : base(AnalyticsEventNames.GAME_PAUSED)
        {
            GameName = gameName;
            PlayTimeSeconds = playTimeSeconds;
            PauseReason = pauseReason;
        }
    }

    /// <summary>
    /// Event fired when the game is resumed
    /// </summary>
    [Serializable]
    public class GameResumedEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public float PauseDurationSeconds { get; private set; }

        public GameResumedEvent(string gameName, float pauseDurationSeconds)
            : base(AnalyticsEventNames.GAME_RESUMED)
        {
            GameName = gameName;
            PauseDurationSeconds = pauseDurationSeconds;
        }
    }

    #endregion

    #region Progress Events

    /// <summary>
    /// Event fired when a milestone/achievement is reached
    /// </summary>
    [Serializable]
    public class MilestoneReachedEvent : AnalyticsEvent
    {
        public string MilestoneId { get; private set; }
        public string MilestoneName { get; private set; }
        public string GameName { get; private set; }
        public int ValueReached { get; private set; }

        public MilestoneReachedEvent(
            string milestoneId,
            string milestoneName,
            string gameName,
            int valueReached = 0)
            : base(AnalyticsEventNames.MILESTONE_REACHED)
        {
            MilestoneId = milestoneId;
            MilestoneName = milestoneName;
            GameName = gameName;
            ValueReached = valueReached;
        }
    }

    /// <summary>
    /// Event fired when a new high score is achieved
    /// </summary>
    [Serializable]
    public class HighScoreEvent : AnalyticsEvent
    {
        public string GameName { get; private set; }
        public int NewHighScore { get; private set; }
        public int PreviousHighScore { get; private set; }
        public int LevelNumber { get; private set; }

        public HighScoreEvent(string gameName, int newHighScore, int previousHighScore, int levelNumber = 0)
            : base(AnalyticsEventNames.HIGH_SCORE)
        {
            GameName = gameName;
            NewHighScore = newHighScore;
            PreviousHighScore = previousHighScore;
            LevelNumber = levelNumber;
        }
    }

    #endregion

    #region UI Events

    /// <summary>
    /// Event fired when a UI element is interacted with
    /// </summary>
    [Serializable]
    public class UIInteractionEvent : AnalyticsEvent
    {
        public string ElementId { get; private set; }
        public string ElementType { get; private set; }
        public string ScreenName { get; private set; }
        public string InteractionType { get; private set; }

        public UIInteractionEvent(
            string elementId,
            string elementType,
            string screenName,
            string interactionType = "click")
            : base(AnalyticsEventNames.UI_INTERACTION)
        {
            ElementId = elementId;
            ElementType = elementType;
            ScreenName = screenName;
            InteractionType = interactionType;
        }
    }

    /// <summary>
    /// Event fired when a screen/panel is viewed
    /// </summary>
    [Serializable]
    public class ScreenViewEvent : AnalyticsEvent
    {
        public string ScreenName { get; private set; }
        public string PreviousScreen { get; private set; }
        public float TimeOnPreviousScreenSeconds { get; private set; }

        public ScreenViewEvent(
            string screenName,
            string previousScreen = "",
            float timeOnPreviousScreen = 0f)
            : base(AnalyticsEventNames.SCREEN_VIEW)
        {
            ScreenName = screenName;
            PreviousScreen = previousScreen;
            TimeOnPreviousScreenSeconds = timeOnPreviousScreen;
        }
    }

    #endregion
}
