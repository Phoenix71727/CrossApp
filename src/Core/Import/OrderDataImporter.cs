using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class OrderDataImporter
{
    private const char Separator = ';';

    public static ImportResult<IEntityDto> Load(string path)
    {
        var items = new List<IEntityDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue; // рядок заголовків

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<IEntityDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 } => new ParseFailed($"очікую щонайменше 4 колонки, отримав {parts.Length}"),

            // Валідний товар: P;id;name;price або P;id;name;price;category
            ["P", var id, var name, var priceStr, ..]
                when decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p) && p > 0
                => new ParseOk(new ProductDto(id, name, p, parts.Length > 4 ? parts[4] : null)),

            // Валідний клієнт: C;id;fullname;email або C;id;fullname;email;phone
            ["C", var id, var name, var email, ..]
                when !string.IsNullOrWhiteSpace(email) && email.Contains('@')
                => new ParseOk(new CustomerDto(id, name, email, parts.Length > 4 ? parts[4] : null)),

            // Помилки валідації полів
            ["P" or "C", "", ..] or ["P" or "C", _, "", ..]
                => new ParseFailed("Id або назва/ім'я не можуть бути порожніми"),

            ["P", _, _, var priceStr, ..]
                => new ParseFailed($"ціна товару '{priceStr}' не є додатним числом"),

            ["C", _, _, var email, ..]
                => new ParseFailed($"некоректний email клієнта '{email}'"),

            // Невідомий тип
            [var type, ..]
                => new ParseFailed($"невідомий префікс типу: '{type}' (очікується P або C)")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(IEntityDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
