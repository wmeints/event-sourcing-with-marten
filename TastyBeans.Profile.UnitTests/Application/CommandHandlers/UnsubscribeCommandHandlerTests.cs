using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Marten;

using Shouldly;

using TastyBeans.Profile.Application.CommandHandlers;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

namespace TastyBeans.Profile.UnitTests.Application.CommandHandlers;

public class UnsubscribeCommandHandlerTests: IAsyncLifetime
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
        _documentStore = DocumentStore.For(_testcontainer.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        _documentStore?.Dispose();
        await _testcontainer.StopAsync();
    }

    [Fact]
    public async Task CanHandleUnsubscribeCustomer()
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

        var cmd = new UnsubscribeCustomer(customerId, startDate.AddMonths(6));

        await using var documentSession = _documentStore!.OpenSession();
        var commandHandler = new UnsubscribeCustomerCommandHandler(documentSession);
        
        documentSession.Events.StartStream<Customer>(customerId, events);
        await documentSession.SaveChangesAsync();

        await commandHandler.HandleAsync(cmd);

        var savedEvents = await documentSession.Events.FetchStreamAsync(customerId);
        
        savedEvents.Count.ShouldBe(3);
    }
}