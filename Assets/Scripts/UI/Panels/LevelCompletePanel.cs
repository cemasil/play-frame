using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFrame.UI.Base;

namespace PlayFrame.UI.Panels
{
    /// <summary>
    /// Level complete panel with reward display, star rating, and next actions.
    /// Supports sub-states: main complete view, rewards detail, booster earned.
    /// </summary>
    public class LevelCompletePanel : UIPanel
    {
        public enum SubState { Main, Rewards, Booster }

        [Header("Main View")]
        [SerializeField] private GameObject mainView;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Stars")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite starFilled;
        [SerializeField] private Sprite starEmpty;

        [Header("Rewards View")]
        [SerializeField] private GameObject rewardsView;
        [SerializeField] private TextMeshProUGUI coinsEarnedText;
        [SerializeField] private TextMeshProUGUI bonusText;

        [Header("Booster View")]
        [SerializeField] private GameObject boosterView;
        [SerializeField] private Image boosterIcon;
        [SerializeField] private TextMeshProUGUI boosterNameText;
        [SerializeField] private TextMeshProUGUI boosterDescText;

        [Header("Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button claimRewardsButton;
        [SerializeField] private Button claimBoosterButton;
        [SerializeField] private Button doubleRewardsButton;

        private SubState _currentSubState;

        /// <summary>Called when player taps Next Level</summary>
        public event Action OnNextLevel;

        /// <summary>Called when player taps Replay</summary>
        public event Action OnReplay;

        /// <summary>Called when player taps Home</summary>
        public event Action OnHome;

        /// <summary>Called when player taps Claim Rewards</summary>
        public event Action OnClaimRewards;

        /// <summary>Called when player taps Claim Booster</summary>
        public event Action OnClaimBooster;

        /// <summary>Called when player taps Double Rewards (ad)</summary>
        public event Action OnDoubleRewards;

        protected override void OnInitialize()
        {
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(() => OnNextLevel?.Invoke());
            if (replayButton != null) replayButton.onClick.AddListener(() => OnReplay?.Invoke());
            if (homeButton != null) homeButton.onClick.AddListener(() => OnHome?.Invoke());
            if (claimRewardsButton != null) claimRewardsButton.onClick.AddListener(() => OnClaimRewards?.Invoke());
            if (claimBoosterButton != null) claimBoosterButton.onClick.AddListener(() => OnClaimBooster?.Invoke());
            if (doubleRewardsButton != null) doubleRewardsButton.onClick.AddListener(() => OnDoubleRewards?.Invoke());
        }

        protected override void OnShow()
        {
            SetSubState(SubState.Main);
        }

        /// <summary>
        /// Configure the level complete display.
        /// </summary>
        public void Setup(int level, int score, int starCount, int coinsEarned, int bonus = 0)
        {
            if (levelText != null) levelText.text = $"Level {level}";
            if (scoreText != null) scoreText.text = score.ToString("N0");
            if (coinsEarnedText != null) coinsEarnedText.text = $"+{coinsEarned}";
            if (bonusText != null)
            {
                bonusText.text = bonus > 0 ? $"+{bonus} Bonus" : "";
                bonusText.gameObject.SetActive(bonus > 0);
            }

            SetStars(starCount);
        }

        /// <summary>
        /// Configure booster earned display.
        /// </summary>
        public void SetupBooster(Sprite icon, string boosterName, string description)
        {
            if (boosterIcon != null) boosterIcon.sprite = icon;
            if (boosterNameText != null) boosterNameText.text = boosterName;
            if (boosterDescText != null) boosterDescText.text = description;
        }

        /// <summary>
        /// Switch between sub-states (Main, Rewards, Booster).
        /// </summary>
        public void SetSubState(SubState state)
        {
            _currentSubState = state;

            if (mainView != null) mainView.SetActive(state == SubState.Main);
            if (rewardsView != null) rewardsView.SetActive(state == SubState.Rewards);
            if (boosterView != null) boosterView.SetActive(state == SubState.Booster);
        }

        private void SetStars(int count)
        {
            if (starImages == null) return;

            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                    starImages[i].sprite = i < count ? starFilled : starEmpty;
            }
        }

        protected override void OnCleanup()
        {
            if (nextLevelButton != null) nextLevelButton.onClick.RemoveAllListeners();
            if (replayButton != null) replayButton.onClick.RemoveAllListeners();
            if (homeButton != null) homeButton.onClick.RemoveAllListeners();
            if (claimRewardsButton != null) claimRewardsButton.onClick.RemoveAllListeners();
            if (claimBoosterButton != null) claimBoosterButton.onClick.RemoveAllListeners();
            if (doubleRewardsButton != null) doubleRewardsButton.onClick.RemoveAllListeners();
        }
    }
}
