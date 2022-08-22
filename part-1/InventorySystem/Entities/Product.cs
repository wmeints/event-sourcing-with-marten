using InventorySystem.Events;

namespace InventorySystem.Entities;

public record Product(
    Guid Id,
    string ProductCode,
    string Name,
    int Available,
    int Reserved,
    StorageLocation? Location,
    long Version = 0L)
{
    public static Product Create(ProductRegistered evt)
    {
        return new Product(evt.Id, evt.ProductCode, evt.Name, 0, 0, null, 1);
    }

    public static Product Apply(ShipmentReceived evt, Product current) =>
        current with
        {
            Available = evt.Quantity,
            Location = evt.Location,
            Version = current.Version + 1
        };

    public static Product Apply(ProductReserved evt, Product current) =>
        current with
        {
            Reserved = current.Reserved + evt.Quantity,
            Available = current.Available - evt.Quantity,
            Version = current.Version + 1
        };

    public static Product Apply(ProductPicked evt, Product current) =>
        current with
        {
            Reserved = current.Reserved - evt.Quantity,
            Version = current.Version + 1
        };
}