using UnityEngine;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// ScriptableObject for configuring analytics settings.
    /// Create via Assets > Create > PlayFrame > Analytics > Analytics Settings
    /// Place in GameSettings/Analytics folder.
    /// </summary>
    [CreateAssetMenu(fileName = "AnalyticsSettings", menuName = "PlayFrame/Analytics/Analytics Settings")]
    public class AnalyticsSettings : ScriptableObject
    {
        private static readonly ILogger _logger = LoggerFactory.CreateAnalytics("AnalyticsSettings");

        [Header("General Settings")]
        [Tooltip("Enable/disable analytics collection globally")]
        [SerializeField] private bool enableAnalytics = true;

        [Tooltip("Enable debug logging for analytics events")]
        [SerializeField] private bool enableDebugLogs = false;

        [Header("Batching Settings")]
        [Tooltip("Enable event batching for performance")]
        [SerializeField] private bool enableBatching = true;

        [Tooltip("Interval in seconds between batch flushes")]
        [Range(5f, 120f)]
        [SerializeField] private float batchFlushInterval = 30f;

        [Tooltip("Maximum events to batch before auto-flush")]
        [Range(10, 200)]
        [SerializeField] private int maxBatchSize = 50;

        [Header("Privacy Settings")]
        [Tooltip("Collect device information")]
        [SerializeField] private bool collectDeviceInfo = true;

        [Tooltip("Collect user session data")]
        [SerializeField] private bool collectSessionData = true;

        [Tooltip("Collect detailed gameplay metrics")]
        [SerializeField] private bool collectDetailedMetrics = true;

        [Header("Built-in Providers")]
        [Tooltip("Enable console logging provider (Editor/Debug builds only)")]
        [SerializeField] private bool enableConsoleProvider = true;

        [Tooltip("Enable local file storage provider for offline analytics")]
        [SerializeField] private bool enableLocalStorageProvider = true;

        [Tooltip("Maximum events to store locally")]
        [Range(100, 5000)]
        [SerializeField] private int maxLocalStorageEvents = 1000;

        [Header("External Providers")]
        [Tooltip("Enable Unity Analytics provider (requires Unity Analytics package)")]
        [SerializeField] private bool enableUnityAnalytics = false;

        [Tooltip("Enable Firebase Analytics provider (requires Firebase SDK)")]
        [SerializeField] private bool enableFirebaseAnalytics = false;

        // Public Properties - General
        public bool EnableAnalytics => enableAnalytics;
        public bool EnableDebugLogs => enableDebugLogs;

        // Public Properties - Batching
        public bool EnableBatching => enableBatching;
        public float BatchFlushInterval => batchFlushInterval;
        public int MaxBatchSize => maxBatchSize;

        // Public Properties - Privacy
        public bool CollectDeviceInfo => collectDeviceInfo;
        public bool CollectSessionData => collectSessionData;
        public bool CollectDetailedMetrics => collectDetailedMetrics;

        // Public Properties - Built-in Providers
        public bool EnableConsoleProvider => enableConsoleProvider;
        public bool EnableLocalStorageProvider => enableLocalStorageProvider;
        public int MaxLocalStorageEvents => maxLocalStorageEvents;

        // Public Properties - External Providers
        public bool EnableUnityAnalytics => enableUnityAnalytics;
        public bool EnableFirebaseAnalytics => enableFirebaseAnalytics;

        /// <summary>
        /// Create default settings instance when no settings asset is assigned.
        /// For production, assign AnalyticsSettings from GameSettings/Analytics via Inspector.
        /// </summary>
        public static AnalyticsSettings CreateDefault()
        {
            var settings = CreateInstance<AnalyticsSettings>();
            _logger.LogWarning("No AnalyticsSettings assigned. Using default settings. " +
                           "Create one via Assets > Create > PlayFrame > Analytics > Analytics Settings " +
                           "and place in GameSettings/Analytics folder.");
            return settings;
        }

        private void OnValidate()
        {
            // Ensure valid values
            batchFlushInterval = Mathf.Max(5f, batchFlushInterval);
            maxBatchSize = Mathf.Max(10, maxBatchSize);
            maxLocalStorageEvents = Mathf.Max(100, maxLocalStorageEvents);
        }
    }
}
