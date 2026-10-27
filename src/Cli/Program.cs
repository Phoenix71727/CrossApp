using System.Text;
using Cli.Notifications;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

string? customPath = args.FirstOrDefault(a => !a.StartsWith("--"));
string path = customPath ?? Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string fileName = Path.GetFileName(path).ToLowerInvariant();
string extension = Path.GetExtension(path).ToLowerInvariant();

if (fileName.Contains("mixed"))
{
    ImportResult<IEntityDto> mixedResult = OrderDataImporter.Load(path);

    Console.WriteLine($"Завантажено різнорідних записів: {mixedResult.Items.Count}");
    Console.WriteLine(new string('-', 72));

    foreach (IEntityDto item in mixedResult.Items)
    {
        string display = item switch
        {
            ProductDto p => $" [ТОВАР]  {p.Id,-7} {p.Name,-28} {p.Price,10:F2} грн  ({p.Category})",
            CustomerDto c => $" [КЛІЄНТ] {c.Id,-7} {c.FullName,-28} {c.Email}  {c.Phone}",
            _ => item.ToString() ?? string.Empty
        };
        Console.WriteLine(display);
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"Пропущено рядків: {mixedResult.Errors.Count}");
        foreach (string error in mixedResult.Errors)
        {
            Console.WriteLine($" ! {error}");
        }
    }

    PrintSummary(mixedResult.Items.Count, mixedResult.Errors.Count);
    return 0;
}

ImportResult<ProductDto>? result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: '{extension}'. Підтримуються лише .csv та .json");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine(new string('-', 68));

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-7} {p.Name,-32} {p.Price,10:F2} грн  {p.Category}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(new string('-', 68));
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string error in result.Errors)
    {
        Console.WriteLine($" ! {error}");
    }
}

PrintSummary(result.Items.Count, result.Errors.Count);

// Доменна модель та інваріанти
RunLab4Demo(result);

// Сховище даних та Dependency Injection (Лабораторна робота №5)
RunLab5Demo(args);

// Доменні події, делегати та лямбди (Лабораторна робота №6)
RunLab6Demo();
return 0;

static void PrintSummary(int accepted, int skipped)
{
    int total = accepted + skipped;
    double errorRate = total > 0 ? (double)skipped / total * 100 : 0.0;
    Console.WriteLine(new string('-', 68));
    Console.WriteLine($"Статистика імпорту: усього {total} | прийнято {accepted} | пропущено {skipped} | помилок {errorRate:F1}%");
}

