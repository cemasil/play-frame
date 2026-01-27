using UnityEngine;
using UnityEngine.UI;
using MiniGameFramework.Systems.SceneManagement;
using MiniGameFramework.Systems.SaveSystem;
using MiniGameFramework.Systems.UI;
using MiniGameFramework.UI.Prefabs;
using MiniGameFramework.MiniGames;

namespace MiniGameFramework.UI.Panels
{
    public class GameSelectionPanel : UIPanel
    {
        [Header("Container")]
        [SerializeField] private Transform gameContainer;
        [SerializeField] private Button backButton;

        protected override void OnInitialize()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            PopulateGameCards();
        }

        private void PopulateGameCards()
        {
            if (gameContainer == null)
            {
                Debug.LogError("[GameSelectionPanel] Game Container is not assigned!");
                return;
            }

            if (!GameRegistry.HasInstance)
            {
                Debug.LogError("[GameSelectionPanel] GameRegistry is not available!");
                return;
            }

            var games = GameRegistry.Instance.AvailableGames;

            foreach (var game in games)
            {
                int highScore = SaveManager.HasInstance
                    ? SaveManager.Instance.GetGameHighScore(game.gameName)
                    : 0;

                var card = UIPrefabFactory.CreateGameCard(
                    game.displayName,
                    highScore,
                    game.themeColor,
                    gameContainer
                );

                if (card == null)
                {
                    Debug.LogWarning($"[GameSelectionPanel] Failed to create card for {game.displayName}");
                    continue;
                }

                var playButton = card.GetComponentInChildren<Button>();
                if (playButton != null)
                {
                    string sceneName = game.sceneName; // Capture for lambda
                    playButton.onClick.AddListener(() =>
                    {
                        if (SceneLoader.HasInstance)
                            SceneLoader.Instance.LoadScene(sceneName);
                    });
                }
            }
        }

        private void OnBackClicked()
        {
            if (SceneLoader.HasInstance)
                SceneLoader.Instance.LoadScene(SceneNames.MAIN_MENU);
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
