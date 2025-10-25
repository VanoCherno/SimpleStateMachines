using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Transitions.Executors
{
    public class EventExecutor<TEvent, TId>
    {
        private readonly IStateSwitcher<TId> m_switcher;
        private readonly Dictionary<TEvent, List<Transition<TId>>> m_events;

        public EventExecutor(IStateSwitcher<TId> switcher)
        {
            if (switcher == null)
                throw new ArgumentNullException(nameof(switcher));

            m_switcher = switcher;
            m_events = new Dictionary<TEvent, List<Transition<TId>>>();
        }

        public void AddTransition(TEvent eventName, Transition<TId> transition)
        {
            if (m_events.TryGetValue(eventName, out List<Transition<TId>> transitions))
                transitions.Add(transition);
            else
                m_events.Add(eventName, new List<Transition<TId>>() { transition });
        }

        public bool RemoveTransition(TEvent eventName, Transition<TId> transition)
        {
            if (m_events.TryGetValue(eventName, out List<Transition<TId>> transitions))
                return transitions.Remove(transition);
            else
                return false;
        }

        public Transition<TId>[] GetTransitionsFromEvent(TEvent eventName)
        {
            if (m_events.TryGetValue(eventName, out List<Transition<TId>> transitions))
                return transitions.ToArray();
            else
                return new Transition<TId>[0];
        }

        public TEvent[] GetAllEvents()
        {
            TEvent[] events = new TEvent[m_events.Keys.Count];
            int i = 0;

            foreach (TEvent e in m_events.Keys)
            {
                events[i] = e;
                i++;
            }

            return events;
        }

        public bool Execute(TEvent eventName)
        {
            if (!m_events.TryGetValue(eventName, out List<Transition<TId>> transitions))
                return false;

            foreach (Transition<TId> transition in transitions)
            {
                if (!m_switcher.IsInState(transition.From))
                    continue;

                return m_switcher.ChangeState(transition.To);
            }

            return false;
        }
    }
}
