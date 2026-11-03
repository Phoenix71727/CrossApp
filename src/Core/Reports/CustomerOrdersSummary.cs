namespace Core.Reports;

public sealed record CustomerOrdersSummary(
    string CustomerId,
    string CustomerName,
    string Email,
    int OrdersCount,
    decimal TotalSpent);
