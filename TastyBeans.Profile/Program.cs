using Marten;
using Marten.Events.Projections;

using TastyBeans.Profile.Application.CommandHandlers;
using TastyBeans.Profile.Application.Projections;
using TastyBeans.Profile.Application.ReadModels;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Events;

using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(marten =>
{
    marten.Connection(builder.Configuration.GetConnectionString("DefaultDatabase"));
    marten.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

    marten.Schema.For<CustomerRegistered>().Identity(x => x.CustomerId);
    marten.Schema.For<SubscriptionCanceled>().Identity(x => x.CustomerId);
    marten.Schema.For<SubscriptionStarted>().Identity(x => x.CustomerId);
    marten.Schema.For<SubscriptionEnded>().Identity(x => x.CustomerId);

    // Run the projection in the same transaction.
    marten.Projections.Add<CustomerInfoProjection>(ProjectionLifecycle.Inline);
});

var app = builder.Build();

app.MapPost("/customers", async (RegisterCustomer cmd, RegisterCustomerCommandHandler handler) =>
{
    await handler.HandleAsync(cmd);
    return Results.Accepted();
});

app.MapGet("/customers", async (ICustomerInfoRepository repository) => Results.Ok(await repository.FindAllAsync()));

app.MapGet("/customers/{id:guid}", async (Guid id, ICustomerInfoRepository repository) =>
{
    var customer = await repository.FindByIdAsync(id);

    return customer switch
    {
        { } => Results.Ok(customer),
        null => Results.NotFound()
    };
});

app.Run();
