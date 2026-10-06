using System.Text;
using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

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
