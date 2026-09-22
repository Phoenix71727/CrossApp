using System.Text;
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
return 0;

static void PrintSummary(int accepted, int skipped)
{
    int total = accepted + skipped;
    double errorRate = total > 0 ? (double)skipped / total * 100 : 0.0;
    Console.WriteLine(new string('-', 68));
    Console.WriteLine($"Статистика імпорту: усього {total} | прийнято {accepted} | пропущено {skipped} | помилок {errorRate:F1}%");
}
