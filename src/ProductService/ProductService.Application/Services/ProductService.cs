namespace ProductServiceApplication.Services;

using ProductServiceApplication.DTOs;
using ProductServiceApplication.Interfaces;
using ProductServiceDomain.Entities;

public sealed class ProductService(IProductRepository repository, IEventPublisher eventPublisher)
{
    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
        => (await repository.GetAllAsync()).Select(ToResponse);

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await repository.GetByIdAsync(id);
        return product is null ? null : ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest req)
    {
        var product = Product.Create(req.SKU, req.Name, req.Price, req.Stock);
        await repository.AddAsync(product);
        await eventPublisher.PublishProductUpdatedAsync(product.Id, product.SKU, product.Price, product.Stock );
        return ToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, UpdateProductRequest req)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null) return null;

        product.Update(req.Name, req.Price, req.Stock);
        await repository.UpdateAsync(product);
        await eventPublisher.PublishProductUpdatedAsync(product.Id, product.SKU, product.Price, product.Stock);
        return ToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (await repository.GetByIdAsync(id) is null) return false;
        await repository.DeleteAsync(id);
        return true;
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.SKU, p.Name, p.Price, p.Stock, p.CreatedAt, p.UpdatedAt);
}
