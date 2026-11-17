using System.Reflection;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

namespace Cli.Demos;

public static class Lab9Demo
{
    public static void Run(string[] args)
    {
        var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")
            .AddCommandLine(args)
            .Build();

        var options = config.Get<AppOptions>() ?? new AppOptions();

        var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
        var storage = new StorageOptions
        {
            DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
            FileName = options.Storage.FileName
        };

        var resolvedDataPath = storage.ResolveFullPath(AppContext.BaseDirectory);
        var apiKey = config["Integration:ApiKey"];

        Console.WriteLine($"Файл даних : {resolvedDataPath}");
        Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");

        bool isExportCommand = args.Contains("export", StringComparer.OrdinalIgnoreCase) ||
                               args.Contains("--export", StringComparer.OrdinalIgnoreCase);

        bool isImportCommand = args.Contains("import", StringComparer.OrdinalIgnoreCase) ||
                               args.Contains("--import", StringComparer.OrdinalIgnoreCase);

        try
        {
            var store = new FileOrderStore(resolvedDataPath);

            // Якщо передано команду export або запуск без конкретної підкоманди іншої лабораторної
            if (isExportCommand || (!isImportCommand && args.Length == 0) || args.Contains("lab9", StringComparer.OrdinalIgnoreCase))
            {
                var sampleOrders = CreateSampleOrders();
                store.SaveAll(sampleOrders);
                Console.WriteLine($"Експортовано {sampleOrders.Count} записи.");
            }

            if (isImportCommand)
            {
                var loaded = store.List();
                Console.WriteLine($"Імпортовано {loaded.Count} записів з файлу:");
                foreach (var ord in loaded)
                {
                    Console.WriteLine($"  - Замовлення #{ord.Id} ({ord.CustomerId}), позицій: {ord.Lines.Count}, сума: {ord.Total} {options.Orders.Currency}");
                    foreach (var line in ord.Lines)
                    {
                        Console.WriteLine($"      * {line.ProductName} x{line.Quantity} @ {line.Price} {options.Orders.Currency}");
                    }
                }
            }
        }
        catch (FormatException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка читання даних: {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Помилка виконання операції: {ex.Message}");
            Console.ResetColor();
        }
    }

    public static List<Order> CreateSampleOrders()
    {
        var o1 = Order.Create("ORD-001", "CUST-KIEV-1");
        o1.AddLine("PROD-01", "Цемент М400 (50 кг)", 245.50m, 20);
        o1.AddLine("PROD-02", "Пісок будівельний фасований", 85.00m, 50);

        var o2 = Order.Create("ORD-002", "CUST-LVIV-2");
        o2.AddLine("PROD-03", "Цегла керамічна рядова", 12.30m, 500);

        var o3 = Order.Create("ORD-003", "CUST-ODESA-3");
        o3.AddLine("PROD-04", "Фарба акрилова фасадна 'Сніжка'", 650.00m, 3);
        o3.AddLine("PROD-05", "Грунтовка глибокого проникнення", 180.00m, 5);

        return [o1, o2, o3];
    }
}
