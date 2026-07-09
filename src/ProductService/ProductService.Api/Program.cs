using Microsoft.EntityFrameworkCore;
using ProductServiceApi;
using ProductServiceApi.Extensions;
using ProductServiceApplication.DTOs;
using ProductServiceApplication.Interfaces;
using ProductServiceApplication.Services;
using ProductServiceInfrastructure.Messaging;
using ProductServiceInfrastructure.Persistence;
using ProductServiceInfrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));
builder.Services.AddCors(o =>
    
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddApiDocumentation();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddApplicationServices();
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration);

var app = builder.Build();

app.UseCors();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.UseHttpsRedirection();

var products = app.MapGroup("/products").WithTags("Products");

products.MapGet("/", async (ProductService svc, CancellationToken ct) =>
    Results.Ok(await svc.GetAllAsync(ct)))
    .WithSummary("Get all products")
    .WithDescription("Returns a list of all available products.")
    .Produces<IEnumerable<ProductResponse>>();

products.MapGet("/{id:guid}", async (Guid id, ProductService svc, CancellationToken ct) =>
    await svc.GetByIdAsync(id, ct) is { } p ? Results.Ok(p) : Results.NotFound())
    .WithName("GetProductById")
    .WithSummary("Get a product by ID")
    .WithDescription("Returns a single product. Returns 404 if not found.")
    .Produces<ProductResponse>()
    .Produces(StatusCodes.Status404NotFound);

products.MapPost("/", async (CreateProductRequest req, ProductService svc, CancellationToken ct) =>
{
    var result = await svc.CreateAsync(req, ct);
    return Results.CreatedAtRoute("GetProductById", new { id = result.Id }, result);
})
    .WithSummary("Create a product")
    .WithDescription("Creates a new product and returns it with its generated ID.")
    .Produces<ProductResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

products.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest req, ProductService svc, CancellationToken ct) =>
    await svc.UpdateAsync(id, req, ct) is { } p ? Results.Ok(p) : Results.NotFound())
    .WithSummary("Update a product")
    .WithDescription("Updates an existing product. Returns 404 if not found.")
    .Produces<ProductResponse>()
    .Produces(StatusCodes.Status404NotFound);

products.MapDelete("/{id:guid}", async (Guid id, ProductService svc, CancellationToken ct) =>
    await svc.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound())
    .WithSummary("Delete a product")
    .WithDescription("Deletes a product by ID. Returns 404 if not found.")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.Run();
