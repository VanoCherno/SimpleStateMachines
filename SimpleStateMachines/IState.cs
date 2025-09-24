namespace SimpleStateMachines
{
    public interface IState<TId>
    {
        TId Id { get; }
        void Enter();
        void Exit();
    }
}
