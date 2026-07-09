namespace ProductService.Tests.Unit.Domain;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductServiceDomain.Entities;
using ProductServiceInfrastructure.Persistence;
using ProductServiceInfrastructure.Repositories;

public sealed class ProductRepositoryTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    private static string Sku(string tag) => $"{tag}-{Guid.NewGuid():N}"[..20];

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsPersistedProduct()
    {
        var db = Guid.NewGuid().ToString();
        var product = Product.Create(Sku("ADD"), "Integration Jeans", 79.99m, 50);

        await using var writeCtx = CreateContext(db);
        await new ProductRepository(writeCtx).AddAsync(product);

        await using var readCtx = CreateContext(db);
        var found = await new ProductRepository(readCtx).GetByIdAsync(product.Id);

        found.Should().NotBeNull();
        found!.SKU.Should().Be(product.SKU);
        found.Name.Should().Be("Integration Jeans");
        found.Price.Should().Be(79.99m);
        found.Stock.Should().Be(50);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPreviouslyAddedProducts()
    {
        var db = Guid.NewGuid().ToString();
        var p1 = Product.Create(Sku("ALL1"), "Hoodie", 59m, 10);
        var p2 = Product.Create(Sku("ALL2"), "T-Shirt", 29m, 20);

        await using var writeCtx = CreateContext(db);
        var writeRepo = new ProductRepository(writeCtx);
        await writeRepo.AddAsync(p1);
        await writeRepo.AddAsync(p2);

        await using var readCtx = CreateContext(db);
        var all = (await new ProductRepository(readCtx).GetAllAsync()).ToList();

        all.Should().Contain(p => p.Id == p1.Id);
        all.Should().Contain(p => p.Id == p2.Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var db = Guid.NewGuid().ToString();
        var product = Product.Create(Sku("UPD"), "Original Name", 49m, 5);

        await using var writeCtx = CreateContext(db);
        await new ProductRepository(writeCtx).AddAsync(product);

        product.Update("Updated Name", 89m, 99);

        await using var updateCtx = CreateContext(db);
        await new ProductRepository(updateCtx).UpdateAsync(product);

        await using var readCtx = CreateContext(db);
        var found = await new ProductRepository(readCtx).GetByIdAsync(product.Id);

        found!.Name.Should().Be("Updated Name");
        found.Price.Should().Be(89m);
        found.Stock.Should().Be(99);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var db = Guid.NewGuid().ToString();
        var product = Product.Create(Sku("DEL"), "To Delete", 19m, 1);

        await using var writeCtx = CreateContext(db);
        await new ProductRepository(writeCtx).AddAsync(product);

        await using var deleteCtx = CreateContext(db);
        await new ProductRepository(deleteCtx).DeleteAsync(product.Id);

        await using var readCtx = CreateContext(db);
        var found = await new ProductRepository(readCtx).GetByIdAsync(product.Id);

        found.Should().BeNull();
    }

    [Fact(Skip = "InMemory provider does not enforce unique index constraints — requires a real database.")]
    public async Task AddAsync_DuplicateSku_ThrowsDbUpdateException()
    {
        var db = Guid.NewGuid().ToString();
        var sku = Sku("DUP");
        var first  = Product.Create(sku, "First",  19m, 1);
        var second = Product.Create(sku, "Second", 29m, 2);

        await using var ctx1 = CreateContext(db);
        await new ProductRepository(ctx1).AddAsync(first);

        await using var ctx2 = CreateContext(db);
        var act = async () => await new ProductRepository(ctx2).AddAsync(second);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
