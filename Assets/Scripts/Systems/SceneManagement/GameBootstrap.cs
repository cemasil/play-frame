using System.Collections;
using UnityEngine;
using PlayFrame.Systems.SceneManagement;
using PlayFrame.Systems.SaveSystem;
using PlayFrame.Systems.Audio;
using PlayFrame.Systems.Events;
using PlayFrame.Systems.Input;
using PlayFrame.Systems.Localization;
using PlayFrame.Systems.Analytics;

namespace PlayFrame.Systems.Bootstrap
{
    /// <summary>
    /// Bootstrap scene controller - initializes all managers and services.
    /// This should be the first scene in Build Settings (index 0).
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minimumSplashDuration = 2f;
        [SerializeField] private bool showSplashScreen = true;

        [Header("Managers (Optional - will auto-create if not assigned)")]
        [SerializeField] private GameObject eventManagerPrefab;
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

        private void Start()
        {
            _startTime = Time.time;
            StartCoroutine(InitializeGame());
        }

        private IEnumerator InitializeGame()
        {
            Debug.Log("[Bootstrap] Starting game initialization...");

            yield return InitializeManagers();

            yield return LoadSavedData();

            yield return InitializeAudio();

            yield return AdditionalInitialization();

            _isInitialized = true;
            Debug.Log("[Bootstrap] Game initialization complete!");

            float elapsed = Time.time - _startTime;
            if (elapsed < minimumSplashDuration)
            {
                yield return new WaitForSeconds(minimumSplashDuration - elapsed);
            }

            LoadNextScene();
        }

        private IEnumerator InitializeManagers()
        {
            Debug.Log("[Bootstrap] Initializing managers...");

            EnsureManagerFromPrefab<EventManager>(eventManagerPrefab);
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

            EnsureManagerFromPrefab<SceneLoader>(sceneLoaderPrefab);
            _ = SceneLoader.Instance;
            yield return null;
        }

        private void EnsureManagerFromPrefab<T>(GameObject prefab) where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() != null)
            {
                Debug.Log($"[Bootstrap] {typeof(T).Name} already exists.");
                return;
            }

            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log($"[Bootstrap] {typeof(T).Name} instantiated from prefab.");
            }
        }

        private IEnumerator LoadSavedData()
        {
            Debug.Log("[Bootstrap] Loading saved data...");

            var saveManager = SaveManager.Instance;
            saveManager.LoadGame();

            yield return null;
        }

        private IEnumerator InitializeAudio()
        {
            Debug.Log("[Bootstrap] Initializing audio...");

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
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadScene(nextSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
