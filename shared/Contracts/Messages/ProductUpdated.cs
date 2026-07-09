namespace Contracts.Messages;

public record ProductUpdated(
    Guid ProductId,
    string SKU,
    decimal Price,
    int Stock);
