using Marten.Schema;

namespace TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

public record CustomerRegistered(
    Guid CustomerId,
    string FirstName, 
    string LastName, 
    Address InvoiceAddress,
    Address ShippingAddress);
