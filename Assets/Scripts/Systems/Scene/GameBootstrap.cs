using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PlayFrame.Core.Events;
using PlayFrame.Core.Logging;
using PlayFrame.Systems.Save;
using PlayFrame.Systems.Audio;
using PlayFrame.Systems.Input;
using PlayFrame.Systems.Localization;
using PlayFrame.Systems.Analytics;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Scene
{
    /// <summary>
    /// Bootstrap scene controller - initializes all managers and services.
    /// This should be the first scene in Build Settings (index 0).
    /// For UI initialization, add UIBootstrap component separately.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minimumSplashDuration = 2f;
        [SerializeField] private bool showSplashScreen = true;

        [Header("Managers (Optional - will auto-create if not assigned)")]
        private GameObject eventManager;
        [SerializeField] private GameObject saveManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject sceneLoaderPrefab;
        [SerializeField] private GameObject inputManagerPrefab;
        [SerializeField] private GameObject localizationManagerPrefab;
        [SerializeField] private GameObject analyticsManagerPrefab;

        [Header("Next Scene")]
        [SerializeField] private string nextSceneName = SceneNames.MAIN_MENU;

        private float _startTime;
        private bool _isInitialized;
        private ILogger _logger;

        private void Start()
        {
            _logger = LoggerFactory.CreateScene("Bootstrap");
            _startTime = Time.time;
            InitializeGameAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid InitializeGameAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.Log("Starting game initialization...");

                await InitializeManagersAsync(cancellationToken);

                await LoadSavedDataAsync(cancellationToken);

                await InitializeAudioAsync(cancellationToken);

                await AdditionalInitialization(cancellationToken);

                _isInitialized = true;
                _logger.Log("Game initialization complete!");

                float elapsed = Time.time - _startTime;
                if (elapsed < minimumSplashDuration)
                {
                    await UniTask.WaitForSeconds(minimumSplashDuration - elapsed, cancellationToken: cancellationToken);
                }

                LoadNextScene();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bootstrap initialization failed: {ex}");
            }
        }

        private async UniTask InitializeManagersAsync(CancellationToken cancellationToken)
        {
            _logger.Log("Initializing managers...");

            EnsureManagerFromPrefab<EventManager>(eventManager);
            _ = EventManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            EnsureManagerFromPrefab<SaveManager>(saveManagerPrefab);
            _ = SaveManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            EnsureManagerFromPrefab<AudioManager>(audioManagerPrefab);
            _ = AudioManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            EnsureManagerFromPrefab<InputManager>(inputManagerPrefab);
            _ = InputManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            EnsureManagerFromPrefab<LocalizationManager>(localizationManagerPrefab);
            _ = LocalizationManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            EnsureManagerFromPrefab<AnalyticsManager>(analyticsManagerPrefab);
            _ = AnalyticsManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

            EnsureManagerFromPrefab<SceneLoaderManager>(sceneLoaderPrefab);
            _ = SceneLoaderManager.Instance;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        private void EnsureManagerFromPrefab<T>(GameObject prefab) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() != null)
            {
                _logger.Log($"{typeof(T).Name} already exists.");
                return;
            }

            if (prefab != null)
            {
                Instantiate(prefab);
                _logger.Log($"{typeof(T).Name} instantiated from prefab.");
            }
        }

        private async UniTask LoadSavedDataAsync(CancellationToken cancellationToken)
        {
            _logger.Log("Loading saved data...");

            var saveManager = SaveManager.Instance;
            saveManager.LoadGame();

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        private async UniTask InitializeAudioAsync(CancellationToken cancellationToken)
        {
            _logger.Log("Initializing audio...");

            var audioManager = AudioManager.Instance;

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        /// <summary>
        /// Override this in a derived class for game-specific initialization
        /// </summary>
        protected virtual UniTask AdditionalInitialization(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        private void LoadNextScene()
        {
            if (SceneLoaderManager.HasInstance)
            {
                SceneLoaderManager.Instance.LoadScene(nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
