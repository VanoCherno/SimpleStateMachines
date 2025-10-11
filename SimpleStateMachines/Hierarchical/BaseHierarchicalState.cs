namespace SimpleStateMachines.Hierarchical
{
    /// <summary>
    /// An empty base class that enforces inheritance constraints
    /// </summary>
    public abstract class BaseHierarchicalState<TId, TState> : BaseState<TId> where TState : BaseHierarchicalState<TId, TState>
    {
        protected BaseHierarchicalState(TId id) : base(id) {}
    }
}
