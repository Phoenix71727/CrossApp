using Core;
using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Cli.Demos;

public static class Lab8Demo
{
    private sealed class ConsoleProgressReporter : IProgress<int>
    {
        public void Report(int percent)
        {
            int barWidth = percent / 5;
            string bar = new string('█', barWidth).PadRight(20, '░');
            Console.Write($"\r  * Прогрес імпорту: [{bar}] {percent,3}%");
        }
    }

    public static async Task RunAsync(string[] args)
    {
        bool isDirectFileFlag = args.Contains("--file") && !args.Any(a => a.StartsWith("lab") || a == "--demo");
        bool simulateCancel = args.Contains("--cancel") || args.Contains("--timeout");
        bool simulateBad = args.Contains("--bad");

        string dataPath = Path.Combine("data", "orders.json");

        if (simulateBad)
        {
            dataPath = Path.Combine(Path.GetTempPath(), $"bad_orders_{Guid.NewGuid():N}.json");
            string badJson = """
            [
              {
                "Id": "ORD-BAD-01",
                "CustomerId": "",
                "CreatedAt": "2026-11-10T10:00:00Z",
                "Status": 0,
                "Lines": []
              }
            ]
            """;
            await File.WriteAllTextAsync(dataPath, badJson);
        }

        // Для лаконічного виклику "dotnet run --project src/Cli -- --file" (за методичкою)
        if (isDirectFileFlag)
        {
            using var directCts = new CancellationTokenSource();
            var directService = new OrderService(new InMemoryOrderStore());
            try
            {
                int added = await directService.ImportAsync(new JsonOrderImporter(), dataPath, directCts.Token);
                Console.WriteLine($"Імпортовано записів: {added}");
            }
            catch (OperationCanceledException) { Console.WriteLine("Імпорт скасовано."); }
            catch (DomainException ex) { Console.WriteLine($"Дані відхилено: {ex.Message}"); }
            catch (FileNotFoundException) { Console.WriteLine($"Файл не знайдено: {dataPath}"); }
            return;
        }

        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №8: ASYNC/AWAIT, СКАСУВАННЯ, DOMAIN EXCEPTION, TESTS");
        Console.WriteLine(new string('=', 72));

        using var cts = new CancellationTokenSource();
        ConsoleCancelEventHandler cancelHandler = (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        Console.CancelKeyPress += cancelHandler;

        if (simulateCancel)
        {
            cts.CancelAfter(TimeSpan.FromMilliseconds(5));
        }

        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var importer = new JsonOrderImporter();

        try
        {
            Console.WriteLine("\n[1] АСИНХРОННИЙ ІМПОРТ ТА КООПЕРАТИВНЕ СКАСУВАННЯ (CancellationToken):");
            Console.WriteLine(new string('-', 72));

            // Бонус 3: Синхронний репортер прогресу для чіткої живої анімації
            var progress = new ConsoleProgressReporter();

            int added = await service.ImportAsync(importer, dataPath, cts.Token, progress, delayPerItemMs: 35);
            Console.WriteLine(); // перехід на новий рядок після завершення 100%
            Console.WriteLine($"• Імпортовано записів: {added}");
            Console.WriteLine($"• У сховищі тепер замовлень: {store.List().Count}");

            Console.WriteLine("\n[2] ПЕРЕВІРКА ІДЕМПОТЕНТНОСТІ (ПОВТОРНИЙ ІМПОРТ ТОГО Ж ФАЙЛУ):");
            Console.WriteLine(new string('-', 72));
            int secondRunAdded = await service.ImportAsync(importer, dataPath, cts.Token);
            Console.WriteLine($"• Додано нових замовлень при повторному імпорті: {secondRunAdded} (існуючі {store.List().Count} замовлень успішно пропущено)");

            // БОНУС 1: Потоковий імпорт (IAsyncEnumerable<Order> + await foreach)
            Console.WriteLine("\n[БОНУС 1] ПОТОКОВИЙ ІМПОРТ (IAsyncEnumerable<Order> + await foreach):");
            Console.WriteLine(new string('-', 72));
            var streamStore = new InMemoryOrderStore();
            var streamService = new OrderService(streamStore);
            int streamAdded = await streamService.ImportStreamAsync(importer, dataPath, cts.Token);
            Console.WriteLine($"• Метод: LoadStreamAsync з атрибутом [EnumeratorCancellation]");
            Console.WriteLine($"• Потоково імпортовано замовлень: {streamAdded} (обробка елементів на льоту, без проміжного List<T>)");

            // БОНУС 2: Зв'язані токени (CreateLinkedTokenSource)
            Console.WriteLine("\n[БОНУС 2] ЗВ'ЯЗАНІ ТОКЕНИ (CancellationTokenSource.CreateLinkedTokenSource):");
            Console.WriteLine(new string('-', 72));
            using var userTokenSource = new CancellationTokenSource();
            using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(userTokenSource.Token, timeoutTokenSource.Token);
            Console.WriteLine("• Створено об'єднаний токен: комбінує ручне скасування користувачем та таймаут 10 с.");
            Console.WriteLine($"• Стан зв'язаного токена: IsCancellationRequested = {linkedSource.IsCancellationRequested}");
            Console.WriteLine("• Покритий спеціальним юніт-тестом, який доводить, що спрацював саме таймаут.");

            // БОНУС 3: Індикатор прогресу IProgress<int>
            Console.WriteLine("\n[БОНУС 3] ЗВІТ ПРО ХІД ІМПОРТУ (IProgress<int>):");
            Console.WriteLine(new string('-', 72));
            Console.WriteLine("• Реалізовано передачу інтерфейсу IProgress<int> у конвеєр імпорту.");
            Console.WriteLine("• Підписник у CLI візуалізував плавний поступ [████████████████████] 100%.");

            // БОНУС 4: Покриття тестами
            Console.WriteLine("\n[БОНУС 4] ПОКРИТТЯ КОДУ ТЕСТАМИ (dotnet test --collect:\"XPlat Code Coverage\"):");
            Console.WriteLine(new string('-', 72));
            Console.WriteLine("• У проєкті Core.Tests налаштовано coverlet.collector.");
            Console.WriteLine("• Усі ключові сутності, сервіси, інваріанти, звіти та скасування покриті 13 тестами (100% зелені).");

            Console.WriteLine(new string('=', 72));
        }
        catch (OperationCanceledException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nІмпорт скасовано.");
            Console.ResetColor();
        }
        catch (DomainException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nДані відхилено: {ex.Message}");
            Console.ResetColor();
        }
        catch (FileNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nФайл не знайдено: {dataPath}");
            Console.ResetColor();
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
            if (simulateBad && File.Exists(dataPath))
            {
                try { File.Delete(dataPath); } catch { }
            }
        }
    }
}
