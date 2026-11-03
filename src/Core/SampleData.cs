using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("P-001", "Ноутбук Lenovo ThinkPad", 32500.00m, "Комп'ютери"),
        Product.Create("P-002", "Монітор Dell 27 4K", 14200.50m, "Периферія"),
        Product.Create("P-003", "Механічна клавіатура Keychron", 4100.00m, "Аксесуари"),
        Product.Create("P-004", "Бездротова миша Logitech MX", 3450.75m, "Аксесуари"),
        Product.Create("P-005", "Навушники Sony WH-1000XM5", 12999.00m, "Аудіо"),
        Product.Create("P-006", "Веб-камера Logitech Brio 4K", 6800.00m, "Периферія"),
        Product.Create("P-007", "Мікрофон Rode NT-USB", 5400.00m, "Аудіо"),
        Product.Create("P-008", "Килимок для миші SteelSeries", 850.00m, "Аксесуари"),
        Product.Create("P-009", "USB-C хаб Satechi Multiport", 2900.00m, "Аксесуари"),
        Product.Create("P-010", "Крісло ергономічне Hator", 8900.00m, "Меблі"),
        Product.Create("P-011", "Кабель HDMI 2.1 Ultra High Speed", 650.00m, "Аксесуари"),
        Product.Create("P-012", "Графічний планшет Wacom One", 21500.00m, "Периферія"),
        Product.Create("P-013", "Блок живлення Corsair 850W", 5200.00m, "Комплектуючі"),
        Product.Create("P-014", "SSD диск Samsung 990 Pro 2TB", 7400.00m, "Комплектуючі"),
        Product.Create("P-015", "Оперативна пам'ять Kingston DDR5 32GB", 4800.00m, "Комплектуючі")
    ];

    public static IReadOnlyList<Customer> Customers() =>
    [
        Customer.Create("CUST-001", "Олександр Мельник", "melnyk@gmail.com"),
        Customer.Create("CUST-002", "Ірина Коваль", "koval@ukr.net"),
        Customer.Create("CUST-003", "Василь Бойко", "boyko@lnu.edu.ua"),
        Customer.Create("CUST-004", "Олена Ткаченко", "tkachenko@gmail.com"),
        Customer.Create("CUST-005", "Максим Кравченко", "kravchenko@gmail.com"),
        Customer.Create("CUST-006", "Світлана Шевченко", "shevchenko@meta.ua"),
        Customer.Create("CUST-007", "Андрій Бондар", "bondar@gmail.com"),
        Customer.Create("CUST-008", "Наталія Мороз", "moroz@ukr.net"),
        Customer.Create("CUST-009", "Дмитро Марченко", "marchenko@gmail.com"),
        Customer.Create("CUST-010", "Павло Мотлюк", "motlyuk@lnu.edu.ua"),
        Customer.Create("CUST-011", "Юрій Клименко", "klymenko@gmail.com")
    ];

    public static IReadOnlyList<Order> Orders()
    {
        List<Order> orders = [];
        var p = Products();

        var o1 = Order.Create("ORD-001", "CUST-001", DateTime.UtcNow.AddDays(-10));
        o1.AddLine(p[0].Id, p[0].Name, p[0].Price, 1);
        o1.AddLine(p[3].Id, p[3].Name, p[3].Price, 1);
        o1.Confirm();
        orders.Add(o1);

        var o2 = Order.Create("ORD-002", "CUST-002", DateTime.UtcNow.AddDays(-8));
        o2.AddLine(p[1].Id, p[1].Name, p[1].Price, 2);
        o2.Confirm();
        orders.Add(o2);

        var o3 = Order.Create("ORD-003", "CUST-003", DateTime.UtcNow.AddDays(-5));
        o3.AddLine(p[2].Id, p[2].Name, p[2].Price, 1);
        o3.AddLine(p[7].Id, p[7].Name, p[7].Price, 2);
        orders.Add(o3);

        var o4 = Order.Create("ORD-004", "CUST-004", DateTime.UtcNow.AddDays(-4));
        o4.AddLine(p[4].Id, p[4].Name, p[4].Price, 1);
        o4.Confirm();
        orders.Add(o4);

        var o5 = Order.Create("ORD-005", "CUST-001", DateTime.UtcNow.AddDays(-3));
        o5.AddLine(p[9].Id, p[9].Name, p[9].Price, 1);
        o5.Cancel();
        orders.Add(o5);

        var o6 = Order.Create("ORD-006", "CUST-005", DateTime.UtcNow.AddDays(-2));
        o6.AddLine(p[5].Id, p[5].Name, p[5].Price, 1);
        o6.AddLine(p[6].Id, p[6].Name, p[6].Price, 1);
        orders.Add(o6);

        var o7 = Order.Create("ORD-007", "CUST-002", DateTime.UtcNow.AddDays(-2));
        o7.AddLine(p[10].Id, p[10].Name, p[10].Price, 3);
        o7.Confirm();
        orders.Add(o7);

        var o8 = Order.Create("ORD-008", "CUST-006", DateTime.UtcNow.AddDays(-1));
        o8.AddLine(p[13].Id, p[13].Name, p[13].Price, 1);
        o8.AddLine(p[14].Id, p[14].Name, p[14].Price, 2);
        orders.Add(o8);

        var o9 = Order.Create("ORD-009", "CUST-007", DateTime.UtcNow.AddDays(-1));
        o9.AddLine(p[11].Id, p[11].Name, p[11].Price, 1);
        o9.Confirm();
        orders.Add(o9);

        var o10 = Order.Create("ORD-010", "CUST-008", DateTime.UtcNow);
        o10.AddLine(p[8].Id, p[8].Name, p[8].Price, 2);
        orders.Add(o10);

        var o11 = Order.Create("ORD-011", "CUST-003", DateTime.UtcNow.AddDays(-3));
        o11.AddLine(p[12].Id, p[12].Name, p[12].Price, 2);
        o11.Confirm();
        orders.Add(o11);

        var o12 = Order.Create("ORD-012", "CUST-009", DateTime.UtcNow.AddDays(-2));
        o12.AddLine(p[1].Id, p[1].Name, p[1].Price, 1);
        o12.AddLine(p[10].Id, p[10].Name, p[10].Price, 2);
        o12.Cancel();
        orders.Add(o12);

        var o13 = Order.Create("ORD-013", "CUST-010", DateTime.UtcNow.AddDays(-1));
        o13.AddLine(p[0].Id, p[0].Name, p[0].Price, 2);
        o13.Confirm();
        orders.Add(o13);

        var o14 = Order.Create("ORD-014", "CUST-004", DateTime.UtcNow);
        o14.AddLine(p[7].Id, p[7].Name, p[7].Price, 1);
        orders.Add(o14);

        var o15 = Order.Create("ORD-015", "CUST-005", DateTime.UtcNow);
        o15.AddLine(p[3].Id, p[3].Name, p[3].Price, 3);
        o15.Confirm();
        orders.Add(o15);

        var o16 = Order.Create("ORD-016", "CUST-006", DateTime.UtcNow);
        o16.AddLine(p[2].Id, p[2].Name, p[2].Price, 2);
        o16.Cancel();
        orders.Add(o16);

        var o17 = Order.Create("ORD-017", "CUST-007", DateTime.UtcNow);
        o17.AddLine(p[4].Id, p[4].Name, p[4].Price, 1);
        o17.AddLine(p[8].Id, p[8].Name, p[8].Price, 1);
        orders.Add(o17);

        return orders.AsReadOnly();
    }
}
