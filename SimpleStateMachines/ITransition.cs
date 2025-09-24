namespace SimpleStateMachines
{
    public interface ITransition<TId>
    {
        public TId From { get; }
        public TId To { get; }
        public bool ShouldTransition();
    }
}
