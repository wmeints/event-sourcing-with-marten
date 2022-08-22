using InventorySystem.Commands;
using InventorySystem.Entities;
using InventorySystem.Events;
using InventorySystem.Projections;
using InventorySystem.ReadModels;
using Marten;
using Marten.Pagination;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(marten =>
{
    marten.Connection(builder.Configuration.GetConnectionString("DefaultDatabase"));
    marten.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;

    marten.Projections.Add<ProductInfoProjection>();
});

var app = builder.Build();

app.MapGet("/products", async (int pageIndex, IDocumentSession documentSession) =>
{
    var results = await documentSession
        .Query<ProductInfo>()
        .OrderBy(x => x.ProductCode)
        .ToPagedListAsync(pageIndex, 20);

    return Results.Ok(new
    {
        items = results,
        totalItems = results.TotalItemCount,
        pageIndex = pageIndex,
        pageSize = 20
    });
});

app.MapGet("/products/{id:guid}", async (Guid id, IDocumentSession documentSession) =>
{
    var product = await documentSession.Query<ProductInfo>().FirstOrDefaultAsync(x => x.Id == id);

    return product switch
    {
        { } => Results.Ok(product),
        _ => Results.NotFound()
    };
}).WithName("ProductDetails");

app.MapPost("/products", async (RegisterProduct cmd, IDocumentSession documentSession) =>
{
    var productId = Guid.NewGuid();
    var productRegistered = new ProductRegistered(productId, cmd.ProductCode, cmd.Name);
    
    documentSession.Events.StartStream<Product>(productId, productRegistered);
    await documentSession.SaveChangesAsync();

    return Results.AcceptedAtRoute("ProductDetails", new { id = productId });
});

app.MapPost("/products/{id:guid}/shipments", async (Guid id, HandleShipment cmd, IDocumentSession documentSession) =>
{
    var product = await documentSession.Events.AggregateStreamAsync<Product>(id);

    if (product == null)
    {
        return Results.NotFound();
    }

    var shipmentReceived = new ShipmentReceived(id, cmd.Quantity, cmd.Location);
    product = Product.Apply(shipmentReceived, product);

    documentSession.Events.Append(id, product.Version, shipmentReceived);
    await documentSession.SaveChangesAsync();

    return Results.Accepted();
});

app.MapPost("/products/{id:guid}/reservations", async (Guid id, ReserveProduct cmd, IDocumentSession documentSession) =>
{
    var product = await documentSession.Events.AggregateStreamAsync<Product>(id);

    if (product == null)
    {
        return Results.NotFound();
    }

    if (product.Available - cmd.Quantity < 0)
    {
        return Results.BadRequest();
    }

    var productReserved = new ProductReserved(id, cmd.Quantity);
    product = Product.Apply(productReserved, product);

    documentSession.Events.Append(id, product.Version, productReserved);
    await documentSession.SaveChangesAsync();

    return Results.Accepted();
});

app.MapPost("/products/{id:guid}/picking", async (Guid id, PickProduct cmd, IDocumentSession documentSession) =>
{
    var product = await documentSession.Events.AggregateStreamAsync<Product>(id);

    if (product == null)
    {
        return Results.NotFound();
    }

    if (product.Reserved - cmd.Quantity < 0)
    {
        return Results.BadRequest();
    }

    var productPicked = new ProductPicked(cmd.Quantity);
    product = Product.Apply(productPicked, product);

    documentSession.Events.Append(id, product.Version, productPicked);
    await documentSession.SaveChangesAsync();

    return Results.Accepted();
});

app.Run();