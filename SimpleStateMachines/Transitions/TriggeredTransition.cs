using System;

namespace SimpleStateMachines.Transitions
{
    /// <summary>
    /// Switches the state upon being triggered externally, only if in state transitioning from
    /// </summary>
    public class TriggeredTransition<TId>
    {
        private readonly TId m_from;
        private readonly TId m_to;
        private readonly IStateSwitcher<TId> m_stateSwitcher;

        public TriggeredTransition(IStateSwitcher<TId> switcher, TId from, TId to)
        {
            if (switcher == null)
                throw new ArgumentNullException(nameof(switcher));

            m_from = from;
            m_to = to;
            m_stateSwitcher = switcher;
        }

        public TId From => m_from;
        public TId To => m_to;

        public bool CanTrigger() => m_stateSwitcher.IsInState(m_from);

        public void Trigger()
        {
            if (CanTrigger())
                m_stateSwitcher.ChangeState(m_to);
        }
    }
}
