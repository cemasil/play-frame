using System.Collections.Generic;

namespace PlayFrame.Systems.Analytics
{
    /// <summary>
    /// Unity Analytics provider stub.
    /// Uncomment and configure when Unity Analytics package is installed.
    /// 
    /// To use:
    /// 1. Install Unity Analytics package via Package Manager
    /// 2. Enable Unity Services in Project Settings
    /// 3. Uncomment the implementation below
    /// 4. Register this provider with AnalyticsManager
    /// </summary>
    public class UnityAnalyticsProvider : BaseAnalyticsProvider
    {
        public override string ProviderId => "UnityAnalytics";

        public UnityAnalyticsProvider()
        {
            InitializeLogger(false); // Disabled by default for stub
        }

        public override void Initialize()
        {
            // Uncomment when Unity Analytics is configured:
            // Unity.Services.Analytics.AnalyticsService.Instance.StartDataCollection();

            Logger.Log("Provider initialized (stub - configure Unity Analytics to enable)");
        }

        public override void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            if (!IsEnabled) return;

            // Uncomment when Unity Analytics is configured:
            // var parameters = new Dictionary<string, object>();
            // foreach (var param in analyticsEvent.Parameters)
            // {
            //     parameters[param.Key] = param.Value;
            // }
            // Unity.Services.Analytics.AnalyticsService.Instance.CustomData(analyticsEvent.EventName, parameters);
        }

        public override void TrackLevelCompleted(LevelCompletedEvent levelEvent)
        {
            if (!IsEnabled) return;

            // Uncomment when Unity Analytics is configured:
            // var parameters = new Dictionary<string, object>
            // {
            //     { "game_name", levelEvent.GameName },
            //     { "level_number", levelEvent.LevelNumber },
            //     { "score", levelEvent.Score },
            //     { "completion_time", levelEvent.CompletionTimeSeconds },
            //     { "move_count", levelEvent.MoveCount },
            //     { "retry_count", levelEvent.RetryCount },
            //     { "is_new_high_score", levelEvent.IsNewHighScore },
            //     { "stars", levelEvent.Stars }
            // };
            // Unity.Services.Analytics.AnalyticsService.Instance.CustomData("level_completed", parameters);
        }

        public override void TrackLevelFailed(LevelFailedEvent levelEvent)
        {
            if (!IsEnabled) return;

            // Uncomment when Unity Analytics is configured:
            // var parameters = new Dictionary<string, object>
            // {
            //     { "game_name", levelEvent.GameName },
            //     { "level_number", levelEvent.LevelNumber },
            //     { "fail_reason", levelEvent.FailReason },
            //     { "play_time", levelEvent.PlayTimeSeconds },
            //     { "score", levelEvent.Score },
            //     { "move_count", levelEvent.MoveCount }
            // };
            // Unity.Services.Analytics.AnalyticsService.Instance.CustomData("level_failed", parameters);
        }

        public override void SetUserProperty(string propertyName, string value)
        {
            if (!IsEnabled) return;

            // Uncomment when Unity Analytics is configured:
            // Unity.Services.Analytics.AnalyticsService.Instance.SetCustomUserId(value); // for user_id
        }

        public override void Flush()
        {
            // Uncomment when Unity Analytics is configured:
            // Unity.Services.Analytics.AnalyticsService.Instance.Flush();
        }

        public override void Shutdown()
        {
            // Uncomment when Unity Analytics is configured:
            // Unity.Services.Analytics.AnalyticsService.Instance.StopDataCollection();
        }
    }
}
