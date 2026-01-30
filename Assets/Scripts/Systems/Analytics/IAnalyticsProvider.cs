namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Interface for analytics providers.
    /// Implement this interface to add custom analytics backends (Firebase, Unity Analytics, etc.)
    /// </summary>
    public interface IAnalyticsProvider
    {
        /// <summary>
        /// Unique identifier for this provider
        /// </summary>
        string ProviderId { get; }

        /// <summary>
        /// Whether the provider is currently enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Initialize the analytics provider
        /// </summary>
        void Initialize();

        /// <summary>
        /// Track a generic analytics event
        /// </summary>
        void TrackEvent(AnalyticsEvent analyticsEvent);

        /// <summary>
        /// Track a level completed event
        /// </summary>
        void TrackLevelCompleted(LevelCompletedEvent levelEvent);

        /// <summary>
        /// Track a level started event
        /// </summary>
        void TrackLevelStarted(LevelStartedEvent levelEvent);

        /// <summary>
        /// Track a level failed event
        /// </summary>
        void TrackLevelFailed(LevelFailedEvent levelEvent);

        /// <summary>
        /// Track a game session start
        /// </summary>
        void TrackSessionStart(SessionStartEvent sessionEvent);

        /// <summary>
        /// Track a game session end
        /// </summary>
        void TrackSessionEnd(SessionEndEvent sessionEvent);

        /// <summary>
        /// Set a user property
        /// </summary>
        void SetUserProperty(string propertyName, string value);

        /// <summary>
        /// Flush any buffered events
        /// </summary>
        void Flush();

        /// <summary>
        /// Shutdown the provider
        /// </summary>
        void Shutdown();
    }
}
