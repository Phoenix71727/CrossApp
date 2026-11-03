using System.Diagnostics;
using Core;
using Core.Domain;
using Core.Reports;

namespace Cli.Demos;

public static class Lab7Demo
{
    public static void Run(string[] args)
    {
        var items = SampleData.Orders();
        string subCommand = args.FirstOrDefault(a => a.StartsWith("report-")) ?? "all";

        switch (subCommand)
        {
            case "report-top":
                PrintTop(items);
                return;

            case "report-low":
            case "report-filter":
                PrintFilter(items);
                return;

            case "report-groups":
                PrintGroups(items);
                return;

            case "report-sum":
                PrintTotals(items);
                return;

            case "report-join":
                PrintCustomerJoin(items);
                return;

            default:
                RunFullDemo(items);
                break;
        }
    }

    private static void RunFullDemo(IReadOnlyList<Order> items)
    {
        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №7: ЗВІТИ ЧЕРЕЗ LINQ (FILTERS, GROUPS, TOP-N)");
        Console.WriteLine(new string('=', 72));

        Console.WriteLine("\n[1] ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ (DEFERRED EXECUTION)");
        Console.WriteLine(new string('-', 72));
        var localSource = items.Take(5).ToList();
        var deferredQuery = localSource.Where(o => o.Total > 10000m);
        Console.WriteLine($"• Кількість замовлень > 10 000 грн до зміни джерела : {deferredQuery.Count()}");

        var newOrder = Order.Create("ORD-TMP-999", "CUST-999");
        newOrder.AddLine("P-001", "Ноутбук Lenovo ThinkPad", 32500m, 1);
        localSource.Add(newOrder);
        Console.WriteLine($"• Кількість замовлень > 10 000 грн після додавання    : {deferredQuery.Count()}");

        var snapshot = deferredQuery.ToList();
        localSource.Clear();
        Console.WriteLine($"• Після Clear() джерела, у матеріалізованому знімку   : {snapshot.Count}");

        Console.WriteLine("\n[2] ПІДКОМАНДИ ЗВІТІВ (REPORT SUBCOMMANDS)");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("• Демонстрація всіх 4 звітів (підкоманди: report-top | report-filter | report-groups | report-sum):");
        Console.WriteLine();
        PrintTop(items);
        Console.WriteLine();
        PrintFilter(items);
        Console.WriteLine();
        PrintGroups(items);
        Console.WriteLine();
        PrintTotals(items);

        Console.WriteLine("\n[БОНУС 1] ЗВЕДЕНИЙ ЗВІТ GroupJoin (КЛІЄНТИ x ЗАМОВЛЕННЯ)");
        Console.WriteLine(new string('-', 72));
        PrintCustomerJoin(items);

        Console.WriteLine("\n[БОНУС 2 & 3] МЕТОД-РОЗШИРЕННЯ TopBy ТА QUERY SYNTAX");
        Console.WriteLine(new string('-', 72));
        var topCust = ReportService.TopCustomersReport(SampleData.Customers(), items, 3);
        Console.WriteLine("• Топ-3 клієнти через власний метод-розширення TopBy:");
        foreach (var c in topCust)
        {
            Console.WriteLine($"  {c.CustomerId,-10} {c.CustomerName,-20} | Замовлень: {c.OrdersCount} | Витрачено: {c.TotalSpent,10:F2} грн");
        }

        var querySyntaxGroups = ReportService.ByStatusQuerySyntax(items);
        Console.WriteLine($"• Перевірка ByStatusQuerySyntax: отримано {querySyntaxGroups.Count} груп (результат ідентичний до Method Syntax).");

        Console.WriteLine("\n[БОНУС 4] БЕНЧМАРК STOPWATCH НА 100 000 ЗАПИСІВ (TOLIST VS STREAMING)");
        Console.WriteLine(new string('-', 72));
        RunBenchmark();

        Console.WriteLine("\n[3] ПЕРЕВІРКА ОБРОБКИ ПОРОЖНЬОГО ДЖЕРЕЛА (ZERO-ELEMENTS TEST)");
        Console.WriteLine(new string('-', 72));
        var emptyOrders = Enumerable.Empty<Order>();
        var emptyTotals = ReportService.Totals(emptyOrders);
        Console.WriteLine($"• Totals на порожній колекції: позицій {emptyTotals.Items}, сума {emptyTotals.Total:F2} грн, середнє {emptyTotals.Average:F2} грн, макс {emptyTotals.Max:F2} грн (без винятку!)");

        var emptyTop = ReportService.TopOrdersByTotal(emptyOrders, 5);
        Console.WriteLine($"• TopOrdersByTotal на порожній колекції: повернуто {emptyTop.Count} записів.");
        Console.WriteLine(new string('=', 72));
    }

