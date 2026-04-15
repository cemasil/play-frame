using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Generic information panel for displaying messages, announcements, or notifications.
    /// Supports title, message, icon, and configurable buttons.
    /// </summary>
    public class InfoPanel : UIPanel
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image iconImage;

        [Header("Buttons")]
        [SerializeField] private Button primaryButton;
        [SerializeField] private TextMeshProUGUI primaryButtonText;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TextMeshProUGUI secondaryButtonText;
        [SerializeField] private Button closeButton;

        private Action _onPrimary;
        private Action _onSecondary;

        protected override void OnInitialize()
        {
            if (primaryButton != null) primaryButton.onClick.AddListener(OnPrimaryClicked);
            if (secondaryButton != null) secondaryButton.onClick.AddListener(OnSecondaryClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        /// <summary>
        /// Configure the info panel with a single OK button.
        /// </summary>
        public void Setup(string title, string message, Sprite icon = null, string buttonLabel = "OK", Action onClose = null)
        {
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;

            if (iconImage != null)
            {
                bool hasIcon = icon != null;
                iconImage.gameObject.SetActive(hasIcon);
                if (hasIcon) iconImage.sprite = icon;
            }

            if (primaryButtonText != null) primaryButtonText.text = buttonLabel;
            if (secondaryButton != null) secondaryButton.gameObject.SetActive(false);

            _onPrimary = onClose ?? Close;
            _onSecondary = null;
        }

        /// <summary>
        /// Configure with two buttons (e.g., "Yes" / "No").
        /// </summary>
        public void Setup(string title, string message, Sprite icon,
            string primaryLabel, Action onPrimary,
            string secondaryLabel, Action onSecondary)
        {
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;

            if (iconImage != null)
            {
                bool hasIcon = icon != null;
                iconImage.gameObject.SetActive(hasIcon);
                if (hasIcon) iconImage.sprite = icon;
            }

            if (primaryButtonText != null) primaryButtonText.text = primaryLabel;
            if (secondaryButtonText != null) secondaryButtonText.text = secondaryLabel;
            if (secondaryButton != null) secondaryButton.gameObject.SetActive(true);

            _onPrimary = onPrimary;
            _onSecondary = onSecondary;
        }

        private void OnPrimaryClicked()
        {
            _onPrimary?.Invoke();
        }

        private void OnSecondaryClicked()
        {
            _onSecondary?.Invoke();
        }

        private void Close()
        {
            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);
        }

        protected override void OnCleanup()
        {
            if (primaryButton != null) primaryButton.onClick.RemoveAllListeners();
            if (secondaryButton != null) secondaryButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        }
    }
}
