namespace SimpleStateMachines.Hierarchical
{
    /// <summary>
    /// An empty base class that enforces inheritance constraints
    /// </summary>
    public abstract class BaseHierarchicalState<TId> : BaseState<TId>
    {
        protected BaseHierarchicalState(TId id) : base(id) {}
    }
}
