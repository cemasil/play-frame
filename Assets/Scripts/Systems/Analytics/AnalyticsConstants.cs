namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Centralized analytics event name constants.
    /// Use these instead of hardcoded strings for consistency.
    /// </summary>
    public static class AnalyticsEventNames
    {
        // Session Events
        public const string SESSION_START = "session_start";
        public const string SESSION_END = "session_end";

        // Level Events
        public const string LEVEL_STARTED = "level_started";
        public const string LEVEL_COMPLETED = "level_completed";
        public const string LEVEL_FAILED = "level_failed";
        public const string LEVEL_RETRY = "level_retry";

        // Gameplay Events
        public const string GAME_MOVE = "game_move";
        public const string MATCH_MADE = "match_made";
        public const string GAME_PAUSED = "game_paused";
        public const string GAME_RESUMED = "game_resumed";

        // Progress Events
        public const string MILESTONE_REACHED = "milestone_reached";
        public const string HIGH_SCORE = "high_score";
        public const string ACHIEVEMENT_UNLOCKED = "achievement_unlocked";

        // UI Events
        public const string UI_INTERACTION = "ui_interaction";
        public const string SCREEN_VIEW = "screen_view";
        public const string TUTORIAL_STEP = "tutorial_step";

        // Economy Events (for future use)
        public const string PURCHASE_STARTED = "purchase_started";
        public const string PURCHASE_COMPLETED = "purchase_completed";
        public const string CURRENCY_EARNED = "currency_earned";
        public const string CURRENCY_SPENT = "currency_spent";

        // Error Events
        public const string ERROR_OCCURRED = "error_occurred";
        public const string CRASH_OCCURRED = "crash_occurred";
    }

    /// <summary>
    /// Common parameter names for analytics events
    /// </summary>
    public static class AnalyticsParameterNames
    {
        // Common Parameters
        public const string GAME_NAME = "game_name";
        public const string LEVEL_NUMBER = "level_number";
        public const string LEVEL_ID = "level_id";
        public const string SCORE = "score";
        public const string TIME_SECONDS = "time_seconds";
        public const string MOVE_COUNT = "move_count";
        public const string RETRY_COUNT = "retry_count";
        public const string DIFFICULTY = "difficulty";

        // Session Parameters
        public const string SESSION_ID = "session_id";
        public const string SESSION_DURATION = "session_duration";
        public const string LEVELS_PLAYED = "levels_played";

        // Match Parameters
        public const string MATCH_SIZE = "match_size";
        public const string MATCH_TYPE = "match_type";
        public const string COMBO_COUNT = "combo_count";

        // Progress Parameters
        public const string HIGH_SCORE_NEW = "high_score_new";
        public const string HIGH_SCORE_PREVIOUS = "high_score_previous";
        public const string STARS = "stars";

        // Fail Parameters
        public const string FAIL_REASON = "fail_reason";

        // UI Parameters
        public const string SCREEN_NAME = "screen_name";
        public const string ELEMENT_ID = "element_id";
        public const string ELEMENT_TYPE = "element_type";
    }

    /// <summary>
    /// Common fail reasons for level_failed events
    /// </summary>
    public static class FailReasons
    {
        public const string OUT_OF_MOVES = "out_of_moves";
        public const string OUT_OF_TIME = "out_of_time";
        public const string TARGET_NOT_MET = "target_not_met";
        public const string USER_QUIT = "user_quit";
        public const string GAME_OVER = "game_over";
    }

    /// <summary>
    /// Common pause reasons for game_paused events
    /// </summary>
    public static class PauseReasons
    {
        public const string USER_INITIATED = "user_initiated";
        public const string APP_BACKGROUND = "app_background";
        public const string PHONE_CALL = "phone_call";
        public const string NOTIFICATION = "notification";
        public const string SYSTEM = "system";
    }
}
