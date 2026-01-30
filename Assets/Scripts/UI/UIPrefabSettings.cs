using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayFrame.UI
{
    /// <summary>
    /// ScriptableObject for UI prefab and theme references.
    /// Create via Assets > Create > PlayFrame > UI > UI Prefab Settings
    /// Place in GameSettings/UI folder.
    /// </summary>
    [CreateAssetMenu(fileName = "UIPrefabSettings", menuName = "PlayFrame/UI/UI Prefab Settings")]
    public class UIPrefabSettings : ScriptableObject
    {
        [Header("Default Theme")]
        [Tooltip("Default UI theme used when no theme is specified")]
        [SerializeField] private UITheme defaultTheme;

        [Header("Button Prefabs")]
        [SerializeField] private Button themedButtonPrefab;

        [Header("Panel Prefabs")]
        [SerializeField] private GameObject themedPanelPrefab;

        [Header("Text Prefabs")]
        [SerializeField] private TextMeshProUGUI themedTextBodyPrefab;
        [SerializeField] private TextMeshProUGUI themedTextHeaderPrefab;
        [SerializeField] private TextMeshProUGUI themedTextTitlePrefab;

        [Header("Card Prefabs")]
        [SerializeField] private GameObject gameCardPrefab;

        // Public Properties
        public UITheme DefaultTheme => defaultTheme;
        public Button ThemedButtonPrefab => themedButtonPrefab;
        public GameObject ThemedPanelPrefab => themedPanelPrefab;
        public TextMeshProUGUI ThemedTextBodyPrefab => themedTextBodyPrefab;
        public TextMeshProUGUI ThemedTextHeaderPrefab => themedTextHeaderPrefab;
        public TextMeshProUGUI ThemedTextTitlePrefab => themedTextTitlePrefab;
        public GameObject GameCardPrefab => gameCardPrefab;

        /// <summary>
        /// Get text prefab by type
        /// </summary>
        public TextMeshProUGUI GetTextPrefab(TextType textType)
        {
            return textType switch
            {
                TextType.Body => themedTextBodyPrefab,
                TextType.Header => themedTextHeaderPrefab,
                TextType.Title => themedTextTitlePrefab,
                _ => themedTextBodyPrefab
            };
        }

        /// <summary>
        /// Create default settings instance when no settings asset is assigned.
        /// For production, assign UIPrefabSettings from GameSettings/UI via Inspector.
        /// </summary>
        public static UIPrefabSettings CreateDefault()
        {
            var settings = CreateInstance<UIPrefabSettings>();
            Debug.LogWarning("[UIPrefabSettings] No UIPrefabSettings assigned. Using default settings. " +
                           "Create one via Assets > Create > PlayFrame > UI > UI Prefab Settings " +
                           "and place in GameSettings/UI folder.");
            return settings;
        }

        private void OnValidate()
        {
            if (defaultTheme == null)
            {
                Debug.LogWarning("[UIPrefabSettings] Default theme is not assigned!");
            }
        }
    }
}
