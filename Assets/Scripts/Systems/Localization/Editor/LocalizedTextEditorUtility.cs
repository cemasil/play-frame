#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using PlayFrame.Core.Logging;
using ILogger = PlayFrame.Core.Logging.ILogger;

namespace PlayFrame.Systems.Localization.Editor
{
    /// <summary>
    /// Editor utility to quickly add LocalizedText components to selected objects.
    /// </summary>
    public static class LocalizedTextEditorUtility
    {
        private static readonly ILogger _logger = LoggerFactory.CreateLocalization("LocalizedTextEditor");

        [MenuItem("GameObject/Localization/Add LocalizedText", false, 0)]
        private static void AddLocalizedTextToSelected()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
                if (text == null)
                {
                    _logger.LogWarning($"{obj.name} has no TextMeshProUGUI component");
                    continue;
                }

                if (obj.GetComponent<LocalizedText>() != null)
                {
                    _logger.Log($"{obj.name} already has LocalizedText");
                    continue;
                }

                LocalizedText localizedText = obj.AddComponent<LocalizedText>();
                _logger.Log($"Added LocalizedText to {obj.name}");

                // Try to guess key from object name
                string guessedKey = GuessKeyFromName(obj.name);
                if (!string.IsNullOrEmpty(guessedKey))
                {
                    SerializedObject so = new SerializedObject(localizedText);
                    so.FindProperty("localizationKey").stringValue = guessedKey;
                    so.ApplyModifiedProperties();
                    _logger.Log($"Auto-assigned key: {guessedKey}");
                }
            }
        }

        [MenuItem("GameObject/Localization/Add LocalizedText", true)]
        private static bool ValidateAddLocalizedText()
        {
            return Selection.gameObjects.Length > 0;
        }

        private static string GuessKeyFromName(string objectName)
        {
            string lower = objectName.ToLower().Replace(" ", "").Replace("_", "");

            // Buttons
            if (lower.Contains("play")) return LocalizationKeys.PLAY;
            if (lower.Contains("restart")) return LocalizationKeys.RESTART;
            if (lower.Contains("resume")) return LocalizationKeys.RESUME;
            if (lower.Contains("pause")) return LocalizationKeys.PAUSE;
            if (lower.Contains("quit") || lower.Contains("exit")) return LocalizationKeys.QUIT;
            if (lower.Contains("mainmenu")) return LocalizationKeys.MAIN_MENU;
            if (lower.Contains("settings")) return LocalizationKeys.SETTINGS;
            if (lower.Contains("nextlevel")) return LocalizationKeys.NEXT_LEVEL;
            if (lower.Contains("tryagain")) return LocalizationKeys.TRY_AGAIN;

            // Labels
            if (lower.Contains("loading")) return LocalizationKeys.LOADING;
            if (lower.Contains("music")) return LocalizationKeys.MUSIC;
            if (lower.Contains("sfx") || lower.Contains("sound")) return LocalizationKeys.SFX;
            if (lower.Contains("language")) return LocalizationKeys.LANGUAGE;

            return string.Empty;
        }
    }
}
#endif
