using InventorySystem.Entities;

namespace InventorySystem.Events;

public record ShipmentReceived(
    Guid Id,
    int Quantity,
    StorageLocation Location);