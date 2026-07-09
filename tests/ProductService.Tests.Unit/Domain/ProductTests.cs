namespace ProductService.Tests.Unit.Domain;

using FluentAssertions;
using ProductServiceDomain.Entities;

public sealed class ProductTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        var before = DateTime.UtcNow;

        var product = Product.Create("JJ-001", "Slim Fit Jeans", 79.99m, 50);

        product.Id.Should().NotBeEmpty();
        product.SKU.Should().Be("JJ-001");
        product.Name.Should().Be("Slim Fit Jeans");
        product.Price.Should().Be(79.99m);
        product.Stock.Should().Be(50);
        product.CreatedAt.Should().BeOnOrAfter(before);
        product.UpdatedAt.Should().Be(product.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankSku_Throws(string sku)
    {
        var act = () => Product.Create(sku, "Jeans", 79m, 10);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        var act = () => Product.Create("SKU-1", name, 79m, 10);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativePrice_Throws()
    {
        var act = () => Product.Create("SKU-1", "Jeans", -0.01m, 10);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("price");
    }

    [Fact]
    public void Create_WithNegativeStock_Throws()
    {
        var act = () => Product.Create("SKU-1", "Jeans", 79m, -1);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("stock");
    }

    [Fact]
    public void Create_WithZeroPriceAndStock_Succeeds()
    {
        var act = () => Product.Create("SKU-1", "Jeans", 0m, 0);
        act.Should().NotThrow();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WithValidData_ChangesFieldsAndBumpsUpdatedAt()
    {
        var product = Product.Create("JJ-001", "Old Name", 49.99m, 30);
        var createdAt = product.CreatedAt;

        product.Update("New Name", 69.99m, 15);

        product.Name.Should().Be("New Name");
        product.Price.Should().Be(69.99m);
        product.Stock.Should().Be(15);
        product.SKU.Should().Be("JJ-001");        // SKU must not change
        product.CreatedAt.Should().Be(createdAt); // CreatedAt must not change
        product.UpdatedAt.Should().BeOnOrAfter(createdAt);
    }

    [Fact]
    public void Update_WithNegativePrice_Throws()
    {
        var product = Product.Create("SKU-1", "Jeans", 79m, 10);
        var act = () => product.Update("Jeans", -1m, 10);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("price");
    }

    [Fact]
    public void Update_WithNegativeStock_Throws()
    {
        var product = Product.Create("SKU-1", "Jeans", 79m, 10);
        var act = () => product.Update("Jeans", 79m, -1);
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("stock");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithBlankName_Throws(string name)
    {
        var product = Product.Create("SKU-1", "Jeans", 79m, 10);
        var act = () => product.Update(name, 79m, 10);
        act.Should().Throw<ArgumentException>();
    }
}
