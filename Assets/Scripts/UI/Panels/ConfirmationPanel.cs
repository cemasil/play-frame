using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Generic confirmation dialog with Yes/No buttons.
    /// Use for destructive actions like delete save, quit game, etc.
    /// </summary>
    public class ConfirmationPanel : UIPanel
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image iconImage;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        private Action _onConfirm;
        private Action _onCancel;

        protected override void OnInitialize()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);
        }

        /// <summary>
        /// Configure the confirmation dialog.
        /// </summary>
        public void Setup(string title, string message, Action onConfirm, Action onCancel = null,
            string confirmLabel = "Yes", string cancelLabel = "No", Sprite icon = null)
        {
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;
            if (confirmButtonText != null) confirmButtonText.text = confirmLabel;
            if (cancelButtonText != null) cancelButtonText.text = cancelLabel;

            if (iconImage != null)
            {
                bool hasIcon = icon != null;
                iconImage.gameObject.SetActive(hasIcon);
                if (hasIcon) iconImage.sprite = icon;
            }

            _onConfirm = onConfirm;
            _onCancel = onCancel;
        }

        private void OnConfirmClicked()
        {
            var callback = _onConfirm;
            Close();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            var callback = _onCancel;
            Close();
            callback?.Invoke();
        }

        private void Close()
        {
            _onConfirm = null;
            _onCancel = null;

            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);
        }

        protected override void OnCleanup()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
            if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
        }
    }
}
