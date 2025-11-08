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
                EventManager.Instance.TriggerEvent(GameEvents.GAME_SAVED);
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

                EventManager.Instance.TriggerEvent(GameEvents.GAME_LOADED);
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
                EventManager.Instance.TriggerEvent(GameEvents.HIGH_SCORE_UPDATED, _currentSaveData.highScore);
            }

            EventManager.Instance.TriggerEvent(GameEvents.SCORE_UPDATED, _currentSaveData.totalScore);
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
    }
}
