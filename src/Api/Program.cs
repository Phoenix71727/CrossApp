using Api.Contracts;
using Api.Endpoints;
using Api.Infrastructure;
using Api.Validators;
using Core.Abstractions;
using Core.Domain;
using Core.Storage;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); // RFC 9457
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton<IValidator<SaveOrderRequest>, SaveOrderRequestValidator>();
builder.Services.AddSingleton<IOrderStore>(_ => new InMemoryOrderStore(CreateSeedOrders()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" })).WithTags("Health");
app.MapOrders();

app.Run();

static IEnumerable<Order> CreateSeedOrders()
{
    var o1 = Order.Create("ORD-1001", "CUST-KIEV-1", new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc));
    o1.AddLine("PROD-01", "Цемент М400 (50 кг)", 245.50m, 10);
    o1.AddLine("PROD-02", "Пісок будівельний фасований", 85.00m, 20);

    var o2 = Order.Create("ORD-1002", "CUST-LVIV-2", new DateTime(2026, 11, 21, 12, 30, 0, DateTimeKind.Utc));
    o2.AddLine("PROD-03", "Цегла керамічна рядова", 12.30m, 200);

    return [o1, o2];
}

public partial class Program;
