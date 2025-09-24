using System;
using System.Collections.Generic;

namespace SimpleStateMachines
{
    public interface IReadonlyStateMachine<TId, TState>
    {
        TId CurrentStateId { get; }
        TId PreviousStateId { get; }

        event Action OnStateChanged;

        TState GetStateById(TId id);
        IReadOnlyList<ITransition<TId>> GetTransitionsFromState(TId id);
        List<TState> GetAllStates();
        bool IsInState(TId id);
    }
}
