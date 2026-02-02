using UnityEngine;

namespace PlayFrame.Core.Logging
{
    /// <summary>
    /// A logger implementation that prepends a tag to all messages.
    /// Uses Unity's Debug.Log internally but can be easily swapped for testing.
    /// </summary>
    public class TaggedLogger : ILogger
    {
        private readonly string _tag;
        private bool _isEnabled;

        /// <summary>
        /// Whether logging is enabled. When false, Log() calls are ignored.
        /// Warning and Error calls are always logged regardless of this setting.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Create a new tagged logger
        /// </summary>
        /// <param name="tag">The tag to prepend to messages (e.g., "SaveManager")</param>
        /// <param name="isEnabled">Whether debug logging is initially enabled</param>
        public TaggedLogger(string tag, bool isEnabled = true)
        {
            _tag = tag;
            _isEnabled = isEnabled;
        }

        /// <summary>
        /// Log a debug message. Only logs if IsEnabled is true.
        /// </summary>
        public void Log(string message)
        {
            if (_isEnabled)
            {
                Debug.Log(FormatMessage(message));
            }
        }

        /// <summary>
        /// Log a warning message. Always logs regardless of IsEnabled.
        /// </summary>
        public void LogWarning(string message)
        {
            Debug.LogWarning(FormatMessage(message));
        }

        /// <summary>
        /// Log an error message. Always logs regardless of IsEnabled.
        /// </summary>
        public void LogError(string message)
        {
            Debug.LogError(FormatMessage(message));
        }

        private string FormatMessage(string message)
        {
            return $"[{_tag}] {message}";
        }
    }
}
