using UnityEngine;

namespace PlayFrame.Core.Logging
{
    /// <summary>
    /// Centralized logging configuration for the entire project.
    /// Create via Assets > Create > PlayFrame > Core > Log Settings
    /// Place in GameSettings folder.
    /// </summary>
    [CreateAssetMenu(fileName = "LogSettings", menuName = "PlayFrame/Core/Log Settings")]
    public class LogSettings : ScriptableObject
    {
        private static LogSettings _instance;

        [Header("Global Settings")]
        [Tooltip("Master switch for all debug logging. When disabled, no debug logs will be output.")]
        [SerializeField] private bool enableAllLogs = true;

        [Tooltip("When enabled, allows per-module log control. When disabled, uses global setting only.")]
        [SerializeField] private bool useModuleOverrides = false;

        [Header("Module Overrides (requires 'Use Module Overrides' enabled)")]
        [Tooltip("Enable logs for Analytics system")]
        [SerializeField] private bool enableAnalyticsLogs = true;

        [Tooltip("Enable logs for Save system")]
        [SerializeField] private bool enableSaveLogs = true;

        [Tooltip("Enable logs for Localization system")]
        [SerializeField] private bool enableLocalizationLogs = true;

        [Tooltip("Enable logs for Scene/Bootstrap system")]
        [SerializeField] private bool enableSceneLogs = true;

        [Tooltip("Enable logs for UI system")]
        [SerializeField] private bool enableUILogs = true;

        [Tooltip("Enable logs for Event system")]
        [SerializeField] private bool enableEventLogs = true;

        [Tooltip("Enable logs for MiniGames")]
        [SerializeField] private bool enableGameLogs = true;

        [Tooltip("Enable logs for Core systems (StateMachine, Pooling, etc.)")]
        [SerializeField] private bool enableCoreLogs = true;

        // Public accessors
        public bool EnableAllLogs => enableAllLogs;
        public bool UseModuleOverrides => useModuleOverrides;
        public bool EnableAnalyticsLogs => enableAnalyticsLogs;
        public bool EnableSaveLogs => enableSaveLogs;
        public bool EnableLocalizationLogs => enableLocalizationLogs;
        public bool EnableSceneLogs => enableSceneLogs;
        public bool EnableUILogs => enableUILogs;
        public bool EnableEventLogs => enableEventLogs;
        public bool EnableGameLogs => enableGameLogs;
        public bool EnableCoreLogs => enableCoreLogs;

        /// <summary>
        /// Get or load the LogSettings instance
        /// </summary>
        public static LogSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LogSettings>("LogSettings");
                    if (_instance == null)
                    {
                        _instance = CreateDefault();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Check if a specific module has logging enabled
        /// </summary>
        public bool IsModuleEnabled(LogModule module)
        {
            if (!enableAllLogs) return false;
            if (!useModuleOverrides) return true;

            return module switch
            {
                LogModule.Analytics => enableAnalyticsLogs,
                LogModule.Save => enableSaveLogs,
                LogModule.Localization => enableLocalizationLogs,
                LogModule.Scene => enableSceneLogs,
                LogModule.UI => enableUILogs,
                LogModule.Event => enableEventLogs,
                LogModule.Game => enableGameLogs,
                LogModule.Core => enableCoreLogs,
                _ => true
            };
        }

        /// <summary>
        /// Create default settings instance (runtime only)
        /// </summary>
        public static LogSettings CreateDefault()
        {
            var settings = CreateInstance<LogSettings>();
            settings.enableAllLogs = true;
            settings.useModuleOverrides = false;
            return settings;
        }

        /// <summary>
        /// Set instance manually (useful for testing)
        /// </summary>
        public static void SetInstance(LogSettings settings)
        {
            _instance = settings;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Enable all logs for debugging
        /// </summary>
        [ContextMenu("Enable All Logs")]
        private void EnableAll()
        {
            enableAllLogs = true;
            useModuleOverrides = false;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Editor-only: Disable all logs for production
        /// </summary>
        [ContextMenu("Disable All Logs")]
        private void DisableAll()
        {
            enableAllLogs = false;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    /// <summary>
    /// Log module categories for filtering
    /// </summary>
    public enum LogModule
    {
        Analytics,
        Save,
        Localization,
        Scene,
        UI,
        Event,
        Game,
        Core
    }
}
