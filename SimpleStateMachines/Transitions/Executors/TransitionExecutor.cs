namespace SimpleStateMachines.Transitions.Executors
{
    public class TransitionExecutor<TId>
    {
        private readonly IStateSwitcher<TId> m_switcher;
        private readonly Transition<TId> m_transition;

        public TransitionExecutor(IStateSwitcher<TId> switcher, Transition<TId> transition)
        {
            m_switcher = switcher;
            m_transition = transition;
        }

        public bool CanExecute()
        {
            return m_switcher.IsInState(m_transition.From);
        }

        public bool Execute()
        {
            if (m_switcher.IsInState(m_transition.From))
                return m_switcher.ChangeState(m_transition.To);

            return false;
        }
    }
}
