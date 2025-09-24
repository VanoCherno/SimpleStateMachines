using System;

namespace SimpleStateMachines.Hierarchical
{
    public interface IStateTree<TId, TState>
    {
        bool IsStateActiveInHierarchy(TId id);
        bool IsStateActiveInHierarchy(TState state);
        TState GetStateById(TId id);
    }
}
