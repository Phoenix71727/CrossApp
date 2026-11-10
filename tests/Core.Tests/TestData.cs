using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static IReadOnlyList<Order> Orders()
    {
        var o1 = Order.Create("ORD-T01", "CUST-01", DateTime.UtcNow.AddDays(-3));
        o1.AddLine("P-01", "Тестовий монітор", 15000m, 1);
        o1.Confirm();

        var o2 = Order.Create("ORD-T02", "CUST-02", DateTime.UtcNow.AddDays(-2));
        o2.AddLine("P-02", "Тестовий ноутбук A", 25000m, 1);
        o2.Confirm();

        var o3 = Order.Create("ORD-T03", "CUST-01", DateTime.UtcNow.AddDays(-1));
        o3.AddLine("P-03", "Тестова миша", 5000m, 1);
        // лишається Draft

        var o4 = Order.Create("ORD-T04", "CUST-03", DateTime.UtcNow.AddDays(-1));
        o4.AddLine("P-04", "Тестовий ноутбук B", 25000m, 1);
        o4.Confirm();

        return [o1, o2, o3, o4];
    }

    public static IReadOnlyList<Customer> Customers() =>
    [
        Customer.Create("CUST-01", "Тестовий Клієнт 1", "test1@example.com"),
        Customer.Create("CUST-02", "Тестовий Клієнт 2", "test2@example.com"),
        Customer.Create("CUST-03", "Тестовий Клієнт 3", "test3@example.com")
    ];
}
