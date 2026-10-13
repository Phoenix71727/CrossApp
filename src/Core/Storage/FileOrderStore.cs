using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            var dtos = JsonSerializer.Deserialize<List<OrderDto>>(json) ?? [];

            foreach (var dto in dtos)
            {
                var order = Order.FromDto(dto);
                _cache[order.Id] = order;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var dtos = _cache.Values.Select(o => o.ToDto()).ToList();
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з id='{item.Id}' уже існує.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();

        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }
}
