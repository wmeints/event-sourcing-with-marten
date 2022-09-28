using Marten;

using TastyBeans.Profile.Application.CommandHandlers;
using TastyBeans.Profile.Domain.Aggregates.CustomerAggregate.Commands;

using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(marten =>
{
    marten.Connection(builder.Configuration.GetConnectionString("DefaultDatabase"));
    marten.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
});

var app = builder.Build();

app.MapPost("/customers", async (RegisterCustomer cmd, RegisterCustomerCommandHandler handler) =>
{
    await handler.HandleAsync(cmd);
    return Results.Accepted();
});

app.Run();
