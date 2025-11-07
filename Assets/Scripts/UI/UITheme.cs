using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MiniGameFramework.UI
{
    /// <summary>
    /// UI Theme configuration using Scriptable Object
    /// Create via: Assets -> Create -> UI -> UI Theme
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "UI/UI Theme", order = 1)]
    public class UITheme : ScriptableObject
    {
        [Header("Colors")]
        public Color primaryColor = new Color(0.2f, 0.6f, 1f);
        public Color secondaryColor = new Color(0.8f, 0.8f, 0.8f);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        public Color textColor = Color.white;
        public Color accentColor = new Color(1f, 0.8f, 0f);

        [Header("Button Colors")]
        public Color buttonNormalColor = Color.white;
        public Color buttonHighlightColor = new Color(1f, 1f, 0.8f);
        public Color buttonPressedColor = new Color(0.8f, 0.8f, 0.6f);
        public Color buttonDisabledColor = new Color(0.5f, 0.5f, 0.5f);

        [Header("Fonts")]
        public Font titleFont;
        public Font bodyFont;
        public TMP_FontAsset tmpTitleFont;
        public TMP_FontAsset tmpBodyFont;

        [Header("Font Sizes")]
        public int titleSize = 48;
        public int headerSize = 36;
        public int bodySize = 24;
        public int buttonTextSize = 32;

        [Header("Spacing")]
        public float smallSpacing = 10f;
        public float mediumSpacing = 20f;
        public float largeSpacing = 40f;

        [Header("Sprites")]
        public Sprite panelBackground;
        public Sprite buttonSprite;
        public Sprite inputFieldSprite;

        public void ApplyToButton(Button button)
        {
            if (button == null) return;

            var colors = button.colors;
            colors.normalColor = buttonNormalColor;
            colors.highlightedColor = buttonHighlightColor;
            colors.pressedColor = buttonPressedColor;
            colors.disabledColor = buttonDisabledColor;
            button.colors = colors;

            if (buttonSprite != null)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                    image.sprite = buttonSprite;
            }

            var tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.color = textColor;
                tmpText.fontSize = buttonTextSize;
                if (tmpBodyFont != null)
                    tmpText.font = tmpBodyFont;
            }
        }

        public void ApplyToPanel(Image background)
        {
            if (background == null) return;

            background.color = backgroundColor;
            if (panelBackground != null)
                background.sprite = panelBackground;
        }

        public void ApplyToTextTMP(TextMeshProUGUI tmp, TextType textType = TextType.Body)
        {
            if (tmp == null) return;

            tmp.color = textColor;

            switch (textType)
            {
                case TextType.Title:
                    tmp.fontSize = titleSize;
                    if (tmpTitleFont != null) tmp.font = tmpTitleFont;
                    break;
                case TextType.Header:
                    tmp.fontSize = headerSize;
                    if (tmpTitleFont != null) tmp.font = tmpTitleFont;
                    break;
                case TextType.Body:
                    tmp.fontSize = bodySize;
                    if (tmpBodyFont != null) tmp.font = tmpBodyFont;
                    break;
            }
        }
    }

    public enum TextType
    {
        Title,
        Header,
        Body
    }
}
