using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.Core.Pooling;
using PlayFrame.Systems.Audio;
using PlayFrame.Systems.SceneManagement;
using PlayFrame.Systems.SaveSystem;
using PlayFrame.Systems.Localization;
using PlayFrame.Systems.Analytics;
using PlayFrame.MiniGames.Common;

namespace PlayFrame.MiniGames.Memory
{
    /// <summary>
    /// Main Memory game controller with analytics integration
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

        [Header("Audio")]
        [SerializeField] private SFXCollection sfxCollection;
        [SerializeField] private MusicCollection musicCollection;
        [SerializeField] private AudioClip flipSound;
        [SerializeField] private AudioClip matchSound;
        [SerializeField] private AudioClip mismatchSound;
        [SerializeField] private AudioClip gameMusic;

        private ObjectPool<MemoryCard> cardPool;
        private List<MemoryCard> cards = new List<MemoryCard>();
        private MemoryCard firstCard = null;
        private MemoryCard secondCard = null;

        private int moves = 0;
        private int matchedPairs = 0;
        private int totalPairs;
        private int currentScore = 0;
        private float gameTime = 0f;

        #region Analytics Override Properties

        protected override string GameName => GameNames.MEMORY;
        protected override string Difficulty => $"{gridRows}x{gridColumns}";

        #endregion

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
        }

        protected override void OnGameStart()
        {
            PlayGameMusic();
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
            if (IsPlaying)
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
            if (!CanAcceptInput || clickedCard.IsRevealed || clickedCard.IsMatched)
                return;

            clickedCard.Reveal();
            PlayFlipSound();

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
            BeginProcessing();
            SetAllCardsInteractable(false);

            yield return new WaitForSeconds(cardRevealDuration);

            if (firstCard.CardId == secondCard.CardId)
            {
                PlayMatchSound();
                firstCard.SetMatched();
                secondCard.SetMatched();
                matchedPairs++;

                currentScore += pointsPerMatch;
                UpdateUI();

                if (matchedPairs >= totalPairs)
                {
                    yield return new WaitForSeconds(0.5f);
                    EndGame();
                    ShowGameOver();
                }
                else
                {
                    EndProcessing();
                }
            }
            else
            {
                PlayMismatchSound();
                yield return new WaitForSeconds(mismatchHideDuration);
                firstCard.Hide();
                secondCard.Hide();
                EndProcessing();
            }

            firstCard = null;
            secondCard = null;
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
                movesText.text = LocalizationManager.Get(LocalizationKeys.MOVES, moves);

            if (scoreText != null)
                scoreText.text = LocalizationManager.Get(LocalizationKeys.SCORE, currentScore);
        }

        private void UpdateTimeDisplay()
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timeText.text = LocalizationManager.Get(LocalizationKeys.TIME, $"{minutes}:{seconds:00}");
            }
        }

        private void ShowGameOver()
        {
            int totalSeconds = Mathf.FloorToInt(gameTime);
            int savedBestTime = SaveManager.Instance.GetGameHighScore(GameNames.MEMORY);

            bool isNewHighScore = (savedBestTime == 0 || totalSeconds < savedBestTime);

            if (isNewHighScore)
            {
                // Track high score before updating
                TrackHighScore(totalSeconds, savedBestTime);
                SaveManager.Instance.UpdateGameHighScore(GameNames.MEMORY, totalSeconds);
                savedBestTime = totalSeconds;
            }

            // Track level completion with analytics
            TrackLevelCompleted(
                score: currentScore,
                moveCount: moves,
                isNewHighScore: isNewHighScore,
                stars: CalculateStars(),
                matchCount: matchedPairs
            );

            // Play win sound
            PlayWinSound();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (resultText != null)
                resultText.text = LocalizationManager.Get(LocalizationKeys.YOU_WIN);

            if (finalTimeText != null)
            {
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                finalTimeText.text = LocalizationManager.Get(LocalizationKeys.TIME, $"{minutes}:{seconds:00}");
            }

            if (finalMovesText != null)
                finalMovesText.text = LocalizationManager.Get(LocalizationKeys.MOVES, moves);

            if (highScoreText != null)
            {
                int bestMinutes = savedBestTime / 60;
                int bestSeconds = savedBestTime % 60;
                highScoreText.text = LocalizationManager.Get(LocalizationKeys.BEST_TIME, $"{bestMinutes}:{bestSeconds:00}");
            }
        }

        /// <summary>
        /// Calculate stars based on performance (moves vs optimal)
        /// </summary>
        private int CalculateStars()
        {
            // Optimal moves = total pairs (one move per pair if perfect memory)
            int optimalMoves = totalPairs;
            float moveRatio = (float)moves / optimalMoves;

            if (moveRatio <= 1.5f) return 3; // Excellent
            if (moveRatio <= 2.5f) return 2; // Good
            return 1; // Completed
        }

        #region Audio Methods

        private void PlayGameMusic()
        {
            if (gameMusic != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlayMusic(gameMusic);
            }
            else if (musicCollection != null)
            {
                musicCollection.PlayRandomGameTrack();
            }
        }

        private void PlayFlipSound()
        {
            if (flipSound != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(flipSound);
            }
            else if (sfxCollection != null)
            {
                sfxCollection.Play(sfxCollection.swap);
            }
        }

        private void PlayMatchSound()
        {
            if (matchSound != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(matchSound);
            }
            else if (sfxCollection != null)
            {
                sfxCollection.PlayMatch();
            }
        }

        private void PlayMismatchSound()
        {
            if (mismatchSound != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(mismatchSound);
            }
            else if (sfxCollection != null)
            {
                sfxCollection.PlayMismatch();
            }
        }

        private void PlayWinSound()
        {
            if (sfxCollection != null)
            {
                sfxCollection.PlayWin();
            }
            else if (musicCollection != null)
            {
                musicCollection.PlayVictory();
            }
        }

        #endregion

        private void OnBackClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.GAME_SELECTION);
        }

        private void OnRestartClicked()
        {
            // Track retry before reloading
            TrackLevelRetried();
            SceneLoader.Instance.LoadScene(SceneNames.MEMORY);
        }

        private void OnMenuClicked()
        {
            SceneLoader.Instance.LoadScene(SceneNames.MAIN_MENU);
        }
    }
}
