namespace PlayFrame.Core.Logging
{
    /// <summary>
    /// Factory for creating loggers with centralized settings.
    /// All loggers should be created through this factory to ensure
    /// consistent behavior and centralized control.
    /// </summary>
    public static class LoggerFactory
    {
        /// <summary>
        /// Create a lazy logger that defers settings check until first use.
        /// Safe to use in static field initializers.
        /// </summary>
        /// <param name="tag">Tag to prepend to log messages</param>
        /// <param name="module">Module category for filtering</param>
        /// <returns>ILogger instance with lazy initialization</returns>
        public static ILogger Create(string tag, LogModule module)
        {
            return new LazyLogger(tag, module);
        }

        /// <summary>
        /// Create a logger with custom enable flag (ignores global settings)
        /// Useful for providers that need their own control.
        /// NOT safe for static field initializers - use in Awake/Start.
        /// </summary>
        public static ILogger CreateCustom(string tag, bool isEnabled)
        {
            // Still respect global master switch
            var settings = LogSettings.Instance;
            bool actualEnabled = settings.EnableAllLogs && isEnabled;
            return new TaggedLogger(tag, actualEnabled);
        }

        /// <summary>
        /// Create a logger immediately (not lazy). 
        /// NOT safe for static field initializers - use in Awake/Start.
        /// </summary>
        public static ILogger CreateImmediate(string tag, LogModule module)
        {
            var settings = LogSettings.Instance;
            bool isEnabled = settings.IsModuleEnabled(module);
            return new TaggedLogger(tag, isEnabled);
        }

        /// <summary>
        /// Create an Analytics module logger
        /// </summary>
        public static ILogger CreateAnalytics(string tag) => Create(tag, LogModule.Analytics);

        /// <summary>
        /// Create a Save module logger
        /// </summary>
        public static ILogger CreateSave(string tag) => Create(tag, LogModule.Save);

        /// <summary>
        /// Create a Localization module logger
        /// </summary>
        public static ILogger CreateLocalization(string tag) => Create(tag, LogModule.Localization);

        /// <summary>
        /// Create a Scene module logger
        /// </summary>
        public static ILogger CreateScene(string tag) => Create(tag, LogModule.Scene);

        /// <summary>
        /// Create a UI module logger
        /// </summary>
        public static ILogger CreateUI(string tag) => Create(tag, LogModule.UI);

        /// <summary>
        /// Create an Event module logger
        /// </summary>
        public static ILogger CreateEvent(string tag) => Create(tag, LogModule.Event);

        /// <summary>
        /// Create a Game module logger
        /// </summary>
        public static ILogger CreateGame(string tag) => Create(tag, LogModule.Game);

        /// <summary>
        /// Create a Core module logger
        /// </summary>
        public static ILogger CreateCore(string tag) => Create(tag, LogModule.Core);

        /// <summary>
        /// Get a null logger that discards all messages
        /// </summary>
        public static ILogger GetNull() => NullLogger.Instance;
    }
}
