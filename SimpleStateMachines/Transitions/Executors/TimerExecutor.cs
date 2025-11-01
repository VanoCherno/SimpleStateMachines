using System;
using System.Threading;

namespace SimpleStateMachines.Transitions.Executors
{
    public class TimerExecutor<TId>
    {
        private readonly IStateSwitcher<TId> m_switcher;
        private readonly Timer m_timer;
        private readonly Transition<TId> m_transition;
        private readonly int m_delayMilliseconds;
        private bool m_isActive;

        public TimerExecutor(IStateSwitcher<TId> switcher, Transition<TId> transition, int delayMilliseconds)
        {
            if (switcher == null)
                throw new ArgumentNullException(nameof(switcher));

            if (delayMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));

            m_switcher = switcher;
            m_transition = transition;
            m_delayMilliseconds = delayMilliseconds;
            m_timer = new Timer(Transition, null, Timeout.Infinite, Timeout.Infinite);
            switcher.OnStateChanged += HandleStateChange;
        }

        public TimerExecutor(IStateSwitcher<TId> switcher, Transition<TId> transition, float delaySeconds)
            : this(switcher, transition, (int)Math.Floor(delaySeconds * 1000))
        { }

        ~TimerExecutor()
        {
            m_switcher.OnStateChanged -= HandleStateChange;
        }

        public bool IsActive => m_isActive;
        public int DelayMilliseconds => m_delayMilliseconds;
        public float DelaySeconds => m_delayMilliseconds / 1000f;

        public void Activate()
        {
            m_isActive = true;
            HandleStateChange();
        }

        public void Deactivate()
        {
            m_isActive = false;
            Stop();
        }

        private void Transition(object state)
        {
            if (m_switcher.IsInState(m_transition.From))
                m_switcher.ChangeState(m_transition.To);

            Stop();
        }

        private void Start()
        {
            m_timer.Change(0, m_delayMilliseconds);
        }

        private void Stop()
        {
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void HandleStateChange()
        {
            if (m_isActive)
                Stop();

            if (m_switcher.IsInState(m_transition.From))
                Start();
            else
                Stop();
        }
    }
}
