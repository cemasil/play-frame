using UnityEngine;
using UnityEngine.UI;

namespace PlayFrame.MiniGames
{
    /// <summary>
    /// Configuration for a mini-game
    /// Create via: Assets -> Create -> MiniGames -> Game Config
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MiniGames/Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Info")]
        public string gameName;
        public string displayName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        public Sprite thumbnail;

        [Header("Scene")]
        public string sceneName;

        [Header("UI")]
        public Color themeColor = Color.white;
        public Sprite backgroundSprite;

        [Header("Gameplay")]
        public int defaultHighScore = 0;
        public bool hasLevels = false;
        public int maxLevel = 10;
    }
}
