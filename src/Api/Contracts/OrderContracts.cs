using Core.Domain;

namespace Api.Contracts;

public sealed record OrderLineResponse(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity,
    decimal LineTotal);

public sealed record OrderResponse(
    string Id,
    string CustomerId,
    DateTime CreatedAt,
    string Status,
    decimal Total,
    IReadOnlyList<OrderLineResponse> Lines);

public sealed record OrderTotalResponse(
    string Id,
    decimal Total,
    string Currency);

public sealed record PagedOrdersResponse(
    IReadOnlyList<OrderResponse> Items,
    int Total,
    int Page,
    int PageSize);

public sealed record SaveOrderLineRequest(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

public sealed record SaveOrderRequest(
    string CustomerId,
    DateTime? CreatedAt,
    IReadOnlyList<SaveOrderLineRequest>? Lines);

public static class OrderContractMappings
{
    public static OrderResponse ToApiResponse(this Order order) => new(
        order.Id,
        order.CustomerId,
        order.CreatedAt,
        order.Status.ToString(),
        order.Total,
        order.Lines
            .Select(l => new OrderLineResponse(l.ProductId, l.ProductName, l.Price, l.Quantity, l.LineTotal))
            .ToList());
}
