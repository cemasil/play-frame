using UnityEngine;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Save
{
    /// <summary>
    /// ScriptableObject for configuring save system settings.
    /// Create via Assets > Create > PlayFrame > Save System > Save Settings
    /// Place in GameSettings/SaveSystem folder.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveSettings", menuName = "PlayFrame/Save System/Save Settings")]
    public class SaveSettings : ScriptableObject
    {
        private static readonly ILogger _logger = LoggerFactory.CreateSave("SaveSettings");
        [Header("Save Configuration")]
        [Tooltip("The key used to store save data in PlayerPrefs")]
        [SerializeField] private string saveKey = "GameSaveData";

        [Tooltip("Use pretty print (formatted) JSON for easier debugging")]
        [SerializeField] private bool prettyPrintJson = true;

        [Header("Auto-Save")]
        [Tooltip("Enable automatic saving at regular intervals")]
        [SerializeField] private bool enableAutoSave = false;

        [Tooltip("Auto-save interval in seconds")]
        [Range(30f, 600f)]
        [SerializeField] private float autoSaveInterval = 60f;

        [Tooltip("Auto-save when application loses focus (goes to background)")]
        [SerializeField] private bool saveOnApplicationPause = true;

        [Tooltip("Auto-save when application quits")]
        [SerializeField] private bool saveOnApplicationQuit = true;

        [Header("Backup Settings")]
        [Tooltip("Keep a backup of the previous save")]
        [SerializeField] private bool enableBackup = true;

        [Tooltip("Key suffix for backup save data")]
        [SerializeField] private string backupKeySuffix = "_backup";

        [Tooltip("Number of backup slots to keep (1-5)")]
        [Range(1, 5)]
        [SerializeField] private int backupSlotCount = 1;

        [Header("Data Validation")]
        [Tooltip("Validate save data integrity on load")]
        [SerializeField] private bool validateOnLoad = true;

        [Tooltip("Attempt to restore from backup if main save is corrupted")]
        [SerializeField] private bool restoreFromBackupOnError = true;

        [Header("Debug Settings")]
        [Tooltip("Enable debug logging for save operations")]
        [SerializeField] private bool enableDebugLogs = false;

        [Tooltip("Log detailed save data contents (WARNING: may expose sensitive data)")]
        [SerializeField] private bool logSaveContents = false;

        [Header("Encryption (Future)")]
        [Tooltip("Enable save data encryption (not yet implemented)")]
        [SerializeField] private bool enableEncryption = false;

        // Public Properties - Save Configuration
        public string SaveKey => saveKey;
        public bool PrettyPrintJson => prettyPrintJson;

        // Public Properties - Auto-Save
        public bool EnableAutoSave => enableAutoSave;
        public float AutoSaveInterval => autoSaveInterval;
        public bool SaveOnApplicationPause => saveOnApplicationPause;
        public bool SaveOnApplicationQuit => saveOnApplicationQuit;

        // Public Properties - Backup
        public bool EnableBackup => enableBackup;
        public string BackupKey => saveKey + backupKeySuffix;
        public int BackupSlotCount => backupSlotCount;

        // Public Properties - Validation
        public bool ValidateOnLoad => validateOnLoad;
        public bool RestoreFromBackupOnError => restoreFromBackupOnError;

        // Public Properties - Debug
        public bool EnableDebugLogs => enableDebugLogs;
        public bool LogSaveContents => logSaveContents;

        // Public Properties - Encryption
        public bool EnableEncryption => enableEncryption;

        /// <summary>
        /// Get the backup key for a specific slot
        /// </summary>
        public string GetBackupKey(int slot = 0)
        {
            if (slot <= 0)
                return BackupKey;
            return $"{saveKey}{backupKeySuffix}_{slot}";
        }

        /// <summary>
        /// Create default settings instance when no settings asset is assigned.
        /// For production, assign SaveSettings from GameSettings/SaveSystem via Inspector.
        /// </summary>
        public static SaveSettings CreateDefault()
        {
            var settings = CreateInstance<SaveSettings>();
            _logger.LogWarning("No SaveSettings assigned. Using default settings. " +
                           "Create one via Assets > Create > PlayFrame > Save System > Save Settings " +
                           "and place in GameSettings/SaveSystem folder.");
            return settings;
        }

        private void OnValidate()
        {
            // Ensure valid values
            autoSaveInterval = Mathf.Max(30f, autoSaveInterval);
            backupSlotCount = Mathf.Clamp(backupSlotCount, 1, 5);

            // Ensure save key is not empty
            if (string.IsNullOrWhiteSpace(saveKey))
            {
                saveKey = "GameSaveData";
            }
        }
    }
}
