using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFrame.Core;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Central analytics manager that coordinates multiple analytics providers.
    /// Implements the facade pattern for simplified analytics API.
    /// Thread-safe and performant with batching support.
    /// Configuration is loaded from AnalyticsSettings ScriptableObject.
    /// </summary>
    public class AnalyticsManager : PersistentSingleton<AnalyticsManager>
    {
        [Header("Settings")]
        [Tooltip("Optional: Assign settings directly. If null, loads from Resources/AnalyticsSettings")]
        [SerializeField] private AnalyticsSettings settings;

        private readonly List<IAnalyticsProvider> _providers = new List<IAnalyticsProvider>();
        private readonly Queue<AnalyticsEvent> _eventQueue = new Queue<AnalyticsEvent>();
        private readonly object _lockObject = new object();

        private string _currentSessionId;
        private float _sessionStartTime;
        private int _levelsPlayedThisSession;
        private int _totalScoreThisSession;
        private float _lastFlushTime;
        private bool _isInitialized;

        // Logging
        private ILogger _logger;

        // Cached settings for runtime access
        private bool _enableAnalytics;
        private bool _enableBatching;
        private float _batchFlushInterval;
        private int _maxBatchSize;

        /// <summary>
        /// Current analytics settings
        /// </summary>
        public AnalyticsSettings Settings => settings;

        /// <summary>
        /// Whether analytics collection is enabled
        /// </summary>
        public bool IsEnabled => _enableAnalytics;

        /// <summary>
        /// Current session ID
        /// </summary>
        public string CurrentSessionId => _currentSessionId;

        /// <summary>
        /// Number of registered providers
        /// </summary>
        public int ProviderCount => _providers.Count;

        protected override void OnSingletonAwake()
        {
            LoadSettings();
            InitializeProviders();
            _isInitialized = true;
        }

        private void LoadSettings()
        {
            // Use assigned reference or create default
            if (settings == null)
            {
                settings = AnalyticsSettings.CreateDefault();
            }

            // Initialize logger using factory with global settings integration
            _logger = LoggerFactory.CreateAnalytics("Analytics");

            // Cache settings for runtime performance
            _enableAnalytics = settings.EnableAnalytics;
            _enableBatching = settings.EnableBatching;
            _batchFlushInterval = settings.BatchFlushInterval;
            _maxBatchSize = settings.MaxBatchSize;

            _logger.Log("Analytics settings loaded");
        }

        private void Update()
        {
            if (!_enableAnalytics || !_enableBatching) return;

            if (Time.time - _lastFlushTime >= _batchFlushInterval)
            {
                FlushEventQueue();
                _lastFlushTime = Time.time;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App going to background - flush events
                FlushEventQueue();
            }
        }

#if UNITY_EDITOR
        protected override void OnApplicationQuit()
        {
            EndSession();
            FlushEventQueue();
            ShutdownProviders();
            base.OnApplicationQuit();
        }
#else
        private void OnApplicationQuit()
        {
            EndSession();
            FlushEventQueue();
            ShutdownProviders();
        }
#endif

        #region Provider Management

        /// <summary>
        /// Initialize providers based on settings
        /// </summary>
        private void InitializeProviders()
        {
            // Console provider (Editor/Debug only)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (settings.EnableConsoleProvider)
            {
                RegisterProvider(new ConsoleAnalyticsProvider(settings.EnableDebugLogs));
            }
#endif

            // Local storage provider
            if (settings.EnableLocalStorageProvider)
            {
                RegisterProvider(new LocalStorageAnalyticsProvider());
            }

            // Unity Analytics provider
            if (settings.EnableUnityAnalytics)
            {
                RegisterProvider(new UnityAnalyticsProvider());
            }

            // Firebase Analytics provider
            if (settings.EnableFirebaseAnalytics)
            {
                RegisterProvider(new FirebaseAnalyticsProvider());
            }

            _logger.Log($"Analytics initialized with {_providers.Count} provider(s)");
        }

        /// <summary>
        /// Register a new analytics provider
        /// </summary>
        public void RegisterProvider(IAnalyticsProvider provider)
        {
            if (provider == null)
            {
                _logger.LogWarning("Attempted to register null analytics provider");
                return;
            }

            lock (_lockObject)
            {
                if (_providers.Exists(p => p.ProviderId == provider.ProviderId))
                {
                    _logger.LogWarning($"Analytics provider '{provider.ProviderId}' is already registered");
                    return;
                }

                provider.Initialize();
                _providers.Add(provider);
                _logger.Log($"Registered analytics provider: {provider.ProviderId}");
            }
        }

        /// <summary>
        /// Unregister an analytics provider
        /// </summary>
        public void UnregisterProvider(string providerId)
        {
            lock (_lockObject)
            {
                var provider = _providers.Find(p => p.ProviderId == providerId);
                if (provider != null)
                {
                    provider.Shutdown();
                    _providers.Remove(provider);
                    _logger.Log($"Unregistered analytics provider: {providerId}");
                }
            }
        }

        /// <summary>
        /// Get a registered provider by ID
        /// </summary>
        public IAnalyticsProvider GetProvider(string providerId)
        {
            lock (_lockObject)
            {
                return _providers.Find(p => p.ProviderId == providerId);
            }
        }

        private void ShutdownProviders()
        {
            lock (_lockObject)
            {
                foreach (var provider in _providers)
                {
                    try
                    {
                        provider.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error shutting down provider {provider.ProviderId}: {ex.Message}");
                    }
                }
                _providers.Clear();
            }
        }

        #endregion

        #region Session Management

        /// <summary>
        /// Start a new analytics session
        /// </summary>
        public void StartSession(string gameName, string gameVersion = "1.0.0")
        {
            if (!_enableAnalytics) return;

            _sessionStartTime = Time.time;
            _levelsPlayedThisSession = 0;
            _totalScoreThisSession = 0;

            var sessionEvent = new SessionStartEvent(gameName, gameVersion);
            _currentSessionId = sessionEvent.SessionId;

            DispatchToProviders(p => p.TrackSessionStart(sessionEvent));
            _logger.Log($"Session started: {_currentSessionId}");
        }

        /// <summary>
        /// End the current analytics session
        /// </summary>
        public void EndSession()
        {
            if (!_enableAnalytics || string.IsNullOrEmpty(_currentSessionId)) return;

            float sessionDuration = Time.time - _sessionStartTime;
            var sessionEvent = new SessionEndEvent(
                _currentSessionId,
                sessionDuration,
                _levelsPlayedThisSession,
                _totalScoreThisSession
            );

            DispatchToProviders(p => p.TrackSessionEnd(sessionEvent));
            _logger.Log($"Session ended: {_currentSessionId}, Duration: {sessionDuration:F1}s");

            _currentSessionId = null;
        }

        #endregion

        #region Level Tracking

        /// <summary>
        /// Track when a level/game starts
        /// </summary>
        public void TrackLevelStart(string gameName, int levelNumber, string difficulty = "normal", int attemptNumber = 1)
        {
            if (!_enableAnalytics) return;

            var levelEvent = new LevelStartedEvent(gameName, levelNumber, "", difficulty, attemptNumber);
            DispatchToProviders(p => p.TrackLevelStarted(levelEvent));
            _logger.Log($"Level started: {gameName} - Level {levelNumber}");
        }

        /// <summary>
        /// Track when a level/game is completed successfully
        /// </summary>
        public void TrackLevelComplete(
            string gameName,
            int levelNumber,
            int score,
            float completionTimeSeconds,
            int moveCount,
            int retryCount = 0,
            bool isNewHighScore = false,
            int stars = 0,
            string difficulty = "normal",
            int matchCount = 0)
        {
            if (!_enableAnalytics) return;

            _levelsPlayedThisSession++;
            _totalScoreThisSession += score;

            var levelEvent = new LevelCompletedEvent(
                gameName,
                levelNumber,
                score,
                completionTimeSeconds,
                moveCount,
                retryCount,
                isNewHighScore,
                stars,
                difficulty,
                "",
                matchCount
            );

            DispatchToProviders(p => p.TrackLevelCompleted(levelEvent));
            _logger.Log($"Level completed: {gameName} - Level {levelNumber}, Score: {score}, Time: {completionTimeSeconds:F1}s");
        }

        /// <summary>
        /// Track when a level/game is failed
        /// </summary>
        public void TrackLevelFail(
            string gameName,
            int levelNumber,
            string failReason,
            float playTimeSeconds,
            int score,
            int moveCount,
            int retryCount = 0,
            string difficulty = "normal")
        {
            if (!_enableAnalytics) return;

            _levelsPlayedThisSession++;

            var levelEvent = new LevelFailedEvent(
                gameName,
                levelNumber,
                failReason,
                playTimeSeconds,
                score,
                moveCount,
                retryCount,
                difficulty
            );

            DispatchToProviders(p => p.TrackLevelFailed(levelEvent));
            _logger.Log($"Level failed: {gameName} - Level {levelNumber}, Reason: {failReason}");
        }

        /// <summary>
        /// Track when a level is retried
        /// </summary>
        public void TrackLevelRetry(string gameName, int levelNumber, int retryNumber, float timeSinceLastAttempt = 0f)
        {
            if (!_enableAnalytics) return;

            var retryEvent = new LevelRetryEvent(gameName, levelNumber, retryNumber, timeSinceLastAttempt);
            TrackEvent(retryEvent);
            _logger.Log($"Level retry: {gameName} - Level {levelNumber}, Retry #{retryNumber}");
        }

        #endregion

        #region Gameplay Tracking

        /// <summary>
        /// Track a game move/action
        /// </summary>
        public void TrackGameMove(string gameName, string moveType, int moveNumber, bool wasSuccessful, int pointsEarned = 0)
        {
            if (!_enableAnalytics) return;

            var moveEvent = new GameMoveEvent(gameName, moveType, moveNumber, wasSuccessful, pointsEarned);
            TrackEvent(moveEvent);
        }

        /// <summary>
        /// Track when a match is made
        /// </summary>
        public void TrackMatchMade(string gameName, int matchSize, string matchType = "normal", int comboCount = 1, int pointsEarned = 0)
        {
            if (!_enableAnalytics) return;

            var matchEvent = new MatchMadeEvent(gameName, matchSize, matchType, comboCount, pointsEarned);
            TrackEvent(matchEvent);
        }

        /// <summary>
        /// Track game pause
        /// </summary>
        public void TrackGamePaused(string gameName, float playTimeSeconds, string pauseReason = "user_initiated")
        {
            if (!_enableAnalytics) return;

            var pauseEvent = new GamePausedEvent(gameName, playTimeSeconds, pauseReason);
            TrackEvent(pauseEvent);
            _logger.Log($"Game paused: {gameName}");
        }

        /// <summary>
        /// Track game resume
        /// </summary>
        public void TrackGameResumed(string gameName, float pauseDurationSeconds)
        {
            if (!_enableAnalytics) return;

            var resumeEvent = new GameResumedEvent(gameName, pauseDurationSeconds);
            TrackEvent(resumeEvent);
            _logger.Log($"Game resumed: {gameName}");
        }

        #endregion

        #region Progress Tracking

        /// <summary>
        /// Track when a new high score is achieved
        /// </summary>
        public void TrackHighScore(string gameName, int newHighScore, int previousHighScore, int levelNumber = 0)
        {
            if (!_enableAnalytics) return;

            var highScoreEvent = new HighScoreEvent(gameName, newHighScore, previousHighScore, levelNumber);
            TrackEvent(highScoreEvent);
            _logger.Log($"New high score: {gameName} - {newHighScore} (was {previousHighScore})");
        }

        /// <summary>
        /// Track when a milestone/achievement is reached
        /// </summary>
        public void TrackMilestone(string milestoneId, string milestoneName, string gameName, int valueReached = 0)
        {
            if (!_enableAnalytics) return;

            var milestoneEvent = new MilestoneReachedEvent(milestoneId, milestoneName, gameName, valueReached);
            TrackEvent(milestoneEvent);
            _logger.Log($"Milestone reached: {milestoneName} in {gameName}");
        }

        #endregion

        #region UI Tracking

        /// <summary>
        /// Track UI element interaction
        /// </summary>
        public void TrackUIInteraction(string elementId, string elementType, string screenName, string interactionType = "click")
        {
            if (!_enableAnalytics) return;

            var uiEvent = new UIInteractionEvent(elementId, elementType, screenName, interactionType);
            TrackEvent(uiEvent);
        }

        /// <summary>
        /// Track screen/panel view
        /// </summary>
        public void TrackScreenView(string screenName, string previousScreen = "", float timeOnPreviousScreen = 0f)
        {
            if (!_enableAnalytics) return;

            var screenEvent = new ScreenViewEvent(screenName, previousScreen, timeOnPreviousScreen);
            TrackEvent(screenEvent);
            _logger.Log($"Screen view: {screenName}");
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Set a user property across all providers
        /// </summary>
        public void SetUserProperty(string propertyName, string value)
        {
            if (!_enableAnalytics) return;

            DispatchToProviders(p => p.SetUserProperty(propertyName, value));
            _logger.Log($"User property set: {propertyName} = {value}");
        }

        #endregion

        #region Generic Event Tracking

        /// <summary>
        /// Track a generic analytics event
        /// </summary>
        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            if (!_enableAnalytics || analyticsEvent == null) return;

            if (_enableBatching)
            {
                QueueEvent(analyticsEvent);
            }
            else
            {
                DispatchToProviders(p => p.TrackEvent(analyticsEvent));
            }
        }

        /// <summary>
        /// Track a simple named event with optional parameters
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_enableAnalytics) return;

            var analyticsEvent = new AnalyticsEvent(eventName);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    analyticsEvent.WithParameter(param.Key, param.Value);
                }
            }

            TrackEvent(analyticsEvent);
        }

        private void QueueEvent(AnalyticsEvent analyticsEvent)
        {
            lock (_lockObject)
            {
                _eventQueue.Enqueue(analyticsEvent);

                if (_eventQueue.Count >= _maxBatchSize)
                {
                    FlushEventQueue();
                }
            }
        }

        /// <summary>
        /// Flush all queued events to providers
        /// </summary>
        public void FlushEventQueue()
        {
            if (!_enableAnalytics) return;

            lock (_lockObject)
            {
                while (_eventQueue.Count > 0)
                {
                    var analyticsEvent = _eventQueue.Dequeue();
                    DispatchToProviders(p => p.TrackEvent(analyticsEvent));
                }
            }

            DispatchToProviders(p => p.Flush());
            _lastFlushTime = Time.time;
        }

        #endregion

        #region Helper Methods

        private void DispatchToProviders(Action<IAnalyticsProvider> action)
        {
            lock (_lockObject)
            {
                foreach (var provider in _providers)
                {
                    if (!provider.IsEnabled) continue;

                    try
                    {
                        action(provider);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error dispatching to provider {provider.ProviderId}: {ex.Message}");
                    }
                }
            }
        }

        #endregion

        #region Public Settings

        /// <summary>
        /// Enable or disable analytics collection at runtime
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _enableAnalytics = enabled;
            _logger.Log($"Analytics {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Enable or disable debug logging at runtime
        /// </summary>
        public void SetDebugLogging(bool enabled)
        {
            _logger.IsEnabled = enabled;
        }

        /// <summary>
        /// Reload settings from ScriptableObject (useful after changing settings at runtime)
        /// </summary>
        public void ReloadSettings()
        {
            LoadSettings();
            _logger.Log("Analytics settings reloaded");
        }

        #endregion
    }
}
