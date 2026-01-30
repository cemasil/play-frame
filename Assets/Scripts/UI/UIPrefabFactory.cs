using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayFrame.UI.Prefabs
{
    /// <summary>
    /// Factory for creating common UI elements from prefabs.
    /// Requires UIPrefabSettings to be initialized via UIManager or SetSettings().
    /// </summary>
    public static class UIPrefabFactory
    {
        private static UIPrefabSettings _settings;

        /// <summary>
        /// Set the prefab settings (called by UIManager or at startup)
        /// </summary>
        public static void SetSettings(UIPrefabSettings settings)
        {
            _settings = settings;
            UIPrefabSettings.SetInstance(settings);
        }

        private static UIPrefabSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = UIPrefabSettings.Instance;
                    if (_settings == null)
                    {
                        Debug.LogError("[UIPrefabFactory] UIPrefabSettings not initialized. " +
                            "Call UIPrefabFactory.SetSettings() or ensure UIManager has settings assigned.");
                    }
                }
                return _settings;
            }
        }

        public static Button CreateButton(string text, Transform parent)
        {
            if (Settings?.ThemedButtonPrefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedButton prefab not assigned in UIPrefabSettings");
                return null;
            }

            var button = Object.Instantiate(Settings.ThemedButtonPrefab, parent);
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = text;

            return button;
        }

        public static GameObject CreatePanel(Transform parent)
        {
            if (Settings?.ThemedPanelPrefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedPanel prefab not assigned in UIPrefabSettings");
                return null;
            }

            return Object.Instantiate(Settings.ThemedPanelPrefab, parent);
        }

        public static TextMeshProUGUI CreateText(string content, Transform parent, TextType textType = TextType.Body)
        {
            var prefab = Settings?.GetTextPrefab(textType);
            if (prefab == null)
            {
                Debug.LogWarning($"[UIPrefabFactory] Text prefab for {textType} not assigned in UIPrefabSettings");
                return null;
            }

            var text = Object.Instantiate(prefab, parent);
            text.text = content;

            var themedElement = text.GetComponent<ThemedUIElement>();
            if (themedElement != null && Settings?.DefaultTheme != null)
            {
                themedElement.SetTheme(Settings.DefaultTheme);
            }

            return text;
        }

        public static GameObject CreateGameCard(string gameName, int highScore, Color color, Transform parent)
        {
            if (Settings?.GameCardPrefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] GameCard prefab not assigned in UIPrefabSettings");
                return null;
            }

            var card = Object.Instantiate(Settings.GameCardPrefab, parent);

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
