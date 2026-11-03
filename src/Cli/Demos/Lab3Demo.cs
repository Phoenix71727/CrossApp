using Core.Dto;
using Core.Import;

namespace Cli.Demos;

public static class Lab3Demo
{
    public static void Run(string[] args)
    {
        string? customPath = args.FirstOrDefault(a => !a.StartsWith("--"));
        string path = customPath ?? Path.Combine("data", "sample.csv");

        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №3: ІМПОРТ ДАНИХ ТА DTO (PATTERN MATCHING)");
        Console.WriteLine(new string('=', 72));

        if (!File.Exists(path))
        {
            Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
            return;
        }

        string fileName = Path.GetFileName(path).ToLowerInvariant();
        string extension = Path.GetExtension(path).ToLowerInvariant();

        Console.WriteLine($"Файл для імпорту: {path}");
        Console.WriteLine(new string('-', 72));

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
            Console.WriteLine(new string('=', 72));
            return;
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
            return;
        }

        Console.WriteLine($"Завантажено записів: {result.Items.Count}");
        Console.WriteLine(new string('-', 72));

        foreach (ProductDto p in result.Items)
        {
            Console.WriteLine($" {p.Id,-7} {p.Name,-30} {p.Price,10:F2} грн  ({p.Category})");
        }

        if (result.Errors.Count > 0)
        {
            Console.WriteLine(new string('-', 72));
            Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
            foreach (string error in result.Errors)
            {
                Console.WriteLine($" ! {error}");
            }
        }

        PrintSummary(result.Items.Count, result.Errors.Count);
        Console.WriteLine(new string('=', 72));
    }

    private static void PrintSummary(int accepted, int skipped)
    {
        int total = accepted + skipped;
        double errorRate = total > 0 ? (double)skipped / total * 100 : 0.0;
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"Статистика імпорту: усього {total} | прийнято {accepted} | пропущено {skipped} | помилок {errorRate:F1}%");
    }
}
