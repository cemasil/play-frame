using System.Collections.Generic;
using UnityEngine;
using PlayFrame.Core;

namespace PlayFrame.MiniGames
{
    /// <summary>
    /// Registry for all available mini-games.
    /// Assign GameConfig assets via Inspector from GameSettings/Games/
    /// </summary>
    public class GameRegistry : PersistentSingleton<GameRegistry>
    {
        [Header("Game Configurations")]
        [Tooltip("Assign all GameConfig assets from GameSettings/Games folder")]
        [SerializeField] private List<GameConfig> gameConfigs = new List<GameConfig>();

        public List<GameConfig> AvailableGames => gameConfigs;

        protected override void OnSingletonAwake()
        {
            ValidateConfigs();
        }

        /// <summary>
        /// Validate that configs are assigned
        /// </summary>
        private void ValidateConfigs()
        {
            if (gameConfigs == null || gameConfigs.Count == 0)
            {
                Debug.LogWarning("[GameRegistry] No GameConfig assets assigned. " +
                    "Please assign GameConfig assets from GameSettings/Games folder via Inspector.");
            }
            else
            {
                // Remove any null entries
                gameConfigs.RemoveAll(config => config == null);
                Debug.Log($"[GameRegistry] Loaded {gameConfigs.Count} game(s)");
            }
        }

        public GameConfig GetGameConfig(string gameName)
        {
            return gameConfigs.Find(g => g.gameName == gameName);
        }

        /// <summary>
        /// Check if a game is available
        /// </summary>
        public bool HasGame(string gameName)
        {
            return gameConfigs.Exists(g => g.gameName == gameName);
        }

        /// <summary>
        /// Get game count
        /// </summary>
        public int GameCount => gameConfigs?.Count ?? 0;
    }
}
