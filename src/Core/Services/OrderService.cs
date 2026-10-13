using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService(IOrderStore store)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string customerId, string? orderId = null)
    {
        string id = orderId ?? $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        Order order = Order.Create(id, customerId);
        _store.Add(order);
        return order;
    }

    public void AddLine(string orderId, string productId, string productName, decimal price, int quantity)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id='{orderId}'.");

        order.AddLine(productId, productName, price, quantity);
        _store.Update(order);
    }

    public void Confirm(string orderId)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id='{orderId}'.");

        order.Confirm();
        _store.Update(order);
    }

    public void Cancel(string orderId)
    {
        Order order = _store.GetById(orderId)
            ?? throw new InvalidOperationException($"Немає замовлення з id='{orderId}'.");

        order.Cancel();
        _store.Update(order);
    }

    public IReadOnlyList<Order> All() => _store.List();

    public Order? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) => _store.Find(predicate);
}
