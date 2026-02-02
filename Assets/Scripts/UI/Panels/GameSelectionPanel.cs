using UnityEngine;
using UnityEngine.UI;
using PlayFrame.Core.Logging;
using PlayFrame.Systems.Scene;
using PlayFrame.Systems.Save;
using PlayFrame.MiniGames;
using PlayFrame.UI.Base;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.UI.Panels
{
    public class GameSelectionPanel : UIPanel
    {
        [Header("Container")]
        [SerializeField] private Transform gameContainer;
        [SerializeField] private Button backButton;

        private ILogger _logger;

        protected override void OnInitialize()
        {
            _logger = LoggerFactory.CreateUI("GameSelectionPanel");

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            PopulateGameCards();
        }

        private void PopulateGameCards()
        {
            if (gameContainer == null)
            {
                _logger.LogError("Game Container is not assigned!");
                return;
            }

            var games = GameRegistry.Instance.AvailableGames;

            foreach (var game in games)
            {
                int highScore = SaveManager.Instance.GetGameHighScore(game.gameName);

                var card = UIPrefabFactory.CreateGameCard(
                    game.displayName,
                    highScore,
                    game.themeColor,
                    gameContainer
                );

                if (card == null)
                {
                    _logger.LogWarning($"Failed to create card for {game.displayName}");
                    continue;
                }

                var playButton = card.GetComponentInChildren<Button>();
                if (playButton != null)
                {
                    string sceneName = game.sceneName; // Capture for lambda
                    playButton.onClick.AddListener(() => SceneLoaderManager.Instance.LoadScene(sceneName));
                }
            }
        }

        private void OnBackClicked()
        {
            SceneLoaderManager.Instance.LoadScene(SceneNames.MAIN_MENU);
        }

        protected override void OnCleanup()
        {
            if (backButton != null)
                backButton.onClick.RemoveAllListeners();

            if (gameContainer != null)
            {
                foreach (Transform child in gameContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
