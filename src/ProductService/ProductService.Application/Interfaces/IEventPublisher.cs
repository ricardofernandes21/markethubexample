namespace ProductServiceApplication.Interfaces;

public interface IEventPublisher
{
    Task PublishProductUpdatedAsync(Guid productId, string sku, decimal price, int stock, CancellationToken ct = default);
}
