using UnityEngine;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// ScriptableObject for configuring analytics settings.
    /// Create via Assets > Create > PlayFrame > Analytics > Analytics Settings
    /// </summary>
    [CreateAssetMenu(fileName = "AnalyticsSettings", menuName = "PlayFrame/Analytics/Analytics Settings")]
    public class AnalyticsSettings : ScriptableObject
    {
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

        [Header("Storage Settings")]
        [Tooltip("Enable local storage for offline analytics")]
        [SerializeField] private bool enableLocalStorage = true;

        [Tooltip("Maximum events to store locally")]
        [Range(100, 5000)]
        [SerializeField] private int maxLocalStorageEvents = 1000;

        [Header("Provider Settings")]
        [Tooltip("Enable console logging provider (Editor/Debug only)")]
        [SerializeField] private bool enableConsoleProvider = true;

        [Tooltip("Enable local file storage provider")]
        [SerializeField] private bool enableLocalStorageProvider = true;

        // Public Properties
        public bool EnableAnalytics => enableAnalytics;
        public bool EnableDebugLogs => enableDebugLogs;
        public bool EnableBatching => enableBatching;
        public float BatchFlushInterval => batchFlushInterval;
        public int MaxBatchSize => maxBatchSize;
        public bool CollectDeviceInfo => collectDeviceInfo;
        public bool CollectSessionData => collectSessionData;
        public bool CollectDetailedMetrics => collectDetailedMetrics;
        public bool EnableLocalStorage => enableLocalStorage;
        public int MaxLocalStorageEvents => maxLocalStorageEvents;
        public bool EnableConsoleProvider => enableConsoleProvider;
        public bool EnableLocalStorageProvider => enableLocalStorageProvider;

        /// <summary>
        /// Apply settings to the AnalyticsManager
        /// </summary>
        public void ApplyToManager(AnalyticsManager manager)
        {
            if (manager == null) return;

            manager.SetEnabled(enableAnalytics);
            manager.SetDebugLogging(enableDebugLogs);
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
