using System;

namespace SimpleStateMachines.Transitions
{
    public class Transition<TId> : ITransition<TId>
    {
        private TId m_from;
        private TId m_to;
        private Func<bool> condition;

        public Transition(TId from, TId to, Func<bool> condition)
        {
            m_from = from;
            m_to = to;
            this.condition = condition;
        }

        public TId From => m_from;
        public TId To => m_to;

        public bool ShouldTransition()
        {
            return condition();
        }
    }
}
