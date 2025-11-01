namespace SimpleStateMachines.Transitions
{
    public readonly struct Transition<TId>
    {
        private readonly TId m_from;
        private readonly TId m_to;

        public Transition(TId from, TId to)
        {
            m_from = from;
            m_to = to;
        }

        public TId From => m_from;
        public TId To => m_to;
    }
}
