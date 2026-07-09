namespace ProductServiceApplication.DTOs;

public sealed record UpdateProductRequest(string Name, decimal Price, int Stock);
