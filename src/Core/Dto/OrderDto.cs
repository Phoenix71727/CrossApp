using Core.Domain;

namespace Core.Dto;

public record OrderLineDto(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

public record OrderDto(
    string Id,
    string CustomerId,
    DateTime CreatedAt,
    OrderStatus Status,
    IReadOnlyList<OrderLineDto> Lines) : IEntityDto;
