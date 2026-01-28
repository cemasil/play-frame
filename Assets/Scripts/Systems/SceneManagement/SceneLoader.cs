using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using PlayFrame.Core;
using PlayFrame.Systems.Events;

namespace PlayFrame.Systems.SceneManagement
{
    /// <summary>
    /// Handles scene loading with progress tracking and events.
    /// Uses SceneLoadingSettings for configurable timing parameters.
    /// </summary>
    public class SceneLoader : PersistentSingleton<SceneLoader>
    {
        [Header("Settings")]
        [SerializeField] private SceneLoadingSettings settings;

        private bool _isLoading;
        private float _displayProgress;
        private float _loadStartTime;

        public bool IsLoading => _isLoading;
        public float CurrentProgress => _displayProgress;

        private SceneLoadingSettings Settings => settings != null ? settings : SceneLoadingSettings.Default;

        public void LoadScene(string sceneName, Action onComplete = null)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading a scene. Ignoring request for: {sceneName}");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
        {
            _isLoading = true;
            _displayProgress = 0f;
            _loadStartTime = Time.unscaledTime;

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(GameEvents.SceneLoadStarted);

            // Pre-load delay (allows UI to show loading screen)
            if (Settings.PreLoadDelay > 0f)
            {
                yield return Settings.PreLoadWait;
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
                    EventManager.Instance.TriggerEvent(GameEvents.SceneLoadProgress, _displayProgress);

                // Check if load is complete
                if (Settings.IsLoadComplete(asyncOperation.progress))
                {
                    // Ensure minimum load duration
                    float elapsed = Time.unscaledTime - _loadStartTime;
                    float remainingMinDuration = Settings.MinimumLoadDuration - elapsed;

                    // Note: remainingMinDuration is dynamic, cannot cache
                    if (remainingMinDuration > 0f)
                    {
                        yield return new WaitForSecondsRealtime(remainingMinDuration);
                    }

                    // Post-load delay (allows progress bar to reach 100%)
                    if (Settings.PostLoadDelay > 0f)
                    {
                        yield return Settings.PostLoadWait;
                    }

                    // Ensure progress shows 100%
                    _displayProgress = 1f;
                    if (EventManager.HasInstance)
                        EventManager.Instance.TriggerEvent(GameEvents.SceneLoadProgress, 1f);

                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            _isLoading = false;

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(GameEvents.SceneLoadCompleted);

            onComplete?.Invoke();
        }

        /// <summary>
        /// Reload the current active scene
        /// </summary>
        public void ReloadCurrentScene(Action onComplete = null)
        {
            LoadScene(SceneManager.GetActiveScene().name, onComplete);
        }
    }
}
