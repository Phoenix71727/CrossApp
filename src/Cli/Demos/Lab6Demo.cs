using Cli.Notifications;
using Core;
using Core.Domain;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

namespace Cli.Demos;

public static class Lab6Demo
{
    public static void Run()
    {
        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №6: ДОМЕННІ ПОДІЇ, ДЕЛЕГАТИ ACTION/FUNC, ЛЯМБДИ");
        Console.WriteLine(new string('=', 72));

        var store = new InMemoryOrderStore();
        var service = new OrderService(store);

        string logPath = Directory.Exists("data")
            ? Path.Combine("logs", "app.log")
            : Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.WriteAllText(logPath, string.Empty);

        void OnOrderChanged(object? sender, OrderChangedEventArgs e)
        {
            try
            {
                string line = $"{e.At:HH:mm:ss} {e.Kind,-14} {e.OrderId,-16} Позицій: {e.LinesCount,2}  Сума: {e.Total,10:F2} грн";
                Console.WriteLine($"  [EVENT LOG] {line}");
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [Log Error]: Не вдалося записати в лог-файл: {ex.Message}");
                Console.ResetColor();
            }
        }

        Console.WriteLine("\n[1] НАЛАШТУВАННЯ ПІДПИСНИКІВ НА ПОДІЮ service.Changed");
        Console.WriteLine(new string('-', 72));

        service.Changed += OnOrderChanged;
        Console.WriteLine("  * Підписник №1: підписано локальним методом OnOrderChanged (консоль + logs/app.log)");

        int changes = 0;
        service.Changed += (_, _) => changes++;
        Console.WriteLine("  * Підписник №2: підписано лямбдою із замиканням над changes");

        Console.WriteLine("\n[2] СЦЕНАРІЙ У CLI: УСПІШНІ ЗМІНИ ТА СПОВІЩЕННЯ");
        Console.WriteLine(new string('-', 72));

        var products = SampleData.Products();
        var created = service.CreateOrder("CUST-101", "ORD-LAB6-001");
        service.AddLine(created.Id, products[0].Id, products[0].Name, products[0].Price, 1);
        service.Confirm(created.Id);

        Console.WriteLine($"\n>> Подій отримано (обидва підписники активні): {changes}");

        Console.WriteLine("\n[3] ВІДПИСКА ПІДПИСНИКА №1 (service.Changed -= OnOrderChanged)");
        Console.WriteLine(new string('-', 72));
        service.Changed -= OnOrderChanged;
        Console.WriteLine("  * Підписник №1 успішно відписаний від service.Changed.");
        Console.WriteLine("  * Виконуємо наступну операцію (створення замовлення ORD-LAB6-002)...");

        service.CreateOrder("CUST-102", "ORD-LAB6-002");
        Console.WriteLine($"\n>> Подій отримано після відписки: {changes} (+1 лише від лямбди-лічильника, логер мовчав)");

        Console.WriteLine("\n[4] СЦЕНАРІЙ ВІДМОВИ: ПОРУШЕННЯ ІНВАРІАНТУ (ПОДІЇ НЕ МАЄ БУТИ)");
        Console.WriteLine(new string('-', 72));
        int beforeErrorCount = changes;
        try
        {
            Console.WriteLine("  * Спроба підтвердити неіснуюче замовлення 'ORD-NOT-FOUND'...");
            service.Confirm("ORD-NOT-FOUND");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  [ОЧІКУВАНИЙ ВИНЯТОК]: {ex.GetType().Name} -> {ex.Message}");
            Console.ResetColor();
        }
        Console.WriteLine($"  * Лічильник подій: {changes} (до помилки було: {beforeErrorCount}) -> подія НЕ піднялася!");

        Console.WriteLine("\n[5] ДЕЛЕГАТ ЗАМІСТЬ ПОДІЇ: service.FindAll(Func<Order, bool>)");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("  * Пошук підтверджених замовлень (o => o.IsConfirmed) через лямбда-предикат:");
        var confirmed = service.FindAll(o => o.IsConfirmed);
        foreach (var o in confirmed)
        {
            Console.WriteLine($"    - Замовлення: {o.Id,-16} Клієнт: {o.CustomerId,-10} Сума: {o.Total,10:F2} грн  Статус: {o.Status}");
        }

        Console.WriteLine("\n[6] ПЕРЕВІРКА ФАЙЛУ logs/app.log");
        Console.WriteLine(new string('-', 72));
        if (File.Exists(logPath))
        {
            string[] logLines = File.ReadAllLines(logPath);
            Console.WriteLine($"  * Файл створено: {logPath}");
            Console.WriteLine($"  * Кількість записів у файлі: {logLines.Length} (операція після відписки сюди не потрапила)");
            foreach (string l in logLines)
            {
                Console.WriteLine($"    [FILE CONTENT] {l}");
            }
        }

        Console.WriteLine("\n[БОНУС 1] АЛЬТЕРНАТИВА: ПРИЙМАЧ СПОВІЩЕНЬ INotificationSink (РУЧНИЙ DI)");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("  * Створюємо ConsoleSink та передаємо в OrderService через конструктор:");
        var consoleSink = new ConsoleSink();
        var sinkStore = new InMemoryOrderStore();
        var sinkService = new OrderService(sinkStore, consoleSink);
        var sinkOrder = sinkService.CreateOrder("CUST-BONUS-1", "ORD-BONUS-01");
        sinkService.AddLine(sinkOrder.Id, products[1].Id, products[1].Name, products[1].Price, 1);
        sinkService.Confirm(sinkOrder.Id);

        Console.WriteLine("\n[БОНУС 2] ДЕЛЕГАТ Action<string> У МЕТОДІ ІМПОРТУ ДЛЯ ПРОПУЩЕНИХ РЯДКІВ");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("  * Читання sample.csv із передачею callback-делегата Action<string> (онлайн-сповіщення):");
        string sampleCsvPath = Path.Combine("data", "sample.csv");
        if (File.Exists(sampleCsvPath))
        {
            _ = ProductCsvImporter.Load(sampleCsvPath, onSkippedRow: err =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"    [ACTION CALLBACK] Пропущено битий рядок: {err}");
                Console.ResetColor();
            });
        }

        Console.WriteLine("\n[БОНУС 3] ТЕОРЕТИЧНИЙ АНАЛІЗ: ЧОМУ async-ОБРОБНИК ПОДІЇ — ЦЕ async void");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("  1. Чому async void:");
        Console.WriteLine("     - Сигнатура EventHandler<T> повертає void, а не Task;");
        Console.WriteLine("     - Мова C# не дозволяє події очікувати Task без власного складного делегата.");
        Console.WriteLine("  2. Чим це небезпечно:");
        Console.WriteLine("     - Fire-and-forget: джерело події (Changed?.Invoke) не знає, коли обробник завершиться;");
        Console.WriteLine("     - Неперехоплювані винятки: виняток усередині async void обходить зовнішній try/catch");
        Console.WriteLine("       і спричиняє падіння всього процесу (AppCrash / UnhandledException).");
        Console.WriteLine("     - Правило: усередині async void тіло ПОВИННО бути повністю в try/catch!");
        Console.WriteLine(new string('=', 72));
    }
}
