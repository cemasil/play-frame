using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Systems.SceneManagement;
using MiniGameFramework.Systems.SaveSystem;
using MiniGameFramework.MiniGames.Common;

namespace MiniGameFramework.MiniGames.Match3
{
    public class Match3Game : BaseGameUI
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private int currentScore = 0;
        private int highScore = 0;

        protected override void OnInitialize()
        {
            SaveManager.Instance.LoadGame();
            highScore = SaveManager.Instance.GetGameHighScore(GameNames.MATCH3);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        protected override void OnGameStart()
        {
            currentScore = 0;
            UpdateScore(currentScore);
        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddScore(10);
            }

            if (Input.GetKeyDown(KeyCode.Escape) && gameOverPanel != null && !gameOverPanel.activeSelf)
            {
                ShowGameOver(currentScore);
            }
        }

        private void AddScore(int points)
        {
            currentScore += points;
            UpdateScore(currentScore);

            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveManager.Instance.UpdateGameHighScore(GameNames.MATCH3, highScore);
            }
        }

        protected override void UpdateScore(int score)
        {
            currentScore = score;

            if (scoreText != null)
                scoreText.text = $"Score: {currentScore}";

            if (highScoreText != null)
                highScoreText.text = $"Best: {highScore}";
        }

        protected override void ShowGameOver(int finalScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                if (finalScoreText != null)
                    finalScoreText.text = $"Final Score: {finalScore}";
            }
        }

        private void OnBackClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.GAME_SELECTION);
        }

        private void OnRestartClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.MATCH3);
        }

        private void OnMenuClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.MAIN_MENU);
        }

        protected override void Cleanup()
        {
            if (backButton != null)
                backButton.onClick.RemoveAllListeners();

            if (restartButton != null)
                restartButton.onClick.RemoveAllListeners();

            if (menuButton != null)
                menuButton.onClick.RemoveAllListeners();
        }
    }
}
