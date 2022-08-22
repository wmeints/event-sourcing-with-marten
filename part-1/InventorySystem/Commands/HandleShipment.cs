using InventorySystem.Entities;

namespace InventorySystem.Commands;

public record HandleShipment(int Quantity, StorageLocation Location);