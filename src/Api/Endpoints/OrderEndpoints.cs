using System.Security.Cryptography;
using Api.Contracts;
using Api.Infrastructure;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", Results<Ok<IReadOnlyList<OrderResponse>>, Ok<PagedOrdersResponse>>
            (int? page, int? pageSize, string? search, IOrderStore store) =>
        {
            IEnumerable<Order> query = store.List();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(o =>
                    o.Id.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.CustomerId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.Lines.Any(l => l.ProductName.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(search))
            {
                int currentPage = Math.Max(1, page ?? 1);
                int currentPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

                var filtered = query.ToList();
                int total = filtered.Count;
                var items = filtered
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .Select(o => o.ToApiResponse())
                    .ToList();

                return TypedResults.Ok(new PagedOrdersResponse(items, total, currentPage, currentPageSize));
            }

            return TypedResults.Ok<IReadOnlyList<OrderResponse>>(
                query.Select(o => o.ToApiResponse()).ToList());
        });

        group.MapGet("/{id}", Results<Ok<OrderResponse>, NotFound>
            (string id, IOrderStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToApiResponse())
                : TypedResults.NotFound());

        group.MapGet("/{id}/total", Results<Ok<OrderTotalResponse>, NotFound>
            (string id, IOrderStore store, IConfiguration config) =>
        {
            if (store.GetById(id) is not { } found)
                return TypedResults.NotFound();

            string currency = config["Orders:Currency"] ?? "UAH";
            return TypedResults.Ok(new OrderTotalResponse(found.Id, found.Total, currency));
        });

        group.MapPost("/", Results<Created<OrderResponse>, ValidationProblem>
            (SaveOrderRequest request, IOrderStore store) =>
        {
            var id = $"ORD-{Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToUpperInvariant()}";
            var created = Order.Create(id, request.CustomerId, request.CreatedAt);

            foreach (var line in request.Lines!)
                created.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);

            store.Add(created);
            return TypedResults.Created($"/api/orders/{created.Id}", created.ToApiResponse());
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>();

        group.MapPut("/{id}", Results<Ok<OrderResponse>, NotFound, ValidationProblem>
            (string id, SaveOrderRequest request, IOrderStore store) =>
        {
            if (store.GetById(id) is not { } existing)
                return TypedResults.NotFound();

            // PUT повністю замінює стан: створюємо новий валідний Order із тим самим Id
            var replacement = Order.Create(existing.Id, request.CustomerId, request.CreatedAt ?? existing.CreatedAt);
            foreach (var line in request.Lines!)
                replacement.AddLine(line.ProductId, line.ProductName, line.Price, line.Quantity);

            store.Update(replacement);
            return TypedResults.Ok(replacement.ToApiResponse());
        })
        .AddEndpointFilter<ValidationFilter<SaveOrderRequest>>();

        group.MapPost("/{id}/confirm", Results<Ok<OrderResponse>, NotFound, Conflict>
            (string id, IOrderStore store) =>
        {
            if (store.GetById(id) is not { } found)
                return TypedResults.NotFound();

            // Повторне підтвердження викличе DomainException -> ApiExceptionHandler поверне 409 Conflict
            found.Confirm();
            store.Update(found);
            return TypedResults.Ok(found.ToApiResponse());
        });

        group.MapDelete("/{id}", Results<NoContent, NotFound>
            (string id, IOrderStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }
}
