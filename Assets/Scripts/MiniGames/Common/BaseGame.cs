using UnityEngine;
using PlayFrame.Core;
using PlayFrame.Core.StateMachine;
using PlayFrame.Systems.Analytics;

namespace PlayFrame.MiniGames.Common
{
    /// <summary>
    /// Base class for all mini-games with built-in state machine and analytics support
    /// </summary>
    public abstract class BaseGame : MonoBehaviour, IInitializable, IUpdatable
    {
        protected StateMachine<GameState> StateMachine { get; private set; }

        #region Analytics Properties

        /// <summary>
        /// Override this to specify the game name for analytics
        /// </summary>
        protected virtual string GameName => GetType().Name;

        /// <summary>
        /// Override this to specify the current level number
        /// </summary>
        protected virtual int CurrentLevel => 1;

        /// <summary>
        /// Override this to specify the current difficulty
        /// </summary>
        protected virtual string Difficulty => "normal";

        /// <summary>
        /// Current retry/attempt count for this level
        /// </summary>
        protected int RetryCount { get; set; } = 0;

        /// <summary>
        /// Time when the current level started
        /// </summary>
        protected float LevelStartTime { get; private set; }

        /// <summary>
        /// Time when the game was paused (for calculating pause duration)
        /// </summary>
        private float _pauseStartTime;

        /// <summary>
        /// Total time spent paused during this session
        /// </summary>
        private float _totalPausedTime;

        /// <summary>
        /// Whether analytics tracking is enabled for this game
        /// </summary>
        protected virtual bool EnableAnalytics => true;

        #endregion

        /// <summary>
        /// Current game state
        /// </summary>
        public GameState CurrentState => StateMachine?.CurrentState ?? GameState.Initializing;

        /// <summary>
        /// Check if game is in Playing state
        /// </summary>
        public bool IsPlaying => StateMachine?.IsInState(GameState.Playing) ?? false;

        /// <summary>
        /// Check if game is in Processing state (input should be blocked)
        /// </summary>
        public bool IsProcessing => StateMachine?.IsInState(GameState.Processing) ?? false;

        /// <summary>
        /// Check if game is paused
        /// </summary>
        public bool IsPaused => StateMachine?.IsInState(GameState.Paused) ?? false;

        /// <summary>
        /// Check if game is over
        /// </summary>
        public bool IsGameOver => StateMachine?.IsInState(GameState.GameOver) ?? false;

        /// <summary>
        /// Check if player input should be accepted
        /// </summary>
        public bool CanAcceptInput => IsPlaying && !IsProcessing && !IsPaused && !IsGameOver;

        private void Awake()
        {
            InitializeStateMachine();
            Initialize();
        }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine<GameState>();
            StateMachine.OnStateChanged += HandleStateChanged;
            RegisterStates();
        }

        /// <summary>
        /// Register custom states. Override to add game-specific states.
        /// </summary>
        protected virtual void RegisterStates()
        {
            // Default simple states - games can override with custom implementations
            StateMachine.RegisterState(GameState.Initializing, new SimpleState(OnStateEnterInitializing, null, null));
            StateMachine.RegisterState(GameState.Ready, new SimpleState(OnStateEnterReady, null, null));
            StateMachine.RegisterState(GameState.Playing, new SimpleState(OnStateEnterPlaying, OnStateUpdatePlaying, OnStateExitPlaying));
            StateMachine.RegisterState(GameState.Processing, new SimpleState(OnStateEnterProcessing, null, OnStateExitProcessing));
            StateMachine.RegisterState(GameState.Paused, new SimpleState(OnStateEnterPaused, null, OnStateExitPaused));
            StateMachine.RegisterState(GameState.GameOver, new SimpleState(OnStateEnterGameOver, null, null));
        }

        public void Initialize()
        {
            StateMachine.SetInitialState(GameState.Initializing);
            OnInitialize();
            SetState(GameState.Ready);
        }

        private void Start()
        {
            OnGameStart();
            SetState(GameState.Playing);

            // Track level start
            TrackLevelStarted();
        }

        public void Update()
        {
            StateMachine?.Update();
            OnUpdate();
        }

        protected virtual void OnDestroy()
        {
            if (StateMachine != null)
            {
                StateMachine.OnStateChanged -= HandleStateChanged;
            }
            Cleanup();
        }

        /// <summary>
        /// Change to a new state
        /// </summary>
        protected void SetState(GameState newState)
        {
            StateMachine?.ChangeState(newState);
        }

        /// <summary>
        /// Start processing (blocks input)
        /// </summary>
        protected void BeginProcessing()
        {
            SetState(GameState.Processing);
        }

