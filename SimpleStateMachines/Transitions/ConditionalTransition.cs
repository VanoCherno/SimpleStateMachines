using System;

namespace SimpleStateMachines.Transitions
{
    /// <summary>
    /// Performs transition only after a condition is met.
    /// </summary>
    public class ConditionalTransition<TId>
    {
        private readonly IStateSwitcher<TId> m_switcher;
        private readonly TId m_from;
        private readonly TId m_to;
        private readonly Func<bool> condition;

        public ConditionalTransition(IStateSwitcher<TId> switcher, TId from, TId to, Func<bool> condition)
        {
            if (switcher == null)
                throw new ArgumentNullException(nameof(switcher));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            m_switcher = switcher;
            m_from = from;
            m_to = to;
            this.condition = condition;
        }

        public TId From => m_from;
        public TId To => m_to;
        public Func<bool> Condition => condition;

        public void TickCondition()
        {
            if (m_switcher.IsInState(m_from) && condition())
                m_switcher.ChangeState(m_to);
        }
    }
}
