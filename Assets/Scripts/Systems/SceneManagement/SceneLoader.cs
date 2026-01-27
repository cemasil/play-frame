using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniGameFramework.Core;
using MiniGameFramework.Systems.Events;

namespace MiniGameFramework.Systems.SceneManagement
{
    /// <summary>
    /// Handles scene loading with progress tracking and events
    /// </summary>
    public class SceneLoader : PersistentSingleton<SceneLoader>
    {
        private bool _isLoading = false;

        public bool IsLoading => _isLoading;

        public void LoadScene(string sceneName, Action onComplete = null)
        {
            if (_isLoading)
            {
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }

        private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
        {
            _isLoading = true;

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(GameEvents.SceneLoadStarted);

            yield return new WaitForSeconds(0.1f);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

                if (EventManager.HasInstance)
                    EventManager.Instance.TriggerEvent(GameEvents.SceneLoadProgress, progress);

                if (asyncOperation.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(0.5f);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            _isLoading = false;

            if (EventManager.HasInstance)
                EventManager.Instance.TriggerEvent(GameEvents.SceneLoadCompleted);

            onComplete?.Invoke();
        }
    }
}
