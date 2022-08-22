namespace InventorySystem.Events;

public record ProductRegistered(Guid Id, string ProductCode, string Name);