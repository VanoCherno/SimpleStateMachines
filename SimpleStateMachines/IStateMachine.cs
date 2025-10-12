using System;

namespace SimpleStateMachines
{
    public interface IStateMachine<TId, TState> : IReadonlyStateMachine<TId, TState>, IStateSwitcher<TId>
    {
        new TId ActiveStateId { get; }
        new TId PreviousStateId { get; }

        new event Action OnStateChanged;

        void AddState(TState state);
        bool TryAddState(TState state);
        bool RemoveState(TState state);
        bool RemoveState(TId id);
        void Clear();
        void Exit();
        new TState GetState(TId id);
        new TState[] GetAllStates();
        new TId[] GetAllIds();
        new bool ChangeState(TId to);
        new bool IsInState(TId id);
    }
}
