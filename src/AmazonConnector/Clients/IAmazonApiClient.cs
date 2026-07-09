namespace AmazonConnector.Clients;

using Contracts.Messages;

public interface IAmazonApiClient
{
    Task UpsertProductAsync(ProductUpdated product, CancellationToken ct = default);
    Task SubmitInventoryFeedAsync(string xml, CancellationToken ct = default);
}
