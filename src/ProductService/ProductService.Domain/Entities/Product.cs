namespace ProductServiceDomain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }
    public string SKU { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Product() { }

    public static Product Create(string sku, string name, decimal price, int stock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");

        var now = DateTime.UtcNow;
        return new Product
        {
            Id = Guid.NewGuid(),
            SKU = sku,
            Name = name,
            Price = price,
            Stock = stock,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, decimal price, int stock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");

        Name = name;
        Price = price;
        Stock = stock;
        UpdatedAt = DateTime.UtcNow;
    }
}
