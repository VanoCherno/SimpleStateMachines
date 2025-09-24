using System.Collections.Generic;

namespace SimpleStateMachines.Hierarchical
{
    public interface IHierarchicalState<TId, TState> : IState<TId>
        where TState : class, IHierarchicalState<TId, TState>
    {
        TState Parent { get; }
        IReadOnlyCollection<TState> States { get; }
    }
}
