using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;
using TastyBeans.Profile.Domain.Shared;

namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;

public class Customer : AggregateRoot
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Address InvoiceAddress { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Subscription? Subscription { get; private set; }

    public Customer(RegisterCustomer cmd)
    {
        Emit(new CustomerRegistered(
            cmd.CustomerId,
            cmd.FirstName,
            cmd.LastName,
            cmd.InvoiceAddress,
            cmd.ShippingAddress));

        Emit(new SubscriptionStarted(cmd.CustomerId, cmd.StartDate));
    }

    public void Unsubscribe(UnsubscribeCustomer cmd)
    {
        if (Subscription == null || Subscription.EndDate != null)
        {
            throw new InvalidOperationException("Can't unsubscribe an inactive or canceled subscription");
        }

        Emit(new SubscriptionCanceled(Id, cmd.EndDate));
    }

    public void Resubscribe(ResubscribeCustomer cmd)
    {
        if (Subscription is {EndDate: null})
        {
            throw new InvalidOperationException("Can't subscribe when the customer is already subscribed");
        }

        if (Subscription != null)
        {
            // Continue the subscription if it wasn't ended yet.
            // In practice, this means we're supplying the current start date of the subscription.
            Emit(new SubscriptionStarted(Id, Subscription.StartDate));    
        }
        else
        {
            // If the subscription was ended, we start a new one.
            Emit(new SubscriptionStarted(Id, cmd.StartDate));
        }
    }

    public void EndSubscription(EndSubscription cmd)
    {
        if (Subscription == null || Subscription.EndDate == null)
        {
            throw new InvalidOperationException("Subscription must be canceled before it can be ended");
        }
        
        Emit(new SubscriptionEnded(cmd.CustomerId));
    }

    protected override bool TryApplyEvent(object domainEvent)
    {
        switch (domainEvent)
        {
            case CustomerRegistered customerRegistered:
                Apply(customerRegistered);
                break;
            case SubscriptionStarted subscriptionStarted:
                Apply(subscriptionStarted);
                break;
            case SubscriptionCanceled subscriptionCanceled:
                Apply(subscriptionCanceled);
                break;
            case SubscriptionEnded subscriptionEnded:
                Apply(subscriptionEnded);
                break;
            default:
                return false;
        }

        return true;
    }

    private void Apply(SubscriptionEnded subscriptionCanceled)
    {
        Subscription = null;
    }

    private void Apply(SubscriptionCanceled subscriptionCanceled)
    {
        if (Subscription == null)
        {
            return;
        }

        Subscription = Subscription with {EndDate = subscriptionCanceled.EndDate};
    }

    private void Apply(SubscriptionStarted subscriptionStarted)
    {
        Subscription = new Subscription(subscriptionStarted.StartDate);
    }

    private void Apply(CustomerRegistered customerRegistered)
    {
        Id = customerRegistered.CustomerId;
        FirstName = customerRegistered.FirstName;
        LastName = customerRegistered.LastName;
        InvoiceAddress = customerRegistered.InvoiceAddress;
        ShippingAddress = customerRegistered.ShippingAddress;
    }
}