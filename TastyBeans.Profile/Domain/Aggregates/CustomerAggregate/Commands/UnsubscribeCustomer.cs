namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

public record UnsubscribeCustomer(Guid CustomerId, DateTime EndDate);