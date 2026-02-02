using System;
using System.Collections.Generic;
using PlayFrame.Core.Logging;

namespace PlayFrame.Core.StateMachine
{
    /// <summary>
    /// Generic state machine for managing game states
    /// </summary>
    /// <typeparam name="TState">Enum type for states</typeparam>
    public class StateMachine<TState> where TState : Enum
    {
        private static readonly ILogger _logger = LoggerFactory.CreateCore("StateMachine");
        private readonly Dictionary<TState, IState> _states = new Dictionary<TState, IState>();
        private IState _currentState;
        private TState _currentStateKey;
        private bool _isInitialized;

        /// <summary>
        /// Current state key
        /// </summary>
        public TState CurrentState => _currentStateKey;

        /// <summary>
        /// Event fired when state changes
        /// </summary>
        public event Action<TState, TState> OnStateChanged;

        /// <summary>
        /// Register a state with the state machine
        /// </summary>
        public void RegisterState(TState stateKey, IState state)
        {
            if (_states.ContainsKey(stateKey))
            {
                _logger.LogWarning($"State '{stateKey}' already registered. Replacing.");
            }
            _states[stateKey] = state;
        }

        /// <summary>
        /// Set the initial state without triggering Enter
        /// </summary>
        public void SetInitialState(TState stateKey)
        {
            if (!_states.TryGetValue(stateKey, out var state))
            {
                _logger.LogError($"State '{stateKey}' not found!");
                return;
            }

            _currentStateKey = stateKey;
            _currentState = state;
            _isInitialized = true;
            _currentState.Enter();
        }

        /// <summary>
        /// Transition to a new state
        /// </summary>
        public void ChangeState(TState newStateKey)
        {
            if (!_isInitialized)
            {
                SetInitialState(newStateKey);
                return;
            }

            if (EqualityComparer<TState>.Default.Equals(_currentStateKey, newStateKey))
            {
                return; // Already in this state
            }

            if (!_states.TryGetValue(newStateKey, out var newState))
            {
                _logger.LogError($"State '{newStateKey}' not found!");
                return;
            }

            var previousState = _currentStateKey;
            _currentState?.Exit();
            _currentStateKey = newStateKey;
            _currentState = newState;
            _currentState.Enter();

            OnStateChanged?.Invoke(previousState, newStateKey);
        }

        /// <summary>
        /// Update the current state
        /// </summary>
        public void Update()
        {
            _currentState?.Update();
        }

        /// <summary>
        /// Check if currently in a specific state
        /// </summary>
        public bool IsInState(TState stateKey)
        {
            return EqualityComparer<TState>.Default.Equals(_currentStateKey, stateKey);
        }

        /// <summary>
        /// Get a registered state
        /// </summary>
        public T GetState<T>(TState stateKey) where T : class, IState
        {
            return _states.TryGetValue(stateKey, out var state) ? state as T : null;
        }
    }
}
