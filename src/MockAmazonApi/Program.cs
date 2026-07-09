var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

int[] statusPool = [200, 200, 200, 200, 200, 200, 200, 429, 500, 503];

IResult Simulate(string endpoint, ILogger logger, object? payload = null)
{
    var status = statusPool[Random.Shared.Next(statusPool.Length)];

    logger.LogInformation(
        "{Endpoint} → {Status} | {@Payload}",
        endpoint, status, payload);

    return status switch
    {
        429 => Results.StatusCode(429),
        500 => Results.StatusCode(500),
        503 => Results.StatusCode(503),
        _   => Results.Ok(new { message = "accepted", endpoint })
    };
}

app.MapPut("/products", (UpsertProductRequest req, ILogger<Program> logger) =>
    Simulate("PUT /products", logger, req));

app.MapPatch("/stock", (UpdateStockRequest req, ILogger<Program> logger) =>
    Simulate("PATCH /stock", logger, req));

app.MapPatch("/price", (UpdatePriceRequest req, ILogger<Program> logger) =>
    Simulate("PATCH /price", logger, req));

app.MapPost("/feeds/inventory", async (HttpRequest request, ILogger<Program> logger) =>
{
    using var reader = new StreamReader(request.Body);
    var xml = await reader.ReadToEndAsync();

    logger.LogInformation("Received inventory feed ({Bytes} bytes)", xml.Length);

    return Simulate("POST /feeds/inventory", logger, new { bytes = xml.Length });
});

app.Run();

record UpsertProductRequest(string SKU, decimal Price, int Stock);
record UpdateStockRequest(string SKU, int Stock);
record UpdatePriceRequest(string SKU, decimal Price);
