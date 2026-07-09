namespace ProductService.Tests.Unit.Services;

using FluentAssertions;
using Moq;
using ProductServiceApplication.DTOs;
using ProductServiceApplication.Interfaces;
using ProductServiceDomain.Entities;
using Svc = ProductServiceApplication.Services.ProductService;

public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<IEventPublisher> _events = new();
    private readonly Svc _sut;

    public ProductServiceTests()
    {
        _sut = new Svc(_repo.Object, _events.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldPersistAndPublishEvent()
    {
        var req = new CreateProductRequest("JJ-001", "Slim Fit Jeans", 79.99m, 50);

        var result = await _sut.CreateAsync(req);

        result.SKU.Should().Be("JJ-001");
        result.Name.Should().Be("Slim Fit Jeans");
        result.Price.Should().Be(79.99m);
        result.Stock.Should().Be(50);
        result.Id.Should().NotBeEmpty();

        _repo.Verify(r => r.AddAsync(It.IsAny<Product>(), default), Times.Once);
        _events.Verify(e => e.PublishProductUpdatedAsync(
            result.Id, "JJ-001", 79.99m, 50, default), Times.Once);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsResponse()
    {
        var product = Product.Create("JJ-002", "Hoodie", 59.99m, 20);
        _repo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.SKU.Should().Be("JJ-002");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
             .ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsMappedResponses()
    {
        var products = new[]
        {
            Product.Create("SKU-1", "Jeans", 79m, 10),
            Product.Create("SKU-2", "T-Shirt", 29m, 5)
        };
        _repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(products);

        var result = (await _sut.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result.Select(r => r.SKU).Should().BeEquivalentTo("SKU-1", "SKU-2");
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenFound_UpdatesAndPublishesEvent()
    {
        var product = Product.Create("JJ-003", "Old Name", 49.99m, 30);
        _repo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var req = new UpdateProductRequest("New Name", 59.99m, 25);
        var result = await _sut.UpdateAsync(product.Id, req);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Price.Should().Be(59.99m);
        result.Stock.Should().Be(25);

        _repo.Verify(r => r.UpdateAsync(product, default), Times.Once);
        _events.Verify(e => e.PublishProductUpdatedAsync(
            product.Id, "JJ-003", 59.99m, 25, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
             .ReturnsAsync((Product?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateProductRequest("X", 1m, 1));

        result.Should().BeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Product>(), default), Times.Never);
        _events.Verify(e => e.PublishProductUpdatedAsync(
            It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>(), default),
            Times.Never);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
    {
        var product = Product.Create("JJ-004", "Jacket", 99m, 5);
        _repo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        var result = await _sut.DeleteAsync(product.Id);

        result.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(product.Id, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
             .ReturnsAsync((Product?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), default), Times.Never);
    }
}
