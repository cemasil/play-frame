using System;
using UnityEngine;
using PlayFrame.Core;
using PlayFrame.Core.Events;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Localization
{
    /// <summary>
    /// Manages localization and language switching.
    /// Supports runtime language changes with event broadcasting.
    /// </summary>
    public class LocalizationManager : PersistentSingleton<LocalizationManager>
    {
        private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

        [Header("Language Tables")]
        [Tooltip("All available language tables. First one is default.")]
        [SerializeField] private LocalizedStringTable[] availableLanguages;

        [Header("Settings")]
        [Tooltip("Use system language on first launch")]
        [SerializeField] private bool useSystemLanguageOnFirstLaunch = true;

        private LocalizedStringTable _currentTable;
        private int _currentLanguageIndex;
        private ILogger _logger;

        /// <summary>
        /// Event fired when language changes.
        /// </summary>
        public event Action<LocalizedStringTable> OnLanguageChanged;

        /// <summary>
        /// Current active language table.
        /// </summary>
        public LocalizedStringTable CurrentLanguage => _currentTable;

        /// <summary>
        /// Current language code (e.g., "en", "tr").
        /// </summary>
        public string CurrentLanguageCode => _currentTable?.LanguageCode ?? "en";

        /// <summary>
        /// All available language tables.
        /// </summary>
        public LocalizedStringTable[] AvailableLanguages => availableLanguages;

        /// <summary>
        /// Number of available languages.
        /// </summary>
        public int LanguageCount => availableLanguages?.Length ?? 0;

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
            _logger = LoggerFactory.CreateLocalization("Localization");
            InitializeLanguage();
        }

        private void InitializeLanguage()
        {
            if (availableLanguages == null || availableLanguages.Length == 0)
            {
                _logger.LogError("No language tables assigned!");
                return;
            }

            // Try to load saved language preference
            string savedLanguage = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, string.Empty);
            _logger.Log($"Saved language: '{savedLanguage}', System language: {Application.systemLanguage}");

            if (!string.IsNullOrEmpty(savedLanguage))
            {
                // Find saved language
                for (int i = 0; i < availableLanguages.Length; i++)
                {
                    if (availableLanguages[i] != null &&
                        availableLanguages[i].LanguageCode == savedLanguage)
                    {
                        _logger.Log($"Using saved language: {savedLanguage}");
                        SetLanguageInternal(i, false);
                        return;
                    }
                }
                _logger.Log($"Saved language '{savedLanguage}' not found in available languages");
            }

            // First launch - try system language
            if (useSystemLanguageOnFirstLaunch)
            {
                string systemLang = GetSystemLanguageCode();
                _logger.Log($"Looking for system language: {systemLang}");
                for (int i = 0; i < availableLanguages.Length; i++)
                {
                    if (availableLanguages[i] != null &&
                        availableLanguages[i].LanguageCode == systemLang)
                    {
                        _logger.Log($"Using system language: {systemLang}");
                        SetLanguageInternal(i, true);
                        return;
                    }
                }
                _logger.Log($"System language '{systemLang}' not found, falling back to first language");
            }

            // Fallback to first language
            _logger.Log($"Using first language: {availableLanguages[0]?.LanguageCode}");
            SetLanguageInternal(0, true);
        }

        /// <summary>
        /// Get localized string by key.
        /// </summary>
        public static string Get(string key)
        {
            if (!HasInstance || Instance._currentTable == null)
                return $"[{key}]";

            return Instance._currentTable.GetString(key);
        }

        /// <summary>
        /// Get localized string with format arguments.
        /// Example: Get("ui.score", 100) -> "Score: 100"
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            if (!HasInstance || Instance._currentTable == null)
                return $"[{key}]";

            return Instance._currentTable.GetString(key, args);
        }

        /// <summary>
        /// Set language by index.
        /// </summary>
        public void SetLanguage(int index)
        {
            if (index < 0 || index >= availableLanguages.Length)
            {
                _logger.LogError($"Invalid language index: {index}");
                return;
            }

            SetLanguageInternal(index, true);
        }

        /// <summary>
        /// Set language by language code (e.g., "en", "tr").
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            for (int i = 0; i < availableLanguages.Length; i++)
            {
                if (availableLanguages[i] != null &&
                    availableLanguages[i].LanguageCode == languageCode)
                {
                    SetLanguageInternal(i, true);
                    return;
                }
            }

            _logger.LogWarning($"Language not found: {languageCode}");
        }

        /// <summary>
        /// Cycle to next language (useful for simple language toggle buttons).
        /// </summary>
        public void NextLanguage()
        {
            int nextIndex = (_currentLanguageIndex + 1) % availableLanguages.Length;
            SetLanguageInternal(nextIndex, true);
        }

        private void SetLanguageInternal(int index, bool save)
        {
            if (availableLanguages[index] == null)
            {
                _logger.LogError($"Language table at index {index} is null!");
                return;
            }

            _currentLanguageIndex = index;
            _currentTable = availableLanguages[index];

            if (save)
            {
                PlayerPrefs.SetString(LANGUAGE_PREF_KEY, _currentTable.LanguageCode);
                PlayerPrefs.Save();
            }

            _logger.Log($"Language set to: {_currentTable.DisplayName} ({_currentTable.LanguageCode})");

            // Notify listeners
            OnLanguageChanged?.Invoke(_currentTable);

            // Broadcast via EventManager for decoupled listeners
            if (EventManager.HasInstance)
            {
                EventManager.Instance.TriggerEvent(CoreEvents.LanguageChanged, _currentTable.LanguageCode);
            }
        }

        private string GetSystemLanguageCode()
        {
            return Application.systemLanguage switch
            {
                SystemLanguage.Turkish => "tr",
                SystemLanguage.German => "de",
                SystemLanguage.French => "fr",
                SystemLanguage.Spanish => "es",
                SystemLanguage.Italian => "it",
                SystemLanguage.Portuguese => "pt",
                SystemLanguage.Russian => "ru",
                SystemLanguage.Japanese => "ja",
                SystemLanguage.Korean => "ko",
                SystemLanguage.Chinese => "zh",
                SystemLanguage.ChineseSimplified => "zh-CN",
                SystemLanguage.ChineseTraditional => "zh-TW",
                SystemLanguage.Arabic => "ar",
                _ => "en"
            };
        }
    }
}
