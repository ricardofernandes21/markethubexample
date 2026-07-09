namespace ProductServiceApplication.DTOs;

public sealed record CreateProductRequest(string SKU, string Name, decimal Price, int Stock);
