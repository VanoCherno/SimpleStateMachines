using System;

namespace SimpleStateMachines
{
    public interface IStateSwitcher<TId>
    {
        TId ActiveStateId { get; }
        TId PreviousStateId { get; }

        event Action OnStateChanged;

        bool ChangeState(TId to);
        bool IsInState(TId id);
    }
}
