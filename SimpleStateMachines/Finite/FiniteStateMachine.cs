using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Finite
{
    public class FiniteStateMachine<TId, TState> : IStateMachine<TId, TState>
        where TState : class, IState<TId>
    {
        private readonly Dictionary<TId, List<ITransition<TId>>> m_transitions;
        private readonly Dictionary<TId, TState> m_states;
        private TId m_previousStateId;
        private TState m_currentState;

        public FiniteStateMachine()
        {
            m_transitions = new Dictionary<TId, List<ITransition<TId>>>();
            m_states = new Dictionary<TId, TState>();
            m_currentState = null;
            m_previousStateId = default;
        }

        public TId CurrentStateId => (m_currentState != null) ? m_currentState.Id : default;
        public TId PreviousStateId => m_previousStateId;
        protected TState CurrentState => m_currentState;

        public event Action OnStateChanged;

        public void AddTransition(ITransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ITransition<TId>> transitions))
                transitions.Add(transition);
            else
                m_transitions.Add(transition.From, new List<ITransition<TId>> { transition });
        }

        public void RemoveTransition(ITransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ITransition<TId>> transitions))
                transitions.Remove(transition);
        }

        /// <summary>Provides list of transitions from a given state, if state has no transitions returns empty list</summary>
        public IReadOnlyList<ITransition<TId>> GetTransitionsFromState(TId id)
        {
            if (m_transitions.TryGetValue(id, out List<ITransition<TId>> transitions))
                return transitions.AsReadOnly();

            return new List<ITransition<TId>>().AsReadOnly();
        }

        public void RemoveAllTransitionsFromState(TId id)
        {
            m_transitions.Remove(id);
        }

        public void ClearTransitions()
        {
            m_transitions.Clear();
        }

        /// <summary>Provides a state of given id, returns null if state with this id is not found</summary>
        public TState GetStateById(TId id)
        {
            if (m_states.TryGetValue(id, out TState state))
                return state;

            return null;
        }

        public List<TState> GetAllStates()
        {
            List<TState> states = new List<TState>();

            foreach (TState state in m_states.Values)
                states.Add(state);

            return states;
        }

        /// <summary>Adds state if state with this id does not already exist</summary>
        public bool TryAddState(TState state)
        {
            return m_states.TryAdd(state.Id, state);
        }

        /// <summary>Removes state if state with this id exists and is not currently active state</summary>
        public bool TryRemoveState(TState state)
        {
            return TryRemoveState(state.Id);
        }

        /// <summary>Removes state if state with this id exists and is not currently active state</summary>
        public bool TryRemoveState(TId id)
        {
            if (!m_states.ContainsKey(id))
                return false;

            if (IsInState(id))
                return false;

            m_states.Remove(id);
            m_transitions.Remove(id);
            return true;
        }

        /// <summary>Checks all transitions from the active state, if a transition should be made changes state</summary>
        public void TickTransitions()
        {
            List<ITransition<TId>> transitions;

            if (m_currentState == null)
                return;

            if (!m_transitions.TryGetValue(m_currentState.Id, out transitions))
                return;

            for (int i = 0; i < transitions.Count; i++)
            {
                if (!transitions[i].ShouldTransition())
                    continue;

                if (TryChangeState(transitions[i].To))
                    return;
            }
        }

        /// <summary>Immediately changes state of state machine, if state with this id exists and is not currently active state</summary>
        public bool TryChangeState(TId id)
        {
            if (!m_states.TryGetValue(id, out TState next))
                return false;

            if (IsInState(id))
                return false;

            if (m_currentState != null)
            {
                m_currentState.Exit();
                m_previousStateId = m_currentState.Id;
            }

            m_currentState = next;
            m_currentState.Enter();
            OnStateChanged?.Invoke();
            return true;
        }

        public bool IsInState(TId id)
        {
            if (m_currentState == null)
                return false;

            return EqualityComparer<TId>.Default.Equals(m_currentState.Id, id);
        }
    }
}




