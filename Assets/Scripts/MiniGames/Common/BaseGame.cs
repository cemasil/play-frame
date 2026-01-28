using UnityEngine;
using PlayFrame.Core;
using PlayFrame.Core.StateMachine;

namespace PlayFrame.MiniGames.Common
{
    /// <summary>
    /// Base class for all mini-games with built-in state machine
    /// </summary>
    public abstract class BaseGame : MonoBehaviour, IInitializable, IUpdatable
    {
        protected StateMachine<GameState> StateMachine { get; private set; }

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
