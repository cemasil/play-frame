using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Scene
{
    /// <summary>
    /// Handles scene loading with progress tracking and events.
    /// Uses SceneLoadingSettings for configurable timing parameters.
    /// </summary>
    public class SceneLoaderManager : PersistentSingleton<SceneLoaderManager>
    {
        [Header("Settings")]
        [Tooltip("Optional: Assign settings directly. If null, uses default settings")]
        [SerializeField] private SceneLoadingSettings settings;

        [Header("Loading Screen")]
        [Tooltip("Default loading scene used when none is specified")]
        [SerializeField] private string defaultLoadingScene = SceneNames.LOADING;

        private bool _isLoading;
        private float _displayProgress;
        private float _loadStartTime;
        private ILogger _logger;
        private string _pendingTargetScene;
        private Action _pendingOnComplete;

        public bool IsLoading => _isLoading;
        public float CurrentProgress => _displayProgress;

        /// <summary>
        /// The scene that a loading scene should load next.
        /// Set by LoadSceneWithLoading, read by LoadingSceneController.
        /// </summary>
        public string PendingTargetScene => _pendingTargetScene;

        /// <summary>
        /// Current scene loading settings
        /// </summary>
        public SceneLoadingSettings Settings => settings;

        protected override void OnSingletonAwake()
        {
            _logger = LoggerFactory.CreateScene("SceneLoader");
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (settings == null)
            {
                settings = SceneLoadingSettings.CreateDefault();
            }
        }

        public void LoadScene(string sceneName, Action onComplete = null)
        {
            if (_isLoading)
            {
                _logger.LogWarning($"Already loading a scene. Ignoring request for: {sceneName}");
                return;
            }

            LoadSceneAsync(sceneName, onComplete, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid LoadSceneAsync(string sceneName, Action onComplete, CancellationToken cancellationToken)
        {
            try
            {
                _isLoading = true;
                _displayProgress = 0f;
                _loadStartTime = Time.unscaledTime;

                if (EventManager.HasInstance)
                    EventManager.Instance.TriggerEvent(CoreEvents.SceneLoadStarted);

                // Pre-load delay (allows UI to show loading screen)
                if (Settings.PreLoadDelay > 0f)
                {
                    await UniTask.WaitForSeconds(Settings.PreLoadDelay, true, cancellationToken: cancellationToken);
                }

                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
                asyncOperation.allowSceneActivation = false;

                float targetProgress = 0f;

                while (!asyncOperation.isDone)
                {
                    // Calculate normalized progress
                    targetProgress = Settings.NormalizeProgress(asyncOperation.progress);

                    // Smooth or instant progress update
                    if (Settings.SmoothProgress)
                    {
                        _displayProgress = Mathf.MoveTowards(
                            _displayProgress,
                            targetProgress,
                            Settings.ProgressSmoothSpeed * Time.unscaledDeltaTime
                        );
                    }
                    else
                    {
                        _displayProgress = targetProgress;
                    }

                    // Broadcast progress
                    if (EventManager.HasInstance)
                        EventManager.Instance.TriggerEvent(CoreEvents.SceneLoadProgress, _displayProgress);

                    // Check if load is complete
                    if (Settings.IsLoadComplete(asyncOperation.progress))
                    {
                        // Ensure minimum load duration
                        float elapsed = Time.unscaledTime - _loadStartTime;
                        float remainingMinDuration = Settings.MinimumLoadDuration - elapsed;

                        if (remainingMinDuration > 0f)
                        {
                            await UniTask.WaitForSeconds(remainingMinDuration, true, cancellationToken: cancellationToken);
                        }

                        // Post-load delay (allows progress bar to reach 100%)
                        if (Settings.PostLoadDelay > 0f)
                        {
                            await UniTask.WaitForSeconds(Settings.PostLoadDelay, true, cancellationToken: cancellationToken);
                        }

                        // Ensure progress shows 100%
                        _displayProgress = 1f;
                        if (EventManager.HasInstance)
                            EventManager.Instance.TriggerEvent(CoreEvents.SceneLoadProgress, 1f);

                        asyncOperation.allowSceneActivation = true;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                if (EventManager.HasInstance)
                    EventManager.Instance.TriggerEvent(CoreEvents.SceneLoadCompleted);

                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError($"Scene load failed for '{sceneName}': {ex}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        /// <summary>
        /// Reload the current active scene
        /// </summary>
        public void ReloadCurrentScene(Action onComplete = null)
        {
            LoadScene(SceneManager.GetActiveScene().name, onComplete);
        }

        /// <summary>
        /// Load a scene through a loading scene.
        /// The loading scene should have a LoadingSceneController that calls LoadPendingScene().
        /// </summary>
        /// <param name="targetScene">The final destination scene</param>
        /// <param name="loadingScene">Loading scene to show during transition. If null, uses defaultLoadingScene.</param>
        /// <param name="onComplete">Called after the target scene is fully loaded</param>
        public void LoadSceneWithLoading(string targetScene, string loadingScene = null, Action onComplete = null)
        {
            if (_isLoading)
            {
                _logger.LogWarning($"Already loading a scene. Ignoring request for: {targetScene}");
                return;
            }

            _pendingTargetScene = targetScene;
            _pendingOnComplete = onComplete;

            var loadScene = string.IsNullOrEmpty(loadingScene) ? defaultLoadingScene : loadingScene;
            _logger.Log($"Loading '{targetScene}' via loading scene '{loadScene}'");

            SceneManager.LoadScene(loadScene);
        }

        /// <summary>
        /// Called by LoadingSceneController to start loading the pending target scene.
        /// </summary>
        public void LoadPendingScene()
        {
            if (string.IsNullOrEmpty(_pendingTargetScene))
            {
                _logger.LogError("No pending target scene. Use LoadSceneWithLoading() first.");
                return;
            }

            var target = _pendingTargetScene;
            var callback = _pendingOnComplete;
            _pendingTargetScene = null;
            _pendingOnComplete = null;

            LoadScene(target, callback);
        }
    }
}
