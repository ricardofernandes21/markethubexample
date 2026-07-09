namespace AmazonConnector.Clients;

using System.Net.Http.Json;
using System.Text;
using Contracts.Messages;

public sealed class AmazonApiClient(HttpClient httpClient, ILogger<AmazonApiClient> logger) : IAmazonApiClient
{
    public async Task UpsertProductAsync(ProductUpdated product, CancellationToken ct = default)
    {
        var payload = new { product.SKU, product.Price, product.Stock };

        logger.LogDebug("PUT /products for SKU={SKU}", product.SKU);

        var response = await httpClient.PutAsJsonAsync("/products", payload, ct);

        logger.LogInformation(
            "Amazon API responded {Status} for SKU={SKU}",
            (int)response.StatusCode, product.SKU);

        response.EnsureSuccessStatusCode();
    }

    public async Task SubmitInventoryFeedAsync(string xml, CancellationToken ct = default)
    {
        using var content = new StringContent(xml, Encoding.UTF8, "application/xml");

        logger.LogDebug("POST /feeds/inventory ({Bytes} bytes)", xml.Length);

        var response = await httpClient.PostAsync("/feeds/inventory", content, ct);

        logger.LogInformation(
            "Amazon feed submission responded {Status}",
            (int)response.StatusCode);

        response.EnsureSuccessStatusCode();
    }
}
