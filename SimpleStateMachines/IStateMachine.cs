namespace SimpleStateMachines
{
    public interface IStateMachine<TId, TState> : IReadonlyStateMachine<TId, TState>
    {
        void AddTransition(ITransition<TId> transition);
        void RemoveTransition(ITransition<TId> transition);
        void RemoveAllTransitionsFromState(TId id);
        void ClearTransitions();
        public void TickTransitions();
        bool TryChangeState(TId id);
    }
}
