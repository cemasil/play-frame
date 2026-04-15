using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// In-App Purchase panel with store view and sub-states for purchase/restore flows.
    /// Supports configurable product items via IAPProductEntry.
    /// </summary>
    public class IAPPanel : UIPanel
    {
        public enum SubState { Store, PurchaseSuccess, PurchaseError, RestoreSuccess, RestoreError }

        [Header("Store View")]
        [SerializeField] private GameObject storeView;
        [SerializeField] private Transform productContainer;
        [SerializeField] private GameObject productItemPrefab;

        [Header("Purchase Success View")]
        [SerializeField] private GameObject purchaseSuccessView;
        [SerializeField] private TextMeshProUGUI purchaseSuccessText;
        [SerializeField] private Image purchaseSuccessIcon;

        [Header("Purchase Error View")]
        [SerializeField] private GameObject purchaseErrorView;
        [SerializeField] private TextMeshProUGUI purchaseErrorText;

        [Header("Restore Success View")]
        [SerializeField] private GameObject restoreSuccessView;
        [SerializeField] private TextMeshProUGUI restoreSuccessText;

        [Header("Restore Error View")]
        [SerializeField] private GameObject restoreErrorView;
        [SerializeField] private TextMeshProUGUI restoreErrorText;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button okSuccessButton;
        [SerializeField] private Button okErrorButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button okRestoreSuccessButton;
        [SerializeField] private Button okRestoreErrorButton;

        private SubState _currentSubState;

        /// <summary>Called when a product purchase is requested (product ID)</summary>
        public event Action<string> OnPurchaseRequested;

        /// <summary>Called when restore purchases is requested</summary>
        public event Action OnRestoreRequested;

        /// <summary>Called when panel is closed</summary>
        public event Action OnClosed;

        protected override void OnInitialize()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (restoreButton != null) restoreButton.onClick.AddListener(() => OnRestoreRequested?.Invoke());
            if (okSuccessButton != null) okSuccessButton.onClick.AddListener(Close);
            if (okErrorButton != null) okErrorButton.onClick.AddListener(() => SetSubState(SubState.Store));
            if (retryButton != null) retryButton.onClick.AddListener(() => SetSubState(SubState.Store));
            if (okRestoreSuccessButton != null) okRestoreSuccessButton.onClick.AddListener(Close);
            if (okRestoreErrorButton != null) okRestoreErrorButton.onClick.AddListener(() => SetSubState(SubState.Store));
        }

        protected override void OnShow()
        {
            SetSubState(SubState.Store);
        }

        /// <summary>
        /// Show purchase success feedback.
        /// </summary>
        public void ShowPurchaseSuccess(string productName, Sprite icon = null)
        {
            if (purchaseSuccessText != null) purchaseSuccessText.text = $"{productName} purchased!";
            if (purchaseSuccessIcon != null && icon != null)
            {
                purchaseSuccessIcon.sprite = icon;
                purchaseSuccessIcon.gameObject.SetActive(true);
            }
            SetSubState(SubState.PurchaseSuccess);
        }

        /// <summary>
        /// Show purchase error feedback.
        /// </summary>
        public void ShowPurchaseError(string errorMessage)
        {
            if (purchaseErrorText != null) purchaseErrorText.text = errorMessage;
            SetSubState(SubState.PurchaseError);
        }

        /// <summary>
        /// Show restore success feedback.
        /// </summary>
        public void ShowRestoreSuccess(int restoredCount)
        {
            if (restoreSuccessText != null)
                restoreSuccessText.text = $"{restoredCount} purchase(s) restored!";
            SetSubState(SubState.RestoreSuccess);
        }

        /// <summary>
        /// Show restore error feedback.
        /// </summary>
        public void ShowRestoreError(string errorMessage)
        {
            if (restoreErrorText != null) restoreErrorText.text = errorMessage;
            SetSubState(SubState.RestoreError);
        }

        /// <summary>
        /// Switch between sub-states.
        /// </summary>
        public void SetSubState(SubState state)
        {
            _currentSubState = state;

            if (storeView != null) storeView.SetActive(state == SubState.Store);
            if (purchaseSuccessView != null) purchaseSuccessView.SetActive(state == SubState.PurchaseSuccess);
            if (purchaseErrorView != null) purchaseErrorView.SetActive(state == SubState.PurchaseError);
            if (restoreSuccessView != null) restoreSuccessView.SetActive(state == SubState.RestoreSuccess);
            if (restoreErrorView != null) restoreErrorView.SetActive(state == SubState.RestoreError);
        }

        /// <summary>
        /// Request purchase of a product (called from product item buttons).
        /// </summary>
        public void RequestPurchase(string productId)
        {
            OnPurchaseRequested?.Invoke(productId);
        }

        private void Close()
        {
            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);

            OnClosed?.Invoke();
        }

        protected override void OnCleanup()
        {
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
            if (restoreButton != null) restoreButton.onClick.RemoveAllListeners();
            if (okSuccessButton != null) okSuccessButton.onClick.RemoveAllListeners();
            if (okErrorButton != null) okErrorButton.onClick.RemoveAllListeners();
            if (retryButton != null) retryButton.onClick.RemoveAllListeners();
            if (okRestoreSuccessButton != null) okRestoreSuccessButton.onClick.RemoveAllListeners();
            if (okRestoreErrorButton != null) okRestoreErrorButton.onClick.RemoveAllListeners();
        }
    }
}
