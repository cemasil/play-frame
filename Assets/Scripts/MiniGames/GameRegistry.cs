using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core;

namespace MiniGameFramework.MiniGames
{
    /// <summary>
    /// Registry for all available mini-games
    /// Automatically finds all GameConfig assets in Resources/Games/
    /// </summary>
    public class GameRegistry : PersistentSingleton<GameRegistry>
    {
        private List<GameConfig> _availableGames;
        public List<GameConfig> AvailableGames => _availableGames;

        protected override void Awake()
        {
            base.Awake();
            LoadGames();
        }

        /// <summary>
        /// Load all game configs from Resources/Games/
        /// </summary>
        private void LoadGames()
        {
            _availableGames = new List<GameConfig>();
            var configs = Resources.LoadAll<GameConfig>("Games");

            foreach (var config in configs)
            {
                _availableGames.Add(config);
            }
        }

        public GameConfig GetGameConfig(string gameName)
        {
            return _availableGames.Find(g => g.gameName == gameName);
        }
    }
}
