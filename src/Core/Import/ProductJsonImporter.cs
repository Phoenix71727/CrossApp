using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path, Encoding.UTF8);

        try
        {
            List<ProductDto> rawItems = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

            for (int i = 0; i < rawItems.Count; i++)
            {
                ProductDto item = rawItems[i];
                int index = i + 1;

                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Name))
                {
                    errors.Add($"елемент #{index}: Id або назва порожні");
                    continue;
                }

                if (item.Price <= 0)
                {
                    errors.Add($"елемент #{index}: ціна '{item.Price}' не є додатним числом");
                    continue;
                }

                items.Add(item);
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка синтаксису JSON: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}
