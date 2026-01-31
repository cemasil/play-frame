namespace PlayFrame.Core.Logging
{
    /// <summary>
    /// Interface for logging functionality.
    /// Allows decoupling from Unity's Debug.Log for testability.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Whether logging is currently enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Log a debug message
        /// </summary>
        void Log(string message);

        /// <summary>
        /// Log a warning message
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Log an error message
        /// </summary>
        void LogError(string message);
    }
}
