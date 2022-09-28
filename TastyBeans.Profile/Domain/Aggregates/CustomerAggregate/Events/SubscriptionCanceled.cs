namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

public record SubscriptionCanceled(Guid CustomerId, DateTime EndDate);