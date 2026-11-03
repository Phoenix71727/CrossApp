using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    public static IReadOnlyList<OrderReportLine> HighValueOrders(IEnumerable<Order> source, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .Where(o => o.Total >= threshold)
            .OrderBy(o => o.Total)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Lines.Count, o.Total, o.Status.ToString()))
            .ToList();
    }

    public static IReadOnlyList<OrderReportLine> TopOrdersByTotal(IEnumerable<Order> source, int n)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .TopBy(o => o.Total, n)
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Lines.Count, o.Total, o.Status.ToString()))
            .ToList();
    }

    public static IReadOnlyList<CustomerOrdersSummary> TopCustomersReport(
        IEnumerable<Customer> customers,
        IEnumerable<Order> orders,
        int n)
    {
        return CustomerOrdersReport(customers, orders)
            .TopBy(c => c.TotalSpent, n)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByStatus(IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source
            .GroupBy(o => o.Status.ToString(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(o => o.Total)))
            .OrderByDescending(s => s.TotalAmount)
            .ToList();
    }

    public static IReadOnlyList<GroupSummary> ByStatusQuerySyntax(IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var query = from o in source
                    group o by o.Status.ToString() into g
                    orderby g.Sum(x => x.Total) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Total));

        return query.ToList();
    }

    public static (int Items, decimal Total, decimal Average, decimal Max) Totals(IEnumerable<Order> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.ToList();
        return list.Count == 0
            ? (0, 0m, 0m, 0m)
            : (list.Count,
               list.Sum(o => o.Total),
               list.Average(o => o.Total),
               list.Max(o => o.Total));
    }

    public static IReadOnlyList<CustomerOrdersSummary> CustomerOrdersReport(
        IEnumerable<Customer> customers,
        IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(orders);

        return customers
            .GroupJoin(
                orders,
                c => c.Id,
                o => o.CustomerId,
                (c, clientOrders) => new CustomerOrdersSummary(
                    c.Id,
                    c.FullName,
                    c.Email,
                    clientOrders.Count(),
                    clientOrders.Sum(o => o.Total)))
            .OrderByDescending(s => s.TotalSpent)
            .ThenBy(s => s.CustomerId, StringComparer.Ordinal)
            .ToList();
    }
}
