using System;

namespace SimpleStateMachines
{
    public interface IReadOnlyStateMachine<TId, TState>
    {
        TId ActiveStateId { get; }
        TId PreviousStateId { get; }

        event Action OnStateChanged;

        TState GetState(TId id);
        bool HasState(TId id);
        TId[] GetAllIds();
        bool IsInState(TId id);
    }
}
