using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Systems.SceneManagement;
using MiniGameFramework.Systems.SaveSystem;
using MiniGameFramework.MiniGames.Common;

namespace MiniGameFramework.MiniGames.Match3
{
    /// <summary>
    /// Main Match3 game controller
    /// </summary>
    public class Match3Game : BaseGameUI
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;
        [SerializeField] private float gemSize = 90f;
        [SerializeField] private float gemSpacing = 10f;

        [Header("Prefabs")]
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private Transform gridContainer;

        [Header("Colors")]
        [SerializeField] private Color[] gemColors = new Color[5];

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private Button backButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Game Settings")]
        [SerializeField] private int maxMoves = 15;
        [SerializeField] private int targetScore = 500;
        [SerializeField] private int pointsPerGem = 10;

        private Match3Grid match3Grid;
        private int currentScore = 0;
        private int remainingMoves;
        private bool isProcessing = false;

        protected override void OnInitialize()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            match3Grid = new Match3Grid(gridWidth, gridHeight);
            remainingMoves = maxMoves;

            FillGrid();
            UpdateUI();
        }

        private void FillGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (match3Grid.GetGem(x, y) == null)
                    {
                        float totalWidth = (gridWidth * gemSize) + ((gridWidth - 1) * gemSpacing);
                        float totalHeight = (gridHeight * gemSize) + ((gridHeight - 1) * gemSpacing);
                        float startX = -totalWidth / 2 + gemSize / 2;
                        float startY = totalHeight / 2 - gemSize / 2;

                        Vector2 position = new Vector2(
                            startX + (x * (gemSize + gemSpacing)),
                            startY - (y * (gemSize + gemSpacing))
                        );

                        CreateGemAt(x, y, position);
                    }
                }
            }
        }

        private void CreateGemAt(int x, int y, Vector2 position)
        {
            GameObject gemObj = Instantiate(gemPrefab, gridContainer);
            RectTransform rectTransform = gemObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;

            Gem gem = gemObj.GetComponent<Gem>();
            int colorIndex = Random.Range(0, gemColors.Length);
            gem.SetColor(gemColors[colorIndex], colorIndex);
            gem.SetPosition(x, y);
            gem.OnSwipe(HandleGemSwipe);

            match3Grid.SetGem(x, y, gem);
        }

        private void HandleGemSwipe(Gem gem, Vector2Int direction)
        {
            if (isProcessing || remainingMoves <= 0) return;

            int targetX = gem.X + direction.x;
            int targetY = gem.Y + direction.y;

            if (!match3Grid.IsValidPosition(targetX, targetY)) return;

            Gem targetGem = match3Grid.GetGem(targetX, targetY);
            if (targetGem == null) return;

            StartCoroutine(SwapAndMatchRoutine(gem, targetGem));
        }

        private IEnumerator SwapAndMatchRoutine(Gem gem1, Gem gem2)
        {
            isProcessing = true;

            Vector2 pos1 = gem1.GetComponent<RectTransform>().anchoredPosition;
            Vector2 pos2 = gem2.GetComponent<RectTransform>().anchoredPosition;

            StartCoroutine(MoveGemTo(gem1, pos1, pos2));
            StartCoroutine(MoveGemTo(gem2, pos2, pos1));

            yield return new WaitForSeconds(0.3f);

            match3Grid.SwapGems(gem1, gem2);
            List<Gem> matches = match3Grid.FindMatchesForGems(gem1, gem2);
            remainingMoves--;

            if (matches.Count > 0)
            {
                foreach (Gem gem in matches)
                {
                    match3Grid.RemoveGem(gem);
                    Destroy(gem.gameObject);
                }

                currentScore += matches.Count * pointsPerGem;

                yield return new WaitForSeconds(0.2f);

                FillGrid();
            }
            else
            {
                yield return new WaitForSeconds(0.2f);

                StartCoroutine(MoveGemTo(gem1, pos2, pos1));
                StartCoroutine(MoveGemTo(gem2, pos1, pos2));

                yield return new WaitForSeconds(0.3f);

                match3Grid.SwapGems(gem1, gem2);
            }

            UpdateUI();

            if (remainingMoves <= 0)
            {
                ShowGameOver();
            }

            isProcessing = false;
        }

        private IEnumerator MoveGemTo(Gem gem, Vector2 startPos, Vector2 targetPos)
        {
            if (gem == null) yield break;

            RectTransform rectTransform = gem.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (gem == null || rectTransform == null) yield break;

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            if (gem != null && rectTransform != null)
                rectTransform.anchoredPosition = targetPos;
        }

        private void UpdateUI()
        {
            if (movesText != null)
                movesText.text = $"Moves: {remainingMoves}";

            if (scoreText != null)
                scoreText.text = $"Score: {currentScore}";

            if (targetText != null)
                targetText.text = $"Target: {targetScore}";
        }

        private void ShowGameOver()
        {
            bool isWin = currentScore >= targetScore;
            int savedHighScore = SaveManager.Instance.GetGameHighScore(GameNames.MATCH3);

            if (currentScore > savedHighScore)
            {
                SaveManager.Instance.UpdateGameHighScore(GameNames.MATCH3, currentScore);
                savedHighScore = currentScore;
            }

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (resultText != null)
                resultText.text = isWin ? "You Win!" : "Game Over!";

            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {currentScore}";

            if (highScoreText != null)
                highScoreText.text = $"Best: {savedHighScore}";
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
    }
}