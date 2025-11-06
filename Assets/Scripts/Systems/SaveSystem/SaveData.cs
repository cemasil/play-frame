using System;
using UnityEngine;

namespace MiniGameFramework.Systems.SaveSystem
{
    /// <summary>
    /// Main save data structure
    /// Field naming convention: {gameName}HighScore (e.g., match3HighScore)
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Player Progress
        public int totalScore;
        public int highScore;
        public int gamesPlayed;

        // Game Specific Data
        public int match3HighScore;
        public int memoryHighScore;

        // Settings
        public float musicVolume = 1f;
        public float sfxVolume = 1f;

        // Metadata
        public string lastPlayedGame;
        public string saveDate;

        public SaveData()
        {
            totalScore = 0;
            highScore = 0;
            gamesPlayed = 0;
            match3HighScore = 0;
            memoryHighScore = 0;
            musicVolume = 1f;
            sfxVolume = 1f;
            lastPlayedGame = "";
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