static void RunLab4Demo(ImportResult<ProductDto> importResult)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 72));
    Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №4: ДОМЕННА МОДЕЛЬ ТА ІНВАРІАНТИ (ORDER / PRODUCT)");
    Console.WriteLine(new string('=', 72));

    // Зв'язок з Лабою 3 (ImportResult -> DomainImportResult)
    Console.WriteLine("\n[БОНУС 1] ВАЛІДАЦІЯ DTO ТА СТВОРЕННЯ ДОМЕННИХ СУТНОСТЕЙ (FromDtos):");
    Console.WriteLine(new string('-', 72));
    DomainImportResult<Product> domainResult = Product.FromDtos(importResult);
    Console.WriteLine($"Успішно відновлено валідних доменних сутностей: {domainResult.Entities.Count}");
    Console.WriteLine($"Відсіяно через помилки парсингу та інваріантів: {domainResult.Errors.Count}");

    Console.WriteLine("\n[1] СЦЕНАРІЙ: УСПІШНИЙ ЖИТТЄВИЙ ЦИКЛ ЗАМОВЛЕННЯ (HAPPY PATH)");
    Console.WriteLine(new string('-', 72));

    Product product1 = domainResult.Entities.Count > 0
        ? domainResult.Entities[0]
        : Product.Create("P-001", "Тестовий товар 1", 1200.00m, "Електроніка");

    Product product2 = domainResult.Entities.Count > 1
        ? domainResult.Entities[1]
        : Product.Create("P-002", "Тестовий товар 2", 450.50m, "Аксесуари");

    Console.WriteLine($"Використовуємо товари в замовленні:");
    Console.WriteLine($"  * {product1}");
    Console.WriteLine($"  * {product2}");

    // Створення замовлення
    Order order = Order.Create("ORD-2026-001", "CUST-777");
    Console.WriteLine($"\nСтворено замовлення: {order}");

    // Додавання рядків
    order.AddLine(product1.Id, product1.Name, product1.Price, 2);
    order.AddLine(product2.Id, product2.Name, product2.Price, 1);
    Console.WriteLine($"\nДодано 2 позиції. Поточний склад замовлення:");
    foreach (OrderLine line in order.Lines)
    {
        Console.WriteLine($"  - {line}");
    }
    Console.WriteLine($"Загальна сума (Total): {order.Total:F2} грн");

    // Підтвердження (перехід стану перевіряється switch expression)
    order.Confirm();
    Console.WriteLine($"\nЗамовлення підтверджено (switch: Draft -> Confirmed): {order}");
    Console.WriteLine("\n[2] СЦЕНАРІЙ: ПЕРЕВІРКА ЗАХИСТУ ІНВАРІАНТІВ (EDGE CASES)");
    Console.WriteLine(new string('-', 72));

    TryDo("Створення рядка з від'ємною ціною (-100 грн)", () =>
    {
        OrderLine.Create("P-ERR", "Бракований товар", -100m, 1);
    });

    TryDo("Створення рядка з нульовою кількістю (x0)", () =>
    {
        OrderLine.Create("P-ERR", "Товар без кількості", 250m, 0);
    });

    TryDo("Створення замовлення з порожнім CustomerId", () =>
    {
        Order.Create("ORD-ERR", "   ");
    });

    TryDo("Підтвердження порожнього замовлення (без позицій)", () =>
    {
        Order emptyOrder = Order.Create("ORD-EMPTY", "CUST-001");
        emptyOrder.Confirm();
    });

    TryDo("Додавання позиції у вже підтверджене замовлення", () =>
    {
        order.AddLine("P-999", "Спроба додати після підтвердження", 100m, 1);
    });

    TryDo("Скасування підтвердженого замовлення (switch: Confirmed -> Cancelled заборонено)", () =>
    {
        order.Cancel();
    });

    TryDo("Оновлення ціни продукту на від'ємну (-500 грн)", () =>
    {
        product1.UpdatePrice(-500m);
    });

    // Інваріант двох сутностей через доменний сервіс
    Console.WriteLine("\nІНВАРІАНТ ДВОХ СУТНОСТЕЙ (Customer + Orders -> OrderLimitService):");
    Console.WriteLine(new string('-', 72));
    Customer customer = Customer.Create("CUST-100", "Олександр Мельник", "melnyk@lnu.edu.ua");
    Console.WriteLine($"Клієнт: {customer}");

    List<Order> customerOrders =
    [
        Order.Create("ORD-01", customer.Id),
        Order.Create("ORD-02", customer.Id),
        Order.Create("ORD-03", customer.Id)
    ];

    Console.WriteLine($"У клієнта вже створено {customerOrders.Count} активних замовлення.");
    bool canPlace = OrderLimitService.CanPlaceOrder(customer, customerOrders, out string? reason);
    if (!canPlace)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[BLOCKED BY DOMAIN SERVICE]: {reason}");
        Console.ResetColor();
    }
    Console.WriteLine(new string('=', 72));
}

static void TryDo(string testTitle, Action action)
{
    Console.Write($"• {testTitle,-54} -> ");
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

static void RunLab5Demo(string[] args)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 72));
    Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №5: СХОВИЩЕ ДАНИХ ТА DEPENDENCY INJECTION (DIP)");
    Console.WriteLine(new string('=', 72));

    // Крок 7 & Бонус 3: Створення сховища через OrderStoreFactory (Composition Root)
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

    // Створення замовлення через сервіс
    string customerId = "CUST-PAVLO";
    Order createdOrder = service.CreateOrder(customerId);
    Console.WriteLine($"• Створено нове замовлення через сервіс: Id = {createdOrder.Id}, CustomerId = {createdOrder.CustomerId}, Status = {createdOrder.Status}");

    // Додавання позицій через сервіс
    service.AddLine(createdOrder.Id, "P-001", "Ноутбук Lenovo ThinkPad", 32500.00m, 1);
    service.AddLine(createdOrder.Id, "P-004", "Бездротова миша Logitech MX", 3450.75m, 2);
    Console.WriteLine($"• Додано 2 позиції товару до замовлення '{createdOrder.Id}'");

    // Підтвердження замовлення через сервіс
    service.Confirm(createdOrder.Id);
    Console.WriteLine($"• Замовлення '{createdOrder.Id}' успішно підтверджено (Confirmed)");

    // Пошук замовлення в сховищі
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

    // Виведення списку всіх замовлень
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

