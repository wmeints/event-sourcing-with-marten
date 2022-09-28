namespace TastyBeans.Profile.Domain.Shared;

public abstract class AggregateRoot
{
    private List<object> _pendingDomainEvents = new();

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<object> PendingDomainEvents => _pendingDomainEvents.AsReadOnly();

    protected void Emit(object domainEvent)
    {
        if (TryApplyEvent(domainEvent))
        {
            _pendingDomainEvents.Add(domainEvent);
        }
    }

    protected abstract bool TryApplyEvent(object domainEvent);
}