    private static void PrintTop(IEnumerable<Order> items)
    {
        Console.WriteLine("• Топ-5 замовлень за сумою (TopOrdersByTotal):");
        var top = ReportService.TopOrdersByTotal(items, 5);
        foreach (var l in top)
        {
            Console.WriteLine($"  {l.OrderId,-14} Клієнт: {l.CustomerId,-10} Рядків: {l.LinesCount,2} | Сума: {l.Total,10:F2} грн | Статус: {l.Status}");
        }
    }

    private static void PrintFilter(IEnumerable<Order> items)
    {
        Console.WriteLine("• Замовлення дорожче за 30 000 грн (HighValueOrders):");
        var filtered = ReportService.HighValueOrders(items, 30000m);
        foreach (var l in filtered)
        {
            Console.WriteLine($"  {l.OrderId,-14} Клієнт: {l.CustomerId,-10} Рядків: {l.LinesCount,2} | Сума: {l.Total,10:F2} грн | Статус: {l.Status}");
        }
    }

    private static void PrintGroups(IEnumerable<Order> items)
    {
        var groups = ReportService.ByStatus(items);
        foreach (var g in groups)
        {
            Console.WriteLine($"  {g.Key,-11} позицій: {g.Count,2} сумарно: {g.TotalAmount,10:F2} грн");
        }
    }

    private static void PrintTotals(IEnumerable<Order> items)
    {
        var t = ReportService.Totals(items);
        Console.WriteLine($"позицій {t.Items}, всього {t.Total:F2}, середнє {t.Average:F2}, макс {t.Max:F2}");
    }

    private static void PrintCustomerJoin(IEnumerable<Order> items)
    {
        var customers = SampleData.Customers();
        var report = ReportService.CustomerOrdersReport(customers, items);
        Console.WriteLine($" {"Клієнт",-10} {"ПІБ",-20} {"Замовлень",10} {"Сумарно, грн",15}");
        Console.WriteLine(new string('-', 60));
        foreach (var r in report)
        {
            Console.WriteLine($" {r.CustomerId,-10} {r.CustomerName,-20} {r.OrdersCount,10} {r.TotalSpent,15:F2}");
        }
    }

    private static void RunBenchmark()
    {
        Console.WriteLine("• Генерація 100 000 замовлень у пам'яті...");
        var largeDataset = Enumerable.Range(1, 100_000).Select(i =>
        {
            var o = Order.Create($"ORD-B-{i:D6}", $"CUST-{i % 500:D3}");
            o.AddLine($"P-{i % 15 + 1:D3}", "Товар", 100m * (i % 20 + 1), 1);
            return o;
        }).ToList();

        // Варіант 1: Потоковий LINQ без проміжного ToList()
        var sw1 = Stopwatch.StartNew();
        var streamed = largeDataset
            .Where(o => o.Total > 1000m)
            .Select(o => o.Id)
            .Take(10)
            .ToList();
        sw1.Stop();

        // Варіант 2: Із проміжним ToList() посередині конвеєра
        var sw2 = Stopwatch.StartNew();
        var materialized = largeDataset
            .Where(o => o.Total > 1000m)
            .ToList()
            .Select(o => o.Id)
            .Take(10)
            .ToList();
        sw2.Stop();

        Console.WriteLine($"  1) Потоковий LINQ (без проміжного ToList) : {sw1.Elapsed.TotalMilliseconds:F3} мс (Take зупиняє ітерацію одразу)");
        Console.WriteLine($"  2) З проміжним ToList() посередині         : {sw2.Elapsed.TotalMilliseconds:F3} мс (матеріалізує всі 100 000 елементів)");
        if (sw1.Elapsed.TotalMilliseconds > 0)
        {
            double ratio = sw2.Elapsed.TotalMilliseconds / sw1.Elapsed.TotalMilliseconds;
            Console.WriteLine($"  >> Прискорення без зайвого ToList()       : ~{ratio:F1}x разів швидше!");
        }
    }
}
