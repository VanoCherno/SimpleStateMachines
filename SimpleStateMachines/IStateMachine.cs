using System;

namespace SimpleStateMachines
{
    public interface IStateMachine<TId, TState>
    {
        TId ActiveStateId { get; }
        TId PreviousStateId { get; }

        event Action OnStateChanged;

        void AddState(TId id, TState state);
        void RemoveState(TId id);
        TState GetState(TId id);
        bool HasState(TId id);
        TId[] GetAllIds();
        void Clear();
        void Exit();
        bool ChangeState(TId to);
        bool IsInState(TId id);
    }
}
