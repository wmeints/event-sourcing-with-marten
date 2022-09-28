namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

public record SubscriptionStarted(Guid CustomerId, DateTime StartDate);
