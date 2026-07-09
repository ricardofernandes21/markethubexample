namespace ProductServiceApplication.DTOs;

public sealed record ProductResponse(
    Guid Id,
    string SKU,
    string Name,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime UpdatedAt);
