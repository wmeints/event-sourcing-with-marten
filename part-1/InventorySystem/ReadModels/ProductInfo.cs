namespace InventorySystem.ReadModels;

public record ProductInfo(Guid Id, string ProductCode, string Name, int Available);