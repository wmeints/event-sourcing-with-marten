using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Marten;
using Marten.Events.Projections;

using Shouldly;

using TastyBeans.Profile.Application.Projections;
using TastyBeans.Profile.Application.ReadModels;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

using Weasel.Core;

namespace TastyBeans.Profile.UnitTests.Application.Projections;

public class CustomerInfoProjectionTests: IAsyncLifetime
{
    private DocumentStore? _documentStore = null;

    private readonly TestcontainerDatabase _testcontainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "profile", Username = "postgres", Password = "postgres"
            })
            .Build();

    public async Task InitializeAsync()
    {
        await _testcontainer.StartAsync();
        _documentStore = DocumentStore.For(options =>
        {
            options.Connection(_testcontainer.ConnectionString);
            
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

            options.Schema.For<CustomerRegistered>().Identity(x => x.CustomerId);
            options.Schema.For<SubscriptionCanceled>().Identity(x => x.CustomerId);
            options.Schema.For<SubscriptionStarted>().Identity(x => x.CustomerId);
            options.Schema.For<SubscriptionEnded>().Identity(x => x.CustomerId);

            // Run the projection in the same transaction.
            options.Projections.Add<CustomerInfoProjection>(ProjectionLifecycle.Inline);
        });
        
    }

    public async Task DisposeAsync()
    {
        _documentStore?.Dispose();
        await _testcontainer.StopAsync();
    }

    [Fact]
    public async Task ProjectsCustomerOnRegistration()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;

        var events = new object[]
        {
            new CustomerRegistered(customerId, "Test", "Customer", invoiceAddress, shippingAddress),
            new SubscriptionStarted(customerId, startDate)
        };

        await using var documentSession = _documentStore!.OpenSession();

        documentSession.Events.StartStream<Customer>(events);
        await documentSession.SaveChangesAsync();

        var customer = await documentSession.Query<CustomerInfo>().SingleAsync();

        customer.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateCustomerInfoWhenSubscriptionIsCancelled()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;

        var events = new object[]
        {
            new CustomerRegistered(customerId, "Test", "Customer", invoiceAddress, shippingAddress),
            new SubscriptionStarted(customerId, startDate),
            new SubscriptionCanceled(customerId, startDate.AddMonths(6))
        };

        await using var documentSession = _documentStore!.OpenSession();

        documentSession.Events.StartStream<Customer>(events);
        await documentSession.SaveChangesAsync();
        
        var customer = await documentSession.Query<CustomerInfo>().SingleAsync();

        customer.ShouldNotBeNull().Status.ShouldBe(SubscriptionStatus.Canceled);
    }
    
    [Fact]
    public async Task DeleteCustomerInfoWhenSubscriptionIsEnded()
    {
        var customerId = Guid.NewGuid();
        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;

        var events = new object[]
        {
            new CustomerRegistered(customerId, "Test", "Customer", invoiceAddress, shippingAddress),
            new SubscriptionStarted(customerId, startDate),
            new SubscriptionCanceled(customerId, startDate.AddMonths(6)),
            new SubscriptionEnded(customerId)
        };

        await using var documentSession = _documentStore!.OpenSession();

        documentSession.Events.StartStream<Customer>(events);
        await documentSession.SaveChangesAsync();
        
        var customer = await documentSession.Query<CustomerInfo>().SingleOrDefaultAsync();

        customer.ShouldBeNull();
    }
}