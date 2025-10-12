using System.Collections.Generic;

namespace SimpleStateMachines.Transitions
{
    /// <summary>
    /// Helper class to check conditions more efficiently.
    /// </summary>
    public class ConditionalTransitionContainer<TId>
    {
        private readonly IStateSwitcher<TId> m_switcher;
        private readonly Dictionary<TId, List<ConditionalTransition<TId>>> m_transitions;

        public ConditionalTransitionContainer(IStateSwitcher<TId> switcher)
        {
            m_switcher = switcher;
            m_transitions = new Dictionary<TId, List<ConditionalTransition<TId>>>();
        }

        public void Add(ConditionalTransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ConditionalTransition<TId>> transitions))
                transitions.Add(transition);
            else
                m_transitions.Add(transition.From, new List<ConditionalTransition<TId>>() { transition });
        }

        public void Remove(ConditionalTransition<TId> transition)
        {
            if (m_transitions.TryGetValue(transition.From, out List<ConditionalTransition<TId>> transitions))
            {
                transitions.Remove(transition);

                if (transitions.Count == 0)
                    m_transitions.Remove(transition.From);
            }
        }

        public ConditionalTransition<TId>[] GetTransitionsFrom(TId id)
        {
            if (m_transitions.TryGetValue(id, out List<ConditionalTransition<TId>> transitions))
                return transitions.ToArray();

            return new ConditionalTransition<TId>[0];
        }

        public ConditionalTransition<TId>[] GetTransitionsTo(TId id)
        {
            List<ConditionalTransition<TId>> transitions = new List<ConditionalTransition<TId>>();

            foreach (List<ConditionalTransition<TId>> transitionList in m_transitions.Values)
            {
                foreach (ConditionalTransition<TId> transition in transitionList)
                {
                    if (EqualityComparer<TId>.Default.Equals(transition.To, id))
                        transitions.Add(transition);
                }
            }

            return transitions.ToArray();
        }

        public ConditionalTransition<TId>[] GetAllTransitions()
        {
            List<ConditionalTransition<TId>> transitions = new List<ConditionalTransition<TId>>();

            foreach (List<ConditionalTransition<TId>> transitionList in m_transitions.Values)
            {
                foreach (ConditionalTransition<TId> transition in transitionList)
                    transitions.Add(transition);
            }

            return transitions.ToArray();
        }

        public void TickCondition()
        {
            if (!m_transitions.TryGetValue(m_switcher.ActiveStateId, out List<ConditionalTransition<TId>> transitions))
                return;

            foreach (ConditionalTransition<TId> transition in transitions)
            {
                transition.TickCondition();
            }
        }
    }
}
