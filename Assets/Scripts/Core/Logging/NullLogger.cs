namespace PlayFrame.Core.Logging
{
    /// <summary>
    /// A no-op logger implementation that discards all messages.
    /// Useful for testing or when logging should be completely disabled.
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <summary>
        /// Singleton instance for convenience
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        public bool IsEnabled { get; set; }

        public void Log(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
    }
}