static void RunLab6Demo()
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 72));
    Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №6: ДОМЕННІ ПОДІЇ, ДЕЛЕГАТИ ACTION/FUNC, ЛЯМБДИ");
    Console.WriteLine(new string('=', 72));

    var store = new InMemoryOrderStore();
    var service = new OrderService(store);

    // Підготовка шляху до файлу логів (створюється в корені проєкту logs/app.log)
    string logPath = Directory.Exists("data")
        ? Path.Combine("logs", "app.log")
        : Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
    File.WriteAllText(logPath, string.Empty); // Очищення перед початком для чистих логів демонстрації

    // Підписник №1 — локальний іменований метод
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

    // Підписка №1: локальний метод
    service.Changed += OnOrderChanged;
    Console.WriteLine("  * Підписник №1: підписано локальним методом OnOrderChanged (консоль + logs/app.log)");

    // Підписник №2: лямбда із замиканням над локальною змінною changes
    int changes = 0;
    service.Changed += (_, _) => changes++;
    Console.WriteLine("  * Підписник №2: підписано лямбдою із замиканням над changes");

    // Крок 5 методички: Сценарій у Cli: додати запис, зробити 2-3 зміни, вивести changes, відписатися
    Console.WriteLine("\n[2] СЦЕНАРІЙ У CLI: УСПІШНІ ЗМІНИ ТА СПОВІЩЕННЯ");
    Console.WriteLine(new string('-', 72));

    var products = SampleData.Products();
    var created = service.CreateOrder("CUST-101", "ORD-LAB6-001");
    service.AddLine(created.Id, products[0].Id, products[0].Name, products[0].Price, 1);
    service.Confirm(created.Id);

    Console.WriteLine($"\n>> Подій отримано (обидва підписники активні): {changes}");

    // Демонстрація керованої відписки (-=)
    Console.WriteLine("\n[3] ВІДПИСКА ПІДПИСНИКА №1 (service.Changed -= OnOrderChanged)");
    Console.WriteLine(new string('-', 72));
    service.Changed -= OnOrderChanged;
    Console.WriteLine("  * Підписник №1 успішно відписаний від service.Changed.");
    Console.WriteLine("  * Виконуємо наступну операцію (створення замовлення ORD-LAB6-002)...");

    // Тиха операція: OnOrderChanged вже мовчить, але лямбда рахує (+1)
    service.CreateOrder("CUST-102", "ORD-LAB6-002");
    Console.WriteLine($"\n>> Подій отримано після відписки: {changes} (+1 лише від лямбди-лічильника, логер мовчав)");

    // Крок 6 методички: Сценарій відмови (порушення інваріанту або неіснуючий id)
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

    // Крок 7 методички: Делегат замість події там, де потрібне значення (Func)
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

    // БОНУС 1: INotificationSink через ручний DI
    Console.WriteLine("\n[БОНУС 1] АЛЬТЕРНАТИВА: ПРИЙМАЧ СПОВІЩЕНЬ INotificationSink (РУЧНИЙ DI)");
    Console.WriteLine(new string('-', 72));
    Console.WriteLine("  * Створюємо ConsoleSink та передаємо в OrderService через конструктор:");
    var consoleSink = new ConsoleSink();
    var sinkStore = new InMemoryOrderStore();
    var sinkService = new OrderService(sinkStore, consoleSink);
    var sinkOrder = sinkService.CreateOrder("CUST-BONUS-1", "ORD-BONUS-01");
    sinkService.AddLine(sinkOrder.Id, products[1].Id, products[1].Name, products[1].Price, 1);
    sinkService.Confirm(sinkOrder.Id);

    // БОНУС 2: Action<string> у методі імпорту для звіту про пропущені рядки
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

    // БОНУС 3: Теоретичний аналіз async void в обробниках подій
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

