using System;
using UnityEngine;
using PlayFrame.Core;
using PlayFrame.Core.Events;

namespace PlayFrame.Systems.SaveSystem
{
    /// <summary>
    /// Manages saving and loading game data using PlayerPrefs.
    /// Uses PersistentSingleton to survive scene transitions.
    /// Configuration is loaded from SaveSettings ScriptableObject.
    /// </summary>
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        [Header("Settings")]
        [Tooltip("Optional: Assign settings directly. If null, uses default settings")]
        [SerializeField] private SaveSettings settings;

        private SaveData _currentSaveData;
        private bool _isInitialized = false;
        private float _lastAutoSaveTime;

        // Cached settings for runtime access
        private string _saveKey;
        private bool _prettyPrintJson;
        private bool _enableAutoSave;
        private float _autoSaveInterval;
        private bool _enableBackup;
        private bool _enableDebugLogs;
        private bool _logSaveContents;

        public SaveData CurrentSaveData => _currentSaveData;
        public SaveSettings Settings => settings;

        protected override void OnSingletonAwake()
        {
            LoadSettings();
            LoadGame();
            _isInitialized = true;
            _lastAutoSaveTime = Time.time;
        }

        private void LoadSettings()
        {
            if (settings == null)
            {
                settings = SaveSettings.CreateDefault();
            }

            // Cache settings for runtime performance
            _saveKey = settings.SaveKey;
            _prettyPrintJson = settings.PrettyPrintJson;
            _enableAutoSave = settings.EnableAutoSave;
            _autoSaveInterval = settings.AutoSaveInterval;
            _enableBackup = settings.EnableBackup;
            _enableDebugLogs = settings.EnableDebugLogs;
            _logSaveContents = settings.LogSaveContents;

            LogDebug("Save settings loaded");
        }

        private void Update()
        {
            if (!_isInitialized || !_enableAutoSave) return;

            if (Time.time - _lastAutoSaveTime >= _autoSaveInterval)
            {
                LogDebug("Auto-saving...");
                SaveGame();
                _lastAutoSaveTime = Time.time;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _isInitialized && settings.SaveOnApplicationPause)
            {
                LogDebug("Saving on application pause...");
                SaveGame();
            }
        }

#if UNITY_EDITOR
        protected override void OnApplicationQuit()
        {
            if (_isInitialized && settings.SaveOnApplicationQuit)
            {
                LogDebug("Saving on application quit...");
                SaveGame();
            }
            base.OnApplicationQuit();
        }
#else
        private void OnApplicationQuit()
        {
            if (_isInitialized && settings.SaveOnApplicationQuit)
            {
                LogDebug("Saving on application quit...");
                SaveGame();
            }
        }
#endif

