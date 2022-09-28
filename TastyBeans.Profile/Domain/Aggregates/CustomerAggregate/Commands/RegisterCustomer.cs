namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

public record RegisterCustomer(Guid CustomerId, string FirstName, string LastName, Address InvoiceAddress, Address ShippingAddress);