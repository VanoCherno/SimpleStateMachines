namespace SimpleStateMachines
{
    public interface ITransition<TId>
    {
        TId From { get; }
        TId To { get; }
        bool ShouldTransition();
    }
}
