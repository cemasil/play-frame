using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayFrame.UI
{
    /// <summary>
    /// Factory for creating common UI elements from prefabs.
    /// Requires UIPrefabSettings to be initialized via SetSettings() at startup.
    /// </summary>
    public static class UIPrefabFactory
    {
        private static UIPrefabSettings _settings;
        private static bool _isInitialized;

        /// <summary>
        /// Whether the factory has been initialized with settings
        /// </summary>
        public static bool IsInitialized => _isInitialized && _settings != null;

        /// <summary>
        /// Current settings reference
        /// </summary>
        public static UIPrefabSettings Settings => _settings;

        /// <summary>
        /// Set the prefab settings (called by UIBootstrap at startup)
        /// </summary>
        public static void SetSettings(UIPrefabSettings settings)
        {
            if (settings == null)
            {
                Debug.LogError("[UIPrefabFactory] Cannot set null settings!");
                return;
            }

            _settings = settings;
            _isInitialized = true;
        }

        /// <summary>
        /// Get settings with lazy initialization fallback
        /// </summary>
        private static UIPrefabSettings GetSettings()
        {
            if (_settings == null)
            {
                Debug.LogError("[UIPrefabFactory] UIPrefabSettings not initialized. " +
                    "Call UIPrefabFactory.SetSettings() during initialization (e.g., in UIBootstrap).");
            }
            return _settings;
        }

        public static Button CreateButton(string text, Transform parent)
        {
            var settings = GetSettings();
            if (settings?.ThemedButtonPrefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedButton prefab not assigned in UIPrefabSettings");
                return null;
            }

            var button = Object.Instantiate(settings.ThemedButtonPrefab, parent);
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = text;

            return button;
        }

        public static GameObject CreatePanel(Transform parent)
        {
            var settings = GetSettings();
            if (settings?.ThemedPanelPrefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedPanel prefab not assigned in UIPrefabSettings");
                return null;
            }

            return Object.Instantiate(settings.ThemedPanelPrefab, parent);
        }

        public static TextMeshProUGUI CreateText(string content, Transform parent, TextType textType = TextType.Body)
        {
            var settings = GetSettings();
            var prefab = settings?.GetTextPrefab(textType);
            if (prefab == null)
            {
                Debug.LogWarning($"[UIPrefabFactory] Text prefab for {textType} not assigned in UIPrefabSettings");
                return null;
            }

            var text = Object.Instantiate(prefab, parent);
            text.text = content;

            var themedElement = text.GetComponent<ThemedUIElement>();
            if (themedElement != null && settings?.DefaultTheme != null)
            {
                themedElement.SetTheme(settings.DefaultTheme);
            }

            return text;
        }

        public static GameObject CreateGameCard(string gameName, int highScore, Color color, Transform parent)
        {
            var settings = GetSettings();
            if (settings?.GameCardPrefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] GameCard prefab not assigned in UIPrefabSettings");
                return null;
            }

            var card = Object.Instantiate(settings.GameCardPrefab, parent);

            var nameTMP = card.transform.Find("GameName")?.GetComponent<TextMeshProUGUI>();
            if (nameTMP == null)
                nameTMP = card.GetComponentInChildren<TextMeshProUGUI>();

            if (nameTMP != null)
            {
                nameTMP.text = gameName;
            }

            var scoreTMP = card.transform.Find("HighScore")?.GetComponent<TextMeshProUGUI>();
            if (scoreTMP == null)
            {
                var tmps = card.GetComponentsInChildren<TextMeshProUGUI>();
                if (tmps.Length > 1)
                    scoreTMP = tmps[1];
            }

            if (scoreTMP != null)
            {
                scoreTMP.text = $"High Score: {highScore}";
            }

            var gameIcon = card.transform.Find("GameIcon")?.GetComponent<Image>();
            if (gameIcon == null)
                gameIcon = card.GetComponentInChildren<Image>();

            if (gameIcon != null)
            {
                gameIcon.color = color;
            }

            return card;
        }
    }
}
