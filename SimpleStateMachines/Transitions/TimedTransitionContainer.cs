using System.Collections.Generic;

namespace SimpleStateMachines.Transitions
{
    public class TimedTransitionContainer<TId>
    {
        private readonly List<TimedTransition<TId>> m_transitions;

        public TimedTransitionContainer()
        {
            m_transitions = new List<TimedTransition<TId>>();
        }

        public IReadOnlyList<TimedTransition<TId>> Transitions => m_transitions;

        public void Add(TimedTransition<TId> transition)
        {
            m_transitions.Add(transition);
        }

        public void Remove(TimedTransition<TId> transition)
        {
            m_transitions.Remove(transition);
        }

        public TimedTransition<TId>[] GetAllTransitions()
        {
            return m_transitions.ToArray();
        }

        public TimedTransition<TId>[] GetAllTransitionsFrom(TId id)
        {
            List<TimedTransition<TId>> transitions = new List<TimedTransition<TId>>();

            foreach (TimedTransition<TId> transition in m_transitions)
            {
                if (EqualityComparer<TId>.Default.Equals(transition.From, id))
                    transitions.Add(transition);
            }

            return transitions.ToArray();
        }

        public TimedTransition<TId>[] GetAllTransitionsTo(TId id)
        {
            List<TimedTransition<TId>> transitions = new List<TimedTransition<TId>>();

            foreach (TimedTransition<TId> transition in m_transitions)
            {
                if (EqualityComparer<TId>.Default.Equals(transition.To, id))
                    transitions.Add(transition);
            }

            return transitions.ToArray();
        }

        public void Tick(float deltaTime)
        {
            foreach (TimedTransition<TId> transition in m_transitions)
                transition.Tick(deltaTime);
        }
    }
}
