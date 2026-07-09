namespace AmazonConnector.Consumers;

using AmazonConnector.Clients;
using AmazonConnector.Feeds;
using Contracts.Messages;
using MassTransit;

public sealed class ProductUpdatedConsumer(
    IAmazonApiClient amazonApiClient,
    ILogger<ProductUpdatedConsumer> logger) : IConsumer<ProductUpdated>
{
    private const string MerchantId = "MARKETHUB_SELLER";

    public async Task Consume(ConsumeContext<ProductUpdated> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Received ProductUpdated — SKU={SKU} Price={Price} Stock={Stock}",
            msg.SKU, msg.Price, msg.Stock);

        try
        {
            // JSON REST call — real-time price/stock sync
            await amazonApiClient.UpsertProductAsync(msg, context.CancellationToken);

            // XML feed submission — Amazon-style inventory feed
            var xml = AmazonFeedBuilder.BuildInventoryFeed(MerchantId, msg);
            await amazonApiClient.SubmitInventoryFeedAsync(xml, context.CancellationToken);

            logger.LogInformation("Synced SKU={SKU} to Amazon (REST + feed)", msg.SKU);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex,
                "All retries exhausted for SKU={SKU} — message will move to error queue",
                msg.SKU);
            throw;
        }
    }
}
