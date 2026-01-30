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

        // Singleton instance for easy access
        private static UIPrefabSettings _instance;

        /// <summary>
        /// Get the singleton instance.
        /// Must be set via SetInstance() at startup (e.g., in GameBootstrap).
        /// </summary>
        public static UIPrefabSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogWarning("[UIPrefabSettings] Instance not set. " +
                        "Call UIPrefabSettings.SetInstance() during initialization.");
                }
                return _instance;
            }
        }

        /// <summary>
        /// Set the singleton instance (called by UIManager)
        /// </summary>
        public static void SetInstance(UIPrefabSettings settings)
        {
            _instance = settings;
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
