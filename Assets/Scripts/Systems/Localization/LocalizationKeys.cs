namespace MiniGameFramework.Systems.Localization
{
    /// <summary>
    /// Predefined localization keys used throughout the framework.
    /// Using const strings instead of enum for flexibility and performance.
    /// New keys can be added here or used directly as strings.
    /// </summary>
    public static class LocalizationKeys
    {
        // Common UI
        public const string MOVES = "ui.moves";
        public const string SCORE = "ui.score";
        public const string TIME = "ui.time";
        public const string TARGET = "ui.target";
        public const string BEST_TIME = "ui.best_time";
        public const string HIGH_SCORE = "ui.high_score";
        public const string FINAL_SCORE = "ui.final_score";
        public const string PAIRS = "ui.pairs";
        public const string UI_MAIN_MENU = "ui.main_menu";
        public const string SELECT_GAME = "ui.select_game";
        public const string LOADING = "ui.loading";

        // Game Results
        public const string YOU_WIN = "result.win";
        public const string GAME_OVER = "result.game_over";
        public const string LEVEL_COMPLETE = "result.level_complete";
        public const string NEW_HIGH_SCORE = "result.new_high_score";

        // Buttons
        public const string PLAY = "button.play";
        public const string RESTART = "button.restart";
        public const string RESUME = "button.resume";
        public const string PAUSE = "button.pause";
        public const string QUIT = "button.quit";
        public const string MAIN_MENU = "button.main_menu";
        public const string SETTINGS = "button.settings";
        public const string NEXT_LEVEL = "button.next_level";
        public const string TRY_AGAIN = "button.try_again";
        public const string BACK = "button.back";

        // Settings
        public const string MUSIC = "settings.music";
        public const string SFX = "settings.sfx";
        public const string LANGUAGE = "settings.language";

        // Game Names
        public const string MATCH3_GAME = "game.match3";
        public const string MEMORY_GAME = "game.memory";
    }
}
