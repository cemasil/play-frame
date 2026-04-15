namespace PlayFrame.UI
{
    /// <summary>
    /// Panel ID constants for use with PanelManager.
    /// </summary>
    public static class PanelIds
    {
        // Core Flow Panels
        public const string MAIN_MENU = "MainMenu";
        public const string GAME_SELECTION = "GameSelection";
        public const string LOADING = "Loading";
        public const string SETTINGS = "Settings";

        // Level Flow Panels
        public const string LEVEL_COMPLETE = "LevelComplete";
        public const string LEVEL_COMPLETE_REWARDS = "LevelCompleteRewards";
        public const string LEVEL_COMPLETE_BOOSTER = "LevelCompleteBooster";
        public const string LEVEL_FAILED = "LevelFailed";
        public const string LEVEL_FAILED_EXTRA_MOVES = "LevelFailedExtraMoves";
        public const string LEVEL_FAILED_WATCH_AD = "LevelFailedWatchAd";
        public const string LEVEL_FAILED_GIVE_UP = "LevelFailedGiveUp";

        // IAP Panels
        public const string IAP = "IAP";
        public const string IAP_SUCCESS = "IAPSuccess";
        public const string IAP_ERROR = "IAPError";
        public const string IAP_RESTORE_SUCCESS = "IAPRestoreSuccess";
        public const string IAP_RESTORE_ERROR = "IAPRestoreError";

        // Tutorial
        public const string TUTORIAL = "Tutorial";

        // Info / Offers
        public const string INFO = "Info";
        public const string SPECIAL_OFFER = "SpecialOffer";

        // Common
        public const string PAUSE = "Pause";
        public const string CONFIRMATION = "Confirmation";
    }
}
