using System.Text;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Analytics provider that outputs events to the Unity console.
    /// Useful for development and debugging.
    /// </summary>
    public class ConsoleAnalyticsProvider : BaseAnalyticsProvider
    {
        private readonly bool _verboseLogging;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public override string ProviderId => "ConsoleAnalytics";

        public ConsoleAnalyticsProvider(bool verboseLogging = false)
        {
            _verboseLogging = verboseLogging;
            InitializeLogger(verboseLogging);
        }

        public override void Initialize()
        {
            Logger.Log("Provider initialized");
        }

        public override void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            if (!_verboseLogging) return;

            _stringBuilder.Clear();
            _stringBuilder.Append($"Event: {analyticsEvent.EventName}");

            if (analyticsEvent.Parameters.Count > 0)
            {
                _stringBuilder.Append(" | Params: ");
                foreach (var param in analyticsEvent.Parameters)
                {
                    _stringBuilder.Append($"{param.Key}={param.Value}, ");
                }
            }

            Logger.Log(_stringBuilder.ToString());
        }

        public override void TrackLevelCompleted(LevelCompletedEvent levelEvent)
        {
            Logger.Log($"✓ Level Completed: {levelEvent.GameName} - Level {levelEvent.LevelNumber} | " +
                     $"Score: {levelEvent.Score}, Time: {levelEvent.CompletionTimeSeconds:F1}s, Moves: {levelEvent.MoveCount}, " +
                     $"Retries: {levelEvent.RetryCount}, NewHighScore: {levelEvent.IsNewHighScore}");
        }

        public override void TrackLevelStarted(LevelStartedEvent levelEvent)
        {
            Logger.Log($"► Level Started: {levelEvent.GameName} - Level {levelEvent.LevelNumber} | " +
                     $"Difficulty: {levelEvent.Difficulty}, Attempt: {levelEvent.AttemptNumber}");
        }

        public override void TrackLevelFailed(LevelFailedEvent levelEvent)
        {
            Logger.Log($"✗ Level Failed: {levelEvent.GameName} - Level {levelEvent.LevelNumber} | " +
                     $"Reason: {levelEvent.FailReason}, Time: {levelEvent.PlayTimeSeconds:F1}s, Score: {levelEvent.Score}");
        }

        public override void TrackSessionStart(SessionStartEvent sessionEvent)
        {
            Logger.Log($"● Session Started: {sessionEvent.SessionId} | " +
                     $"Game: {sessionEvent.GameName}, Version: {sessionEvent.GameVersion}, Platform: {sessionEvent.Platform}");
        }

        public override void TrackSessionEnd(SessionEndEvent sessionEvent)
        {
            Logger.Log($"○ Session Ended: {sessionEvent.SessionId} | " +
                     $"Duration: {sessionEvent.SessionDurationSeconds:F1}s, Levels: {sessionEvent.LevelsPlayed}, Score: {sessionEvent.TotalScore}");
        }

        public override void SetUserProperty(string propertyName, string value)
        {
            Logger.Log($"User Property: {propertyName} = {value}");
        }

        public override void Shutdown()
        {
            Logger.Log("Provider shutdown");
        }
    }
}
