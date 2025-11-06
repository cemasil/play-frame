using System;
using UnityEngine;
using MiniGameFramework.Core;
using MiniGameFramework.Systems.Events;

namespace MiniGameFramework.Systems.SaveSystem
{
    /// <summary>
    /// Manages saving and loading game data using PlayerPrefs
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        private const string SAVE_KEY = "GameSaveData";
        private SaveData _currentSaveData;

        public SaveData CurrentSaveData => _currentSaveData;

        public SaveManager()
        {
            LoadGame();
        }

        /// <summary>
        /// Save current game data to PlayerPrefs
        /// </summary>
        public void SaveGame()
        {
            try
            {
                if (_currentSaveData == null)
                {
                    _currentSaveData = new SaveData();
                }

                _currentSaveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string json = JsonUtility.ToJson(_currentSaveData, true);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
                Debug.Log($"[SaveManager] Game saved successfully at {_currentSaveData.saveDate}");
                EventManager.Instance.TriggerEvent(GameEvents.GAME_SAVED);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Load game data from PlayerPrefs
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    _currentSaveData = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log($"[SaveManager] Game loaded. Last saved: {_currentSaveData.saveDate}");
                }
                else
                {
                    Debug.Log("[SaveManager] No save data found. Creating new save.");
                    _currentSaveData = new SaveData();
                    SaveGame();
                }

                EventManager.Instance.TriggerEvent(GameEvents.GAME_LOADED);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                _currentSaveData = new SaveData();
            }
        }

        /// <summary>
        /// Delete all save data
        /// </summary>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            _currentSaveData = new SaveData();
            Debug.Log("[SaveManager] Save data deleted");
        }

        /// <summary>
        /// Check if save data exists
        /// </summary>
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }

        #region Helper Methods

        /// <summary>
        /// Update total score and save
        /// </summary>
        public void UpdateScore(int score)
        {
            _currentSaveData.totalScore += score;

            if (_currentSaveData.totalScore > _currentSaveData.highScore)
            {
                _currentSaveData.highScore = _currentSaveData.totalScore;
                EventManager.Instance.TriggerEvent(GameEvents.HIGH_SCORE_UPDATED, _currentSaveData.highScore);
            }

            EventManager.Instance.TriggerEvent(GameEvents.SCORE_UPDATED, _currentSaveData.totalScore);
            SaveGame();
        }

        /// <summary>
        /// Update game-specific high score
        /// </summary>
        public void UpdateGameHighScore(string gameName, int score)
        {
            switch (gameName)
            {
                case GameNames.MATCH3:
                    if (score > _currentSaveData.match3HighScore)
                    {
                        _currentSaveData.match3HighScore = score;
                        SaveGame();
                    }
                    break;

                case GameNames.MEMORY:
                    if (score > _currentSaveData.memoryHighScore)
                    {
                        _currentSaveData.memoryHighScore = score;
                        SaveGame();
                    }
                    break;
            }
        }

        /// <summary>
        /// Increment games played counter
        /// </summary>
        public void IncrementGamesPlayed()
        {
            _currentSaveData.gamesPlayed++;
            SaveGame();
        }

        #endregion
    }
}