        public void SaveGame()
        {
            try
            {
                if (_currentSaveData == null)
                {
                    _currentSaveData = new SaveData();
                }

                // Create backup before saving (if enabled)
                if (_enableBackup && PlayerPrefs.HasKey(_saveKey))
                {
                    CreateBackup();
                }

                _currentSaveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _currentSaveData.PrepareForSave();

                string json = JsonUtility.ToJson(_currentSaveData, _prettyPrintJson);
                PlayerPrefs.SetString(_saveKey, json);
                PlayerPrefs.Save();

                LogDebug("Game saved successfully");
                if (_logSaveContents)
                {
                    Debug.Log($"[SaveManager] Save contents:\n{json}");
                }

                if (_isInitialized && EventManager.HasInstance)
                {
                    EventManager.Instance.TriggerEvent(CoreEvents.GameSaved);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            }
        }

        public void LoadGame()
        {
            try
            {
                if (PlayerPrefs.HasKey(_saveKey))
                {
                    string json = PlayerPrefs.GetString(_saveKey);

                    // Validate data if enabled
                    if (settings.ValidateOnLoad && !ValidateSaveData(json))
                    {
                        Debug.LogWarning("[SaveManager] Save data validation failed");

                        if (settings.RestoreFromBackupOnError)
                        {
                            LogDebug("Attempting to restore from backup...");
                            if (RestoreFromBackup())
                            {
                                return;
                            }
                        }

                        // If backup restore failed, create new save
                        _currentSaveData = new SaveData();
                        SaveGame();
                        return;
                    }

                    _currentSaveData = JsonUtility.FromJson<SaveData>(json);
                    _currentSaveData.InitializeDictionary();

                    LogDebug("Game loaded successfully");
                    if (_logSaveContents)
                    {
                        Debug.Log($"[SaveManager] Loaded contents:\n{json}");
                    }
                }
                else
                {
                    LogDebug("No save data found, creating new save");
                    _currentSaveData = new SaveData();
                    SaveGame();
                }

                if (_isInitialized && EventManager.HasInstance)
                {
                    EventManager.Instance.TriggerEvent(CoreEvents.GameLoaded);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");

                if (settings.RestoreFromBackupOnError)
                {
                    LogDebug("Attempting to restore from backup after error...");
                    if (!RestoreFromBackup())
                    {
                        _currentSaveData = new SaveData();
                    }
                }
                else
                {
                    _currentSaveData = new SaveData();
                }
            }
        }

        private void CreateBackup()
        {
            try
            {
                string currentData = PlayerPrefs.GetString(_saveKey);

                // Rotate backups if multiple slots
                if (settings.BackupSlotCount > 1)
                {
                    for (int i = settings.BackupSlotCount - 1; i > 0; i--)
                    {
                        string sourceKey = settings.GetBackupKey(i - 1);
                        string targetKey = settings.GetBackupKey(i);

                        if (PlayerPrefs.HasKey(sourceKey))
                        {
                            PlayerPrefs.SetString(targetKey, PlayerPrefs.GetString(sourceKey));
                        }
                    }
                }

                PlayerPrefs.SetString(settings.BackupKey, currentData);
                LogDebug("Backup created");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to create backup: {e.Message}");
            }
        }

        private bool RestoreFromBackup()
        {
            // Try each backup slot
            for (int i = 0; i < settings.BackupSlotCount; i++)
            {
                string backupKey = settings.GetBackupKey(i);

                if (PlayerPrefs.HasKey(backupKey))
                {
                    try
                    {
                        string backupJson = PlayerPrefs.GetString(backupKey);

                        if (ValidateSaveData(backupJson))
                        {
                            _currentSaveData = JsonUtility.FromJson<SaveData>(backupJson);
                            _currentSaveData.InitializeDictionary();

                            // Save restored data as main save
                            SaveGame();

                            Debug.Log($"[SaveManager] Successfully restored from backup slot {i}");
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[SaveManager] Failed to restore from backup slot {i}: {e.Message}");
                    }
                }
            }

            Debug.LogWarning("[SaveManager] No valid backup found");
            return false;
        }

        private bool ValidateSaveData(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                // Try to parse and check for basic required fields
                var testData = JsonUtility.FromJson<SaveData>(json);
                return testData != null;
            }
            catch
            {
                return false;
            }
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(_saveKey);

            // Also delete backups
            if (_enableBackup)
            {
                for (int i = 0; i < settings.BackupSlotCount; i++)
                {
                    PlayerPrefs.DeleteKey(settings.GetBackupKey(i));
                }
            }

            PlayerPrefs.Save();
            _currentSaveData = new SaveData();

            LogDebug("Save data deleted");
        }

        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(_saveKey);
        }

        public bool HasBackup()
        {
            return PlayerPrefs.HasKey(settings.BackupKey);
        }

        public void UpdateScore(int score)
        {
            _currentSaveData.totalScore += score;

            if (_currentSaveData.totalScore > _currentSaveData.highScore)
            {
                _currentSaveData.highScore = _currentSaveData.totalScore;
                TriggerEventSafe(CoreEvents.HighScoreUpdated, _currentSaveData.highScore);
            }

            TriggerEventSafe(CoreEvents.ScoreUpdated, _currentSaveData.totalScore);
            SaveGame();
        }

        public void UpdateGameHighScore(string gameName, int score)
        {
            _currentSaveData.UpdateGameHighScore(gameName, score);
            SaveGame();
        }

        public int GetGameHighScore(string gameName)
        {
            return _currentSaveData.GetGameHighScore(gameName);
        }

        public GameSaveData GetGameData(string gameName)
        {
            return _currentSaveData.GetGameData(gameName);
        }

        public void SetGameData(string gameName, GameSaveData data)
        {
            _currentSaveData.SetGameData(gameName, data);
            SaveGame();
        }

        public void IncrementGamesPlayed()
        {
            _currentSaveData.gamesPlayed++;
            SaveGame();
        }

        /// <summary>
        /// Force reload settings from ScriptableObject
        /// </summary>
        public void ReloadSettings()
        {
            LoadSettings();
        }

        /// <summary>
        /// Safely trigger a type-safe event only if EventManager is available
        /// </summary>
        private void TriggerEventSafe<T>(GameEvent<T> gameEvent, T parameter)
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(gameEvent, parameter);
            }
        }

        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[SaveManager] {message}");
            }
        }
    }
}
