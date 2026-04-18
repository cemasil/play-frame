using UnityEngine;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Scene
{
    /// <summary>
    /// Controls the loading scene flow.
    /// Place on a GameObject in any loading scene.
    /// On Start(), reads the pending target scene from SceneLoaderManager
    /// and triggers the async load with progress events.
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("Fallback")]
        [Tooltip("Scene to load if no pending target is set (e.g. opened loading scene directly)")]
        [SerializeField] private string fallbackScene = SceneNames.MAIN_MENU;

        private ILogger _logger;

        private void Start()
        {
            _logger = LoggerFactory.CreateScene("LoadingSceneController");

            if (!SceneLoaderManager.HasInstance)
            {
                _logger.LogError("SceneLoaderManager not found. Cannot load target scene.");
                return;
            }

            var targetScene = SceneLoaderManager.Instance.PendingTargetScene;

            if (string.IsNullOrEmpty(targetScene))
            {
                _logger.LogWarning($"No pending target scene. Loading fallback: {fallbackScene}");
                targetScene = fallbackScene;
            }

            SceneLoaderManager.Instance.LoadPendingScene();
        }
    }
}
