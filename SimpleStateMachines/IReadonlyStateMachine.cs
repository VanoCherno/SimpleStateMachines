using System;

namespace SimpleStateMachines
{
    public interface IReadonlyStateMachine<TId, TState>
    {
        TId ActiveStateId { get; }
        TId PreviousStateId { get; }
        event Action OnStateChanged;

        TState GetState(TId id);
        TState[] GetAllStates();
        TId[] GetAllIds();
        bool IsInState(TId id);
    }
}
