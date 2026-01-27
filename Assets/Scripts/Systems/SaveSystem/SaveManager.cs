using System;
using UnityEngine;
using MiniGameFramework.Core;
using MiniGameFramework.Systems.Events;

namespace MiniGameFramework.Systems.SaveSystem
{
    /// <summary>
    /// Manages saving and loading game data using PlayerPrefs
    /// Uses PersistentSingleton to survive scene transitions and avoid race conditions with EventManager
    /// </summary>
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        private const string SAVE_KEY = "GameSaveData";
        private SaveData _currentSaveData;
        private bool _isInitialized = false;

        public SaveData CurrentSaveData => _currentSaveData;

        protected override void OnSingletonAwake()
        {
            LoadGame();
            _isInitialized = true;
        }

        public void SaveGame()
        {
            try
            {
                if (_currentSaveData == null)
                {
                    _currentSaveData = new SaveData();
                }

                _currentSaveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _currentSaveData.PrepareForSave();

                string json = JsonUtility.ToJson(_currentSaveData, true);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();

                if (_isInitialized && EventManager.HasInstance)
                {
                    EventManager.Instance.TriggerEvent(GameEvents.GameSaved);
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
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    _currentSaveData = JsonUtility.FromJson<SaveData>(json);
                    _currentSaveData.InitializeDictionary();
                }
                else
                {
                    _currentSaveData = new SaveData();
                    SaveGame();
                }

                if (_isInitialized && EventManager.HasInstance)
                {
                    EventManager.Instance.TriggerEvent(GameEvents.GameLoaded);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                _currentSaveData = new SaveData();
            }
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            _currentSaveData = new SaveData();
        }

        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }

        public void UpdateScore(int score)
        {
            _currentSaveData.totalScore += score;

            if (_currentSaveData.totalScore > _currentSaveData.highScore)
            {
                _currentSaveData.highScore = _currentSaveData.totalScore;
                TriggerEventSafe(GameEvents.HighScoreUpdated, _currentSaveData.highScore);
            }

            TriggerEventSafe(GameEvents.ScoreUpdated, _currentSaveData.totalScore);
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
        /// Safely trigger a type-safe event only if EventManager is available
        /// </summary>
        private void TriggerEventSafe<T>(GameEvent<T> gameEvent, T parameter)
        {
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(gameEvent, parameter);
            }
        }
    }
}