        /// <summary>
        /// End processing (resumes input)
        /// </summary>
        protected void EndProcessing()
        {
            if (IsProcessing)
            {
                SetState(GameState.Playing);
            }
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public virtual void Pause()
        {
            if (IsPlaying || IsProcessing)
            {
                _pauseStartTime = Time.time;
                TrackGamePaused();
                SetState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public virtual void Resume()
        {
            if (IsPaused)
            {
                float pauseDuration = Time.time - _pauseStartTime;
                _totalPausedTime += pauseDuration;
                TrackGameResumed(pauseDuration);
                SetState(GameState.Playing);
            }
        }

        /// <summary>
        /// End the game
        /// </summary>
        protected void EndGame()
        {
            SetState(GameState.GameOver);
        }

        // State change handler
        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            OnStateChanged(previousState, newState);
        }

        // Virtual methods for state transitions
        protected virtual void OnStateChanged(GameState previousState, GameState newState) { }
        protected virtual void OnStateEnterInitializing() { }
        protected virtual void OnStateEnterReady() { }
        protected virtual void OnStateEnterPlaying() { }
        protected virtual void OnStateUpdatePlaying() { }
        protected virtual void OnStateExitPlaying() { }
        protected virtual void OnStateEnterProcessing() { }
        protected virtual void OnStateExitProcessing() { }
        protected virtual void OnStateEnterPaused() { }
        protected virtual void OnStateExitPaused() { }
        protected virtual void OnStateEnterGameOver() { }

        // Existing virtual methods
        protected virtual void OnInitialize() { }
        protected virtual void OnGameStart() { }
        protected virtual void OnUpdate() { }
        protected virtual void UpdateScore(int score) { }
        protected virtual void ShowGameOver(int finalScore) { }
        protected virtual void Cleanup() { }

        #region Analytics Methods

        /// <summary>
        /// Get the current play time (excluding paused time)
        /// </summary>
        protected float GetCurrentPlayTime()
        {
            float totalTime = Time.time - LevelStartTime;
            return totalTime - _totalPausedTime;
        }

        /// <summary>
        /// Track when a level starts (called automatically)
        /// </summary>
        protected virtual void TrackLevelStarted()
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            LevelStartTime = Time.time;
            _totalPausedTime = 0f;

            AnalyticsManager.Instance.TrackLevelStart(
                GameName,
                CurrentLevel,
                Difficulty,
                RetryCount + 1
            );
        }

        /// <summary>
        /// Track when a level is completed successfully
        /// Call this when the player wins/completes the level
        /// </summary>
        protected virtual void TrackLevelCompleted(int score, int moveCount, bool isNewHighScore = false, int stars = 0, int matchCount = 0)
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            float completionTime = GetCurrentPlayTime();

            AnalyticsManager.Instance.TrackLevelComplete(
                GameName,
                CurrentLevel,
                score,
                completionTime,
                moveCount,
                RetryCount,
                isNewHighScore,
                stars,
                Difficulty,
                matchCount
            );
        }

        /// <summary>
        /// Track when a level fails
        /// Call this when the player loses/fails the level
        /// </summary>
        protected virtual void TrackLevelFailed(string failReason, int score, int moveCount)
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            float playTime = GetCurrentPlayTime();

            AnalyticsManager.Instance.TrackLevelFail(
                GameName,
                CurrentLevel,
                failReason,
                playTime,
                score,
                moveCount,
                RetryCount,
                Difficulty
            );
        }

        /// <summary>
        /// Track when a level is retried
        /// Call this when the player restarts the level
        /// </summary>
        protected virtual void TrackLevelRetried()
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            float timeSinceLastAttempt = GetCurrentPlayTime();
            RetryCount++;

            AnalyticsManager.Instance.TrackLevelRetry(
                GameName,
                CurrentLevel,
                RetryCount,
                timeSinceLastAttempt
            );
        }

        /// <summary>
        /// Track when the game is paused
        /// </summary>
        protected virtual void TrackGamePaused(string pauseReason = "user_initiated")
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            float playTime = GetCurrentPlayTime();
            AnalyticsManager.Instance.TrackGamePaused(GameName, playTime, pauseReason);
        }

        /// <summary>
        /// Track when the game is resumed
        /// </summary>
        protected virtual void TrackGameResumed(float pauseDuration)
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            AnalyticsManager.Instance.TrackGameResumed(GameName, pauseDuration);
        }

        /// <summary>
        /// Track a match made (for match-3 type games)
        /// </summary>
        protected virtual void TrackMatchMade(int matchSize, string matchType = "normal", int comboCount = 1, int pointsEarned = 0)
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            AnalyticsManager.Instance.TrackMatchMade(GameName, matchSize, matchType, comboCount, pointsEarned);
        }

        /// <summary>
        /// Track a game move/action
        /// </summary>
        protected virtual void TrackGameMove(string moveType, int moveNumber, bool wasSuccessful, int pointsEarned = 0)
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            AnalyticsManager.Instance.TrackGameMove(GameName, moveType, moveNumber, wasSuccessful, pointsEarned);
        }

        /// <summary>
        /// Track a new high score
        /// </summary>
        protected virtual void TrackHighScore(int newHighScore, int previousHighScore)
        {
            if (!EnableAnalytics || !AnalyticsManager.HasInstance) return;

            AnalyticsManager.Instance.TrackHighScore(GameName, newHighScore, previousHighScore, CurrentLevel);
        }

        #endregion
    }

    /// <summary>
    /// Simple state implementation using delegates
    /// </summary>
    internal class SimpleState : IState
    {
        private readonly System.Action _onEnter;
        private readonly System.Action _onUpdate;
        private readonly System.Action _onExit;

        public SimpleState(System.Action onEnter, System.Action onUpdate, System.Action onExit)
        {
            _onEnter = onEnter;
            _onUpdate = onUpdate;
            _onExit = onExit;
        }

        public void Enter() => _onEnter?.Invoke();
        public void Update() => _onUpdate?.Invoke();
        public void Exit() => _onExit?.Invoke();
    }
}
