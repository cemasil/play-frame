using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFrame.Systems.Save
{
    [Serializable]
    public class SaveData
    {
        public int totalScore;
        public int highScore;
        public int gamesPlayed;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public string lastPlayedGame;
        public string saveDate;

        [SerializeField] private List<GameDataEntry> gameDataEntries = new List<GameDataEntry>();
        private Dictionary<string, GameSaveData> _gameDataDictionary;

        public SaveData()
        {
            totalScore = 0;
            highScore = 0;
            gamesPlayed = 0;
            musicVolume = 1f;
            sfxVolume = 1f;
            lastPlayedGame = "";
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _gameDataDictionary = new Dictionary<string, GameSaveData>();
        }

        public GameSaveData GetGameData(string gameName)
        {
            if (_gameDataDictionary == null)
                InitializeDictionary();

            if (!_gameDataDictionary.ContainsKey(gameName))
            {
                _gameDataDictionary[gameName] = new GameSaveData();
            }

            return _gameDataDictionary[gameName];
        }

        public void SetGameData(string gameName, GameSaveData data)
        {
            if (_gameDataDictionary == null)
                InitializeDictionary();

            _gameDataDictionary[gameName] = data;
        }

        public int GetGameHighScore(string gameName)
        {
            return GetGameData(gameName).highScore;
        }

        public void UpdateGameHighScore(string gameName, int score)
        {
            var gameData = GetGameData(gameName);
            if (score > gameData.highScore)
            {
                gameData.highScore = score;
            }
        }

        public void PrepareForSave()
        {
            if (_gameDataDictionary == null) return;

            gameDataEntries.Clear();
            foreach (var kvp in _gameDataDictionary)
            {
                gameDataEntries.Add(new GameDataEntry
                {
                    gameName = kvp.Key,
                    data = kvp.Value
                });
            }
        }

        public void InitializeDictionary()
        {
            _gameDataDictionary = new Dictionary<string, GameSaveData>();

            if (gameDataEntries != null)
            {
                foreach (var entry in gameDataEntries)
                {
                    _gameDataDictionary[entry.gameName] = entry.data;
                }
            }
        }
    }

    [Serializable]
    public class GameSaveData
    {
        public int highScore;
        public int gamesPlayed;
        public int currentLevel;
        public string customData;

        public GameSaveData()
        {
            highScore = 0;
            gamesPlayed = 0;
            currentLevel = 1;
            customData = "";
        }
    }

    [Serializable]
    internal class GameDataEntry
    {
        public string gameName;
        public GameSaveData data;
    }
}
