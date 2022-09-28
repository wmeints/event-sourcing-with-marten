using Marten.Events.Aggregation;

using TastyBeans.Profile.Application.ReadModels;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

namespace TastyBeans.Profile.Application.Projections;

public class CustomerInfoProjection : SingleStreamAggregation<CustomerInfo>
{
    public CustomerInfo Create(CustomerRegistered evt)
    {
        return new CustomerInfo(evt.CustomerId,
            evt.FirstName,
            evt.LastName,
            SubscriptionStatus.Active);
    }

    public CustomerInfo Apply(SubscriptionStarted evt, CustomerInfo customerInfo)
    {
        return customerInfo with
        {
            Status = SubscriptionStatus.Active
        };
    }

    public CustomerInfo Apply(SubscriptionCanceled evt, CustomerInfo customerInfo)
    {
        return customerInfo with {Status = SubscriptionStatus.Canceled};
    }

    public bool ShouldDelete(SubscriptionEnded evt) => true;
}