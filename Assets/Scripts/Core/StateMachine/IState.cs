namespace PlayFrame.Core.StateMachine
{
    /// <summary>
    /// Interface for state machine states
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
    }

    /// <summary>
    /// Interface for states with a specific context
    /// </summary>
    public interface IState<T> : IState
    {
        void SetContext(T context);
    }
}
