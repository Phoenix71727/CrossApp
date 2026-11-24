using System.Collections.Concurrent;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    private readonly ConcurrentDictionary<string, Order> _items = new(
        (seed ?? []).Select(o => new KeyValuePair<string, Order>(o.Id, o)),
        StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _items.Values.ToList();

    public Order? GetById(string id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_items.TryAdd(item.Id, item))
            throw new InvalidOperationException($"Замовлення з id='{item.Id}' уже існує.");
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.Id] = item;
    }

    public bool Remove(string id) => _items.TryRemove(id, out _);
}
