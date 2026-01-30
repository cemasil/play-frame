using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Analytics provider that stores events locally for offline analysis.
    /// Events are stored in a JSON file and can be exported or synced later.
    /// </summary>
    public class LocalStorageAnalyticsProvider : BaseAnalyticsProvider
    {
        private const string ANALYTICS_FILE_NAME = "analytics_events.json";
        private const int MAX_EVENTS_IN_MEMORY = 100;
        private const int MAX_EVENTS_IN_FILE = 1000;

        private readonly List<StoredAnalyticsEvent> _eventBuffer = new List<StoredAnalyticsEvent>();
        private readonly object _lockObject = new object();
        private string _filePath;

        public override string ProviderId => "local_storage";

        public override void Initialize()
        {
            _filePath = Path.Combine(Application.persistentDataPath, ANALYTICS_FILE_NAME);
            LoadExistingEvents();
        }

        public override void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            var storedEvent = CreateStoredEvent(analyticsEvent);
            AddToBuffer(storedEvent);
        }

        public override void TrackLevelCompleted(LevelCompletedEvent levelEvent)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = levelEvent.EventName,
                timestamp = levelEvent.Timestamp.ToString("o"),
                parameters = new Dictionary<string, string>
                {
                    { AnalyticsParameterNames.GAME_NAME, levelEvent.GameName },
                    { AnalyticsParameterNames.LEVEL_NUMBER, levelEvent.LevelNumber.ToString() },
                    { AnalyticsParameterNames.LEVEL_ID, levelEvent.LevelId },
                    { AnalyticsParameterNames.SCORE, levelEvent.Score.ToString() },
                    { AnalyticsParameterNames.TIME_SECONDS, levelEvent.CompletionTimeSeconds.ToString("F2") },
                    { AnalyticsParameterNames.MOVE_COUNT, levelEvent.MoveCount.ToString() },
                    { AnalyticsParameterNames.RETRY_COUNT, levelEvent.RetryCount.ToString() },
                    { AnalyticsParameterNames.HIGH_SCORE_NEW, levelEvent.IsNewHighScore.ToString() },
                    { AnalyticsParameterNames.STARS, levelEvent.Stars.ToString() },
                    { AnalyticsParameterNames.DIFFICULTY, levelEvent.Difficulty },
                    { "match_count", levelEvent.MatchCount.ToString() }
                }
            };

            AddToBuffer(storedEvent);
        }

        public override void TrackLevelStarted(LevelStartedEvent levelEvent)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = levelEvent.EventName,
                timestamp = levelEvent.Timestamp.ToString("o"),
                parameters = new Dictionary<string, string>
                {
                    { AnalyticsParameterNames.GAME_NAME, levelEvent.GameName },
                    { AnalyticsParameterNames.LEVEL_NUMBER, levelEvent.LevelNumber.ToString() },
                    { AnalyticsParameterNames.LEVEL_ID, levelEvent.LevelId },
                    { AnalyticsParameterNames.DIFFICULTY, levelEvent.Difficulty },
                    { "attempt_number", levelEvent.AttemptNumber.ToString() }
                }
            };

            AddToBuffer(storedEvent);
        }

        public override void TrackLevelFailed(LevelFailedEvent levelEvent)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = levelEvent.EventName,
                timestamp = levelEvent.Timestamp.ToString("o"),
                parameters = new Dictionary<string, string>
                {
                    { AnalyticsParameterNames.GAME_NAME, levelEvent.GameName },
                    { AnalyticsParameterNames.LEVEL_NUMBER, levelEvent.LevelNumber.ToString() },
                    { AnalyticsParameterNames.LEVEL_ID, levelEvent.LevelId },
                    { AnalyticsParameterNames.FAIL_REASON, levelEvent.FailReason },
                    { AnalyticsParameterNames.TIME_SECONDS, levelEvent.PlayTimeSeconds.ToString("F2") },
                    { AnalyticsParameterNames.SCORE, levelEvent.Score.ToString() },
                    { AnalyticsParameterNames.MOVE_COUNT, levelEvent.MoveCount.ToString() },
                    { AnalyticsParameterNames.RETRY_COUNT, levelEvent.RetryCount.ToString() },
                    { AnalyticsParameterNames.DIFFICULTY, levelEvent.Difficulty }
                }
            };

            AddToBuffer(storedEvent);
        }

        public override void TrackSessionStart(SessionStartEvent sessionEvent)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = sessionEvent.EventName,
                timestamp = sessionEvent.Timestamp.ToString("o"),
                parameters = new Dictionary<string, string>
                {
                    { AnalyticsParameterNames.SESSION_ID, sessionEvent.SessionId },
                    { AnalyticsParameterNames.GAME_NAME, sessionEvent.GameName },
                    { "game_version", sessionEvent.GameVersion },
                    { "platform", sessionEvent.Platform },
                    { "device_model", sessionEvent.DeviceModel },
                    { "operating_system", sessionEvent.OperatingSystem }
                }
            };

            AddToBuffer(storedEvent);
        }

        public override void TrackSessionEnd(SessionEndEvent sessionEvent)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = sessionEvent.EventName,
                timestamp = sessionEvent.Timestamp.ToString("o"),
                parameters = new Dictionary<string, string>
                {
                    { AnalyticsParameterNames.SESSION_ID, sessionEvent.SessionId },
                    { AnalyticsParameterNames.SESSION_DURATION, sessionEvent.SessionDurationSeconds.ToString("F2") },
                    { AnalyticsParameterNames.LEVELS_PLAYED, sessionEvent.LevelsPlayed.ToString() },
                    { AnalyticsParameterNames.SCORE, sessionEvent.TotalScore.ToString() }
                }
            };

            AddToBuffer(storedEvent);
        }

        public override void SetUserProperty(string propertyName, string value)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = "user_property_set",
                timestamp = DateTime.UtcNow.ToString("o"),
                parameters = new Dictionary<string, string>
                {
                    { "property_name", propertyName },
                    { "property_value", value }
                }
            };

            AddToBuffer(storedEvent);
        }

        public override void Flush()
        {
            SaveToFile();
        }

        public override void Shutdown()
        {
            SaveToFile();
        }

        #region Helper Methods

        private StoredAnalyticsEvent CreateStoredEvent(AnalyticsEvent analyticsEvent)
        {
            var storedEvent = new StoredAnalyticsEvent
            {
                eventName = analyticsEvent.EventName,
                timestamp = analyticsEvent.Timestamp.ToString("o"),
                parameters = new Dictionary<string, string>()
            };

            foreach (var param in analyticsEvent.Parameters)
            {
                storedEvent.parameters[param.Key] = param.Value?.ToString() ?? "";
            }

            return storedEvent;
        }

        private void AddToBuffer(StoredAnalyticsEvent storedEvent)
        {
            lock (_lockObject)
            {
                _eventBuffer.Add(storedEvent);

                if (_eventBuffer.Count >= MAX_EVENTS_IN_MEMORY)
                {
                    SaveToFile();
                }
            }
        }

        private void LoadExistingEvents()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    var container = JsonUtility.FromJson<AnalyticsEventContainer>(json);
                    if (container?.events != null)
                    {
                        lock (_lockObject)
                        {
                            _eventBuffer.AddRange(container.events);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocalStorageAnalytics] Failed to load existing events: {ex.Message}");
            }
        }

        private void SaveToFile()
        {
            lock (_lockObject)
            {
                if (_eventBuffer.Count == 0) return;

                try
                {
                    // Trim old events if too many
                    while (_eventBuffer.Count > MAX_EVENTS_IN_FILE)
                    {
                        _eventBuffer.RemoveAt(0);
                    }

                    var container = new AnalyticsEventContainer
                    {
                        events = _eventBuffer.ToArray(),
                        lastUpdated = DateTime.UtcNow.ToString("o")
                    };

                    string json = JsonUtility.ToJson(container, true);
                    File.WriteAllText(_filePath, json);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LocalStorageAnalytics] Failed to save events: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Get all stored events for export
        /// </summary>
        public StoredAnalyticsEvent[] GetStoredEvents()
        {
            lock (_lockObject)
            {
                return _eventBuffer.ToArray();
            }
        }

        /// <summary>
        /// Clear all stored events
        /// </summary>
        public void ClearStoredEvents()
        {
            lock (_lockObject)
            {
                _eventBuffer.Clear();

                try
                {
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[LocalStorageAnalytics] Failed to delete file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Export events as JSON string
        /// </summary>
        public string ExportAsJson()
        {
            lock (_lockObject)
            {
                var container = new AnalyticsEventContainer
                {
                    events = _eventBuffer.ToArray(),
                    lastUpdated = DateTime.UtcNow.ToString("o")
                };

                return JsonUtility.ToJson(container, true);
            }
        }

        #endregion
    }

    #region Serializable Types

    [Serializable]
    public class StoredAnalyticsEvent
    {
        public string eventName;
        public string timestamp;
        public Dictionary<string, string> parameters;

        // For Unity serialization (Dictionary needs special handling)
        [SerializeField] private List<string> paramKeys = new List<string>();
        [SerializeField] private List<string> paramValues = new List<string>();

        public void OnBeforeSerialize()
        {
            if (parameters == null) return;

            paramKeys.Clear();
            paramValues.Clear();

            foreach (var kvp in parameters)
            {
                paramKeys.Add(kvp.Key);
                paramValues.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            parameters = new Dictionary<string, string>();

            for (int i = 0; i < Mathf.Min(paramKeys.Count, paramValues.Count); i++)
            {
                parameters[paramKeys[i]] = paramValues[i];
            }
        }
    }

    [Serializable]
    public class AnalyticsEventContainer
    {
        public StoredAnalyticsEvent[] events;
        public string lastUpdated;
    }

    #endregion
}
