using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Level failed panel with retry options, extra moves (via ad/coins/IAP), and give-up.
    /// Supports sub-states for different failure recovery flows.
    /// </summary>
    public class LevelFailedPanel : UIPanel
    {
        public enum SubState { Main, ExtraMoves, WatchAd, GiveUp }

        [Header("Main View")]
        [SerializeField] private GameObject mainView;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Extra Moves View")]
        [SerializeField] private GameObject extraMovesView;
        [SerializeField] private TextMeshProUGUI extraMovesCountText;
        [SerializeField] private TextMeshProUGUI extraMovesCostText;

        [Header("Watch Ad View")]
        [SerializeField] private GameObject watchAdView;
        [SerializeField] private TextMeshProUGUI adRewardText;

        [Header("Give Up View")]
        [SerializeField] private GameObject giveUpView;
        [SerializeField] private TextMeshProUGUI giveUpMessageText;

        [Header("Main Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button extraMovesButton;
        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button giveUpButton;

        [Header("Sub-View Buttons")]
        [SerializeField] private Button buyExtraMovesButton;
        [SerializeField] private Button buyExtraMovesCoinsButton;
        [SerializeField] private Button buyExtraMovesIAPButton;
        [SerializeField] private Button confirmWatchAdButton;
        [SerializeField] private Button confirmGiveUpButton;
        [SerializeField] private Button cancelSubViewButton;

        private SubState _currentSubState;

        /// <summary>Called when player taps Retry</summary>
        public event Action OnRetry;

        /// <summary>Called when player taps Home</summary>
        public event Action OnHome;

        /// <summary>Called when player requests extra moves via coins</summary>
        public event Action OnBuyExtraMovesCoins;

        /// <summary>Called when player requests extra moves via IAP</summary>
        public event Action OnBuyExtraMovesIAP;

        /// <summary>Called when player wants to watch ad for extra moves/reward</summary>
        public event Action OnWatchAd;

        /// <summary>Called when player confirms give up</summary>
        public event Action OnGiveUp;

        protected override void OnInitialize()
        {
            // Main buttons
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRetry?.Invoke());
            if (homeButton != null) homeButton.onClick.AddListener(() => OnHome?.Invoke());
            if (extraMovesButton != null) extraMovesButton.onClick.AddListener(() => SetSubState(SubState.ExtraMoves));
            if (watchAdButton != null) watchAdButton.onClick.AddListener(() => SetSubState(SubState.WatchAd));
            if (giveUpButton != null) giveUpButton.onClick.AddListener(() => SetSubState(SubState.GiveUp));

            // Sub-view buttons
            if (buyExtraMovesCoinsButton != null) buyExtraMovesCoinsButton.onClick.AddListener(() => OnBuyExtraMovesCoins?.Invoke());
            if (buyExtraMovesIAPButton != null) buyExtraMovesIAPButton.onClick.AddListener(() => OnBuyExtraMovesIAP?.Invoke());
            if (confirmWatchAdButton != null) confirmWatchAdButton.onClick.AddListener(() => OnWatchAd?.Invoke());
            if (confirmGiveUpButton != null) confirmGiveUpButton.onClick.AddListener(() => OnGiveUp?.Invoke());
            if (cancelSubViewButton != null) cancelSubViewButton.onClick.AddListener(() => SetSubState(SubState.Main));
        }

        protected override void OnShow()
        {
            SetSubState(SubState.Main);
        }

        /// <summary>
        /// Configure the level failed display.
        /// </summary>
        public void Setup(int level, string message = null)
        {
            if (levelText != null) levelText.text = $"Level {level}";
            if (messageText != null) messageText.text = message ?? "Out of moves!";
        }

        /// <summary>
        /// Configure extra moves offer.
        /// </summary>
        public void SetupExtraMoves(int movesCount, int coinsCost)
        {
            if (extraMovesCountText != null) extraMovesCountText.text = $"+{movesCount} Moves";
            if (extraMovesCostText != null) extraMovesCostText.text = coinsCost.ToString();
        }

        /// <summary>
        /// Configure ad reward display.
        /// </summary>
        public void SetupAdReward(string rewardDescription)
        {
            if (adRewardText != null) adRewardText.text = rewardDescription;
        }

        /// <summary>
        /// Show/hide specific recovery options.
        /// </summary>
        public void SetOptionsVisibility(bool showExtraMoves, bool showWatchAd, bool showGiveUp)
        {
            if (extraMovesButton != null) extraMovesButton.gameObject.SetActive(showExtraMoves);
            if (watchAdButton != null) watchAdButton.gameObject.SetActive(showWatchAd);
            if (giveUpButton != null) giveUpButton.gameObject.SetActive(showGiveUp);
        }

        /// <summary>
        /// Switch between sub-states.
        /// </summary>
        public void SetSubState(SubState state)
        {
            _currentSubState = state;

            if (mainView != null) mainView.SetActive(state == SubState.Main);
            if (extraMovesView != null) extraMovesView.SetActive(state == SubState.ExtraMoves);
            if (watchAdView != null) watchAdView.SetActive(state == SubState.WatchAd);
            if (giveUpView != null) giveUpView.SetActive(state == SubState.GiveUp);
        }

        protected override void OnCleanup()
        {
            if (retryButton != null) retryButton.onClick.RemoveAllListeners();
            if (homeButton != null) homeButton.onClick.RemoveAllListeners();
            if (extraMovesButton != null) extraMovesButton.onClick.RemoveAllListeners();
            if (watchAdButton != null) watchAdButton.onClick.RemoveAllListeners();
            if (giveUpButton != null) giveUpButton.onClick.RemoveAllListeners();
            if (buyExtraMovesCoinsButton != null) buyExtraMovesCoinsButton.onClick.RemoveAllListeners();
            if (buyExtraMovesIAPButton != null) buyExtraMovesIAPButton.onClick.RemoveAllListeners();
            if (confirmWatchAdButton != null) confirmWatchAdButton.onClick.RemoveAllListeners();
            if (confirmGiveUpButton != null) confirmGiveUpButton.onClick.RemoveAllListeners();
            if (cancelSubViewButton != null) cancelSubViewButton.onClick.RemoveAllListeners();
        }
    }
}
