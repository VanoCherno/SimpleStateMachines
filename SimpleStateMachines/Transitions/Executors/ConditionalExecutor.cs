using System;
using System.Collections.Generic;

namespace SimpleStateMachines.Transitions.Executors
{
    public class ConditionalExecutor<TId>
    {
        private readonly IStateSwitcher<TId> m_switcher;
        private readonly List<ConditionalTransition> m_transitions;

        public ConditionalExecutor(IStateSwitcher<TId> switcher)
        {
            m_switcher = switcher;
            m_transitions = new List<ConditionalTransition>();
        }

        public void AddTransition(Transition<TId> transition, Func<bool> condition)
        {
            if (m_transitions.Contains(new ConditionalTransition(condition, transition)))
                return;

            m_transitions.Add(new ConditionalTransition(condition, transition));
        }

        public bool RemoveTransition(ConditionalTransition transition)
        {
            return m_transitions.Remove(transition);
        }

        public ConditionalTransition[] GetAllTransitions()
        {
            return m_transitions.ToArray();
        }

        public bool Execute()
        {
            foreach (var transition in m_transitions)
            {
                if (!transition.Condition())
                    continue;

                if (!m_switcher.IsInState(transition.Transition.From))
                    continue;
                
                return m_switcher.ChangeState(transition.Transition.To);
            }

            return false;
        }

        public struct ConditionalTransition
        {
            public Transition<TId> Transition;
            public Func<bool> Condition;

            public ConditionalTransition(Func<bool> condition, Transition<TId> transition)
            {
                Transition = transition;
                Condition = condition;
            }
        }
    }
}
