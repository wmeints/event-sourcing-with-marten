namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

public record ResubscribeCustomer(Guid CustomerId, DateTime StartDate);