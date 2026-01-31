using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Base class for analytics providers that simplifies implementation.
    /// Extend this class instead of implementing IAnalyticsProvider directly.
    /// </summary>
    public abstract class BaseAnalyticsProvider : IAnalyticsProvider
    {
        public abstract string ProviderId { get; }

        public virtual bool IsEnabled { get; protected set; } = true;

        /// <summary>
        /// Logger instance for this provider. Set via SetLogger or in derived class constructor.
        /// </summary>
        protected ILogger Logger { get; private set; } = NullLogger.Instance;

        /// <summary>
        /// Set the logger for this provider
        /// </summary>
        public void SetLogger(ILogger logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Create a logger for this provider using the factory with global settings
        /// </summary>
        protected void InitializeLogger()
        {
            Logger = LoggerFactory.CreateAnalytics(ProviderId);
        }

        /// <summary>
        /// Create a logger with custom enable flag (still respects global master switch)
        /// </summary>
        protected void InitializeLogger(bool enableLogging)
        {
            Logger = LoggerFactory.CreateCustom(ProviderId, enableLogging);
        }

        public virtual void Initialize()
        {
            // Override in derived classes if needed
        }

        public virtual void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            // Default implementation - override in derived classes
        }

        public virtual void TrackLevelCompleted(LevelCompletedEvent levelEvent)
        {
            // Default: convert to generic event
            TrackEvent(levelEvent);
        }

        public virtual void TrackLevelStarted(LevelStartedEvent levelEvent)
        {
            // Default: convert to generic event
            TrackEvent(levelEvent);
        }

        public virtual void TrackLevelFailed(LevelFailedEvent levelEvent)
        {
            // Default: convert to generic event
            TrackEvent(levelEvent);
        }

        public virtual void TrackSessionStart(SessionStartEvent sessionEvent)
        {
            // Default: convert to generic event
            TrackEvent(sessionEvent);
        }

        public virtual void TrackSessionEnd(SessionEndEvent sessionEvent)
        {
            // Default: convert to generic event
            TrackEvent(sessionEvent);
        }

        public virtual void SetUserProperty(string propertyName, string value)
        {
            // Override in derived classes if needed
        }

        public virtual void Flush()
        {
            // Override in derived classes if needed
        }

        public virtual void Shutdown()
        {
            // Override in derived classes if needed
        }

        /// <summary>
        /// Enable or disable this provider
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
        }
    }
}
