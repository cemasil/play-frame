using System;
using UnityEngine;

namespace MiniGameFramework.Systems.Localization
{
    /// <summary>
    /// Represents a single key-value pair for localization.
    /// </summary>
    [Serializable]
    public struct LocalizedEntry
    {
        [Tooltip("The localization key (e.g., 'ui.score')")]
        public string key;

        [Tooltip("The localized text for this key")]
        [TextArea(1, 3)]
        public string value;
    }

    /// <summary>
    /// ScriptableObject containing all localized strings for a specific language.
    /// Create one per language: English.asset, Turkish.asset, etc.
    /// Create via: Assets > Create > MiniGameFramework > Localization > String Table
    /// </summary>
    [CreateAssetMenu(fileName = "NewLanguage", menuName = "MiniGameFramework/Localization/String Table")]
    public class LocalizedStringTable : ScriptableObject
    {
        [Header("Language Info")]
        [Tooltip("Language code (e.g., 'en', 'tr', 'de')")]
        [SerializeField] private string languageCode = "en";

        [Tooltip("Display name for UI (e.g., 'English', 'Türkçe')")]
        [SerializeField] private string displayName = "English";

        [Tooltip("Native display name (e.g., 'English', 'Türkçe')")]
        [SerializeField] private string nativeDisplayName = "English";

        [Header("Localized Strings")]
        [SerializeField] private LocalizedEntry[] entries;

        // Runtime lookup cache
        private System.Collections.Generic.Dictionary<string, string> _lookupCache;

        public string LanguageCode => languageCode;
        public string DisplayName => displayName;
        public string NativeDisplayName => nativeDisplayName;

        /// <summary>
        /// Get localized string by key.
        /// Returns the key itself if not found (helps identify missing translations).
        /// </summary>
        public string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            BuildCacheIfNeeded();

            if (_lookupCache.TryGetValue(key, out string value))
                return value;

#if UNITY_EDITOR
            Debug.LogWarning($"[Localization] Missing key '{key}' in {displayName} ({languageCode})");
#endif
            return $"[{key}]";
        }

        /// <summary>
        /// Get localized string with format arguments.
        /// Example: GetString("ui.score", score) -> "Score: 100"
        /// </summary>
        public string GetString(string key, params object[] args)
        {
            string format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                Debug.LogError($"[Localization] Format error for key '{key}' with {args.Length} arguments");
                return format;
            }
        }

        /// <summary>
        /// Check if a key exists in this table.
        /// </summary>
        public bool HasKey(string key)
        {
            BuildCacheIfNeeded();
            return _lookupCache.ContainsKey(key);
        }

        private void BuildCacheIfNeeded()
        {
            if (_lookupCache != null)
                return;

            _lookupCache = new System.Collections.Generic.Dictionary<string, string>(
                entries?.Length ?? 0,
                StringComparer.Ordinal
            );

            if (entries == null)
                return;

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.key))
                    continue;

                if (_lookupCache.ContainsKey(entry.key))
                {
                    Debug.LogWarning($"[Localization] Duplicate key '{entry.key}' in {displayName}");
                    continue;
                }

                _lookupCache[entry.key] = entry.value;
            }
        }

        /// <summary>
        /// Clear the lookup cache (useful for hot-reload in editor).
        /// </summary>
        public void InvalidateCache()
        {
            _lookupCache = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InvalidateCache();
        }
#endif
    }
}
