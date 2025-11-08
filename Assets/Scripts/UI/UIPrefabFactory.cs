using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGameFramework.UI.Prefabs
{
    /// <summary>
    /// Factory for creating common UI elements from prefabs in Resources/UI/Prefabs/
    /// </summary>
    public static class UIPrefabFactory
    {
        private const string PREFAB_PATH = "UI/Prefabs/";

        public static Button CreateButton(string text, Transform parent)
        {
            var prefab = Resources.Load<Button>(PREFAB_PATH + "ThemedButton");
            if (prefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedButton prefab not found in Resources/UI/Prefabs/");
                return null;
            }

            var button = Object.Instantiate(prefab, parent);
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = text;

            return button;
        }

        public static GameObject CreatePanel(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PREFAB_PATH + "ThemedPanel");
            if (prefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedPanel prefab not found in Resources/UI/Prefabs/");
                return null;
            }

            return Object.Instantiate(prefab, parent);
        }

        public static TextMeshProUGUI CreateText(string content, Transform parent, TextType textType = TextType.Body)
        {
            string prefabName = textType switch
            {
                TextType.Body => "ThemedTextBody",
                TextType.Header => "ThemedTextHeader",
                TextType.Title => "ThemedTextTitle",
                _ => null
            };

            if (prefabName == null)
            {
                Debug.LogWarning($"[UIPrefabFactory] Invalid TextType: {textType}");
                return null;
            }

            var prefab = Resources.Load<TextMeshProUGUI>(PREFAB_PATH + prefabName);
            if (prefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] ThemedText prefab not found in Resources/UI/Prefabs/");
                return null;
            }

            var text = Object.Instantiate(prefab, parent);
            text.text = content;

            var themedElement = text.GetComponent<ThemedUIElement>();
            if (themedElement != null)
            {
                themedElement.SetTheme(Resources.Load<UITheme>("DefaultUITheme"));
            }

            return text;
        }

        public static GameObject CreateGameCard(string gameName, int highScore, Color color, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PREFAB_PATH + "GameCard");
            if (prefab == null)
            {
                Debug.LogWarning("[UIPrefabFactory] GameCard prefab not found in Resources/UI/Prefabs/");
                return null;
            }

            var card = Object.Instantiate(prefab, parent);

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
