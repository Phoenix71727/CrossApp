using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    public string Id { get; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public string? Category { get; private set; }

    private Product(string id, string name, decimal price, string? category)
    {
        Id = id;
        Name = name;
        Price = price;
        Category = category;
    }

    public static Product Create(string id, string name, decimal price, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор товару не може бути порожнім", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(name));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Ціна товару не може бути від'ємною");

        return new Product(id.Trim(), name.Trim(), price, category?.Trim());
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice), newPrice, "Ціна товару не може бути від'ємною");

        Price = newPrice;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(newName));

        Name = newName.Trim();
    }

    public ProductDto ToDto() => new(Id, Name, Price, Category);

    public static Product FromDto(ProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return Create(dto.Id, dto.Name, dto.Price, dto.Category);
    }

    public static DomainImportResult<Product> FromDtos(ImportResult<ProductDto> importResult)
    {
        ArgumentNullException.ThrowIfNull(importResult);

        List<Product> entities = [];
        List<string> errors = [.. importResult.Errors];

        foreach (ProductDto dto in importResult.Items)
        {
            try
            {
                entities.Add(FromDto(dto));
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
            {
                errors.Add($"Товар '{dto.Id}': порушено інваріант — {ex.Message}");
            }
        }

        return new DomainImportResult<Product>(entities.AsReadOnly(), errors.AsReadOnly());
    }

    public override string ToString() =>
        $"{Name} (ID: {Id}) — {Price:F2} грн" + (Category != null ? $" [{Category}]" : string.Empty);
}
