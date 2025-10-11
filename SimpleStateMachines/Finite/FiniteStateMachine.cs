using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Finite
{
    /// <summary>
    /// A simple finite state machine no comments needed
    /// </summary>
    public class FiniteStateMachine<TId, TState> : IReadonlyStateMachine<TId, TState>
        where TState : BaseState<TId>
    {
        private readonly Dictionary<TId, TState> m_states;
        private readonly ITransitionManager<TId> m_transitionManager;
        private TState m_activeState;
        private TId m_previousStateId;

        public FiniteStateMachine(ITransitionManager<TId> transitionManager)
        {
            m_states = new Dictionary<TId, TState>();
            m_activeState = null;
            m_previousStateId = default;
            m_transitionManager = transitionManager;
        }

        public TId ActiveStateId => (m_activeState != null) ? m_activeState.Id : default;
        public TId PreviousStateId => m_previousStateId;
        public ITransitionManager<TId> Transitions => m_transitionManager;

        protected IReadOnlyDictionary<TId, TState> States => m_states;
        protected TState ActiveState => m_activeState;

        public event Action OnStateChanged;

        public void AddState(TState state)
        {
            if (m_states.ContainsKey(state.Id))
                throw new InvalidOperationException("Can not add state. State with this id has already been added.");

            m_states.Add(state.Id, state);
        }

        public bool TryAddState(TState state)
        {
            return m_states.TryAdd(state.Id, state);
        }

        public bool RemoveState(TState state)
        {
            if (m_activeState == state)
                return false;

            return m_states.Remove(state.Id);
        }

        public bool RemoveState(TId id)
        {
            if (!m_states.TryGetValue(id, out TState state))
                return false;

            return RemoveState(state);
        }

        /// <summary>Exits active state and deletes all states from state machine</summary>
        public void Clear()
        {
            m_activeState?.Exit();
            m_activeState = null;
            m_previousStateId = default;
            m_states.Clear();
        }

        /// <summary>Exits active state</summary>
        public void Exit()
        {
            if (m_activeState == null)
                return;

            m_previousStateId = m_activeState.Id;
            m_activeState.Exit();
            m_activeState = null;
            OnStateChanged?.Invoke();
        }

        public TState GetState(TId id)
        {
            if (m_states.TryGetValue(id, out TState state))
                return state;

            return null;
        }

        public TState[] GetAllStates()
        {
            int i = 0;
            TState[] states = new TState[m_states.Values.Count];

            foreach (TState state in m_states.Values)
            {
                states[i] = state;
                i++;
            }

            return states;
        }

        public TId[] GetAllIds()
        {
            int i = 0;
            TId[] ids = new TId[m_states.Keys.Count];

            foreach(TId id in m_states.Keys)
            {
                ids[i] = id;
                i++;
            }

            return ids;
        }

        /// <summary>Checks if any transitions possible from currently active state. If so - performs transition</summary>
        public void TickTransitions()
        {
            if (m_activeState == null)
                return;

            if (m_transitionManager.ShouldTransition(m_activeState.Id, out TId to))
                ChangeState(to);
        }

        /// <summary>Transitions to a another state if this state is assigned to this state machine</summary>
        public bool ChangeState(TId to)
        {
            if (!m_states.TryGetValue(to, out TState next))
                return false;

            if (m_activeState == next)
                return false;

            if (m_activeState != null)
            {
                m_activeState.Exit();
                m_previousStateId = m_activeState.Id;
            }

            m_activeState = next;
            m_activeState.Enter();
            OnStateChanged?.Invoke();
            return true;
        }
    }
}
