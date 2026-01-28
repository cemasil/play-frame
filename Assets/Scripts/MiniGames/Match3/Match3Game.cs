using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Pooling;
using MiniGameFramework.Systems.Audio;
using MiniGameFramework.Systems.SceneManagement;
using MiniGameFramework.Systems.SaveSystem;
using MiniGameFramework.Systems.Localization;
using MiniGameFramework.MiniGames.Common;

namespace MiniGameFramework.MiniGames.Match3
{
    /// <summary>
    /// Main Match3 game controller
    /// </summary>
    public class Match3Game : BaseGame
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;
        [SerializeField] private float gemSize = 90f;
        [SerializeField] private float gemSpacing = 10f;

        [Header("Prefabs")]
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private Transform gridContainer;

        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 36;
        [SerializeField] private int maxPoolSize = 100;

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

        [Header("Audio")]
        [SerializeField] private SFXCollection sfxCollection;
        [SerializeField] private MusicCollection musicCollection;
        [SerializeField] private AudioClip swapSound;
        [SerializeField] private AudioClip matchSound;
        [SerializeField] private AudioClip noMatchSound;
        [SerializeField] private AudioClip gameMusic;

        private Match3Grid match3Grid;
        private ObjectPool<Gem> gemPool;
        private int currentScore = 0;
        private int remainingMoves;

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

            match3Grid = new Match3Grid(gridWidth, gridHeight);
            remainingMoves = maxMoves;

            FillGrid();
            UpdateUI();
        }

        protected override void OnGameStart()
        {
            PlayGameMusic();
        }

        private void InitializePool()
        {
            if (gemPrefab == null)
            {
                Debug.LogError("[Match3Game] Gem prefab is not assigned!");
                return;
            }

            var gemComponent = gemPrefab.GetComponent<Gem>();
            if (gemComponent == null)
            {
                Debug.LogError("[Match3Game] Gem prefab must have a Gem component!");
                return;
            }

            gemPool = new ObjectPool<Gem>(
                gemComponent,
                gridContainer,
                initialPoolSize,
                maxPoolSize,
                onCreate: OnGemCreated,
                onGet: OnGemGet,
                onRelease: OnGemRelease
            );
        }

        private void OnGemCreated(Gem gem)
        {
            // Called when a new gem is instantiated
        }

        private void OnGemGet(Gem gem)
        {
            gem.gameObject.SetActive(true);
        }

        private void OnGemRelease(Gem gem)
        {
            gem.ResetPiece();
            gem.gameObject.SetActive(false);
        }

        protected override void Cleanup()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (menuButton != null)
                menuButton.onClick.RemoveListener(OnMenuClicked);

            // Clear all pooled gems
            gemPool?.Clear();
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
            Gem gem = gemPool.Get();
            RectTransform rectTransform = gem.RectTransform;
            rectTransform.anchoredPosition = position;

            int colorIndex = Random.Range(0, gemColors.Length);
            gem.SetColor(gemColors[colorIndex], colorIndex);
            gem.SetPosition(x, y);
            gem.OnSwipeCallback(HandleGemSwipe);

            match3Grid.SetGem(x, y, gem);
        }

        private void HandleGemSwipe(Gem gem, Vector2Int direction)
        {
            if (!CanAcceptInput || remainingMoves <= 0) return;

            int targetX = gem.X + direction.x;
            int targetY = gem.Y + direction.y;

            if (!match3Grid.IsValidPosition(targetX, targetY)) return;

            Gem targetGem = match3Grid.GetGem(targetX, targetY);
            if (targetGem == null) return;

            StartCoroutine(SwapAndMatchRoutine(gem, targetGem));
        }

        private IEnumerator SwapAndMatchRoutine(Gem gem1, Gem gem2)
        {
            BeginProcessing();
            PlaySwapSound();

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
                PlayMatchSound();

                foreach (Gem gem in matches)
                {
                    match3Grid.RemoveGem(gem);
                    gemPool.Release(gem);
                }

                currentScore += matches.Count * pointsPerGem;

                yield return new WaitForSeconds(0.2f);

                FillGrid();
            }
            else
            {
                PlayNoMatchSound();
                yield return new WaitForSeconds(0.2f);

                StartCoroutine(MoveGemTo(gem1, pos2, pos1));
                StartCoroutine(MoveGemTo(gem2, pos1, pos2));

                yield return new WaitForSeconds(0.3f);

                match3Grid.SwapGems(gem1, gem2);
            }

            UpdateUI();

            if (remainingMoves <= 0)
            {
                EndGame();
                ShowGameOver();
            }
            else
            {
                EndProcessing();
            }
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
                movesText.text = LocalizationManager.Get(LocalizationKeys.MOVES, remainingMoves);

            if (scoreText != null)
                scoreText.text = LocalizationManager.Get(LocalizationKeys.SCORE, currentScore);

            if (targetText != null)
                targetText.text = LocalizationManager.Get("ui.target", targetScore);
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

            // Play win/lose sound
            if (isWin)
                PlayWinSound();
            else
                PlayLoseSound();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (resultText != null)
                resultText.text = isWin
                    ? LocalizationManager.Get(LocalizationKeys.YOU_WIN)
                    : LocalizationManager.Get(LocalizationKeys.GAME_OVER);

            if (finalScoreText != null)
                finalScoreText.text = LocalizationManager.Get(LocalizationKeys.FINAL_SCORE, currentScore);

            if (highScoreText != null)
                highScoreText.text = LocalizationManager.Get(LocalizationKeys.HIGH_SCORE, savedHighScore);
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

        private void PlaySwapSound()
        {
            if (swapSound != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(swapSound);
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

        private void PlayNoMatchSound()
        {
            if (noMatchSound != null && AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(noMatchSound);
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

        private void PlayLoseSound()
        {
            if (sfxCollection != null)
            {
                sfxCollection.PlayLose();
            }
            else if (musicCollection != null)
            {
                musicCollection.PlayDefeat();
            }
        }

        #endregion

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