using InventorySystem.Events;
using InventorySystem.ReadModels;
using Marten.Events.Aggregation;

namespace InventorySystem.Projections;

public class ProductInfoProjection : SingleStreamAggregation<ProductInfo>
{
    public static ProductInfo Create(ProductRegistered evt) => new ProductInfo(evt.Id, evt.ProductCode, evt.Name, 0);

    public ProductInfo Apply(ShipmentReceived evt, ProductInfo current) => current with
    {
        Available = current.Available + evt.Quantity
    };
    
    public ProductInfo Apply(ProductReserved evt, ProductInfo current) => current with
    {
        Available = current.Available - evt.Quantity
    };
}