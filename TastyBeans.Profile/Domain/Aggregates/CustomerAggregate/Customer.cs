using TastyBeans.Profile.Domain.Shared;

namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;

public class Customer: AggregateRoot
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; }= null!;
    public Address InvoiceAddress { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Subscription? Subscription { get; private set; }
    
    protected override bool TryApplyEvent(object domainEvent)
    {
        switch (domainEvent)
        {
            default:
                return false;
        }

        return true;
    }
}