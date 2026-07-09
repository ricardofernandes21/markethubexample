using System.Net;
using AmazonConnector.Clients;
using AmazonConnector.Consumers;
using MassTransit;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(lc =>
    lc.ReadFrom.Configuration(builder.Configuration));

builder.Services
    .AddHttpClient<IAmazonApiClient, AmazonApiClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["AmazonApi:BaseUrl"] ?? "http://localhost:5200");
    })
    .AddResilienceHandler("amazon-retry", (pipeline, ctx) =>
    {
        var logger = ctx.ServiceProvider.GetRequiredService<ILogger<AmazonApiClient>>();

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = false,
            ShouldHandle = args => ValueTask.FromResult(
                args.Outcome.Result?.StatusCode is
                    HttpStatusCode.InternalServerError or
                    HttpStatusCode.TooManyRequests or
                    HttpStatusCode.ServiceUnavailable
                || args.Outcome.Exception is not null),
            OnRetry = args =>
            {
                var status = args.Outcome.Result?.StatusCode.ToString()
                             ?? args.Outcome.Exception?.GetType().Name;

                logger.LogWarning(
                    "Amazon API retry {Attempt}/{Max} after {Status} — waiting {Delay}s",
                    args.AttemptNumber + 1,
                    4,
                    status,
                    args.RetryDelay.TotalSeconds);

                return ValueTask.CompletedTask;
            }
        });
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductUpdatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

        cfg.ReceiveEndpoint("product-updated", e =>
        {
            // Message-level retry: 5 attempts before moving to product-updated_error (DLQ)
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(16)));

            e.ConfigureConsumer<ProductUpdatedConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
