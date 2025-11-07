using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGameFramework.UI
{
    /// <summary>
    /// Helper component to automatically apply UI theme to elements
    /// </summary>
    [ExecuteInEditMode]
    public class ThemedUIElement : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] private UITheme theme;

        [Header("Element Type")]
        [SerializeField] private ElementType elementType = ElementType.Button;
        [SerializeField] private TextType textType = TextType.Body;

        [Header("Auto Apply")]
        [SerializeField] private bool applyOnEnable = true;

        private void OnEnable()
        {
            if (applyOnEnable)
                ApplyTheme();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (theme == null)
            {
                theme = Resources.Load<UITheme>("DefaultUITheme");
                if (theme == null) return;
            }

            switch (elementType)
            {
                case ElementType.Button:
                    var button = GetComponent<Button>();
                    if (button != null)
                        theme.ApplyToButton(button);
                    break;

                case ElementType.Text:
                    var tmp = GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                        theme.ApplyToTextTMP(tmp, textType);
                    break;

                case ElementType.Panel:
                    var image = GetComponent<Image>();
                    if (image != null)
                        theme.ApplyToPanel(image);
                    break;
            }
        }

        /// <summary>
        /// Set theme dynamically
        /// </summary>
        public void SetTheme(UITheme newTheme)
        {
            theme = newTheme;
            ApplyTheme();
        }
    }

    public enum ElementType
    {
        Button,
        Text,
        Panel,
        InputField
    }
}
