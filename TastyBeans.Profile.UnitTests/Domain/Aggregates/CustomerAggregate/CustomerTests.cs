using Shouldly;

using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

namespace TastyBeans.Profile.UnitTests.Domain.Aggregates.CustomerAggregate;

public class CustomerTests
{
    [Fact]
    public void CanRegisterCustomer()
    {
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;

        var cmd = new RegisterCustomer(Guid.NewGuid(), "Test", "Customer", invoiceAddress, shippingAddress, startDate);

        var customer = new Customer(cmd);

        customer.Id.ShouldBe(cmd.CustomerId);
        customer.FirstName.ShouldBe(cmd.FirstName);
        customer.LastName.ShouldBe(cmd.LastName);
        customer.ShippingAddress.ShouldBe(cmd.ShippingAddress);
        customer.InvoiceAddress.ShouldBe(cmd.InvoiceAddress);
        customer.Subscription.ShouldNotBeNull().EndDate.ShouldBeNull();

        customer.PendingDomainEvents.ShouldContain(x => x is CustomerRegistered);
    }

    [Fact]
    public void CanCancelActiveSubscription()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;
        var cancellationDate = DateTime.UtcNow.AddMonths(1);

        var registerCustomerCmd =
            new RegisterCustomer(customerId, "Test", "Customer", invoiceAddress, shippingAddress, startDate);
        var unsubscribeCmd = new UnsubscribeCustomer(customerId, cancellationDate);

        var customer = new Customer(registerCustomerCmd);
        customer.Unsubscribe(unsubscribeCmd);

        customer.Subscription.ShouldNotBeNull().EndDate.ShouldNotBeNull();
        customer.PendingDomainEvents.ShouldContain(x => x is SubscriptionCanceled);
    }

    [Fact]
    public void CanEndCanceledSubscription()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;
        var cancellationDate = DateTime.UtcNow.AddMonths(1);

        var registerCustomerCmd =
            new RegisterCustomer(customerId, "Test", "Customer", invoiceAddress, shippingAddress, startDate);
        var unsubscribeCmd = new UnsubscribeCustomer(customerId, cancellationDate);
        var endSubscriptionCmd = new EndSubscription(customerId);

        var customer = new Customer(registerCustomerCmd);
        customer.Unsubscribe(unsubscribeCmd);
        customer.EndSubscription(endSubscriptionCmd);

        customer.Subscription.ShouldBeNull();
        customer.PendingDomainEvents.ShouldContain(x => x is SubscriptionEnded);
    }

    [Fact]
    public void CanResubscribeAfterCancelling()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;
        var cancellationDate = DateTime.UtcNow.AddMonths(1);

        var registerCustomerCmd = new RegisterCustomer(
            customerId, 
            "Test",
            "Customer",
            invoiceAddress, 
            shippingAddress,
            startDate);
        
        var unsubscribeCmd = new UnsubscribeCustomer(customerId, cancellationDate);

        var customer = new Customer(registerCustomerCmd);
        
        customer.Unsubscribe(unsubscribeCmd);
        customer.Resubscribe(new ResubscribeCustomer(customerId, cancellationDate));

        customer.Subscription.ShouldNotBeNull().EndDate.ShouldBeNull();
        customer.PendingDomainEvents.ShouldContain(x => x is SubscriptionStarted);
    }
    
    [Fact]
    public void CanResubscribeAfterEndingCurrentSubscription()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;
        var cancellationDate = DateTime.UtcNow.AddMonths(1);

        var registerCustomerCmd = new RegisterCustomer(
            customerId, 
            "Test",
            "Customer",
            invoiceAddress, 
            shippingAddress,
            startDate);
        
        var unsubscribeCmd = new UnsubscribeCustomer(customerId, cancellationDate);
        var endSubscriptionCmd = new EndSubscription(customerId);
        
        var customer = new Customer(registerCustomerCmd);
        
        customer.Unsubscribe(unsubscribeCmd);
        customer.EndSubscription(endSubscriptionCmd);
        customer.Resubscribe(new ResubscribeCustomer(customerId, cancellationDate));

        customer.Subscription.ShouldNotBeNull().EndDate.ShouldBeNull();
        customer.PendingDomainEvents.ShouldContain(x => x is SubscriptionStarted);
    }
}