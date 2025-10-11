namespace SimpleStateMachines
{
    public abstract class BaseState<TId>
    {
        private readonly TId m_id;

        protected BaseState(TId id)
        {
            m_id = id;
        }

        public TId Id => m_id;
        public abstract void Enter();
        public abstract void Exit();
    }
}
