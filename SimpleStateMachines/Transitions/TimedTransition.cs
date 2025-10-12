using System;

namespace SimpleStateMachines.Transitions
{
    /// <summary>
    /// Performs transition after timer runs out. Timer starts when state transitioning from is entered.
    /// </summary>
    public class TimedTransition<TId>
    {
        private readonly IStateSwitcher<TId> m_stateSwitcher;
        private readonly TId m_from;
        private readonly TId m_to;
        private readonly float m_delay;
        private float m_elapsedTime;
        private bool m_isRunning;

        public TimedTransition(IStateSwitcher<TId> switcher, TId from, TId to, float delay)
        {
            if (switcher == null)
                throw new ArgumentNullException(nameof(switcher));

            if (delay < 0)
                throw new ArgumentOutOfRangeException(nameof(delay));

            m_stateSwitcher = switcher;
            m_from = from;
            m_to = to;
            m_delay = delay;
            m_elapsedTime = 0f;
            m_isRunning = false;
            switcher.OnStateChanged += OnStateChanged;
        }

        ~TimedTransition()
        {
            m_stateSwitcher.OnStateChanged -= OnStateChanged;
        }

        public TId From => m_from;
        public TId To => m_to;
        public float Delay => m_delay;
        public float ElapsedTime => m_elapsedTime;

        public void Tick(float deltaTime)
        {
            if (!m_isRunning)
                return;

            m_elapsedTime += deltaTime;

            if (m_elapsedTime < m_delay)
                return;

            m_stateSwitcher.ChangeState(m_to);
        }

        private void OnStateChanged()
        {
            if (m_stateSwitcher.IsInState(m_from))
                Start();
            else
                Stop();
        }

        private void Start()
        {
            m_elapsedTime = 0f;
            m_isRunning = true;
        }

        private void Stop()
        {
            m_elapsedTime = 0f;
            m_isRunning = false;
        }
    }
}
