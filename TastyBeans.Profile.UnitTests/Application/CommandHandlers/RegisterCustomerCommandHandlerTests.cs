using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Marten;

using Shouldly;

using TastyBeans.Profile.Application.CommandHandlers;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

namespace TastyBeans.Profile.UnitTests.Application.CommandHandlers;

public class RegisterCustomerCommandHandlerTests : IAsyncLifetime
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
    public async Task CanHandleRegisterCustomer()
    {
        await using var documentSession = _documentStore!.OpenSession();
        var commandHandler = new RegisterCustomerCommandHandler(documentSession);

        var invoiceAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var shippingAddress = new Address("Test street", "1", "0000 AA", "Test city");
        var startDate = DateTime.UtcNow;

        var cmd = new RegisterCustomer(
            Guid.NewGuid(),
            "Test",
            "Customer",
            invoiceAddress, 
            shippingAddress, 
            startDate);

        await commandHandler.HandleAsync(cmd);

        var events = await documentSession.Events.FetchStreamAsync(cmd.CustomerId);
        
        events.Count.ShouldBe(2);
    }
}