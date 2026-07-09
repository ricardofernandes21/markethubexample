namespace ProductServiceInfrastructure.Messaging;

using Contracts.Messages;
using MassTransit;
using ProductServiceApplication.Interfaces;

public sealed class EventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishProductUpdatedAsync(Guid productId, string sku, decimal price, int stock, CancellationToken ct = default)
        => publishEndpoint.Publish(new ProductUpdated(productId, sku, price, stock), ct);
}
