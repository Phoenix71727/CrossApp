using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Cli.Demos;

public static class Lab5Demo
{
    public static void Run(string[] args)
    {
        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №5: СХОВИЩЕ ДАНИХ ТА DEPENDENCY INJECTION (DIP)");
        Console.WriteLine(new string('=', 72));

        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cached");
        string dataPath = Directory.Exists("data")
            ? Path.Combine("data", "orders.json")
            : Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

        IOrderStore store = OrderStoreFactory.Create(args, dataPath);
        var service = new OrderService(store);

        Console.WriteLine("\n[COMPOSITION ROOT & ФАБРИКА СХОВИЩ]");
        Console.WriteLine($"  * Створення сховища : OrderStoreFactory.Create(args)");
        Console.WriteLine($"  * Прапорець --file  : {(useFile ? "активовано (FileOrderStore)" : "не задано (InMemoryOrderStore)")}");
        Console.WriteLine($"  * Прапорець --cached: {(useCache ? "активовано (обгорнуто в CachingOrderStore)" : "не задано")}");
        Console.WriteLine($"  * Тип сховища       : {store.GetType().Name}");
        if (useFile)
        {
            Console.WriteLine($"  * Файл даних JSON   : {dataPath}");
        }
        Console.WriteLine($"  * Початковий вміст  : {service.All().Count} замовлень у сховищі");

        Console.WriteLine("\n[1] СЦЕНАРІЙ: УСПІШНИЙ ПОТІК (HAPPY PATH ЧЕРЕЗ OrderService)");
        Console.WriteLine(new string('-', 72));

        string customerId = "CUST-PAVLO";
        Order createdOrder = service.CreateOrder(customerId);
        Console.WriteLine($"• Створено нове замовлення через сервіс: Id = {createdOrder.Id}, CustomerId = {createdOrder.CustomerId}, Status = {createdOrder.Status}");

        service.AddLine(createdOrder.Id, "P-001", "Ноутбук Lenovo ThinkPad", 32500.00m, 1);
        service.AddLine(createdOrder.Id, "P-004", "Бездротова миша Logitech MX", 3450.75m, 2);
        Console.WriteLine($"• Додано 2 позиції товару до замовлення '{createdOrder.Id}'");

        service.Confirm(createdOrder.Id);
        Console.WriteLine($"• Замовлення '{createdOrder.Id}' успішно підтверджено (Confirmed)");

        Order? foundOrder = service.Find(createdOrder.Id);
        if (foundOrder is not null)
        {
            Console.WriteLine($"\n• Знайдено в сховищі: {foundOrder}");
            Console.WriteLine($"  Позиції ({foundOrder.Lines.Count} шт.):");
            foreach (var line in foundOrder.Lines)
            {
                Console.WriteLine($"    - {line}");
            }
            Console.WriteLine($"  Разом до сплати: {foundOrder.Total:F2} грн");
        }

        var allOrders = service.All();
        Console.WriteLine($"\n• Усього замовлень у сховищі зараз: {allOrders.Count}");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($" {"Id",-20} {"CustomerId",-14} {"Status",-12} {"Lines",-6} {"Total, грн",12}");
        Console.WriteLine(new string('-', 72));
        foreach (var o in allOrders.TakeLast(5))
        {
            Console.WriteLine($" {o.Id,-20} {o.CustomerId,-14} {o.Status,-12} {o.Lines.Count,-6} {o.Total,12:F2}");
        }
        if (allOrders.Count > 5)
        {
            Console.WriteLine($"   ... (показано останні 5 із {allOrders.Count})");
        }

        Console.WriteLine("\n[2] СЦЕНАРІЙ: ВІДМОВА ТА ОБРОБКА ПОМИЛОК (EDGE CASES & TRY/CATCH)");
        Console.WriteLine(new string('-', 72));

        TryDo("Спроба додати позицію у неіснуюче замовлення (service.AddLine)", () =>
        {
            service.AddLine("ORD-NON-EXISTENT", "P-001", "Тест", 100m, 1);
        });

        TryDo("Спроба підтвердити неіснуюче замовлення (service.Confirm)", () =>
        {
            service.Confirm("ORD-NON-EXISTENT");
        });

        TryDo($"Спроба додати замовлення з дублікатом Id '{createdOrder.Id}' (store.Add)", () =>
        {
            var duplicateOrder = Order.Create(createdOrder.Id, "CUST-DUPLICATE");
            store.Add(duplicateOrder);
        });

        TryDo("Спроба додати позицію у вже підтверджене замовлення (service.AddLine)", () =>
        {
            service.AddLine(createdOrder.Id, "P-003", "Клавіатура", 4100m, 1);
        });

        Console.WriteLine("\n[3] ДОДАТКОВЕ ЗАВДАННЯ: ПОШУК З ПАРАМЕТРОМ Func<Order, bool>");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("• Пошук замовлень зі статусом 'Confirmed' та сумою > 30 000 грн через предикат:");
        var highValueConfirmed = service.Find(o => o.Status == OrderStatus.Confirmed && o.Total > 40000m);
        foreach (var o in highValueConfirmed)
        {
            Console.WriteLine($"  [ЗНАЙДЕНО] {o.Id,-16} Клієнт: {o.CustomerId,-12} Рядків: {o.Lines.Count}  Сума: {o.Total,10:F2} грн");
        }

        Console.WriteLine("\n[4] ДОДАТКОВЕ ЗАВДАННЯ: ДЕКОРАТОР СХОВИЩА CachingOrderStore");
        Console.WriteLine(new string('-', 72));
        if (store is CachingOrderStore cachingStore)
        {
            Console.WriteLine($"• Декоратор CachingOrderStore АКТИВОВАНО (задано прапорець --cached)");
            _ = service.Find(createdOrder.Id);
            Console.WriteLine($"  Повторний виклик Find('{createdOrder.Id}') -> Hits: {cachingStore.CacheHits}, Misses: {cachingStore.CacheMisses} (дані повернено з кешу декоратора)");
        }
        else
        {
            Console.WriteLine($"• Декоратор CachingOrderStore НЕ АКТИВОВАНО (прапорець --cached не задано)");
            Console.WriteLine($"  Сховище {store.GetType().Name} працює напряму без кешування.");
            Console.WriteLine($"  Щоб активувати кешуючий декоратор, додайте прапорець: --cached");
        }

        Console.WriteLine("\n[5] ДОДАТКОВЕ ЗАВДАННЯ: ПОРІВНЯННЯ StoreFactory VS INLINE В Program.cs");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("  1. Варіант 'усе в Program.cs':");
        Console.WriteLine("     - Плюси: простий тернарний вираз, без додаткових класів;");
        Console.WriteLine("     - Мінуси: змішує парсинг CLI та інстанціювання сховищ; важко тестувати.");
        Console.WriteLine("  2. Варіант 'OrderStoreFactory.Create(args)':");
        Console.WriteLine("     - Плюси: інкапсулює правила створення (File/InMemory/Cache), чистий SRP, легко тестувати окремо;");
        Console.WriteLine("     - Мінуси: додатковий допоміжний клас у архітектурі.");
        Console.WriteLine(new string('=', 72));
    }

    private static void TryDo(string testTitle, Action action)
    {
        Console.Write($"• {testTitle,-68} -> ");
        try
        {
            action();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[FAIL: інваріант не спрацював!]");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{ex.GetType().Name}]: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }
}
