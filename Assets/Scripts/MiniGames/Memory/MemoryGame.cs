using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Pooling;
using MiniGameFramework.Systems.SceneManagement;
using MiniGameFramework.Systems.SaveSystem;
using MiniGameFramework.MiniGames.Common;

namespace MiniGameFramework.MiniGames.Memory
{
    /// <summary>
    /// Main Memory game controller
    /// </summary>
    public class MemoryGame : BaseGame
    {
        [Header("Grid Settings")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private int gridRows = 4;
        [SerializeField] private int gridColumns = 4;

        [Header("Prefabs")]
        [SerializeField] private GameObject cardPrefab;

        [Header("Colors")]
        [SerializeField] private Color[] cardColors = new Color[8];

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button backButton;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI finalTimeText;
        [SerializeField] private TextMeshProUGUI finalMovesText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Game Settings")]
        [SerializeField] private float cardRevealDuration = 0.5f;
        [SerializeField] private float mismatchHideDuration = 1f;
        [SerializeField] private int pointsPerMatch = 100;

        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 16;
        [SerializeField] private int maxPoolSize = 64;

        private ObjectPool<MemoryCard> cardPool;
        private List<MemoryCard> cards = new List<MemoryCard>();
        private MemoryCard firstCard = null;
        private MemoryCard secondCard = null;
        private bool isProcessing = false;

        private int moves = 0;
        private int matchedPairs = 0;
        private int totalPairs;
        private int currentScore = 0;
        private float gameTime = 0f;
        private bool isGameActive = false;

        protected override void OnInitialize()
        {
            InitializePool();

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            totalPairs = gridRows * gridColumns / 2;
            CreateGrid();
            UpdateUI();
            isGameActive = true;
        }

        private void InitializePool()
        {
            if (cardPrefab == null)
            {
                Debug.LogError("[MemoryGame] Card prefab is not assigned!");
                return;
            }

            var cardComponent = cardPrefab.GetComponent<MemoryCard>();
            if (cardComponent == null)
            {
                Debug.LogError("[MemoryGame] Card prefab must have a MemoryCard component!");
                return;
            }

            cardPool = new ObjectPool<MemoryCard>(
                cardComponent,
                gridContainer,
                initialPoolSize,
                maxPoolSize,
                onCreate: OnCardCreated,
                onGet: OnCardGet,
                onRelease: OnCardRelease
            );
        }

        private void OnCardCreated(MemoryCard card)
        {
            // Called when a new card is instantiated
        }

        private void OnCardGet(MemoryCard card)
        {
            card.gameObject.SetActive(true);
        }

        private void OnCardRelease(MemoryCard card)
        {
            card.ResetPiece();
            card.gameObject.SetActive(false);
        }

        protected override void Cleanup()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (menuButton != null)
                menuButton.onClick.RemoveListener(OnMenuClicked);

            // Clear all pooled cards
            cards.Clear();
            cardPool?.Clear();
        }

        protected override void OnUpdate()
        {
            if (isGameActive)
            {
                gameTime += Time.deltaTime;
                UpdateTimeDisplay();
            }
        }

        private void CreateGrid()
        {
            if (cardColors.Length < totalPairs)
            {
                Debug.LogError($"Not enough card colors! Need {totalPairs}, have {cardColors.Length}");
                return;
            }

            List<int> cardIds = new List<int>();
            for (int i = 0; i < totalPairs; i++)
            {
                cardIds.Add(i);
                cardIds.Add(i);
            }

            ShuffleList(cardIds);

            for (int i = 0; i < cardIds.Count; i++)
            {
                MemoryCard card = cardPool.Get();

                if (card != null)
                {
                    int cardId = cardIds[i];
                    Color cardColor = cardColors[cardId];
                    card.Setup(cardId, cardColor, OnCardClicked);
                    cards.Add(card);
                }
            }
        }

        private void OnCardClicked(MemoryCard clickedCard)
        {
            if (isProcessing || clickedCard.IsRevealed || clickedCard.IsMatched)
                return;

            clickedCard.Reveal();

            if (firstCard == null)
            {
                firstCard = clickedCard;
            }
            else if (secondCard == null)
            {
                secondCard = clickedCard;
                moves++;
                UpdateUI();
                StartCoroutine(CheckMatchRoutine());
            }
        }

        private IEnumerator CheckMatchRoutine()
        {
            isProcessing = true;
            SetAllCardsInteractable(false);

            yield return new WaitForSeconds(cardRevealDuration);

            if (firstCard.CardId == secondCard.CardId)
            {
                firstCard.SetMatched();
                secondCard.SetMatched();
                matchedPairs++;

                currentScore += pointsPerMatch;
                UpdateUI();

                if (matchedPairs >= totalPairs)
                {
                    isGameActive = false;
                    yield return new WaitForSeconds(0.5f);
                    ShowGameOver();
                }
            }
            else
            {
                yield return new WaitForSeconds(mismatchHideDuration);
                firstCard.Hide();
                secondCard.Hide();
            }

            firstCard = null;
            secondCard = null;
            isProcessing = false;
            SetAllCardsInteractable(true);
        }

        private void SetAllCardsInteractable(bool interactable)
        {
            foreach (MemoryCard card in cards)
            {
                card.SetInteractable(interactable);
            }
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        private void UpdateUI()
        {
            if (movesText != null)
                movesText.text = $"Moves: {moves}";

            if (scoreText != null)
                scoreText.text = $"Score: {currentScore}";
        }

        private void UpdateTimeDisplay()
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timeText.text = $"Time: {minutes}:{seconds:00}";
            }
        }

        private void ShowGameOver()
        {
            int totalSeconds = Mathf.FloorToInt(gameTime);
            int savedBestTime = SaveManager.Instance.GetGameHighScore(GameNames.MEMORY);

            if (savedBestTime == 0 || totalSeconds < savedBestTime)
            {
                SaveManager.Instance.UpdateGameHighScore(GameNames.MEMORY, totalSeconds);
                savedBestTime = totalSeconds;
            }

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (resultText != null)
                resultText.text = "You Win!";

            if (finalTimeText != null)
            {
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                finalTimeText.text = $"Time: {minutes}:{seconds:00}";
            }

            if (finalMovesText != null)
                finalMovesText.text = $"Moves: {moves}";

            if (highScoreText != null)
            {
                int bestMinutes = savedBestTime / 60;
                int bestSeconds = savedBestTime % 60;
                highScoreText.text = $"Best Time: {bestMinutes}:{bestSeconds:00}";
            }
        }

        private void OnBackClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.GAME_SELECTION);
        }

        private void OnRestartClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.MEMORY);
        }

        private void OnMenuClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.MAIN_MENU);
        }
    }
}
