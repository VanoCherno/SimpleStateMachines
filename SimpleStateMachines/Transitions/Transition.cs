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
            m_to = to;
            m_from = from;
            this.condition = condition;
        }

        public Transition(BaseState<TId> from, BaseState<TId> to, Func<bool> condition)
        {
            m_from = from.Id;
            m_to = to.Id;
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
