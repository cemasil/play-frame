namespace PlayFrame.Core.StateMachine
{
    /// <summary>
    /// Simple state implementation using delegates
    /// </summary>
    public class State : IState
    {
        private readonly System.Action _onEnter;
        private readonly System.Action _onUpdate;
        private readonly System.Action _onExit;

        public State(System.Action onEnter, System.Action onUpdate, System.Action onExit)
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
