using UnityEngine;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.UI
{
    /// <summary>
    /// UI Bootstrap - Initializes UI settings and factory.
    /// Add this component to the same GameObject as GameBootstrap.
    /// </summary>
    public class UIBootstrap : MonoBehaviour
    {
        [Header("UI Settings")]
        [Tooltip("Optional: Assign settings directly. If null, uses default settings")]
        [SerializeField] private UIPrefabSettings uiPrefabSettings;

        private ILogger _logger;

        private void Awake()
        {
            _logger = LoggerFactory.CreateUI("UIBootstrap");
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (uiPrefabSettings != null)
            {
                UIPrefabFactory.SetSettings(uiPrefabSettings);
                _logger.Log("UI Prefab Settings initialized");
            }
            else
            {
                // Create default settings if not assigned
                var defaultSettings = UIPrefabSettings.CreateDefault();
                UIPrefabFactory.SetSettings(defaultSettings);
            }
        }
    }
}
