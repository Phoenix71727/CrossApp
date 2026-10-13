using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

/// <summary>
/// Декоратор для сховища замовлень (патерн Decorator).
/// Приймає внутрішній IOrderStore: той самий контракт, але додає кешування та збір метрик (CacheHits, CacheMisses).
/// </summary>
public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
    private readonly IOrderStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public IReadOnlyList<Order> List()
    {
        var items = _inner.List();
        foreach (var item in items)
        {
            _cache[item.Id] = item;
        }
        return items;
    }

    public Order? GetById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (_cache.TryGetValue(id, out var cached))
        {
            CacheHits++;
            return cached;
        }

        CacheMisses++;
        var item = _inner.GetById(id);
        if (item is not null)
        {
            _cache[item.Id] = item;
        }

        return item;
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        var results = _inner.Find(predicate);
        foreach (var item in results)
        {
            _cache[item.Id] = item;
        }
        return results;
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Add(item);
        _cache[item.Id] = item;
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Update(item);
        _cache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed)
        {
            _cache.Remove(id);
        }
        return removed;
    }
}
