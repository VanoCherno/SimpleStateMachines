namespace SimpleStateMachines
{
    public interface ITransitionManager<TId>
    {
        void Add(ITransition<TId> transition);
        void Remove(ITransition<TId> transition);
        void Clear();
        ITransition<TId>[] GetTransitionsFrom(TId id);
        ITransition<TId>[] GetTransitionsTo(TId id);
        ITransition<TId>[] GetAllTransitions();
        bool ShouldTransition(TId from, out TId to);
    }
}
