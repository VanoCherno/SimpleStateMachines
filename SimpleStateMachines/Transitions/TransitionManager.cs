using System.Collections.Generic;

namespace SimpleStateMachines.Transitions
{
    /// <summary>
    /// Holds a collection of transitions, allows for search and quick check if a transition should be made
    /// </summary>
    public class TransitionManager<TId> : ITransitionManager<TId>
    {
        private readonly Dictionary<TId, List<ITransition<TId>>> m_transitions;

        public TransitionManager()
        {
            m_transitions = new Dictionary<TId, List<ITransition<TId>>>();
        }

        public void Add(ITransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ITransition<TId>> transitions))
                transitions.Add(transition);
            else
                m_transitions.Add(transition.From, new List<ITransition<TId>>() { transition });
        }

        public void Remove(ITransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ITransition<TId>> transitions))
                transitions.Remove(transition);
        }

        /// <summary>Removes all transitions</summary>
        public void Clear()
        {
            m_transitions.Clear();
        }

        /// <summary>Provides all transitions from a given state</summary>
        public ITransition<TId>[] GetTransitionsFrom(TId id)
        {
            if (m_transitions.TryGetValue(id, out List<ITransition<TId>> transitions))
                return transitions.ToArray();

            return new ITransition<TId>[0];
        }

        /// <summary>Provides all transitions to a given state. This operation is quite expensive</summary>
        public ITransition<TId>[] GetTransitionsTo(TId id)
        {
            List<ITransition<TId>> transitions = new List<ITransition<TId>>();

            foreach (List<ITransition<TId>> transition in m_transitions.Values)
            {
                for (int i = 0; i < transition.Count; i++)
                {
                    if (EqualityComparer<TId>.Default.Equals(transition[i].To, id))
                        transitions.Add(transition[i]);
                }
            }

            return transitions.ToArray();
        }

        public ITransition<TId>[] GetAllTransitions()
        {
            List<ITransition<TId>> allTransitions = new List<ITransition<TId>>();

            foreach(List<ITransition<TId>> transitions in m_transitions.Values)
            {
                foreach(ITransition<TId> transition in transitions)
                    allTransitions.Add(transition);
            }

            return allTransitions.ToArray();
        }

        public bool ShouldTransition(TId from, out TId to)
        {
            if (m_transitions.TryGetValue(from, out List<ITransition<TId>> transitions))
            {
                foreach(ITransition<TId> transition in transitions)
                {
                    if (transition.ShouldTransition())
                    {
                        to = transition.To;
                        return true;
                    }
                }
            }

            to = default;
            return false;
        }
    }
}
