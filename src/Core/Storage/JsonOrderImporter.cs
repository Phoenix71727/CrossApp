using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonOrderImporter : IOrderImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        List<OrderDto>? dtos;
        try
        {
            dtos = await JsonSerializer.DeserializeAsync<List<OrderDto>>(
                stream,
                JsonOptions,
                cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Помилка десеріалізації JSON у файлі {Path.GetFileName(path)}: {ex.Message}", ex);
        }

        if (dtos is null)
            throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку замовлень.");

        var result = new List<Order>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                result.Add(Order.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }

    public async IAsyncEnumerable<Order> LoadStreamAsync(string path, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        List<OrderDto>? dtos;
        try
        {
            dtos = await JsonSerializer.DeserializeAsync<List<OrderDto>>(
                stream,
                JsonOptions,
                cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            throw new DomainException($"Помилка десеріалізації JSON у файлі {Path.GetFileName(path)}: {ex.Message}", ex);
        }

        if (dtos is null)
            throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку замовлень.");

        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            Order order;
            try
            {
                order = Order.FromDto(dtos[i]);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }

            yield return order;
        }
    }
}
