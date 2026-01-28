using UnityEngine;
using TMPro;

namespace MiniGameFramework.Systems.Localization
{
    /// <summary>
    /// Automatically localizes a TextMeshPro text component.
    /// Attach to any GameObject with TextMeshProUGUI and set the localization key.
    /// Updates automatically when language changes.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization")]
        [Tooltip("The localization key to use (e.g., 'ui.score')")]
        [SerializeField] private string localizationKey;

        [Tooltip("Optional prefix (not localized)")]
        [SerializeField] private string prefix;

        [Tooltip("Optional suffix (not localized)")]
        [SerializeField] private string suffix;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            UpdateText();
        }

        private void OnDisable()
        {
            if (LocalizationManager.HasInstance)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(LocalizedStringTable newLanguage)
        {
            UpdateText();
        }

        /// <summary>
        /// Update the text with current localization.
        /// </summary>
        public void UpdateText()
        {
            if (_text == null || string.IsNullOrEmpty(localizationKey))
                return;

            string localizedValue = LocalizationManager.Get(localizationKey);
            _text.text = $"{prefix}{localizedValue}{suffix}";
        }

        /// <summary>
        /// Update the text with format arguments.
        /// Example: SetText(100) where key="ui.score" and format="Score: {0}"
        /// </summary>
        public void SetText(params object[] args)
        {
            if (_text == null || string.IsNullOrEmpty(localizationKey))
                return;

            string localizedValue = LocalizationManager.Get(localizationKey, args);
            _text.text = $"{prefix}{localizedValue}{suffix}";
        }

        /// <summary>
        /// Change the localization key at runtime.
        /// </summary>
        public void SetKey(string newKey)
        {
            localizationKey = newKey;
            UpdateText();
        }

        /// <summary>
        /// Change key and update with format arguments.
        /// </summary>
        public void SetKey(string newKey, params object[] args)
        {
            localizationKey = newKey;
            SetText(args);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_text == null)
                _text = GetComponent<TextMeshProUGUI>();

            // Preview in editor if possible
            if (!string.IsNullOrEmpty(localizationKey) && _text != null)
            {
                _text.text = $"{prefix}[{localizationKey}]{suffix}";
            }
        }
#endif
    }
}
