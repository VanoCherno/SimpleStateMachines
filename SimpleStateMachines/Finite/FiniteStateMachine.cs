using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Finite
{
    public class FiniteStateMachine<TId, TState> : IStateMachine<TId, TState>, IReadOnlyStateMachine<TId, TState>, IStateSwitcher<TId>
        where TState : BaseState
    {
        private readonly Dictionary<TId, TState> m_states;
        private TState m_activeState;
        private TId m_activeStateId;
        private TId m_previousStateId;

        public FiniteStateMachine()
        {
            m_states = new Dictionary<TId, TState>();
            m_activeState = null;
            m_activeStateId = default;
            m_previousStateId = default;
        }

        public TId ActiveStateId => m_activeStateId;
        public TId PreviousStateId => m_previousStateId;

        protected IReadOnlyDictionary<TId, TState> States => m_states;
        protected TState ActiveState => m_activeState;

        public event Action OnStateChanged;

        public void AddState(TId id, TState state)
        {
            if (m_states.ContainsKey(id))
                throw new InvalidOperationException("Can not add state. State machine already has a state with this id.");

            m_states.Add(id, state);
        }

        public void RemoveState(TId id)
        {
            if (!m_states.ContainsKey(id))
                return;

            m_states.Remove(id);
        }

        public TState GetState(TId id)
        {
            if (!m_states.ContainsKey(id))
                return null;

            return m_states[id];
        }

        public bool HasState(TId id)
        {
            return m_states.ContainsKey(id);
        }

        public TId[] GetAllIds()
        {
            int i = 0;
            TId[] ids = new TId[m_states.Keys.Count];

            foreach (TId id in m_states.Keys)
            {
                ids[i] = id;
                i++;
            }

            return ids;
        }

        /// <summary>Exits active state and deletes all states</summary>
        public void Clear()
        {
            Exit();
            m_states.Clear();
        }

        /// <summary>Exits active state</summary>
        public void Exit()
        {
            m_previousStateId = default;
            m_activeStateId = default;
            m_activeState?.Exit();
            m_activeState = null;
            OnStateChanged?.Invoke();
        }

        public bool ChangeState(TId to)
        {
            if (!m_states.TryGetValue(to, out TState next))
                return false;

            if (m_activeState == next)
                return false;

            if (m_activeState != null)
            {
                m_activeState.Exit();
                m_previousStateId = m_activeStateId;
            }

            m_activeState = next;
            m_activeState.Enter();
            m_activeStateId = to;
            return true;
        }

        /// <summary>Whether the state is current state</summary>
        public bool IsInState(TId id)
        {
            if (m_activeState == null)
                return false;

            return EqualityComparer<TId>.Default.Equals(ActiveStateId, id);
        }
    }
}
