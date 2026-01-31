using System.Collections;
using UnityEngine;
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
            StartCoroutine(InitializeGame());
        }

        private IEnumerator InitializeGame()
        {
            _logger.Log("Starting game initialization...");

            yield return InitializeManagers();

            yield return LoadSavedData();

            yield return InitializeAudio();

            yield return AdditionalInitialization();

            _isInitialized = true;
            _logger.Log("Game initialization complete!");

            float elapsed = Time.time - _startTime;
            if (elapsed < minimumSplashDuration)
            {
                yield return new WaitForSeconds(minimumSplashDuration - elapsed);
            }

            LoadNextScene();
        }

        private IEnumerator InitializeManagers()
        {
            _logger.Log("Initializing managers...");

            EnsureManagerFromPrefab<EventManager>(eventManager);
            _ = EventManager.Instance;
            yield return null;

            EnsureManagerFromPrefab<SaveManager>(saveManagerPrefab);
            _ = SaveManager.Instance;
            yield return null;

            EnsureManagerFromPrefab<AudioManager>(audioManagerPrefab);
            _ = AudioManager.Instance;
            yield return null;

            EnsureManagerFromPrefab<InputManager>(inputManagerPrefab);
            _ = InputManager.Instance;
            yield return null;

            EnsureManagerFromPrefab<LocalizationManager>(localizationManagerPrefab);
            _ = LocalizationManager.Instance;
            yield return null;

            EnsureManagerFromPrefab<AnalyticsManager>(analyticsManagerPrefab);
            _ = AnalyticsManager.Instance;
            yield return null;

            EnsureManagerFromPrefab<SceneLoaderManager>(sceneLoaderPrefab);
            _ = SceneLoaderManager.Instance;
            yield return null;
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

        private IEnumerator LoadSavedData()
        {
            _logger.Log("Loading saved data...");

            var saveManager = SaveManager.Instance;
            saveManager.LoadGame();

            yield return null;
        }

        private IEnumerator InitializeAudio()
        {
            _logger.Log("Initializing audio...");

            var audioManager = AudioManager.Instance;

            yield return null;
        }

        /// <summary>
        /// Override this in a derived class for game-specific initialization
        /// </summary>
        protected virtual IEnumerator AdditionalInitialization()
        {
            yield return null;
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
