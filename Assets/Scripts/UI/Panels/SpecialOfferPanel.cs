using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Special offer / time-limited deal panel.
    /// Supports countdown timer, product display, and purchase flow.
    /// </summary>
    public class SpecialOfferPanel : UIPanel
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image offerImage;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI originalPriceText;
        [SerializeField] private TextMeshProUGUI discountBadgeText;

        [Header("Timer")]
        [SerializeField] private GameObject timerContainer;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Reward Preview")]
        [SerializeField] private Transform rewardPreviewContainer;

        [Header("Buttons")]
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI purchaseButtonText;

        private string _productId;
        private DateTime _expirationTime;
        private CancellationTokenSource _timerCts;

        /// <summary>Called when purchase is requested (product ID)</summary>
        public event Action<string> OnPurchaseRequested;

        /// <summary>Called when offer is dismissed</summary>
        public event Action OnDismissed;

        /// <summary>Called when offer timer expires</summary>
        public event Action OnExpired;

        protected override void OnInitialize()
        {
            if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        /// <summary>
        /// Configure the offer display.
        /// </summary>
        public void Setup(string title, string description, string price, string productId,
            Sprite image = null, string originalPrice = null, int discountPercent = 0)
        {
            _productId = productId;

            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
            if (priceText != null) priceText.text = price;
            if (purchaseButtonText != null) purchaseButtonText.text = price;

            if (offerImage != null && image != null)
            {
                offerImage.sprite = image;
                offerImage.gameObject.SetActive(true);
            }

            if (originalPriceText != null)
            {
                bool hasOriginal = !string.IsNullOrEmpty(originalPrice);
                originalPriceText.gameObject.SetActive(hasOriginal);
                if (hasOriginal) originalPriceText.text = $"<s>{originalPrice}</s>";
            }

            if (discountBadgeText != null)
            {
                bool hasDiscount = discountPercent > 0;
                discountBadgeText.gameObject.SetActive(hasDiscount);
                if (hasDiscount) discountBadgeText.text = $"-{discountPercent}%";
            }
        }

        /// <summary>
        /// Start a countdown timer. The panel will auto-close when time expires.
        /// </summary>
        public void StartTimer(TimeSpan duration)
        {
            _expirationTime = DateTime.UtcNow + duration;
            StopTimer();
            _timerCts = new CancellationTokenSource();

            if (timerContainer != null) timerContainer.SetActive(true);
            RunTimerAsync(_timerCts.Token).Forget();
        }

        /// <summary>
        /// Start a countdown timer with absolute expiration.
        /// </summary>
        public void StartTimer(DateTime expirationUtc)
        {
            _expirationTime = expirationUtc;
            StopTimer();
            _timerCts = new CancellationTokenSource();

            if (timerContainer != null) timerContainer.SetActive(true);
            RunTimerAsync(_timerCts.Token).Forget();
        }

        private async UniTaskVoid RunTimerAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var remaining = _expirationTime - DateTime.UtcNow;

                    if (remaining.TotalSeconds <= 0)
                    {
                        if (timerText != null) timerText.text = "00:00:00";
                        OnExpired?.Invoke();
                        OnCloseClicked();
                        break;
                    }

                    if (timerText != null)
                    {
                        if (remaining.TotalHours >= 1)
                            timerText.text = $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                        else
                            timerText.text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                    }

                    await UniTask.WaitForSeconds(1f, cancellationToken: ct);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void StopTimer()
        {
            if (_timerCts != null)
            {
                _timerCts.Cancel();
                _timerCts.Dispose();
                _timerCts = null;
            }
        }

        private void OnPurchaseClicked()
        {
            if (!string.IsNullOrEmpty(_productId))
                OnPurchaseRequested?.Invoke(_productId);
        }

        private void OnCloseClicked()
        {
            StopTimer();
            OnDismissed?.Invoke();

            if (PanelManager.HasInstance)
                PanelManager.Instance.HidePanel(this);
        }

        protected override void OnHide()
        {
            StopTimer();
        }

        protected override void OnCleanup()
        {
            StopTimer();
            if (purchaseButton != null) purchaseButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        }
    }
}
