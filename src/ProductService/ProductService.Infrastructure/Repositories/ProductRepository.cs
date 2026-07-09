namespace ProductServiceInfrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using ProductServiceApplication.Interfaces;
using ProductServiceDomain.Entities;
using ProductServiceInfrastructure.Persistence;

public sealed class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id)
        => await context.Products.FindAsync(new object[] { id }).AsTask();

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await context.Products.AsNoTracking().ToListAsync();

    public async Task AddAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Guid id)
    {
        var product = await context.Products.FindAsync(new object[] { id });
        if (product is not null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}
