namespace PlayFrame.Core.Logging
{
    /// <summary>
    /// A lazy-initialized logger that defers LogSettings access until first use.
    /// This is safe to use in static field initializers because it doesn't
    /// call Resources.Load until a log method is actually invoked.
    /// </summary>
    public class LazyLogger : ILogger
    {
        private readonly string _tag;
        private readonly LogModule _module;
        private ILogger _innerLogger;
        private bool _initialized;

        public LazyLogger(string tag, LogModule module)
        {
            _tag = tag;
            _module = module;
            _initialized = false;
        }

        /// <summary>
        /// Whether logging is enabled. Triggers lazy initialization on first access.
        /// </summary>
        public bool IsEnabled
        {
            get => GetLogger().IsEnabled;
            set => GetLogger().IsEnabled = value;
        }

        private ILogger GetLogger()
        {
            if (!_initialized)
            {
                var settings = LogSettings.Instance;
                bool isEnabled = settings.IsModuleEnabled(_module);
                _innerLogger = new TaggedLogger(_tag, isEnabled);
                _initialized = true;
            }
            return _innerLogger;
        }

        public void Log(string message)
        {
            GetLogger().Log(message);
        }

        public void LogWarning(string message)
        {
            GetLogger().LogWarning(message);
        }

        public void LogError(string message)
        {
            GetLogger().LogError(message);
        }
    }
}
